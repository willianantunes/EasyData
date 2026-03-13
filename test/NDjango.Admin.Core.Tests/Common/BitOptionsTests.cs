using System;
using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class BitOptionsTests
    {
        [Fact]
        public void With_OptionAdded_ContainsOption()
        {
            // Arrange
            var options = new BitOptions();

            // Act
            options.With(MetaDataReadWriteOptions.Entities);

            // Assert
            var signature = (ulong)(options & MetaDataReadWriteOptions.Entities);
            Assert.True(signature > 0);
        }

        [Fact]
        public void Without_OptionRemoved_DoesNotContainOption()
        {
            // Arrange
            var options = new BitOptions();
            options.Without(MetaDataReadWriteOptions.Entities);

            // Act
            var signature = (ulong)(options & MetaDataReadWriteOptions.Entities);

            // Assert
            Assert.Equal(0UL, signature);
        }

        [Fact]
        public void Contains_OptionPresent_ReturnsTrue()
        {
            // Arrange
            var options = (BitOptions)(MetaDataReadWriteOptions.Entities | MetaDataReadWriteOptions.Description);

            // Act
            var result = options.Contains(MetaDataReadWriteOptions.Description);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Contains_OptionAbsent_ReturnsFalse()
        {
            // Arrange
            var options = (BitOptions)(MetaDataReadWriteOptions.Entities | MetaDataReadWriteOptions.Description);

            // Act
            var result = options.Contains(MetaDataReadWriteOptions.CustomInfo);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ExplicitCastToUlong_ReturnsSignatureValue()
        {
            // Arrange
            BitOptions options = MetaDataReadWriteOptions.Entities;

            // Act
            var result = (ulong)options;

            // Assert
            Assert.Equal(MetaDataReadWriteOptions.Entities, result);
        }

        [Fact]
        public void ImplicitCastFromUlong_CreatesBitOptionsWithSignature()
        {
            // Arrange
            ulong value = MetaDataReadWriteOptions.Editors;

            // Act
            BitOptions options = value;

            // Assert
            Assert.True(options.Contains(MetaDataReadWriteOptions.Editors));
        }

        [Fact]
        public void NotOperator_InvertsAllBits()
        {
            // Arrange
            BitOptions options = MetaDataReadWriteOptions.Entities;

            // Act
            var inverted = ~options;

            // Assert
            Assert.False(inverted.Contains(MetaDataReadWriteOptions.Entities));
            Assert.Equal(~MetaDataReadWriteOptions.Entities, (ulong)inverted);
        }

        [Fact]
        public void OrOperator_CombinesBothOptions()
        {
            // Arrange
            BitOptions left = MetaDataReadWriteOptions.Entities;
            BitOptions right = MetaDataReadWriteOptions.Description;

            // Act
            var combined = left | right;

            // Assert
            Assert.True(combined.Contains(MetaDataReadWriteOptions.Entities));
            Assert.True(combined.Contains(MetaDataReadWriteOptions.Description));
            Assert.Equal(MetaDataReadWriteOptions.Entities | MetaDataReadWriteOptions.Description, (ulong)combined);
        }

        [Fact]
        public void AndOperator_RetainsOnlyCommonBits()
        {
            // Arrange
            BitOptions all = MetaDataReadWriteOptions.Entities | MetaDataReadWriteOptions.Description;
            BitOptions mask = MetaDataReadWriteOptions.Entities;

            // Act
            var result = all & mask;

            // Assert
            Assert.Equal(MetaDataReadWriteOptions.Entities, (ulong)result);
            Assert.False(result.Contains(MetaDataReadWriteOptions.Description));
        }

        [Fact]
        public void AndOperator_NoCommonBits_ReturnsZero()
        {
            // Arrange
            BitOptions left = MetaDataReadWriteOptions.Entities;
            BitOptions right = MetaDataReadWriteOptions.Description;

            // Act
            var result = left & right;

            // Assert
            Assert.Equal(0UL, (ulong)result);
        }

        [Fact]
        public void With_ChainedCalls_AccumulatesOptions()
        {
            // Arrange
            var options = new BitOptions();

            // Act
            var result = options
                .With(MetaDataReadWriteOptions.Entities)
                .With(MetaDataReadWriteOptions.Description);

            // Assert
            Assert.True(result.Contains(MetaDataReadWriteOptions.Entities));
            Assert.True(result.Contains(MetaDataReadWriteOptions.Description));
            Assert.Same(options, result);
        }

        [Fact]
        public void Without_RemovesOnlySpecifiedOption()
        {
            // Arrange
            var options = new BitOptions()
                .With(MetaDataReadWriteOptions.Entities)
                .With(MetaDataReadWriteOptions.Description);

            // Act
            options.Without(MetaDataReadWriteOptions.Entities);

            // Assert
            Assert.False(options.Contains(MetaDataReadWriteOptions.Entities));
            Assert.True(options.Contains(MetaDataReadWriteOptions.Description));
        }

        [Fact]
        public void DefaultConstructor_SignatureIsZero()
        {
            // Arrange / Act
            var options = new BitOptions();

            // Assert
            Assert.Equal(0UL, (ulong)options);
        }

        [Fact]
        public void NotOperator_ZeroValue_ReturnsAllBitsSet()
        {
            // Arrange
            var options = new BitOptions();

            // Act
            var inverted = ~options;

            // Assert
            Assert.Equal(ulong.MaxValue, (ulong)inverted);
        }
    }
}
