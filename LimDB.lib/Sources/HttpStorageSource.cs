using LimDB.lib.Sources.Base;

namespace LimDB.lib.Sources
{
    public class HttpStorageSource(string url) : BaseStorageSource(url)
    {
        public override async Task<string> GetDbAsync()
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(DbFileName);

            return await response.Content.ReadAsStringAsync();
        }

        protected override Task<bool> WriteAsync(string json)
        {
            throw new NotImplementedException();
        }
    }
}