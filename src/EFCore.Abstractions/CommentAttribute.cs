// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Marks a class, property or field with a comment to be set on the corresponding database table or column.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
public sealed class CommentAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CommentAttribute" /> class.
    /// </summary>
    /// <param name="comment">The comment.</param>
    public CommentAttribute(string comment)
    {
        Check.NotEmpty(comment, nameof(comment));

        Comment = comment;
    }

    /// <summary>
    ///     The comment to be configured.
    /// </summary>
    public string Comment { get; }
}
