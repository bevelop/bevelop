using System;

namespace Bevelop.Messages
{
    public class FileChange
    {
        public FileAddress Address { get; set; }
        public string Branch { get; set; }
        public string User { get; set; }
        public byte[] DiffZip { get; set; }
        public DateTime Date { get; set; }
    }
}
