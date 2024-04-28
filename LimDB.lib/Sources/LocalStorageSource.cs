using LimDB.lib.Common;
using LimDB.lib.Sources.Base;

namespace LimDB.lib.Sources
{
    public class LocalStorageSource : BaseStorageSource
    {
        public LocalStorageSource(string dbFileName = LibConstants.DefaultDbFileName) : base(dbFileName) { }

        public override async Task<string> GetDbAsync()
        {
            if (!File.Exists(DbFileName))
            {
                throw new FileNotFoundException($"Could not find file {DbFileName}");
            }

            return await File.ReadAllTextAsync(DbFileName);
        }
    }
}