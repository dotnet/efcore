// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableDbFunction" /> that an entity type is mapped to.
/// </summary>
public class OwnedNavigationTableValuedFunctionBuilder : DbFunctionBuilderBase, IInfrastructure<OwnedNavigationBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public OwnedNavigationTableValuedFunctionBuilder(
        IMutableDbFunction function,
        OwnedNavigationBuilder ownedNavigationBuilder)
        : base(function)
    {
        OwnedNavigationBuilder = ownedNavigationBuilder;
    }

    private OwnedNavigationBuilder OwnedNavigationBuilder { get; }

    /// <summary>
    ///     Sets the name of the database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the function in the database.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationTableValuedFunctionBuilder HasName(string name)
        => (OwnedNavigationTableValuedFunctionBuilder)base.HasName(name);

    /// <summary>
    ///     Sets the schema of the database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="schema">The schema of the function in the database.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationTableValuedFunctionBuilder HasSchema(string? schema)
        => (OwnedNavigationTableValuedFunctionBuilder)base.HasSchema(schema);

    /// <summary>
    ///     Marks whether the database function is built-in.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="builtIn">The value indicating whether the database function is built-in.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationTableValuedFunctionBuilder IsBuiltIn(bool builtIn = true)
        => (OwnedNavigationTableValuedFunctionBuilder)base.IsBuiltIn(builtIn);

    /// <summary>
    ///     Returns an object that can be used to configure a parameter with the given name.
    ///     If no parameter with the given name exists, then a new parameter will be added.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The parameter name.</param>
    /// <param name="buildAction">An action that performs configuration of the parameter.</param>
    /// <returns>The builder to use for further parameter configuration.</returns>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public new virtual OwnedNavigationTableValuedFunctionBuilder HasParameter(string name, Action<DbFunctionParameterBuilder> buildAction)
        => (OwnedNavigationTableValuedFunctionBuilder)base.HasParameter(name, buildAction);

    /// <summary>
    ///     Adds or updates an annotation on the database function. If an annotation with the key specified in
    ///     <paramref name="annotation" /> already exists, its value will be updated.
    /// </summary>
    /// <param name="annotation">The key of the annotation to be added or updated.</param>
    /// <param name="value">The value to be stored in the annotation.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual OwnedNavigationTableValuedFunctionBuilder HasAnnotation(string annotation, object? value)
    {
        Check.NotEmpty(annotation, nameof(annotation));

        Builder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);

        return this;
    }

    OwnedNavigationBuilder IInfrastructure<OwnedNavigationBuilder>.Instance
        => OwnedNavigationBuilder;
}
