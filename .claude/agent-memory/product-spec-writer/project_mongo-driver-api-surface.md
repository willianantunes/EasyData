---
name: MongoDB Driver API Surface Usage in NDjango.Admin.MongoDB
description: Comprehensive inventory of MongoDB.Driver APIs used across the NDjango.Admin.MongoDB project, mapped to v2 vs v3 compatibility
type: project
---

The NDjango.Admin.MongoDB project uses the following MongoDB.Driver API surface:

**Core Collection APIs (used in NDjangoAdminManagerMongo, MongoAuthStorageQueries, tests):**
- `IMongoDatabase.GetCollection<T>(collectionName)` -- same in v2 and v3
- `collection.AsQueryable()` -- returns `IMongoQueryable<T>` in v2, `IQueryable<T>` in v3. NDjango casts with `as IQueryable<T>` which works in both.
- `collection.Find(filter)` / `collection.Find(session, filter)` -- same in both
- `.FirstOrDefault()` / `.FirstOrDefaultAsync(ct)` -- same in both
- `.ToList()` / `.ToListAsync(ct)` -- same in both
- `collection.InsertOneAsync(doc, cancellationToken: ct)` -- in v2, the overload is `InsertOneAsync(doc, InsertOneOptions, ct)` so named param `cancellationToken:` works. Same in v3.
- `collection.InsertManyAsync(docs)` / `InsertManyAsync(session, docs, cancellationToken: ct)` -- same in both
- `collection.ReplaceOneAsync(filter, replacement, cancellationToken: ct)` -- same signature in both
- `collection.DeleteOneAsync(filter, ct)` -- same in both
- `collection.DeleteManyAsync(filter, ct)` / `DeleteManyAsync(session, filter, cancellationToken: ct)` -- same in both
- `collection.UpdateOneAsync(filter, updateDef, cancellationToken: ct)` -- same in both
- `collection.CountDocumentsAsync(filter)` -- same in both
- `Builders<T>.Filter.Eq()` / `Builders<T>.Filter.In()` / `Builders<T>.Update.Set()` -- same in both
- `Builders<T>.IndexKeys.Ascending()` -- same in both
- `collection.Indexes.CreateOneAsync(CreateIndexModel<T>)` -- same in both
- `collection.Indexes.ListAsync()` -- returns `Task<IAsyncCursor<BsonDocument>>` in both
- `_database.Client.StartSessionAsync()` / `session.StartTransaction()` / `session.CommitTransactionAsync()` / `session.AbortTransactionAsync()` -- same in both
- `InsertManyOptions { IsOrdered = false }` -- same in both
- `MongoBulkWriteException` / `MongoWriteException` / `ServerErrorCategory.DuplicateKey` -- same in both (namespace `MongoDB.Driver`)
- `new MongoClient(connectionString)` -- same in both

**BSON APIs (used in entities and metadata loader):**
- `ObjectId`, `ObjectId.Parse()`, `ObjectId.GenerateNewId()` -- same in both
- `[BsonId]`, `[BsonIgnoreExtraElements]`, `[BsonIgnore]`, `[BsonElement("name")]` -- same in both
- `[BsonGuidRepresentation(GuidRepresentation.Standard)]` -- exists in both but in v2 with default `GuidRepresentationMode.V2`, behavior may differ. In v3, GuidRepresentationMode was removed.

**Key v2 vs v3 difference affecting this project:**
- `collection.AsQueryable()` return type: `IMongoQueryable<T>` (v2) vs `IQueryable<T>` (v3). No code change needed because `IMongoQueryable<T>` extends `IQueryable<T>`.
- `using MongoDB.Driver.Linq` is needed in v2 for `IMongoQueryable`. This import already exists in NDjangoAdminManagerMongo.cs.
- v2 default LinqProvider is V3 (LINQ3) since v2.14+, so LINQ expression translation should behave identically.
- v2 has `GuidRepresentationMode` (default: V2) which affects how Guid properties are serialized globally. The sample project uses `[BsonGuidRepresentation(GuidRepresentation.Standard)]` which overrides per-property, so this should work. However, consumers without per-property annotation may see different Guid serialization behavior.

**Why:** This inventory was created during the MongoDB.Driver v3-to-v2 downgrade analysis.
**How to apply:** Use this as a reference when making v2/v3 compatibility decisions for the MongoDB provider.
