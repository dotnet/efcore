using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public abstract class MongoDbModelAttributeConvention<TModelAttribute> : IModelConvention
        where TModelAttribute : Attribute, IModelAttribute
    {
        private readonly DbContext _dbContext;

        protected MongoDbModelAttributeConvention([NotNull] DbContext dbContext)
        {
            _dbContext = Check.NotNull(dbContext, nameof(dbContext));
        }

        public virtual InternalModelBuilder Apply([NotNull] InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            foreach (TModelAttribute modelAttribute in GetAttributes(_dbContext.GetType()))
            {
                if (!Apply(modelBuilder, modelAttribute))
                {
                    break;
                }
            }
            return modelBuilder;
        }

        protected virtual IEnumerable<TModelAttribute> GetAttributes([NotNull] Type dbContextType)
            => Check.NotNull(dbContextType, nameof(dbContextType))
                .GetTypeInfo()
                .GetCustomAttributes<TModelAttribute>();

        protected virtual bool Apply([NotNull] InternalModelBuilder modelBuilder,
            [NotNull] TModelAttribute modelAttribute)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(modelAttribute, nameof(modelAttribute));
            modelAttribute.Apply(modelBuilder);
            return true;
        }
    }
}