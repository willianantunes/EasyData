using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class EntitiesAndAttributesTests
    {
        #region MetaEntity Tests

        [Fact]
        public void IsEditable_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var entity = model.AddEntity(null, "TestEntity");

            // Assert
            Assert.True(entity.IsEditable);
        }

        [Fact]
        public void Index_DefaultValue_ReturnsIntMaxValue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var entity = model.AddEntity(null, "TestEntity");

            // Assert
            Assert.Equal(int.MaxValue, entity.Index);
        }

        [Fact]
        public void IsEmpty_EntityWithNoAttributes_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "EmptyEntity");

            // Act
            var result = entity.IsEmpty;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEmpty_EntityWithAttributes_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String
            });

            // Act
            var result = entity.IsEmpty;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsEmpty_EntityWithNonEmptySubEntity_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "ParentEntity");
            var child = model.AddEntity(parent, "ChildEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "Field1",
                DataType = DataType.String
            });

            // Act
            var result = parent.IsEmpty;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsEmpty_EntityWithEmptySubEntity_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "ParentEntity");
            model.AddEntity(parent, "EmptyChild");

            // Act
            var result = parent.IsEmpty;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsRoot_EntityWithNoParent_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var isRoot = model.EntityRoot.IsRoot;

            // Assert
            Assert.True(isRoot);
        }

        [Fact]
        public void IsRoot_EntityWithParent_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "ChildEntity");

            // Act
            var isRoot = entity.IsRoot;

            // Assert
            Assert.False(isRoot);
        }

        [Fact]
        public void GetFullName_SingleEntity_ReturnsName()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Category");

            // Act
            var fullName = entity.GetFullName(".");

            // Assert
            Assert.Equal("Category", fullName);
        }

        [Fact]
        public void GetFullName_NestedEntity_ReturnsSeparatedPath()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");

            // Act
            var fullName = child.GetFullName(".");

            // Assert
            Assert.Equal("Parent.Child", fullName);
        }

        [Fact]
        public void GetFullName_DifferentSeparator_UsesProvidedSeparator()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");

            // Act
            var fullName = child.GetFullName(" / ");

            // Assert
            Assert.Equal("Parent / Child", fullName);
        }

        [Fact]
        public void GetFirstLeaf_EntityWithAttributes_ReturnsFirstAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "First",
                DataType = DataType.String
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Second",
                DataType = DataType.Int32
            });

            // Act
            var result = entity.GetFirstLeaf();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attr1.Id, result.Id);
        }

        [Fact]
        public void GetFirstLeaf_EmptyEntityWithSubEntities_ReturnsFirstAttributeFromSubEntity()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "ChildField",
                DataType = DataType.String
            });

            // Act
            var result = parent.GetFirstLeaf();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attr.Id, result.Id);
        }

        [Fact]
        public void GetFirstLeaf_EmptyEntityNoSubEntities_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "EmptyEntity");

            // Act
            var result = entity.GetFirstLeaf();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindAttributeById_ExistingId_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });

            // Act
            var result = entity.FindAttributeById(attr.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attr.Id, result.Id);
        }

        [Fact]
        public void FindAttributeById_NonExistingId_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });

            // Act
            var result = entity.FindAttributeById("NonExistingId");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindAttributeById_InSubEntity_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "ChildField",
                DataType = DataType.String
            });

            // Act
            var result = parent.FindAttributeById(attr.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attr.Id, result.Id);
        }

        [Fact]
        public void FindAttributeByCaption_ExistingCaption_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Full Name";

            // Act
            var result = entity.FindAttributeByCaption("Full Name");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Full Name", result.Caption);
        }

        [Fact]
        public void FindAttributeByCaption_NonExistingCaption_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Full Name";

            // Act
            var result = entity.FindAttributeByCaption("Does Not Exist");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindAttributeByExpression_ExistingExpression_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            });

            // Act
            var result = entity.FindAttributeByExpression("CompanyName");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CompanyName", result.Expr);
        }

        [Fact]
        public void FindAttributeByExpression_CaseInsensitive_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            });

            // Act
            var result = entity.FindAttributeByExpression("companyname");

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FindAttribute_UsingPredicate_ReturnsMatchingAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String
            });
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field2",
                DataType = DataType.Int32
            });

            // Act
            var result = entity.FindAttribute(a => a.DataType == DataType.Int32);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attr2.Id, result.Id);
        }

        [Fact]
        public void FindAttribute_NoMatch_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String
            });

            // Act
            var result = entity.FindAttribute(a => a.DataType == DataType.Bool);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindAttribute_InSubEntity_ReturnsMatchingAttribute()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = parent,
                Expression = "ParentField",
                DataType = DataType.String
            });
            var childAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "ChildField",
                DataType = DataType.Bool
            });

            // Act
            var result = parent.FindAttribute(a => a.DataType == DataType.Bool);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(childAttr.Id, result.Id);
        }

        [Fact]
        public void FindSubEntity_ByName_ReturnsEntity()
        {
            // Arrange
            var model = new MetaData();
            var entity1 = model.AddEntity(null, "Category");
            model.AddEntity(null, "Product");

            // Act
            var result = model.EntityRoot.FindSubEntity("Category");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Category", result.Name);
        }

        [Fact]
        public void FindSubEntity_CaseInsensitive_ReturnsEntity()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");

            // Act
            var result = model.EntityRoot.FindSubEntity("category");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Category", result.Name);
        }

        [Fact]
        public void FindSubEntity_NonExistingName_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");

            // Act
            var result = model.EntityRoot.FindSubEntity("NonExisting");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FindSubEntity_NestedEntity_ReturnsEntity()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            model.AddEntity(parent, "DeepChild");

            // Act
            var result = model.EntityRoot.FindSubEntity("DeepChild");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("DeepChild", result.Name);
        }

        [Fact]
        public void FindSubEntity_ByPredicate_ReturnsMatchingEntity()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");
            var product = model.AddEntity(null, "Product");
            product.IsEditable = false;

            // Act
            var result = model.EntityRoot.FindSubEntity(e => !e.IsEditable);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product", result.Name);
        }

        [Fact]
        public void FindSubEntity_ByPredicateNested_ReturnsMatchingEntity()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "SpecialChild");
            child.Description = "special";

            // Act
            var result = model.EntityRoot.FindSubEntity(e => e.Description == "special");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SpecialChild", result.Name);
        }

        [Fact]
        public void FindSubEntity_ByPredicateNoMatch_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");

            // Act
            var result = model.EntityRoot.FindSubEntity(e => e.Name == "NonExisting");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DeleteSubEntities_ByNames_RemovesMatchingEntities()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");
            model.AddEntity(null, "Product");
            model.AddEntity(null, "Order");

            // Act
            var deleted = model.EntityRoot.DeleteSubEntities("Category", "Order");

            // Assert
            Assert.Equal(2, deleted);
            Assert.Single(model.EntityRoot.SubEntities);
            Assert.Equal("Product", model.EntityRoot.SubEntities[0].Name);
        }

        [Fact]
        public void DeleteSubEntities_ByNames_NoMatchReturnsZero()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");

            // Act
            var deleted = model.EntityRoot.DeleteSubEntities("NonExisting");

            // Assert
            Assert.Equal(0, deleted);
            Assert.Single(model.EntityRoot.SubEntities);
        }

        [Fact]
        public void DeleteSubEntities_ByPredicate_RemovesMatchingEntities()
        {
            // Arrange
            var model = new MetaData();
            var cat = model.AddEntity(null, "Category");
            cat.IsEditable = false;
            model.AddEntity(null, "Product");
            var order = model.AddEntity(null, "Order");
            order.IsEditable = false;

            // Act
            var deleted = model.EntityRoot.DeleteSubEntities(e => !e.IsEditable);

            // Assert
            Assert.Equal(2, deleted);
            Assert.Single(model.EntityRoot.SubEntities);
            Assert.Equal("Product", model.EntityRoot.SubEntities[0].Name);
        }

        [Fact]
        public void DeleteSubEntities_ByPredicateNoMatch_ReturnsZero()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");

            // Act
            var deleted = model.EntityRoot.DeleteSubEntities(e => e.Name == "NonExisting");

            // Assert
            Assert.Equal(0, deleted);
            Assert.Single(model.EntityRoot.SubEntities);
        }

        [Fact]
        public void Scan_WithEntityAndAttrHandlers_VisitsAllEntitiesAndAttributes()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = parent,
                Expression = "ParentField",
                DataType = DataType.String
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "ChildField",
                DataType = DataType.Int32
            });

            var visitedEntities = new List<string>();
            var visitedAttrs = new List<string>();

            // Act
            model.EntityRoot.Scan(
                entity => visitedEntities.Add(entity.Name ?? "Root"),
                attr => visitedAttrs.Add(attr.Expr),
                processRoot: true
            );

            // Assert
            Assert.Equal(3, visitedEntities.Count);
            Assert.Contains("Parent", visitedEntities);
            Assert.Contains("Child", visitedEntities);
            Assert.Equal(2, visitedAttrs.Count);
            Assert.Contains("ParentField", visitedAttrs);
            Assert.Contains("ChildField", visitedAttrs);
        }

        [Fact]
        public void Scan_ProcessRootFalse_SkipsRootEntity()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Child");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = model.EntityRoot,
                Expression = "RootField",
                DataType = DataType.String
            });

            var visitedEntities = new List<string>();
            var visitedAttrs = new List<string>();

            // Act
            model.EntityRoot.Scan(
                entity => visitedEntities.Add(entity.Name ?? "Root"),
                attr => visitedAttrs.Add(attr.Expr),
                processRoot: false
            );

            // Assert
            Assert.Single(visitedEntities);
            Assert.Equal("Child", visitedEntities[0]);
            Assert.Empty(visitedAttrs);
        }

        [Fact]
        public void Scan_NullHandlers_DoesNotThrow()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Child");

            // Act
            var exception = Record.Exception(() =>
                model.EntityRoot.Scan(null, null, processRoot: true)
            );

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void CompareTo_EntitiesByName_SortsByNameAlphabetically()
        {
            // Arrange
            var model = new MetaData();
            var entityA = model.AddEntity(null, "Apple");
            var entityB = model.AddEntity(null, "Banana");

            // Act
            var comparison = ((IComparable<MetaEntity>)entityA).CompareTo(entityB);

            // Assert
            Assert.True(comparison < 0);
        }

        [Fact]
        public void CompareTo_SameNames_ReturnsZero()
        {
            // Arrange
            var model = new MetaData();
            var entity1 = model.AddEntity(null, "Same");
            var model2 = new MetaData();
            var entity2 = model2.AddEntity(null, "Same");

            // Act
            var comparison = ((IComparable<MetaEntity>)entity1).CompareTo(entity2);

            // Assert
            Assert.Equal(0, comparison);
        }

        [Fact]
        public void Model_EntityWithParent_ReturnsModelFromAncestor()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");

            // Act
            var childModel = child.Model;

            // Assert
            Assert.NotNull(childModel);
            Assert.Same(model, childModel);
        }

        [Fact]
        public void Properties_SetAndGet_ReturnsCorrectValues()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act
            entity.Description = "A test entity";
            entity.NamePlural = "TestEntities";
            entity.UserData = "custom";
            entity.DbSetName = "TestEntities";
            entity.TypeName = "TestType";
            entity.ClrType = typeof(string);
            entity.Index = 5;
            entity.IsEditable = false;

            // Assert
            Assert.Equal("A test entity", entity.Description);
            Assert.Equal("TestEntities", entity.NamePlural);
            Assert.Equal("custom", entity.UserData);
            Assert.Equal("TestEntities", entity.DbSetName);
            Assert.Equal("TestType", entity.TypeName);
            Assert.Equal(typeof(string), entity.ClrType);
            Assert.Equal(5, entity.Index);
            Assert.False(entity.IsEditable);
        }

        #endregion

        #region MetaEntityStore Tests

        [Fact]
        public void InsertItem_AddEntityToItself_ThrowsArgumentException()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => entity.SubEntities.Add(entity));
            Assert.Contains("Can't add an entity to itself", ex.Message);
        }

        [Fact]
        public void InsertItem_MoveEntityFromOldParent_RemovesFromOldParent()
        {
            // Arrange
            var model = new MetaData();
            var parent1 = model.AddEntity(null, "Parent1");
            var parent2 = model.AddEntity(null, "Parent2");
            var child = model.AddEntity(parent1, "Child");

            // Act
            parent2.SubEntities.Add(child);

            // Assert
            Assert.Empty(parent1.SubEntities);
            Assert.Single(parent2.SubEntities);
            Assert.Same(parent2, child.Parent);
        }

        #endregion

        #region MetaEntityList Tests

        [Fact]
        public void SortByName_MultipleEntities_SortsAlphabetically()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Zebra");
            model.AddEntity(null, "Apple");
            model.AddEntity(null, "Mango");

            // Act
            model.EntityRoot.SubEntities.SortByName();

            // Assert
            Assert.Equal("Apple", model.EntityRoot.SubEntities[0].Name);
            Assert.Equal("Mango", model.EntityRoot.SubEntities[1].Name);
            Assert.Equal("Zebra", model.EntityRoot.SubEntities[2].Name);
        }

        [Fact]
        public void Reorder_MultipleEntities_SortsByIndex()
        {
            // Arrange
            var model = new MetaData();
            var entity1 = model.AddEntity(null, "Third");
            entity1.Index = 3;
            var entity2 = model.AddEntity(null, "First");
            entity2.Index = 1;
            var entity3 = model.AddEntity(null, "Second");
            entity3.Index = 2;

            // Act
            model.EntityRoot.SubEntities.Reorder();

            // Assert
            Assert.Equal("First", model.EntityRoot.SubEntities[0].Name);
            Assert.Equal("Second", model.EntityRoot.SubEntities[1].Name);
            Assert.Equal("Third", model.EntityRoot.SubEntities[2].Name);
        }

        #endregion

        #region MetaEntityAttr Tests

        [Fact]
        public void MetaEntityAttr_DefaultProperties_HaveExpectedValues()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "TestField",
                DataType = DataType.String
            });

            // Assert
            Assert.True(attr.IsEditable);
            Assert.True(attr.ShowOnView);
            Assert.True(attr.ShowOnEdit);
            Assert.True(attr.ShowOnCreate);
            Assert.False(attr.IsPrimaryKey);
            Assert.False(attr.IsForeignKey);
            Assert.False(attr.ShowInLookup);
            Assert.False(attr.IsNullable);
            Assert.False(attr.IsVirtual);
            Assert.Equal(EntityAttrKind.Data, attr.Kind);
            Assert.Equal(int.MaxValue, attr.Index);
            Assert.Equal("", attr.Description);
        }

        [Fact]
        public void IsVirtual_VirtualKind_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "VirtualField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });

            // Assert
            Assert.True(attr.IsVirtual);
            Assert.Equal(EntityAttrKind.Virtual, attr.Kind);
        }

        [Fact]
        public void IsVirtual_DataKind_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "DataField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Data
            });

            // Assert
            Assert.False(attr.IsVirtual);
        }

        [Fact]
        public void Expr_SetValue_ReturnsCorrectExpression()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "InitialField",
                DataType = DataType.String
            });

            // Act
            attr.Expr = "UpdatedField";

            // Assert
            Assert.Equal("UpdatedField", attr.Expr);
        }

        [Fact]
        public void FullExpr_DataAttribute_ReturnsEntityNameColonExpr()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });

            // Act
            var fullExpr = attr.FullExpr;

            // Assert
            Assert.Equal("TestEntity:Name", fullExpr);
        }

        [Fact]
        public void FullExpr_VirtualAttribute_ReturnsExpr()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CalcField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });

            // Act
            var fullExpr = attr.FullExpr;

            // Assert
            Assert.Equal("CalcField", fullExpr);
        }

        [Fact]
        public void GetValueEditor_DateType_ReturnsDateTimeValueEditor()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "DateField",
                DataType = DataType.Date
            });

            // Act
            var editor = attr.GetValueEditor();

            // Assert
            Assert.IsType<DateTimeValueEditor>(editor);
        }

        [Fact]
        public void GetValueEditor_TimeType_ReturnsDateTimeValueEditor()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "TimeField",
                DataType = DataType.Time
            });

            // Act
            var editor = attr.GetValueEditor();

            // Assert
            Assert.IsType<DateTimeValueEditor>(editor);
        }

        [Fact]
        public void GetValueEditor_DateTimeType_ReturnsDateTimeValueEditor()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "DateTimeField",
                DataType = DataType.DateTime
            });

            // Act
            var editor = attr.GetValueEditor();

            // Assert
            Assert.IsType<DateTimeValueEditor>(editor);
            Assert.Equal(DataType.DateTime, ((DateTimeValueEditor)editor).SubType);
        }

        [Fact]
        public void GetValueEditor_BoolType_ReturnsCustomListValueEditor()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "BoolField",
                DataType = DataType.Bool
            });

            // Act
            var editor = attr.GetValueEditor();

            // Assert
            Assert.IsType<CustomListValueEditor>(editor);
        }

        [Fact]
        public void GetValueEditor_StringType_ReturnsTextValueEditor()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "StringField",
                DataType = DataType.String
            });

            // Act
            var editor = attr.GetValueEditor();

            // Assert
            Assert.IsType<TextValueEditor>(editor);
        }

        [Fact]
        public void GetValueEditor_Int32Type_ReturnsTextValueEditor()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "IntField",
                DataType = DataType.Int32
            });

            // Act
            var editor = attr.GetValueEditor();

            // Assert
            Assert.IsType<TextValueEditor>(editor);
        }

        [Fact]
        public void GetValueEditor_WithDefaultEditor_ReturnsDefaultEditor()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });
            var customEditor = new TextValueEditor();
            attr.DefaultEditor = customEditor;

            // Act
            var editor = attr.GetValueEditor();

            // Assert
            Assert.Same(customEditor, editor);
        }

        [Fact]
        public void GetValueEditor_OverrideTypeParameter_UsesProvidedType()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });

            // Act
            var editor = attr.GetValueEditor(DataType.Date);

            // Assert
            Assert.IsType<DateTimeValueEditor>(editor);
        }

        [Fact]
        public void SetDefaultEditorWithoutChecking_SetsEditorDirectly()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });
            var editor = new TextValueEditor();

            // Act
            attr.SetDefaultEditorWithoutChecking(editor);

            // Assert
            Assert.Same(editor, attr.DefaultEditor);
        }

        [Fact]
        public void CompareWithExpr_SameExpression_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            });

            // Act
            var result = attr.CompareWithExpr("CompanyName");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CompareWithExpr_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            });

            // Act
            var result = attr.CompareWithExpr("companyname");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CompareWithExpr_DifferentExpression_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            });

            // Act
            var result = attr.CompareWithExpr("ProductName");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetFullCaption_EntityWithCaption_ReturnsCombinedString()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Full Name";

            // Act
            var fullCaption = attr.GetFullCaption(" ");

            // Assert
            Assert.Equal("Customer Full Name", fullCaption);
        }

        [Fact]
        public void GetFullCaption_DefaultSeparator_UsesSpace()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Date",
                DataType = DataType.Date
            });
            attr.Caption = "Order Date";

            // Act
            var fullCaption = attr.GetFullCaption();

            // Assert
            Assert.Equal("Order Order Date", fullCaption);
        }

        [Fact]
        public void CopyFrom_AllProperties_CopiesCorrectly()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var source = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "SourceField",
                DataType = DataType.Int32
            });
            source.Caption = "Source Caption";
            source.Description = "Source Description";
            source.Size = 500;
            source.UserData = "source-data";
            source.IsNullable = true;
            source.IsPrimaryKey = true;
            source.IsForeignKey = true;
            source.IsEditable = false;
            source.ShowInLookup = true;
            source.ShowOnView = false;
            source.ShowOnEdit = false;
            source.ShowOnCreate = false;

            var target = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "TargetField",
                DataType = DataType.String
            });

            // Act
            target.CopyFrom(source);

            // Assert
            Assert.Equal("Source Caption", target.Caption);
            Assert.Equal(DataType.Int32, target.DataType);
            Assert.Equal("Source Description", target.Description);
            Assert.Equal("SourceField", target.Expr);
            Assert.Equal(source.Id, target.Id);
            Assert.Equal(EntityAttrKind.Data, target.Kind);
            Assert.Equal(500, target.Size);
            Assert.Equal("source-data", target.UserData);
            Assert.True(target.IsNullable);
            Assert.True(target.IsPrimaryKey);
            Assert.True(target.IsForeignKey);
            Assert.False(target.IsEditable);
            Assert.True(target.ShowInLookup);
            Assert.False(target.ShowOnView);
            Assert.False(target.ShowOnEdit);
            Assert.False(target.ShowOnCreate);
        }

        [Fact]
        public void CompareTo_AttributesByCaption_SortsAlphabetically()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Apple",
                DataType = DataType.String
            });
            attr1.Caption = "Apple";
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Banana",
                DataType = DataType.String
            });
            attr2.Caption = "Banana";

            // Act
            var comparison = ((IComparable<MetaEntityAttr>)attr1).CompareTo(attr2);

            // Assert
            Assert.True(comparison < 0);
        }

        [Fact]
        public void DisplayFormat_ValidFormat_SetsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency
            });

            // Act
            attr.DisplayFormat = "{0:F2}";

            // Assert
            Assert.Equal("{0:F2}", attr.DisplayFormat);
        }

        [Fact]
        public void DisplayFormat_InvalidFormat_ThrowsInvalidDataFormatException()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency
            });

            // Act & Assert
            Assert.Throws<InvalidDataFormatException>(() => attr.DisplayFormat = "bad format");
        }

        [Fact]
        public void DisplayFormat_NullOrEmpty_DoesNotThrow()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency
            });

            // Act
            attr.DisplayFormat = null;

            // Assert
            Assert.Null(attr.DisplayFormat);
        }

        [Fact]
        public void CheckModel_NullEntity_ThrowsMetaDataException()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });
            // Simulate a detached attribute by creating one without entity
            var detachedAttr = model.CreateEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = null,
                Expression = "Detached",
                DataType = DataType.String
            });
            detachedAttr._lookupEntityId = "some-id";

            // Act & Assert
            var ex = Assert.Throws<MetaDataException>(() =>
            {
                var _ = detachedAttr.LookupEntity;
            });
            Assert.Contains("Entity is not specified for attribute", ex.Message);
        }

        [Fact]
        public void LookupEntity_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var lookupEntity = model.AddEntity(null, "Customer");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            attr.LookupEntity = lookupEntity;

            // Assert
            Assert.Same(lookupEntity, attr.LookupEntity);
        }

        [Fact]
        public void DataAttr_SetAndGet_WorksCorrectly()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var dataAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            lookupAttr.DataAttr = dataAttr;

            // Assert
            Assert.Same(dataAttr, lookupAttr.DataAttr);
        }

        [Fact]
        public void LookupAttr_SetAndGet_SetsReciprocalId()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            attr2.LookupAttr = attr1;

            // Assert
            Assert.Same(attr1, attr2.LookupAttr);
            Assert.Equal(attr2.Id, attr1._lookupAttrId);
        }

        [Fact]
        public void LookupAttr_SetNull_ClearsId()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            attr2.LookupAttr = attr1;

            // Act
            attr2.LookupAttr = null;

            // Assert
            Assert.Null(attr2._lookupAttrId);
        }

        [Fact]
        public void DefaultValue_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Status",
                DataType = DataType.String
            });

            // Act
            attr.DefaultValue = "Active";

            // Assert
            Assert.Equal("Active", attr.DefaultValue);
        }

        [Fact]
        public void PropName_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });

            // Act
            attr.PropName = "MyProperty";
            attr.ColumnName = "my_column";

            // Assert
            Assert.Equal("MyProperty", attr.PropName);
            Assert.Equal("my_column", attr.ColumnName);
        }

        #endregion

        #region MetaEntityAttrList Tests

        [Fact]
        public void SortByCaption_MultipleAttributes_SortsAlphabetically()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Zebra",
                DataType = DataType.String
            });
            attr1.Caption = "Zebra";
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Apple",
                DataType = DataType.String
            });
            attr2.Caption = "Apple";
            var attr3 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Mango",
                DataType = DataType.String
            });
            attr3.Caption = "Mango";

            // Act
            entity.Attributes.SortByCaption();

            // Assert
            Assert.Equal("Apple", entity.Attributes[0].Caption);
            Assert.Equal("Mango", entity.Attributes[1].Caption);
            Assert.Equal("Zebra", entity.Attributes[2].Caption);
        }

        [Fact]
        public void Reorder_MultipleAttributes_SortsByIndex()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Third",
                DataType = DataType.String
            });
            attr1.Index = 3;
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "First",
                DataType = DataType.String
            });
            attr2.Index = 1;
            var attr3 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Second",
                DataType = DataType.String
            });
            attr3.Index = 2;

            // Act
            entity.Attributes.Reorder();

            // Assert
            Assert.Equal(1, entity.Attributes[0].Index);
            Assert.Equal(2, entity.Attributes[1].Index);
            Assert.Equal(3, entity.Attributes[2].Index);
        }

        #endregion

        #region MetaEntityAttrStore Tests

        [Fact]
        public void InsertItem_SetsEntityOnAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });

            // Assert
            Assert.Same(entity, attr.Entity);
        }

        [Fact]
        public void Model_ReturnsModelFromParentEntity()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });

            // Act
            var attrModel = attr.Model;

            // Assert
            Assert.Same(model, attrModel);
        }

        #endregion

        #region MetaEntityAttrDescriptor Tests

        [Fact]
        public void MetaEntityAttrDescriptor_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var desc = new MetaEntityAttrDescriptor();

            // Assert
            Assert.Equal(EntityAttrKind.Data, desc.Kind);
            Assert.Equal(DataType.Unknown, desc.DataType);
            Assert.Equal(100, desc.Size);
            Assert.False(desc.IsVirtual);
            Assert.Null(desc.Id);
            Assert.Null(desc.Parent);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_Caption_DerivedFromExpression()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor
            {
                Expression = "Entity.FieldName"
            };

            // Act
            var caption = desc.Caption;

            // Assert
            Assert.Equal("FieldName", caption);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_Caption_ExplicitValueOverridesExpression()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor
            {
                Expression = "Entity.FieldName",
                Caption = "Custom Caption"
            };

            // Act
            var caption = desc.Caption;

            // Assert
            Assert.Equal("Custom Caption", caption);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_IsVirtual_SetTrue_ChangesKindToVirtual()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor();

            // Act
            desc.IsVirtual = true;

            // Assert
            Assert.Equal(EntityAttrKind.Virtual, desc.Kind);
            Assert.True(desc.IsVirtual);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_IsVirtual_SetFalse_ChangesKindToData()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor { Kind = EntityAttrKind.Virtual };

            // Act
            desc.IsVirtual = false;

            // Assert
            Assert.Equal(EntityAttrKind.Data, desc.Kind);
            Assert.False(desc.IsVirtual);
        }

        #endregion

        #region MetaData AddEntity / AddEntityAttr Tests

        [Fact]
        public void AddEntity_NullParent_AddsToRoot()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var entity = model.AddEntity(null, "Category");

            // Assert
            Assert.Single(model.EntityRoot.SubEntities);
            Assert.Equal("Category", entity.Name);
            Assert.Same(model.EntityRoot, entity.Parent);
        }

        [Fact]
        public void AddEntity_WithParent_AddsToParentSubEntities()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");

            // Act
            var child = model.AddEntity(parent, "Child");

            // Assert
            Assert.Single(parent.SubEntities);
            Assert.Equal("Child", child.Name);
            Assert.Same(parent, child.Parent);
        }

        [Fact]
        public void AddEntityAttr_SetsIdFromExpression()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "FieldName",
                DataType = DataType.String
            });

            // Assert
            Assert.NotNull(attr.Id);
            Assert.NotEmpty(attr.Id);
        }

        [Fact]
        public void AddEntityAttr_NullParent_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new MetaData();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                model.AddEntityAttr(new MetaEntityAttrDescriptor
                {
                    Parent = null,
                    Expression = "Field",
                    DataType = DataType.String
                })
            );
        }

        [Fact]
        public void AddEntityAttr_NullDescriptor_ThrowsArgumentNullException()
        {
            // Arrange
            var model = new MetaData();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => model.AddEntityAttr(null));
        }

        #endregion

        #region JSON Roundtrip Tests

        [Fact]
        public void JsonRoundtrip_EntityWithAttributes_PreservesStructure()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "TestModel";
            model.Description = "Test Description";

            var entity = model.AddEntity(null, "Category");
            entity.Description = "A category";
            entity.NamePlural = "Categories";
            entity.IsEditable = false;
            entity.UserData = "custom-data";

            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Id",
                DataType = DataType.Int32
            });
            attr1.Caption = "ID";
            attr1.IsPrimaryKey = true;
            attr1.ShowOnCreate = false;

            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr2.Caption = "Name";
            attr2.Size = 200;
            attr2.Description = "Category name";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            Assert.Equal("TestModel", model2.Name);
            Assert.Equal("Test Description", model2.Description);

            Assert.Single(model2.EntityRoot.SubEntities);
            var loadedEntity = model2.EntityRoot.SubEntities[0];
            Assert.Equal("Category", loadedEntity.Name);
            Assert.Equal("A category", loadedEntity.Description);
            Assert.Equal("Categories", loadedEntity.NamePlural);
            Assert.False(loadedEntity.IsEditable);
            Assert.Equal("custom-data", loadedEntity.UserData);

            Assert.Equal(2, loadedEntity.Attributes.Count);

            var loadedAttr1 = loadedEntity.Attributes[0];
            Assert.Equal("ID", loadedAttr1.Caption);
            Assert.Equal(DataType.Int32, loadedAttr1.DataType);
            Assert.True(loadedAttr1.IsPrimaryKey);
            Assert.False(loadedAttr1.ShowOnCreate);

            var loadedAttr2 = loadedEntity.Attributes[1];
            Assert.Equal("Name", loadedAttr2.Caption);
            Assert.Equal(DataType.String, loadedAttr2.DataType);
            Assert.Equal(200, loadedAttr2.Size);
            Assert.Equal("Category name", loadedAttr2.Description);
        }

        [Fact]
        public void JsonRoundtrip_NestedEntities_PreservesHierarchy()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "HierarchyModel";

            var parent = model.AddEntity(null, "Parent");
            parent.Description = "Parent Entity";

            var child = model.AddEntity(parent, "Child");
            child.Description = "Child Entity";

            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = parent,
                Expression = "ParentField",
                DataType = DataType.String
            });

            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "ChildField",
                DataType = DataType.Int32
            });

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            Assert.Single(model2.EntityRoot.SubEntities);
            var loadedParent = model2.EntityRoot.SubEntities[0];
            Assert.Equal("Parent", loadedParent.Name);
            Assert.Equal("Parent Entity", loadedParent.Description);
            Assert.Single(loadedParent.Attributes);

            Assert.Single(loadedParent.SubEntities);
            var loadedChild = loadedParent.SubEntities[0];
            Assert.Equal("Child", loadedChild.Name);
            Assert.Equal("Child Entity", loadedChild.Description);
            Assert.Single(loadedChild.Attributes);
            Assert.Equal(DataType.Int32, loadedChild.Attributes[0].DataType);
        }

        [Fact]
        public void JsonRoundtrip_AttributeWithAllProperties_PreservesAllValues()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "FullAttrModel";

            var entity = model.AddEntity(null, "Order");

            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "TotalAmount",
                DataType = DataType.Currency
            });
            attr.Caption = "Total Amount";
            attr.Description = "The total order amount";
            attr.Size = 50;
            attr.IsPrimaryKey = false;
            attr.IsForeignKey = true;
            attr.IsNullable = true;
            attr.IsEditable = false;
            attr.ShowOnView = false;
            attr.ShowOnEdit = false;
            attr.ShowOnCreate = false;
            attr.ShowInLookup = true;
            attr.UserData = "attr-userdata";
            attr.DisplayFormat = "{0:F2}";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedEntity = model2.EntityRoot.SubEntities[0];
            var loadedAttr = loadedEntity.Attributes[0];

            Assert.Equal("Total Amount", loadedAttr.Caption);
            Assert.Equal("The total order amount", loadedAttr.Description);
            Assert.Equal(DataType.Currency, loadedAttr.DataType);
            Assert.Equal(50, loadedAttr.Size);
            Assert.False(loadedAttr.IsPrimaryKey);
            Assert.True(loadedAttr.IsForeignKey);
            Assert.True(loadedAttr.IsNullable);
            Assert.False(loadedAttr.IsEditable);
            Assert.False(loadedAttr.ShowOnView);
            Assert.False(loadedAttr.ShowOnEdit);
            Assert.False(loadedAttr.ShowOnCreate);
            Assert.True(loadedAttr.ShowInLookup);
            Assert.Equal("attr-userdata", loadedAttr.UserData);
            Assert.Equal("{0:F2}", loadedAttr.DisplayFormat);
        }

        [Fact]
        public void JsonRoundtrip_LookupAttributes_PreservesReferences()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "LookupModel";

            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");

            var customerIdAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            customerIdAttr.Caption = "Customer ID";
            customerIdAttr.IsForeignKey = true;

            var customerNameAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customerEntity,
                Expression = "Name",
                DataType = DataType.String
            });
            customerNameAttr.Caption = "Customer Name";

            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.Caption = "Customer";
            lookupAttr.LookupEntity = customerEntity;
            lookupAttr.DataAttr = customerIdAttr;
            lookupAttr.LookupDataAttribute = customerNameAttr;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedOrderEntity = model2.FindEntity("Order");
            Assert.NotNull(loadedOrderEntity);

            var loadedLookupAttr = loadedOrderEntity.FindAttributeByCaption("Customer");
            Assert.NotNull(loadedLookupAttr);
            Assert.Equal(EntityAttrKind.Lookup, loadedLookupAttr.Kind);

            Assert.NotNull(loadedLookupAttr.LookupEntity);
            Assert.Equal("Customer", loadedLookupAttr.LookupEntity.Name);

            Assert.NotNull(loadedLookupAttr.DataAttr);
            Assert.Equal("Customer ID", loadedLookupAttr.DataAttr.Caption);

            Assert.NotNull(loadedLookupAttr.LookupDataAttribute);
            Assert.Equal("Customer Name", loadedLookupAttr.LookupDataAttribute.Caption);
        }

        [Fact]
        public void JsonRoundtrip_MultipleEntities_PreservesAll()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "MultiEntityModel";

            var entity1 = model.AddEntity(null, "Category");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity1,
                Expression = "Id",
                DataType = DataType.Int32
            });

            var entity2 = model.AddEntity(null, "Product");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity2,
                Expression = "Name",
                DataType = DataType.String
            });

            var entity3 = model.AddEntity(null, "Order");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity3,
                Expression = "Date",
                DataType = DataType.DateTime
            });

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            Assert.Equal(3, model2.EntityRoot.SubEntities.Count);
            Assert.NotNull(model2.FindEntity("Category"));
            Assert.NotNull(model2.FindEntity("Product"));
            Assert.NotNull(model2.FindEntity("Order"));
        }

        [Fact]
        public void JsonRoundtrip_EmptyModel_PreservesBasicProperties()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "EmptyModel";
            model.Description = "An empty model";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            Assert.Equal("EmptyModel", model2.Name);
            Assert.Equal("An empty model", model2.Description);
            Assert.Empty(model2.EntityRoot.SubEntities);
            Assert.Empty(model2.EntityRoot.Attributes);
        }

        [Fact]
        public void JsonRoundtrip_EntityAttrKinds_PreservedCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "KindModel";

            var entity = model.AddEntity(null, "TestEntity");

            var dataAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "DataField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Data
            });

            var virtualAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "VirtualField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });

            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "LookupField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedEntity = model2.EntityRoot.SubEntities[0];
            Assert.Equal(3, loadedEntity.Attributes.Count);
            Assert.Equal(EntityAttrKind.Data, loadedEntity.Attributes[0].Kind);
            Assert.Equal(EntityAttrKind.Virtual, loadedEntity.Attributes[1].Kind);
            Assert.Equal(EntityAttrKind.Lookup, loadedEntity.Attributes[2].Kind);
        }

        [Fact]
        public void JsonRoundtrip_DoubleSerialize_ProducesConsistentOutput()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "ConsistencyModel";
            model.Description = "Test consistency";

            var entity = model.AddEntity(null, "Product");
            entity.NamePlural = "Products";
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency
            });
            attr.Caption = "Product Price";
            attr.Size = 100;

            // Act
            var json1 = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json1);
            var json2 = model2.SaveToJsonString();

            // Assert
            Assert.Equal(json1, json2);
        }

        #endregion

        #region MetaData Tests

        [Fact]
        public void MetaData_Constructor_InitializesCorrectly()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.NotNull(model.Id);
            Assert.NotEmpty(model.Id);
            Assert.NotNull(model.EntityRoot);
            Assert.NotNull(model.Editors);
            Assert.True(model.IsEmpty);
            Assert.Equal(1, model.ModelVersion);
        }

        [Fact]
        public void IsEmpty_EmptyModel_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var isEmpty = model.IsEmpty;

            // Assert
            Assert.True(isEmpty);
        }

        [Fact]
        public void IsEmpty_ModelWithEntities_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");

            // Act
            var isEmpty = model.IsEmpty;

            // Assert
            Assert.False(isEmpty);
        }

        [Fact]
        public void Clear_RemovesAllEntitiesAndAttributes()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Category");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });

            // Act
            model.Clear();

            // Assert
            Assert.True(model.IsEmpty);
            Assert.Empty(model.EntityRoot.SubEntities);
            Assert.Empty(model.EntityRoot.Attributes);
        }

        [Fact]
        public void FindEntity_ByName_ReturnsEntity()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Category");
            model.AddEntity(null, "Product");

            // Act
            var result = model.FindEntity("Category");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Category", result.Name);
        }

        [Fact]
        public void FindEntity_ByPredicate_ReturnsMatchingEntity()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Category");
            entity.Description = "target";

            // Act
            var result = model.FindEntity(e => e.Description == "target");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Category", result.Name);
        }

        [Fact]
        public void FindEntityAttr_ById_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });

            // Act
            var result = model.FindEntityAttr(attr.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attr.Id, result.Id);
        }

        [Fact]
        public void FindEntityAttr_ByExpression_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            });

            // Act
            var result = model.FindEntityAttr("CompanyName");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CompanyName", result.Expr);
        }

        [Fact]
        public void FindEntityAttr_ByCaption_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });
            attr.Caption = "My Special Caption";

            // Act
            var result = model.FindEntityAttr("My Special Caption");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("My Special Caption", result.Caption);
        }

        [Fact]
        public void FindEntityAttr_NotFound_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });

            // Act
            var result = model.FindEntityAttr("NonExisting");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetAttributeById_ExistingId_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });

            // Act
            var result = model.GetAttributeById(attr.Id, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(attr.Id, result.Id);
        }

        [Fact]
        public void IncreaseModelVersion_IncrementsByOne()
        {
            // Arrange
            var model = new MetaData();
            var initialVersion = model.ModelVersion;

            // Act
            model.IncreaseModelVersion();

            // Assert
            Assert.Equal(initialVersion + 1, model.ModelVersion);
        }

        [Fact]
        public void SortEntities_SortsEntitiesAndAttributesByName()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Zebra");
            model.AddEntity(null, "Apple");
            model.AddEntity(null, "Mango");

            var entity = model.EntityRoot.SubEntities[0];
            var attrZ = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "ZField",
                DataType = DataType.String
            });
            attrZ.Caption = "ZField";
            var attrA = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "AField",
                DataType = DataType.String
            });
            attrA.Caption = "AField";

            // Act
            model.SortEntities();

            // Assert
            Assert.Equal("Apple", model.EntityRoot.SubEntities[0].Name);
            Assert.Equal("Mango", model.EntityRoot.SubEntities[1].Name);
            Assert.Equal("Zebra", model.EntityRoot.SubEntities[2].Name);
        }

        #endregion

        #region CheckModel Tests

        [Fact]
        public void CheckModel_NullModel_ThrowsMetaDataExceptionWithRootInfo()
        {
            // Arrange
            // Create an entity without a model (using internal constructor via reflection-free approach:
            // MetaEntity(MetaEntity parent) sets parent but no model)
            var standaloneRoot = new MetaEntity((MetaEntity)null);
            standaloneRoot.Name = "StandaloneRoot";

            var child = new MetaEntity(standaloneRoot);
            child.Name = "ChildEntity";
            standaloneRoot.SubEntities.Add(child);

            var attr = new MetaEntityAttr(child, EntityAttrKind.Data);
            attr.Id = "test-attr";
            attr._lookupEntityId = "SomeEntity";
            child.Attributes.Add(attr);

            // Act & Assert
            var ex = Assert.Throws<MetaDataException>(() => { var _ = attr.LookupEntity; });
            Assert.Contains("Model is not specified for entity:", ex.Message);
            Assert.Contains("StandaloneRoot", ex.Message);
        }

        [Fact]
        public void CheckModel_NullModelSingleEntity_ThrowsMetaDataException()
        {
            // Arrange
            var standaloneEntity = new MetaEntity((MetaEntity)null);
            standaloneEntity.Name = "Standalone";

            var attr = new MetaEntityAttr(standaloneEntity, EntityAttrKind.Data);
            attr.Id = "attr1";
            attr._lookupEntityId = "X";
            standaloneEntity.Attributes.Add(attr);

            // Act & Assert
            var ex = Assert.Throws<MetaDataException>(() => { var _ = attr.LookupEntity; });
            Assert.Contains("Model is not specified for entity: Standalone", ex.Message);
            Assert.Contains("root: True", ex.Message);
        }

        #endregion

        #region LookupEntity Tests

        [Fact]
        public void LookupEntity_EmptyLookupEntityId_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.Int32
            });

            // Act
            var lookupEntity = attr.LookupEntity;

            // Assert
            Assert.Null(lookupEntity);
        }

        [Fact]
        public void LookupEntity_SetAndGet_ReturnsCorrectEntity()
        {
            // Arrange
            var model = new MetaData();
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });

            // Act
            attr.LookupEntity = customerEntity;

            // Assert
            Assert.Same(customerEntity, attr.LookupEntity);
            Assert.Equal(customerEntity.Id, attr._lookupEntityId);
        }

        [Fact]
        public void LookupEntity_SetNull_ClearsId()
        {
            // Arrange
            var model = new MetaData();
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            attr.LookupEntity = customerEntity;

            // Act
            attr.LookupEntity = null;

            // Assert
            Assert.Null(attr._lookupEntityId);
        }

        [Fact]
        public void LookupEntity_LazyLoadFromId_FindsEntityInModel()
        {
            // Arrange
            var model = new MetaData();
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            // Set the internal ID directly to simulate deserialization
            attr._lookupEntityId = customerEntity.Id;

            // Act
            var result = attr.LookupEntity;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Customer", result.Name);
        }

        #endregion

        #region LookupAttr Bidirectional Link Tests

        [Fact]
        public void LookupAttr_SetValue_SetsBidirectionalLink()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var dataAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            lookupAttr.LookupAttr = dataAttr;

            // Assert
            Assert.Same(dataAttr, lookupAttr.LookupAttr);
            Assert.Equal(lookupAttr.Id, dataAttr._lookupAttrId);
            Assert.Equal(dataAttr.Id, lookupAttr._lookupAttrId);
        }

        [Fact]
        public void LookupAttr_SetNull_ClearsLookupAttrId()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var dataAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.LookupAttr = dataAttr;

            // Act
            lookupAttr.LookupAttr = null;

            // Assert
            Assert.Null(lookupAttr._lookupAttrId);
        }

        #endregion

        #region JSON Serialization - Optional Properties

        [Fact]
        public void WriteAndReadJson_WithDisplayFormat_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "DisplayFormatModel";
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency
            });
            attr.Caption = "Price";
            attr.DisplayFormat = "{0:F2}";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Equal("{0:F2}", loadedAttr.DisplayFormat);
        }

        [Fact]
        public void WriteAndReadJson_WithDescription_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "DescModel";
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Name";
            attr.Description = "The product name";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Equal("The product name", loadedAttr.Description);
        }

        [Fact]
        public void WriteAndReadJson_WithUserData_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "UserDataModel";
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Category",
                DataType = DataType.String
            });
            attr.Caption = "Category";
            attr.UserData = "custom-user-data-value";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Equal("custom-user-data-value", loadedAttr.UserData);
        }

        [Fact]
        public void WriteAndReadJson_WithLookupAttr_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "LookupAttrModel";
            var entity = model.AddEntity(null, "Order");
            var dataAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            dataAttr.Caption = "Customer ID";
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.Caption = "Customer";
            lookupAttr.LookupAttr = dataAttr;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedEntity = model2.EntityRoot.SubEntities[0];
            var loadedLookup = loadedEntity.FindAttributeByCaption("Customer");
            Assert.NotNull(loadedLookup);
            Assert.NotNull(loadedLookup.LookupAttr);
            Assert.Equal("Customer ID", loadedLookup.LookupAttr.Caption);
        }

        [Fact]
        public void WriteAndReadJson_WithDataAttr_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "DataAttrModel";
            var entity = model.AddEntity(null, "Order");
            var dataAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            dataAttr.Caption = "Customer ID";
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.Caption = "Customer";
            lookupAttr.DataAttr = dataAttr;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedEntity = model2.EntityRoot.SubEntities[0];
            var loadedLookup = loadedEntity.FindAttributeByCaption("Customer");
            Assert.NotNull(loadedLookup);
            Assert.NotNull(loadedLookup.DataAttr);
            Assert.Equal("Customer ID", loadedLookup.DataAttr.Caption);
        }

        [Fact]
        public void WriteAndReadJson_WithLookupEntity_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "LookupEntityModel";
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customerEntity,
                Expression = "Name",
                DataType = DataType.String
            });

            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.Caption = "Customer";
            lookupAttr.LookupEntity = customerEntity;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedOrderEntity = model2.FindEntity("Order");
            var loadedLookup = loadedOrderEntity.FindAttributeByCaption("Customer");
            Assert.NotNull(loadedLookup);
            Assert.NotNull(loadedLookup.LookupEntity);
            Assert.Equal("Customer", loadedLookup.LookupEntity.Name);
        }

        [Fact]
        public void WriteAndReadJson_WithLookupDataAttribute_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "LookupDataAttrModel";
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            var customerNameAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customerEntity,
                Expression = "Name",
                DataType = DataType.String
            });
            customerNameAttr.Caption = "Customer Name";

            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.Caption = "Customer";
            lookupAttr.LookupDataAttribute = customerNameAttr;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedOrderEntity = model2.FindEntity("Order");
            var loadedLookup = loadedOrderEntity.FindAttributeByCaption("Customer");
            Assert.NotNull(loadedLookup);
            Assert.NotNull(loadedLookup.LookupDataAttribute);
            Assert.Equal("Customer Name", loadedLookup.LookupDataAttribute.Caption);
        }

        [Fact]
        public void WriteAndReadJson_WithDefaultEditor_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "EditorModel";
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Name";
            var editor = new TextValueEditor();
            attr.DefaultEditor = editor;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.NotNull(loadedAttr.DefaultEditor);
            Assert.Equal(editor.Id, loadedAttr.DefaultEditor.Id);
        }

        [Fact]
        public void WriteAndReadJson_NoOptionalProperties_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "MinimalModel";
            var entity = model.AddEntity(null, "Simple");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });
            attr.Caption = "Field";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Null(loadedAttr.DefaultValue);
            Assert.Null(loadedAttr.LookupAttr);
            Assert.Null(loadedAttr.DataAttr);
            Assert.Null(loadedAttr.LookupEntity);
            Assert.Null(loadedAttr.LookupDataAttribute);
            Assert.Null(loadedAttr.DefaultEditor);
            Assert.True(string.IsNullOrEmpty(loadedAttr.DisplayFormat));
            Assert.True(string.IsNullOrEmpty(loadedAttr.Description));
            Assert.Null(loadedAttr.UserData);
        }

        [Fact]
        public void WriteAndReadJson_VirtualKind_SetsKindToVirtual()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "VirtualReadModel";
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Computed",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });
            attr.Caption = "Computed";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Equal(EntityAttrKind.Virtual, loadedAttr.Kind);
            Assert.True(loadedAttr.IsVirtual);
        }

        #endregion

        #region MetaEntityAttrStore.ReadFromJsonAsync Tests

        [Fact]
        public void MetaEntityAttrStore_ReadFromJson_ReadsMultipleAttributes()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "StoreReadModel";
            var entity = model.AddEntity(null, "Product");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Id",
                DataType = DataType.Int32
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency
            });

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedEntity = model2.EntityRoot.SubEntities[0];
            Assert.Equal(3, loadedEntity.Attributes.Count);
            Assert.Equal(DataType.Int32, loadedEntity.Attributes[0].DataType);
            Assert.Equal(DataType.String, loadedEntity.Attributes[1].DataType);
            Assert.Equal(DataType.Currency, loadedEntity.Attributes[2].DataType);
        }

        [Fact]
        public void MetaEntityAttrStore_Model_ReturnsEntityModel()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Product");

            // Act
            var storeModel = entity.Attributes.Model;

            // Assert
            Assert.Same(model, storeModel);
        }

        [Fact]
        public void MetaEntityAttrStore_Model_NullEntity_ReturnsNull()
        {
            // Arrange
            var store = new MetaEntityAttrStore(null);

            // Act
            var storeModel = store.Model;

            // Assert
            Assert.Null(storeModel);
        }

        #endregion

        #region FullExpr Property Tests

        [Fact]
        public void FullExpr_DataAttr_ReturnsEntityDotExpr()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "TotalAmount",
                DataType = DataType.Currency
            });

            // Act
            var fullExpr = attr.FullExpr;

            // Assert
            Assert.Equal("Order:TotalAmount", fullExpr);
        }

        [Fact]
        public void FullExpr_VirtualAttr_ReturnsExprOnly()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CalculatedField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });

            // Act
            var fullExpr = attr.FullExpr;

            // Assert
            Assert.Equal("CalculatedField", fullExpr);
        }

        [Fact]
        public void FullExpr_NestedEntity_ReturnsFullPath()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            var child = model.AddEntity(parent, "Child");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "Field",
                DataType = DataType.String
            });

            // Act
            var fullExpr = attr.FullExpr;

            // Assert
            Assert.Equal("Parent.Child:Field", fullExpr);
        }

        #endregion

        #region CompareWithExpr Tests

        [Fact]
        public void CompareWithExpr_ExactMatch_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "FieldName",
                DataType = DataType.String
            });

            // Act
            var result = attr.CompareWithExpr("FieldName");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CompareWithExpr_DifferentCase_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "FieldName",
                DataType = DataType.String
            });

            // Act
            var result = attr.CompareWithExpr("fieldname");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CompareWithExpr_DifferentExpr_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "FieldName",
                DataType = DataType.String
            });

            // Act
            var result = attr.CompareWithExpr("OtherField");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CompareWithExpr_MixedCaseMatch_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            });

            // Act
            var result = attr.CompareWithExpr("COMPANYNAME");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region Expr Setter Tests

        [Fact]
        public void Expr_SetOnVirtualAttr_UpdatesExprValue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Initial",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });

            // Act
            attr.Expr = "NewExpression";

            // Assert
            Assert.Equal("NewExpression", attr.Expr);
        }

        [Fact]
        public void Expr_SetOnDataAttr_UpdatesExprValue()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Initial",
                DataType = DataType.String,
                Kind = EntityAttrKind.Data
            });

            // Act
            attr.Expr = "UpdatedExpression";

            // Assert
            Assert.Equal("UpdatedExpression", attr.Expr);
        }

        [Fact]
        public void Expr_SetSameValue_DoesNotChange()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });

            // Act
            attr.Expr = "Field"; // same value

            // Assert
            Assert.Equal("Field", attr.Expr);
        }

        #endregion

        #region LookupDataAttribute Tests

        [Fact]
        public void LookupDataAttribute_EmptyId_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            var result = attr.LookupDataAttribute;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LookupDataAttribute_SetAndGet_ReturnsCorrectAttr()
        {
            // Arrange
            var model = new MetaData();
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            var customerNameAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customerEntity,
                Expression = "Name",
                DataType = DataType.String
            });
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            lookupAttr.LookupDataAttribute = customerNameAttr;

            // Assert
            Assert.Same(customerNameAttr, lookupAttr.LookupDataAttribute);
            Assert.Equal(customerNameAttr.Id, lookupAttr._lookupDataAttrId);
        }

        [Fact]
        public void LookupDataAttribute_SetNull_ClearsId()
        {
            // Arrange
            var model = new MetaData();
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            var customerNameAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customerEntity,
                Expression = "Name",
                DataType = DataType.String
            });
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.LookupDataAttribute = customerNameAttr;

            // Act
            lookupAttr.LookupDataAttribute = null;

            // Assert
            Assert.Null(lookupAttr._lookupDataAttrId);
        }

        #endregion

        #region DataAttr Lazy Load Tests

        [Fact]
        public void DataAttr_EmptyId_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            var result = attr.DataAttr;

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void DataAttr_LazyLoadFromId_FindsAttrInModel()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var dataAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerId",
                DataType = DataType.Int32
            });
            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            // Simulate deserialization by setting internal ID directly
            lookupAttr._dataAttrId = dataAttr.Id;

            // Act
            var result = lookupAttr.DataAttr;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CustomerId", result.Expr);
        }

        #endregion

        #region CopyFrom Tests

        [Fact]
        public void CopyFrom_CopiesAllProperties()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var source = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Source",
                DataType = DataType.Currency
            });
            source.Caption = "Source Caption";
            source.Description = "Source Desc";
            source.Size = 250;
            source.IsNullable = true;
            source.IsPrimaryKey = true;
            source.IsForeignKey = true;
            source.IsEditable = false;
            source.ShowInLookup = true;
            source.ShowOnView = false;
            source.ShowOnEdit = false;
            source.ShowOnCreate = false;
            source.UserData = "user-data";

            var target = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Target",
                DataType = DataType.String
            });

            // Act
            target.CopyFrom(source);

            // Assert
            Assert.Equal("Source Caption", target.Caption);
            Assert.Equal("Source Desc", target.Description);
            Assert.Equal("Source", target.Expr);
            Assert.Equal(DataType.Currency, target.DataType);
            Assert.Equal(250, target.Size);
            Assert.True(target.IsNullable);
            Assert.True(target.IsPrimaryKey);
            Assert.True(target.IsForeignKey);
            Assert.False(target.IsEditable);
            Assert.True(target.ShowInLookup);
            Assert.False(target.ShowOnView);
            Assert.False(target.ShowOnEdit);
            Assert.False(target.ShowOnCreate);
            Assert.Equal("user-data", target.UserData);
        }

        #endregion

        #region IsVirtual Property Tests (additional)

        [Fact]
        public void IsVirtual_LookupKind_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Lookup",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });

            // Act
            var result = attr.IsVirtual;

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetFullCaption Tests

        [Fact]
        public void GetFullCaption_DefaultSeparator_ReturnsEntitySpaceCaption()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Date",
                DataType = DataType.Date
            });
            attr.Caption = "Order Date";

            // Act
            var result = attr.GetFullCaption();

            // Assert
            Assert.Equal("Order Order Date", result);
        }

        [Fact]
        public void GetFullCaption_CustomSeparator_UsesProvidedSeparator()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Date",
                DataType = DataType.Date
            });
            attr.Caption = "Order Date";

            // Act
            var result = attr.GetFullCaption(".");

            // Assert
            Assert.Equal("Order.Order Date", result);
        }

        #endregion

        #region JSON Roundtrip - Unknown Property Handling

        [Fact]
        public void WriteAndReadJson_UnknownProperty_SkippedGracefully()
        {
            // Arrange - build a model, serialize, then manually inject an unknown property
            var model = new MetaData();
            model.Name = "UnknownPropModel";
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Name";

            var json = model.SaveToJsonString();
            // Inject an unknown attribute property into the JSON
            json = json.Replace("\"cptn\":\"Name\"", "\"cptn\":\"Name\",\"unknownProp\":\"unknownValue\"");

            // Act
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert - should load without error, unknown property is skipped
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Equal("Name", loadedAttr.Caption);
        }

        #endregion

        #region Entity JSON Roundtrip - Optional Properties

        [Fact]
        public void JsonRoundtrip_EntityWithNamePlural_PreservesNamePlural()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            entity.NamePlural = "TestEntities";
            entity.Description = "A test entity";
            entity.UserData = "some-data";
            entity.IsEditable = false;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);
            var entity2 = model2.FindEntity("TestEntity");

            // Assert
            Assert.Equal("TestEntities", entity2.NamePlural);
            Assert.Equal("A test entity", entity2.Description);
            Assert.Equal("some-data", entity2.UserData);
            Assert.False(entity2.IsEditable);
        }

        #endregion

        #region MetaEntityStore - Self-Reference and Reparenting

        [Fact]
        public void MetaEntityStore_AddSelfReference_ThrowsArgumentException()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => entity.SubEntities.Add(entity));
        }

        [Fact]
        public void MetaEntityStore_AddEntityWithDifferentParent_ReparentsEntity()
        {
            // Arrange
            var model = new MetaData();
            var parent1 = model.AddEntity(null, "Parent1");
            var parent2 = model.AddEntity(null, "Parent2");
            var child = model.CreateEntity(parent1);
            child.Name = "Child";
            parent1.SubEntities.Add(child);
            Assert.Single(parent1.SubEntities);

            // Act
            parent2.SubEntities.Add(child);

            // Assert
            Assert.Empty(parent1.SubEntities);
            Assert.Single(parent2.SubEntities);
        }

        #endregion

        #region Attribute JSON Roundtrip - Virtual Kind

        [Fact]
        public void JsonRoundtrip_VirtualAttribute_PreservesVirtualKind()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "VirtualAttrModel";
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "ComputedField",
                DataType = DataType.String,
                Kind = EntityAttrKind.Virtual
            });
            attr.Caption = "Computed Field";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];

            // Assert
            Assert.Equal(EntityAttrKind.Virtual, loadedAttr.Kind);
            Assert.True(loadedAttr.IsVirtual);
            Assert.Equal("Computed Field", loadedAttr.Caption);
        }

        #endregion

        #region Attribute JSON Roundtrip - DefaultValue

        [Fact]
        public void JsonRoundtrip_AttributeWithDefaultValue_WritesDefValToJson()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "DefaultValueModel";
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Status",
                DataType = DataType.String
            });
            attr.Caption = "Status";
            attr.DefaultValue = "Active";

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("defVal", json);
        }

        #endregion

        #region Attribute ReadPropertyFromJsonAsync - Opg Case

        [Fact]
        public void JsonRoundtrip_WithOpgProperty_SkippedGracefully()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "OpgModel";
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Name";

            var json = model.SaveToJsonString();
            // Inject an "opg" property into the attribute JSON
            json = json.Replace("\"cptn\":\"Name\"", "\"cptn\":\"Name\",\"opg\":42");

            // Act
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Equal("Name", loadedAttr.Caption);
        }

        #endregion

        #region Attribute ReadPropertyFromJsonAsync - Unknown Property Case

        [Fact]
        public void JsonRoundtrip_WithUnknownAttrProperty_SkippedGracefully()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "UnknownAttrModel";
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });
            attr.Caption = "Name";

            var json = model.SaveToJsonString();
            // Inject a completely unknown property into the attribute JSON
            json = json.Replace("\"cptn\":\"Name\"", "\"cptn\":\"Name\",\"totallyUnknown\":\"abc\"");

            // Act
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];
            Assert.Equal("Name", loadedAttr.Caption);
        }

        #endregion

        #region LookupEntity Getter - Non-Empty LookupEntityId via JSON Roundtrip

        [Fact]
        public void JsonRoundtrip_LookupEntityId_ResolvesAfterDeserialization()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "LookupEntityResolveModel";
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customerEntity,
                Expression = "Id",
                DataType = DataType.Int32
            });

            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.Caption = "Customer";
            lookupAttr.LookupEntity = customerEntity;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);
            var loadedOrderEntity = model2.FindEntity("Order");
            var loadedLookup = loadedOrderEntity.FindAttributeByCaption("Customer");

            // Assert
            Assert.NotNull(loadedLookup.LookupEntity);
            Assert.Equal("Customer", loadedLookup.LookupEntity.Name);
        }

        #endregion

        #region LookupDataAttribute Getter - Non-Empty LookupDataAttrId via JSON Roundtrip

        [Fact]
        public void JsonRoundtrip_LookupDataAttrId_ResolvesAfterDeserialization()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "LookupDataAttrResolveModel";
            var orderEntity = model.AddEntity(null, "Order");
            var customerEntity = model.AddEntity(null, "Customer");
            var customerNameAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customerEntity,
                Expression = "Name",
                DataType = DataType.String
            });
            customerNameAttr.Caption = "Customer Name";

            var lookupAttr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderEntity,
                Expression = "Customer",
                DataType = DataType.String,
                Kind = EntityAttrKind.Lookup
            });
            lookupAttr.Caption = "Customer";
            lookupAttr.LookupDataAttribute = customerNameAttr;

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);
            var loadedOrderEntity = model2.FindEntity("Order");
            var loadedLookup = loadedOrderEntity.FindAttributeByCaption("Customer");

            // Assert
            Assert.NotNull(loadedLookup.LookupDataAttribute);
            Assert.Equal("Customer Name", loadedLookup.LookupDataAttribute.Caption);
        }

        #endregion

        #region GetFullCaption - Combined Entity Name and Attr Caption

        [Fact]
        public void GetFullCaption_NestedEntity_ConcatenatesFullPath()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Order");
            var child = model.AddEntity(parent, "Details");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "Quantity",
                DataType = DataType.Int32
            });
            attr.Caption = "Quantity";

            // Act
            var result = attr.GetFullCaption(".");

            // Assert
            Assert.Equal("Order.Details.Quantity", result);
        }

        [Fact]
        public void GetFullCaption_WithDashSeparator_UsesSeparatorConsistently()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Product");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency
            });
            attr.Caption = "Unit Price";

            // Act
            var result = attr.GetFullCaption(" - ");

            // Assert
            Assert.Equal("Product - Unit Price", result);
        }

        #endregion

        #region MetaEntityAttrList.Reorder Tests

        [Fact]
        public void Reorder_AttributesWithDifferentIndexValues_SortsByIndex()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "ZField",
                DataType = DataType.String
            });
            attr1.Caption = "ZField";
            attr1.Index = 30;
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "AField",
                DataType = DataType.String
            });
            attr2.Caption = "AField";
            attr2.Index = 10;
            var attr3 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "MField",
                DataType = DataType.String
            });
            attr3.Caption = "MField";
            attr3.Index = 20;

            // Act
            entity.Attributes.Reorder();

            // Assert
            Assert.Equal("AField", entity.Attributes[0].Caption);
            Assert.Equal("MField", entity.Attributes[1].Caption);
            Assert.Equal("ZField", entity.Attributes[2].Caption);
        }

        [Fact]
        public void Reorder_AttributesWithSameIndex_DoesNotCrash()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String
            });
            attr1.Index = 5;
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field2",
                DataType = DataType.String
            });
            attr2.Index = 5;

            // Act
            var exception = Record.Exception(() => entity.Attributes.Reorder());

            // Assert
            Assert.Null(exception);
            Assert.Equal(2, entity.Attributes.Count);
        }

        #endregion

        #region MetaEntityAttrList.SortByCaption Tests

        [Fact]
        public void SortByCaption_AttributesWithDifferentCaptions_SortsAlphabetically()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "C",
                DataType = DataType.String
            });
            attr1.Caption = "Charlie";
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "A",
                DataType = DataType.String
            });
            attr2.Caption = "Alpha";
            var attr3 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "B",
                DataType = DataType.String
            });
            attr3.Caption = "Bravo";

            // Act
            entity.Attributes.SortByCaption();

            // Assert
            Assert.Equal("Alpha", entity.Attributes[0].Caption);
            Assert.Equal("Bravo", entity.Attributes[1].Caption);
            Assert.Equal("Charlie", entity.Attributes[2].Caption);
        }

        [Fact]
        public void SortByCaption_CaseInsensitive_SortsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Z",
                DataType = DataType.String
            });
            attr1.Caption = "zebra";
            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "A",
                DataType = DataType.String
            });
            attr2.Caption = "Apple";

            // Act
            entity.Attributes.SortByCaption();

            // Assert
            Assert.Equal("Apple", entity.Attributes[0].Caption);
            Assert.Equal("zebra", entity.Attributes[1].Caption);
        }

        #endregion

        #region Entity JSON Roundtrip - Unknown Entity Property

        [Fact]
        public void JsonRoundtrip_WithUnknownEntityProperty_SkippedGracefully()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "UnknownEntPropModel";
            var entity = model.AddEntity(null, "Product");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String
            });

            var json = model.SaveToJsonString();
            // Inject an unknown entity property into the JSON
            json = json.Replace("\"name\":\"Product\"", "\"name\":\"Product\",\"unknownEntProp\":\"xyz\"");

            // Act
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            var loadedEntity = model2.FindEntity("Product");
            Assert.NotNull(loadedEntity);
            Assert.Equal("Product", loadedEntity.Name);
        }

        #endregion

        #region Attribute JSON Roundtrip - Virtual Boolean Property

        [Fact]
        public void JsonRoundtrip_VirtualBooleanProperty_SetsKindToVirtual()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "VirtualBoolModel";
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String
            });
            attr.Caption = "Field";

            var json = model.SaveToJsonString();
            // Replace "kind":0 with legacy "virtual":true format
            json = json.Replace("\"kind\":0", "\"virtual\":true");

            // Act
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);
            var loadedAttr = model2.EntityRoot.SubEntities[0].Attributes[0];

            // Assert
            Assert.Equal(EntityAttrKind.Virtual, loadedAttr.Kind);
            Assert.True(loadedAttr.IsVirtual);
        }

        #endregion
    }
}
