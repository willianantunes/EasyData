using System;
using System.Security.Cryptography;
using System.Text;
using NDjango.Admin.AspNetCore.AdminDashboard.Authentication;
using Xunit;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.AuthenticationTests
{
    public class StaticKeyDataProtectorTests
    {
        private static byte[] NewValidKey(byte fill = 0x42)
        {
            var key = new byte[32];
            for (var i = 0; i < key.Length; i++)
                key[i] = fill;
            return key;
        }

        [Fact]
        public void Ctor_KeyTooShort_ThrowsArgumentException()
        {
            // Arrange
            var shortKey = new byte[16];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new StaticKeyDataProtector(shortKey));
        }

        [Fact]
        public void Ctor_KeyTooLong_ThrowsArgumentException()
        {
            // Arrange
            var longKey = new byte[64];

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new StaticKeyDataProtector(longKey));
        }

        [Fact]
        public void Ctor_KeyEmpty_ThrowsArgumentException()
        {
            // Arrange
            var emptyKey = Array.Empty<byte>();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new StaticKeyDataProtector(emptyKey));
        }

        [Fact]
        public void Ctor_KeyNull_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new StaticKeyDataProtector(null));
        }

        [Fact]
        public void Protect_Unprotect_RoundTrip_ReturnsOriginal()
        {
            // Arrange
            var key = NewValidKey();
            var protector = new StaticKeyDataProtector(key);
            var plaintext = Encoding.UTF8.GetBytes("hello, ndjango!");

            // Act
            var protectedBytes = protector.Protect(plaintext);
            var unprotected = protector.Unprotect(protectedBytes);

            // Assert
            Assert.Equal(plaintext, unprotected);
        }

        [Fact]
        public void Protect_EmptyPayload_RoundTripsSuccessfully()
        {
            // Arrange
            var protector = new StaticKeyDataProtector(NewValidKey());
            var empty = Array.Empty<byte>();

            // Act
            var protectedBytes = protector.Protect(empty);
            var unprotected = protector.Unprotect(protectedBytes);

            // Assert
            Assert.Equal(empty, unprotected);
        }

        [Fact]
        public void Protect_CalledTwice_ProducesDifferentOutputs()
        {
            // Arrange
            var protector = new StaticKeyDataProtector(NewValidKey());
            var plaintext = Encoding.UTF8.GetBytes("same message");

            // Act
            var first = protector.Protect(plaintext);
            var second = protector.Protect(plaintext);

            // Assert
            Assert.NotEqual(first, second);
        }

        [Fact]
        public void Unprotect_TamperedCiphertext_ThrowsCryptographicException()
        {
            // Arrange
            var protector = new StaticKeyDataProtector(NewValidKey());
            var plaintext = Encoding.UTF8.GetBytes("payload");
            var protectedBytes = protector.Protect(plaintext);
            // Flip a bit inside the ciphertext region (after nonce[12] + tag[16])
            protectedBytes[protectedBytes.Length - 1] ^= 0x01;

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => protector.Unprotect(protectedBytes));
        }

        [Fact]
        public void Unprotect_TamperedTag_ThrowsCryptographicException()
        {
            // Arrange
            var protector = new StaticKeyDataProtector(NewValidKey());
            var plaintext = Encoding.UTF8.GetBytes("payload");
            var protectedBytes = protector.Protect(plaintext);
            // Flip a bit inside the tag region (bytes 12..27)
            protectedBytes[20] ^= 0x01;

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => protector.Unprotect(protectedBytes));
        }

        [Fact]
        public void Unprotect_TamperedNonce_ThrowsCryptographicException()
        {
            // Arrange
            var protector = new StaticKeyDataProtector(NewValidKey());
            var plaintext = Encoding.UTF8.GetBytes("payload");
            var protectedBytes = protector.Protect(plaintext);
            // Flip a bit inside the nonce region (bytes 0..11)
            protectedBytes[0] ^= 0x01;

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => protector.Unprotect(protectedBytes));
        }

        [Fact]
        public void Unprotect_TooShortInput_ThrowsCryptographicException()
        {
            // Arrange
            var protector = new StaticKeyDataProtector(NewValidKey());
            var tooShort = new byte[27];

            // Act & Assert
            Assert.Throws<CryptographicException>(() => protector.Unprotect(tooShort));
        }

        [Fact]
        public void Unprotect_EmptyInput_ThrowsCryptographicException()
        {
            // Arrange
            var protector = new StaticKeyDataProtector(NewValidKey());

            // Act & Assert
            Assert.Throws<CryptographicException>(() => protector.Unprotect(Array.Empty<byte>()));
        }

        [Fact]
        public void Unprotect_WithDifferentKey_ThrowsCryptographicException()
        {
            // Arrange
            var plaintext = Encoding.UTF8.GetBytes("secret");
            var protectorA = new StaticKeyDataProtector(NewValidKey(0x11));
            var protectorB = new StaticKeyDataProtector(NewValidKey(0x22));
            var encryptedByA = protectorA.Protect(plaintext);

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => protectorB.Unprotect(encryptedByA));
        }

        [Fact]
        public void CreateProtector_DifferentPurpose_ProducesIncompatibleProtectors()
        {
            // Arrange
            var root = new StaticKeyDataProtector(NewValidKey());
            var protectorA = root.CreateProtector("purpose-a");
            var protectorB = root.CreateProtector("purpose-b");
            var plaintext = Encoding.UTF8.GetBytes("message");
            var encryptedByA = protectorA.Protect(plaintext);

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => protectorB.Unprotect(encryptedByA));
        }

        [Fact]
        public void CreateProtector_SamePurpose_ProducesInteroperableProtectors()
        {
            // Arrange
            var root = new StaticKeyDataProtector(NewValidKey());
            var protectorA = root.CreateProtector("purpose");
            var protectorB = root.CreateProtector("purpose");
            var plaintext = Encoding.UTF8.GetBytes("message");

            // Act
            var encryptedByA = protectorA.Protect(plaintext);
            var decryptedByB = protectorB.Unprotect(encryptedByA);

            // Assert
            Assert.Equal(plaintext, decryptedByB);
        }

        [Fact]
        public void CreateProtector_NullPurpose_ThrowsArgumentNullException()
        {
            // Arrange
            var root = new StaticKeyDataProtector(NewValidKey());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => root.CreateProtector(null));
        }
    }
}
