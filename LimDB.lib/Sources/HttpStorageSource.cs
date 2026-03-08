using LimDB.lib.Sources.Base;

namespace LimDB.lib.Sources
{
    /// <summary>
    /// HTTP-based storage source for reading database files from remote URLs.
    /// Note: HTTP sources are read-only. Write operations are not supported.
    /// </summary>
    public class HttpStorageSource(string url) : BaseStorageSource(url)
    {
        private static readonly HttpClient HttpClient = new();

        /// <summary>
        /// Retrieves the database content from the specified HTTP URL.
        /// </summary>
        /// <returns>Database content as a string</returns>
        public override async Task<string> GetDbAsync()
        {
            var response = await HttpClient.GetAsync(DbFileName);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Write operations are not supported for HTTP storage sources.
        /// </summary>
        /// <param name="jsonBytes">JSON data to write</param>
        /// <exception cref="NotSupportedException">Always thrown - HTTP sources are read-only</exception>
        protected override Task WriteAsync(ReadOnlyMemory<byte> jsonBytes)
        {
            throw new NotSupportedException(
                "Write operations are not supported for HTTP storage sources. " +
                "HTTP-based databases are read-only. Use LocalStorageSource for read/write operations.");
        }
    }
}