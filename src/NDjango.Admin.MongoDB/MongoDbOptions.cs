using System;
using System.Collections.Generic;

namespace NDjango.Admin.MongoDB
{
    /// <summary>
    /// Builder class for configuring MongoDB collections exposed in the admin dashboard.
    /// </summary>
    public class MongoDbOptions
    {
        private readonly List<MongoCollectionDescriptor> _collections = new List<MongoCollectionDescriptor>();

        /// <summary>
        /// Gets the registered collection descriptors.
        /// </summary>
        public IReadOnlyList<MongoCollectionDescriptor> Collections => _collections;

        /// <summary>
        /// Registers a collection with an explicit name.
        /// </summary>
        /// <typeparam name="T">The CLR document type.</typeparam>
        /// <param name="collectionName">The MongoDB collection name.</param>
        public MongoDbOptions AddCollection<T>(string collectionName) where T : class
        {
            return AddCollection(typeof(T), collectionName);
        }

        /// <summary>
        /// Registers a collection with an explicit name and read-only flag.
        /// </summary>
        /// <typeparam name="T">The CLR document type.</typeparam>
        /// <param name="collectionName">The MongoDB collection name.</param>
        /// <param name="readOnly">When true, the collection is read-only in the admin dashboard.</param>
        public MongoDbOptions AddCollection<T>(string collectionName, bool readOnly) where T : class
        {
            return AddCollection(typeof(T), collectionName, readOnly);
        }

        /// <summary>
        /// Registers a collection using a convention name (pluralized lowercase type name).
        /// </summary>
        /// <typeparam name="T">The CLR document type.</typeparam>
        public MongoDbOptions AddCollection<T>() where T : class
        {
            var name = (typeof(T).Name + "s").ToLowerInvariant();
            return AddCollection(typeof(T), name);
        }

        /// <summary>
        /// Registers a collection using a convention name (pluralized lowercase type name) with a read-only flag.
        /// </summary>
        /// <typeparam name="T">The CLR document type.</typeparam>
        /// <param name="readOnly">When true, the collection is read-only in the admin dashboard.</param>
        public MongoDbOptions AddCollection<T>(bool readOnly) where T : class
        {
            var name = (typeof(T).Name + "s").ToLowerInvariant();
            return AddCollection(typeof(T), name, readOnly);
        }

        /// <summary>
        /// Registers a collection with a non-generic type and explicit name.
        /// </summary>
        /// <param name="documentType">The CLR document type.</param>
        /// <param name="collectionName">The MongoDB collection name.</param>
        public MongoDbOptions AddCollection(Type documentType, string collectionName)
        {
            _collections.Add(new MongoCollectionDescriptor(documentType, collectionName));
            return this;
        }

        /// <summary>
        /// Registers a collection with a non-generic type, explicit name, and read-only flag.
        /// </summary>
        /// <param name="documentType">The CLR document type.</param>
        /// <param name="collectionName">The MongoDB collection name.</param>
        /// <param name="isReadOnly">When true, the collection is read-only in the admin dashboard.</param>
        public MongoDbOptions AddCollection(Type documentType, string collectionName, bool isReadOnly)
        {
            _collections.Add(new MongoCollectionDescriptor(documentType, collectionName, isReadOnly));
            return this;
        }
    }
}
