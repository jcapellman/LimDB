namespace LimDB.lib.Common
{
    public class LibConstants
    {
        public const string DefaultDbFileName = "db.file";

        // JSON Serialization Buffer Sizes
        public const int JsonBufferInitialCapacity = 256 * 1024; // 256 KB
        public const int JsonBufferDefaultSizeHint = 4096; // 4 KB

        // File I/O Buffer Size
        public const int FileStreamBufferSize = 81920; // 80 KB
    }
}