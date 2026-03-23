using System;
using System.Collections.Generic;
using System.Reflection;

namespace NDjango.Admin.MongoDB
{
    /// <summary>
    /// Options for <see cref="MongoMetaDataLoader"/> controlling which entities and properties are included.
    /// </summary>
    public class MongoMetaDataLoaderOptions
    {
        private readonly List<Func<Type, bool>> _entityFilters = new List<Func<Type, bool>>();
        private readonly List<Func<PropertyInfo, bool>> _propertyFilters = new List<Func<PropertyInfo, bool>>();

        /// <summary>
        /// Gets the list of entity (document type) filters.
        /// </summary>
        public IReadOnlyList<Func<Type, bool>> EntityFilters => _entityFilters;

        /// <summary>
        /// Gets the list of property filters.
        /// </summary>
        public IReadOnlyList<Func<PropertyInfo, bool>> PropertyFilters => _propertyFilters;

        /// <summary>
        /// Gets or sets a value indicating whether primary key fields should be hidden from the view.
        /// </summary>
        public bool HidePrimaryKeys { get; set; } = true;

        /// <summary>
        /// Adds an entity filter.
        /// </summary>
        /// <param name="filter">The filter predicate. Return true to include the entity.</param>
        public MongoMetaDataLoaderOptions AddEntityFilter(Func<Type, bool> filter)
        {
            _entityFilters.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds a property filter.
        /// </summary>
        /// <param name="filter">The filter predicate. Return true to include the property.</param>
        public MongoMetaDataLoaderOptions AddPropertyFilter(Func<PropertyInfo, bool> filter)
        {
            _propertyFilters.Add(filter);
            return this;
        }

        /// <summary>
        /// Skips a document type entirely, or specific properties when selectors are provided.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        public MongoMetaDataLoaderOptions Skip<TDocument>()
        {
            AddEntityFilter(type => type != typeof(TDocument));
            return this;
        }
    }
}
