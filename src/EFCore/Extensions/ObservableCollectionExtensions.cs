// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
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
        public static BindingList<T> ToBindingList<T>([NotNull] this ObservableCollection<T> source)
            where T : class
        {
            Check.NotNull(source, nameof(source));

            return new ObservableBackedBindingList<T>(source);
        }
    }
}
