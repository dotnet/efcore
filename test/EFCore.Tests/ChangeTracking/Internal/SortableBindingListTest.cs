// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class SortableBindingListTest
    {
        private void SortTest(string property, ListSortDirection direction)
        {
            var list = new List<ListElement>
            {
                3,
                1,
                4,
                1,
                5,
                9
            };
            var sortedList = direction == ListSortDirection.Ascending
                ? new List<ListElement>
                {
                    1,
                    1,
                    3,
                    4,
                    5,
                    9
                }
                : new List<ListElement>
                {
                    9,
                    5,
                    4,
                    3,
                    1,
                    1
                };

            var bindingList = new SortableBindingList<ListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property(property), direction);

            Assert.True(list.SequenceEqual(sortedList, new ListElementComparer()));
        }

        [Fact]
        public void SortableBindingList_can_sort_ascending_using_IComparable_on_value_type()
        {
            SortTest("Int", ListSortDirection.Ascending);
        }

        [Fact]
        public void SortableBindingList_can_sort_ascending_using_IComparable_on_nullable_value_type()
        {
            SortTest("NullableInt", ListSortDirection.Ascending);
        }

        [Fact]
        public void SortableBindingList_can_sort_ascending_using_IComparable_on_reference_type()
        {
            SortTest("String", ListSortDirection.Ascending);
        }

        [Fact]
        public void SortableBindingList_can_sort_descending_using_IComparable_on_value_type()
        {
            SortTest("Int", ListSortDirection.Descending);
        }

        [Fact]
        public void SortableBindingList_can_sort_descending_using_IComparable_on_nullable_value_type()
        {
            SortTest("NullableInt", ListSortDirection.Descending);
        }

        [Fact]
        public void SortableBindingList_can_sort_descending_using_IComparable_on_reference_type()
        {
            SortTest("String", ListSortDirection.Descending);
        }

        [Fact]
        public void SortableBindingList_does_not_sort_for_non_XNode_that_does_not_implement_IComparable()
        {
            var list = new List<ListElement>
            {
                3,
                1,
                4,
                1,
                5,
                9
            };
            var unsortedList = new List<ListElement>
            {
                3,
                1,
                4,
                1,
                5,
                9
            };
            var bindingList = new SortableBindingList<ListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property("Random"), ListSortDirection.Ascending);

            Assert.True(list.SequenceEqual(unsortedList, new ListElementComparer()));
        }

        [Fact]
        public void SortableBindingList_does_not_sort_for_byte_arrays()
        {
            var list = new List<ListElement>
            {
                3,
                1,
                4,
                1,
                5,
                9
            };
            var unsortedList = new List<ListElement>
            {
                3,
                1,
                4,
                1,
                5,
                9
            };
            var bindingList = new SortableBindingList<ListElement>(list);

            ((IBindingList)bindingList).ApplySort(ListElement.Property("ByteArray"), ListSortDirection.Descending);

            Assert.True(list.SequenceEqual(unsortedList, new ListElementComparer()));
        }

        [Fact]
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

        [Fact]
        public void SortableBindingList_can_sort_when_list_is_of_derived_type()
        {
            var list = new List<DerivedListElement>
            {
                new DerivedListElement(3),
                new DerivedListElement(1),
                new DerivedListElement(4)
            };
            var sortedList = new List<DerivedListElement>
            {
                new DerivedListElement(1),
                new DerivedListElement(3),
                new DerivedListElement(4)
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
                Random = new Random();
                ByteArray = new[] { (byte)i, (byte)i, (byte)i, (byte)i };
            }

            public static implicit operator ListElement(int i) => new ListElement(i);

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
            public bool Equals(ListElement x, ListElement y) => x.Int == y.Int;
            public int GetHashCode(ListElement obj) => obj.Int;
        }
    }
}
