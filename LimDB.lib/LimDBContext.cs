using System.Text.Json;

using LimDB.lib.Objects.Base;
using LimDB.lib.Sources.Base;

namespace LimDB.lib
{
    public class LimDbContext<T>(BaseStorageSource storageSource) where T : BaseObject
    {
        private readonly BaseStorageSource _storageSource = storageSource;

        private List<T> _dbObjects = new();

        public static async Task<LimDbContext<T>> CreateAsync(BaseStorageSource storageSource)
        {
            var dbContext = new LimDbContext<T>(storageSource);
            await dbContext.InitializeAsync();

            return dbContext;
        }

        private async Task InitializeAsync()
        {
            var strDb = await _storageSource.GetDbAsync();

            var tempDb = JsonSerializer.Deserialize<List<T>>(strDb);

            _dbObjects = tempDb ?? throw new ArgumentNullException($"Db was null or empty");
        }

        public IEnumerable<T> GetMany(Func<T, bool>? expression = null) => expression is null ? _dbObjects : _dbObjects.Where(expression);

        public T? GetOne(Func<T, bool>? expression = null) =>
            expression is null ? _dbObjects.FirstOrDefault() : _dbObjects.FirstOrDefault(expression);

        public T? GetOneById(int id) => _dbObjects.FirstOrDefault(a => a.Id == id);
    }
}