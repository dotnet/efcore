// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Configures the class to indicate how a delete operation is applied to dependent entities
///     in a relationship when it is deleted or the relationship is severed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DeleteBehaviorAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteBehaviorAttribute" /> class.
    /// </summary>
    /// <param name="behavior">The DeleteBehavior value of entity</param>
    public DeleteBehaviorAttribute(int behavior)
    {
        if ( behavior < 0 || behavior > 6) // Valid values for DeleteBehavior enum
        {
            throw new ArgumentException("This behavior is not defined in DeleteBehavior Enum.");
        }

        Behavior = behavior;
    }
    
    /// <summary>
    ///     The DeleteBehavior value
    /// </summary>
    public int Behavior { get; }
}
