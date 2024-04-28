using LimDB.lib.Sources.Base;

namespace LimDB.lib.Sources
{
    public class HttpStorageSource(string dbFileName) : BaseStorageSource(dbFileName)
    {
        public override async Task<string> GetDbAsync()
        {
            var httpClient = new HttpClient();

            var response = await httpClient.GetAsync(DbFileName);

            return await response.Content.ReadAsStringAsync();
        }
    }
}