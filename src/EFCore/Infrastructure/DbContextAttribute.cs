// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Identifies the <see cref="DbContext" /> that a class belongs to. For example, this attribute is used
///     to identify which context a migration applies to. It is also used to indicate the contexts used in an assembly
///     for design-time tools.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-manage-schemas">Managing database schemas with EF Core</see> for more information and
///     examples.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class DbContextAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbContextAttribute" /> class.
    /// </summary>
    /// <param name="contextType">The associated context.</param>
    public DbContextAttribute(Type contextType)
    {
        Check.NotNull(contextType, nameof(contextType));

        ContextType = contextType;
    }

    /// <summary>
    ///     Gets the associated context.
    /// </summary>
    public Type ContextType { get; }
}
