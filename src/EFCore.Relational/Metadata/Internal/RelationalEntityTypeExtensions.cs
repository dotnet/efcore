// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class RelationalEntityTypeExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public const int MaxEntityTypesSharingTable = 128;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<ITableMappingBase> GetViewOrTableMappings([NotNull] this IEntityType entityType) =>
            (IEnumerable<ITableMappingBase>)(entityType[RelationalAnnotationNames.ViewMappings]
                ?? entityType[RelationalAnnotationNames.TableMappings])
                ?? Enumerable.Empty<ITableMappingBase>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IEnumerable<IForeignKey> FindRowInternalForeignKeys(
            [NotNull] this IEntityType entityType, [CanBeNull] string name, [CanBeNull] string schema, StoreObjectType objectType)
        {
            var primaryKey = entityType.FindPrimaryKey();
            if (primaryKey == null
                || name == null)
            {
                yield break;
            }

            foreach (var fk in entityType.GetForeignKeys())
            {
                var principalEntityType = fk.PrincipalEntityType;
                if (!fk.PrincipalKey.IsPrimaryKey()
                    || principalEntityType == fk.DeclaringEntityType
                    || !fk.IsUnique
#pragma warning disable EF1001 // Internal EF Core API usage.
                    || !PropertyListComparer.Instance.Equals(fk.Properties, primaryKey.Properties))
#pragma warning restore EF1001 // Internal EF Core API usage.
                {
                    continue;
                }

                switch (objectType)
                {
                    case StoreObjectType.Table:
                        if (name == principalEntityType.GetTableName()
                            && schema == principalEntityType.GetSchema())
                        {
                            yield return fk;
                        }
                        break;
                    case StoreObjectType.View:
                        if (name == principalEntityType.GetViewName()
                            && schema == principalEntityType.GetViewSchema())
                        {
                            yield return fk;
                        }
                        break;
                    default:
                        throw new NotImplementedException(objectType.ToString());
                }
            }
        }
    }
}
