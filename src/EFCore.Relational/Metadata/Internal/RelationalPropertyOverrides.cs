// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalPropertyOverrides : ConventionAnnotatable
    {
        private string _columnName;

        private ConfigurationSource? _columnNameConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string ColumnName
        {
            get => _columnName;
            [param: CanBeNull]
            set => SetColumnName(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string SetColumnName([CanBeNull] string columnName, ConfigurationSource configurationSource)
        {
            _columnName = columnName;
            _columnNameConfigurationSource = configurationSource.Max(_columnNameConfigurationSource);

            return columnName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetColumnNameConfigurationSource()
            => _columnNameConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationalPropertyOverrides Find([NotNull] IProperty property, in StoreObjectIdentifier storeObject)
        {
            var tableOverrides = (SortedDictionary<StoreObjectIdentifier, RelationalPropertyOverrides>)
                property[RelationalAnnotationNames.RelationalOverrides];
            return tableOverrides != null
                && tableOverrides.TryGetValue(storeObject, out var overrides)
                    ? overrides
                    : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationalPropertyOverrides GetOrCreate(
            [NotNull] IMutableProperty property,
            in StoreObjectIdentifier storeObject)
        {
            var tableOverrides = (SortedDictionary<StoreObjectIdentifier, RelationalPropertyOverrides>)
                property[RelationalAnnotationNames.RelationalOverrides];
            if (tableOverrides == null)
            {
                tableOverrides = new SortedDictionary<StoreObjectIdentifier, RelationalPropertyOverrides>();
                property[RelationalAnnotationNames.RelationalOverrides] = tableOverrides;
            }

            if (!tableOverrides.TryGetValue(storeObject, out var overrides))
            {
                overrides = new RelationalPropertyOverrides();
                tableOverrides.Add(storeObject, overrides);
            }

            return overrides;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationalPropertyOverrides GetOrCreate(
            [NotNull] IConventionProperty property,
            in StoreObjectIdentifier storeObject)
            => GetOrCreate((IMutableProperty)property, storeObject);
    }
}
