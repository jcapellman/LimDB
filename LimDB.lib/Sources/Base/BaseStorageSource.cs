using System.Text.Json;
using LimDB.lib.Common;

namespace LimDB.lib.Sources.Base
{
    public abstract class BaseStorageSource(string dbFileName = LibConstants.DefaultDbFileName)
    {
        protected readonly string DbFileName = dbFileName;

        public abstract Task<string> GetDbAsync();

        protected abstract Task<bool> WriteAsync(string json);

        public async Task<bool> WriteDbAsync<T>(List<T> objects)
        {
            var json = JsonSerializer.Serialize(objects);

            return await WriteAsync(json);
        }
    }
}