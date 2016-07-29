using System.Collections.Generic;
using Bevelop.Messages;

namespace Bevelop.Server.Services
{
    public interface IFileChangeStore
    {
        void Save(FileChange fileChange);

        IList<FileChange> GetByAddress(FileAddress address);
    }
}