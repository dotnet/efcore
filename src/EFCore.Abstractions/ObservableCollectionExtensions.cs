// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for <see cref="ObservableCollection{T}" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
///     examples.
/// </remarks>
public static class ObservableCollectionExtensions
{
    /// <summary>
    ///     Returns a <see cref="BindingList{T}" /> implementation that stays in sync with the given
    ///     <see cref="ObservableCollection{T}" />.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The collection that the binding list will stay in sync with.</param>
    /// <returns>The binding list.</returns>
    [RequiresUnreferencedCode(
        "BindingList raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
    [RequiresDynamicCode("Requires calling MakeGenericType on the property descriptor's type")]
    public static BindingList<T> ToBindingList<T>(this ObservableCollection<T> source)
        where T : class
        => new ObservableBackedBindingList<T>(source);
}
