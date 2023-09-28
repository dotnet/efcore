// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

/// <summary>
///     A simple model for a database trigger used when reverse engineering an existing database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-scaffolding">Reverse engineering (scaffolding) an existing database</see>, and
///     <see href="https://aka.ms/efcore-docs-design-time-services">EF Core design-time services</see> for more information and examples.
/// </remarks>
public class DatabaseTrigger : Annotatable
{
    /// <summary>
    ///     The trigger name.
    /// </summary>
    public virtual string Name { get; set; } = null!;
}
