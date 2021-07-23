// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Extension methods for <see cref="ObservableCollection{T}" />.
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        ///     Returns an <see cref="BindingList{T}" /> implementation that stays in sync with the given
        ///     <see cref="ObservableCollection{T}" />.
        /// </summary>
        /// <typeparam name="T"> The element type. </typeparam>
        /// <param name="source"> The collection that the binding list will stay in sync with. </param>
        /// <returns> The binding list. </returns>
        public static BindingList<T> ToBindingList<T>(this ObservableCollection<T> source)
            where T : class
        {
            Check.NotNull(source, nameof(source));

            return new ObservableBackedBindingList<T>(source);
        }
    }
}
