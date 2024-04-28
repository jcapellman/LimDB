using LimDB.lib.Common;

namespace LimDB.lib.Sources.Base
{
    public  abstract class BaseStorageSource(string dbFileName = LibConstants.DefaultDbFileName)
    {
        protected readonly string DbFileName = dbFileName;

        public abstract Task<string> GetDbAsync();
    }
}