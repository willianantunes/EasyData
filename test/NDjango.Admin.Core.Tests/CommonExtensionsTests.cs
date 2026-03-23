using NDjango.Admin;
using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class CommonExtensionsTests
    {
        #region IntToDataType

        [Theory]
        [InlineData(0, DataType.Unknown)]
        [InlineData(1, DataType.String)]
        [InlineData(4, DataType.Int32)]
        [InlineData(6, DataType.Bool)]
        [InlineData(12, DataType.DateTime)]
        [InlineData(17, DataType.Guid)]
        public void IntToDataType_ValidInt_ReturnsExpectedDataType(int value, DataType expected)
        {
            // Arrange
            // (value provided via InlineData)

            // Act
            var result = value.IntToDataType();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region ToInt

        [Theory]
        [InlineData(DataType.Unknown, 0)]
        [InlineData(DataType.String, 1)]
        [InlineData(DataType.Int32, 4)]
        [InlineData(DataType.Bool, 6)]
        [InlineData(DataType.DateTime, 12)]
        [InlineData(DataType.Guid, 17)]
        public void ToInt_ValidDataType_ReturnsExpectedInt(DataType dataType, int expected)
        {
            // Arrange
            // (dataType provided via InlineData)

            // Act
            var result = dataType.ToInt();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region Roundtrip IntToDataType / ToInt

        [Theory]
        [InlineData(DataType.Unknown)]
        [InlineData(DataType.String)]
        [InlineData(DataType.Byte)]
        [InlineData(DataType.Word)]
        [InlineData(DataType.Int32)]
        [InlineData(DataType.Int64)]
        [InlineData(DataType.Bool)]
        [InlineData(DataType.Float)]
        [InlineData(DataType.Currency)]
        [InlineData(DataType.Date)]
        [InlineData(DataType.Time)]
        [InlineData(DataType.DateTime)]
        [InlineData(DataType.Guid)]
        [InlineData(DataType.Geometry)]
        [InlineData(DataType.Geography)]
        public void IntToDataType_ToInt_Roundtrip_ReturnsOriginalValue(DataType original)
        {
            // Arrange
            var intValue = original.ToInt();

            // Act
            var result = intValue.IntToDataType();

            // Assert
            Assert.Equal(original, result);
        }

        #endregion

        #region StrToDataType - Special aliases

        [Fact]
        public void StrToDataType_WideString_ReturnsString()
        {
            // Arrange
            var typeName = "WideString";

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(DataType.String, result);
        }

        [Fact]
        public void StrToDataType_Int_ReturnsInt32()
        {
            // Arrange
            var typeName = "Int";

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(DataType.Int32, result);
        }

        #endregion

        #region StrToDataType - Standard enum names

        [Theory]
        [InlineData("String", DataType.String)]
        [InlineData("Bool", DataType.Bool)]
        [InlineData("DateTime", DataType.DateTime)]
        [InlineData("Date", DataType.Date)]
        [InlineData("Time", DataType.Time)]
        [InlineData("Int32", DataType.Int32)]
        [InlineData("Int64", DataType.Int64)]
        [InlineData("Float", DataType.Float)]
        [InlineData("Currency", DataType.Currency)]
        [InlineData("Guid", DataType.Guid)]
        [InlineData("Byte", DataType.Byte)]
        [InlineData("Word", DataType.Word)]
        [InlineData("Memo", DataType.Memo)]
        [InlineData("Blob", DataType.Blob)]
        [InlineData("Unknown", DataType.Unknown)]
        public void StrToDataType_StandardEnumName_ReturnsExpectedDataType(string typeName, DataType expected)
        {
            // Arrange
            // (typeName provided via InlineData)

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region StrToDataType - Case insensitive

        [Theory]
        [InlineData("string", DataType.String)]
        [InlineData("STRING", DataType.String)]
        [InlineData("bool", DataType.Bool)]
        [InlineData("BOOL", DataType.Bool)]
        [InlineData("dateTime", DataType.DateTime)]
        [InlineData("int32", DataType.Int32)]
        public void StrToDataType_CaseInsensitive_ReturnsExpectedDataType(string typeName, DataType expected)
        {
            // Arrange
            // (typeName provided via InlineData)

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region StrToDataType - Whitespace

        [Theory]
        [InlineData("  String  ", DataType.String)]
        [InlineData(" Bool ", DataType.Bool)]
        [InlineData("\tDateTime\t", DataType.DateTime)]
        [InlineData("  WideString  ", DataType.String)]
        [InlineData("  Int  ", DataType.Int32)]
        public void StrToDataType_WithWhitespace_TrimsAndReturnsExpectedDataType(string typeName, DataType expected)
        {
            // Arrange
            // (typeName provided via InlineData)

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region StrToDataType - Invalid

        [Theory]
        [InlineData("InvalidType")]
        [InlineData("FooBar")]
        [InlineData("")]
        public void StrToDataType_InvalidTypeName_ReturnsUnknown(string typeName)
        {
            // Arrange
            // (typeName provided via InlineData)

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(DataType.Unknown, result);
        }

        #endregion
    }
}
