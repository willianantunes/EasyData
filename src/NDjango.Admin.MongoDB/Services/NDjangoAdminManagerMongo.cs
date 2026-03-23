using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NDjango.Admin.Services;

namespace NDjango.Admin.MongoDB
{
    public class NDjangoAdminManagerMongo : NDjangoAdminManager
    {
        private readonly MongoDbOptions _mongoOptions;
        private readonly IMongoDatabase _database;

        private static readonly MethodInfo _queryRecordsGeneric;
        private static readonly MethodInfo _countRecordsGeneric;
        private static readonly MethodInfo _fetchRecordGeneric;
        private static readonly MethodInfo _fetchRecordsByKeysGeneric;

        static NDjangoAdminManagerMongo()
        {
            var methods = typeof(NDjangoAdminManagerMongo)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.IsGenericMethodDefinition)
                .ToList();

            _queryRecordsGeneric = methods.Single(m => m.Name == nameof(QueryRecords));
            _countRecordsGeneric = methods.Single(m => m.Name == nameof(CountRecordsAsync));
            _fetchRecordGeneric = methods.Single(m => m.Name == nameof(FetchRecordInternal));
            _fetchRecordsByKeysGeneric = methods.Single(m => m.Name == nameof(FetchRecordsByKeysInternal));
        }

        public NDjangoAdminManagerMongo(IServiceProvider services, NDjangoAdminOptions options, MongoDbOptions mongoOptions)
            : base(services, options)
        {
            _mongoOptions = mongoOptions ?? throw new ArgumentNullException(nameof(mongoOptions));
            _database = (IMongoDatabase)services.GetService(typeof(IMongoDatabase))
                ?? throw new InvalidOperationException("IMongoDatabase is not registered in the service provider.");
        }

        public override Task LoadModelAsync(string modelId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            Model.Id = modelId;

            var loaderOptions = new MongoMetaDataLoaderOptions();
            Options.MetaDataLoaderOptionsBuilder?.Invoke(loaderOptions);

            var loader = new MongoMetaDataLoader(Model, _mongoOptions.Collections, loaderOptions);
            loader.LoadFromCollections();

            return base.LoadModelAsync(modelId, ct);
        }

        public override async Task<IEnumerable<EasySorter>> GetDefaultSortersAsync(string modelId, string sourceId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await GetModelAsync(modelId);
            var modelEntity = FindEntityBySourceId(sourceId);

            return modelEntity.Attributes
                .Where(a => a.Sorting != 0)
                .OrderBy(a => Math.Abs(a.Sorting))
                .Select(a => new EasySorter
                {
                    FieldName = a.PropName,
                    Direction = a.Sorting > 0 ? SortDirection.Ascending : SortDirection.Descending
                });
        }

        public override async Task<NDjangoAdminResultSet> FetchDatasetAsync(
            string modelId, string sourceId,
            IEnumerable<EasyFilter> filters = null,
            IEnumerable<EasySorter> sorters = null,
            bool isLookup = false, int? offset = null, int? fetch = null,
            CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (filters == null)
                filters = Enumerable.Empty<EasyFilter>();

            await GetModelAsync(modelId);

            var modelEntity = FindEntityBySourceId(sourceId);
            var collectionName = modelEntity.DbSetName;
            var entityType = modelEntity.ClrType;

            var targetMethod = _queryRecordsGeneric.MakeGenericMethod(entityType);
            var records = (IEnumerable)targetMethod.Invoke(this,
                new object[] { collectionName, modelEntity, filters, sorters, isLookup, offset, fetch, ct });

            var result = new NDjangoAdminResultSet();

            var attrs = modelEntity.Attributes.Where(attr => attr.Kind != EntityAttrKind.Lookup).ToList();

            foreach (var attr in attrs) {
                var dataType = attr.DataType;
                var dfmt = attr.DisplayFormat;

                if (string.IsNullOrEmpty(dfmt)) {
                    dfmt = Model.DisplayFormats.GetDefault(attr.DataType)?.Format;
                }

                result.Cols.Add(new NDjangoAdminCol(new NDjangoAdminColDesc
                {
                    Id = attr.Id,
                    Label = attr.Caption,
                    AttrId = attr.Id,
                    DisplayFormat = dfmt,
                    DataType = dataType,
                    Description = attr.Description
                }));
            }

            foreach (var record in records) {
                var values = attrs.Select(attr => {
                    if (attr.PropInfo == null)
                        return null;

                    var value = attr.PropInfo.GetValue(record);
                    return NormalizeValue(value);
                }).ToList();
                result.Rows.Add(new NDjangoAdminRow(values));
            }

            return result;
        }

        public override async Task<long> GetTotalRecordsAsync(string modelId, string sourceId,
            IEnumerable<EasyFilter> filters = null, bool isLookup = false, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (filters == null)
                filters = Enumerable.Empty<EasyFilter>();

            await GetModelAsync(modelId);

            var modelEntity = FindEntityBySourceId(sourceId);
            var collectionName = modelEntity.DbSetName;
            var entityType = modelEntity.ClrType;

            var targetMethod = _countRecordsGeneric.MakeGenericMethod(entityType);
            return await (Task<long>)targetMethod.Invoke(this,
                new object[] { collectionName, modelEntity, filters, isLookup, ct });
        }

        public override async Task<object> FetchRecordAsync(string modelId, string sourceId,
            Dictionary<string, string> recordKeys, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await GetModelAsync(modelId);

            var modelEntity = FindEntityBySourceId(sourceId);
            var collectionName = modelEntity.DbSetName;
            var entityType = modelEntity.ClrType;

            var pkAttr = modelEntity.Attributes.FirstOrDefault(a => a.IsPrimaryKey);
            if (pkAttr == null)
                throw new NDjangoAdminManagerException($"No primary key found for entity {sourceId}");

            var keyValue = recordKeys.Values.First();

            var targetMethod = _fetchRecordGeneric.MakeGenericMethod(entityType);
            var record = targetMethod.Invoke(this, new object[] { collectionName, pkAttr.PropInfo, keyValue });

            if (record == null) {
                throw new RecordNotFoundException(sourceId, keyValue);
            }

            return record;
        }

        public override async Task<IReadOnlyList<object>> FetchRecordsByKeysAsync(string modelId, string sourceId,
            IReadOnlyList<Dictionary<string, string>> recordKeysList, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            await GetModelAsync(modelId);

            var modelEntity = FindEntityBySourceId(sourceId);
            var collectionName = modelEntity.DbSetName;
            var entityType = modelEntity.ClrType;

            var pkAttr = modelEntity.Attributes.FirstOrDefault(a => a.IsPrimaryKey);
            if (pkAttr == null)
                throw new NDjangoAdminManagerException($"No primary key found for entity {sourceId}");

            var keyValues = recordKeysList.Select(rk => rk.Values.First()).ToList();

            var targetMethod = _fetchRecordsByKeysGeneric.MakeGenericMethod(entityType);
            return (IReadOnlyList<object>)targetMethod.Invoke(this,
                new object[] { collectionName, pkAttr.PropInfo, keyValues });
        }

        public override Task<object> CreateRecordAsync(string modelId, string sourceId, JObject props, CancellationToken ct = default)
        {
            throw new NotSupportedException("MongoDB provider V1 is read-only.");
        }

        public override Task<object> UpdateRecordAsync(string modelId, string sourceId, JObject props, CancellationToken ct = default)
        {
            throw new NotSupportedException("MongoDB provider V1 is read-only.");
        }

        public override Task DeleteRecordAsync(string modelId, string sourceId, JObject props, CancellationToken ct = default)
        {
            throw new NotSupportedException("MongoDB provider V1 is read-only.");
        }

        public override Task DeleteRecordsByKeysAsync(string modelId, string sourceId,
            IReadOnlyList<Dictionary<string, string>> recordKeysList, CancellationToken ct = default)
        {
            throw new NotSupportedException("MongoDB provider V1 is read-only.");
        }

        // --- Private generic methods invoked via reflection ---

        private IEnumerable QueryRecords<T>(string collectionName, MetaEntity entity,
            IEnumerable<EasyFilter> filters, IEnumerable<EasySorter> sorters,
            bool isLookup, int? offset, int? fetch, CancellationToken ct) where T : class
        {
            ct.ThrowIfCancellationRequested();

            var collection = _database.GetCollection<T>(collectionName);
            var query = collection.AsQueryable() as IQueryable<T>;

            if (filters != null) {
                foreach (var filter in filters) {
                    query = (IQueryable<T>)filter.Apply(entity, isLookup, query);
                }
            }

            if (sorters != null) {
                using (var e = sorters.GetEnumerator()) {
                    if (e.MoveNext()) {
                        var sorter = e.Current;
                        var isDescending = sorter.Direction == SortDirection.Descending;
                        var orderedQuery = query.OrderBy(sorter.FieldName, isDescending);
                        while (e.MoveNext()) {
                            sorter = e.Current;
                            isDescending = sorter.Direction == SortDirection.Descending;
                            orderedQuery = orderedQuery.ThenBy(sorter.FieldName, isDescending);
                        }
                        query = orderedQuery.AsQueryable();
                    }
                }
            }

            if (offset.HasValue) {
                query = query.Skip(offset.Value);
            }

            if (fetch.HasValue) {
                query = query.Take(fetch.Value);
            }

            return query.ToList();
        }

        private async Task<long> CountRecordsAsync<T>(string collectionName, MetaEntity entity,
            IEnumerable<EasyFilter> filters, bool isLookup, CancellationToken callerToken) where T : class
        {
            var collection = _database.GetCollection<T>(collectionName);
            var query = collection.AsQueryable() as IQueryable<T>;

            foreach (var filter in filters) {
                query = (IQueryable<T>)filter.Apply(entity, isLookup, query);
            }

            if (Options.PaginationCountTimeoutMs <= 0)
                return query.LongCount();

            using var timeoutCts = new CancellationTokenSource(Options.PaginationCountTimeoutMs);
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(callerToken, timeoutCts.Token);
            try {
                return await Task.Run(() => query.LongCount(), linkedCts.Token);
            }
            catch (OperationCanceledException) when (!callerToken.IsCancellationRequested) {
                return NDjangoAdminOptions.PaginationCountFallbackValue;
            }
        }

        private object FetchRecordInternal<T>(string collectionName, PropertyInfo pkProperty, string keyString) where T : class
        {
            var parsedKey = ParseKey(pkProperty.PropertyType, keyString);
            var collection = _database.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("_id", parsedKey);
            return collection.Find(filter).FirstOrDefault();
        }

        private IReadOnlyList<object> FetchRecordsByKeysInternal<T>(string collectionName, PropertyInfo pkProperty,
            List<string> keyStrings) where T : class
        {
            var parsedKeys = keyStrings.Select(k => ParseKey(pkProperty.PropertyType, k)).ToList();
            var collection = _database.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.In("_id", parsedKeys);
            return collection.Find(filter).ToList().Cast<object>().ToList();
        }

        // --- Helpers ---

        private MetaEntity FindEntityBySourceId(string sourceId)
        {
            var entity = Model.EntityRoot.SubEntities.FirstOrDefault(e => e.Id == sourceId);
            if (entity == null) {
                throw new ContainerNotFoundException(sourceId);
            }
            return entity;
        }

        private static object ParseKey(Type keyType, string keyString)
        {
            if (keyType == typeof(ObjectId))
                return ObjectId.Parse(keyString);
            if (keyType == typeof(Guid))
                return Guid.Parse(keyString);
            if (keyType == typeof(int))
                return int.Parse(keyString);
            if (keyType == typeof(long))
                return long.Parse(keyString);
            if (keyType == typeof(string))
                return keyString;

            return Convert.ChangeType(keyString, keyType);
        }

        private static object NormalizeValue(object value)
        {
            if (value == null)
                return null;

            if (value is ObjectId objectId)
                return objectId.ToString();

            var type = value.GetType();
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal)
                || type == typeof(DateTime) || type == typeof(DateTimeOffset)
                || type == typeof(Guid) || type == typeof(TimeSpan)
                || type == typeof(DateOnly) || type == typeof(TimeOnly)
                || type.IsEnum) {
                return value;
            }

            // Collection or complex type: serialize to JSON string
            return JsonConvert.SerializeObject(value);
        }
    }
}
