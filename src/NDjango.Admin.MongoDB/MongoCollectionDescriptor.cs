using System;

namespace NDjango.Admin.MongoDB
{
    /// <summary>
    /// Pairs a CLR document type with a MongoDB collection name.
    /// </summary>
    public sealed class MongoCollectionDescriptor
    {
        public Type DocumentType { get; }
        public string CollectionName { get; }
        public bool IsReadOnly { get; }

        public MongoCollectionDescriptor(Type documentType, string collectionName, bool isReadOnly = false)
        {
            DocumentType = documentType ?? throw new ArgumentNullException(nameof(documentType));
            CollectionName = collectionName ?? throw new ArgumentNullException(nameof(collectionName));
            IsReadOnly = isReadOnly;
        }
    }
}
