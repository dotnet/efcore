// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Extension methods for the <see cref="ILazyLoader" /> service that make it more
///     convenient to use from entity classes.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
/// </remarks>
public static class LazyLoaderExtensions
{
    /// <summary>
    ///     Loads a navigation property if it has not already been loaded.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-lazy-loading">Lazy loading</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TRelated">The type of the navigation property.</typeparam>
    /// <param name="loader">The loader instance, which may be <see langword="null" />.</param>
    /// <param name="entity">The entity on which the navigation property is located.</param>
    /// <param name="navigationField">A reference to the backing field for the navigation.</param>
    /// <param name="navigationName">The navigation property name.</param>
    /// <returns>
    ///     The loaded navigation property value, or the navigation property value unchanged if the loader is <see langword="null" />.
    /// </returns>
    public static TRelated? Load<TRelated>(
        this ILazyLoader? loader,
        object entity,
        ref TRelated? navigationField,
        [CallerMemberName] string navigationName = "")
        where TRelated : class
    {
        loader?.Load(entity, navigationName);

        return navigationField;
    }
}
