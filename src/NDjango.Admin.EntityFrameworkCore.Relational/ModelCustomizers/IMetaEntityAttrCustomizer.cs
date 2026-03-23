namespace NDjango.Admin.EntityFrameworkCore
{
    public interface IMetaEntityAttrCustomizer
    {
        public IMetaEntityAttrCustomizer SetDescription(string description);
        public IMetaEntityAttrCustomizer SetDisplayFormat(string displayFormat);
        public IMetaEntityAttrCustomizer SetDisplayName(string displayName);
        public IMetaEntityAttrCustomizer SetEditable(bool editable);
        public IMetaEntityAttrCustomizer SetIndex(int index);
        public IMetaEntityAttrCustomizer SetShowInLookup(bool showInLookup);
        public IMetaEntityAttrCustomizer SetShowOnCreate(bool showOnCreate);
        public IMetaEntityAttrCustomizer SetShowOnEdit(bool showOnEdit);
        public IMetaEntityAttrCustomizer SetShowOnView(bool showOnView);
        public IMetaEntityAttrCustomizer SetSorting(int sorting);
        public IMetaEntityAttrCustomizer SetDataType(DataType dataType);
        public IMetaEntityAttrCustomizer SetDefaultValue(object value);
    }
}
