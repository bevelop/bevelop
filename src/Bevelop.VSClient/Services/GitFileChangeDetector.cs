using System;
using System.IO;
using System.Linq;
using Bevelop.Messages;
using LibGit2Sharp;

namespace Bevelop.VSClient.Services
{
    public class GitFileChangeDetector : IGitFileChangeDetector
    {
        readonly Repository _repository;
        readonly IZipper _zipper;
        FileSystemWatcher _fileSystemWatcher;

        public event EventHandler<FileLocallyChangedEventArgs> FileLocallyChanged = delegate { };
        public event EventHandler<EventArgs> FileReset = delegate { };

        public FileAddress FileAddress { get; }
        public string FullPath { get; }

        public GitFileChangeDetector(Repository repository, string fullPath, IZipper zipper)
        {
            _repository = repository;
            FullPath = fullPath;
            _zipper = zipper;
            var repoUri = new Uri(repository.Network.Remotes.First().Url);
            var repo = repoUri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.UserInfo, UriFormat.UriEscaped);

            FileAddress = new FileAddress
            {
                Repository = repo,
                FilePath = GetRepoRelativePath()
            };

            SetupFileWatch();
        }

        public void Dispose()
        {
            _fileSystemWatcher.Dispose();
        }

        string GetRepoRelativePath()
        {
            var fullUri = new Uri(FullPath);
            var rootUri = new Uri(_repository.Info.Path);
            var relative = rootUri.MakeRelativeUri(fullUri).ToString();
            var relativePath = relative.Substring(relative.IndexOf('/') + 1);
            relativePath = relativePath.Replace('/', '\\');

            return relativePath;
        }

        void SetupFileWatch()
        {
            _fileSystemWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(FullPath),
                Filter = Path.GetFileName(FullPath),
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
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

                var modified = _repository.RetrieveStatus().Modified
                    .FirstOrDefault(m => m.FilePath.Equals(FileAddress.FilePath, StringComparison.OrdinalIgnoreCase));

                if (modified != null)
                {
                    var zippedText = _zipper.Zip(File.ReadAllText(FullPath));

                    var fileChange = new FileChange
                    {
                        User = _repository.Config.Get<string>("user.name").Value,
                        Branch = _repository.Head.FriendlyName,
                        DiffZip = zippedText,
                        Address = FileAddress
                    };

                    FileLocallyChanged(this, new FileLocallyChangedEventArgs(fileChange));
                }
                else
                {
                    FileReset(this, new EventArgs());
                }
            }
            finally
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }
    }
}