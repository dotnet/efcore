using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Extensions
{
    public static class ModelBuilderContributorDbContextOptionsExtensions
    {
        public static DbContextOptionsBuilder UseContributor<TContributor>(this DbContextOptionsBuilder builder)
            where TContributor : class, IModelBuilderConvention
        {
            var extension = builder.Options.FindExtension<ModelBuilderContributorExtension>() ??
                            new ModelBuilderContributorExtension();
            extension.AddConvention<TContributor>();
            return builder;
        }
    }
}
