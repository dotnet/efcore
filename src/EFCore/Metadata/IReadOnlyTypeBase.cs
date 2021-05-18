// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a type in the model.
    /// </summary>
    public interface IReadOnlyTypeBase : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the model that this type belongs to.
        /// </summary>
        IReadOnlyModel Model { get; }

        /// <summary>
        ///     Gets the name of this type.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     <para>
        ///         Gets the CLR class that is used to represent instances of this type.
        ///         Returns <see langword="null" /> if the type does not have a corresponding CLR class (known as a shadow type).
        ///     </para>
        ///     <para>
        ///         Shadow types are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
        ///         Therefore, shadow types will only exist in migration model snapshots, etc.
        ///     </para>
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     Gets a value indicating whether this entity type is mapped to a <see cref="Type"/> that
        ///     other entity types are also mapped to.
        /// </summary>
        bool HasSharedClrType { get; }

        /// <summary>
        ///     Gets a value indicating whether this entity type has an indexer which is able to contain arbitrary properties
        ///     and a method that can be used to determine whether a given indexer property contains a value.
        /// </summary>
        bool IsPropertyBag { get; }

        /// <summary>
        ///     Gets a value indicating whether this entity type represents an abstract type.
        /// </summary>
        /// <returns> <see langword="true" /> if the type is abstract, <see langword="false" /> otherwise. </returns>
        [DebuggerStepThrough]
        bool IsAbstract() => ClrType.IsAbstract;

        /// <summary>
        ///     Gets the friendly display name for the given <see cref="IReadOnlyTypeBase" />.
        /// </summary>
        /// <returns> The display name. </returns>
        [DebuggerStepThrough]
        string DisplayName()
        {
            if (!HasSharedClrType)
            {
                return ClrType.ShortDisplayName();
            }

            var shortName = Name;
            var hashIndex = shortName.IndexOf("#", StringComparison.Ordinal);
            if (hashIndex == -1)
            {
                return Name + " (" + ClrType.ShortDisplayName() + ")";
            }

            var plusIndex = shortName.LastIndexOf("+", StringComparison.Ordinal);
            if (plusIndex != -1)
            {
                shortName = shortName[(plusIndex + 1)..];
            }
            else
            {
                var length = shortName.Length;
                var dotIndex = shortName.LastIndexOf(".", hashIndex, hashIndex + 1, StringComparison.Ordinal);
                if (dotIndex != -1)
                {
                    dotIndex = shortName.LastIndexOf(".", dotIndex - 1, dotIndex, StringComparison.Ordinal);
                    if (dotIndex != -1)
                    {
                        shortName = shortName[(dotIndex + 1)..];
                    }
                }
            }

            return shortName == Name
                       ? shortName + " (" + ClrType.ShortDisplayName() + ")"
                       : shortName;
        }

        /// <summary>
        ///     Gets a short name for the given <see cref="IReadOnlyTypeBase" /> that can be used in other identifiers.
        /// </summary>
        /// <returns> The short name. </returns>
        [DebuggerStepThrough]
        string ShortName()
        {
            if (!HasSharedClrType)
            {
                return ClrType.ShortDisplayName();
            }

            var hashIndex = Name.LastIndexOf("#", StringComparison.Ordinal);
            if (hashIndex == -1)
            {
                var plusIndex = Name.LastIndexOf("+", StringComparison.Ordinal);
                if (plusIndex == -1)
                {
                    var dotIndex = Name.LastIndexOf(".", StringComparison.Ordinal);
                    return dotIndex == -1
                            ? Name
                            : Name[(dotIndex + 1)..];
                }

                return Name[(plusIndex + 1)..];
            }

            return Name[(hashIndex + 1)..];
        }

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for properties and navigations of this type.
        ///     </para>
        ///     <para>
        ///         Note that individual properties and navigations can override this access mode. The value returned here will
        ///         be used for any property or navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <returns> The access mode being used. </returns>
        PropertyAccessMode GetPropertyAccessMode();

        /// <summary>
        ///     <para>
        ///         Gets the <see cref="PropertyAccessMode" /> being used for navigations of this type.
        ///     </para>
        ///     <para>
        ///         Note that individual navigations can override this access mode. The value returned here will
        ///         be used for any navigation for which no override has been specified.
        ///     </para>
        /// </summary>
        /// <returns> The access mode being used. </returns>
        PropertyAccessMode GetNavigationAccessMode();

        /// <summary>
        ///     Returns the <see cref="PropertyInfo"/> for the indexer on the associated CLR type if one exists.
        /// </summary>
        /// <returns> The <see cref="PropertyInfo"/> for the indexer on the associated CLR type if one exists. </returns>
        PropertyInfo? FindIndexerPropertyInfo();
    }
}
