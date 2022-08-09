// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a type in the model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
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
    ///     Gets the CLR class that is used to represent instances of this type.
    ///     Returns <see langword="null" /> if the type does not have a corresponding CLR class (known as a shadow type).
    /// </summary>
    /// <remarks>
    ///     Shadow types are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
    ///     Therefore, shadow types will only exist in migration model snapshots, etc.
    /// </remarks>
    Type ClrType { get; }

    /// <summary>
    ///     Gets a value indicating whether this entity type is mapped to a <see cref="Type" /> that
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
    /// <returns><see langword="true" /> if the type is abstract, <see langword="false" /> otherwise.</returns>
    [DebuggerStepThrough]
    bool IsAbstract()
        => ClrType.IsAbstract;

    /// <summary>
    ///     Gets the friendly display name for the given <see cref="IReadOnlyTypeBase" />.
    /// </summary>
    /// <returns>The display name.</returns>
    [DebuggerStepThrough]
    string DisplayName()
        => DisplayName(omitSharedType: false);

    /// <summary>
    ///     Gets the friendly display name for the given <see cref="IReadOnlyTypeBase" />.
    /// </summary>
    /// <param name="omitSharedType">
    ///     A value indicating whether the name of the type for shared type entity types should be omitted from the returned value.
    /// </param>
    /// <returns>The display name.</returns>
    [DebuggerStepThrough]
    string DisplayName(bool omitSharedType)
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
    /// <returns>The short name.</returns>
    [DebuggerStepThrough]
    string ShortName()
    {
        if (!HasSharedClrType)
        {
            var name = ClrType.ShortDisplayName();
            var lessIndex = name.IndexOf("<", StringComparison.Ordinal);
            if (lessIndex == -1)
            {
                return name;
            }

            return name[..lessIndex];
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
    ///     Gets the <see cref="PropertyAccessMode" /> being used for properties and navigations of this type.
    /// </summary>
    /// <remarks>
    ///     Note that individual properties and navigations can override this access mode. The value returned here will
    ///     be used for any property or navigation for which no override has been specified.
    /// </remarks>
    /// <returns>The access mode being used.</returns>
    PropertyAccessMode GetPropertyAccessMode();

    /// <summary>
    ///     Gets the <see cref="PropertyAccessMode" /> being used for navigations of this type.
    /// </summary>
    /// <remarks>
    ///     Note that individual navigations can override this access mode. The value returned here will
    ///     be used for any navigation for which no override has been specified.
    /// </remarks>
    /// <returns>The access mode being used.</returns>
    PropertyAccessMode GetNavigationAccessMode();

    /// <summary>
    ///     Returns the <see cref="PropertyInfo" /> for the indexer on the associated CLR type if one exists.
    /// </summary>
    /// <returns>The <see cref="PropertyInfo" /> for the indexer on the associated CLR type if one exists.</returns>
    PropertyInfo? FindIndexerPropertyInfo();
}
