using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LimDB.lib.Common;

namespace LimDB.lib.Sources.Base
{
    public abstract class BaseStorageSource(string dbFileName = LibConstants.DefaultDbFileName)
    {
        protected readonly string DbFileName = dbFileName;

        public abstract Task<string> GetDbAsync();

        protected abstract Task<bool> WriteAsync(string json);

        public async Task<bool> WriteDbAsync<T>(List<T> objects, JsonTypeInfo<List<T>> jsonTypeInfo)
        {
            var json = JsonSerializer.Serialize(objects, jsonTypeInfo);

            return await WriteAsync(json);
        }
    }
}
