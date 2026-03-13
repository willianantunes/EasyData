using System;
using NDjango.Admin.EntityFrameworkCore;
using Xunit;

namespace NDjango.Admin.EntityFrameworkCore.Relational.Tests
{
    public class MetaEntityVoidCustomizerTests
    {
        #region MetaEntityVoidCustomizer

        [Fact]
        public void SetDisplayName_ReturnsItself()
        {
            // Arrange
            var metadata = new MetaData();
            var modelBuilder = new MetadataCustomizer(metadata);
            var customizer = new MetaEntityVoidCustomizer<SampleEntity>(modelBuilder);

            // Act
            var result = customizer.SetDisplayName("Test");

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void SetDisplayNamePlural_ReturnsItself()
        {
            // Arrange
            var metadata = new MetaData();
            var modelBuilder = new MetadataCustomizer(metadata);
            var customizer = new MetaEntityVoidCustomizer<SampleEntity>(modelBuilder);

            // Act
            var result = customizer.SetDisplayNamePlural("Tests");

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void SetDescription_ReturnsItself()
        {
            // Arrange
            var metadata = new MetaData();
            var modelBuilder = new MetadataCustomizer(metadata);
            var customizer = new MetaEntityVoidCustomizer<SampleEntity>(modelBuilder);

            // Act
            var result = customizer.SetDescription("Desc");

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void SetEditable_ReturnsItself()
        {
            // Arrange
            var metadata = new MetaData();
            var modelBuilder = new MetadataCustomizer(metadata);
            var customizer = new MetaEntityVoidCustomizer<SampleEntity>(modelBuilder);

            // Act
            var result = customizer.SetEditable(false);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void Attribute_ReturnsSameVoidAttrCustomizer()
        {
            // Arrange
            var metadata = new MetaData();
            var modelBuilder = new MetadataCustomizer(metadata);
            var customizer = new MetaEntityVoidCustomizer<SampleEntity>(modelBuilder);

            // Act
            var attr1 = customizer.Attribute(x => x.Name);
            var attr2 = customizer.Attribute(x => x.Id);

            // Assert
            Assert.NotNull(attr1);
            Assert.Same(attr1, attr2);
        }

        #endregion

        #region MetaEntityAttrVoidCustomizer

        [Fact]
        public void AttrVoid_SetDisplayName_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetDisplayName("Name");

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetDisplayFormat_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetDisplayFormat("{0}");

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetDescription_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetDescription("desc");

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetEditable_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetEditable(true);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetIndex_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetIndex(5);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetShowInLookup_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetShowInLookup(true);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetShowOnView_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetShowOnView(false);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetShowOnEdit_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetShowOnEdit(true);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetShowOnCreate_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetShowOnCreate(false);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetSorting_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetSorting(1);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetDataType_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetDataType(DataType.String);

            // Assert
            Assert.Same(customizer, result);
        }

        [Fact]
        public void AttrVoid_SetDefaultValue_ReturnsItself()
        {
            // Arrange
            var customizer = new MetaEntityAttrVoidCustomizer();

            // Act
            var result = customizer.SetDefaultValue("default");

            // Assert
            Assert.Same(customizer, result);
        }

        #endregion

        #region Helper classes

        public class SampleEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}
