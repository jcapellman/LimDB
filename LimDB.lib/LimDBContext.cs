using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using LimDB.lib.Json;
using LimDB.lib.Objects.Base;
using LimDB.lib.Sources;
using LimDB.lib.Sources.Base;

namespace LimDB.lib
{
    public class LimDbContext<T> where T : BaseObject
    {
        private static JsonTypeInfo<List<T>> _jsonTypeInfo = null!;

        private readonly BaseStorageSource _storageSource;
        private readonly Lock _syncRoot = new();

        private List<T>? _dbObjects;
        private Dictionary<int, T>? _idIndex;
        private int _maxId;

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
            return await CreateAsync(storageSource, null);
        }

        /// <summary>
        /// Creates a LimDbContext from a custom BaseStorageSource with a custom JsonSerializerContext
        /// </summary>
        /// <param name="storageSource">Custom BaseStorageSource</param>
        /// <param name="jsonContext">Custom JsonSerializerContext for AOT source generation</param>
        /// <returns>LimDbContext</returns>
        public static async Task<LimDbContext<T>> CreateAsync(BaseStorageSource storageSource, JsonSerializerContext? jsonContext)
        {
            // Try to get type info from the provided context, or fallback to default LimDbJsonContext
            var context = jsonContext ?? LimDbJsonContext.Default;
            var typeInfo = context.GetTypeInfo(typeof(List<T>));

            // Enforce AOT-only: type must be registered in source generation
            if (typeInfo is not JsonTypeInfo<List<T>> jsonTypeInfo)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(T).Name}' is not registered for AOT serialization. " +
                    $"Add [JsonSerializable(typeof(List<{typeof(T).Name}>))] to your JsonSerializerContext.");
            }

            _jsonTypeInfo = jsonTypeInfo;

            var dbContext = new LimDbContext<T>(storageSource);
            await dbContext.InitializeAsync();

            return dbContext;
        }

        private async Task InitializeAsync()
        {
            var strDb = await _storageSource.GetDbAsync() ?? throw new ArgumentException("Database String was null");

            // _jsonTypeInfo is guaranteed non-null by CreateAsync contract
            var tempDb = JsonSerializer.Deserialize(strDb, _jsonTypeInfo);

            _dbObjects = tempDb ?? throw new ArgumentException("Db was null or empty");
            _idIndex = _dbObjects.ToDictionary(obj => obj.Id);
            _maxId = _dbObjects.Count == 0 ? 0 : _dbObjects.Max(obj => obj.Id);
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
                return _idIndex?.TryGetValue(id, out var obj) == true ? obj : null;
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
                if (_dbObjects is null || _idIndex is null)
                {
                    return false;
                }

                if (!_idIndex.TryGetValue(id, out var obj))
                {
                    throw new ArgumentException($"{id} was not found");
                }

                _dbObjects.Remove(obj);
                _idIndex.Remove(id);

                if (_dbObjects.Count == 0)
                {
                    _maxId = 0;
                }

                snapshot = [.. _dbObjects];
            }

            return await _storageSource.WriteDbAsync(snapshot, _jsonTypeInfo);
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
                if (_dbObjects is null || _idIndex is null)
                {
                    return null;
                }

                id = ++_maxId;

                obj.Id = id;
                obj.Active = true;
                obj.Created = DateTime.UtcNow;
                obj.Modified = DateTime.UtcNow;

                _dbObjects.Add(obj);
                _idIndex[id] = obj;
                snapshot = [.. _dbObjects];
            }

            var result = await _storageSource.WriteDbAsync(snapshot, _jsonTypeInfo);

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
                if (_dbObjects is null || _idIndex is null)
                {
                    return false;
                }

                if (!_idIndex.TryGetValue(obj.Id, out var existingObj))
                {
                    return false;
                }

                var index = _dbObjects.IndexOf(existingObj);
                _dbObjects[index] = obj;
                _idIndex[obj.Id] = obj;
                snapshot = [.. _dbObjects];
            }

            return await _storageSource.WriteDbAsync(snapshot, _jsonTypeInfo);
        }
    }
}