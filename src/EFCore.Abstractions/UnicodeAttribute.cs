// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Configures the property as capable of persisting unicode characters.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UnicodeAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UnicodeAttribute" /> class.
    /// </summary>
    /// <param name="unicode">A value indicating whether the property can contain unicode characters or not.</param>
    public UnicodeAttribute(bool unicode = true)
    {
        IsUnicode = unicode;
    }

    /// <summary>
    ///     A value indicating whether the property can contain unicode characters or not.
    /// </summary>
    public bool IsUnicode { get; }
}
