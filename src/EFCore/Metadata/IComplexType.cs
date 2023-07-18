// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the type of a complex property of a structural type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IComplexType : IReadOnlyComplexType, ITypeBase
{
    /// <summary>
    ///     Gets the associated property.
    /// </summary>
    new IComplexProperty ComplexProperty { get; }

    /// <summary>
    ///     Gets the entity type on which the complex property chain is declared.
    /// </summary>
    IEntityType ITypeBase.ContainingEntityType
        => (IEntityType)((IReadOnlyComplexType)this).ContainingEntityType;
}
