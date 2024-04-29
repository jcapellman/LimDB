using System.Text.Json;

using LimDB.lib.Objects.Base;
using LimDB.lib.Sources;
using LimDB.lib.Sources.Base;

namespace LimDB.lib
{
    public class LimDbContext<T>(BaseStorageSource storageSource) where T : BaseObject
    {
        private List<T>? _dbObjects;

        /// <summary>
        /// Creates a LimDbContext from an Http Storage Source
        /// </summary>
        /// <param name="url">Full URL to the Database File</param>
        /// <returns>LimDbContext</returns>
        public static async Task<LimDbContext<T>> CreateFromHttpStorageSourceAsync(string url)
        {
            var hss = new HttpStorageSource(url);

            return await CreateAsync(hss);
        }

        public static async Task<LimDbContext<T>> CreateAsync(BaseStorageSource storageSource)
        {
            var dbContext = new LimDbContext<T>(storageSource);
            await dbContext.InitializeAsync();

            return dbContext;
        }

        private async Task InitializeAsync()
        {
            var strDb = await storageSource.GetDbAsync() ?? throw new ArgumentException("Database String was null");
            var tempDb = JsonSerializer.Deserialize<List<T>>(strDb);

            _dbObjects = tempDb ?? throw new ArgumentException($"Db was null or empty");
        }

        public IEnumerable<T>? GetMany(Func<T, bool>? expression = null) => expression is null ? _dbObjects : _dbObjects?.Where(expression);

        public T? GetOne(Func<T, bool>? expression = null) =>
            expression is null ? _dbObjects?.FirstOrDefault() : _dbObjects?.FirstOrDefault(expression);

        public T? GetOneById(int id) => _dbObjects?.FirstOrDefault(a => a.Id == id);
    }
}