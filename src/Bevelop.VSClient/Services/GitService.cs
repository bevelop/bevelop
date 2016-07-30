using System.IO;
using LibGit2Sharp;

namespace Bevelop.VSClient.Services
{
    public class GitService : IGitService
    {
        readonly IZipper _zipper;

        public GitService(IZipper zipper)
        {
            _zipper = zipper;
        }

        public bool IsInRepo(string path)
        {
            return GetRepoRoot(path) != null;
        }

        public IGitFileChangeDetector GetFileChangeDetector(string path)
        {
            var repository = new Repository(GetRepoRoot(path));
            return new GitFileChangeDetector(repository, path, _zipper);
        }

        string GetRepoRoot(string path)
        {
            var root = Path.GetDirectoryName(path);
            while (root != null && !Directory.Exists(Path.Combine(root, ".git")))
            {
                var parent = Directory.GetParent(root);
                root = parent?.FullName;
            }

            return root;
        }
    }
}