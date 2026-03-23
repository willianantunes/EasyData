using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using NDjango.Admin.MongoDB;
using Newtonsoft.Json;
using Xunit;

namespace NDjango.Admin.MongoDB.Tests
{
    #region Test document classes for filter

    public class FilterTestDocument
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
    }

    public class FilterSearchFieldsDocument : IAdminSettings<FilterSearchFieldsDocument>
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Secret { get; set; }
        public PropertyList<FilterSearchFieldsDocument> SearchFields =>
            new(x => x.Name, x => x.Description);
    }

    #endregion

    public class MongoSubstringFilterTests
    {
        private MetaData BuildModel<T>(string collectionName = "test")
        {
            var model = new MetaData();
            var collections = new List<MongoCollectionDescriptor>
            {
                new MongoCollectionDescriptor(typeof(T), collectionName)
            };
            var options = new MongoMetaDataLoaderOptions { HidePrimaryKeys = false };
            var loader = new MongoMetaDataLoader(model, collections, options);
            loader.LoadFromCollections();
            return model;
        }

        private async Task<MongoSubstringFilter> CreateFilterAsync(MetaData model, string searchText)
        {
            var filter = new MongoSubstringFilter(model);
            var json = $"{{\"value\":\"{searchText}\"}}";
            using var reader = new JsonTextReader(new StringReader(json));
            await filter.ReadFromJsonAsync(reader);
            return filter;
        }

        [Fact]
        public async Task Filter_ReturnsMatchingItems()
        {
            var model = BuildModel<FilterTestDocument>();
            var entity = model.EntityRoot.SubEntities.Single();

            var data = new List<FilterTestDocument>
            {
                new() { Id = ObjectId.GenerateNewId(), Name = "Alice", Description = "First", Score = 10 },
                new() { Id = ObjectId.GenerateNewId(), Name = "Bob", Description = "Second", Score = 20 },
                new() { Id = ObjectId.GenerateNewId(), Name = "Charlie", Description = "Third about alice", Score = 30 }
            }.AsQueryable();

            var filter = await CreateFilterAsync(model, "alice");
            var result = ((IQueryable<FilterTestDocument>)filter.Apply(entity, false, data)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, d => d.Name == "Alice");
            Assert.Contains(result, d => d.Name == "Charlie");
        }

        [Fact]
        public async Task Filter_EmptySearchText_ReturnsAll()
        {
            var model = BuildModel<FilterTestDocument>();
            var entity = model.EntityRoot.SubEntities.Single();

            var data = new List<FilterTestDocument>
            {
                new() { Id = ObjectId.GenerateNewId(), Name = "Alice", Description = "First", Score = 10 },
                new() { Id = ObjectId.GenerateNewId(), Name = "Bob", Description = "Second", Score = 20 }
            }.AsQueryable();

            var filter = await CreateFilterAsync(model, "");
            var result = ((IQueryable<FilterTestDocument>)filter.Apply(entity, false, data)).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Filter_IsCaseInsensitive()
        {
            var model = BuildModel<FilterTestDocument>();
            var entity = model.EntityRoot.SubEntities.Single();

            var data = new List<FilterTestDocument>
            {
                new() { Id = ObjectId.GenerateNewId(), Name = "ALICE", Description = "First", Score = 10 },
                new() { Id = ObjectId.GenerateNewId(), Name = "bob", Description = "Second", Score = 20 }
            }.AsQueryable();

            var filter = await CreateFilterAsync(model, "Alice");
            var result = ((IQueryable<FilterTestDocument>)filter.Apply(entity, false, data)).ToList();

            Assert.Single(result);
            Assert.Equal("ALICE", result[0].Name);
        }

        [Fact]
        public async Task Filter_OnlySearchesStringProperties()
        {
            var model = BuildModel<FilterTestDocument>();
            var entity = model.EntityRoot.SubEntities.Single();

            var data = new List<FilterTestDocument>
            {
                new() { Id = ObjectId.GenerateNewId(), Name = "Item", Description = "Desc", Score = 10 },
                new() { Id = ObjectId.GenerateNewId(), Name = "Other", Description = "Other", Score = 20 }
            }.AsQueryable();

            // "10" is the Score value - should not match because Score is int, not string
            var filter = await CreateFilterAsync(model, "10");
            var result = ((IQueryable<FilterTestDocument>)filter.Apply(entity, false, data)).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task Filter_WithSearchFields_OnlySearchesSpecifiedFields()
        {
            var model = BuildModel<FilterSearchFieldsDocument>();
            var entity = model.EntityRoot.SubEntities.Single();

            var data = new List<FilterSearchFieldsDocument>
            {
                new() { Id = ObjectId.GenerateNewId(), Name = "Alice", Description = "Something", Secret = "hidden alice" },
                new() { Id = ObjectId.GenerateNewId(), Name = "Bob", Description = "alice related", Secret = "hidden" }
            }.AsQueryable();

            var filter = await CreateFilterAsync(model, "alice");
            var result = ((IQueryable<FilterSearchFieldsDocument>)filter.Apply(entity, false, data)).ToList();

            // "Alice" matches Name (search field), "alice related" matches Description (search field)
            // "hidden alice" should NOT match because Secret is not a search field
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Filter_WithSearchFields_DoesNotSearchNonSearchFields()
        {
            var model = BuildModel<FilterSearchFieldsDocument>();
            var entity = model.EntityRoot.SubEntities.Single();

            var data = new List<FilterSearchFieldsDocument>
            {
                new() { Id = ObjectId.GenerateNewId(), Name = "Bob", Description = "Something", Secret = "hidden match" }
            }.AsQueryable();

            var filter = await CreateFilterAsync(model, "hidden");
            var result = ((IQueryable<FilterSearchFieldsDocument>)filter.Apply(entity, false, data)).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public async Task Filter_HandlesNullStringProperties()
        {
            var model = BuildModel<FilterTestDocument>();
            var entity = model.EntityRoot.SubEntities.Single();

            var data = new List<FilterTestDocument>
            {
                new() { Id = ObjectId.GenerateNewId(), Name = null, Description = "alice", Score = 10 },
                new() { Id = ObjectId.GenerateNewId(), Name = "alice", Description = null, Score = 20 }
            }.AsQueryable();

            var filter = await CreateFilterAsync(model, "alice");
            var result = ((IQueryable<FilterTestDocument>)filter.Apply(entity, false, data)).ToList();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FilterClass_MatchesSubstringConstant()
        {
            Assert.Equal("__substring", MongoSubstringFilter.Class);
        }
    }
}
