using LimDB.lib.Common;
using LimDB.lib.Sources.Base;

namespace LimDB.lib.Sources
{
    public class LocalStorageSource(string dbFileName = LibConstants.DefaultDbFileName) : BaseStorageSource(dbFileName)
    {
        public override async Task<string> GetDbAsync()
        {
            if (!File.Exists(DbFileName))
            {
                throw new FileNotFoundException($"Could not find file {DbFileName}");
            }

            return await File.ReadAllTextAsync(DbFileName);
        }

        protected override async Task<bool> WriteAsync(string json)
        {
            await File.WriteAllTextAsync(DbFileName, json);

            return true;
        }
    }
}