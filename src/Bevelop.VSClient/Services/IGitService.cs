using Bevelop.Messages;

namespace Bevelop.VSClient.Services
{
    public interface IGitService
    {
        bool IsInRepo(string path);
        IGitFileChangeDetector GetFileChangeDetector(string path);
        FileAddress GetFileAddress(string path);
    }
}