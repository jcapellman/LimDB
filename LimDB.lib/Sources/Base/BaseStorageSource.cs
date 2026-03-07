using System.Diagnostics.CodeAnalysis;
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

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Fallback to reflection-based JSON serialization when source generation is not available")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "Fallback to reflection-based JSON serialization when source generation is not available")]
        public async Task<bool> WriteDbAsync<T>(List<T> objects, JsonTypeInfo<List<T>>? jsonTypeInfo)
        {
            // Use source generation if available, otherwise fall back to reflection
            var json = jsonTypeInfo != null
                ? JsonSerializer.Serialize(objects, jsonTypeInfo)
                : JsonSerializer.Serialize(objects, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true });

            return await WriteAsync(json);
        }
    }
}
