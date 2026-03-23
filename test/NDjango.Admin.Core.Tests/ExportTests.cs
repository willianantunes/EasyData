using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NDjango.Admin;
using NDjango.Admin.Export;
using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class ExportTests
    {
        #region BasicDataExportSettings - Constructors

        [Fact]
        public void Constructor_DefaultParameterless_SetsCultureToCurrentCulture()
        {
            // Arrange
            var expectedCulture = CultureInfo.CurrentCulture;

            // Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Equal(expectedCulture, settings.Culture);
        }

        [Fact]
        public void Constructor_WithCultureInfo_SetsCulture()
        {
            // Arrange
            var culture = new CultureInfo("fr-FR");

            // Act
            var settings = new BasicDataExportSettings(culture);

            // Assert
            Assert.Equal(culture, settings.Culture);
        }

        [Fact]
        public void Constructor_WithLocaleString_SetsCorrectCulture()
        {
            // Arrange
            var locale = "de-DE";

            // Act
            var settings = new BasicDataExportSettings(locale);

            // Assert
            Assert.Equal(new CultureInfo("de-DE"), settings.Culture);
        }

        #endregion

        #region BasicDataExportSettings - Default Property

        [Fact]
        public void Default_ReturnsNewInstance()
        {
            // Arrange & Act
            var first = BasicDataExportSettings.Default;
            var second = BasicDataExportSettings.Default;

            // Assert
            Assert.NotSame(first, second);
        }

        [Fact]
        public void Default_HasCurrentCulture()
        {
            // Arrange
            var expectedCulture = CultureInfo.CurrentCulture;

            // Act
            var settings = BasicDataExportSettings.Default;

            // Assert
            Assert.Equal(expectedCulture, settings.Culture);
        }

        #endregion

        #region BasicDataExportSettings - Default Property Values

        [Fact]
        public void Constructor_Default_ShowColumnNamesIsTrue()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.True(settings.ShowColumnNames);
        }

        [Fact]
        public void Constructor_Default_ShowDatasetInfoIsTrue()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.True(settings.ShowDatasetInfo);
        }

        [Fact]
        public void Constructor_Default_PreserveFormattingIsTrue()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.True(settings.PreserveFormatting);
        }

        [Fact]
        public void Constructor_Default_RowLimitIsZero()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Equal(0, settings.RowLimit);
        }

        [Fact]
        public void Constructor_Default_AggregationIsNotNull()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.NotNull(settings.Aggregation);
        }

        [Fact]
        public void Constructor_Default_ColumnFilterIsNull()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Null(settings.ColumnFilter);
        }

        [Fact]
        public void Constructor_Default_RowFilterIsNull()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Null(settings.RowFilter);
        }

        [Fact]
        public void Constructor_Default_TitleIsNull()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Null(settings.Title);
        }

        [Fact]
        public void Constructor_Default_DescriptionIsNull()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Null(settings.Description);
        }

        [Fact]
        public void Constructor_Default_BeforeRowAddedIsNull()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Null(settings.BeforeRowAdded);
        }

        [Fact]
        public void Constructor_Default_BeforeRowInsertIsNull()
        {
            // Arrange & Act
            var settings = new BasicDataExportSettings();

            // Assert
            Assert.Null(settings.BeforeRowInsert);
        }

        #endregion

        #region BasicDataExportSettings - BeforeRowAdded setter

        [Fact]
        public void BeforeRowAdded_WhenSet_SetsBeforeRowInsert()
        {
            // Arrange
            var settings = new BasicDataExportSettings();
            Func<NDjangoAdminRow, BeforeRowAddedCallback, CancellationToken, Task> callback =
                (row, cb, ct) => Task.CompletedTask;

            // Act
            settings.BeforeRowAdded = callback;

            // Assert
            Assert.NotNull(settings.BeforeRowInsert);
        }

        [Fact]
        public void BeforeRowAdded_WhenSetToNull_ClearsBeforeRowInsert()
        {
            // Arrange
            var settings = new BasicDataExportSettings
            {
                BeforeRowAdded = (row, cb, ct) => Task.CompletedTask
            };
            Assert.NotNull(settings.BeforeRowInsert);

            // Act
            settings.BeforeRowAdded = null;

            // Assert
            Assert.Null(settings.BeforeRowInsert);
        }

        #endregion

        #region BasicDataExportSettings - Settable Properties

        [Fact]
        public void Title_CanBeSet()
        {
            // Arrange
            var settings = new BasicDataExportSettings
            {
                // Act
                Title = "My Report"
            };

            // Assert
            Assert.Equal("My Report", settings.Title);
        }

        [Fact]
        public void Description_CanBeSet()
        {
            // Arrange
            var settings = new BasicDataExportSettings
            {
                // Act
                Description = "A detailed description"
            };

            // Assert
            Assert.Equal("A detailed description", settings.Description);
        }

        [Fact]
        public void ShowColumnNames_CanBeSetToFalse()
        {
            // Arrange
            var settings = new BasicDataExportSettings
            {
                // Act
                ShowColumnNames = false
            };

            // Assert
            Assert.False(settings.ShowColumnNames);
        }

        [Fact]
        public void RowLimit_CanBeSet()
        {
            // Arrange
            var settings = new BasicDataExportSettings
            {
                // Act
                RowLimit = 100
            };

            // Assert
            Assert.Equal(100, settings.RowLimit);
        }

        [Fact]
        public void ColumnFilter_CanBeSet()
        {
            // Arrange
            var settings = new BasicDataExportSettings();
            Func<NDjangoAdminCol, bool> filter = col => true;

            // Act
            settings.ColumnFilter = filter;

            // Assert
            Assert.Same(filter, settings.ColumnFilter);
        }

        [Fact]
        public void RowFilter_CanBeSet()
        {
            // Arrange
            var settings = new BasicDataExportSettings();
            Func<NDjangoAdminRow, bool> filter = row => true;

            // Act
            settings.RowFilter = filter;

            // Assert
            Assert.Same(filter, settings.RowFilter);
        }

        #endregion

        #region ExportHelpers - ApplyGroupFooterColumnTemplate

        [Fact]
        public void ApplyGroupFooterColumnTemplate_ReplacesGroupValue()
        {
            // Arrange
            var template = "Total for {{GroupValue}}";
            var val = "USA";

            // Act
            var result = ExportHelpers.ApplyGroupFooterColumnTemplate(template, val, null);

            // Assert
            Assert.Equal("Total for USA", result);
        }

        [Fact]
        public void ApplyGroupFooterColumnTemplate_ReplacesExtraDataVars()
        {
            // Arrange
            var template = "Total for {{GroupValue}}: {{Count}}";
            var val = "USA";
            var extraData = new Dictionary<string, object> { { "Count", 42 } };

            // Act
            var result = ExportHelpers.ApplyGroupFooterColumnTemplate(template, val, extraData);

            // Assert
            Assert.Equal("Total for USA: 42", result);
        }

        [Fact]
        public void ApplyGroupFooterColumnTemplate_UnknownVarReturnsEmpty()
        {
            // Arrange
            var template = "Value: {{Unknown}}";
            var val = "USA";
            var extraData = new Dictionary<string, object> { { "Count", 42 } };

            // Act
            var result = ExportHelpers.ApplyGroupFooterColumnTemplate(template, val, extraData);

            // Assert
            Assert.Equal("Value: ", result);
        }

        [Fact]
        public void ApplyGroupFooterColumnTemplate_NullExtraDataOnlyReplacesGroupValue()
        {
            // Arrange
            var template = "{{GroupValue}} - {{SomeVar}}";
            var val = "Japan";

            // Act
            var result = ExportHelpers.ApplyGroupFooterColumnTemplate(template, val, null);

            // Assert
            Assert.Equal("Japan - ", result);
        }

        [Fact]
        public void ApplyGroupFooterColumnTemplate_MultipleExtraDataVars_ReplacesAll()
        {
            // Arrange
            var template = "{{GroupValue}}: Count={{Count}}, Sum={{Sum}}";
            var val = "Brazil";
            var extraData = new Dictionary<string, object>
            {
                { "Count", 10 },
                { "Sum", "500.5" }
            };

            // Act
            var result = ExportHelpers.ApplyGroupFooterColumnTemplate(template, val, extraData);

            // Assert
            Assert.Equal("Brazil: Count=10, Sum=500.5", result);
        }

        [Fact]
        public void ApplyGroupFooterColumnTemplate_NoPlaceholders_ReturnsTemplateUnchanged()
        {
            // Arrange
            var template = "Static text";

            // Act
            var result = ExportHelpers.ApplyGroupFooterColumnTemplate(template, "val", null);

            // Assert
            Assert.Equal("Static text", result);
        }

        #endregion

        #region ExportHelpers - GetPredefinedFormatters

        [Fact]
        public void GetPredefinedFormatters_WithSequenceFormat_CreatesFormatter()
        {
            // Arrange
            var col = new NDjangoAdminCol(new NDjangoAdminColDesc
            {
                Id = "col1",
                Index = 0,
                DisplayFormat = "{0:SYes=1|No=0}",
                DataType = DataType.Bool
            });
            var cols = new List<NDjangoAdminCol> { col };
            var settings = new BasicDataExportSettings(CultureInfo.InvariantCulture);

            // Act
            var result = ExportHelpers.GetPredefinedFormatters(cols, settings);

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("{0:SYes=1|No=0}"));
            Assert.IsType<SequenceFormatter>(result["{0:SYes=1|No=0}"]);
        }

        [Fact]
        public void GetPredefinedFormatters_EmptyDisplayFormat_SkipsColumn()
        {
            // Arrange
            var col = new NDjangoAdminCol(new NDjangoAdminColDesc
            {
                Id = "col1",
                Index = 0,
                DisplayFormat = "",
                DataType = DataType.String
            });
            var cols = new List<NDjangoAdminCol> { col };
            var settings = new BasicDataExportSettings(CultureInfo.InvariantCulture);

            // Act
            var result = ExportHelpers.GetPredefinedFormatters(cols, settings);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPredefinedFormatters_NullDisplayFormat_SkipsColumn()
        {
            // Arrange
            var col = new NDjangoAdminCol(new NDjangoAdminColDesc
            {
                Id = "col1",
                Index = 0,
                DisplayFormat = null,
                DataType = DataType.String
            });
            var cols = new List<NDjangoAdminCol> { col };
            var settings = new BasicDataExportSettings(CultureInfo.InvariantCulture);

            // Act
            var result = ExportHelpers.GetPredefinedFormatters(cols, settings);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPredefinedFormatters_NonSequenceFormat_SkipsColumn()
        {
            // Arrange
            var col = new NDjangoAdminCol(new NDjangoAdminColDesc
            {
                Id = "col1",
                Index = 0,
                DisplayFormat = "{0:N2}",
                DataType = DataType.Float
            });
            var cols = new List<NDjangoAdminCol> { col };
            var settings = new BasicDataExportSettings(CultureInfo.InvariantCulture);

            // Act
            var result = ExportHelpers.GetPredefinedFormatters(cols, settings);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetPredefinedFormatters_DuplicateDisplayFormat_SkipsDuplicate()
        {
            // Arrange
            var format = "{0:SYes=1|No=0}";
            var col1 = new NDjangoAdminCol(new NDjangoAdminColDesc
            {
                Id = "col1",
                Index = 0,
                DisplayFormat = format,
                DataType = DataType.Bool
            });
            var col2 = new NDjangoAdminCol(new NDjangoAdminColDesc
            {
                Id = "col2",
                Index = 1,
                DisplayFormat = format,
                DataType = DataType.Bool
            });
            var cols = new List<NDjangoAdminCol> { col1, col2 };
            var settings = new BasicDataExportSettings(CultureInfo.InvariantCulture);

            // Act
            var result = ExportHelpers.GetPredefinedFormatters(cols, settings);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void GetPredefinedFormatters_EmptyColumnList_ReturnsEmptyDictionary()
        {
            // Arrange
            var cols = new List<NDjangoAdminCol>();
            var settings = new BasicDataExportSettings(CultureInfo.InvariantCulture);

            // Act
            var result = ExportHelpers.GetPredefinedFormatters(cols, settings);

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
