using System.Text.RegularExpressions;
using EasyData.AspNetCore.AdminDashboard.Authentication;
using Xunit;

namespace EasyData.AspNetCore.AdminDashboard.Tests.AuthenticationTests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void HashPassword_SameInput_ReturnsDeterministicHash()
        {
            // Arrange
            var password = "admin";

            // Act
            var hash1 = PasswordHasher.HashPassword(password);
            var hash2 = PasswordHasher.HashPassword(password);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void HashPassword_AnyInput_ReturnsLowercaseHex64Chars()
        {
            // Arrange
            var password = "test";

            // Act
            var hash = PasswordHasher.HashPassword(password);

            // Assert
            Assert.Matches("^[0-9a-f]{64}$", hash);
        }

        [Fact]
        public void HashPassword_DifferentInputs_ReturnsDifferentHashes()
        {
            // Arrange & Act
            var hash1 = PasswordHasher.HashPassword("password1");
            var hash2 = PasswordHasher.HashPassword("password2");

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var hash = PasswordHasher.HashPassword("admin");

            // Act
            var result = PasswordHasher.VerifyPassword("admin", hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            var hash = PasswordHasher.HashPassword("admin");

            // Act
            var result = PasswordHasher.VerifyPassword("wrong", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_UppercaseStoredHash_ReturnsTrue()
        {
            // Arrange
            var hash = PasswordHasher.HashPassword("admin").ToUpperInvariant();

            // Act
            var result = PasswordHasher.VerifyPassword("admin", hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HashPassword_KnownInput_MatchesExpectedSHA256()
        {
            // Arrange
            var expected = "8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918";

            // Act
            var hash = PasswordHasher.HashPassword("admin");

            // Assert
            Assert.Equal(expected, hash);
        }
    }
}
