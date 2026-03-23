using System;
using System.Globalization;

using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class SequenceFormatterTests
    {
        [Fact]
        public void Constructor_ValidFormatStartingWithS_CreatesInstance()
        {
            // Arrange
            var format = "S";

            // Act
            var formatter = new SequenceFormatter(format);

            // Assert
            Assert.NotNull(formatter);
        }

        [Fact]
        public void Constructor_PositionalValues_ParsesCorrectly()
        {
            // Arrange
            var format = "SFalse|True";

            // Act
            var formatter = new SequenceFormatter(format);

            // Assert
            Assert.NotNull(formatter);
            // Verify by formatting: index 0 -> "False", index 1 -> "True"
            var result0 = formatter.Format(format, false, formatter);
            var result1 = formatter.Format(format, true, formatter);
            Assert.Equal("False", result0);
            Assert.Equal("True", result1);
        }

        [Fact]
        public void Constructor_KeyedValues_ParsesCorrectly()
        {
            // Arrange
            var format = "SZero=0|One=1|Five=5";

            // Act
            var formatter = new SequenceFormatter(format);

            // Assert
            Assert.NotNull(formatter);
            var result0 = formatter.Format(format, 0, formatter);
            var result1 = formatter.Format(format, 1, formatter);
            var result5 = formatter.Format(format, 5, formatter);
            Assert.Equal("Zero", result0);
            Assert.Equal("One", result1);
            Assert.Equal("Five", result5);
        }

        [Fact]
        public void Constructor_FormatWithoutSPrefix_ThrowsFormatException()
        {
            // Arrange
            var format = "XInvalid";

            // Act & Assert
            Assert.Throws<FormatException>(() => new SequenceFormatter(format));
        }

        [Fact]
        public void Constructor_WithCultureInfo_CreatesInstance()
        {
            // Arrange
            var format = "SNo|Yes";
            var culture = new CultureInfo("en-US");

            // Act
            var formatter = new SequenceFormatter(format, culture);

            // Assert
            Assert.NotNull(formatter);
        }

        [Fact]
        public void GetFormat_ICustomFormatterType_ReturnsThis()
        {
            // Arrange
            var formatter = new SequenceFormatter("S");

            // Act
            var result = formatter.GetFormat(typeof(ICustomFormatter));

            // Assert
            Assert.Same(formatter, result);
        }

        [Fact]
        public void GetFormat_OtherType_ReturnsNull()
        {
            // Arrange
            var formatter = new SequenceFormatter("S");

            // Act
            var result = formatter.GetFormat(typeof(IFormatProvider));

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(true, "Yes")]
        [InlineData(false, "No")]
        public void Format_BoolValues_ReturnsCorrectMapping(bool input, string expected)
        {
            // Arrange
            var format = "SNo|Yes";
            var formatter = new SequenceFormatter(format);

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0, "Zero")]
        [InlineData(1, "One")]
        [InlineData(5, "Five")]
        public void Format_IntegerValues_ReturnsCorrectMapping(int input, string expected)
        {
            // Arrange
            var format = "SZero=0|One=1|Five=5";
            var formatter = new SequenceFormatter(format);

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Format_LongValue_ReturnsCorrectMapping()
        {
            // Arrange
            var format = "SZero=0|One=1|Ten=10";
            var formatter = new SequenceFormatter(format);
            var input = 10L;

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal("Ten", result);
        }

        [Fact]
        public void Format_ByteValue_ReturnsCorrectMapping()
        {
            // Arrange
            var format = "SOff=0|On=1";
            var formatter = new SequenceFormatter(format);
            byte input = 1;

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal("On", result);
        }

        [Fact]
        public void Format_NullArg_ReturnsEmptyString()
        {
            // Arrange
            var format = "SNo|Yes";
            var formatter = new SequenceFormatter(format);

            // Act
            var result = formatter.Format(format, null, formatter);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Format_NonMatchingFormat_FallsThroughToHandleOtherFormats()
        {
            // Arrange
            var format = "SNo|Yes";
            var formatter = new SequenceFormatter(format);
            var differentFormat = "D";

            // Act
            var result = formatter.Format(differentFormat, 42, formatter);

            // Assert
            // HandleOtherFormats will use IFormattable.ToString("D", culture) for int
            Assert.Equal(42.ToString("D", CultureInfo.InvariantCulture), result);
        }

        [Fact]
        public void Format_NonSupportedType_FallsThroughToHandleOtherFormats()
        {
            // Arrange
            var format = "SNo|Yes";
            var formatter = new SequenceFormatter(format);
            var stringArg = "hello";

            // Act
            var result = formatter.Format(format, stringArg, formatter);

            // Assert
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Format_ValueNotInDictionary_ReturnsArgToString()
        {
            // Arrange
            var format = "SZero=0|One=1";
            var formatter = new SequenceFormatter(format);
            var input = 99;

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal("99", result);
        }

        [Fact]
        public void Format_ULongValue_ReturnsCorrectMapping()
        {
            // Arrange
            var format = "SInactive=0|Active=1";
            var formatter = new SequenceFormatter(format);
            ulong input = 1;

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal("Active", result);
        }

        [Fact]
        public void Format_SByteValue_ReturnsCorrectMapping()
        {
            // Arrange
            var format = "SOff=0|On=1";
            var formatter = new SequenceFormatter(format);
            sbyte input = 0;

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal("Off", result);
        }

        [Fact]
        public void Format_UIntValue_ReturnsCorrectMapping()
        {
            // Arrange
            var format = "SZero=0|One=1";
            var formatter = new SequenceFormatter(format);
            uint input = 1;

            // Act
            var result = formatter.Format(format, input, formatter);

            // Assert
            Assert.Equal("One", result);
        }

        [Fact]
        public void Format_IFormattableArg_WithNonMatchingFormat_UsesToStringWithCulture()
        {
            // Arrange
            var culture = new CultureInfo("de-DE");
            var format = "SNo|Yes";
            var formatter = new SequenceFormatter(format, culture);
            var value = 1234.56;

            // Act
            var result = formatter.Format("N2", value, formatter);

            // Assert
            Assert.Equal(value.ToString("N2", culture), result);
        }

        [Fact]
        public void Format_NonIFormattableArg_NonMatchingFormat_ReturnsArgToString()
        {
            // Arrange
            var format = "SNo|Yes";
            var formatter = new SequenceFormatter(format);
            var arg = new NonFormattableObject("test-value");

            // Act
            var result = formatter.Format("X", arg, formatter);

            // Assert
            Assert.Equal("test-value", result);
        }

        [Fact]
        public void Format_IFormattableArgWithBadFormat_ThrowsFormatException()
        {
            // Arrange
            var format = "SNo|Yes";
            var formatter = new SequenceFormatter(format);
            var arg = new ThrowingFormattable();

            // Act / Assert
            var ex = Assert.Throws<FormatException>(() => formatter.Format("Q", arg, formatter));
            Assert.Contains("The format of 'Q' is invalid", ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        /// <summary>
        /// A simple class that does NOT implement IFormattable, used to exercise
        /// the HandleOtherFormats branch for non-IFormattable objects.
        /// </summary>
        private class NonFormattableObject
        {
            private readonly string _value;
            public NonFormattableObject(string value) => _value = value;
            public override string ToString() => _value;
        }

        /// <summary>
        /// An IFormattable implementation that always throws FormatException,
        /// used to exercise the catch(FormatException) rethrow in Format.
        /// </summary>
        private class ThrowingFormattable : IFormattable
        {
            public string ToString(string format, IFormatProvider formatProvider)
            {
                throw new FormatException("Bad format");
            }
        }
    }
}
