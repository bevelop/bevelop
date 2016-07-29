using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Bevelop.Messages;

namespace Bevelop.Server.Services
{
    public class FileChangeStore : IFileChangeStore
    {
        readonly IClock _clock;
        readonly ConcurrentDictionary<string, ConcurrentDictionary<string, FileChange>> _fileChangeCache;

        public FileChangeStore(IClock clock)
        {
            _clock = clock;
            _fileChangeCache = new ConcurrentDictionary<string, ConcurrentDictionary<string, FileChange>>();
        }

        public void Save(FileChange fileChange)
        {
            var compactAddress = CompactAddress(fileChange.Address);
            var userChangeAddress = CompactUser(fileChange);

            if (!_fileChangeCache.ContainsKey(compactAddress))
                _fileChangeCache[compactAddress] = new ConcurrentDictionary<string, FileChange>();

            _fileChangeCache[compactAddress][userChangeAddress] = fileChange;
        }

        public IList<FileChange> GetByAddress(FileAddress address)
        {
            var compactAddress = CompactAddress(address);
            if (!_fileChangeCache.ContainsKey(compactAddress))
                return new List<FileChange>();

            return _fileChangeCache[compactAddress].Values
                .Where(c => c.Date < _clock.UtcNow.AddDays(1))
                .ToList();
        }

        string CompactAddress(FileAddress address)
        {
            return $"{address.Repository}|{address.FilePath}";
        }

        string CompactUser(FileChange fileChange)
        {
            return $"{fileChange.User}|{fileChange.Branch}";
        }
    }
}