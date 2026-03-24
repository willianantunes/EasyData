using System;
using System.Collections.Generic;

using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class CompositeKeyEncoderTests
    {
        #region Encode Tests

        [Fact]
        public void Encode_WithValidKeyParts_ReturnsCommaSeparatedValues()
        {
            // Arrange
            var keyParts = new List<KeyValuePair<string, string>>
            {
                new("MenuItemId", "1"),
                new("IngredientId", "2")
            };

            // Act
            var result = CompositeKeyEncoder.Encode(keyParts);

            // Assert
            Assert.Equal("1,2", result);
        }

        [Fact]
        public void Encode_WithSingleKeyPart_ReturnsSingleValue()
        {
            // Arrange
            var keyParts = new List<KeyValuePair<string, string>>
            {
                new("Id", "42")
            };

            // Act
            var result = CompositeKeyEncoder.Encode(keyParts);

            // Assert
            Assert.Equal("42", result);
        }

        [Fact]
        public void Encode_WithSpecialCharacters_UrlEncodesValues()
        {
            // Arrange
            var keyParts = new List<KeyValuePair<string, string>>
            {
                new("Key1", "hello world"),
                new("Key2", "foo,bar")
            };

            // Act
            var result = CompositeKeyEncoder.Encode(keyParts);

            // Assert
            Assert.Equal("hello+world,foo%2Cbar", result);
        }

        [Fact]
        public void Encode_WithNullKeyParts_ThrowsArgumentException()
        {
            // Arrange
            IReadOnlyList<KeyValuePair<string, string>> keyParts = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CompositeKeyEncoder.Encode(keyParts));
            Assert.Equal("keyParts", ex.ParamName);
        }

        [Fact]
        public void Encode_WithEmptyKeyParts_ThrowsArgumentException()
        {
            // Arrange
            var keyParts = new List<KeyValuePair<string, string>>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CompositeKeyEncoder.Encode(keyParts));
            Assert.Equal("keyParts", ex.ParamName);
        }

        [Fact]
        public void Encode_WithThreeKeyParts_ReturnsThreeCommaSeparatedValues()
        {
            // Arrange
            var keyParts = new List<KeyValuePair<string, string>>
            {
                new("A", "10"),
                new("B", "20"),
                new("C", "30")
            };

            // Act
            var result = CompositeKeyEncoder.Encode(keyParts);

            // Assert
            Assert.Equal("10,20,30", result);
        }

        #endregion

        #region Decode Tests

        [Fact]
        public void Decode_WithValidEncodedString_ReturnsDictionary()
        {
            // Arrange
            var encoded = "1,2";
            var pkPropNames = new List<string> { "MenuItemId", "IngredientId" };

            // Act
            var result = CompositeKeyEncoder.Decode(encoded, pkPropNames);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("1", result["MenuItemId"]);
            Assert.Equal("2", result["IngredientId"]);
        }

        [Fact]
        public void Decode_WithSingleValue_ReturnsSingleEntryDictionary()
        {
            // Arrange
            var encoded = "42";
            var pkPropNames = new List<string> { "Id" };

            // Act
            var result = CompositeKeyEncoder.Decode(encoded, pkPropNames);

            // Assert
            Assert.Single(result);
            Assert.Equal("42", result["Id"]);
        }

        [Fact]
        public void Decode_WithUrlEncodedValues_DecodesCorrectly()
        {
            // Arrange
            var encoded = "hello+world,foo%2Cbar";
            var pkPropNames = new List<string> { "Key1", "Key2" };

            // Act
            var result = CompositeKeyEncoder.Decode(encoded, pkPropNames);

            // Assert
            Assert.Equal("hello world", result["Key1"]);
            Assert.Equal("foo,bar", result["Key2"]);
        }

        [Fact]
        public void Decode_WithNullEncodedString_ThrowsArgumentException()
        {
            // Arrange
            var pkPropNames = new List<string> { "Id" };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CompositeKeyEncoder.Decode(null, pkPropNames));
            Assert.Equal("encoded", ex.ParamName);
        }

        [Fact]
        public void Decode_WithEmptyEncodedString_ThrowsArgumentException()
        {
            // Arrange
            var pkPropNames = new List<string> { "Id" };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CompositeKeyEncoder.Decode("", pkPropNames));
            Assert.Equal("encoded", ex.ParamName);
        }

        [Fact]
        public void Decode_WithNullPkPropNames_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CompositeKeyEncoder.Decode("1,2", null));
            Assert.Equal("pkPropNames", ex.ParamName);
        }

        [Fact]
        public void Decode_WithEmptyPkPropNames_ThrowsArgumentException()
        {
            // Arrange
            var pkPropNames = new List<string>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CompositeKeyEncoder.Decode("1,2", pkPropNames));
            Assert.Equal("pkPropNames", ex.ParamName);
        }

        [Fact]
        public void Decode_WithMismatchedPartCount_ThrowsArgumentException()
        {
            // Arrange
            var encoded = "1,2,3";
            var pkPropNames = new List<string> { "A", "B" };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => CompositeKeyEncoder.Decode(encoded, pkPropNames));
            Assert.Contains("Expected 2 key parts but got 3", ex.Message);
        }

        [Fact]
        public void Decode_WithThreeKeyParts_ReturnsCorrectDictionary()
        {
            // Arrange
            var encoded = "10,20,30";
            var pkPropNames = new List<string> { "A", "B", "C" };

            // Act
            var result = CompositeKeyEncoder.Decode(encoded, pkPropNames);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("10", result["A"]);
            Assert.Equal("20", result["B"]);
            Assert.Equal("30", result["C"]);
        }

        #endregion

        #region Round-trip Tests

        [Fact]
        public void Encode_ThenDecode_ReturnsOriginalValues()
        {
            // Arrange
            var originalKeys = new List<KeyValuePair<string, string>>
            {
                new("MenuItemId", "5"),
                new("IngredientId", "10")
            };
            var pkPropNames = new List<string> { "MenuItemId", "IngredientId" };

            // Act
            var encoded = CompositeKeyEncoder.Encode(originalKeys);
            var decoded = CompositeKeyEncoder.Decode(encoded, pkPropNames);

            // Assert
            Assert.Equal("5", decoded["MenuItemId"]);
            Assert.Equal("10", decoded["IngredientId"]);
        }

        [Fact]
        public void Encode_ThenDecode_WithSpecialCharacters_ReturnsOriginalValues()
        {
            // Arrange
            var originalKeys = new List<KeyValuePair<string, string>>
            {
                new("Key1", "value with spaces"),
                new("Key2", "value,with,commas"),
                new("Key3", "value&with=special+chars")
            };
            var pkPropNames = new List<string> { "Key1", "Key2", "Key3" };

            // Act
            var encoded = CompositeKeyEncoder.Encode(originalKeys);
            var decoded = CompositeKeyEncoder.Decode(encoded, pkPropNames);

            // Assert
            Assert.Equal("value with spaces", decoded["Key1"]);
            Assert.Equal("value,with,commas", decoded["Key2"]);
            Assert.Equal("value&with=special+chars", decoded["Key3"]);
        }

        #endregion
    }
}
