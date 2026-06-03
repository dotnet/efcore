// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class ObservableBackedBindingListTest
{
    [ConditionalFact]
    public void Items_added_to_ObservableCollection_are_added_to_binding_list()
    {
        var oc = new ObservableCollection<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = new ListElement(1);
        oc.Add(item);

        Assert.Contains(item, obbl);
    }

    [ConditionalFact]
    public void Items_removed_from_ObservableCollection_are_removed_from_binding_list()
    {
        var item = new ListElement(4);
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        oc.Remove(item);

        Assert.DoesNotContain(item, obbl);
        Assert.Equal(5, obbl.Count);
    }

    [ConditionalFact]
    public void Items_replaced_in_the_ObservableCollection_are_replaced_in_the_binding_list()
    {
        var item = new ListElement(4);
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var newItem = new ListElement(-4);
        oc[2] = newItem;

        Assert.DoesNotContain(item, obbl);
        Assert.Contains(newItem, obbl);
        Assert.Equal(6, obbl.Count);
    }

    [ConditionalFact]
    public void Items_cleared_in_the_ObservableCollection_are_cleared_in_the_binding_list()
    {
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            4,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        oc.Clear();

        Assert.Empty(obbl);
    }

    [ConditionalFact]
    public void Adding_duplicate_item_to_the_ObservableCollection_adds_duplicate_to_the_binding_list()
    {
        var item = new ListElement(4);
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        oc.Add(item);

        Assert.Equal(7, obbl.Count);
        Assert.Equal(2, obbl.Count(i => ReferenceEquals(i, item)));
    }

    [ConditionalFact]
    public void Items_added_to_the_binding_list_are_added_to_the_ObservableCollection()
    {
        var oc = new ObservableCollection<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = new ListElement(7);
        obbl.Add(item);

        Assert.Contains(item, oc);
    }

    [ConditionalFact]
    public void Items_added_to_the_binding_list_with_AddNew_are_added_to_the_ObservableCollection()
    {
        var oc = new ObservableCollection<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = obbl.AddNew();
        obbl.EndNew(0);

        Assert.Contains(item, oc);
    }

    [ConditionalFact]
    public void Items_canceled_during_AddNew_are_not_added_to_the_ObservableCollection()
    {
        var oc = new ObservableCollection<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = obbl.AddNew();
        obbl.CancelNew(0);

        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_inserted_into_the_binding_list_are_added_to_the_ObservableCollection()
    {
        var oc = new ObservableCollection<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = new ListElement(7);
        obbl.Insert(0, item);

        Assert.Contains(item, oc);
    }

    [ConditionalFact]
    public void Items_set_in_the_binding_list_are_replaced_in_the_ObservableCollection()
    {
        var item = new ListElement(4);
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var newItem = new ListElement(7);
        obbl[2] = newItem;

        Assert.Contains(newItem, oc);
        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_removed_from_the_binding_list_are_removed_from_the_ObservableCollection()
    {
        var item = new ListElement(4);
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        obbl.Remove(item);

        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_removed_by_index_from_the_binding_list_are_removed_from_the_ObservableCollection()
    {
        var item = new ListElement(4);
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        obbl.RemoveAt(2);

        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_cleared_from_the_binding_list_are_cleared_from_the_ObservableCollection()
    {
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            4,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        obbl.Clear();

        Assert.Empty(oc);
    }

    [ConditionalFact]
    public void Adding_duplicate_item_to_the_binding_list_adds_duplicate_to_the_ObservableCollection()
    {
        var item = new ListElement(4);
        var oc = new ObservableCollection<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc) { item };

        Assert.Equal(7, oc.Count);
        Assert.Equal(2, oc.Count(i => ReferenceEquals(i, item)));
    }

    [ConditionalFact]
    public void Attempt_to_AddNew_for_abstract_type_works_if_AddingNew_event_is_used_to_create_new_object()
    {
        var obbl = new ObservableBackedBindingList<NotXNode>(new ObservableCollection<NotXNode>());
        var item = new NotXText("Some Value");

        obbl.AddingNew += (s, e) => e.NewObject = item;
        obbl.AddNew();
        obbl.EndNew(0);

        Assert.Contains(item, obbl);
    }

    [ConditionalFact]
    public void Attempt_to_AddNew_for_type_without_parameterless_constructor_works_if_AddingNew_event_is_used_to_create_new_object()
    {
        var obbl = new ObservableBackedBindingList<NotXText>(new ObservableCollection<NotXText>());
        var item = new NotXText("Some Value");

        obbl.AddingNew += (s, e) => e.NewObject = item;
        obbl.AddNew();
        obbl.EndNew(0);

        Assert.Contains(item, obbl);
    }

    [ConditionalFact]
    public void Items_added_to_ObservableHashSet_are_added_to_binding_list()
    {
        var oc = new ObservableHashSet<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = new ListElement(1);
        oc.Add(item);

        Assert.Contains(item, obbl);
    }

    [ConditionalFact]
    public void Items_removed_from_ObservableHashSet_are_removed_from_binding_list()
    {
        var item = new ListElement(4);
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        oc.Remove(item);

        Assert.DoesNotContain(item, obbl);
        Assert.Equal(5, obbl.Count);
    }

    [ConditionalFact]
    public void Items_cleared_in_the_ObservableHashSet_are_cleared_in_the_binding_list()
    {
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            4,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        oc.Clear();

        Assert.Empty(obbl);
    }

    [ConditionalFact]
    public void Adding_duplicate_item_to_the_ObservableHashSet_is_ignored()
    {
        var item = new ListElement(4);
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        oc.Add(item);

        Assert.Equal(6, obbl.Count);
        Assert.Equal(1, obbl.Count(i => ReferenceEquals(i, item)));
    }

    [ConditionalFact]
    public void Items_added_to_the_binding_list_are_added_to_the_ObservableHashSet()
    {
        var oc = new ObservableHashSet<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = new ListElement(7);
        obbl.Add(item);

        Assert.Contains(item, oc);
    }

    [ConditionalFact]
    public void Items_added_to_the_binding_list_with_AddNew_are_added_to_the_ObservableHashSet()
    {
        var oc = new ObservableHashSet<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = obbl.AddNew();
        obbl.EndNew(0);

        Assert.Contains(item, oc);
    }

    [ConditionalFact]
    public void Items_canceled_during_AddNew_are_not_added_to_the_ObservableHashSet()
    {
        var oc = new ObservableHashSet<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = obbl.AddNew();
        obbl.CancelNew(0);

        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_inserted_into_the_binding_list_are_added_to_the_ObservableHashSet()
    {
        var oc = new ObservableHashSet<ListElement>();
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var item = new ListElement(7);
        obbl.Insert(0, item);

        Assert.Contains(item, oc);
    }

    [ConditionalFact]
    public void Items_set_in_the_binding_list_are_replaced_in_the_ObservableHashSet()
    {
        var item = new ListElement(4);
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        var newItem = new ListElement(7);
        obbl[2] = newItem;

        Assert.Contains(newItem, oc);
        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_removed_from_the_binding_list_are_removed_from_the_ObservableHashSet()
    {
        var item = new ListElement(4);
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        obbl.Remove(item);

        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_removed_by_index_from_the_binding_list_are_removed_from_the_ObservableHashSet()
    {
        var item = new ListElement(4);
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        obbl.RemoveAt(2);

        Assert.DoesNotContain(item, oc);
    }

    [ConditionalFact]
    public void Items_cleared_from_the_binding_list_are_cleared_from_the_ObservableHashSet()
    {
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            4,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc);

        obbl.Clear();

        Assert.Empty(oc);
    }

    [ConditionalFact]
    public void Adding_duplicate_item_to_the_binding_list_is_ignored()
    {
        var item = new ListElement(4);
        var oc = new ObservableHashSet<ListElement>
        {
            3,
            1,
            item,
            1,
            5,
            9
        };
        var obbl = new ObservableBackedBindingList<ListElement>(oc) { item };

        Assert.Equal(6, oc.Count);
        Assert.Equal(1, oc.Count(i => ReferenceEquals(i, item)));
    }

    [ConditionalFact]
    public void Attempt_to_AddNew_on_set_for_abstract_type_works_if_AddingNew_event_is_used_to_create_new_object()
    {
        var obbl = new ObservableBackedBindingList<NotXNode>(new ObservableHashSet<NotXNode>());
        var item = new NotXText("Some Value");

        obbl.AddingNew += (s, e) => e.NewObject = item;
        obbl.AddNew();
        obbl.EndNew(0);

        Assert.Contains(item, obbl);
    }

    [ConditionalFact]
    public void
        Attempt_to_AddNew_on_set_for_type_without_parameterless_constructor_works_if_AddingNew_event_is_used_to_create_new_object()
    {
        var obbl = new ObservableBackedBindingList<NotXText>(new ObservableHashSet<NotXText>());
        var item = new NotXText("Some Value");

        obbl.AddingNew += (s, e) => e.NewObject = item;
        obbl.AddNew();
        obbl.EndNew(0);

        Assert.Contains(item, obbl);
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
            XNode = new NotXText(i.ToString());
            Random = Random.Shared;
            ByteArray = [(byte)i, (byte)i, (byte)i, (byte)i];
        }

        public static implicit operator ListElement(int i)
            => new(i);

        public int Int { get; }
        public int? NullableInt { get; }
        public string String { get; }
        public NotXNode XNode { get; }
        public Random Random { get; }
        public byte[] ByteArray { get; }

        public static PropertyDescriptor Property(string name)
            => TypeDescriptor.GetProperties(typeof(ListElement))[name];
    }

    private abstract class NotXNode;

    private class NotXText(string value) : NotXNode
    {
        private readonly string _value = value;
    }
}
