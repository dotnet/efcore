// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Represents the mapping between a .NET type and a database type, where the .NET type used in the EF model
///         is statically known.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <typeparam name="T">The .NET type used in the EF model.</typeparam>
public abstract class CoreTypeMapping<
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicMethods
        | DynamicallyAccessedMemberTypes.PublicProperties)]
    T> : CoreTypeMapping
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CoreTypeMapping{T}" /> class.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    protected CoreTypeMapping(CoreTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override ValueComparer CreateDefaultComparer(bool favorStructuralComparisons)
        => ClrType == typeof(T)
            ? ValueComparer.CreateDefault<T>(favorStructuralComparisons)
            : base.CreateDefaultComparer(favorStructuralComparisons);
}
