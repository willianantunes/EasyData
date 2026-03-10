using System;
using System.Collections.Generic;
using System.Text;
using NDjango.Admin.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace NDjango.Admin.Services
{
    public static class NDjangoAdminOptionsExtensions
    {
        public static void UseDbContext<TDbContext>(this NDjangoAdminOptions ndjangoAdminOptions, Action<DbContextMetaDataLoaderOptions> loaderOptionsBuilder = null) where TDbContext : DbContext
        {
            if (loaderOptionsBuilder != null) {
                ndjangoAdminOptions.MetaDataLoaderOptionsBuilder = (options) => loaderOptionsBuilder(options as DbContextMetaDataLoaderOptions);
            }
            ndjangoAdminOptions.UseManager<NDjangoAdminManagerEF<TDbContext>>();
            ndjangoAdminOptions.RegisterFilter<SubstringFilter>(SubstringFilter.Class);
        }
    }
}
