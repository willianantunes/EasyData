using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void GetSecondPart_SeparatorPresent_ReturnsSecondPart()
        {
            // Arrange
            var input = "first.second";

            // Act
            var result = input.GetSecondPart('.');

            // Assert
            Assert.Equal("second", result);
        }

        [Fact]
        public void GetSecondPart_NoSeparator_ReturnsWholeString()
        {
            // Arrange
            var input = "noseparator";

            // Act
            var result = input.GetSecondPart('.');

            // Assert
            Assert.Equal("noseparator", result);
        }

        [Fact]
        public void GetSecondPart_MultipleSeparators_ReturnsOnlySecondPart()
        {
            // Arrange
            var input = "first.second.third";

            // Act
            var result = input.GetSecondPart('.');

            // Assert
            Assert.Equal("second", result);
        }

        [Fact]
        public void GetSecondPart_DifferentSeparator_ReturnsSecondPart()
        {
            // Arrange
            var input = "key:value";

            // Act
            var result = input.GetSecondPart(':');

            // Assert
            Assert.Equal("value", result);
        }

        [Fact]
        public void ToIdentifier_WithDots_ReplacesWithUnderscores()
        {
            // Arrange
            var input = "hello.world";

            // Act
            var result = input.ToIdentifier();

            // Assert
            Assert.Equal("hello_world", result);
        }

        [Fact]
        public void ToIdentifier_VariousSpecialChars_ReplacesAllWithUnderscores()
        {
            // Arrange
            var input = "a;b?c";

            // Act
            var result = input.ToIdentifier();

            // Assert
            Assert.Equal("a_b_c", result);
        }

        [Fact]
        public void ToIdentifier_AlreadyValidIdentifier_ReturnsUnchanged()
        {
            // Arrange
            var input = "abc123";

            // Act
            var result = input.ToIdentifier();

            // Assert
            Assert.Equal("abc123", result);
        }

        [Fact]
        public void ToIdentifier_SpacesAndHyphens_ReplacesWithUnderscores()
        {
            // Arrange
            var input = "my-var name";

            // Act
            var result = input.ToIdentifier();

            // Assert
            Assert.Equal("my_var_name", result);
        }

        [Fact]
        public void ToIdentifier_AllSpecialChars_ReplacesAll()
        {
            // Arrange
            var input = "@#$%";

            // Act
            var result = input.ToIdentifier();

            // Assert
            Assert.Equal("____", result);
        }

        [Fact]
        public void GetSecondPart_SeparatorAtStart_ReturnsEmptyString()
        {
            // Arrange
            var input = ".second";

            // Act
            var result = input.GetSecondPart('.');

            // Assert
            Assert.Equal("second", result);
        }
    }
}
