// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class SqlServerAnnotationProvider : RelationalAnnotationProvider
    {
        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public SqlServerAnnotationProvider([NotNull] RelationalAnnotationProviderDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IRelationalModel model)
        {
            var maxSize = model.Model.GetDatabaseMaxSize();
            var serviceTier = model.Model.GetServiceTierSql();
            var performanceLevel = model.Model.GetPerformanceLevelSql();
            if (maxSize != null
                || serviceTier != null
                || performanceLevel != null)
            {
                var options = new StringBuilder();

                if (maxSize != null)
                {
                    options.Append("MAXSIZE = ");
                    options.Append(maxSize);
                    options.Append(", ");
                }

                if (serviceTier != null)
                {
                    options.Append("EDITION = ");
                    options.Append(serviceTier);
                    options.Append(", ");
                }

                if (performanceLevel != null)
                {
                    options.Append("SERVICE_OBJECTIVE = ");
                    options.Append(performanceLevel);
                    options.Append(", ");
                }

                options.Remove(options.Length - 2, 2);

                yield return new Annotation(SqlServerAnnotationNames.EditionOptions, options.ToString());
            }

            if (model.Tables.Any(t => !t.IsExcludedFromMigrations && (t[SqlServerAnnotationNames.MemoryOptimized] as bool? == true)))
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.MemoryOptimized,
                    true);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEnumerable<IAnnotation> For(ITable table)
        {
            // Model validation ensures that these facets are the same on all mapped entity types
            if (table.EntityTypeMappings.First().EntityType.IsMemoryOptimized())
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.MemoryOptimized,
                    true);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IUniqueConstraint constraint)
        {
            // Model validation ensures that these facets are the same on all mapped keys
            var key = constraint.MappedKeys.First();

            var table = constraint.Table;
            var isClustered = key.IsClustered(StoreObjectIdentifier.Table(table.Name, table.Schema));
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.Clustered,
                    isClustered.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEnumerable<IAnnotation> For(ITableIndex index)
        {
            // Model validation ensures that these facets are the same on all mapped indexes
            var modelIndex = index.MappedIndexes.First();

            var table = index.Table;
            var isClustered = modelIndex.IsClustered(StoreObjectIdentifier.Table(table.Name, table.Schema));
            if (isClustered.HasValue)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.Clustered,
                    isClustered.Value);
            }

            var includeProperties = modelIndex.GetIncludeProperties();
            if (includeProperties != null)
            {
                var includeColumns = (IReadOnlyList<string>)includeProperties
                    .Select(
                        p => modelIndex.DeclaringEntityType.FindProperty(p)
                            .GetColumnName(StoreObjectIdentifier.Table(table.Name, table.Schema)))
                    .ToArray();

                yield return new Annotation(
                    SqlServerAnnotationNames.Include,
                    includeColumns);
            }

            var isOnline = modelIndex.IsCreatedOnline();
            if (isOnline.HasValue)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.CreatedOnline,
                    isOnline.Value);
            }

            var fillFactor = modelIndex.GetFillFactor();
            if (fillFactor.HasValue)
            {
                yield return new Annotation(
                    SqlServerAnnotationNames.FillFactor,
                    fillFactor.Value);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IEnumerable<IAnnotation> For(IColumn column)
        {
            var table = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);
            var property = column.PropertyMappings.Where(
                    m =>
                        m.TableMapping.IsSharedTablePrincipal && m.TableMapping.EntityType == m.Property.DeclaringEntityType)
                .Select(m => m.Property)
                .FirstOrDefault(
                    p => p.GetValueGenerationStrategy(table)
                        == SqlServerValueGenerationStrategy.IdentityColumn);
            if (property != null)
            {
                var seed = property.GetIdentitySeed(table);
                var increment = property.GetIdentityIncrement(table);

                yield return new Annotation(
                    SqlServerAnnotationNames.Identity,
                    string.Format(CultureInfo.InvariantCulture, "{0}, {1}", seed ?? 1, increment ?? 1));
            }
        }
    }
}
