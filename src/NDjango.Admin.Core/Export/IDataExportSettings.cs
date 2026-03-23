using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using NDjango.Admin.Aggregation;

namespace NDjango.Admin.Export
{
    public delegate Task WriteRowFunc(NDjangoAdminRow row, Dictionary<string, object> extraData, CancellationToken ct);

    public delegate Task BeforeRowAddedCallback(NDjangoAdminRow row, Dictionary<string, object> extraData, CancellationToken ct);

    /// <summary>
    /// Represents some settings used during exporting operations
    /// </summary>
    public interface IDataExportSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether column names should be included into export result.
        /// </summary>
        /// <value>
        ///     <see langword="true"/> if column names should be included into result file; otherwise, <see langword="false"/>.
        /// </value>
        public bool ShowColumnNames { get; set; }

        /// <summary>
        /// The culture.
        /// </summary>
        public CultureInfo Culture { get; set; }

        /// <summary>
        /// Filter columns during export
        /// </summary>
        public Func<NDjangoAdminCol, bool> ColumnFilter { get; set; }

        /// <summary>
        /// Filter rows during export
        /// </summary>
        public Func<NDjangoAdminRow, bool> RowFilter { get; set; }

        [Obsolete("Use BeforeRowInsert instead")]
        public Func<NDjangoAdminRow, BeforeRowAddedCallback, CancellationToken, Task> BeforeRowAdded { get; set; }

        /// <summary>
        /// Gets or sets the callback functions that is called for each exported row before its insertion.
        /// </summary>
        /// <value>The callback function.</value>
        public Func<NDjangoAdminRow, WriteRowFunc, CancellationToken, Task> BeforeRowInsert { get; set; }

        /// <summary>
        /// The title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether title and description will be shown 
        /// </summary>
        public bool ShowDatasetInfo { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether the exporter should preserve the formatting in the original value
        /// </summary>
        public bool PreserveFormatting { get; set; }

        public AggregationSettings Aggregation { get; set; }

        public int RowLimit { get; set; }
    }
}
