using System;
using System.Security.Cryptography;
using System.Text;
using NDjango.Admin.AspNetCore.AdminDashboard.Authentication;
using Xunit;

namespace NDjango.Admin.AspNetCore.AdminDashboard.Tests.AuthenticationTests
{
    public class StaticKeyDataProtectionProviderTests
    {
        private const string TestSecret = "this-is-a-shared-secret-for-pods-32-chars";
        private const string OtherSecret = "this-is-a-different-pod-secret-value-xx";
        private const string CookiePurpose = "NDjango.Admin.AdminDashboard.Auth";

        [Fact]
        public void Constructor_NullSecret_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => new StaticKeyDataProtectionProvider(null));
        }

        [Fact]
        public void Constructor_EmptySecret_ThrowsArgumentException()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentException>(() => new StaticKeyDataProtectionProvider(string.Empty));
        }

        [Fact]
        public void CreateProtector_NullPurpose_ThrowsArgumentNullException()
        {
            // Arrange
            var provider = new StaticKeyDataProtectionProvider(TestSecret);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => provider.CreateProtector(null));
        }

        [Fact]
        public void TwoProviders_SameSecret_SamePurpose_Interoperate()
        {
            // Arrange
            var providerPodA = new StaticKeyDataProtectionProvider(TestSecret);
            var providerPodB = new StaticKeyDataProtectionProvider(TestSecret);
            var protectorPodA = providerPodA.CreateProtector(CookiePurpose);
            var protectorPodB = providerPodB.CreateProtector(CookiePurpose);
            var plaintext = Encoding.UTF8.GetBytes("cookie payload");

            // Act
            var encryptedOnA = protectorPodA.Protect(plaintext);
            var decryptedOnB = protectorPodB.Unprotect(encryptedOnA);

            // Assert
            Assert.Equal(plaintext, decryptedOnB);
        }

        [Fact]
        public void TwoProviders_DifferentSecrets_Incompatible()
        {
            // Arrange
            var providerPodA = new StaticKeyDataProtectionProvider(TestSecret);
            var providerPodB = new StaticKeyDataProtectionProvider(OtherSecret);
            var protectorPodA = providerPodA.CreateProtector(CookiePurpose);
            var protectorPodB = providerPodB.CreateProtector(CookiePurpose);
            var plaintext = Encoding.UTF8.GetBytes("cookie payload");
            var encryptedOnA = protectorPodA.Protect(plaintext);

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => protectorPodB.Unprotect(encryptedOnA));
        }

        [Fact]
        public void SameProvider_DifferentPurposes_Incompatible()
        {
            // Arrange
            var provider = new StaticKeyDataProtectionProvider(TestSecret);
            var protectorA = provider.CreateProtector("purpose-a");
            var protectorB = provider.CreateProtector("purpose-b");
            var plaintext = Encoding.UTF8.GetBytes("cookie payload");
            var encryptedByA = protectorA.Protect(plaintext);

            // Act & Assert
            Assert.Throws<AuthenticationTagMismatchException>(() => protectorB.Unprotect(encryptedByA));
        }

        [Fact]
        public void CreateProtector_ReturnsStaticKeyDataProtectorInstance()
        {
            // Arrange
            var provider = new StaticKeyDataProtectionProvider(TestSecret);

            // Act
            var protector = provider.CreateProtector(CookiePurpose);

            // Assert
            Assert.IsType<StaticKeyDataProtector>(protector);
        }
    }
}
