// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableDbFunction" /> that an entity type is mapped to.
/// </summary>
public class TableValuedFunctionBuilder : DbFunctionBuilderBase, IInfrastructure<EntityTypeBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public TableValuedFunctionBuilder(IMutableDbFunction function, EntityTypeBuilder entityTypeBuilder)
        : base(function)
    {
        EntityTypeBuilder = entityTypeBuilder;
    }

    private EntityTypeBuilder EntityTypeBuilder { get; }

    /// <summary>
    ///     Sets the name of the database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the function in the database.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual TableValuedFunctionBuilder HasName(string name)
        => (TableValuedFunctionBuilder)base.HasName(name);

    /// <summary>
    ///     Sets the schema of the database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="schema">The schema of the function in the database.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual TableValuedFunctionBuilder HasSchema(string? schema)
        => (TableValuedFunctionBuilder)base.HasSchema(schema);
    
    /// <summary>
    ///     Marks whether the database function is built-in.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="builtIn">The value indicating whether the database function is built-in.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual TableValuedFunctionBuilder IsBuiltIn(bool builtIn = true)
        => (TableValuedFunctionBuilder)base.IsBuiltIn(builtIn);

    EntityTypeBuilder IInfrastructure<EntityTypeBuilder>.Instance => EntityTypeBuilder;
}
