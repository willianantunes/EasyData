using System;

using NDjango.Admin.Services;

namespace NDjango.Admin.MongoDB
{
    public static class NDjangoAdminOptionsExtensions
    {
        /// <summary>
        /// Configures the NDjango.Admin manager to use MongoDB as the data source.
        /// </summary>
        /// <param name="options">The NDjango admin options.</param>
        /// <param name="configure">Action to configure MongoDB collections.</param>
        public static void UseMongoDB(this NDjangoAdminOptions options, Action<MongoDbOptions> configure)
        {
            var mongoOptions = new MongoDbOptions();
            configure(mongoOptions);
            options.UseManager((services, opts) => new NDjangoAdminManagerMongo(services, opts, mongoOptions));
            options.RegisterFilter<MongoSubstringFilter>(MongoSubstringFilter.Class);
        }
    }
}
