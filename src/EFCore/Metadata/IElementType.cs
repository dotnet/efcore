// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents the elements of a collection property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IElementType : IReadOnlyElementType, IAnnotatable
{
    /// <summary>
    ///     Gets the collection property for which this represents the element.
    /// </summary>
    new IProperty CollectionProperty
    {
        [DebuggerStepThrough]
        get => (IProperty)((IReadOnlyElementType)this).CollectionProperty;
    }
}
