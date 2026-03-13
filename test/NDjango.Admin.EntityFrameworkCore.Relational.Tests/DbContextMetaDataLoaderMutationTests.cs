using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace NDjango.Admin.EntityFrameworkCore.Relational.Tests
{
    /// <summary>
    /// Mutation tests for DbContextMetaDataLoader.
    /// Targets survived mutants from Stryker run 2026-03-13.
    ///
    /// Equivalent mutants (unkillable):
    ///   - id=241 (Max()+1 → Max()-1): attrCount feeds into lookupDataAttr.Index assignment
    ///     at line 387, but that branch never fires because all PK attributes already have
    ///     auto-assigned indices from CreateEntityAttribute processing.
    ///   - id=242 (Max() → Min()): Same reason as 241.
    ///   - id=248 (remove EntityRoot.Attributes.Reorder): EntityRoot.Attributes is always
    ///     empty — no code path adds attributes to the root entity.
    ///   - id=249 (remove EntityRoot.SubEntities.Reorder): All entities have Index=int.MaxValue
    ///     (MetaEntityAttribute has no Index property, ProcessEntityType never sets entity.Index).
    ///     Reorder on items with identical indices is a no-op.
    ///   - id=236 (remove entity.SubEntities.Reorder at line 155): Individual entities never
    ///     have sub-entities added during ProcessEntityType — SubEntities is always empty at that point.
    ///   - id=322 (remove return for collection navigations at line 333): Collection navigations
    ///     have FK properties on the dependent entity (not the current entity), so
    ///     entity.FindAttributeById fails and the rest of ProcessNavigationProperty is skipped.
    ///   - id=323 (First() → FirstOrDefault() at line 336): Every FK always has at least one
    ///     property, so FirstOrDefault() returns the same result as First().
    ///   - id=334 (First() → FirstOrDefault() at line 381): Every principal key always has at least
    ///     one property, so FirstOrDefault() returns the same result as First().
    ///   - id=339 (remove attrCounter++ at line 391): The attrCounter in the navigation loop is only
    ///     used inside the condition at line 387, which never fires (all PK attributes already have
    ///     auto-assigned indices). Removing the increment has no observable effect.
    ///   - id=340 (attrCounter++ → attrCounter-- at line 391): Same reason as 339.
    ///   - id=368 (annotation.Description != null at line 423): Only differs when Description is
    ///     empty string "". Setting entityAttr.Description to "" when it's already "" is a no-op.
    ///   - id=379 (remove Parent from MetaEntityAttrDescriptor at line 464): The attribute's Entity
    ///     reference is set later by MetaEntityAttrStore.Add. No code accesses Entity between
    ///     creation and Add, so the null Parent in the descriptor has no observable effect.
    /// </summary>
    public class DbContextMetaDataLoaderMutationTests
    {
        // ─── Mutants 303, 306: DisplayNamePlural annotation sets NamePlural ────────

        [Fact]
        public void LoadFromDbContext_EntityWithCustomDisplayNamePlural_SetsNamePlural()
        {
            // Arrange — EntityWithCustomPlural has DisplayNamePlural that differs from MakePlural result
            var dbContext = EmptyPluralDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — annotation's DisplayNamePlural must override the auto-generated plural
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(EntityWithCustomPlural));
            Assert.Equal("Mice", entity.NamePlural);
        }

        // ─── Mutant 305: null DisplayNamePlural must not override NamePlural ───────

        [Fact]
        public void LoadFromDbContext_EntityWithNullDisplayNamePlural_PreservesNamePlural()
        {
            // Arrange — CustomerWithAnnotations has [MetaEntity(DisplayName = "Test")] without DisplayNamePlural
            var dbContext = DbContextWithAnnotations.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Line 283 sets Name=NamePlural="Test"; null DisplayNamePlural must not overwrite
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(CustomerWithAnnotations));
            Assert.Equal("Test", entity.NamePlural);
        }

        // ─── Mutant 304: empty-string DisplayNamePlural must not override NamePlural ───

        [Fact]
        public void LoadFromDbContext_EntityWithEmptyDisplayNamePlural_DoesNotOverrideNamePlural()
        {
            // Arrange
            var dbContext = EmptyPluralDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — empty DisplayNamePlural should be treated as "not set"
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(EntityWithEmptyPluralName));
            Assert.Equal("Custom Name", entity.NamePlural);
        }

        // ─── Mutants 315, 316: auto-indexing of attributes ────────────────────────

        [Fact]
        public void LoadFromDbContext_AttributesWithoutExplicitIndex_GetAutoAssignedIndices()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Shipper has no MetaEntityAttrAttribute annotations, all 3 attrs should be auto-indexed
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Shipper));
            foreach (var attr in entity.Attributes)
            {
                Assert.True(attr.Index < int.MaxValue,
                    $"Attribute '{attr.PropName}' should have an auto-assigned index, not int.MaxValue");
            }
        }

        [Fact]
        public void LoadFromDbContext_MixOfAnnotatedAndAutoIndexed_AssignsCorrectIndices()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Category: Description has [MetaEntityAttr(Index = 2)], others are auto-indexed
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));

            var descAttr = entity.FindAttribute(a => a.PropName == "Description");
            Assert.NotNull(descAttr);
            Assert.Equal(2, descAttr.Index);

            // Non-annotated attributes must have auto-assigned sequential indices (not int.MaxValue)
            var nonAnnotated = entity.Attributes.Where(a => a.PropName != "Description");
            foreach (var attr in nonAnnotated)
            {
                Assert.True(attr.Index < int.MaxValue,
                    $"Attribute '{attr.PropName}' should have an auto-assigned index, not int.MaxValue");
            }
        }

        // ─── Mutants 208, 227: KeepDbSetDeclarationOrder and idx++ ──────────────

        [Fact]
        public void LoadFromDbContext_KeepDbSetDeclarationOrder_PreservesDeclarationOrder()
        {
            // Arrange — TestDbContext declares Products (idx 4) before OrderDetails (idx 5)
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();
            var options = new DbContextMetaDataLoaderOptions { KeepDbSetDeclarationOrder = true };

            // Act
            meta.LoadFromDbContext(dbContext, options);

            // Assert — Product must appear before OrderDetail (declaration order)
            // Without KeepDbSetDeclarationOrder, EF Core returns alphabetical: OrderDetail before Product
            var entities = meta.EntityRoot.SubEntities.ToList();
            var productIdx = entities.FindIndex(e => e.ClrType == typeof(Product));
            var orderDetailIdx = entities.FindIndex(e => e.ClrType == typeof(OrderDetail));
            Assert.True(productIdx < orderDetailIdx,
                $"Product (pos {productIdx}) should appear before OrderDetail (pos {orderDetailIdx}) in declaration order");
        }

        // ─── Mutant 235: entity.Attributes.Reorder() sorts attributes by Index ──

        [Fact]
        public void LoadFromDbContext_AttributesWithExplicitIndex_AreReorderedByIndex()
        {
            // Arrange — EntityWithReorderableAttrs has explicit Index=99 on second property
            var dbContext = MutationTestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — attributes must be sorted by Index after loading
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(EntityWithReorderableAttrs));
            var indices = entity.Attributes.Select(a => a.Index).ToList();
            Assert.Equal(indices.OrderBy(i => i).ToList(), indices);

            // LastField (Index=99) must be last
            var lastAttr = entity.Attributes.Last();
            Assert.Equal("LastField", lastAttr.PropName);
        }

        // ─── Mutants 317, 318: attrCounter++ must increment sequentially ────────

        [Fact]
        public void LoadFromDbContext_AutoIndexedAttributes_HaveDistinctSequentialIndices()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Shipper has 3 attrs with no annotations; indices must be 0, 1, 2
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Shipper));
            var indices = entity.Attributes.Select(a => a.Index).ToList();
            Assert.Equal(3, indices.Count);
            for (int i = 0; i < indices.Count; i++)
            {
                Assert.Equal(i, indices[i]);
            }
        }

        // ─── Mutant 328: lookup attribute must have Kind = Lookup ────────────────

        [Fact]
        public void LoadFromDbContext_NavigationProperty_CreatesLookupKindAttribute()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Product's Category navigation should produce a Lookup attribute
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Product));
            var categoryLookup = entity.FindAttributeById("Product.Category");
            Assert.NotNull(categoryLookup);
            Assert.Equal(EntityAttrKind.Lookup, categoryLookup.Kind);
        }

        // ─── Mutant 329: non-enum FK lookup must not get enum display format ─────

        [Fact]
        public void LoadFromDbContext_NonEnumFkLookupAttribute_HasNullDisplayFormat()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Product.Category FK is int? (not enum), so DisplayFormat should be null
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Product));
            var categoryLookup = entity.FindAttributeById("Product.Category");
            Assert.NotNull(categoryLookup);
            Assert.Null(categoryLookup.DisplayFormat);
        }

        // ─── Mutant 333: FK data attr must be hidden from create/edit views ──────

        [Fact]
        public void LoadFromDbContext_FkDataAttribute_HiddenFromCreateAndEditViews()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Product.CategoryID is FK data attr, hidden because Category lookup exists
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Product));
            var categoryIdAttr = entity.FindAttributeById("Product.CategoryID");
            Assert.NotNull(categoryIdAttr);
            Assert.False(categoryIdAttr.ShowOnCreate);
            Assert.False(categoryIdAttr.ShowOnEdit);
        }

        // ─── Mutant 337: navigation processing must not overwrite lookup entity PK index ─

        [Fact]
        public void LoadFromDbContext_LookupEntityPkIndex_PreservedAfterNavigationProcessing()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Category.Id should keep its original auto-assigned index (0), not be
            // overwritten by Product's navigation processing counter
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var idAttr = entity.FindAttribute(a => a.PropName == "Id");
            Assert.NotNull(idAttr);
            Assert.Equal(0, idAttr.Index);
        }

        // ─── Mutant 342: auto-selection skipped when ShowInLookup already exists ─

        [Fact]
        public void LoadFromDbContext_EntityWithExistingShowInLookup_SkipsAutoSelection()
        {
            // Arrange — Employee.LastName has [MetaEntityAttr(ShowInLookup = true)]
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — FirstName should NOT have ShowInLookup because LastName already does
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Employee));
            var firstNameAttr = entity.FindAttribute(a => a.PropName == "FirstName");
            Assert.NotNull(firstNameAttr);
            Assert.False(firstNameAttr.ShowInLookup);
        }

        // ─── Mutant 343: auto-selection runs when no ShowInLookup attributes exist ─

        [Fact]
        public void LoadFromDbContext_EntityWithNoShowInLookup_AutoSelectsNameAttributes()
        {
            // Arrange — Category has no ShowInLookup annotations
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — CategoryName should get ShowInLookup=true via auto-selection
            // (caption "Category Name" contains "name", is string, not lookup)
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var categoryNameAttr = entity.FindAttribute(a => a.PropName == "CategoryName");
            Assert.NotNull(categoryNameAttr);
            Assert.True(categoryNameAttr.ShowInLookup);
        }

        // ─── Mutants 345, 346, 347: auto-selection criteria for ShowInLookup ────

        [Fact]
        public void LoadFromDbContext_AutoSelection_OnlySelectsStringAttrsWithNameInCaption()
        {
            // Arrange — Category's Description is string but caption doesn't contain "name"
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Description should NOT have ShowInLookup (doesn't match "name" criterion)
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var descAttr = entity.FindAttribute(a => a.PropName == "Description");
            Assert.NotNull(descAttr);
            Assert.False(descAttr.ShowInLookup);
        }

        // ─── Mutant 367: MetaEntityAttr Description annotation must be applied ──

        [Fact]
        public void LoadFromDbContext_MetaEntityAttrWithDescription_SetsAttributeDescription()
        {
            // Arrange — EntityWithReorderableAttrs.MiddleField has [MetaEntityAttr(Description = "...")]
            var dbContext = MutationTestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(EntityWithReorderableAttrs));
            var attr = entity.FindAttribute(a => a.PropName == "MiddleField");
            Assert.NotNull(attr);
            Assert.Equal("Custom attr description", attr.Description);
        }

        // ─── Mutant 369: null annotation Description must not overwrite default "" ─

        [Fact]
        public void LoadFromDbContext_MetaEntityAttrWithoutDescription_PreservesEmptyDescription()
        {
            // Arrange — Category.Description has [MetaEntityAttr(Index = 2)] without Description
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — attribute metadata Description should remain "" (not become null)
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var attr = entity.FindAttribute(a => a.PropName == "Description");
            Assert.NotNull(attr);
            Assert.Equal("", attr.Description);
        }

        // ─── Mutant 371: annotation.DataType == Unknown must not overwrite auto-detected type ─

        [Fact]
        public void LoadFromDbContext_AnnotationWithDefaultDataType_PreservesAutoDetectedType()
        {
            // Arrange — Order.OrderDate has [MetaEntityAttr(Editable = false)], DataType defaults to Unknown
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — OrderDate should have auto-detected DateTime type, not Unknown
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Order));
            var attr = entity.FindAttribute(a => a.PropName == "OrderDate");
            Assert.NotNull(attr);
            Assert.NotEqual(DataType.Unknown, attr.DataType);
        }

        // ─── Mutant 380: TryResolveDefaultValue must set DefaultValue for non-CLR-defaults ─

        [Fact]
        public void LoadFromDbContext_PropertyWithNonDefaultValue_SetsDefaultValue()
        {
            // Arrange — EntityWithDefaultValues.Priority has default value = 5
            var dbContext = MutationTestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(EntityWithDefaultValues));
            var attr = entity.FindAttribute(a => a.PropName == "Status");
            Assert.NotNull(attr);
            Assert.Equal("active", attr.DefaultValue);
        }

        // ─── Mutants 381, 383, 384: blob fields hidden from create/edit views ───

        [Fact]
        public void LoadFromDbContext_BlobAttribute_HiddenFromCreateAndEditViews()
        {
            // Arrange — Category.Picture is byte[] (Blob)
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var pictureAttr = entity.FindAttribute(a => a.PropName == "Picture");
            Assert.NotNull(pictureAttr);
            Assert.False(pictureAttr.ShowOnCreate);
            Assert.False(pictureAttr.ShowOnEdit);
        }

        // ─── Mutant 395: [Display] annotation sets Caption and Description ──────

        [Fact]
        public void LoadFromDbContext_DisplayAttribute_SetsCaptionFromDisplayName()
        {
            // Arrange — Employee.LastName has [Display(Name = "Last name")]
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — Caption should be "Last name" (from Display), not "Last Name" (from PrettifyName)
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Employee));
            var attr = entity.FindAttribute(a => a.PropName == "LastName");
            Assert.NotNull(attr);
            Assert.Equal("Last name", attr.Caption);
        }

        // ─── Mutant 396: PrettifyName applied when no Display attribute ─────────

        [Fact]
        public void LoadFromDbContext_PropertyWithoutDisplayAttr_GetsPrettifiedCaption()
        {
            // Arrange
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert — CompanyName has no [Display], so PrettifyName should produce "Company Name"
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Shipper));
            var attr = entity.FindAttribute(a => a.PropName == "CompanyName");
            Assert.NotNull(attr);
            Assert.Equal("Company Name", attr.Caption);
        }

        // ─── Mutant 398: non-generated property keeps ShowOnCreate = true ───────

        [Fact]
        public void LoadFromDbContext_NonGeneratedProperty_KeepsShowOnCreate()
        {
            // Arrange — CategoryName is ValueGenerated.Never, DefaultValue=null
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var attr = entity.FindAttribute(a => a.PropName == "CategoryName");
            Assert.NotNull(attr);
            Assert.True(attr.ShowOnCreate);
        }

        // ─── Mutants 399, 400, 401: auto-generated PK hides ShowOnCreate ───────

        [Fact]
        public void LoadFromDbContext_AutoGeneratedPk_HidesShowOnCreate()
        {
            // Arrange — Shipper.Id is ValueGenerated.OnAdd (SQLite auto-increment)
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Shipper));
            var attr = entity.FindAttribute(a => a.PropName == "Id");
            Assert.NotNull(attr);
            Assert.False(attr.ShowOnCreate);
        }

        // ─── Mutant 402: non-OnUpdate property keeps ShowOnEdit = true ──────────

        [Fact]
        public void LoadFromDbContext_NonOnUpdateProperty_KeepsShowOnEdit()
        {
            // Arrange — CategoryName is ValueGenerated.Never
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var attr = entity.FindAttribute(a => a.PropName == "CategoryName");
            Assert.NotNull(attr);
            Assert.True(attr.ShowOnEdit);
        }

        // ─── Mutants 404, 405: auto-generated property is not editable ──────────

        [Fact]
        public void LoadFromDbContext_AutoGeneratedProperty_IsNotEditable()
        {
            // Arrange — Shipper.Id is ValueGenerated.OnAdd (!= Never)
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Shipper));
            var attr = entity.FindAttribute(a => a.PropName == "Id");
            Assert.NotNull(attr);
            Assert.False(attr.IsEditable);
        }

        // ─── Mutants 406, 408: blob field hidden from grid view ─────────────────

        [Fact]
        public void LoadFromDbContext_BlobAttribute_HiddenFromGridView()
        {
            // Arrange — Category.Picture is byte[] (Blob)
            var dbContext = TestDbContext.Create();
            var meta = new MetaData();

            // Act
            meta.LoadFromDbContext(dbContext);

            // Assert
            var entity = meta.EntityRoot.SubEntities.First(e => e.ClrType == typeof(Category));
            var pictureAttr = entity.FindAttribute(a => a.PropName == "Picture");
            Assert.NotNull(pictureAttr);
            Assert.False(pictureAttr.ShowOnView);
        }
    }

    // ─── Test entities for mutant 304 ──────────────────────────────────────

    [MetaEntity(DisplayName = "Custom Name", DisplayNamePlural = "")]
    public class EntityWithEmptyPluralName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [MetaEntity(DisplayNamePlural = "Mice")]
    public class EntityWithCustomPlural
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class EntityWithReorderableAttrs
    {
        public int Id { get; set; }

        [MetaEntityAttr(Index = 99)]
        public string LastField { get; set; }

        [MetaEntityAttr(Description = "Custom attr description")]
        public string MiddleField { get; set; }
    }

    public class EntityWithDefaultValues
    {
        public int Id { get; set; }
        public string Status { get; set; } = "active";
    }

    internal class EmptyPluralDbContext : DbContext
    {
        public EmptyPluralDbContext(DbContextOptions options) : base(options) { }

        public DbSet<EntityWithEmptyPluralName> EntitiesWithEmptyPlural { get; set; }
        public DbSet<EntityWithCustomPlural> EntitiesWithCustomPlural { get; set; }

        public static EmptyPluralDbContext Create()
        {
            return new EmptyPluralDbContext(new DbContextOptionsBuilder()
                .UseSqlite("Data Source = :memory:")
                .Options);
        }
    }

    internal class MutationTestDbContext : DbContext
    {
        public MutationTestDbContext(DbContextOptions options) : base(options) { }

        public DbSet<EntityWithReorderableAttrs> ReorderableEntities { get; set; }
        public DbSet<EntityWithDefaultValues> DefaultValueEntities { get; set; }

        public static MutationTestDbContext Create()
        {
            return new MutationTestDbContext(new DbContextOptionsBuilder()
                .UseSqlite("Data Source = :memory:")
                .Options);
        }
    }
}
