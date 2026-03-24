using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace NDjango.Admin.EntityFrameworkCore.Relational.Tests
{
    public class DbContextMetadataLoaderTests
    {
        private readonly TestDbContext _dbContext;

        public DbContextMetadataLoaderTests()
        {
            _dbContext = TestDbContext.Create();
        }

        /// <summary>
        /// Checking if we get all the entities and attributes from the testing DbContext.
        /// </summary>
        [Fact]
        public void LoadFromDbContextTest()
        {
            var meta = new MetaData();

            meta.LoadFromDbContext(_dbContext);

            Assert.Equal(8, meta.EntityRoot.SubEntities.Count);

            var entityAttrCount = new Dictionary<string, int>()
            {
                ["Category"] = 4,
                ["Customer"] = 11,
                ["Employee"] = 19,
                ["Order"] = 16,
                ["Order Detail"] = 7,
                ["Product"] = 12,
                ["Shipper"] = 3,
                ["Supplier"] = 12
            };

            foreach (var entity in meta.EntityRoot.SubEntities)
            {
                Assert.Equal(entityAttrCount[entity.Name], entity.Attributes.Count);
            }
        }

        /// <summary>
        /// Checking how entity and property filters work.
        /// </summary>
        [Fact]
        public void TestFilters()
        {
            var meta = new MetaData();
            var loaderOptions = new DbContextMetaDataLoaderOptions();

            loaderOptions.Skip<Category>();

            loaderOptions.Skip<Customer>(c => c.Phone, c => c.PostalCode, c => c.Fax);

            meta.LoadFromDbContext(_dbContext, loaderOptions);

            var entity = meta.FindEntity(ent => ent.ClrType.Equals(typeof(Category)));
            Assert.Null(entity);

            entity = meta.FindEntity(ent => ent.ClrType.Equals(typeof(Customer)));
            Assert.NotNull(entity);

            Assert.Equal(8, entity.Attributes.Count);
            var attr = entity.FindAttribute(a => a.Id.Contains("Phone"));
            Assert.Null(attr);
        }

        [Fact]
        public void SkipUnknownTypes()
        {
            var meta = new MetaData();
            var loaderOptions = new DbContextMetaDataLoaderOptions();

            meta.LoadFromDbContext(_dbContext, loaderOptions);

            var entity = meta.FindEntity(ent => ent.ClrType.Equals(typeof(Customer)));
            Assert.NotNull(entity);

            var attr = entity.FindAttributeByExpression("Customer.TimeCreated");
            Assert.Null(attr);
        }

        [Fact]
        public void CompositeKeyEntity_HasCompositeKey_ReturnsTrue()
        {
            // Arrange
            var meta = new MetaData();
            meta.LoadFromDbContext(_dbContext);

            // Act
            var orderDetail = meta.FindEntity(ent => ent.ClrType == typeof(OrderDetail));

            // Assert
            Assert.NotNull(orderDetail);
            Assert.True(orderDetail.HasCompositeKey);
        }

        [Fact]
        public void CompositeKeyEntity_HasMultiplePkAttributes()
        {
            // Arrange
            var meta = new MetaData();
            meta.LoadFromDbContext(_dbContext);

            // Act
            var orderDetail = meta.FindEntity(ent => ent.ClrType == typeof(OrderDetail));
            var pkAttrs = orderDetail.Attributes.Where(a => a.IsPrimaryKey).ToList();

            // Assert
            Assert.Equal(2, pkAttrs.Count);
            Assert.Contains(pkAttrs, a => a.PropName == "OrderID");
            Assert.Contains(pkAttrs, a => a.PropName == "ProductID");
        }

        [Fact]
        public void CompositeKeyEntity_FkLookupAttrs_AreEditableAtMetadataLevel()
        {
            // Arrange
            var meta = new MetaData();
            meta.LoadFromDbContext(_dbContext);

            // Act
            var orderDetail = meta.FindEntity(ent => ent.ClrType == typeof(OrderDetail));
            var lookupAttrs = orderDetail.Attributes
                .Where(a => a.Kind == EntityAttrKind.Lookup)
                .ToList();

            // Assert
            Assert.Equal(2, lookupAttrs.Count);
            // Lookup attrs are editable at the metadata level; the rendering layer
            // makes them read-only only on edit forms for composite key entities
            foreach (var lookupAttr in lookupAttrs)
            {
                Assert.True(lookupAttr.IsEditable);
            }
        }

        [Fact]
        public void SingleKeyEntity_HasCompositeKey_ReturnsFalse()
        {
            // Arrange
            var meta = new MetaData();
            meta.LoadFromDbContext(_dbContext);

            // Act
            var category = meta.FindEntity(ent => ent.ClrType == typeof(Category));

            // Assert
            Assert.NotNull(category);
            Assert.False(category.HasCompositeKey);
        }
    }
}
