using LimDB.lib.Sources.Base;

namespace LimDB.lib.Sources
{
    public class HttpStorageSource(string url) : BaseStorageSource(url)
    {
        private static readonly HttpClient HttpClient = new();

        public override async Task<string> GetDbAsync()
        {
            var response = await HttpClient.GetAsync(DbFileName);

            return await response.Content.ReadAsStringAsync();
        }

        protected override Task<bool> WriteAsync(ReadOnlyMemory<byte> jsonBytes)
        {
            throw new NotImplementedException();
        }
    }
}