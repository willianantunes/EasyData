using NDjango.Admin.EntityFrameworkCore;
using Xunit;

namespace NDjango.Admin.EntityFrameworkCore.Relational.Tests
{
    public class DbContextMetaDataLoaderExceptionTests
    {
        [Fact]
        public void Constructor_SetsMessage()
        {
            // Arrange
            var message = "Test error message";

            // Act
            var ex = new DbContextMetaDataLoaderException(message);

            // Assert
            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public void Constructor_IsException()
        {
            // Arrange & Act
            var ex = new DbContextMetaDataLoaderException("error");

            // Assert
            Assert.IsAssignableFrom<System.Exception>(ex);
        }
    }
}
