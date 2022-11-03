// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public readonly struct TypeIdentity : IEquatable<TypeIdentity>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public TypeIdentity(string name)
    {
        Name = name;
        Type = null;
        IsNamed = true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public TypeIdentity(string name, [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type)
    {
        Name = name;
        Type = type;
        IsNamed = true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    public TypeIdentity([DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)] Type type, Model model)
    {
        Name = model.GetDisplayName(type);
        Type = type;
        IsNamed = false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public string Name { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DynamicallyAccessedMembers(IEntityType.DynamicallyAccessedMemberTypes)]
    public Type? Type { [DebuggerStepThrough] get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool IsNamed { [DebuggerStepThrough] get; }

    private string DebuggerDisplay()
        => Name;

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is TypeIdentity identity && Equals(identity);

    /// <inheritdoc />
    public bool Equals(TypeIdentity other)
        => Name == other.Name
            && EqualityComparer<Type>.Default.Equals(Type, other.Type)
            && IsNamed == other.IsNamed;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Name, Type, IsNamed);

    /// <summary>
    ///     Compares one id to another id to see if they represent the same type.
    /// </summary>
    /// <param name="left">The first id.</param>
    /// <param name="right">The second id.</param>
    /// <returns><see langword="true" /> if they represent the same type; <see langword="false" /> otherwise.</returns>
    public static bool operator ==(TypeIdentity left, TypeIdentity right)
        => left.Equals(right);

    /// <summary>
    ///     Compares one id to another id to see if they represent different types.
    /// </summary>
    /// <param name="left">The first id.</param>
    /// <param name="right">The second id.</param>
    /// <returns><see langword="true" /> if they represent different types; <see langword="false" /> otherwise.</returns>
    public static bool operator !=(TypeIdentity left, TypeIdentity right)
        => !(left == right);
}
