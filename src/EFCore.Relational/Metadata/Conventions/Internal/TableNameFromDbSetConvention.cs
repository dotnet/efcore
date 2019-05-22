// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class TableNameFromDbSetConvention : IBaseTypeChangedConvention
    {
        private readonly IDictionary<Type, DbSetProperty> _sets;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public TableNameFromDbSetConvention(
            [CanBeNull] DbContext context,
            [CanBeNull] IDbSetFinder setFinder,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            _sets = context != null
                    && setFinder != null
                ? setFinder.CreateClrTypeDbSetMapping(context)
                : new Dictionary<Type, DbSetProperty>();

            Logger = logger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual IDiagnosticsLogger<DbLoggerCategory.Model> Logger { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            if (_sets != null)
            {
                var entityType = entityTypeBuilder.Metadata;

                if (oldBaseType == null
                    && entityType.BaseType != null)
                {
                    entityTypeBuilder.ToTable(null);
                }
                else if (oldBaseType != null
                         && entityType.BaseType == null
                         && _sets.ContainsKey(entityType.ClrType))
                {
                    entityTypeBuilder.ToTable(_sets[entityType.ClrType].Name);
                }
            }

            return true;
        }
    }
}
