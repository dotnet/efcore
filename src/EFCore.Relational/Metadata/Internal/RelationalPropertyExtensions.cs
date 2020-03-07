// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static class RelationalPropertyExtensions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IForeignKey FindSharedObjectLink(
            [NotNull] this IProperty property,
            StoreObjectType objectType = StoreObjectType.Table)
        {
            switch (objectType)
            {
                case StoreObjectType.Table:
                    return property.FindSharedObjectLink(
                        property.DeclaringEntityType.GetTableName(),
                        property.DeclaringEntityType.GetSchema(),
                        objectType);
                case StoreObjectType.View:
                    return property.FindSharedObjectLink(
                        property.DeclaringEntityType.GetViewName(),
                        property.DeclaringEntityType.GetViewSchema(),
                        objectType);
                default:
                    throw new NotImplementedException(objectType.ToString());
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static IForeignKey FindSharedObjectLink(
            [NotNull] this IProperty property,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            StoreObjectType objectType = StoreObjectType.Table)
        {
            var pk = property.FindContainingPrimaryKey();
            if (pk == null
                || name == null)
            {
                return null;
            }

            var entityType = property.DeclaringEntityType;
            foreach (var fk in entityType.FindForeignKeys(pk.Properties))
            {
                if (!fk.PrincipalKey.IsPrimaryKey()
                    || fk.PrincipalEntityType == fk.DeclaringEntityType
                    || !fk.IsUnique)
                {
                    continue;
                }

                var principalEntityType = fk.PrincipalEntityType;
                switch (objectType)
                {
                    case StoreObjectType.Table:
                        if (name == principalEntityType.GetTableName()
                            && schema == principalEntityType.GetSchema())
                        {
                            return fk;
                        }
                        break;
                    case StoreObjectType.View:
                        if (name == principalEntityType.GetViewName()
                            && schema == principalEntityType.GetViewSchema())
                        {
                            return fk;
                        }
                        break;
                    default:
                        throw new NotImplementedException(objectType.ToString());
                }
            }

            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        public static string GetConfiguredColumnType([NotNull] this IProperty property)
            => (string)property[RelationalAnnotationNames.ColumnType];
    }
}
