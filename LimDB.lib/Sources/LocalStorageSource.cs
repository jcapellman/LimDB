using System.Collections.Concurrent;
using LimDB.lib.Common;
using LimDB.lib.Sources.Base;

namespace LimDB.lib.Sources
{
    public class LocalStorageSource : BaseStorageSource
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new();
        private readonly SemaphoreSlim _fileLock;

        public LocalStorageSource(string dbFileName = LibConstants.DefaultDbFileName) : base(dbFileName)
        {
            _fileLock = FileLocks.GetOrAdd(dbFileName, _ => new SemaphoreSlim(1, 1));
        }

        public override async Task<string> GetDbAsync()
        {
            if (!File.Exists(DbFileName))
            {
                throw new FileNotFoundException($"Could not find file {DbFileName}");
            }

            return await File.ReadAllTextAsync(DbFileName);
        }

        protected override async Task WriteAsync(ReadOnlyMemory<byte> jsonBytes)
        {
            await _fileLock.WaitAsync();
            try
            {
                await using var fileStream = new FileStream(DbFileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: LibConstants.FileStreamBufferSize, useAsync: true);
                await fileStream.WriteAsync(jsonBytes);
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }
}
