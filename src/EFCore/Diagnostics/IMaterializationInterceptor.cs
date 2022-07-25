// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Diagnostics;

/// <summary>
///     A <see cref="ISingletonInterceptor" /> used to intercept the various parts of object creation and initialization when
///     Entity Framework is creating an object, typically from data returned by a query.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-interceptors">EF Core interceptors</see> for more information and examples.
/// </remarks>
public interface IMaterializationInterceptor : ISingletonInterceptor
{
    /// <summary>
    ///     Called immediately before EF is going to create an instance of an entity. That is, before the constructor has been called.
    /// </summary>
    /// <param name="materializationData">Contextual information about the materialization happening.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult{Object}.HasResult" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult{Object}.SuppressWithResult" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult{Object}.HasResult" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult{Object}.HasResult" /> is <see langword="true" />, then EF will suppress creation of
    ///     the entity instance and use <see cref="InterceptionResult{Object}.Result" /> instead.
    ///     An implementation of this method for any interceptor that is not attempting to change the result
    ///     should return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult<object> CreatingInstance(MaterializationInterceptionData materializationData, InterceptionResult<object> result)
        => result;

    /// <summary>
    ///     Called immediately after EF has created an instance of an entity. That is, after the constructor has been called, but before
    ///     any properties values not set by the constructor have been set.
    /// </summary>
    /// <param name="materializationData">Contextual information about the materialization happening.</param>
    /// <param name="entity">
    ///     The entity instance that has been created.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The entity instance that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the instance used
    ///     must return the <paramref name="entity" /> value passed in.
    /// </returns>
    object CreatedInstance(MaterializationInterceptionData materializationData, object entity)
        => entity;

    /// <summary>
    ///     Called immediately before EF is going to set property values of an entity that has just been created. Note that property values
    ///     set by the constructor will already have been set.
    /// </summary>
    /// <param name="materializationData">Contextual information about the materialization happening.</param>
    /// <param name="entity">The entity instance for which property values will be set.</param>
    /// <param name="result">
    ///     Represents the current result if one exists.
    ///     This value will have <see cref="InterceptionResult.IsSuppressed" /> set to <see langword="true" /> if some previous
    ///     interceptor suppressed execution by calling <see cref="InterceptionResult.Suppress" />.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="false" />, then EF will continue as normal.
    ///     If <see cref="InterceptionResult.IsSuppressed" /> is <see langword="true" />, then EF will not set any property values.
    ///     An implementation of this method for any interceptor that is not attempting to suppress
    ///     setting property values must return the <paramref name="result" /> value passed in.
    /// </returns>
    InterceptionResult InitializingInstance(MaterializationInterceptionData materializationData, object entity, InterceptionResult result)
        => result;

    /// <summary>
    ///     Called immediately after EF has set property values of an entity that has just been created.
    /// </summary>
    /// <param name="materializationData">Contextual information about the materialization happening.</param>
    /// <param name="entity">
    ///     The entity instance that has been created.
    ///     This value is typically used as the return value for the implementation of this method.
    /// </param>
    /// <returns>
    ///     The entity instance that EF will use.
    ///     An implementation of this method for any interceptor that is not attempting to change the instance used
    ///     must return the <paramref name="entity" /> value passed in.
    /// </returns>
    object InitializedInstance(MaterializationInterceptionData materializationData, object entity)
        => entity;
}
