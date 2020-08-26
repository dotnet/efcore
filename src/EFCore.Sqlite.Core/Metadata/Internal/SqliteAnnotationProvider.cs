// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal
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
    public class SqliteAnnotationProvider : RelationalAnnotationProvider
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqliteAnnotationProvider([NotNull] RelationalAnnotationProviderDependencies dependencies)
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
            if (model.Tables.SelectMany(t => t.Columns).Any(
                c => SqliteTypeMappingSource.IsSpatialiteType(c.StoreType)))
            {
                yield return new Annotation(SqliteAnnotationNames.InitSpatialMetaData, true);
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
            // Model validation ensures that these facets are the same on all mapped properties
            var property = column.PropertyMappings.First().Property;
            // Only return auto increment for integer single column primary key
            var primaryKey = property.DeclaringEntityType.FindPrimaryKey();
            if (primaryKey != null
                && primaryKey.Properties.Count == 1
                && primaryKey.Properties[0] == property
                && property.ValueGenerated == ValueGenerated.OnAdd
                && property.ClrType.UnwrapNullableType().IsInteger()
                && !HasConverter(property))
            {
                yield return new Annotation(SqliteAnnotationNames.Autoincrement, true);
            }

            var srid = property.GetSrid();
            if (srid != null)
            {
                yield return new Annotation(SqliteAnnotationNames.Srid, srid);
            }
        }

        private static bool HasConverter(IProperty property)
            => (property.GetValueConverter() ?? property.FindTypeMapping()?.Converter) != null;
    }
}
