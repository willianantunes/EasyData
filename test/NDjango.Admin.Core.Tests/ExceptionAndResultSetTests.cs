using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace NDjango.Admin.Core.Tests
{
    public class ExceptionAndResultSetTests
    {
        #region BadJsonFormatException

        [Fact]
        public void Constructor_WithPath_MessageContainsPath()
        {
            // Arrange
            var path = "/some/json/path";

            // Act
            var exception = new BadJsonFormatException(path);

            // Assert
            Assert.Contains(path, exception.Message);
            Assert.Equal($"Wrong JSON format at path: {path}", exception.Message);
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_SetsMessageAndInnerException()
        {
            // Arrange
            var message = "Something went wrong";
            var innerException = new InvalidOperationException("inner error");

            // Act
            var exception = new BadJsonFormatException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        #endregion

        #region NDjangoAdminManagerException

        [Fact]
        public void NDjangoAdminManagerException_Constructor_SetsMessage()
        {
            // Arrange
            var message = "manager error occurred";

            // Act
            var exception = new NDjangoAdminManagerException(message);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.IsAssignableFrom<Exception>(exception);
        }

        #endregion

        #region RecordNotFoundException

        [Fact]
        public void RecordNotFoundException_Constructor_MessageContainsSourceIdAndRecordKey()
        {
            // Arrange
            var sourceId = "Restaurants";
            var recordKey = "42";

            // Act
            var exception = new RecordNotFoundException(sourceId, recordKey);

            // Assert
            Assert.Contains(sourceId, exception.Message);
            Assert.Contains(recordKey, exception.Message);
            Assert.Equal($"Can't found the record with ID {recordKey} in {sourceId}", exception.Message);
            Assert.IsAssignableFrom<NDjangoAdminManagerException>(exception);
        }

        #endregion

        #region ContainerNotFoundException

        [Fact]
        public void ContainerNotFoundException_Constructor_MessageContainsSourceId()
        {
            // Arrange
            var sourceId = "MissingContainer";

            // Act
            var exception = new ContainerNotFoundException(sourceId);

            // Assert
            Assert.Contains(sourceId, exception.Message);
            Assert.Equal($"Container is not found: {sourceId}", exception.Message);
            Assert.IsAssignableFrom<NDjangoAdminManagerException>(exception);
        }

        #endregion

        #region NDjangoAdminColStyle

        [Fact]
        public void NDjangoAdminColStyle_Defaults_AlignmentIsNoneAndAllowAutoFormattingIsFalse()
        {
            // Arrange & Act
            var style = new NDjangoAdminColStyle();

            // Assert
            Assert.Equal(ColumnAlignment.None, style.Alignment);
            Assert.False(style.AllowAutoFormatting);
        }

        #endregion

        #region NDjangoAdminColDesc

        [Fact]
        public void NDjangoAdminColDesc_AllProperties_AreSettable()
        {
            // Arrange
            var style = new NDjangoAdminColStyle { Alignment = ColumnAlignment.Right, AllowAutoFormatting = true };

            // Act
            var desc = new NDjangoAdminColDesc
            {
                Id = "col1",
                Index = 3,
                IsAggr = true,
                Label = "Column One",
                Description = "First column",
                DataType = DataType.String,
                AttrId = "attr1",
                DisplayFormat = "{0:N2}",
                GroupFooterColumnTemplate = "Total: {0}",
                Style = style
            };

            // Assert
            Assert.Equal("col1", desc.Id);
            Assert.Equal(3, desc.Index);
            Assert.True(desc.IsAggr);
            Assert.Equal("Column One", desc.Label);
            Assert.Equal("First column", desc.Description);
            Assert.Equal(DataType.String, desc.DataType);
            Assert.Equal("attr1", desc.AttrId);
            Assert.Equal("{0:N2}", desc.DisplayFormat);
            Assert.Equal("Total: {0}", desc.GroupFooterColumnTemplate);
            Assert.Same(style, desc.Style);
        }

        #endregion

        #region NDjangoAdminCol

        [Fact]
        public void NDjangoAdminCol_Constructor_SetsAllPropertiesFromDesc()
        {
            // Arrange
            var style = new NDjangoAdminColStyle { Alignment = ColumnAlignment.Center, AllowAutoFormatting = true };
            var desc = new NDjangoAdminColDesc
            {
                Id = "revenue",
                Index = 5,
                IsAggr = true,
                Label = "Revenue",
                Description = "Total revenue",
                DataType = DataType.Currency,
                AttrId = "revenueAttr",
                DisplayFormat = "{0:C}",
                GroupFooterColumnTemplate = "Sum: {0}",
                Style = style
            };

            // Act
            var col = new NDjangoAdminCol(desc);

            // Assert
            Assert.Equal("revenue", col.Id);
            Assert.Equal(5, col.Index);
            Assert.True(col.IsAggr);
            Assert.Equal("Revenue", col.Label);
            Assert.Equal("Total revenue", col.Description);
            Assert.Equal(DataType.Currency, col.DataType);
            Assert.Equal("revenueAttr", col.OrginAttrId);
            Assert.Equal("{0:C}", col.DisplayFormat);
            Assert.Equal("Sum: {0}", col.GroupFooterColumnTemplate);
            Assert.Same(style, col.Style);
        }

        [Fact]
        public void NDjangoAdminCol_Constructor_NullStyleInDesc_CreatesDefaultStyle()
        {
            // Arrange
            var desc = new NDjangoAdminColDesc
            {
                Id = "name",
                Index = 0,
                Label = "Name",
                DataType = DataType.String,
                Style = null
            };

            // Act
            var col = new NDjangoAdminCol(desc);

            // Assert
            Assert.NotNull(col.Style);
            Assert.Equal(ColumnAlignment.None, col.Style.Alignment);
            Assert.False(col.Style.AllowAutoFormatting);
        }

        [Fact]
        public void NDjangoAdminCol_Constructor_ProvidedStyleInDesc_UsesProvidedStyle()
        {
            // Arrange
            var style = new NDjangoAdminColStyle { Alignment = ColumnAlignment.Left, AllowAutoFormatting = true };
            var desc = new NDjangoAdminColDesc
            {
                Id = "amount",
                Index = 1,
                Label = "Amount",
                DataType = DataType.Float,
                Style = style
            };

            // Act
            var col = new NDjangoAdminCol(desc);

            // Assert
            Assert.Same(style, col.Style);
            Assert.Equal(ColumnAlignment.Left, col.Style.Alignment);
            Assert.True(col.Style.AllowAutoFormatting);
        }

        #endregion

        #region NDjangoAdminRow

        [Fact]
        public void NDjangoAdminRow_DefaultConstructor_IsEmpty()
        {
            // Arrange & Act
            var row = new NDjangoAdminRow();

            // Assert
            Assert.Empty(row);
        }

        [Fact]
        public void NDjangoAdminRow_CollectionConstructor_ContainsItems()
        {
            // Arrange
            var items = new List<object> { "hello", 42, true };

            // Act
            var row = new NDjangoAdminRow(items);

            // Assert
            Assert.Equal(3, row.Count);
            Assert.Equal("hello", row[0]);
            Assert.Equal(42, row[1]);
            Assert.Equal(true, row[2]);
        }

        #endregion

        #region NDjangoAdminResultSet

        [Fact]
        public void NDjangoAdminResultSet_Default_ColsAndRowsAreEmpty()
        {
            // Arrange & Act
            var resultSet = new NDjangoAdminResultSet();

            // Assert
            Assert.NotNull(resultSet.Cols);
            Assert.Empty(resultSet.Cols);
            Assert.NotNull(resultSet.Rows);
            Assert.Empty(resultSet.Rows);
        }

        [Fact]
        public void NDjangoAdminResultSet_AsInterface_ColsAndRowsAreAccessible()
        {
            // Arrange
            var resultSet = new NDjangoAdminResultSet();
            var desc = new NDjangoAdminColDesc
            {
                Id = "id",
                Index = 0,
                Label = "ID",
                DataType = DataType.Int32
            };
            resultSet.Cols.Add(new NDjangoAdminCol(desc));
            resultSet.Rows.Add(new NDjangoAdminRow(new List<object> { 1 }));

            // Act
            INDjangoAdminResultSet iface = resultSet;

            // Assert
            Assert.Single(iface.Cols);
            Assert.Equal("id", iface.Cols[0].Id);
            Assert.Single(iface.Rows);
            Assert.Equal(1, iface.Rows.First().First());
        }

        #endregion
    }
}
