// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Extends <see cref="ObservableCollection{T}" /> and adds an explicit implementation of <see cref="IListSource" />.
/// </summary>
/// <remarks>
///     <para>
///         The method <see cref="IListSource.GetList" /> is implemented to return an <see cref="IBindingList" />
///         implementation that stays in sync with the ObservableCollection.
///     </para>
///     <para>
///         This class can be used to implement navigation properties on entities for use in Windows Forms data binding.
///         For WPF data binding use an ObservableCollection rather than an instance of this class.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-local-views">Local views of tracked entities in EF Core</see> for more information and
///         examples.
///     </para>
/// </remarks>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
[RequiresUnreferencedCode(
    "BindingList raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
public class ObservableCollectionListSource<T> : ObservableCollection<T>, IListSource
    where T : class
{
    private IBindingList? _bindingList;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableCollectionListSource{T}" /> class.
    /// </summary>
    public ObservableCollectionListSource()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableCollectionListSource{T}" /> class that
    ///     contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection from which the elements are copied.</param>
    public ObservableCollectionListSource(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ObservableCollectionListSource{T}" /> class that
    ///     contains elements copied from the specified list.
    /// </summary>
    /// <param name="list">The list from which the elements are copied.</param>
    public ObservableCollectionListSource(List<T> list)
        : base(list)
    {
    }

    /// <summary>
    ///     Always false because there is never a contained collection.
    /// </summary>
    bool IListSource.ContainsListCollection
        => false;

    /// <summary>
    ///     Returns an <see cref="IBindingList" /> implementation that stays in sync with
    ///     this <see cref="ObservableCollection{T}" />. The returned list is cached on this object
    ///     such that the same list is returned each time this method is called.
    /// </summary>
    /// <returns>
    ///     An <see cref="IBindingList" /> in sync with the ObservableCollection.
    /// </returns>
    [RequiresUnreferencedCode(
        "BindingList raises ListChanged events with PropertyDescriptors. PropertyDescriptors require unreferenced code.")]
    [RequiresDynamicCode("Requires calling MakeGenericType on the property descriptor's type")]
    [UnconditionalSuppressMessage(
        "ReflectionAnalysis",
        "IL2046",
        Justification =
            "This method is an interface implementation, and the interface method isn't annotated with RequiresUnreferencedCode. "
            + "The entire type is marked with RequiresUnreferencedCode.")]
    [SuppressMessage(
        "AOT", "IL3051:'RequiresDynamicCodeAttribute' annotations must match across all interface implementations or overrides.",
        Justification = "This method is an override, and the base method isn't annotated with RequiresDynamicCode. "
            + "The entire type is marked with RequiresDynamicCode.")]
    IList IListSource.GetList()
        => _bindingList ??= this.ToBindingList();
}
