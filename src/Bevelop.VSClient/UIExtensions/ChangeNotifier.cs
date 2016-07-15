using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Bevelop.Messages;
using Bevelop.VSClient.Services;
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
        readonly IChangeServer _changeServer;
        readonly IZipper _zipper;
        readonly IAdornmentLayer _adornmentLayer;
        IList<FileChange> _otherPeoplesChanges = new List<FileChange>();
        readonly IGitFileChangeDetector _fileChangeDetector;

        public bool IsTracking { get; }

        public ChangeNotifier(IWpfTextView view, ITextDocument document, IVsCommandWindow commandWindow,
            IGitService gitService, IChangeServer changeServer, IZipper zipper)
        {
            _view = view;
            _document = document;
            _commandWindow = commandWindow;
            _changeServer = changeServer;
            _zipper = zipper;

            _adornmentLayer = view.GetAdornmentLayer("ChangeNotifier");

            if (!gitService.IsInRepo(_document.FilePath))
            {
                IsTracking = false;
                return;
            }

            _fileChangeDetector = gitService.GetFileChangeDetector(_document.FilePath);
            _fileChangeDetector.FileLocallyChanged += OnFileLocallyChanged;
            _fileChangeDetector.FileReset += OnFileReset;

            _changeServer.FileRemotelyChanged += OnFileRemotelyChanged;

            _view.ViewportHeightChanged += OnSizeChanged;
            _view.ViewportWidthChanged += OnSizeChanged;
            _view.Closed += OnClosed;
        }

        void OnFileLocallyChanged(object sender, FileLocallyChangedEventArgs e)
        {
            _changeServer.NotifyFileChange(e.FileChange);
        }

        void OnFileReset(object sender, EventArgs e)
        {
            RunOnUiThread(() =>
            {
                _otherPeoplesChanges = _otherPeoplesChanges.Where(c =>
                    c.FilePath.Equals(_fileChangeDetector.RelativePath, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                RefreshUi();
            });
        }

        void OnFileRemotelyChanged(object sender, FileRemotelyChangedEventArgs e)
        {
            if (!_fileChangeDetector.RelativePath.Equals(e.FileChange.FilePath, StringComparison.OrdinalIgnoreCase))
                return;

            RunOnUiThread(() =>
            {
                _otherPeoplesChanges = _otherPeoplesChanges.Where(c =>
                    !(c.User.Equals(e.FileChange.User) &&
                      c.FilePath.Equals(e.FileChange.FilePath, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                _otherPeoplesChanges.Add(e.FileChange);

                RefreshUi();
            });
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

            _fileChangeDetector.FileLocallyChanged -= OnFileLocallyChanged;
            _fileChangeDetector.FileReset -= OnFileReset;
            _changeServer.FileRemotelyChanged -= OnFileRemotelyChanged;

            _fileChangeDetector.Dispose();
        }

        void RefreshUi()
        {
            _adornmentLayer.RemoveAllAdornments();

            var stackPanel = new StackPanel {Orientation = Orientation.Horizontal};
            for (var i = 0; i < _otherPeoplesChanges.Count; i++)
            {
                var change = _otherPeoplesChanges.ElementAt(i);
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

                    var rightContent = _zipper.Unzip(change.Diff);
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

        static void RunOnUiThread(Action action)
        {
            ThreadHelper.JoinableTaskFactory.Run(async delegate {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                action();
            });
        }
    }
}
