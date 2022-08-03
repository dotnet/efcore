// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides a simple API for configuring a <see cref="IMutableDbFunction" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public abstract class DbFunctionBuilderBase : IInfrastructure<IConventionDbFunctionBuilder>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected DbFunctionBuilderBase(IMutableDbFunction function)
    {
        Builder = ((DbFunction)function).Builder;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalDbFunctionBuilder Builder { [DebuggerStepThrough] get; }

    /// <inheritdoc />
    IConventionDbFunctionBuilder IInfrastructure<IConventionDbFunctionBuilder>.Instance
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     The function being configured.
    /// </summary>
    public virtual IMutableDbFunction Metadata
        => Builder.Metadata;

    /// <summary>
    ///     Sets the name of the database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The name of the function in the database.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual DbFunctionBuilderBase HasName(string name)
    {
        Check.NullButNotEmpty(name, nameof(name));

        Builder.HasName(name, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Sets the schema of the database function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="schema">The schema of the function in the database.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual DbFunctionBuilderBase HasSchema(string? schema)
    {
        Builder.HasSchema(schema, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Marks whether the database function is built-in.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="builtIn">The value indicating whether the database function is built-in.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual DbFunctionBuilderBase IsBuiltIn(bool builtIn = true)
    {
        Builder.IsBuiltIn(builtIn, ConfigurationSource.Explicit);

        return this;
    }

    /// <summary>
    ///     Returns an object that can be used to configure a parameter with the given name.
    ///     If no parameter with the given name exists, then a new parameter will be added.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The parameter name.</param>
    /// <returns>The builder to use for further parameter configuration.</returns>
    public virtual DbFunctionParameterBuilder HasParameter(string name)
        => new(Builder.HasParameter(name, ConfigurationSource.Explicit).Metadata);

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
    public virtual DbFunctionBuilderBase HasParameter(string name, Action<DbFunctionParameterBuilder> buildAction)
    {
        buildAction(HasParameter(name));
        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
