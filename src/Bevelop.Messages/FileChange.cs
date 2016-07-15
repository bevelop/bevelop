namespace Bevelop.Messages
{
    public class FileChange
    {
        public string Branch { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string User { get; set; }
        public byte[] Diff { get; set; }
    }
}
