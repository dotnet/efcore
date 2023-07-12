// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Configures the navigation property on the dependent side of a relationship
///     to indicate how a delete operation is applied to dependent entities
///     in a relationship when it is deleted or the relationship is severed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DeleteBehaviorAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteBehaviorAttribute" /> class.
    /// </summary>
    /// <param name="behavior">The <see cref="DeleteBehavior" /> to be configured.</param>
    public DeleteBehaviorAttribute(DeleteBehavior behavior)
    {
        Behavior = behavior;
    }

    /// <summary>
    ///     Gets the <see cref="DeleteBehavior" /> to be configured.
    /// </summary>
    public DeleteBehavior Behavior { get; }
}
