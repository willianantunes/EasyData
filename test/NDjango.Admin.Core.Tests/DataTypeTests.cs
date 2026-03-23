using System;
using System.Linq;
using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class DataTypeListTests
    {
        [Fact]
        public void Constructor_Default_CreatesEmptyList()
        {
            // Arrange & Act
            var list = new DataTypeList();

            // Assert
            Assert.Empty(list);
        }

        [Fact]
        public void Constructor_WithArray_PopulatesList()
        {
            // Arrange
            var types = new[] { DataType.String, DataType.Int32, DataType.Bool };

            // Act
            var list = new DataTypeList(types);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal(DataType.String, list[0]);
            Assert.Equal(DataType.Int32, list[1]);
            Assert.Equal(DataType.Bool, list[2]);
        }

        [Fact]
        public void Constructor_WithCommaString_ParsesCorrectly()
        {
            // Arrange
            var commaStr = "String, Int32, Bool";

            // Act
            var list = new DataTypeList(commaStr);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal(DataType.String, list[0]);
            Assert.Equal(DataType.Int32, list[1]);
            Assert.Equal(DataType.Bool, list[2]);
        }

        [Fact]
        public void Constructor_WithNullString_CreatesEmptyList()
        {
            // Arrange & Act
            var list = new DataTypeList((string)null);

            // Assert
            Assert.Empty(list);
        }

        [Fact]
        public void Constructor_WithEmptyString_CreatesEmptyList()
        {
            // Arrange & Act
            var list = new DataTypeList(string.Empty);

            // Assert
            Assert.Empty(list);
        }

        [Fact]
        public void InsertItem_DuplicateType_PreventsDuplicates()
        {
            // Arrange
            var list = new DataTypeList
            {
                DataType.String,

                // Act
                DataType.String
            };

            // Assert
            Assert.Single(list);
            Assert.Equal(DataType.String, list[0]);
        }

        [Fact]
        public void InsertItem_UniqueType_AddsSuccessfully()
        {
            // Arrange
            var list = new DataTypeList
            {
                DataType.String,

                // Act
                DataType.Int32
            };

            // Assert
            Assert.Equal(2, list.Count);
        }

        [Fact]
        public void InsertItem_InsertDuplicateAtIndex_SkipsInsertion()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.String, DataType.Bool });

            // Act
            list.Insert(0, DataType.Bool);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal(DataType.String, list[0]);
            Assert.Equal(DataType.Bool, list[1]);
        }

        [Fact]
        public void AddRange_MultipleTypes_AddsAll()
        {
            // Arrange
            var list = new DataTypeList();
            var types = new[] { DataType.Date, DataType.Time, DataType.DateTime };

            // Act
            list.AddRange(types);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal(DataType.Date, list[0]);
            Assert.Equal(DataType.Time, list[1]);
            Assert.Equal(DataType.DateTime, list[2]);
        }

        [Fact]
        public void AddRange_WithDuplicates_SkipsDuplicates()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.String });
            var types = new[] { DataType.String, DataType.Int32 };

            // Act
            list.AddRange(types);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal(DataType.String, list[0]);
            Assert.Equal(DataType.Int32, list[1]);
        }

        [Fact]
        public void InsertRange_InsertsAtCorrectPositions()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.String, DataType.Bool });
            var types = new[] { DataType.Int32, DataType.Float };

            // Act
            list.InsertRange(1, types);

            // Assert
            Assert.Equal(4, list.Count);
            Assert.Equal(DataType.String, list[0]);
            Assert.Equal(DataType.Int32, list[1]);
            Assert.Equal(DataType.Float, list[2]);
            Assert.Equal(DataType.Bool, list[3]);
        }

        [Fact]
        public void InsertRange_AtBeginning_InsertsCorrectly()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.Bool });
            var types = new[] { DataType.String, DataType.Int32 };

            // Act
            list.InsertRange(0, types);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal(DataType.String, list[0]);
            Assert.Equal(DataType.Int32, list[1]);
            Assert.Equal(DataType.Bool, list[2]);
        }

        [Fact]
        public void InsertRange_WithDuplicates_SkipsDuplicates()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.String, DataType.Bool });
            var types = new[] { DataType.String, DataType.Float };

            // Act
            list.InsertRange(1, types);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Contains(DataType.Float, list);
        }

        [Fact]
        public void CommaText_Getter_ProducesCorrectString()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.String, DataType.Int32, DataType.Bool });

            // Act
            var result = list.CommaText;

            // Assert
            Assert.Equal("String, Int32, Bool", result);
        }

        [Fact]
        public void CommaText_Getter_EmptyList_ReturnsEmptyString()
        {
            // Arrange
            var list = new DataTypeList();

            // Act
            var result = list.CommaText;

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void CommaText_Getter_SingleItem_ReturnsWithoutComma()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.Float });

            // Act
            var result = list.CommaText;

            // Assert
            Assert.Equal("Float", result);
        }

        [Fact]
        public void CommaText_Setter_ParsesAndSets()
        {
            // Arrange
            var list = new DataTypeList
            {
                // Act
                CommaText = "Date,Time,DateTime"
            };

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal(DataType.Date, list[0]);
            Assert.Equal(DataType.Time, list[1]);
            Assert.Equal(DataType.DateTime, list[2]);
        }

        [Fact]
        public void CommaText_Setter_ClearsPreviousItems()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.String, DataType.Int32 })
            {
                // Act
                CommaText = "Bool"
            };

            // Assert
            Assert.Single(list);
            Assert.Equal(DataType.Bool, list[0]);
        }

        [Fact]
        public void CommaText_Setter_WithNull_ClearsList()
        {
            // Arrange
            var list = new DataTypeList(new[] { DataType.String, DataType.Int32 })
            {
                // Act
                CommaText = null
            };

            // Assert
            Assert.Empty(list);
        }

        [Fact]
        public void CommonDataTypes_ContainsExpectedTypesAndCount()
        {
            // Arrange & Act
            var list = DataTypeList.CommonDataTypes;

            // Assert
            Assert.Equal(16, list.Count);
            Assert.Contains(DataType.Autoinc, list);
            Assert.Contains(DataType.Blob, list);
            Assert.Contains(DataType.Bool, list);
            Assert.Contains(DataType.Byte, list);
            Assert.Contains(DataType.Currency, list);
            Assert.Contains(DataType.Date, list);
            Assert.Contains(DataType.DateTime, list);
            Assert.Contains(DataType.FixedChar, list);
            Assert.Contains(DataType.Float, list);
            Assert.Contains(DataType.Guid, list);
            Assert.Contains(DataType.Int32, list);
            Assert.Contains(DataType.Int64, list);
            Assert.Contains(DataType.Memo, list);
            Assert.Contains(DataType.String, list);
            Assert.Contains(DataType.Time, list);
            Assert.Contains(DataType.Word, list);
        }

        [Fact]
        public void RangeDataTypes_ContainsExpectedTypesAndCount()
        {
            // Arrange & Act
            var list = DataTypeList.RangeDataTypes;

            // Assert
            Assert.Equal(8, list.Count);
            Assert.Contains(DataType.Byte, list);
            Assert.Contains(DataType.Word, list);
            Assert.Contains(DataType.Int32, list);
            Assert.Contains(DataType.Int64, list);
            Assert.Contains(DataType.Float, list);
            Assert.Contains(DataType.Currency, list);
            Assert.Contains(DataType.BCD, list);
            Assert.Contains(DataType.Autoinc, list);
        }

        [Fact]
        public void FloatDataTypes_ContainsExpectedTypesAndCount()
        {
            // Arrange & Act
            var list = DataTypeList.FloatDataTypes;

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Contains(DataType.Float, list);
            Assert.Contains(DataType.Currency, list);
            Assert.Contains(DataType.BCD, list);
        }

        [Fact]
        public void IntegerDataTypes_ContainsExpectedTypesAndCount()
        {
            // Arrange & Act
            var list = DataTypeList.IntegerDataTypes;

            // Assert
            Assert.Equal(5, list.Count);
            Assert.Contains(DataType.Byte, list);
            Assert.Contains(DataType.Word, list);
            Assert.Contains(DataType.Int32, list);
            Assert.Contains(DataType.Int64, list);
            Assert.Contains(DataType.Autoinc, list);
        }

        [Fact]
        public void StringDataTypes_ContainsExpectedTypesAndCount()
        {
            // Arrange & Act
            var list = DataTypeList.StringDataTypes;

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Contains(DataType.String, list);
            Assert.Contains(DataType.Memo, list);
            Assert.Contains(DataType.FixedChar, list);
        }

        [Fact]
        public void TimeDataTypes_ContainsExpectedTypesAndCount()
        {
            // Arrange & Act
            var list = DataTypeList.TimeDataTypes;

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Contains(DataType.Date, list);
            Assert.Contains(DataType.Time, list);
            Assert.Contains(DataType.DateTime, list);
        }

        [Fact]
        public void BoolDataTypes_ContainsExpectedTypesAndCount()
        {
            // Arrange & Act
            var list = DataTypeList.BoolDataTypes;

            // Assert
            Assert.Single(list);
            Assert.Contains(DataType.Bool, list);
        }

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

        [Theory]
        [InlineData("String", DataType.String)]
        [InlineData("Bool", DataType.Bool)]
        [InlineData("Date", DataType.Date)]
        [InlineData("DateTime", DataType.DateTime)]
        [InlineData("Float", DataType.Float)]
        [InlineData("Int64", DataType.Int64)]
        [InlineData("Guid", DataType.Guid)]
        [InlineData("Blob", DataType.Blob)]
        public void StrToDataType_StandardNames_ReturnsCorrectType(string typeName, DataType expected)
        {
            // Arrange & Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void StrToDataType_CaseInsensitive_ReturnsCorrectType()
        {
            // Arrange
            var typeName = "string";

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(DataType.String, result);
        }

        [Fact]
        public void StrToDataType_InvalidName_ReturnsUnknown()
        {
            // Arrange
            var typeName = "NotAType";

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(DataType.Unknown, result);
        }

        [Fact]
        public void StrToDataType_WithWhitespace_TrimsAndParses()
        {
            // Arrange
            var typeName = "  String  ";

            // Act
            var result = typeName.StrToDataType();

            // Assert
            Assert.Equal(DataType.String, result);
        }

        [Theory]
        [InlineData(DataType.Unknown, 0)]
        [InlineData(DataType.String, 1)]
        [InlineData(DataType.Byte, 2)]
        [InlineData(DataType.Int32, 4)]
        [InlineData(DataType.Int64, 5)]
        [InlineData(DataType.Bool, 6)]
        [InlineData(DataType.Float, 7)]
        [InlineData(DataType.Date, 10)]
        [InlineData(DataType.DateTime, 12)]
        [InlineData(DataType.Guid, 17)]
        [InlineData(DataType.Geography, 19)]
        public void IntToDataType_RoundtripsWithToInt(DataType dataType, int expectedInt)
        {
            // Arrange
            var intValue = dataType.ToInt();

            // Act
            var roundTripped = intValue.IntToDataType();

            // Assert
            Assert.Equal(expectedInt, intValue);
            Assert.Equal(dataType, roundTripped);
        }

        [Fact]
        public void ToInt_ReturnsCorrectIntegerValue()
        {
            // Arrange
            var dt = DataType.Currency;

            // Act
            var result = dt.ToInt();

            // Assert
            Assert.Equal(8, result);
        }

        [Fact]
        public void IntToDataType_ReturnsCorrectDataType()
        {
            // Arrange
            var value = 15;

            // Act
            var result = value.IntToDataType();

            // Assert
            Assert.Equal(DataType.Blob, result);
        }
    }
}
