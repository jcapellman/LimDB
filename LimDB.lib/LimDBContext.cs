using System.Text.Json;
using LimDB.lib.Objects.Base;
using LimDB.lib.Sources;
using LimDB.lib.Sources.Base;

namespace LimDB.lib
{
    public class LimDbContext<T> where T : BaseObject
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly BaseStorageSource _storageSource;
        private readonly Lock _syncRoot = new();

        private List<T>? _dbObjects;

        private LimDbContext(BaseStorageSource storageSource)
        {
            _storageSource = storageSource;
        }

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

        /// <summary>
        /// Creates a LimDbContext from a Local Storage Source
        /// </summary>
        /// <param name="dbFileName">Full Path to the Db Filename</param>
        /// <returns>LimDbContext</returns>
        public static async Task<LimDbContext<T>> CreateFromLocalStorageSourceAsync(string dbFileName)
        {
            var lss = new LocalStorageSource(dbFileName);

            return await CreateAsync(lss);
        }

        /// <summary>
        /// Creates a LimDbContext from a custom BaseStorageSource - if you're just using Http or Local, use the wrappers
        /// </summary>
        /// <param name="storageSource">Custom BaseStorageSource</param>
        /// <returns>LimDbContext</returns>
        public static async Task<LimDbContext<T>> CreateAsync(BaseStorageSource storageSource)
        {
            var dbContext = new LimDbContext<T>(storageSource);
            await dbContext.InitializeAsync();

            return dbContext;
        }

        private async Task InitializeAsync()
        {
            var strDb = await _storageSource.GetDbAsync() ?? throw new ArgumentException("Database String was null");
            var tempDb = JsonSerializer.Deserialize<List<T>>(strDb, SerializerOptions);

            _dbObjects = tempDb ?? throw new ArgumentException("Db was null or empty");
        }

        public IEnumerable<T>? GetMany(Func<T, bool>? expression = null)
        {
            lock (_syncRoot)
            {
                if (_dbObjects is null)
                {
                    return null;
                }

                var query = expression is null ? _dbObjects : _dbObjects.Where(expression);
                return [.. query];
            }
        }

        public T? GetOne(Func<T, bool>? expression = null)
        {
            lock (_syncRoot)
            {
                if (_dbObjects is null)
                {
                    return null;
                }

                return expression is null ? _dbObjects.FirstOrDefault() : _dbObjects.FirstOrDefault(expression);
            }
        }

        public T? GetOneById(int id)
        {
            lock (_syncRoot)
            {
                return _dbObjects?.FirstOrDefault(a => a.Id == id);
            }
        }

        /// <summary>
        /// Deletes an object by the Id
        /// </summary>
        /// <param name="id">id of the object to remove</param>
        /// <returns>True if successful, false otherwise</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> DeleteByIdAsync(int id)
        {
            List<T> snapshot;

            lock (_syncRoot)
            {
                if (_dbObjects is null)
                {
                    return false;
                }

                var obj = _dbObjects.FirstOrDefault(a => a.Id == id) ?? throw new ArgumentException($"{id} was not found");

                _dbObjects.Remove(obj);
                snapshot = [.. _dbObjects];
            }

            return await _storageSource.WriteDbAsync(snapshot);
        }

        /// <summary>
        /// Inserts an object
        /// </summary>
        /// <param name="obj">Object to put into the database</param>
        /// <returns>Id of the new Object</returns>
        public async Task<int?> InsertAsync(T obj)
        {
            List<T> snapshot;
            int id;

            lock (_syncRoot)
            {
                if (_dbObjects is null)
                {
                    return null;
                }

                id = _dbObjects.Count == 0 ? 1 : _dbObjects.Max(a => a.Id) + 1;

                obj.Id = id;
                obj.Active = true;
                obj.Created = DateTime.Now;
                obj.Modified = DateTime.Now;

                _dbObjects.Add(obj);
                snapshot = [.. _dbObjects];
            }

            var result = await _storageSource.WriteDbAsync(snapshot);

            return result ? id : null;
        }

        /// <summary>
        /// Updates the object
        /// </summary>
        /// <param name="obj">Object to update</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateAsync(T obj)
        {
            List<T> snapshot;

            lock (_syncRoot)
            {
                if (_dbObjects is null)
                {
                    return false;
                }

                var index = _dbObjects.FindIndex(a => a.Id == obj.Id);

                if (index == -1)
                {
                    return false;
                }

                _dbObjects[index] = obj;
                snapshot = [.. _dbObjects];
            }

            return await _storageSource.WriteDbAsync(snapshot);
        }
    }
}