using System;
using System.Collections;
using System.Linq;

using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class AdminSettingsTests
    {
        private class SampleDto
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public SampleDto Inner { get; set; }
        }

        private class TestSettings : IAdminSettings<TestSettings> { }

        [Fact]
        public void Constructor_WithValidPropertySelectors_ExtractsPropertyNames()
        {
            // Arrange & Act
            var list = new PropertyList<SampleDto>(x => x.Name, x => x.Age);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal("Name", list[0]);
            Assert.Equal("Age", list[1]);
        }

        [Fact]
        public void Constructor_WithNoSelectors_CreatesEmptyList()
        {
            // Arrange & Act
            var list = new PropertyList<SampleDto>();

            // Assert
            Assert.Empty(list);
        }

        [Fact]
        public void Constructor_WithUnaryExpression_ExtractsPropertyName()
        {
            // Arrange & Act
            // int property causes a Convert (boxing) UnaryExpression wrapping a MemberExpression
            var list = new PropertyList<SampleDto>(x => x.Age);

            // Assert
            var single = Assert.Single(list);
            Assert.Equal("Age", single);
        }

        [Fact]
        public void Validate_InvalidExpression_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            // A method call expression is neither MemberExpression nor UnaryExpression wrapping MemberExpression
            var ex = Assert.Throws<ArgumentException>(
                () => new PropertyList<SampleDto>(x => x.Name.ToString()));

            Assert.Contains("Expression must be a direct property access", ex.Message);
        }

        [Fact]
        public void Validate_NestedProperty_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(
                () => new PropertyList<SampleDto>(x => x.Inner.Name));

            Assert.Contains("Property must be accessed directly on the entity", ex.Message);
        }

        [Fact]
        public void Indexer_ReturnsCorrectName()
        {
            // Arrange
            var list = new PropertyList<SampleDto>(x => x.Name, x => x.Age);

            // Act
            var first = list[0];
            var second = list[1];

            // Assert
            Assert.Equal("Name", first);
            Assert.Equal("Age", second);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_Works()
        {
            // Arrange
            var list = new PropertyList<SampleDto>(x => x.Name, x => x.Age);

            // Act
            var enumerable = (IEnumerable)list;
            var items = enumerable.Cast<string>().ToList();

            // Assert
            Assert.Equal(2, items.Count);
            Assert.Equal("Name", items[0]);
            Assert.Equal("Age", items[1]);
        }

        [Fact]
        public void IAdminSettings_DefaultSearchFields_ReturnsEmptyList()
        {
            // Arrange
            IAdminSettings<TestSettings> settings = new TestSettings();

            // Act
            var searchFields = settings.SearchFields;

            // Assert
            Assert.NotNull(searchFields);
            Assert.Empty(searchFields);
        }

        [Fact]
        public void IAdminSettings_DefaultActions_ReturnsNull()
        {
            // Arrange
            IAdminSettings<TestSettings> settings = new TestSettings();

            // Act
            var actions = settings.Actions;

            // Assert
            Assert.Null(actions);
        }
    }
}
