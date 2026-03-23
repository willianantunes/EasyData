using System;
using System.Linq.Expressions;

namespace NDjango.Admin.EntityFrameworkCore
{
    public interface IMetaEntityCustomizer<TEntity> where TEntity : class
    {
        public IMetaEntityCustomizer<TEntity> SetDescription(string description);
        public IMetaEntityCustomizer<TEntity> SetDisplayName(string displayName);
        public IMetaEntityCustomizer<TEntity> SetDisplayNamePlural(string displayNamePlural);
        public IMetaEntityCustomizer<TEntity> SetEditable(bool editable);
        public IMetaEntityAttrCustomizer Attribute(Expression<Func<TEntity, object>> propertySelector);
    }
}
