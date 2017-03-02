// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class SqlServerIndexConvention :
        IIndexConvention,
        IIndexUniquenessConvention,
        IIndexAnnotationSetConvention,
        IPropertyNullableConvention,
        IPropertyAnnotationSetConvention
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        public SqlServerIndexConvention([NotNull] ISqlGenerationHelper sqlGenerationHelper)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
        }

        InternalIndexBuilder IIndexConvention.Apply(InternalIndexBuilder indexBuilder)
            => SetIndexFilter(indexBuilder);

        bool IIndexUniquenessConvention.Apply(InternalIndexBuilder indexBuilder)
        {
            SetIndexFilter(indexBuilder);
            return true;
        }

        public virtual bool Apply(InternalPropertyBuilder propertyBuilder)
        {
            foreach (var index in propertyBuilder.Metadata.GetContainingIndexes())
            {
                SetIndexFilter(index.Builder);
            }
            return true;
        }

        public virtual Annotation Apply(InternalIndexBuilder indexBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == SqlServerFullAnnotationNames.Instance.Clustered)
            {
                SetIndexFilter(indexBuilder);
            }

            return annotation;
        }

        public virtual Annotation Apply(InternalPropertyBuilder propertyBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            if (name == SqlServerFullAnnotationNames.Instance.ColumnName
                || (name == RelationalFullAnnotationNames.Instance.ColumnName
                    && propertyBuilder.Metadata.FindAnnotation(SqlServerFullAnnotationNames.Instance.ColumnName) == null))
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
                && indexBuilder.Metadata.SqlServer().IsClustered != true
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
