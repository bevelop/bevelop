using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Bevelop.Messages;
using LibGit2Sharp;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Bevelop.VSClient.UIExtensions
{
    sealed class ChangeNotifier
    {
        readonly IWpfTextView _view;
        readonly ITextDocument _document;
        readonly IVsCommandWindow _commandWindow;
        readonly IAdornmentLayer _adornmentLayer;
        IList<FileChange> _otherPeoplesChanges = new List<FileChange>();
        FileSystemWatcher _fileSystemWatcher;
        Repository _repo;
        HubConnection _connection;
        IHubProxy _hub;
        string _relativePath;

        public bool IsTracking => _repo != null;

        public ChangeNotifier(IWpfTextView view, ITextDocument document, IVsCommandWindow commandWindow)
        {
            _view = view;
            _document = document;
            _commandWindow = commandWindow;

            _adornmentLayer = view.GetAdornmentLayer("ChangeNotifier");

            SetupRepo();
            SetupHub();
            SetupFileWatch();

            _view.ViewportHeightChanged += OnSizeChanged;
            _view.ViewportWidthChanged += OnSizeChanged;
            _view.Closed += OnClosed;
        }

        void SetupRepo()
        {
            var root = Path.GetDirectoryName(_document.FilePath);
            while (root != null && !Directory.Exists(Path.Combine(root, ".git")))
            {
                var parent = Directory.GetParent(root);
                root = parent?.FullName;
            }

            if (root == null)
                return;

            var fullUri = new Uri(_document.FilePath);
            var rootUri = new Uri(root);
            var relative = rootUri.MakeRelativeUri(fullUri).ToString();
            _relativePath = relative.Substring(relative.IndexOf('/') + 1);
            _relativePath = _relativePath.Replace('/', '\\');

            _repo = new Repository(root);
        }

        void SetupHub()
        {
            if (!IsTracking)
                return;

            _connection = new HubConnection("http://localhost:8080/");
            _hub = _connection.CreateHubProxy("ChangeHub");
            _connection.Start().Wait();

            _hub.On<FileChange>("notify", OnAnotherUserChangedFile);
        }

        void SetupFileWatch()
        {
            if (!IsTracking)
                return;

            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(_document.FilePath),
                Filter = Path.GetFileName(_document.FilePath),
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            _fileSystemWatcher.Changed += OnFileChanged;
        }

        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                _fileSystemWatcher.EnableRaisingEvents = false;

                var modified = _repo.RetrieveStatus().Modified.FirstOrDefault(m => e.FullPath.Contains(m.FilePath));
                if (modified != null)
                {
                    var text = ZipText(_document.TextBuffer.CurrentSnapshot.GetText());

                    _hub.Invoke("NotifyChange", new FileChange
                    {
                        User = _repo.Config.Get<string>("user.name").Value,
                        Branch = _repo.Branches.FirstOrDefault(x => x.IsCurrentRepositoryHead)?.FriendlyName,
                        FileName = Path.GetFileName(modified.FilePath),
                        FilePath = modified.FilePath,
                        Diff = text
                    });
                }
                else
                {
                    var removedChanges = _otherPeoplesChanges
                        .Where(c => c.FilePath.Equals(_relativePath, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (removedChanges.Any())
                    {
                        _otherPeoplesChanges = _otherPeoplesChanges.Except(removedChanges).ToList();
                        RefreshUi();
                    }
                }
            }
            finally
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        byte[] ZipText(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        string UnzipBytes(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        void OnAnotherUserChangedFile(FileChange change)
        {
            if (!_relativePath.Equals(change.FilePath, StringComparison.OrdinalIgnoreCase))
                return;

            var modified = _repo.RetrieveStatus().Modified;

            if (modified.Any(m => m.FilePath == change.FilePath))
            {
                if (!_otherPeoplesChanges.Any(c => c.User.Equals(change.User) && c.FilePath.Equals(change.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    _otherPeoplesChanges.Add(change);
                    RefreshUi();
                }
            }
        }

        void OnSizeChanged(object sender, EventArgs e)
        {
            if (!IsTracking)
                return;

            RefreshUi();
        }

        void OnClosed(object sender, EventArgs e)
        {
            if (!IsTracking)
                return;

            _connection.Stop();
            _fileSystemWatcher.Dispose();
        }

        void RefreshUi()
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                DoRefreshUi();
            });
        }

        void DoRefreshUi()
        {
            _adornmentLayer.RemoveAllAdornments();

            var stackPanel = new StackPanel {Orientation = Orientation.Horizontal};
            for (var i = 0; i < _otherPeoplesChanges.Count; i++)
            {
                var change = _otherPeoplesChanges[i];
                var grid = new StackPanel {Background = Brushes.Bisque, Orientation = Orientation.Horizontal};

                var text = new Label
                {
                    Content = $"Also changed by {change.User} in {change.Branch}"
                };

                var diffBtn = new Button
                {
                    Content = "Diff",
                    Padding  = new Thickness(10, 5, 10, 5),
                    Cursor = Cursors.Arrow
                };

                diffBtn.Click += (sender, args) =>
                {
                    var leftContent = _document.TextBuffer.CurrentSnapshot.GetText();
                    var leftFile = Path.Combine(Path.GetTempPath(), $"Yours - {Guid.NewGuid().ToString().Substring(0, 10).ToLower()}");
                    File.WriteAllText(leftFile, leftContent);

                    var rightContent = UnzipBytes(change.Diff);
                    var rightFile = Path.Combine(Path.GetTempPath(), $"Theirs - {Guid.NewGuid().ToString().Substring(0, 10).ToLower()}");
                    File.WriteAllText(rightFile, rightContent);

                    _commandWindow.ExecuteCommand($"Tools.DiffFiles \"{leftFile}\" \"{rightFile}\"");
                };

                grid.Children.Add(text);
                grid.Children.Add(diffBtn);
                stackPanel.Children.Add(grid);
            }

            Canvas.SetLeft(stackPanel, _view.ViewportLeft);
            Canvas.SetTop(stackPanel, _view.ViewportBottom - 30.0);

            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, null, null, stackPanel, null);
        }
    }
}
