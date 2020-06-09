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
        private string _viewColumnName;

        private ConfigurationSource? _columnNameConfigurationSource;
        private ConfigurationSource? _viewColumnNameConfigurationSource;

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
        public virtual void ClearColumnName()
        {
            _columnName = null;
            _columnNameConfigurationSource = null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetColumnNameConfigurationSource() => _columnNameConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string ViewColumnName
        {
            get => _viewColumnName;
            [param: CanBeNull]
            set => SetViewColumnName(value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string SetViewColumnName([CanBeNull] string columnName, ConfigurationSource configurationSource)
        {
            _viewColumnName = columnName;
            _viewColumnNameConfigurationSource = configurationSource.Max(_viewColumnNameConfigurationSource);

            return columnName;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ClearViewColumnName()
        {
            _viewColumnName = null;
            _viewColumnNameConfigurationSource = null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConfigurationSource? GetViewColumnNameConfigurationSource() => _viewColumnNameConfigurationSource;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static RelationalPropertyOverrides Find(
            [NotNull] IProperty property, [NotNull] string tableName, [CanBeNull] string schema)
        {
            var tableOverrides = (SortedDictionary<(string Table, string Schema), RelationalPropertyOverrides>)
                property[RelationalAnnotationNames.RelationalOverrides];
            return tableOverrides != null
                && tableOverrides.TryGetValue((tableName, schema), out var overrides)
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
            [NotNull] IMutableProperty property, [NotNull] string tableName, [CanBeNull] string schema)
        {
            var tableOverrides = (SortedDictionary<(string Table, string Schema), RelationalPropertyOverrides>)
                property[RelationalAnnotationNames.RelationalOverrides];
            if (tableOverrides == null)
            {
                tableOverrides = new SortedDictionary<(string Table, string Schema), RelationalPropertyOverrides>();
                property[RelationalAnnotationNames.RelationalOverrides] = tableOverrides;
            }

            if (!tableOverrides.TryGetValue((tableName, schema), out var overrides))
            {
                overrides = new RelationalPropertyOverrides();
                tableOverrides.Add((tableName, schema), overrides);
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
            [NotNull] IConventionProperty property, [NotNull] string tableName, [CanBeNull] string schema)
            => GetOrCreate((IMutableProperty)property, tableName, schema);
    }
}
