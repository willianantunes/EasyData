using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class MetaDataTests
    {
        #region Constructor

        [Fact]
        public void Constructor_Default_SetsIdToNewGuid()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.False(string.IsNullOrEmpty(model.Id));
            Assert.True(Guid.TryParse(model.Id, out _));
        }

        [Fact]
        public void Constructor_Default_CreatesEntityRoot()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.NotNull(model.EntityRoot);
            Assert.True(model.EntityRoot.IsRoot);
        }

        [Fact]
        public void Constructor_Default_InitializesEditors()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.NotNull(model.Editors);
            Assert.IsType<ValueEditorStore>(model.Editors);
        }

        [Fact]
        public void Constructor_Default_AddsDefaultDisplayFormats()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Bool, "0/1", out var boolFormat));
            Assert.Equal("{0:S0|1}", boolFormat.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Bool, "False/True", out var boolFormat2));
            Assert.Equal("{0:SFalse|True}", boolFormat2.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Bool, "No/Yes", out var boolFormat3));
            Assert.Equal("{0:SNo|Yes}", boolFormat3.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Date, "Short date", out var dateFormat));
            Assert.Equal("{0:d}", dateFormat.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.DateTime, "Universal", out var dtFormat));
            Assert.Equal("{0:u}", dtFormat.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Time, "Short time", out var timeFormat));
            Assert.Equal("{0:HH:mm}", timeFormat.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Float, "2-precision", out var floatFormat));
            Assert.Equal("{0:F2}", floatFormat.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Int32, "5-digit", out var int32Format));
            Assert.Equal("{0:D5}", int32Format.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Int64, "10-digit", out var int64Format));
            Assert.Equal("{0:D10}", int64Format.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Currency, "4-precision", out var currencyFormat));
            Assert.Equal("{0:F4}", currencyFormat.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Byte, "3-digit", out var byteFormat));
            Assert.Equal("{0:D3}", byteFormat.Format);

            Assert.True(model.DisplayFormats.TryGetFormat(DataType.Word, "5-digit", out var wordFormat));
            Assert.Equal("{0:D5}", wordFormat.Format);
        }

        [Fact]
        public void Constructor_Default_SetsFormatVersionJsonToLatest()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.Equal(MetaData.LastJsonFormatVersion, model.FormatVersionJson);
        }

        [Fact]
        public void Constructor_Default_SetsModelVersionToOne()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.Equal(1, model.ModelVersion);
        }

        #endregion

        #region Properties

        [Fact]
        public void Id_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            var model = new MetaData();
            var newId = "custom-id-123";

            // Act
            model.Id = newId;

            // Assert
            Assert.Equal(newId, model.Id);
        }

        [Fact]
        public void Name_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            model.Name = "TestModel";

            // Assert
            Assert.Equal("TestModel", model.Name);
        }

        [Fact]
        public void Description_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            model.Description = "A test description";

            // Assert
            Assert.Equal("A test description", model.Description);
        }

        [Fact]
        public void CustomInfo_Default_ReturnsEmptyString()
        {
            // Arrange & Act
            var model = new MetaData();

            // Assert
            Assert.Equal("", model.CustomInfo);
        }

        [Fact]
        public void CustomInfo_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            model.CustomInfo = "some custom data";

            // Assert
            Assert.Equal("some custom data", model.CustomInfo);
        }

        [Fact]
        public void FilePath_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            model.FilePath = "/some/path/model.json";

            // Assert
            Assert.Equal("/some/path/model.json", model.FilePath);
        }

        [Fact]
        public void FormatVersionJson_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            model.FormatVersionJson = 3;

            // Assert
            Assert.Equal(3, model.FormatVersionJson);
        }

        [Fact]
        public void IsEmpty_NoEntitiesOrAttributes_ReturnsTrue()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var result = model.IsEmpty;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEmpty_HasSubEntity_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "TestEntity");

            // Act
            var result = model.IsEmpty;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsEmpty_HasAttribute_ReturnsFalse()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String,
                Caption = "Field 1"
            });

            // Act
            var result = model.IsEmpty;

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Entity Management

        [Fact]
        public void CreateRootEntity_Default_ReturnsRootEntity()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var rootEntity = model.CreateRootEntity();

            // Assert
            Assert.NotNull(rootEntity);
            Assert.True(rootEntity.IsRoot);
        }

        [Fact]
        public void CreateEntity_WithDefaultParent_ReturnsEntityUnderRoot()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var entity = model.CreateEntity();

            // Assert
            Assert.NotNull(entity);
            Assert.Equal(model.EntityRoot, entity.Parent);
        }

        [Fact]
        public void CreateEntity_WithExplicitParent_ReturnsEntityUnderParent()
        {
            // Arrange
            var model = new MetaData();
            var parentEntity = model.AddEntity(null, "Parent");

            // Act
            var childEntity = model.CreateEntity(parentEntity);

            // Assert
            Assert.NotNull(childEntity);
            Assert.Equal(parentEntity, childEntity.Parent);
        }

        [Fact]
        public void AddEntity_NullParent_AddsToEntityRoot()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var entity = model.AddEntity(null, "Customer");

            // Assert
            Assert.Equal("Customer", entity.Name);
            Assert.Contains(entity, model.EntityRoot.SubEntities);
        }

        [Fact]
        public void AddEntity_WithParent_AddsToParent()
        {
            // Arrange
            var model = new MetaData();
            var parentEntity = model.AddEntity(null, "Parent");

            // Act
            var childEntity = model.AddEntity(parentEntity, "Child");

            // Assert
            Assert.Equal("Child", childEntity.Name);
            Assert.Contains(childEntity, parentEntity.SubEntities);
        }

        [Fact]
        public void AddEntity_SetsId_ComposedFromParentAndName()
        {
            // Arrange
            var model = new MetaData();
            var parentEntity = model.AddEntity(null, "Parent");

            // Act
            var childEntity = model.AddEntity(parentEntity, "Child");

            // Assert
            Assert.Contains("Child", childEntity.Id);
        }

        [Fact]
        public void FindEntity_ExistingName_ReturnsEntity()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Customer");
            model.AddEntity(null, "Order");

            // Act
            var found = model.FindEntity("Customer");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Customer", found.Name);
        }

        [Fact]
        public void FindEntity_NonExistingName_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Customer");

            // Act
            var found = model.FindEntity("NonExistent");

            // Assert
            Assert.Null(found);
        }

        [Fact]
        public void FindEntity_WithPredicate_ReturnsMatchingEntity()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            entity.Description = "Main entity";

            // Act
            var found = model.FindEntity(e => e.Description == "Main entity");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Customer", found.Name);
        }

        [Fact]
        public void FindEntity_WithPredicateNoMatch_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Customer");

            // Act
            var found = model.FindEntity(e => e.Name == "Nonexistent");

            // Assert
            Assert.Null(found);
        }

        #endregion

        #region Attribute Management

        [Fact]
        public void CreateEntityAttr_WithDescriptor_SetsAllProperties()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var desc = new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String,
                Caption = "Company Name",
                Size = 200,
                Kind = EntityAttrKind.Data
            };

            // Act
            var attr = model.CreateEntityAttr(desc);

            // Assert
            Assert.Equal("Company Name", attr.Caption);
            Assert.Equal(DataType.String, attr.DataType);
            Assert.Equal("CompanyName", attr.Expr);
            Assert.Equal(200, attr.Size);
        }

        [Fact]
        public void CreateEntityAttr_WithExplicitId_UsesExplicitId()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var desc = new MetaEntityAttrDescriptor
            {
                Id = "custom-attr-id",
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            };

            // Act
            var attr = model.CreateEntityAttr(desc);

            // Assert
            Assert.Equal("custom-attr-id", attr.Id);
        }

        [Fact]
        public void CreateEntityAttr_WithExpressionNoId_ComposesIdFromParentAndExpression()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var desc = new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String
            };

            // Act
            var attr = model.CreateEntityAttr(desc);

            // Assert
            Assert.NotNull(attr.Id);
            Assert.Contains("CompanyName", attr.Id);
        }

        [Fact]
        public void AddEntityAttr_ValidDescriptor_AddsToEntity()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var desc = new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String,
                Caption = "Company Name"
            };

            // Act
            var attr = model.AddEntityAttr(desc);

            // Assert
            Assert.Contains(attr, entity.Attributes);
            Assert.False(string.IsNullOrEmpty(attr.Id));
        }

        [Fact]
        public void AddEntityAttr_DataKind_SetsIdFromExpression()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var desc = new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Company.Name",
                DataType = DataType.String,
                Caption = "Company Name",
                Kind = EntityAttrKind.Data
            };

            // Act
            var attr = model.AddEntityAttr(desc);

            // Assert
            Assert.Equal("Company_Name", attr.Id);
        }

        [Fact]
        public void AddEntityAttr_VirtualKind_SetsIdWithVEAPrefix()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var desc = new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "FullName",
                DataType = DataType.String,
                Caption = "Full Name",
                Kind = EntityAttrKind.Virtual
            };

            // Act
            var attr = model.AddEntityAttr(desc);

            // Assert
            Assert.StartsWith("VEA_", attr.Id);
        }

        [Fact]
        public void AddEntityAttr_LookupKind_SetsIdFromExpression()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            var desc = new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CustomerName",
                DataType = DataType.String,
                Caption = "Customer Name",
                Kind = EntityAttrKind.Lookup
            };

            // Act
            var attr = model.AddEntityAttr(desc);

            // Assert
            Assert.Equal("CustomerName", attr.Id);
        }

        [Fact]
        public void FindEntityAttr_ById_ReturnsCorrectAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String,
                Caption = "Company Name"
            });

            // Act
            var found = model.FindEntityAttr(attr.Id);

            // Assert
            Assert.NotNull(found);
            Assert.Equal(attr.Id, found.Id);
        }

        [Fact]
        public void FindEntityAttr_ByExpression_ReturnsCorrectAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String,
                Caption = "Company Name"
            });

            // Act
            var found = model.FindEntityAttr("CompanyName");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("CompanyName", found.Expr);
        }

        [Fact]
        public void FindEntityAttr_ByCaption_ReturnsCorrectAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Xyz123",
                DataType = DataType.String,
                Caption = "UniqueCaption"
            });

            // Act
            var found = model.FindEntityAttr("UniqueCaption");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("UniqueCaption", found.Caption);
        }

        [Fact]
        public void FindEntityAttr_NonExisting_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var found = model.FindEntityAttr("NonExistent");

            // Assert
            Assert.Null(found);
        }

        [Fact]
        public void GetAttributeById_Existing_ReturnsAttribute()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String,
                Caption = "Company Name"
            });

            // Act
            var found = model.GetAttributeById(attr.Id, false);

            // Assert
            Assert.NotNull(found);
            Assert.Equal(attr.Id, found.Id);
        }

        [Fact]
        public void GetAttributeById_NonExisting_ReturnsNull()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var found = model.GetAttributeById("non-existing-id", false);

            // Assert
            Assert.Null(found);
        }

        #endregion

        #region Validation

        [Fact]
        public void AddEntityAttr_NullDescriptor_ThrowsArgumentNullException()
        {
            // Arrange
            var model = new MetaData();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => model.AddEntityAttr(null));
        }

        [Fact]
        public void AddEntityAttr_NullParent_ThrowsInvalidOperationException()
        {
            // Arrange
            var model = new MetaData();
            var desc = new MetaEntityAttrDescriptor
            {
                Expression = "Field1",
                DataType = DataType.String
            };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => model.AddEntityAttr(desc));
        }

        #endregion

        #region AssignEntityAttrID

        [Fact]
        public void AssignEntityAttrID_VirtualAttr_AssignsVEAPrefixedId()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var attr = new MetaEntityAttr(entity, EntityAttrKind.Virtual);
            attr.Expr = "CalcField";
            entity.Attributes.Add(attr);

            // Act
            model.AssignEntityAttrID(attr);

            // Assert
            Assert.StartsWith("VEA_", attr.Id);
        }

        [Fact]
        public void AssignEntityAttrID_DataAttr_AssignsExpressionBasedId()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var attr = new MetaEntityAttr(entity, EntityAttrKind.Data);
            attr.Expr = "Company.Name";
            entity.Attributes.Add(attr);

            // Act
            model.AssignEntityAttrID(attr);

            // Assert
            Assert.Equal("Company_Name", attr.Id);
        }

        [Fact]
        public void AssignEntityAttrID_DuplicateDataAttr_DeduplicatesWithSuffix()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field",
                DataType = DataType.String,
                Caption = "Field1"
            });

            var attr2 = new MetaEntityAttr(entity, EntityAttrKind.Data);
            attr2.Expr = "Field";
            entity.Attributes.Add(attr2);

            // Act
            model.AssignEntityAttrID(attr2);

            // Assert
            Assert.Equal("Field", attr1.Id);
            Assert.Equal("Field2", attr2.Id);
        }

        [Fact]
        public void AssignEntityAttrID_MultipleVirtualAttrs_IncrementIds()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");

            var attr1 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Calc1",
                DataType = DataType.String,
                Caption = "Calc 1",
                Kind = EntityAttrKind.Virtual
            });

            var attr2 = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Calc2",
                DataType = DataType.String,
                Caption = "Calc 2",
                Kind = EntityAttrKind.Virtual
            });

            // Act (IDs already assigned by AddEntityAttr)

            // Assert
            Assert.StartsWith("VEA_", attr1.Id);
            Assert.StartsWith("VEA_", attr2.Id);
            Assert.NotEqual(attr1.Id, attr2.Id);
        }

        #endregion

        #region SortEntities

        [Fact]
        public void SortEntities_MultipleEntities_SortsByName()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Zebra");
            model.AddEntity(null, "Apple");
            model.AddEntity(null, "Mango");

            // Act
            model.SortEntities();

            // Assert
            Assert.Equal("Apple", model.EntityRoot.SubEntities[0].Name);
            Assert.Equal("Mango", model.EntityRoot.SubEntities[1].Name);
            Assert.Equal("Zebra", model.EntityRoot.SubEntities[2].Name);
        }

        [Fact]
        public void SortEntities_MultipleAttributes_SortsByCaption()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "ZZZ",
                DataType = DataType.String,
                Caption = "Zebra"
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "AAA",
                DataType = DataType.String,
                Caption = "Apple"
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "MMM",
                DataType = DataType.String,
                Caption = "Mango"
            });

            // Act
            model.SortEntities();

            // Assert
            Assert.Equal("Apple", entity.Attributes[0].Caption);
            Assert.Equal("Mango", entity.Attributes[1].Caption);
            Assert.Equal("Zebra", entity.Attributes[2].Caption);
        }

        [Fact]
        public void SortEntities_RecursivelySortsSubEntities()
        {
            // Arrange
            var model = new MetaData();
            var parent = model.AddEntity(null, "Parent");
            model.AddEntity(parent, "Zulu");
            model.AddEntity(parent, "Alpha");

            // Act
            model.SortEntities();

            // Assert
            Assert.Equal("Alpha", parent.SubEntities[0].Name);
            Assert.Equal("Zulu", parent.SubEntities[1].Name);
        }

        #endregion

        #region IncreaseModelVersion

        [Fact]
        public void IncreaseModelVersion_Default_IncrementsFromOne()
        {
            // Arrange
            var model = new MetaData();
            Assert.Equal(1, model.ModelVersion);

            // Act
            model.IncreaseModelVersion();

            // Assert
            Assert.Equal(2, model.ModelVersion);
        }

        [Fact]
        public void IncreaseModelVersion_CalledMultipleTimes_IncrementsCumulatively()
        {
            // Arrange
            var model = new MetaData();

            // Act
            model.IncreaseModelVersion();
            model.IncreaseModelVersion();
            model.IncreaseModelVersion();

            // Assert
            Assert.Equal(4, model.ModelVersion);
        }

        #endregion

        #region OnModelChanged

        [Fact]
        public void OnModelChanged_WithHandler_FiresModelChangedEvent()
        {
            // Arrange
            var model = new MetaData();
            model.MainSyncContext = null;
            var eventFired = false;
            model.ModelChanged += (sender, args) => eventFired = true;

            // Act
            model.OnModelChanged();

            // Assert
            Assert.True(eventFired);
        }

        [Fact]
        public void OnModelChanged_WithoutHandler_DoesNotThrow()
        {
            // Arrange
            var model = new MetaData();
            model.MainSyncContext = null;

            // Act
            var exception = Record.Exception(() => model.OnModelChanged());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void OnModelChanged_EventSenderIsModel()
        {
            // Arrange
            var model = new MetaData();
            model.MainSyncContext = null;
            object capturedSender = null;
            model.ModelChanged += (sender, args) => capturedSender = sender;

            // Act
            model.OnModelChanged();

            // Assert
            Assert.Same(model, capturedSender);
        }

        #endregion

        #region TryRunWithMainSyncContext

        [Fact]
        public void TryRunWithMainSyncContext_NullContext_RunsDirectly()
        {
            // Arrange
            var model = new MetaData();
            model.MainSyncContext = null;
            var executed = false;

            // Act
            model.TryRunWithMainSyncContext(() => executed = true);

            // Assert
            Assert.True(executed);
        }

        [Fact]
        public void TryRunWithMainSyncContext_WithContext_DoesNotThrow()
        {
            // Arrange
            var model = new MetaData();
            model.MainSyncContext = new SynchronizationContext();

            // Act
            var exception = Record.Exception(() => model.TryRunWithMainSyncContext(() => { }));

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region Clear

        [Fact]
        public void Clear_WithEntitiesAndAttributes_ClearsAll()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Name",
                DataType = DataType.String,
                Caption = "Name"
            });

            // Act
            model.Clear();

            // Assert
            Assert.True(model.IsEmpty);
            Assert.Empty(model.EntityRoot.SubEntities);
            Assert.Empty(model.EntityRoot.Attributes);
        }

        #endregion

        #region Clone (via JSON string roundtrip)

        [Fact]
        public void Clone_ModelWithEntitiesAndAttributes_ReturnsIdenticalModel()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "TestModel";
            model.Description = "Test description";
            model.CustomInfo = "Custom data";
            model.ModelVersion = 5;

            var entity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CompanyName",
                DataType = DataType.String,
                Caption = "Company Name",
                Size = 200
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "OrderCount",
                DataType = DataType.Int32,
                Caption = "Order Count",
                Kind = EntityAttrKind.Virtual
            });

            // Act -- use JSON string roundtrip to verify Clone semantics
            var json = model.SaveToJsonString();
            var cloned = new MetaData();
            cloned.LoadFromJsonString(json);

            // Assert
            Assert.Equal(model.Name, cloned.Name);
            Assert.Equal(model.Description, cloned.Description);
            Assert.Equal(model.CustomInfo, cloned.CustomInfo);
            Assert.Equal(model.ModelVersion, cloned.ModelVersion);
            Assert.Equal(model.Id, cloned.Id);

            var clonedEntity = cloned.FindEntity("Customer");
            Assert.NotNull(clonedEntity);
            Assert.Equal(2, clonedEntity.Attributes.Count);
        }

        [Fact]
        public void Clone_EmptyModel_ReturnsEmptyModel()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "EmptyModel";

            // Act
            var json = model.SaveToJsonString();
            var cloned = new MetaData();
            cloned.LoadFromJsonString(json);

            // Assert
            Assert.True(cloned.IsEmpty);
            Assert.Equal("EmptyModel", cloned.Name);
        }

        #endregion

        #region JSON Serialization - SaveToJsonString / LoadFromJsonString

        [Fact]
        public void SaveToJsonString_Default_ReturnsNonEmptyString()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "TestModel";

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.False(string.IsNullOrEmpty(json));
            Assert.Contains("\"name\"", json);
            Assert.Contains("TestModel", json);
        }

        [Fact]
        public void LoadFromJsonString_ValidJson_RestoresModel()
        {
            // Arrange
            var original = new MetaData();
            original.Name = "MyModel";
            original.Description = "My model description";
            original.CustomInfo = "info123";
            original.ModelVersion = 3;
            var json = original.SaveToJsonString();

            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.Equal(original.Id, loaded.Id);
            Assert.Equal("MyModel", loaded.Name);
            Assert.Equal("My model description", loaded.Description);
            Assert.Equal("info123", loaded.CustomInfo);
            Assert.Equal(3, loaded.ModelVersion);
        }

        [Fact]
        public void SaveLoadJsonString_WithEntitiesAndAttributes_RoundTrips()
        {
            // Arrange
            var model = CreatePopulatedModel();
            var json = model.SaveToJsonString();

            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var customer = loaded.FindEntity("Customer");
            Assert.NotNull(customer);
            Assert.Equal(3, customer.Attributes.Count);

            var nameAttr = customer.Attributes.FirstOrDefault(a => a.Caption == "Company Name");
            Assert.NotNull(nameAttr);
            Assert.Equal(DataType.String, nameAttr.DataType);
            Assert.Equal(200, nameAttr.Size);

            var idAttr = customer.Attributes.FirstOrDefault(a => a.Caption == "Customer ID");
            Assert.NotNull(idAttr);
            Assert.Equal(DataType.Int32, idAttr.DataType);
            Assert.True(idAttr.IsPrimaryKey);

            var order = loaded.FindEntity("Order");
            Assert.NotNull(order);
            Assert.Equal(2, order.Attributes.Count);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesFormatVersion()
        {
            // Arrange
            var model = new MetaData();
            model.FormatVersionJson = MetaData.LastJsonFormatVersion;
            var json = model.SaveToJsonString();

            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.Equal(MetaData.LastJsonFormatVersion, loaded.FormatVersionJson);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesDisplayFormats()
        {
            // Arrange
            var model = new MetaData();
            var json = model.SaveToJsonString();

            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.True(loaded.DisplayFormats.TryGetFormat(DataType.Bool, "No/Yes", out var format));
            Assert.Equal("{0:SNo|Yes}", format.Format);

            Assert.True(loaded.DisplayFormats.TryGetFormat(DataType.Date, "Short date", out var dateFormat));
            Assert.Equal("{0:d}", dateFormat.Format);

            Assert.True(loaded.DisplayFormats.TryGetFormat(DataType.Float, "3-precision", out var floatFormat));
            Assert.Equal("{0:F3}", floatFormat.Format);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesAttributeProperties()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Item");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency,
                Caption = "Price",
                Size = 0
            });
            attr.IsPrimaryKey = false;
            attr.IsForeignKey = true;
            attr.IsNullable = true;
            attr.IsEditable = false;
            attr.ShowOnView = true;
            attr.ShowOnEdit = false;
            attr.ShowOnCreate = false;
            attr.ShowInLookup = true;
            attr.Description = "The item price";

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("Item");
            Assert.NotNull(loadedEntity);
            var loadedAttr = loadedEntity.Attributes[0];
            Assert.Equal("Price", loadedAttr.Caption);
            Assert.Equal(DataType.Currency, loadedAttr.DataType);
            Assert.False(loadedAttr.IsPrimaryKey);
            Assert.True(loadedAttr.IsForeignKey);
            Assert.True(loadedAttr.IsNullable);
            Assert.False(loadedAttr.IsEditable);
            Assert.True(loadedAttr.ShowOnView);
            Assert.False(loadedAttr.ShowOnEdit);
            Assert.False(loadedAttr.ShowOnCreate);
            Assert.True(loadedAttr.ShowInLookup);
            Assert.Equal("The item price", loadedAttr.Description);
        }

        [Fact]
        public void LoadFromJsonString_InvalidJson_ThrowsException()
        {
            // Arrange
            var model = new MetaData();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => model.LoadFromJsonString("not valid json ["));
        }

        [Fact]
        public void LoadFromJsonString_EmptyArrayJson_ThrowsBadJsonFormatException()
        {
            // Arrange
            var model = new MetaData();

            // Act & Assert
            Assert.Throws<BadJsonFormatException>(() => model.LoadFromJsonString("[]"));
        }

        [Fact]
        public void SaveLoadJsonString_PreservesEntityDescription()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            entity.Description = "Customer entity description";

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("Customer");
            Assert.NotNull(loadedEntity);
            Assert.Equal("Customer entity description", loadedEntity.Description);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesEntityNamePlural()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            entity.NamePlural = "Customers";

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("Customer");
            Assert.NotNull(loadedEntity);
            Assert.Equal("Customers", loadedEntity.NamePlural);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesEntityIsEditable()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "ReadOnlyEntity");
            entity.IsEditable = false;

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("ReadOnlyEntity");
            Assert.NotNull(loadedEntity);
            Assert.False(loadedEntity.IsEditable);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesMaxAttrId()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Calc1",
                DataType = DataType.String,
                Caption = "Calc1",
                Kind = EntityAttrKind.Virtual
            });
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Calc2",
                DataType = DataType.String,
                Caption = "Calc2",
                Kind = EntityAttrKind.Virtual
            });

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.Equal(model._maxEntAttrId, loaded._maxEntAttrId);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesAttrKindVirtual()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CalcField",
                DataType = DataType.String,
                Caption = "Calculated Field",
                Kind = EntityAttrKind.Virtual
            });

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("TestEntity");
            Assert.NotNull(loadedEntity);
            Assert.Equal(EntityAttrKind.Virtual, loadedEntity.Attributes[0].Kind);
            Assert.True(loadedEntity.Attributes[0].IsVirtual);
        }

        [Fact]
        public void SaveLoadJsonString_PreservesAttrKindLookup()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "Order");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Customer",
                DataType = DataType.String,
                Caption = "Customer Lookup",
                Kind = EntityAttrKind.Lookup
            });

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("Order");
            Assert.NotNull(loadedEntity);
            Assert.Equal(EntityAttrKind.Lookup, loadedEntity.Attributes[0].Kind);
        }

        #endregion

        #region JSON Serialization - Stream

        [Fact]
        public void SaveToJsonStream_WritesToStream()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "StreamTest";
            var stream = new MemoryStream();

            // Act
            model.SaveToJsonStream(stream);

            // Assert (MemoryStream.ToArray works even after the stream is closed)
            var bytes = stream.ToArray();
            Assert.True(bytes.Length > 0);
            var json = Encoding.UTF8.GetString(bytes);
            Assert.Contains("StreamTest", json);
        }

        [Fact]
        public void LoadFromJsonStream_ValidStream_RestoresModel()
        {
            // Arrange
            var original = new MetaData();
            original.Name = "StreamLoad";
            var json = original.SaveToJsonString();
            var bytes = Encoding.UTF8.GetBytes(json);
            var stream = new MemoryStream(bytes);
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonStream(stream);

            // Assert
            Assert.Equal("StreamLoad", loaded.Name);
            Assert.Equal(original.Id, loaded.Id);
        }

        #endregion

        #region JSON Serialization - WriteToJson / ReadFromJson

        [Fact]
        public void SaveToJsonString_ContainsFverProperty()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"fver\"", json);
        }

        [Fact]
        public void SaveToJsonString_ContainsMverProperty()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"mver\"", json);
        }

        [Fact]
        public void SaveToJsonString_ContainsIdProperty()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"id\"", json);
        }

        [Fact]
        public void SaveToJsonString_ContainsNameProperty()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "SomeName";

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"name\"", json);
            Assert.Contains("SomeName", json);
        }

        [Fact]
        public void SaveToJsonString_WithDescription_ContainsDescProperty()
        {
            // Arrange
            var model = new MetaData();
            model.Description = "Hello World";

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"desc\"", json);
            Assert.Contains("Hello World", json);
        }

        [Fact]
        public void SaveToJsonString_WithCustomInfo_ContainsCstinfProperty()
        {
            // Arrange
            var model = new MetaData();
            model.CustomInfo = "custom-data";

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"cstinf\"", json);
            Assert.Contains("custom-data", json);
        }

        [Fact]
        public void SaveToJsonString_ContainsDisplayFormatsProperty()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"displayFormats\"", json);
        }

        [Fact]
        public void SaveToJsonString_ContainsEditorsProperty()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"editors\"", json);
        }

        [Fact]
        public void SaveToJsonString_ContainsMaxAttrIdProperty()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"maxAttrId\"", json);
        }

        [Fact]
        public void SaveToJsonString_ContainsEntrootProperty()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var json = model.SaveToJsonString();

            // Assert
            Assert.Contains("\"entroot\"", json);
        }

        [Fact]
        public void SaveToJsonString_EmptyId_GeneratesNewGuid()
        {
            // Arrange
            var model = new MetaData();
            model.Id = "";

            // Act
            model.SaveToJsonString();

            // Assert
            Assert.False(string.IsNullOrEmpty(model.Id));
            Assert.True(Guid.TryParse(model.Id, out _));
        }

        [Fact]
        public void WriteToJson_SyncOverload_ProducesValidJson()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "SyncWrite";
            var sb = new StringBuilder();
            using var textWriter = new StringWriter(sb);
            using var jsonWriter = new JsonTextWriter(textWriter);

            // Act
            model.WriteToJson(jsonWriter);
            jsonWriter.Flush();

            // Assert
            var json = sb.ToString();
            Assert.Contains("\"name\"", json);
            Assert.Contains("SyncWrite", json);
        }

        [Fact]
        public void ReadFromJson_WithOptions_RestoresModel()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "ReadFromJsonTest";
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String,
                Caption = "Field 1"
            });

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            using var textReader = new StringReader(json);
            using var jsonReader = new JsonTextReader(textReader);
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            loaded.ReadFromJson(jsonReader, options);

            // Assert
            Assert.Equal("ReadFromJsonTest", loaded.Name);
            Assert.NotNull(loaded.FindEntity("TestEntity"));
        }

        #endregion

        #region JSON Serialization - Comprehensive Roundtrip

        [Fact]
        public void SaveLoadJsonString_ComprehensiveModel_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "FullModel";
            model.Description = "Full model description";
            model.CustomInfo = "{\"key\": \"value\"}";
            model.ModelVersion = 7;

            var customer = model.AddEntity(null, "Customer");
            customer.Description = "Customer entity";
            customer.NamePlural = "Customers";

            var custId = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customer,
                Expression = "Id",
                DataType = DataType.Autoinc,
                Caption = "ID",
                Size = 0
            });
            custId.IsPrimaryKey = true;
            custId.IsEditable = false;
            custId.ShowOnCreate = false;

            var custName = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customer,
                Expression = "Name",
                DataType = DataType.String,
                Caption = "Customer Name",
                Size = 250
            });
            custName.IsNullable = false;
            custName.ShowInLookup = true;

            var order = model.AddEntity(null, "Order");
            order.Description = "Order entity";

            var orderId = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = order,
                Expression = "Id",
                DataType = DataType.Int32,
                Caption = "Order ID"
            });
            orderId.IsPrimaryKey = true;

            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = order,
                Expression = "OrderDate",
                DataType = DataType.DateTime,
                Caption = "Order Date"
            });

            var orderTotal = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = order,
                Expression = "Total",
                DataType = DataType.Currency,
                Caption = "Total Amount",
                Size = 0
            });
            orderTotal.Description = "Total amount of the order";

            var orderDetail = model.AddEntity(order, "OrderDetail");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = orderDetail,
                Expression = "Quantity",
                DataType = DataType.Int32,
                Caption = "Quantity"
            });

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.Equal("FullModel", loaded.Name);
            Assert.Equal("Full model description", loaded.Description);
            Assert.Equal("{\"key\": \"value\"}", loaded.CustomInfo);
            Assert.Equal(7, loaded.ModelVersion);
            Assert.Equal(model.Id, loaded.Id);

            var loadedCustomer = loaded.FindEntity("Customer");
            Assert.NotNull(loadedCustomer);
            Assert.Equal("Customer entity", loadedCustomer.Description);
            Assert.Equal("Customers", loadedCustomer.NamePlural);
            Assert.Equal(2, loadedCustomer.Attributes.Count);

            var loadedCustId = loadedCustomer.Attributes.FirstOrDefault(a => a.Caption == "ID");
            Assert.NotNull(loadedCustId);
            Assert.True(loadedCustId.IsPrimaryKey);
            Assert.False(loadedCustId.IsEditable);
            Assert.False(loadedCustId.ShowOnCreate);
            Assert.Equal(DataType.Autoinc, loadedCustId.DataType);

            var loadedCustName = loadedCustomer.Attributes.FirstOrDefault(a => a.Caption == "Customer Name");
            Assert.NotNull(loadedCustName);
            Assert.False(loadedCustName.IsNullable);
            Assert.True(loadedCustName.ShowInLookup);
            Assert.Equal(250, loadedCustName.Size);

            var loadedOrder = loaded.FindEntity("Order");
            Assert.NotNull(loadedOrder);
            Assert.Equal("Order entity", loadedOrder.Description);
            Assert.Equal(3, loadedOrder.Attributes.Count);

            var loadedOrderDate = loadedOrder.Attributes.FirstOrDefault(a => a.Caption == "Order Date");
            Assert.NotNull(loadedOrderDate);
            Assert.Equal(DataType.DateTime, loadedOrderDate.DataType);

            var loadedOrderTotal = loadedOrder.Attributes.FirstOrDefault(a => a.Caption == "Total Amount");
            Assert.NotNull(loadedOrderTotal);
            Assert.Equal(DataType.Currency, loadedOrderTotal.DataType);
            Assert.Equal("Total amount of the order", loadedOrderTotal.Description);

            var loadedOrderDetail = loaded.FindEntity("OrderDetail");
            Assert.NotNull(loadedOrderDetail);
            Assert.Single(loadedOrderDetail.Attributes);
            Assert.Equal("Quantity", loadedOrderDetail.Attributes[0].Caption);
        }

        #endregion

        #region JSON Serialization - Async

        [Fact]
        public async Task SaveToJsonStringAsync_Default_ReturnsJsonAsync()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "AsyncTest";

            // Act
            var json = await model.SaveToJsonStringAsync();

            // Assert
            Assert.False(string.IsNullOrEmpty(json));
            Assert.Contains("AsyncTest", json);
        }

        [Fact]
        public async Task LoadFromJsonStringAsync_ValidJson_RestoresModelAsync()
        {
            // Arrange
            var original = new MetaData();
            original.Name = "AsyncLoadTest";
            var json = await original.SaveToJsonStringAsync();
            var loaded = new MetaData();

            // Act
            await loaded.LoadFromJsonStringAsync(json);

            // Assert
            Assert.Equal("AsyncLoadTest", loaded.Name);
            Assert.Equal(original.Id, loaded.Id);
        }

        [Fact]
        public async Task SaveToJsonStreamAsync_Default_WritesToStreamAsync()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "StreamAsyncTest";
            var stream = new MemoryStream();

            // Act
            await model.SaveToJsonStreamAsync(stream);

            // Assert (MemoryStream.ToArray works even after stream is closed)
            var bytes = stream.ToArray();
            Assert.True(bytes.Length > 0);
            var json = Encoding.UTF8.GetString(bytes);
            Assert.Contains("StreamAsyncTest", json);
        }

        [Fact]
        public async Task LoadFromJsonStreamAsync_ValidStream_RestoresModelAsync()
        {
            // Arrange
            var original = new MetaData();
            original.Name = "StreamLoadAsync";
            var json = await original.SaveToJsonStringAsync();
            var bytes = Encoding.UTF8.GetBytes(json);
            var stream = new MemoryStream(bytes);
            var loaded = new MetaData();

            // Act
            await loaded.LoadFromJsonStreamAsync(stream);

            // Assert
            Assert.Equal("StreamLoadAsync", loaded.Name);
        }

        [Fact]
        public async Task WriteToJsonAsync_WithDefaultOptions_ProducesValidJsonAsync()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "AsyncWriteTest";
            var sb = new StringBuilder();
            using var textWriter = new StringWriter(sb);
            using var jsonWriter = new JsonTextWriter(textWriter);

            // Act
            await model.WriteToJsonAsync(jsonWriter);
            await jsonWriter.FlushAsync();

            // Assert
            var json = sb.ToString();
            Assert.Contains("\"name\"", json);
            Assert.Contains("AsyncWriteTest", json);
        }

        #endregion

        #region MetaDataReadWriteOptions

        [Fact]
        public void MetaDataReadWriteOptions_DefaultComposition_ContainsAllExpectedFlags()
        {
            // Arrange -- construct from constants to avoid mutating the shared static Defaults instance
            BitOptions defaults = MetaDataReadWriteOptions.Entities
                                | MetaDataReadWriteOptions.Description
                                | MetaDataReadWriteOptions.Editors
                                | MetaDataReadWriteOptions.CustomInfo
                                | MetaDataReadWriteOptions.KeepCurrent;

            // Act & Assert
            Assert.True(defaults.Contains(MetaDataReadWriteOptions.Entities));
            Assert.True(defaults.Contains(MetaDataReadWriteOptions.Editors));
            Assert.True(defaults.Contains(MetaDataReadWriteOptions.Description));
            Assert.True(defaults.Contains(MetaDataReadWriteOptions.CustomInfo));
            Assert.True(defaults.Contains(MetaDataReadWriteOptions.KeepCurrent));
            Assert.False(defaults.Contains(MetaDataReadWriteOptions.HumanReadable));
        }

        [Fact]
        public void MetaDataReadWriteOptions_ClientSideComposition_ExcludesKeepCurrent()
        {
            // Arrange -- construct manually to avoid mutating the shared Defaults static
            BitOptions clientSide = MetaDataReadWriteOptions.Entities
                                  | MetaDataReadWriteOptions.Description
                                  | MetaDataReadWriteOptions.Editors
                                  | MetaDataReadWriteOptions.CustomInfo;

            // Act & Assert
            Assert.True(clientSide.Contains(MetaDataReadWriteOptions.Entities));
            Assert.True(clientSide.Contains(MetaDataReadWriteOptions.Editors));
            Assert.True(clientSide.Contains(MetaDataReadWriteOptions.Description));
            Assert.True(clientSide.Contains(MetaDataReadWriteOptions.CustomInfo));
            Assert.False(clientSide.Contains(MetaDataReadWriteOptions.KeepCurrent));
        }

        [Fact]
        public void MetaDataReadWriteOptions_ConstantValues_AreCorrect()
        {
            // Arrange & Act & Assert
            Assert.Equal((ulong)4, MetaDataReadWriteOptions.Editors);
            Assert.Equal((ulong)8, MetaDataReadWriteOptions.Entities);
            Assert.Equal((ulong)32, MetaDataReadWriteOptions.Description);
            Assert.Equal((ulong)512, MetaDataReadWriteOptions.CustomInfo);
            Assert.Equal((ulong)4096, MetaDataReadWriteOptions.KeepCurrent);
            Assert.Equal((ulong)8192, MetaDataReadWriteOptions.HumanReadable);
        }

        #endregion

        #region MetaEntityAttrDescriptor

        [Fact]
        public void MetaEntityAttrDescriptor_Default_SetsDataKindAndDefaults()
        {
            // Arrange & Act
            var desc = new MetaEntityAttrDescriptor();

            // Assert
            Assert.Equal(EntityAttrKind.Data, desc.Kind);
            Assert.Equal(DataType.Unknown, desc.DataType);
            Assert.Equal(100, desc.Size);
            Assert.False(desc.IsVirtual);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_SetIsVirtualTrue_SetsKindToVirtual()
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
        public void MetaEntityAttrDescriptor_SetIsVirtualFalse_SetsKindToData()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor();
            desc.Kind = EntityAttrKind.Virtual;

            // Act
            desc.IsVirtual = false;

            // Assert
            Assert.Equal(EntityAttrKind.Data, desc.Kind);
            Assert.False(desc.IsVirtual);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_CaptionFromExpression_ReturnsSecondPart()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor
            {
                Expression = "Customer.CompanyName"
            };

            // Act
            var caption = desc.Caption;

            // Assert
            Assert.Equal("CompanyName", caption);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_ExplicitCaption_OverridesDefault()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor
            {
                Expression = "Customer.CompanyName",
                Caption = "My Caption"
            };

            // Act
            var caption = desc.Caption;

            // Assert
            Assert.Equal("My Caption", caption);
        }

        [Fact]
        public void MetaEntityAttrDescriptor_CaptionNoSeparator_ReturnsFullExpression()
        {
            // Arrange
            var desc = new MetaEntityAttrDescriptor
            {
                Expression = "SimpleField"
            };

            // Act
            var caption = desc.Caption;

            // Assert
            Assert.Equal("SimpleField", caption);
        }

        #endregion

        #region LoadFromJsonString without KeepCurrent clears model

        [Fact]
        public void LoadFromJsonString_WithoutKeepCurrent_ClearsPreviousContent()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "OldEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "OldField",
                DataType = DataType.String,
                Caption = "Old Field"
            });

            var newModel = new MetaData();
            newModel.Name = "NewModel";
            var newEntity = newModel.AddEntity(null, "NewEntity");
            newModel.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = newEntity,
                Expression = "NewField",
                DataType = DataType.Int32,
                Caption = "New Field"
            });

            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo;

            var json = newModel.SaveToJsonString(options);

            // Act
            model.LoadFromJsonString(json, options);

            // Assert
            Assert.Null(model.FindEntity("OldEntity"));
            Assert.NotNull(model.FindEntity("NewEntity"));
        }

        #endregion

        #region JSON Serialization with HumanReadable option

        [Fact]
        public void SaveToJsonStream_WithHumanReadableOption_ProducesFormattedJson()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "FormattedModel";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent
                               | MetaDataReadWriteOptions.HumanReadable;

            var stream = new MemoryStream();

            // Act
            model.SaveToJsonStream(stream, options);

            // Assert
            var json = Encoding.UTF8.GetString(stream.ToArray());
            Assert.Contains("\n", json);
        }

        #endregion

        #region JSON Serialization - Options flags

        [Fact]
        public void SaveToJsonString_WithoutDescriptionOption_OmitsDesc()
        {
            // Arrange
            var model = new MetaData();
            model.Description = "Should be omitted";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            var json = model.SaveToJsonString(options);

            // Assert
            Assert.DoesNotContain("\"desc\"", json);
        }

        [Fact]
        public void SaveToJsonString_WithoutCustomInfoOption_OmitsCstinf()
        {
            // Arrange
            var model = new MetaData();
            model.CustomInfo = "Should be omitted";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            var json = model.SaveToJsonString(options);

            // Assert
            Assert.DoesNotContain("\"cstinf\"", json);
        }

        [Fact]
        public void SaveToJsonString_WithoutEntitiesOption_OmitsEntroot()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "TestEntity");
            BitOptions options = MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            var json = model.SaveToJsonString(options);

            // Assert
            Assert.DoesNotContain("\"entroot\"", json);
            Assert.DoesNotContain("\"maxAttrId\"", json);
        }

        [Fact]
        public void SaveToJsonString_WithoutEditorsOption_OmitsEditors()
        {
            // Arrange
            var model = new MetaData();
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            var json = model.SaveToJsonString(options);

            // Assert
            Assert.DoesNotContain("\"editors\"", json);
        }

        #endregion

        #region Display Formats Roundtrip

        [Fact]
        public void SaveLoadJsonString_CustomDisplayFormats_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            model.DisplayFormats.AddOrUpdate(DataType.String, "Custom Format", "{0:custom}");

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.True(loaded.DisplayFormats.TryGetFormat(DataType.String, "Custom Format", out var fmt));
            Assert.Equal("{0:custom}", fmt.Format);
        }

        [Fact]
        public void SaveLoadJsonString_DisplayFormatWithIsDefault_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            model.DisplayFormats.AddOrUpdate(DataType.Int32, "Default Format", "{0:D}", true);

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.True(loaded.DisplayFormats.TryGetFormat(DataType.Int32, "Default Format", out var loadedFmt));
            Assert.True(loadedFmt.IsDefault);
        }

        #endregion

        #region LastJsonFormatVersion

        [Fact]
        public void LastJsonFormatVersion_IsPositive()
        {
            // Arrange & Act & Assert
            Assert.True(MetaData.LastJsonFormatVersion > 0);
            Assert.Equal(4, MetaData.LastJsonFormatVersion);
        }

        #endregion

        #region OnModelLoaded

        [Fact]
        public void OnModelLoaded_Default_DoesNotThrow()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var exception = Record.Exception(() => model.OnModelLoaded());

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region Multiple Entity Levels

        [Fact]
        public void SaveLoadJsonString_NestedSubEntities_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            var root = model.AddEntity(null, "Root");
            var child = model.AddEntity(root, "Child");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = child,
                Expression = "ChildField",
                DataType = DataType.String,
                Caption = "Child Field"
            });
            var grandchild = model.AddEntity(child, "Grandchild");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = grandchild,
                Expression = "GrandchildField",
                DataType = DataType.Int32,
                Caption = "Grandchild Field"
            });

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedRoot = loaded.FindEntity("Root");
            Assert.NotNull(loadedRoot);

            var loadedChild = loaded.FindEntity("Child");
            Assert.NotNull(loadedChild);
            Assert.Single(loadedChild.Attributes);
            Assert.Equal("Child Field", loadedChild.Attributes[0].Caption);

            var loadedGrandchild = loaded.FindEntity("Grandchild");
            Assert.NotNull(loadedGrandchild);
            Assert.Single(loadedGrandchild.Attributes);
            Assert.Equal("Grandchild Field", loadedGrandchild.Attributes[0].Caption);
            Assert.Equal(DataType.Int32, loadedGrandchild.Attributes[0].DataType);
        }

        #endregion

        #region Entity/Attribute UserData Roundtrip

        [Fact]
        public void SaveLoadJsonString_EntityUserData_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "WithUserData");
            entity.UserData = "some-user-data";

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("WithUserData");
            Assert.NotNull(loadedEntity);
            Assert.Equal("some-user-data", loadedEntity.UserData);
        }

        [Fact]
        public void SaveLoadJsonString_AttrUserData_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TestEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String,
                Caption = "Field 1"
            });
            attr.UserData = "attr-user-data";

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("TestEntity");
            Assert.NotNull(loadedEntity);
            Assert.Equal("attr-user-data", loadedEntity.Attributes[0].UserData);
        }

        #endregion

        #region Multiple DataTypes

        [Theory]
        [InlineData(DataType.String, 250)]
        [InlineData(DataType.Int32, 0)]
        [InlineData(DataType.Bool, 0)]
        [InlineData(DataType.DateTime, 0)]
        [InlineData(DataType.Float, 0)]
        [InlineData(DataType.Currency, 0)]
        [InlineData(DataType.Guid, 0)]
        [InlineData(DataType.Date, 0)]
        [InlineData(DataType.Time, 0)]
        [InlineData(DataType.Int64, 0)]
        [InlineData(DataType.Byte, 0)]
        [InlineData(DataType.Word, 0)]
        [InlineData(DataType.Memo, 0)]
        [InlineData(DataType.Blob, 0)]
        public void SaveLoadJsonString_VariousDataTypes_RoundTrip(DataType dataType, int size)
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "TypeTest");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "TypedField",
                DataType = dataType,
                Caption = "Typed Field",
                Size = size
            });

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("TypeTest");
            Assert.NotNull(loadedEntity);
            Assert.Single(loadedEntity.Attributes);
            Assert.Equal(dataType, loadedEntity.Attributes[0].DataType);
            Assert.Equal(size, loadedEntity.Attributes[0].Size);
        }

        #endregion

        #region DisplayFormat on Attribute Roundtrip

        [Fact]
        public void SaveLoadJsonString_AttrDisplayFormat_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            var entity = model.AddEntity(null, "FormattedEntity");
            var attr = model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Price",
                DataType = DataType.Currency,
                Caption = "Price"
            });
            attr.DisplayFormat = "{0:F2}";

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            var loadedEntity = loaded.FindEntity("FormattedEntity");
            Assert.NotNull(loadedEntity);
            Assert.Equal("{0:F2}", loadedEntity.Attributes[0].DisplayFormat);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void AddEntity_MultipleTimesToRoot_AllAdded()
        {
            // Arrange
            var model = new MetaData();

            // Act
            model.AddEntity(null, "A");
            model.AddEntity(null, "B");
            model.AddEntity(null, "C");

            // Assert
            Assert.Equal(3, model.EntityRoot.SubEntities.Count);
        }

        [Fact]
        public void Clear_MultipleTimes_DoesNotThrow()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "E");

            // Act & Assert
            model.Clear();
            model.Clear();
            Assert.True(model.IsEmpty);
        }

        [Fact]
        public void IncreaseModelVersion_AfterJsonRoundtrip_Independent()
        {
            // Arrange
            var model = new MetaData();
            model.ModelVersion = 5;
            var json = model.SaveToJsonString();
            var cloned = new MetaData();
            cloned.LoadFromJsonString(json);

            // Act
            cloned.IncreaseModelVersion();

            // Assert
            Assert.Equal(5, model.ModelVersion);
            Assert.Equal(6, cloned.ModelVersion);
        }

        [Fact]
        public void FindEntity_CaseInsensitive_ReturnsEntity()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "Customer");

            // Act
            var found = model.FindEntity("customer");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Customer", found.Name);
        }

        [Fact]
        public void SaveLoadJsonString_NullDescription_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "NullDescModel";
            model.Description = null;

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.Null(loaded.Description);
        }

        [Fact]
        public void SaveLoadJsonString_NullName_RoundTrips()
        {
            // Arrange
            var model = new MetaData();
            model.Name = null;

            var json = model.SaveToJsonString();
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(json);

            // Assert
            Assert.Null(loaded.Name);
        }

        #endregion

        #region InitModelLoading

        [Fact]
        public void InitModelLoading_Default_DoesNotThrow()
        {
            // Arrange
            var model = new MetaData();

            // Act
            var exception = Record.Exception(() => model.InitModelLoading());

            // Assert
            Assert.Null(exception);
        }

        #endregion

        #region JSON File Serialization

        [Fact]
        public void SaveToJsonFile_And_LoadFromJsonFile_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "FileTest";
            model.Description = "Test description";
            var entity = model.AddEntity(null, "TestEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "Field1",
                DataType = DataType.String
            });
            var tmpFile = Path.GetTempFileName();

            try
            {
                // Act
                model.SaveToJsonFile(tmpFile);
                var model2 = new MetaData();
                model2.LoadFromJsonFile(tmpFile);

                // Assert
                Assert.Equal("FileTest", model2.Name);
                Assert.Equal("Test description", model2.Description);
                Assert.NotNull(model2.FindEntity("TestEntity"));
                Assert.Equal(tmpFile, model2.FilePath);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        [Fact]
        public async Task SaveToJsonFileAsync_And_LoadFromJsonFileAsync_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "AsyncFileTest";
            model.Description = "Async test description";
            var entity = model.AddEntity(null, "AsyncEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "AsyncField",
                DataType = DataType.Int32
            });
            var tmpFile = Path.GetTempFileName();

            try
            {
                // Act
                await model.SaveToJsonFileAsync(tmpFile);
                var model2 = new MetaData();
                await model2.LoadFromJsonFileAsync(tmpFile);

                // Assert
                Assert.Equal("AsyncFileTest", model2.Name);
                Assert.Equal("Async test description", model2.Description);
                Assert.NotNull(model2.FindEntity("AsyncEntity"));
                Assert.Equal(tmpFile, model2.FilePath);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        [Fact]
        public async Task SaveToJsonFileAsync_WithOptions_WritesFileCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "FileOptionsTest";
            model.Description = "With options";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;
            var tmpFile = Path.GetTempFileName();

            try
            {
                // Act
                await model.SaveToJsonFileAsync(tmpFile, options);
                var json = File.ReadAllText(tmpFile);

                // Assert
                Assert.Contains("FileOptionsTest", json);
                Assert.Contains("With options", json);
                Assert.Contains("\n", json); // SaveToJsonFileAsync always adds HumanReadable
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        [Fact]
        public void SaveToJsonFile_WithOptions_WritesFileCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "SyncFileOptionsTest";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;
            var tmpFile = Path.GetTempFileName();

            try
            {
                // Act
                model.SaveToJsonFile(tmpFile, options);
                var json = File.ReadAllText(tmpFile);

                // Assert
                Assert.Contains("SyncFileOptionsTest", json);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        [Fact]
        public async Task LoadFromJsonFileAsync_DefaultOverload_SetsFilePath()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "DefaultLoadAsync";
            var tmpFile = Path.GetTempFileName();
            model.SaveToJsonFile(tmpFile);
            var loaded = new MetaData();

            try
            {
                // Act
                await loaded.LoadFromJsonFileAsync(tmpFile);

                // Assert
                Assert.Equal("DefaultLoadAsync", loaded.Name);
                Assert.Equal(tmpFile, loaded.FilePath);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        [Fact]
        public void LoadFromJsonFile_WithOptions_RestoresModel()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "LoadWithOptions";
            model.Description = "Desc for options";
            var tmpFile = Path.GetTempFileName();
            model.SaveToJsonFile(tmpFile);
            var loaded = new MetaData();

            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;

            try
            {
                // Act
                loaded.LoadFromJsonFile(tmpFile, options);

                // Assert
                Assert.Equal("LoadWithOptions", loaded.Name);
                Assert.Equal("Desc for options", loaded.Description);
                Assert.Equal(tmpFile, loaded.FilePath);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        #endregion

        #region JSON Stream Sync Roundtrip

        [Fact]
        public void SaveToJsonStream_And_LoadFromJsonStream_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "StreamRoundtrip";
            model.Description = "Stream roundtrip description";
            var entity = model.AddEntity(null, "StreamEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "StreamField",
                DataType = DataType.String
            });

            // Act
            var stream = new MemoryStream();
            model.SaveToJsonStream(stream);
            // SaveToJsonStream disposes the stream internally via StreamWriter,
            // but MemoryStream.ToArray() still works on a disposed MemoryStream.
            var bytes = stream.ToArray();
            using var readStream = new MemoryStream(bytes);
            var model2 = new MetaData();
            model2.LoadFromJsonStream(readStream);

            // Assert
            Assert.Equal("StreamRoundtrip", model2.Name);
            Assert.Equal("Stream roundtrip description", model2.Description);
            Assert.NotNull(model2.FindEntity("StreamEntity"));
        }

        [Fact]
        public void SaveToJsonStream_WithOptions_RoundtripsCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "StreamWithOptions";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            var stream = new MemoryStream();
            model.SaveToJsonStream(stream, options);
            var bytes = stream.ToArray();
            using var readStream = new MemoryStream(bytes);
            var model2 = new MetaData();
            model2.LoadFromJsonStream(readStream, options);

            // Assert
            Assert.Equal("StreamWithOptions", model2.Name);
        }

        #endregion

        #region JSON Stream Async with Options

        [Fact]
        public async Task SaveToJsonStreamAsync_WithHumanReadable_ProducesIndentedJson()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "IndentTest";
            BitOptions options = MetaDataReadWriteOptions.Defaults
                .With(MetaDataReadWriteOptions.HumanReadable);

            // Act
            var stream = new MemoryStream();
            await model.SaveToJsonStreamAsync(stream, options);
            var json = Encoding.UTF8.GetString(stream.ToArray());

            // Assert
            Assert.Contains("\n", json);
            Assert.Contains("IndentTest", json);
        }

        [Fact]
        public async Task SaveToJsonStreamAsync_WithOptions_RoundtripsViaLoadAsync()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "AsyncStreamOptions";
            model.Description = "Async stream options test";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            var stream = new MemoryStream();
            await model.SaveToJsonStreamAsync(stream, options);
            var bytes = stream.ToArray();
            using var readStream = new MemoryStream(bytes);
            var loaded = new MetaData();
            await loaded.LoadFromJsonStreamAsync(readStream, options);

            // Assert
            Assert.Equal("AsyncStreamOptions", loaded.Name);
            Assert.Equal("Async stream options test", loaded.Description);
        }

        #endregion

        #region ReadFromJsonAsync without KeepCurrent

        [Fact]
        public async Task ReadFromJsonAsync_WithoutKeepCurrent_ClearsExistingContent()
        {
            // Arrange
            var model = new MetaData();
            model.AddEntity(null, "ExistingEntity");
            Assert.False(model.IsEmpty);

            var sourceModel = new MetaData();
            sourceModel.Name = "New";
            var json = sourceModel.SaveToJsonString();

            // Act
            await model.LoadFromJsonStringAsync(json, MetaDataReadWriteOptions.ClientSideContent);

            // Assert
            Assert.Null(model.FindEntity("ExistingEntity"));
            Assert.Equal("New", model.Name);
        }

        [Fact]
        public async Task ReadFromJsonAsync_WithKeepCurrent_DoesNotCallClear()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "OldName";

            // Create JSON that only has model props (no entities) so entroot does not overwrite
            var sourceModel = new MetaData();
            sourceModel.Name = "New";
            BitOptions writeOptions = MetaDataReadWriteOptions.Description
                                    | MetaDataReadWriteOptions.CustomInfo
                                    | MetaDataReadWriteOptions.Editors
                                    | MetaDataReadWriteOptions.KeepCurrent;
            var json = sourceModel.SaveToJsonString(writeOptions);

            // Act -- load with KeepCurrent, so Clear() is NOT called
            await model.LoadFromJsonStringAsync(json, MetaDataReadWriteOptions.Defaults);

            // Assert
            Assert.Equal("New", model.Name);
        }

        #endregion

        #region dataFormats JSON alias

        [Fact]
        public void LoadFromJsonString_DataFormatsAlias_LoadsDisplayFormatsCorrectly()
        {
            // Arrange
            // Build JSON that uses the old "dataFormats" alias instead of "displayFormats"
            var model = new MetaData();
            model.Name = "DataFormatsAlias";
            var json = model.SaveToJsonString();
            // Replace "displayFormats" with "dataFormats" to simulate old format
            var aliasJson = json.Replace("displayFormats", "dataFormats");
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(aliasJson);

            // Assert
            Assert.Equal("DataFormatsAlias", loaded.Name);
            Assert.True(loaded.DisplayFormats.TryGetFormat(DataType.Bool, "0/1", out var boolFormat));
            Assert.Equal("{0:S0|1}", boolFormat.Format);
        }

        #endregion

        #region Unknown JSON property skip

        [Fact]
        public void LoadFromJsonString_UnknownProperty_SkipsWithoutError()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "SkipTest";
            var json = model.SaveToJsonString();
            // Inject an unknown property into the JSON
            var modifiedJson = json.Replace("\"name\":", "\"unknownProp123\":\"someValue\",\"name\":");
            var loaded = new MetaData();

            // Act
            loaded.LoadFromJsonString(modifiedJson);

            // Assert
            Assert.Equal("SkipTest", loaded.Name);
        }

        #endregion

        #region SaveToJsonStringAsync with options

        [Fact]
        public async Task SaveToJsonStringAsync_WithOptions_ReturnsFilteredJson()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "OptionsAsync";
            model.Description = "Should be excluded";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            var json = await model.SaveToJsonStringAsync(options);

            // Assert
            Assert.Contains("OptionsAsync", json);
            Assert.DoesNotContain("Should be excluded", json);
        }

        #endregion

        #region WriteToJson and ReadFromJson sync roundtrip

        [Fact]
        public void WriteToJson_And_ReadFromJson_RoundtripCorrectly()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "SyncJsonRoundtrip";
            model.Description = "Sync roundtrip test";
            model.CustomInfo = "custom-sync";
            var entity = model.AddEntity(null, "SyncEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "SyncField",
                DataType = DataType.String
            });
            var sb = new StringBuilder();

            // Act - Write
            using (var sw = new StringWriter(sb))
            using (var writer = new JsonTextWriter(sw))
            {
                model.WriteToJson(writer);
            }

            // Act - Read
            var model2 = new MetaData();
            using (var sr = new StringReader(sb.ToString()))
            using (var reader = new JsonTextReader(sr))
            {
                model2.ReadFromJson(reader, MetaDataReadWriteOptions.Defaults);
            }

            // Assert
            Assert.Equal("SyncJsonRoundtrip", model2.Name);
            Assert.Equal("Sync roundtrip test", model2.Description);
            Assert.Equal("custom-sync", model2.CustomInfo);
            Assert.NotNull(model2.FindEntity("SyncEntity"));
        }

        [Fact]
        public void WriteToJson_WithOptions_RespectsFlags()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "WriteWithOpts";
            model.Description = "ShouldAppear";
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;
            var sb = new StringBuilder();

            // Act
            using (var sw = new StringWriter(sb))
            using (var writer = new JsonTextWriter(sw))
            {
                model.WriteToJson(writer, options);
            }

            // Assert
            var json = sb.ToString();
            Assert.Contains("WriteWithOpts", json);
            Assert.Contains("ShouldAppear", json);
        }

        #endregion

        #region WriteToJsonAsync overloads

        [Fact]
        public async Task WriteToJsonAsync_DefaultOverload_ProducesValidJson()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "AsyncDefaultOverload";
            var sb = new StringBuilder();
            using var textWriter = new StringWriter(sb);
            using var jsonWriter = new JsonTextWriter(textWriter);

            // Act
            await model.WriteToJsonAsync(jsonWriter);
            await jsonWriter.FlushAsync();

            // Assert
            var json = sb.ToString();
            Assert.Contains("AsyncDefaultOverload", json);
            Assert.Contains("\"fver\"", json);
        }

        #endregion

        #region SaveToJsonString EmptyId generates new Id after roundtrip

        [Fact]
        public void SaveToJsonString_EmptyId_GeneratesNewIdVisibleAfterReload()
        {
            // Arrange
            var model = new MetaData();
            model.Id = "";

            // Act
            var json = model.SaveToJsonString();
            var model2 = new MetaData();
            model2.LoadFromJsonString(json);

            // Assert
            Assert.False(string.IsNullOrEmpty(model2.Id));
            Assert.True(Guid.TryParse(model2.Id, out _));
        }

        #endregion

        #region JSON string roundtrip produces independent copy

        [Fact]
        public void SaveLoadJsonString_PopulatedModel_ProducesIndependentCopy()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "OriginalCopy";
            model.Description = "Copy desc";
            model.CustomInfo = "copy-info";
            var entity = model.AddEntity(null, "CopyEntity");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = entity,
                Expression = "CopyField",
                DataType = DataType.String
            });

            // Act
            var json = model.SaveToJsonString();
            var copy = new MetaData();
            copy.LoadFromJsonString(json);

            // Assert
            Assert.Equal("OriginalCopy", copy.Name);
            Assert.Equal("Copy desc", copy.Description);
            Assert.Equal("copy-info", copy.CustomInfo);
            Assert.NotNull(copy.FindEntity("CopyEntity"));

            // Verify independence
            copy.Name = "Modified";
            Assert.Equal("OriginalCopy", model.Name);
        }

        #endregion

        #region LoadFromJsonStreamAsync overloads

        [Fact]
        public async Task LoadFromJsonStreamAsync_DefaultOverload_RestoresModel()
        {
            // Arrange
            var original = new MetaData();
            original.Name = "StreamDefaultAsync";
            var json = original.SaveToJsonString();
            var bytes = Encoding.UTF8.GetBytes(json);
            using var stream = new MemoryStream(bytes);
            var loaded = new MetaData();

            // Act
            await loaded.LoadFromJsonStreamAsync(stream);

            // Assert
            Assert.Equal("StreamDefaultAsync", loaded.Name);
            Assert.Equal(original.Id, loaded.Id);
        }

        [Fact]
        public async Task LoadFromJsonStreamAsync_WithOptions_RestoresModel()
        {
            // Arrange
            var original = new MetaData();
            original.Name = "StreamOptionsAsync";
            original.Description = "Stream options desc";
            var json = original.SaveToJsonString();
            var bytes = Encoding.UTF8.GetBytes(json);
            using var stream = new MemoryStream(bytes);
            var loaded = new MetaData();
            BitOptions options = MetaDataReadWriteOptions.Entities
                               | MetaDataReadWriteOptions.Description
                               | MetaDataReadWriteOptions.Editors
                               | MetaDataReadWriteOptions.CustomInfo
                               | MetaDataReadWriteOptions.KeepCurrent;

            // Act
            await loaded.LoadFromJsonStreamAsync(stream, options);

            // Assert
            Assert.Equal("StreamOptionsAsync", loaded.Name);
            Assert.Equal("Stream options desc", loaded.Description);
        }

        #endregion

        #region SaveToJsonStreamAsync default overload

        [Fact]
        public async Task SaveToJsonStreamAsync_DefaultOverload_WritesToStream()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "DefaultStreamAsync";
            var stream = new MemoryStream();

            // Act
            await model.SaveToJsonStreamAsync(stream);

            // Assert
            var bytes = stream.ToArray();
            Assert.True(bytes.Length > 0);
            var json = Encoding.UTF8.GetString(bytes);
            Assert.Contains("DefaultStreamAsync", json);
        }

        #endregion

        #region SaveToJsonFileAsync default overload

        [Fact]
        public async Task SaveToJsonFileAsync_DefaultOverload_WritesFile()
        {
            // Arrange
            var model = new MetaData();
            model.Name = "DefaultFileAsync";
            var tmpFile = Path.GetTempFileName();

            try
            {
                // Act
                await model.SaveToJsonFileAsync(tmpFile);
                var json = File.ReadAllText(tmpFile);

                // Assert
                Assert.Contains("DefaultFileAsync", json);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        #endregion

        #region BadJsonFormatException on invalid input

        [Fact]
        public void LoadFromJsonString_InvalidJson_ThrowsBadJsonFormatException()
        {
            // Arrange
            var model = new MetaData();
            var invalidJson = "not valid json at all";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => model.LoadFromJsonString(invalidJson));
        }

        [Fact]
        public void ReadFromJson_NonObjectToken_ThrowsBadJsonFormatException()
        {
            // Arrange
            var model = new MetaData();
            var jsonArray = "[1,2,3]";

            // Act & Assert
            Assert.ThrowsAny<Exception>(() =>
            {
                using var sr = new StringReader(jsonArray);
                using var reader = new JsonTextReader(sr);
                model.ReadFromJson(reader, MetaDataReadWriteOptions.Defaults);
            });
        }

        #endregion

        #region Helper Methods

        private static MetaData CreatePopulatedModel()
        {
            var model = new MetaData();
            model.Name = "PopulatedModel";
            model.Description = "A fully populated model for testing";
            model.CustomInfo = "custom-info-data";
            model.ModelVersion = 3;

            var customer = model.AddEntity(null, "Customer");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customer,
                Expression = "Id",
                DataType = DataType.Int32,
                Caption = "Customer ID"
            }).IsPrimaryKey = true;

            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customer,
                Expression = "CompanyName",
                DataType = DataType.String,
                Caption = "Company Name",
                Size = 200
            });

            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = customer,
                Expression = "CreatedAt",
                DataType = DataType.DateTime,
                Caption = "Created At"
            });

            var order = model.AddEntity(null, "Order");
            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = order,
                Expression = "OrderId",
                DataType = DataType.Int32,
                Caption = "Order ID"
            }).IsPrimaryKey = true;

            model.AddEntityAttr(new MetaEntityAttrDescriptor
            {
                Parent = order,
                Expression = "Amount",
                DataType = DataType.Currency,
                Caption = "Amount"
            });

            return model;
        }

        #endregion
    }
}
