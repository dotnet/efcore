// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that configures the filter for unique non-clustered indexes with nullable columns
    ///     to filter out null values.
    /// </summary>
    public class SqlServerIndexConvention :
        IEntityTypeBaseTypeChangedConvention,
        IIndexAddedConvention,
        IIndexUniquenessChangedConvention,
        IIndexAnnotationChangedConvention,
        IPropertyNullabilityChangedConvention,
        IPropertyAnnotationChangedConvention
    {
        private readonly ISqlGenerationHelper _sqlGenerationHelper;

        /// <summary>
        ///     Creates a new instance of <see cref="SqlServerIndexConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        /// <param name="sqlGenerationHelper"> SQL command generation helper service. </param>
        public SqlServerIndexConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies,
            ISqlGenerationHelper sqlGenerationHelper)
        {
            _sqlGenerationHelper = sqlGenerationHelper;
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after the base type of an entity type changes.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type. </param>
        /// <param name="newBaseType"> The new base entity type. </param>
        /// <param name="oldBaseType"> The old base entity type. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType? newBaseType,
            IConventionEntityType? oldBaseType,
            IConventionContext<IConventionEntityType> context)
        {
            if (oldBaseType == null
                || newBaseType == null)
            {
                foreach (var index in entityTypeBuilder.Metadata.GetDeclaredIndexes())
                {
                    SetIndexFilter(index.Builder);
                }
            }
        }

        /// <summary>
        ///     Called after an index is added to the entity type.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessIndexAdded(
            IConventionIndexBuilder indexBuilder,
            IConventionContext<IConventionIndexBuilder> context)
            => SetIndexFilter(indexBuilder);

        /// <summary>
        ///     Called after the uniqueness for an index is changed.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessIndexUniquenessChanged(
            IConventionIndexBuilder indexBuilder,
            IConventionContext<bool?> context)
            => SetIndexFilter(indexBuilder);

        /// <summary>
        ///     Called after the nullability for a property is changed.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyNullabilityChanged(
            IConventionPropertyBuilder propertyBuilder,
            IConventionContext<bool?> context)
        {
            foreach (var index in propertyBuilder.Metadata.GetContainingIndexes())
            {
                SetIndexFilter(index.Builder);
            }
        }

        /// <summary>
        ///     Called after an annotation is changed on an index.
        /// </summary>
        /// <param name="indexBuilder"> The builder for the index. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessIndexAnnotationChanged(
            IConventionIndexBuilder indexBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == SqlServerAnnotationNames.Clustered)
            {
                SetIndexFilter(indexBuilder);
            }
        }

        /// <summary>
        ///     Called after an annotation is changed on a property.
        /// </summary>
        /// <param name="propertyBuilder"> The builder for the property. </param>
        /// <param name="name"> The annotation name. </param>
        /// <param name="annotation"> The new annotation. </param>
        /// <param name="oldAnnotation"> The old annotation.  </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation,
            IConventionContext<IConventionAnnotation> context)
        {
            if (name == RelationalAnnotationNames.ColumnName)
            {
                foreach (var index in propertyBuilder.Metadata.GetContainingIndexes())
                {
                    SetIndexFilter(index.Builder, columnNameChanged: true);
                }
            }
        }

        private IConventionIndexBuilder SetIndexFilter(IConventionIndexBuilder indexBuilder, bool columnNameChanged = false)
        {
            var index = indexBuilder.Metadata;
            if (index.IsUnique
                && index.IsClustered() != true
                && GetNullableColumns(index) is List<string> nullableColumns
                && nullableColumns.Count > 0)
            {
                if (columnNameChanged
                    || index.GetFilter() == null)
                {
                    indexBuilder.HasFilter(CreateIndexFilter(nullableColumns));
                }
            }
            else
            {
                if (index.GetFilter() != null)
                {
                    indexBuilder.HasFilter(null);
                }
            }

            return indexBuilder;
        }

        private string CreateIndexFilter(List<string> nullableColumns)
        {
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

        private List<string>? GetNullableColumns(IReadOnlyIndex index)
        {
            var tableName = index.DeclaringEntityType.GetTableName();
            if (tableName == null)
            {
                return null;
            }

            var nullableColumns = new List<string>();
            var table = StoreObjectIdentifier.Table(tableName, index.DeclaringEntityType.GetSchema());
            foreach (var property in index.Properties)
            {
                var columnName = property.GetColumnName(table);
                if (columnName == null)
                {
                    return null;
                }

                if (!property.IsColumnNullable(table))
                {
                    continue;
                }

                nullableColumns.Add(columnName);
            }

            return nullableColumns;
        }
    }
}
