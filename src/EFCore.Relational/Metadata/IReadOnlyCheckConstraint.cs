// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a check constraint on the entity type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information and examples.
    /// </remarks>
    public interface IReadOnlyCheckConstraint : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the name of the check constraint in the model.
        /// </summary>
        string ModelName { get; }

        /// <summary>
        ///     Gets the database name of the check constraint.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Returns the default database name that would be used for this check constraint.
        /// </summary>
        /// <returns>The default name that would be used for this check constraint.</returns>
        string? GetDefaultName()
        {
            var table = StoreObjectIdentifier.Create(EntityType, StoreObjectType.Table);
            return !table.HasValue ? null : GetDefaultName(table.Value);
        }

        /// <summary>
        ///     Gets the database name of the check constraint.
        /// </summary>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The database name of the check constraint for the given store object.</returns>
        string? GetName(in StoreObjectIdentifier storeObject);

        /// <summary>
        ///     Returns the default database name that would be used for this check constraint.
        /// </summary>
        /// <param name="storeObject">The identifier of the store object.</param>
        /// <returns>The default name that would be used for this check constraint.</returns>
        string GetDefaultName(in StoreObjectIdentifier storeObject)
        {
            var prefix = $"CK_{storeObject.Name}_";
            return Uniquifier.Truncate(
                ModelName.StartsWith(prefix, StringComparison.Ordinal)
                    ? ModelName
                    : prefix + ModelName,
                EntityType.Model.GetMaxIdentifierLength());
        }

        /// <summary>
        ///     Gets the entity type on which this check constraint is defined.
        /// </summary>
        IReadOnlyEntityType EntityType { get; }

        /// <summary>
        ///     Gets the constraint sql used in a check constraint in the database.
        /// </summary>
        string Sql { get; }
    }
}
