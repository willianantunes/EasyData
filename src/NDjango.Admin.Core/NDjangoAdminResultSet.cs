using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace NDjango.Admin
{

    public enum ColumnAlignment
    {
        None,
        Left,
        Center,
        Right
    }

    public class NDjangoAdminColStyle
    {
        public ColumnAlignment Alignment { get; set; } = ColumnAlignment.None;

        public bool AllowAutoFormatting { get; set; } = false;
    }


    public class NDjangoAdminColDesc
    {
        /// <summary>
        /// Represents the internal column ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Represents the order number of this column among all columns in the result set.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Indicates whether this column is an aggregate one.
        /// </summary>
        public bool IsAggr { get; set; }

        /// <summary>
        /// The label that is used for this column in UI.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The detailed column description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of data represented by the property.
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Represents internal property ID.
        /// </summary>
        public string AttrId { get; set; }

        /// <summary>
        /// The display format for the property.
        /// </summary>
        public string DisplayFormat { get; set; }

        public string GroupFooterColumnTemplate { get; set; }

        /// <summary>
        /// The style of the property to display in UI.
        /// </summary>
        public NDjangoAdminColStyle Style { get; set; }
    }

    public class NDjangoAdminCol
    {
        /// <summary>
        /// Represents the internal column ID.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; }

        /// <summary>
        /// Represents the order number of this column among all columns in the result set.
        /// </summary>
        [JsonIgnore]
        public int Index { get; }

        /// <summary>
        /// Indicates whether this column is an aggregate one.
        /// </summary>
        [JsonProperty("isAggr")]
        public bool IsAggr { get; }

        /// <summary>
        /// The label that is used for this column in UI.
        /// </summary>
        [JsonProperty("label")]
        public string Label { get; set; }

        /// <summary>
        /// The detailed column description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// The type of data represented by the property.
        /// </summary>
        [Obsolete("Use DataType instead")]
        [JsonIgnore]
        public DataType Type => DataType;

        /// <summary>
        /// The type of data represented by the property.
        /// </summary>
        [JsonProperty("type")]
        public DataType DataType { get; }

        /// <summary>
        /// Represents the ID of the metadata attribute this column is based on.
        /// </summary>
        [JsonProperty("originAttrId")]
        public string OrginAttrId { get; }

        /// <summary>
        /// The display format for the property.
        /// </summary>
        [JsonProperty("dfmt")]
        public string DisplayFormat { get; set; }

        [JsonProperty("gfct")]
        public string GroupFooterColumnTemplate { get; set; }

        /// <summary>
        /// The style of the property to display in UI.
        /// </summary>
        [JsonProperty("style")]
        public NDjangoAdminColStyle Style { get; }

        public NDjangoAdminCol(NDjangoAdminColDesc desc)
        {
            Id = desc.Id;
            Style = desc.Style ?? new NDjangoAdminColStyle();
            Index = desc.Index;
            IsAggr = desc.IsAggr;
            OrginAttrId = desc.AttrId;
            Label = desc.Label;
            Description = desc.Description;
            DataType = desc.DataType;
            DisplayFormat = desc.DisplayFormat;
            GroupFooterColumnTemplate = desc.GroupFooterColumnTemplate;
        }
    }

    public class NDjangoAdminRow : List<object>
    {
        public NDjangoAdminRow() : base()
        { }

        public NDjangoAdminRow(IEnumerable<object> collection) : base(collection)
        {
        }
    }

    public interface INDjangoAdminResultSet
    {
        /// <summary>
        /// Gets columns
        /// </summary>
        IReadOnlyList<NDjangoAdminCol> Cols { get; }

        /// <summary>
        /// Gets rows.
        /// </summary>
        IEnumerable<NDjangoAdminRow> Rows { get; }
    }


    public class NDjangoAdminResultSet : INDjangoAdminResultSet
    {
        [JsonProperty("cols")]
        public List<NDjangoAdminCol> Cols { get; } = new List<NDjangoAdminCol>();
        [JsonProperty("rows")]
        public List<NDjangoAdminRow> Rows { get; } = new List<NDjangoAdminRow>();

        IReadOnlyList<NDjangoAdminCol> INDjangoAdminResultSet.Cols => Cols;

        IEnumerable<NDjangoAdminRow> INDjangoAdminResultSet.Rows => Rows;
    }
}