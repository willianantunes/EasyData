using System;
using System.Globalization;
using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class DataFormatUtilsTests
    {
        private static readonly CultureInfo InvCulture = CultureInfo.InvariantCulture;

        #region ExtractFormatString

        [Fact]
        public void ExtractFormatString_WithValidFormat_ReturnsInnerFormat()
        {
            // Arrange
            var displayFormat = "{0:dd/MM/yyyy}";

            // Act
            var result = DataFormatUtils.ExtractFormatString(displayFormat);

            // Assert
            Assert.Equal("dd/MM/yyyy", result);
        }

        [Fact]
        public void ExtractFormatString_WithShortDateFormat_ReturnsD()
        {
            // Arrange
            var displayFormat = "{0:d}";

            // Act
            var result = DataFormatUtils.ExtractFormatString(displayFormat);

            // Assert
            Assert.Equal("d", result);
        }

        [Fact]
        public void ExtractFormatString_WithNoMatch_ReturnsEmptyString()
        {
            // Arrange
            var displayFormat = "plain text";

            // Act
            var result = DataFormatUtils.ExtractFormatString(displayFormat);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ExtractFormatString_WithNumericFormat_ReturnsFormat()
        {
            // Arrange
            var displayFormat = "{0:N2}";

            // Act
            var result = DataFormatUtils.ExtractFormatString(displayFormat);

            // Assert
            Assert.Equal("N2", result);
        }

        [Fact]
        public void ExtractFormatString_WithEmptyInnerFormat_ReturnsEmpty()
        {
            // Arrange
            var displayFormat = "{0:}";

            // Act
            var result = DataFormatUtils.ExtractFormatString(displayFormat);

            // Assert
            Assert.Equal("", result);
        }

        #endregion

        #region CheckFormat

        [Fact]
        public void CheckFormat_NullFormat_DoesNotThrow()
        {
            // Arrange
            string dataFormat = null;

            // Act & Assert
            var exception = Record.Exception(() => DataFormatUtils.CheckFormat(dataFormat));
            Assert.Null(exception);
        }

        [Fact]
        public void CheckFormat_EmptyFormat_DoesNotThrow()
        {
            // Arrange
            var dataFormat = "";

            // Act & Assert
            var exception = Record.Exception(() => DataFormatUtils.CheckFormat(dataFormat));
            Assert.Null(exception);
        }

        [Fact]
        public void CheckFormat_ValidFormat_DoesNotThrow()
        {
            // Arrange
            var dataFormat = "{0:dd/MM/yyyy}";

            // Act & Assert
            var exception = Record.Exception(() => DataFormatUtils.CheckFormat(dataFormat));
            Assert.Null(exception);
        }

        [Fact]
        public void CheckFormat_InvalidFormat_ThrowsInvalidDataFormatException()
        {
            // Arrange
            var dataFormat = "bad-format";

            // Act & Assert
            var exception = Assert.Throws<InvalidDataFormatException>(
                () => DataFormatUtils.CheckFormat(dataFormat));
            Assert.Contains("Invalid display format", exception.Message);
            Assert.Contains("bad-format", exception.Message);
        }

        #endregion

        #region InvalidDataFormatException

        [Fact]
        public void InvalidDataFormatException_DefaultConstructor_CreatesInstance()
        {
            // Arrange & Act
            var exception = new InvalidDataFormatException();

            // Assert
            Assert.NotNull(exception);
            Assert.IsType<InvalidDataFormatException>(exception);
        }

        [Fact]
        public void InvalidDataFormatException_MessageConstructor_SetsMessage()
        {
            // Arrange
            var message = "test error message";

            // Act
            var exception = new InvalidDataFormatException(message);

            // Assert
            Assert.Equal("test error message", exception.Message);
        }

        #endregion

        #region GetDateFormat

        [Fact]
        public void GetDateFormat_DisplayFormatWithSmallD_ReturnsShortDateFormat()
        {
            // Arrange
            var displayFormat = "{0:d}";

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal(InvCulture.DateTimeFormat.ShortDatePattern, result);
        }

        [Fact]
        public void GetDateFormat_DisplayFormatWithCapitalD_ReturnsLongDateFormat()
        {
            // Arrange
            var displayFormat = "{0:D}";

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal(InvCulture.DateTimeFormat.LongDatePattern, result);
        }

        [Fact]
        public void GetDateFormat_DisplayFormatWithSmallF_ReturnsShortDateTimeFormat()
        {
            // Arrange
            var displayFormat = "{0:f}";
            var expected = InvCulture.DateTimeFormat.ShortDatePattern + " "
                           + InvCulture.DateTimeFormat.ShortTimePattern;

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDateFormat_DisplayFormatWithCapitalF_ReturnsLongDateTimeFormat()
        {
            // Arrange
            var displayFormat = "{0:F}";
            var expected = InvCulture.DateTimeFormat.LongDatePattern + " "
                           + InvCulture.DateTimeFormat.LongTimePattern;

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDateFormat_DisplayFormatWithCustom_ReturnsCustomFormat()
        {
            // Arrange
            var displayFormat = "{0:yyyy-MM-dd}";

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal("yyyy-MM-dd", result);
        }

        [Fact]
        public void GetDateFormat_EmptyDisplayFormat_DateType_ReturnsShortDatePattern()
        {
            // Arrange
            var displayFormat = "";

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.Date, InvCulture, displayFormat);

            // Assert
            Assert.Equal(InvCulture.DateTimeFormat.ShortDatePattern, result);
        }

        [Fact]
        public void GetDateFormat_EmptyDisplayFormat_TimeType_ReturnsShortTimePattern()
        {
            // Arrange
            var displayFormat = "";

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.Time, InvCulture, displayFormat);

            // Assert
            Assert.Equal(InvCulture.DateTimeFormat.ShortTimePattern, result);
        }

        [Fact]
        public void GetDateFormat_EmptyDisplayFormat_DateTimeType_ReturnsShortDateTimeCombined()
        {
            // Arrange
            var displayFormat = "";
            var expected = InvCulture.DateTimeFormat.ShortDatePattern + " "
                           + InvCulture.DateTimeFormat.ShortTimePattern;

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDateFormat_NullDisplayFormat_FallsBackToShortFormat()
        {
            // Arrange
            string displayFormat = null;

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.Date, InvCulture, displayFormat);

            // Assert
            Assert.Equal(InvCulture.DateTimeFormat.ShortDatePattern, result);
        }

        [Fact]
        public void GetDateFormat_DisplayFormatNoMatch_ReturnsEmptyExtractedFormat()
        {
            // Arrange
            var displayFormat = "no-format-marker";

            // Act
            var result = DataFormatUtils.GetDateFormat(DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal("", result);
        }

        #endregion

        #region GetFormattedValue

        [Fact]
        public void GetFormattedValue_NullValue_ReturnsEmptyString()
        {
            // Arrange
            object val = null;

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.String, InvCulture, "");

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void GetFormattedValue_DateTime_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            var dt = new DateTime(2024, 3, 15, 14, 30, 45);
            var displayFormat = "{0:yyyy-MM-dd}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(dt, DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal("2024-03-15", result);
        }

        [Fact]
        public void GetFormattedValue_DateTime_WithoutDisplayFormat_UsesShortFormat()
        {
            // Arrange
            var dt = new DateTime(2024, 3, 15, 14, 30, 45);
            var expectedFormat = InvCulture.DateTimeFormat.ShortDatePattern + " "
                                 + InvCulture.DateTimeFormat.ShortTimePattern;
            var expected = dt.ToString(expectedFormat, CultureInfo.InvariantCulture);

            // Act
            var result = DataFormatUtils.GetFormattedValue(dt, DataType.DateTime, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_DateTime_DateType_WithoutDisplayFormat_UsesShortDatePattern()
        {
            // Arrange
            var dt = new DateTime(2024, 3, 15);
            var expected = dt.ToString(InvCulture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture);

            // Act
            var result = DataFormatUtils.GetFormattedValue(dt, DataType.Date, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_DateTime_TimeType_WithoutDisplayFormat_UsesShortTimePattern()
        {
            // Arrange
            var dt = new DateTime(2024, 1, 1, 14, 30, 45);
            var expected = dt.ToString(InvCulture.DateTimeFormat.ShortTimePattern, CultureInfo.InvariantCulture);

            // Act
            var result = DataFormatUtils.GetFormattedValue(dt, DataType.Time, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_DateTimeOffset_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            var dto = new DateTimeOffset(2024, 3, 15, 14, 30, 45, TimeSpan.FromHours(2));
            var displayFormat = "{0:yyyy-MM-dd HH:mm}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(dto, DataType.DateTime, InvCulture, displayFormat);

            // Assert
            Assert.Equal("2024-03-15 14:30", result);
        }

        [Fact]
        public void GetFormattedValue_DateTimeOffset_WithoutDisplayFormat_UsesShortFormat()
        {
            // Arrange
            var dto = new DateTimeOffset(2024, 3, 15, 14, 30, 45, TimeSpan.Zero);
            var expectedFormat = InvCulture.DateTimeFormat.ShortDatePattern + " "
                                 + InvCulture.DateTimeFormat.ShortTimePattern;
            var expected = dto.ToString(expectedFormat, CultureInfo.InvariantCulture);

            // Act
            var result = DataFormatUtils.GetFormattedValue(dto, DataType.DateTime, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_TimeSpan_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            var ts = new TimeSpan(14, 30, 45);
            var displayFormat = "{0:hh\\:mm}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(ts, DataType.Time, InvCulture, displayFormat);

            // Assert
            Assert.Equal("14:30", result);
        }

        [Fact]
        public void GetFormattedValue_TimeSpan_WithoutDisplayFormat_UsesConstantFormat()
        {
            // Arrange
            var ts = new TimeSpan(1, 14, 30, 45);
            var expected = ts.ToString("c", CultureInfo.InvariantCulture);

            // Act
            var result = DataFormatUtils.GetFormattedValue(ts, DataType.Time, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_DateOnly_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            var dateOnly = new DateOnly(2024, 3, 15);
            var displayFormat = "{0:yyyy-MM-dd}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(dateOnly, DataType.Date, InvCulture, displayFormat);

            // Assert
            Assert.Equal("2024-03-15", result);
        }

        [Fact]
        public void GetFormattedValue_DateOnly_WithoutDisplayFormat_UsesShortDatePattern()
        {
            // Arrange
            var dateOnly = new DateOnly(2024, 3, 15);
            var expected = dateOnly.ToString(InvCulture.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture);

            // Act
            var result = DataFormatUtils.GetFormattedValue(dateOnly, DataType.Date, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_TimeOnly_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            var timeOnly = new TimeOnly(14, 30, 45);
            var displayFormat = "{0:HH:mm}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(timeOnly, DataType.Time, InvCulture, displayFormat);

            // Assert
            Assert.Equal("14:30", result);
        }

        [Fact]
        public void GetFormattedValue_TimeOnly_WithoutDisplayFormat_UsesShortTimePattern()
        {
            // Arrange
            var timeOnly = new TimeOnly(14, 30, 45);
            var expected = timeOnly.ToString(InvCulture.DateTimeFormat.ShortTimePattern, CultureInfo.InvariantCulture);

            // Act
            var result = DataFormatUtils.GetFormattedValue(timeOnly, DataType.Time, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_Float_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            float val = 1234.56f;
            var displayFormat = "{0:N2}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Float, InvCulture, displayFormat);

            // Assert
            Assert.Equal(string.Format(InvCulture, "{0:N2}", val), result);
        }

        [Fact]
        public void GetFormattedValue_Float_WithoutDisplayFormat_UsesDefaultFormat()
        {
            // Arrange
            float val = 1234.56f;
            var expected = string.Format(InvCulture, "{0}", val);

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Float, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_Double_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            double val = 9876.54;
            var displayFormat = "{0:F1}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Float, InvCulture, displayFormat);

            // Assert
            Assert.Equal(string.Format(InvCulture, "{0:F1}", val), result);
        }

        [Fact]
        public void GetFormattedValue_Double_WithoutDisplayFormat_UsesDefaultFormat()
        {
            // Arrange
            double val = 9876.54;
            var expected = string.Format(InvCulture, "{0}", val);

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Float, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_Int_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            int val = 42;
            var displayFormat = "{0:D5}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Int32, InvCulture, displayFormat);

            // Assert
            Assert.Equal("00042", result);
        }

        [Fact]
        public void GetFormattedValue_Int_WithoutDisplayFormat_UsesDefaultFormat()
        {
            // Arrange
            int val = 42;
            var expected = string.Format(InvCulture, "{0}", val);

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Int32, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_Decimal_WithDisplayFormat_FormatsWithCulture()
        {
            // Arrange
            decimal val = 1234.5678m;
            var displayFormat = "{0:C}";

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Currency, InvCulture, displayFormat);

            // Assert
            Assert.Equal(string.Format(InvCulture, "{0:C}", val), result);
        }

        [Fact]
        public void GetFormattedValue_Decimal_WithoutDisplayFormat_UsesDefaultFormat()
        {
            // Arrange
            decimal val = 1234.5678m;
            var expected = string.Format(InvCulture, "{0}", val);

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Currency, InvCulture, "");

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetFormattedValue_OtherType_ReturnsToString()
        {
            // Arrange
            var val = "hello world";

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.String, InvCulture, "");

            // Assert
            Assert.Equal("hello world", result);
        }

        [Fact]
        public void GetFormattedValue_BoolValue_ReturnsToString()
        {
            // Arrange
            object val = true;

            // Act
            var result = DataFormatUtils.GetFormattedValue(val, DataType.Bool, InvCulture, "");

            // Assert
            Assert.Equal("True", result);
        }

        [Fact]
        public void GetFormattedValue_GuidValue_ReturnsToString()
        {
            // Arrange
            var guid = new Guid("12345678-1234-1234-1234-123456789abc");

            // Act
            var result = DataFormatUtils.GetFormattedValue(guid, DataType.Guid, InvCulture, "");

            // Assert
            Assert.Equal("12345678-1234-1234-1234-123456789abc", result);
        }

        #endregion
    }
}
