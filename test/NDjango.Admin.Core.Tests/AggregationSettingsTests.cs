using System.Linq;

using Xunit;

using NDjango.Admin.Aggregation;

namespace NDjango.Admin.Core.Tests
{
    public class AggregationSettingsTests
    {
        [Fact]
        public void Constructor_DefaultValues_PropertiesAreFalseAndEmpty()
        {
            // Arrange & Act
            var settings = new AggregationSettings();

            // Assert
            Assert.False(settings.UseGrandTotals);
            Assert.False(settings.UseRecordCount);
            Assert.False(settings.CaseSensitiveGroups);
            Assert.Empty(settings.Groups);
            Assert.Empty(settings.Aggregates);
        }

        [Fact]
        public void HasAggregates_NoAggregates_ReturnsFalse()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            var result = settings.HasAggregates;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasAggregates_WithAggregates_ReturnsTrue()
        {
            // Arrange
            var settings = new AggregationSettings();
            settings.AddAggregate("Amount");

            // Act
            var result = settings.HasAggregates;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasGroups_NoGroups_ReturnsFalse()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            var result = settings.HasGroups;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasGroups_WithGroups_ReturnsTrue()
        {
            // Arrange
            var settings = new AggregationSettings();
            settings.AddGroup("Region", "RegionId");

            // Act
            var result = settings.HasGroups;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasGrandTotals_UseGrandTotalsFalse_ReturnsFalse()
        {
            // Arrange
            var settings = new AggregationSettings { UseGrandTotals = false };

            // Act
            var result = settings.HasGrandTotals;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasGrandTotals_UseGrandTotalsTrue_ReturnsTrue()
        {
            // Arrange
            var settings = new AggregationSettings { UseGrandTotals = true };

            // Act
            var result = settings.HasGrandTotals;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasRecordCount_UseRecordCountFalse_ReturnsFalse()
        {
            // Arrange
            var settings = new AggregationSettings { UseRecordCount = false };

            // Act
            var result = settings.HasRecordCount;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasRecordCount_UseRecordCountTrue_ReturnsTrue()
        {
            // Arrange
            var settings = new AggregationSettings { UseRecordCount = true };

            // Act
            var result = settings.HasRecordCount;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasAggregationData_NoAggregatesNoRecordCount_ReturnsFalse()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            var result = settings.HasAggregationData;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasAggregationData_WithAggregates_ReturnsTrue()
        {
            // Arrange
            var settings = new AggregationSettings();
            settings.AddAggregate("Total");

            // Act
            var result = settings.HasAggregationData;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasAggregationData_WithRecordCountOnly_ReturnsTrue()
        {
            // Arrange
            var settings = new AggregationSettings { UseRecordCount = true };

            // Act
            var result = settings.HasAggregationData;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AddGroup_SingleGroup_AddsGroupWithCorrectColumnsAndName()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            settings.AddGroup("Region", "RegionId", "RegionName");

            // Assert
            Assert.Single(settings.Groups);
            Assert.Equal("Region", settings.Groups[0].Name);
            Assert.Equal(2, settings.Groups[0].Columns.Count);
            Assert.Contains("RegionId", settings.Groups[0].Columns);
            Assert.Contains("RegionName", settings.Groups[0].Columns);
        }

        [Fact]
        public void AddAggregate_DefaultFuncId_AddsSumAggregate()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            settings.AddAggregate("Amount");

            // Assert
            Assert.Single(settings.Aggregates);
            Assert.Equal("Amount", settings.Aggregates[0].ColId);
            Assert.Equal("SUM", settings.Aggregates[0].FuncId);
        }

        [Fact]
        public void AddAggregate_CustomFuncId_AddsAggregateWithSpecifiedFunc()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            settings.AddAggregate("Price", "AVG");

            // Assert
            Assert.Single(settings.Aggregates);
            Assert.Equal("Price", settings.Aggregates[0].ColId);
            Assert.Equal("AVG", settings.Aggregates[0].FuncId);
        }

        [Fact]
        public void GetGroups_MultipleGroups_AccumulatesColumnsProperly()
        {
            // Arrange
            var settings = new AggregationSettings();
            settings.AddGroup("Region", "RegionId");
            settings.AddGroup("City", "CityId");
            settings.AddGroup("Store", "StoreId");

            // Act
            var result = settings.GetGroups();

            // Assert
            Assert.Equal(3, result.Count);

            // First group should have only its own column
            Assert.Equal("Region", result[0].Name);
            Assert.Single(result[0].Columns);
            Assert.Contains("RegionId", result[0].Columns);

            // Second group should accumulate first group's columns
            Assert.Equal("City", result[1].Name);
            Assert.Equal(2, result[1].Columns.Count);
            Assert.Contains("RegionId", result[1].Columns);
            Assert.Contains("CityId", result[1].Columns);

            // Third group should accumulate all previous columns
            Assert.Equal("Store", result[2].Name);
            Assert.Equal(3, result[2].Columns.Count);
            Assert.Contains("RegionId", result[2].Columns);
            Assert.Contains("CityId", result[2].Columns);
            Assert.Contains("StoreId", result[2].Columns);
        }

        [Fact]
        public void GetGroups_NoGroups_ReturnsEmptyList()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            var result = settings.GetGroups();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Clear_WithGroupsAndAggregates_RemovesAll()
        {
            // Arrange
            var settings = new AggregationSettings();
            settings.AddGroup("Region", "RegionId");
            settings.AddAggregate("Amount");

            // Act
            settings.Clear();

            // Assert
            Assert.Empty(settings.Groups);
            Assert.Empty(settings.Aggregates);
            Assert.False(settings.HasGroups);
            Assert.False(settings.HasAggregates);
        }

        [Fact]
        public void AddGroup_FluentChaining_ReturnsSameInstance()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            var returned = settings.AddGroup("Region", "RegionId");

            // Assert
            Assert.Same(settings, returned);
        }

        [Fact]
        public void AddAggregate_FluentChaining_ReturnsSameInstance()
        {
            // Arrange
            var settings = new AggregationSettings();

            // Act
            var returned = settings.AddAggregate("Amount");

            // Assert
            Assert.Same(settings, returned);
        }

        [Fact]
        public void AddGroup_FluentChaining_MultipleGroupsAdded()
        {
            // Arrange & Act
            var settings = new AggregationSettings()
                .AddGroup("Region", "RegionId")
                .AddGroup("City", "CityId")
                .AddAggregate("Total")
                .AddAggregate("Count", "COUNT");

            // Assert
            Assert.Equal(2, settings.Groups.Count);
            Assert.Equal(2, settings.Aggregates.Count);
            Assert.Equal("Region", settings.Groups[0].Name);
            Assert.Equal("City", settings.Groups[1].Name);
            Assert.Equal("Total", settings.Aggregates[0].ColId);
            Assert.Equal("COUNT", settings.Aggregates[1].FuncId);
        }

        [Fact]
        public void GetGroups_WithMultiColumnGroups_AccumulatesAllColumns()
        {
            // Arrange
            var settings = new AggregationSettings();
            settings.AddGroup("Region", "RegionId", "RegionName");
            settings.AddGroup("City", "CityId");

            // Act
            var result = settings.GetGroups();

            // Assert
            Assert.Equal(2, result.Count);

            // First group has its own 2 columns
            Assert.Equal(2, result[0].Columns.Count);
            Assert.Contains("RegionId", result[0].Columns);
            Assert.Contains("RegionName", result[0].Columns);

            // Second group accumulates: RegionId, RegionName, CityId
            Assert.Equal(3, result[1].Columns.Count);
            Assert.Contains("RegionId", result[1].Columns);
            Assert.Contains("RegionName", result[1].Columns);
            Assert.Contains("CityId", result[1].Columns);
        }

        [Fact]
        public void GetGroups_ReturnsNewInstances_NotOriginalGroups()
        {
            // Arrange
            var settings = new AggregationSettings();
            settings.AddGroup("Region", "RegionId");

            // Act
            var result = settings.GetGroups();

            // Assert
            Assert.NotSame(settings.Groups[0], result[0]);
            Assert.Equal(settings.Groups[0].Name, result[0].Name);
        }
    }
}
