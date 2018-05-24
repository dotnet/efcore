// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerIndexConvention :
        IBaseTypeChangedConvention,
        IIndexAddedConvention,
        IIndexUniquenessChangedConvention,
        IIndexAnnotationChangedConvention,
        IPropertyNullabilityChangedConvention,
        IPropertyAnnotationChangedConvention
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerIndexConvention([NotNull] ISqlGenerationHelper sqlGenerationHelper)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (oldBaseType == null
                || entityTypeBuilder.Metadata.BaseType == null)
            {
                foreach (var index in entityTypeBuilder.Metadata.GetDeclaredIndexes())
                {
                    SetIndexFilter(index.Builder);
                }
            }

            return true;
        }

        InternalIndexBuilder IIndexAddedConvention.Apply(InternalIndexBuilder indexBuilder)
            => SetIndexFilter(indexBuilder);

        bool IIndexUniquenessChangedConvention.Apply(InternalIndexBuilder indexBuilder)
        {
            SetIndexFilter(indexBuilder);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalPropertyBuilder propertyBuilder)
        {
            foreach (var index in propertyBuilder.Metadata.GetContainingIndexes())
            {
                SetIndexFilter(index.Builder);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation Apply(InternalIndexBuilder indexBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == SqlServerAnnotationNames.Clustered)
            {
                SetIndexFilter(indexBuilder);
            }

            return annotation;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == RelationalAnnotationNames.ColumnName)
            {
                foreach (var index in propertyBuilder.Metadata.GetContainingIndexes())
                {
                    SetIndexFilter(index.Builder, columnNameChanged: true);
                }
            }

            return annotation;
        }

        private InternalIndexBuilder SetIndexFilter(InternalIndexBuilder indexBuilder, bool columnNameChanged = false)
        {
            // TODO: compare with a cached filter to avoid overriding if it was set by a different convention
            var index = indexBuilder.Metadata;
            if (index.IsUnique
                && index.SqlServer().IsClustered != true
                && index.Properties
                    .Any(property => property.IsColumnNullable()))
            {
                if (columnNameChanged
                    || index.SqlServer().Filter == null)
                {
                    indexBuilder.SqlServer(ConfigurationSource.Convention).HasFilter(CreateIndexFilter(index));
                }
            }
            else
            {
                if (index.SqlServer().Filter != null)
                {
                    indexBuilder.SqlServer(ConfigurationSource.Convention).HasFilter(null);
                }
            }

            return indexBuilder;
        }

        private string CreateIndexFilter(IIndex index)
        {
            var nullableColumns = index.Properties
                .Where(property => property.IsColumnNullable())
                .Select(property => property.SqlServer().ColumnName)
                .ToList();

            var builder = new StringBuilder();
            for (var i = 0; i < nullableColumns.Count; i++)
            {
                if (i != 0)
                {
                    builder.Append(" AND ");
                }

                builder
                    .Append(_sqlGenerationHelper.DelimitIdentifier(nullableColumns[i]))
                    .Append(" IS NOT NULL");
            }

            return builder.ToString();
        }
    }
}
