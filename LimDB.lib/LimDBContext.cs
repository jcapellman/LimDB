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
    /// <summary>
    /// Thread-safe in-memory database context for CRUD operations.
    /// Multiple LimDbContext instances can safely access the same file when using LocalStorageSource,
    /// as file-level locking is implemented per database file.
    /// All read and write operations are synchronized for concurrent access.
    /// </summary>
    /// <typeparam name="T">Entity type that inherits from BaseObject</typeparam>
    public class LimDbContext<T> : IDisposable where T : BaseObject
    {
        private static JsonTypeInfo<List<T>> _jsonTypeInfoForDeserialization = null!;
        private static JsonTypeInfo<IReadOnlyList<T>> _jsonTypeInfoForSerialization = null!;

        private readonly BaseStorageSource _storageSource;
        private readonly Lock _syncRoot = new();

        private List<T> _dbObjects = null!;
        private Dictionary<int, T> _idIndex = null!;
        private int _maxId;
        private bool _disposed;

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
        /// Creates a LimDbContext from an Http Storage Source with a custom JsonSerializerContext
        /// </summary>
        /// <param name="url">Full URL to the Database File</param>
        /// <param name="jsonContext">Custom JsonSerializerContext for AOT source generation</param>
        /// <returns>LimDbContext</returns>
        public static async Task<LimDbContext<T>> CreateFromHttpStorageSourceAsync(string url, JsonSerializerContext jsonContext)
        {
            var hss = new HttpStorageSource(url);

            return await CreateAsync(hss, jsonContext);
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
        /// Creates a LimDbContext from a Local Storage Source with a custom JsonSerializerContext
        /// </summary>
        /// <param name="dbFileName">Full Path to the Db Filename</param>
        /// <param name="jsonContext">Custom JsonSerializerContext for AOT source generation</param>
        /// <returns>LimDbContext</returns>
        public static async Task<LimDbContext<T>> CreateFromLocalStorageSourceAsync(string dbFileName, JsonSerializerContext jsonContext)
        {
            var lss = new LocalStorageSource(dbFileName);

            return await CreateAsync(lss, jsonContext);
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

            var listTypeInfo = context.GetTypeInfo(typeof(List<T>));
            var readOnlyListTypeInfo = context.GetTypeInfo(typeof(IReadOnlyList<T>));

            // Enforce AOT-only: List<T> must be registered for deserialization
            if (listTypeInfo is not JsonTypeInfo<List<T>> jsonListTypeInfo)
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(T).Name}' is not registered for AOT serialization. " +
                    $"Add [JsonSerializable(typeof(List<{typeof(T).Name}>))] to your JsonSerializerContext.");
            }

            _jsonTypeInfoForDeserialization = jsonListTypeInfo;

            // Try to use IReadOnlyList<T> for serialization if available, otherwise fallback to List<T>
            if (readOnlyListTypeInfo is JsonTypeInfo<IReadOnlyList<T>> jsonReadOnlyListTypeInfo)
            {
                _jsonTypeInfoForSerialization = jsonReadOnlyListTypeInfo;
            }
            else
            {
                // Fallback: Use List<T> for serialization (cast will work since List<T> implements IReadOnlyList<T>)
                _jsonTypeInfoForSerialization = (JsonTypeInfo<IReadOnlyList<T>>)(object)jsonListTypeInfo;
            }

            var dbContext = new LimDbContext<T>(storageSource);
            await dbContext.InitializeAsync();

            return dbContext;
        }

        private async Task InitializeAsync()
        {
            var strDb = await _storageSource.GetDbAsync() ?? throw new ArgumentException("Database String was null");

            // _jsonTypeInfoForDeserialization is guaranteed non-null by CreateAsync contract
            var tempDb = JsonSerializer.Deserialize(strDb, _jsonTypeInfoForDeserialization);

            _dbObjects = tempDb ?? throw new ArgumentException("Db was null or empty");
            _idIndex = _dbObjects.ToDictionary(obj => obj.Id);
            _maxId = _dbObjects.Count == 0 ? 0 : _dbObjects.Max(obj => obj.Id);
        }

        /// <summary>
        /// Retrieves multiple objects from the database, optionally filtered by an expression.
        /// This operation is thread-safe.
        /// </summary>
        /// <param name="expression">Optional filter expression. If null, returns all objects.</param>
        /// <returns>Read-only list of objects matching the criteria</returns>
        public IReadOnlyList<T> GetMany(Func<T, bool>? expression = null)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            lock (_syncRoot)
            {
                var query = expression is null ? _dbObjects : _dbObjects.Where(expression);
                return [.. query];
            }
        }

        /// <summary>
        /// Retrieves a single object from the database, optionally filtered by an expression.
        /// This operation is thread-safe.
        /// </summary>
        /// <param name="expression">Optional filter expression. If null, returns the first object.</param>
        /// <returns>First object matching the criteria, or null if not found</returns>
        public T? GetOne(Func<T, bool>? expression = null)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            lock (_syncRoot)
            {
                return expression is null ? _dbObjects.FirstOrDefault() : _dbObjects.FirstOrDefault(expression);
            }
        }

        /// <summary>
        /// Retrieves a single object by its unique identifier.
        /// This operation is thread-safe and uses O(1) dictionary lookup.
        /// </summary>
        /// <param name="id">The unique identifier of the object</param>
        /// <returns>The object with the specified ID, or null if not found</returns>
        public T? GetOneById(int id)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            lock (_syncRoot)
            {
                return _idIndex.TryGetValue(id, out var obj) ? obj : null;
            }
        }

        /// <summary>
        /// Deletes an object by the Id.
        /// This operation is thread-safe and persists changes to storage.
        /// </summary>
        /// <param name="id">id of the object to remove</param>
        /// <returns>True if successful</returns>
        /// <exception cref="ArgumentException">Thrown when the specified ID is not found</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed</exception>
        public async Task<bool> DeleteByIdAsync(int id)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            List<T> snapshot;

            lock (_syncRoot)
            {
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

            await _storageSource.WriteDbAsync(snapshot, _jsonTypeInfoForSerialization);
            return true;
        }

        /// <summary>
        /// Inserts an object into the database.
        /// The object's Id, Active, Created, and Modified properties are automatically set.
        /// This operation is thread-safe and persists changes to storage.
        /// </summary>
        /// <param name="obj">Object to put into the database</param>
        /// <returns>Id of the newly inserted object</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed</exception>
        public async Task<int> InsertAsync(T obj)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            List<T> snapshot;
            int id;

            lock (_syncRoot)
            {
                id = ++_maxId;

                obj.Id = id;
                obj.Active = true;
                obj.Created = DateTime.UtcNow;
                obj.Modified = DateTime.UtcNow;

                _dbObjects.Add(obj);
                _idIndex[id] = obj;
                snapshot = [.. _dbObjects];
            }

            await _storageSource.WriteDbAsync(snapshot, _jsonTypeInfoForSerialization);

            return id;
        }

        /// <summary>
        /// Updates an existing object in the database.
        /// The object's Modified timestamp is automatically updated to the current UTC time.
        /// This operation is thread-safe and persists changes to storage.
        /// </summary>
        /// <param name="obj">Object to update</param>
        /// <returns>True if successful, false if the object was not found</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the context has been disposed</exception>
        public async Task<bool> UpdateAsync(T obj)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            List<T> snapshot;

            lock (_syncRoot)
            {
                if (!_idIndex.TryGetValue(obj.Id, out var existingObj))
                {
                    return false;
                }

                obj.Modified = DateTime.UtcNow;
                var index = _dbObjects.IndexOf(existingObj);
                _dbObjects[index] = obj;
                _idIndex[obj.Id] = obj;
                snapshot = [.. _dbObjects];
            }

            await _storageSource.WriteDbAsync(snapshot, _jsonTypeInfoForSerialization);
            return true;
        }

        /// <summary>
        /// Disposes the database context and releases resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
