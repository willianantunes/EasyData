using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using NDjango.Admin.EntityFrameworkCore;
using Xunit;

namespace NDjango.Admin.EntityFrameworkCore.Relational.Tests
{
    public class TypeExtensionsTests
    {
        #region IsInheritedFrom (string)

        [Fact]
        public void IsInheritedFrom_String_DirectMatch_ReturnsTrue()
        {
            // Arrange
            var type = typeof(ArgumentException);

            // Act
            var result = type.IsInheritedFrom(typeof(ArgumentException).FullName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInheritedFrom_String_BaseClass_ReturnsTrue()
        {
            // Arrange
            var type = typeof(ArgumentException);

            // Act
            var result = type.IsInheritedFrom(typeof(Exception).FullName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInheritedFrom_String_Unrelated_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsInheritedFrom(typeof(int).FullName);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsInheritedFrom (Type)

        [Fact]
        public void IsInheritedFrom_Type_BaseClass_ReturnsTrue()
        {
            // Arrange
            var type = typeof(ArgumentNullException);

            // Act
            var result = type.IsInheritedFrom(typeof(Exception));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInheritedFrom_Type_Unrelated_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsInheritedFrom(typeof(int));

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsInheritedFromGeneric

        [Fact]
        public void IsInheritedFromGeneric_DerivedFromGeneric_ReturnsTrue()
        {
            // Arrange
            var type = typeof(List<string>);

            // Act
            var result = type.IsInheritedFromGeneric(typeof(List<>));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInheritedFromGeneric_NotGenericBase_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsInheritedFromGeneric(typeof(object));

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInheritedFromGeneric_UnrelatedGeneric_ReturnsFalse()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.IsInheritedFromGeneric(typeof(Dictionary<,>));

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsSimpleType

        [Theory]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(string), true)]
        [InlineData(typeof(Guid), true)]
        [InlineData(typeof(Guid?), true)]
        [InlineData(typeof(decimal), true)]
        [InlineData(typeof(decimal?), true)]
        [InlineData(typeof(double), true)]
        [InlineData(typeof(double?), true)]
        [InlineData(typeof(int?), true)]
        [InlineData(typeof(short?), true)]
        [InlineData(typeof(byte?), true)]
        [InlineData(typeof(long?), true)]
        [InlineData(typeof(bool?), true)]
        [InlineData(typeof(DateTime), true)]
        [InlineData(typeof(DateTime?), true)]
        [InlineData(typeof(DateTimeOffset), true)]
        [InlineData(typeof(DateTimeOffset?), true)]
        [InlineData(typeof(TimeSpan), true)]
        [InlineData(typeof(TimeSpan?), true)]
        [InlineData(typeof(bool), true)]
        [InlineData(typeof(List<int>), false)]
        [InlineData(typeof(object), false)]
        public void IsSimpleType_VariousTypes_ReturnsExpected(Type type, bool expected)
        {
            // Arrange & Act
            var result = type.IsSimpleType();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region IsComplexType

        [Fact]
        public void IsComplexType_RegularClass_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsComplexType();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsGenericType

        [Fact]
        public void IsGenericType_GenericList_ReturnsTrue()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var result = type.IsGenericType();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsGenericType_NonGeneric_ReturnsFalse()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var result = type.IsGenericType();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsEnum

        [Fact]
        public void IsEnum_EnumType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(DayOfWeek);

            // Act
            var result = type.IsEnum();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnum_NonEnumType_ReturnsFalse()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsEnum();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsNullable

        [Fact]
        public void IsNullable_NullableInt_ReturnsTrue()
        {
            // Arrange
            var type = typeof(int?);

            // Act
            var result = type.IsNullable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNullable_RegularInt_ReturnsFalse()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsNullable();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsAttributeDefined

        [Fact]
        public void IsAttributeDefined_NoMatchingAttribute_ReturnsFalse()
        {
            // Arrange
            var prop = typeof(SampleClass).GetProperty(nameof(SampleClass.Name));

            // Act
            var result = prop.IsAttributeDefined("NonExistentAttribute");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsAttributeDefined_ObsoleteAttribute_ReturnsTrue()
        {
            // Arrange
            var prop = typeof(SampleClass).GetProperty(nameof(SampleClass.OldProp));

            // Act
            var result = prop.IsAttributeDefined("ObsoleteAttribute");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetMappedProperties

        [Fact]
        public void GetMappedProperties_AllMapped_ReturnsAll()
        {
            // Arrange
            var props = typeof(SampleClass).GetProperties();

            // Act
            var mapped = new List<PropertyInfo>(props.GetMappedProperties());

            // Assert
            Assert.Equal(2, mapped.Count);
        }

        [Fact]
        public void GetMappedProperties_WithNotMapped_FiltersOut()
        {
            // Arrange
            var props = typeof(SampleClassWithNotMapped).GetProperties();

            // Act
            var mapped = new List<PropertyInfo>(props.GetMappedProperties());

            // Assert
            Assert.Single(mapped);
            Assert.Equal("Mapped", mapped[0].Name);
        }

        #endregion

        #region IsEnumerable

        [Fact]
        public void IsEnumerable_ListType_ReturnsTrue()
        {
            // Arrange
            var type = typeof(List<string>);

            // Act
            var result = type.IsEnumerable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnumerable_IEnumerableInterface_ReturnsTrue()
        {
            // Arrange
            var type = typeof(IEnumerable);

            // Act
            var result = type.IsEnumerable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnumerable_GenericIEnumerable_ReturnsTrue()
        {
            // Arrange
            var type = typeof(IEnumerable<int>);

            // Act
            var result = type.IsEnumerable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEnumerable_Int_ReturnsFalse()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var result = type.IsEnumerable();

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsNumeric

        [Theory]
        [InlineData(typeof(int), true)]
        [InlineData(typeof(double), true)]
        [InlineData(typeof(decimal), true)]
        [InlineData(typeof(float), true)]
        [InlineData(typeof(long), true)]
        [InlineData(typeof(short), true)]
        [InlineData(typeof(byte), true)]
        [InlineData(typeof(uint), true)]
        [InlineData(typeof(ulong), true)]
        [InlineData(typeof(ushort), true)]
        [InlineData(typeof(sbyte), true)]
        [InlineData(typeof(int?), true)]
        [InlineData(typeof(string), false)]
        [InlineData(typeof(bool), false)]
        [InlineData(typeof(DateTime), false)]
        public void IsNumeric_VariousTypes_ReturnsExpected(Type type, bool expected)
        {
            // Arrange & Act
            var result = type.IsNumeric();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region Helper classes

        private class SampleClass
        {
            public string Name { get; set; }

            [Obsolete]
            public string OldProp { get; set; }
        }

        private class SampleClassWithNotMapped
        {
            public string Mapped { get; set; }

            [NotMapped]
            public string Ignored { get; set; }
        }

        #endregion
    }
}
