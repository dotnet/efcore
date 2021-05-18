// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalPropertyOverrides : ConventionAnnotatable, IRelationalPropertyOverrides
    {
        private string? _columnName;

        private ConfigurationSource? _columnNameConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalPropertyOverrides(IReadOnlyProperty property)
        {
            Property = property;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IReadOnlyProperty Property { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool IsReadOnly => ((Annotatable)Property).IsReadOnly;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? ColumnName
        {
            get => _columnName;
            set => SetColumnName(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string? SetColumnName(string? columnName, ConfigurationSource configurationSource)
        {
            EnsureMutable();

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
        public virtual bool ColumnNameOverriden
            => _columnNameConfigurationSource != null;

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
        public static IRelationalPropertyOverrides? Find(IReadOnlyProperty property, in StoreObjectIdentifier storeObject)
        {
            var tableOverrides = (SortedDictionary<StoreObjectIdentifier, object>?)
                property[RelationalAnnotationNames.RelationalOverrides];
            return tableOverrides != null
                && tableOverrides.TryGetValue(storeObject, out var overrides)
                    ? (IRelationalPropertyOverrides)overrides
                    : null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationalPropertyOverrides GetOrCreate(
            IMutableProperty property,
            in StoreObjectIdentifier storeObject)
        {
            var tableOverrides = (SortedDictionary<StoreObjectIdentifier, object>?)
                property[RelationalAnnotationNames.RelationalOverrides];
            if (tableOverrides == null)
            {
                tableOverrides = new SortedDictionary<StoreObjectIdentifier, object>();
                property[RelationalAnnotationNames.RelationalOverrides] = tableOverrides;
            }

            if (!tableOverrides.TryGetValue(storeObject, out var overrides))
            {
                overrides = new RelationalPropertyOverrides(property);
                tableOverrides.Add(storeObject, overrides);
            }

            return (RelationalPropertyOverrides)overrides;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationalPropertyOverrides GetOrCreate(
            IConventionProperty property,
            in StoreObjectIdentifier storeObject)
            => GetOrCreate((IMutableProperty)property, storeObject);

        /// <inheritdoc />
        IProperty IRelationalPropertyOverrides.Property
        {
            [DebuggerStepThrough]
            get => (IProperty)Property;
        }
    }
}
