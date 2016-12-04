// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Extends <see cref="ObservableHashSet{T}" /> and adds an explicit implementation of <see cref="IListSource" />.
    ///     </para>
    ///     <para>
    ///         The method <see cref="IListSource.GetList" /> is implemented to return an <see cref="IBindingList" />
    ///         implementation that stays in sync with the ObservableHashSet.
    ///     </para>
    ///     <para>
    ///         This class can be used to implement navigation properties on entities for use in Windows Forms data binding.
    ///         For WPF data binding use an ObservableHashSet rather than an instance of this class.
    ///     </para>
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public class ObservableHashSetListSource<T> : ObservableHashSet<T>, IListSource
        where T : class
    {
        private IBindingList _bindingList;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ObservableHashSetListSource{T}" /> class.
        /// </summary>
        public ObservableHashSetListSource()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ObservableHashSetListSource{T}" /> class that
        ///     contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection"> The collection from which the elements are copied. </param>
        public ObservableHashSetListSource([NotNull] IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.ObservableHashSetListSource{T}" /> class that
        ///     contains elements copied from the specified list.
        /// </summary>
        /// <param name="list"> The list from which the elements are copied. </param>
        public ObservableHashSetListSource([NotNull] List<T> list)
            : base(list)
        {
        }

        /// <summary>
        ///     Always false because there is never a contained collection.
        /// </summary>
        bool IListSource.ContainsListCollection => false;

        /// <summary>
        ///     Returns an <see cref="IBindingList" /> implementation that stays in sync with
        ///     this <see cref="ObservableHashSet{T}" />. The returned list is cached on this object
        ///     such that the same list is returned each time this method is called.
        /// </summary>
        /// <returns>
        ///     An <see cref="IBindingList" /> in sync with the ObservableHashSet.
        /// </returns>
        IList IListSource.GetList() => _bindingList ?? (_bindingList = ToBindingList());
    }
}
#endif
