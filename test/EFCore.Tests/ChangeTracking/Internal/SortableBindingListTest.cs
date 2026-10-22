// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class SortableBindingListTest
{
    private void SortTest(string property, ListSortDirection direction)
    {
        List<ListElement> list = [3, 1, 4, 1, 5, 9];
        List<ListElement> sortedList = direction == ListSortDirection.Ascending
            ? [1, 1, 3, 4, 5, 9]
            : [9, 5, 4, 3, 1, 1];

        var bindingList = new SortableBindingList<ListElement>(list);

        ((IBindingList)bindingList).ApplySort(ListElement.Property(property), direction);

        Assert.True(list.SequenceEqual(sortedList, new ListElementComparer()));
    }

    [ConditionalFact]
    public void SortableBindingList_can_sort_ascending_using_IComparable_on_value_type()
        => SortTest("Int", ListSortDirection.Ascending);

    [ConditionalFact]
    public void SortableBindingList_can_sort_ascending_using_IComparable_on_nullable_value_type()
        => SortTest("NullableInt", ListSortDirection.Ascending);

    [ConditionalFact]
    public void SortableBindingList_can_sort_ascending_using_IComparable_on_reference_type()
        => SortTest("String", ListSortDirection.Ascending);

    [ConditionalFact]
    public void SortableBindingList_can_sort_descending_using_IComparable_on_value_type()
        => SortTest("Int", ListSortDirection.Descending);

    [ConditionalFact]
    public void SortableBindingList_can_sort_descending_using_IComparable_on_nullable_value_type()
        => SortTest("NullableInt", ListSortDirection.Descending);

    [ConditionalFact]
    public void SortableBindingList_can_sort_descending_using_IComparable_on_reference_type()
        => SortTest("String", ListSortDirection.Descending);

    [ConditionalFact]
    public void SortableBindingList_does_not_sort_for_non_XNode_that_does_not_implement_IComparable()
    {
        List<ListElement> list = [3, 1, 4, 1, 5, 9];
        List<ListElement> unsortedList = [3, 1, 4, 1, 5, 9];
        var bindingList = new SortableBindingList<ListElement>(list);

        ((IBindingList)bindingList).ApplySort(ListElement.Property("Random"), ListSortDirection.Ascending);

        Assert.True(list.SequenceEqual(unsortedList, new ListElementComparer()));
    }

    [ConditionalFact]
    public void SortableBindingList_does_not_sort_for_byte_arrays()
    {
        List<ListElement> list = [3, 1, 4, 1, 5, 9];
        List<ListElement> unsortedList = [3, 1, 4, 1, 5, 9];
        var bindingList = new SortableBindingList<ListElement>(list);

        ((IBindingList)bindingList).ApplySort(ListElement.Property("ByteArray"), ListSortDirection.Descending);

        Assert.True(list.SequenceEqual(unsortedList, new ListElementComparer()));
    }

    [ConditionalFact]
    public void SortableBindingList_can_sort_when_list_contains_derived_objects()
    {
        var list = new List<ListElement>
        {
            new DerivedListElement(3),
            new DerivedListElement(1),
            new DerivedListElement(4)
        };
        var sortedList = new List<ListElement>
        {
            new DerivedListElement(1),
            new DerivedListElement(3),
            new DerivedListElement(4)
        };

        var bindingList = new SortableBindingList<ListElement>(list);

        ((IBindingList)bindingList).ApplySort(ListElement.Property("Int"), ListSortDirection.Ascending);

        Assert.True(list.SequenceEqual(sortedList, new ListElementComparer()));
    }

    [ConditionalFact]
    public void SortableBindingList_can_sort_when_list_is_of_derived_type()
    {
        var list = new List<DerivedListElement>
        {
            new(3),
            new(1),
            new(4)
        };
        var sortedList = new List<DerivedListElement>
        {
            new(1),
            new(3),
            new(4)
        };

        var bindingList = new SortableBindingList<DerivedListElement>(list);

        ((IBindingList)bindingList).ApplySort(ListElement.Property("Int"), ListSortDirection.Ascending);

        Assert.True(list.SequenceEqual(sortedList, new ListElementComparer()));
    }

    private class ListElement
    {
        public ListElement()
        {
        }

        public ListElement(int i)
        {
            Int = i;
            NullableInt = i;
            String = i.ToString();
            Random = Random.Shared;
            ByteArray = [(byte)i, (byte)i, (byte)i, (byte)i];
        }

        public static implicit operator ListElement(int i)
            => new(i);

        public int Int { get; }
        public int? NullableInt { get; }
        public string String { get; }
        public Random Random { get; }
        public byte[] ByteArray { get; }

        public static PropertyDescriptor Property(string name)
            => TypeDescriptor.GetProperties(typeof(ListElement))[name];
    }

    private class DerivedListElement : ListElement
    {
        public DerivedListElement()
        {
        }

        public DerivedListElement(int i)
            : base(i)
        {
        }
    }

    private class ListElementComparer : IEqualityComparer<ListElement>
    {
        public bool Equals(ListElement x, ListElement y)
            => x.Int == y.Int;

        public int GetHashCode(ListElement obj)
            => obj.Int;
    }
}
