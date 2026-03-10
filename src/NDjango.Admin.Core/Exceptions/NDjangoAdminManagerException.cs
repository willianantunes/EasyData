using System;

namespace NDjango.Admin
{
    public class NDjangoAdminManagerException : Exception
    {
        public NDjangoAdminManagerException(string message) : base(message)
        { }
    }

    public class RecordNotFoundException : NDjangoAdminManagerException
    {
        public RecordNotFoundException(string sourceId, string recordKey)
            : base($"Can't found the record with ID {recordKey} in {sourceId}")
        { }
    }

    public class ContainerNotFoundException : NDjangoAdminManagerException
    {
        public ContainerNotFoundException(string sourceId) : base($"Container is not found: {sourceId}")
        { }
    }
}
