// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents an operation that should be performed when a trigger is removed from the entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public interface ITriggerRemovedConvention : IConvention
{
    /// <summary>
    ///     Called after a trigger is removed from the entity type.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type that contained the trigger.</param>
    /// <param name="trigger">The removed trigger.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    void ProcessTriggerRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionTrigger trigger,
        IConventionContext<IConventionTrigger> context);
}
