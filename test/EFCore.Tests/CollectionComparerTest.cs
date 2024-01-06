// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore;

public class CollectionComparerTest
{
    [ConditionalFact]
    public void Can_detect_changes_to_primitive_collections_using_arrays()
    {
        using var context = new SomeLists();

        var entity = new Voidbringer
        {
            Id = 1,
            ArrayInt = [0, 1, 2],
            ArrayNullableInt = [0, null, 2],
            ArrayString = ["0", "1", "2"],
            ArrayNullableString = ["0", null, "2"],
            ArrayStruct = [new("0"), new("1"), new("2")],
            ArrayNullableStruct = [new("0"), null, new("2")],
            ArrayClass = [new("0"), new("1"), new("2")],
            ArrayNullableClass = [new("0"), null, new("2")],
            EnumerableInt = new[] { 0, 1, 2 },
            EnumerableNullableInt = new int?[] { 0, null, 2 },
            EnumerableString = new[] { "0", "1", "2" },
            EnumerableNullableString = new[] { "0", null, "2" },
            EnumerableStruct = new MyStruct[] { new("0"), new("1"), new("2") },
            EnumerableNullableStruct = new MyStruct?[] { new("0"), null, new("2") },
            EnumerableClass = new MyClass[] { new("0"), new("1"), new("2") },
            EnumerableNullableClass = new MyClass?[] { new("0"), null, new("2") },
            IListInt = new[] { 0, 1, 2 },
            IListNullableInt = new int?[] { 0, null, 2 },
            IListString = new[] { "0", "1", "2" },
            IListNullableString = new[] { "0", null, "2" },
            IListStruct = new MyStruct[] { new("0"), new("1"), new("2") },
            IListNullableStruct = new MyStruct?[] { new("0"), null, new("2") },
            IListClass = new MyClass[] { new("0"), new("1"), new("2") },
            IListNullableClass = new MyClass?[] { new("0"), null, new("2") },
        };

        var entry = context.Add(entity);

        context.SaveChanges();

        entity.ArrayInt[0] = 10;
        entity.ArrayNullableInt[1] = 11;
        entity.ArrayString[2] = "12";
        entity.ArrayNullableString[0] = null;
        entity.ArrayStruct[1] = new MyStruct("14");
        entity.ArrayNullableStruct[2] = new MyStruct("15");
        entity.ArrayClass[0] = new MyClass("16");
        entity.ArrayNullableClass[2] = null;

        ((IList<int>)entity.EnumerableInt)[2] = 20;
        (((IList<int?>)entity.EnumerableNullableInt))[0] = null;
        ((IList<string>)entity.EnumerableString)[1] = "22";
        ((IList<string?>)entity.EnumerableNullableString)[2] = "23";
        ((IList<MyStruct>)entity.EnumerableStruct)[0] = new MyStruct("24");
        ((IList<MyStruct?>)entity.EnumerableNullableStruct)[0] = null;
        ((IList<MyClass>)entity.EnumerableClass)[2] = new MyClass("26");
        ((IList<MyClass?>)entity.EnumerableNullableClass)[0] = new MyClass("27");

        entity.IListInt[1] = 30;
        entity.IListNullableInt[2] = 31;
        entity.IListString[0] = "32";
        entity.IListNullableString[2] = null;
        entity.IListStruct[2] = new MyStruct("34");
        entity.IListNullableStruct[0] = new MyStruct("35");
        entity.IListClass[1] = new MyClass("36");
        entity.IListNullableClass[2] = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(entry.Property(e => e.ArrayInt).IsModified);
        Assert.True(entry.Property(e => e.ArrayNullableInt).IsModified);
        Assert.True(entry.Property(e => e.ArrayString).IsModified);
        Assert.True(entry.Property(e => e.ArrayNullableString).IsModified);
        Assert.True(entry.Property(e => e.ArrayStruct).IsModified);
        Assert.True(entry.Property(e => e.ArrayNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.ArrayClass).IsModified);
        Assert.True(entry.Property(e => e.ArrayNullableClass).IsModified);

        Assert.True(entry.Property(e => e.EnumerableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableClass).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableClass).IsModified);

        Assert.True(entry.Property(e => e.IListInt).IsModified);
        Assert.True(entry.Property(e => e.IListNullableInt).IsModified);
        Assert.True(entry.Property(e => e.IListString).IsModified);
        Assert.True(entry.Property(e => e.IListNullableString).IsModified);
        Assert.True(entry.Property(e => e.IListStruct).IsModified);
        Assert.True(entry.Property(e => e.IListNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.IListClass).IsModified);
        Assert.True(entry.Property(e => e.IListNullableClass).IsModified);

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entry.State);
    }

    [ConditionalFact]
    public void Can_detect_changes_to_primitive_collections_using_List()
    {
        using var context = new SomeLists();

        var entity = new Voidbringer
        {
            Id = 2,
            EnumerableInt = new List<int>
            {
                0,
                1,
                2
            },
            EnumerableNullableInt = new List<int?>
            {
                0,
                null,
                2
            },
            EnumerableString = new List<string>
            {
                "0",
                "1",
                "2"
            },
            EnumerableNullableString = new List<string?>
            {
                "0",
                null,
                "2"
            },
            EnumerableStruct = new List<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            EnumerableNullableStruct = new List<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            EnumerableClass = new List<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            EnumerableNullableClass = new List<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            IListInt = new List<int>
            {
                0,
                1,
                2
            },
            IListNullableInt = new List<int?>
            {
                0,
                null,
                2
            },
            IListString = new List<string>
            {
                "0",
                "1",
                "2"
            },
            IListNullableString = new List<string?>
            {
                "0",
                null,
                "2"
            },
            IListStruct = new List<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            IListNullableStruct = new List<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            IListClass = new List<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            IListNullableClass = new List<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            ListInt = new List<int>
            {
                0,
                1,
                2
            },
            ListNullableInt = new List<int?>
            {
                0,
                null,
                2
            },
            ListString = new List<string>
            {
                "0",
                "1",
                "2"
            },
            ListNullableString = new List<string?>
            {
                "0",
                null,
                "2"
            },
            ListStruct = new List<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ListNullableStruct = new List<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            ListClass = new List<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ListNullableClass = new List<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            ICollectionInt = new List<int>
            {
                0,
                1,
                2
            },
            ICollectionNullableInt = new List<int?>
            {
                0,
                null,
                2
            },
            ICollectionString = new List<string>
            {
                "0",
                "1",
                "2"
            },
            ICollectionNullableString = new List<string?>
            {
                "0",
                null,
                "2"
            },
            ICollectionStruct = new List<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ICollectionNullableStruct = new List<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            ICollectionClass = new List<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ICollectionNullableClass = new List<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
        };

        var entry = context.Add(entity);

        context.SaveChanges();

        ((IList<int>)entity.EnumerableInt)[0] = 20;
        (((IList<int?>)entity.EnumerableNullableInt))[0] = null;
        ((IList<string>)entity.EnumerableString)[2] = "22";
        ((IList<string?>)entity.EnumerableNullableString)[0] = "23";
        ((IList<MyStruct>)entity.EnumerableStruct)[1] = new MyStruct("24");
        ((IList<MyStruct?>)entity.EnumerableNullableStruct)[2] = null;
        ((IList<MyClass>)entity.EnumerableClass)[0] = new MyClass("26");
        ((IList<MyClass?>)entity.EnumerableNullableClass)[1] = new MyClass("27");

        entity.IListInt[0] = 30;
        entity.IListNullableInt[1] = 31;
        entity.IListString[2] = "32";
        entity.IListNullableString[0] = null;
        entity.IListStruct[1] = new MyStruct("34");
        entity.IListNullableStruct[2] = new MyStruct("35");
        entity.IListClass[0] = new MyClass("36");
        entity.IListNullableClass[2] = null;

        entity.ListInt[2] = 40;
        entity.ListNullableInt[0] = null;
        entity.ListString[1] = "42";
        entity.ListNullableString[2] = "43";
        entity.ListStruct[0] = new MyStruct("44");
        entity.ListNullableStruct[0] = null;
        entity.ListClass[2] = new MyClass("46");
        entity.ListNullableClass[0] = new MyClass("47");

        ((IList<int>)entity.ICollectionInt)[1] = 50;
        (((IList<int?>)entity.ICollectionNullableInt))[2] = 51;
        ((IList<string>)entity.ICollectionString)[0] = "52";
        ((IList<string?>)entity.ICollectionNullableString)[2] = null;
        ((IList<MyStruct>)entity.ICollectionStruct)[2] = new MyStruct("54");
        ((IList<MyStruct?>)entity.ICollectionNullableStruct)[0] = new MyStruct("55");
        ((IList<MyClass>)entity.ICollectionClass)[1] = new MyClass("56");
        ((IList<MyClass?>)entity.ICollectionNullableClass)[2] = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(entry.Property(e => e.EnumerableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableClass).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableClass).IsModified);

        Assert.True(entry.Property(e => e.IListInt).IsModified);
        Assert.True(entry.Property(e => e.IListNullableInt).IsModified);
        Assert.True(entry.Property(e => e.IListString).IsModified);
        Assert.True(entry.Property(e => e.IListNullableString).IsModified);
        Assert.True(entry.Property(e => e.IListStruct).IsModified);
        Assert.True(entry.Property(e => e.IListNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.IListClass).IsModified);
        Assert.True(entry.Property(e => e.IListNullableClass).IsModified);

        Assert.True(entry.Property(e => e.ListInt).IsModified);
        Assert.True(entry.Property(e => e.ListNullableInt).IsModified);
        Assert.True(entry.Property(e => e.ListString).IsModified);
        Assert.True(entry.Property(e => e.ListNullableString).IsModified);
        Assert.True(entry.Property(e => e.ListStruct).IsModified);
        Assert.True(entry.Property(e => e.ListNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.ListClass).IsModified);
        Assert.True(entry.Property(e => e.ListNullableClass).IsModified);

        Assert.True(entry.Property(e => e.ICollectionInt).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableInt).IsModified);
        Assert.True(entry.Property(e => e.ICollectionString).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableString).IsModified);
        Assert.True(entry.Property(e => e.ICollectionStruct).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.ICollectionClass).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableClass).IsModified);

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entry.State);
    }

    [ConditionalFact]
    public void Can_detect_changes_to_primitive_collections_using_Collection()
    {
        using var context = new SomeLists();

        var entity = new Voidbringer
        {
            Id = 3,
            EnumerableInt = new Collection<int>
            {
                0,
                1,
                2
            },
            EnumerableNullableInt = new Collection<int?>
            {
                0,
                null,
                2
            },
            EnumerableString = new Collection<string>
            {
                "0",
                "1",
                "2"
            },
            EnumerableNullableString = new Collection<string?>
            {
                "0",
                null,
                "2"
            },
            EnumerableStruct = new Collection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            EnumerableNullableStruct = new Collection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            EnumerableClass = new Collection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            EnumerableNullableClass = new Collection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            IListInt = new Collection<int>
            {
                0,
                1,
                2
            },
            IListNullableInt = new Collection<int?>
            {
                0,
                null,
                2
            },
            IListString = new Collection<string>
            {
                "0",
                "1",
                "2"
            },
            IListNullableString = new Collection<string?>
            {
                "0",
                null,
                "2"
            },
            IListStruct = new Collection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            IListNullableStruct = new Collection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            IListClass = new Collection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            IListNullableClass = new Collection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            ICollectionInt = new Collection<int>
            {
                0,
                1,
                2
            },
            ICollectionNullableInt = new Collection<int?>
            {
                0,
                null,
                2
            },
            ICollectionString = new Collection<string>
            {
                "0",
                "1",
                "2"
            },
            ICollectionNullableString = new Collection<string?>
            {
                "0",
                null,
                "2"
            },
            ICollectionStruct = new Collection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ICollectionNullableStruct = new Collection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            ICollectionClass = new Collection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ICollectionNullableClass = new Collection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            CollectionInt = new Collection<int>
            {
                0,
                1,
                2
            },
            CollectionNullableInt = new Collection<int?>
            {
                0,
                null,
                2
            },
            CollectionString = new Collection<string>
            {
                "0",
                "1",
                "2"
            },
            CollectionNullableString = new Collection<string?>
            {
                "0",
                null,
                "2"
            },
            CollectionStruct = new Collection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            CollectionNullableStruct = new Collection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            CollectionClass = new Collection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            CollectionNullableClass = new Collection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
        };

        var entry = context.Add(entity);

        context.SaveChanges();

        ((IList<int>)entity.EnumerableInt)[0] = 20;
        (((IList<int?>)entity.EnumerableNullableInt))[0] = null;
        ((IList<string>)entity.EnumerableString)[2] = "22";
        ((IList<string?>)entity.EnumerableNullableString)[0] = "23";
        ((IList<MyStruct>)entity.EnumerableStruct)[1] = new MyStruct("24");
        ((IList<MyStruct?>)entity.EnumerableNullableStruct)[2] = null;
        ((IList<MyClass>)entity.EnumerableClass)[0] = new MyClass("26");
        ((IList<MyClass?>)entity.EnumerableNullableClass)[1] = new MyClass("27");

        entity.IListInt[2] = 30;
        entity.IListNullableInt[0] = 31;
        entity.IListString[1] = "32";
        entity.IListNullableString[2] = null;
        entity.IListStruct[0] = new MyStruct("34");
        entity.IListNullableStruct[1] = new MyStruct("35");
        entity.IListClass[2] = new MyClass("36");
        entity.IListNullableClass[0] = null;

        ((IList<int>)entity.ICollectionInt)[1] = 50;
        (((IList<int?>)entity.ICollectionNullableInt))[2] = 51;
        ((IList<string>)entity.ICollectionString)[0] = "52";
        ((IList<string?>)entity.ICollectionNullableString)[2] = null;
        ((IList<MyStruct>)entity.ICollectionStruct)[2] = new MyStruct("54");
        ((IList<MyStruct?>)entity.ICollectionNullableStruct)[0] = new MyStruct("55");
        ((IList<MyClass>)entity.ICollectionClass)[1] = new MyClass("56");
        ((IList<MyClass?>)entity.ICollectionNullableClass)[2] = null;

        ((IList<int>)entity.CollectionInt)[0] = 60;
        (((IList<int?>)entity.CollectionNullableInt))[0] = null;
        ((IList<string>)entity.CollectionString)[2] = "62";
        ((IList<string?>)entity.CollectionNullableString)[0] = "63";
        ((IList<MyStruct>)entity.CollectionStruct)[1] = new MyStruct("64");
        ((IList<MyStruct?>)entity.CollectionNullableStruct)[2] = null;
        ((IList<MyClass>)entity.CollectionClass)[0] = new MyClass("66");
        ((IList<MyClass?>)entity.CollectionNullableClass)[1] = new MyClass("67");

        context.ChangeTracker.DetectChanges();

        Assert.True(entry.Property(e => e.EnumerableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableClass).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableClass).IsModified);

        Assert.True(entry.Property(e => e.IListInt).IsModified);
        Assert.True(entry.Property(e => e.IListNullableInt).IsModified);
        Assert.True(entry.Property(e => e.IListString).IsModified);
        Assert.True(entry.Property(e => e.IListNullableString).IsModified);
        Assert.True(entry.Property(e => e.IListStruct).IsModified);
        Assert.True(entry.Property(e => e.IListNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.IListClass).IsModified);
        Assert.True(entry.Property(e => e.IListNullableClass).IsModified);

        Assert.True(entry.Property(e => e.ICollectionInt).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableInt).IsModified);
        Assert.True(entry.Property(e => e.ICollectionString).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableString).IsModified);
        Assert.True(entry.Property(e => e.ICollectionStruct).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.ICollectionClass).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableClass).IsModified);

        Assert.True(entry.Property(e => e.CollectionInt).IsModified);
        Assert.True(entry.Property(e => e.CollectionNullableInt).IsModified);
        Assert.True(entry.Property(e => e.CollectionString).IsModified);
        Assert.True(entry.Property(e => e.CollectionNullableString).IsModified);
        Assert.True(entry.Property(e => e.CollectionStruct).IsModified);
        Assert.True(entry.Property(e => e.CollectionNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.CollectionClass).IsModified);
        Assert.True(entry.Property(e => e.CollectionNullableClass).IsModified);

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entry.State);
    }

    [ConditionalFact]
    public void Can_detect_changes_to_primitive_collections_using_ObservableCollection()
    {
        using var context = new SomeLists();

        var entity = new Voidbringer
        {
            Id = 4,
            EnumerableInt = new ObservableCollection<int>
            {
                0,
                1,
                2
            },
            EnumerableNullableInt = new ObservableCollection<int?>
            {
                0,
                null,
                2
            },
            EnumerableString = new ObservableCollection<string>
            {
                "0",
                "1",
                "2"
            },
            EnumerableNullableString = new ObservableCollection<string?>
            {
                "0",
                null,
                "2"
            },
            EnumerableStruct = new ObservableCollection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            EnumerableNullableStruct = new ObservableCollection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            EnumerableClass = new ObservableCollection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            EnumerableNullableClass = new ObservableCollection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            IListInt = new ObservableCollection<int>
            {
                0,
                1,
                2
            },
            IListNullableInt = new ObservableCollection<int?>
            {
                0,
                null,
                2
            },
            IListString = new ObservableCollection<string>
            {
                "0",
                "1",
                "2"
            },
            IListNullableString = new ObservableCollection<string?>
            {
                "0",
                null,
                "2"
            },
            IListStruct = new ObservableCollection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            IListNullableStruct = new ObservableCollection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            IListClass = new ObservableCollection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            IListNullableClass = new ObservableCollection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            ICollectionInt = new ObservableCollection<int>
            {
                0,
                1,
                2
            },
            ICollectionNullableInt = new ObservableCollection<int?>
            {
                0,
                null,
                2
            },
            ICollectionString = new ObservableCollection<string>
            {
                "0",
                "1",
                "2"
            },
            ICollectionNullableString = new ObservableCollection<string?>
            {
                "0",
                null,
                "2"
            },
            ICollectionStruct = new ObservableCollection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ICollectionNullableStruct = new ObservableCollection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            ICollectionClass = new ObservableCollection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ICollectionNullableClass = new ObservableCollection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
            ObservableCollectionInt = new ObservableCollection<int>
            {
                0,
                1,
                2
            },
            ObservableCollectionNullableInt = new ObservableCollection<int?>
            {
                0,
                null,
                2
            },
            ObservableCollectionString = new ObservableCollection<string>
            {
                "0",
                "1",
                "2"
            },
            ObservableCollectionNullableString = new ObservableCollection<string?>
            {
                "0",
                null,
                "2"
            },
            ObservableCollectionStruct = new ObservableCollection<MyStruct>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ObservableCollectionNullableStruct = new ObservableCollection<MyStruct?>
            {
                new("0"),
                null,
                new("2")
            },
            ObservableCollectionClass = new ObservableCollection<MyClass>
            {
                new("0"),
                new("1"),
                new("2")
            },
            ObservableCollectionNullableClass = new ObservableCollection<MyClass?>
            {
                new("0"),
                null,
                new("2")
            },
        };

        var entry = context.Add(entity);

        context.SaveChanges();

        ((IList<int>)entity.EnumerableInt)[0] = 20;
        (((IList<int?>)entity.EnumerableNullableInt))[2] = null;
        ((IList<string>)entity.EnumerableString)[2] = "22";
        ((IList<string?>)entity.EnumerableNullableString)[0] = "23";
        ((IList<MyStruct>)entity.EnumerableStruct)[1] = new MyStruct("24");
        ((IList<MyStruct?>)entity.EnumerableNullableStruct)[2] = null;
        ((IList<MyClass>)entity.EnumerableClass)[0] = new MyClass("26");
        ((IList<MyClass?>)entity.EnumerableNullableClass)[1] = new MyClass("27");

        entity.IListInt[2] = 30;
        entity.IListNullableInt[0] = 31;
        entity.IListString[1] = "32";
        entity.IListNullableString[2] = null;
        entity.IListStruct[0] = new MyStruct("34");
        entity.IListNullableStruct[1] = new MyStruct("35");
        entity.IListClass[2] = new MyClass("36");
        entity.IListNullableClass[0] = null;

        ((IList<int>)entity.ICollectionInt)[1] = 50;
        (((IList<int?>)entity.ICollectionNullableInt))[2] = 51;
        ((IList<string>)entity.ICollectionString)[0] = "52";
        ((IList<string?>)entity.ICollectionNullableString)[0] = null;
        ((IList<MyStruct>)entity.ICollectionStruct)[2] = new MyStruct("54");
        ((IList<MyStruct?>)entity.ICollectionNullableStruct)[0] = new MyStruct("55");
        ((IList<MyClass>)entity.ICollectionClass)[1] = new MyClass("56");
        ((IList<MyClass?>)entity.ICollectionNullableClass)[2] = null;

        ((IList<int>)entity.ObservableCollectionInt)[0] = 70;
        (((IList<int?>)entity.ObservableCollectionNullableInt))[1] = 71;
        ((IList<string>)entity.ObservableCollectionString)[2] = "72";
        ((IList<string?>)entity.ObservableCollectionNullableString)[0] = null;
        ((IList<MyStruct>)entity.ObservableCollectionStruct)[1] = new MyStruct("74");
        ((IList<MyStruct?>)entity.ObservableCollectionNullableStruct)[2] = new MyStruct("75");
        ((IList<MyClass>)entity.ObservableCollectionClass)[0] = new MyClass("76");
        ((IList<MyClass?>)entity.ObservableCollectionNullableClass)[2] = null;

        context.ChangeTracker.DetectChanges();

        Assert.True(entry.Property(e => e.EnumerableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableInt).IsModified);
        Assert.True(entry.Property(e => e.EnumerableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableString).IsModified);
        Assert.True(entry.Property(e => e.EnumerableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.EnumerableClass).IsModified);
        Assert.True(entry.Property(e => e.EnumerableNullableClass).IsModified);

        Assert.True(entry.Property(e => e.IListInt).IsModified);
        Assert.True(entry.Property(e => e.IListNullableInt).IsModified);
        Assert.True(entry.Property(e => e.IListString).IsModified);
        Assert.True(entry.Property(e => e.IListNullableString).IsModified);
        Assert.True(entry.Property(e => e.IListStruct).IsModified);
        Assert.True(entry.Property(e => e.IListNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.IListClass).IsModified);
        Assert.True(entry.Property(e => e.IListNullableClass).IsModified);

        Assert.True(entry.Property(e => e.ICollectionInt).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableInt).IsModified);
        Assert.True(entry.Property(e => e.ICollectionString).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableString).IsModified);
        Assert.True(entry.Property(e => e.ICollectionStruct).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.ICollectionClass).IsModified);
        Assert.True(entry.Property(e => e.ICollectionNullableClass).IsModified);

        Assert.True(entry.Property(e => e.ObservableCollectionInt).IsModified);
        Assert.True(entry.Property(e => e.ObservableCollectionNullableInt).IsModified);
        Assert.True(entry.Property(e => e.ObservableCollectionString).IsModified);
        Assert.True(entry.Property(e => e.ObservableCollectionNullableString).IsModified);
        Assert.True(entry.Property(e => e.ObservableCollectionStruct).IsModified);
        Assert.True(entry.Property(e => e.ObservableCollectionNullableStruct).IsModified);
        Assert.True(entry.Property(e => e.ObservableCollectionClass).IsModified);
        Assert.True(entry.Property(e => e.ObservableCollectionNullableClass).IsModified);

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entry.State);
    }

    [ConditionalFact]
    public void List_comparer_throws_when_used_with_non_list()
    {
        var comparer = new ListComparer<string>(new ValueComparer<string>(favorStructuralComparisons: false));

        Assert.Equal(
            CoreStrings.BadListType("HashSet<string>", "IList<string>"),
            Assert.Throws<InvalidOperationException>(() => comparer.Equals(new List<string>(), new HashSet<string>())).Message);

        Assert.Equal(
            CoreStrings.BadListType("HashSet<string>", "IList<string>"),
            Assert.Throws<InvalidOperationException>(() => comparer.Equals(new HashSet<string>(), new List<string>())).Message);

        Assert.Equal(
            CoreStrings.BadListType("HashSet<string>", "IList<string>"),
            Assert.Throws<InvalidOperationException>(() => comparer.Snapshot(new HashSet<string>())).Message);
    }

    [ConditionalFact]
    public void Nullable_list_comparer_throws_when_used_with_non_list()
    {
        var comparer = new NullableValueTypeListComparer<int>(new ValueComparer<int?>(favorStructuralComparisons: false));

        Assert.Equal(
            CoreStrings.BadListType("HashSet<int?>", "IList<int?>"),
            Assert.Throws<InvalidOperationException>(() => comparer.Equals(new List<int?>(), new HashSet<int?>())).Message);

        Assert.Equal(
            CoreStrings.BadListType("HashSet<int?>", "IList<int?>"),
            Assert.Throws<InvalidOperationException>(() => comparer.Equals(new HashSet<int?>(), new List<int?>())).Message);

        Assert.Equal(
            CoreStrings.BadListType("HashSet<int?>", "IList<int?>"),
            Assert.Throws<InvalidOperationException>(() => comparer.Snapshot(new HashSet<int?>())).Message);
    }

    private class Voidbringer
    {
        public int Id { get; set; }

        public int[]? ArrayInt { get; set; }
        public int?[]? ArrayNullableInt { get; set; }
        public string[]? ArrayString { get; set; }
        public string?[]? ArrayNullableString { get; set; }
        public MyStruct[]? ArrayStruct { get; set; }
        public MyStruct?[]? ArrayNullableStruct { get; set; }
        public MyClass[]? ArrayClass { get; set; }
        public MyClass?[]? ArrayNullableClass { get; set; }

        public IEnumerable<int>? EnumerableInt { get; set; }
        public IEnumerable<int?>? EnumerableNullableInt { get; set; }
        public IEnumerable<string>? EnumerableString { get; set; }
        public IEnumerable<string?>? EnumerableNullableString { get; set; }
        public IEnumerable<MyStruct>? EnumerableStruct { get; set; }
        public IEnumerable<MyStruct?>? EnumerableNullableStruct { get; set; }
        public IEnumerable<MyClass>? EnumerableClass { get; set; }
        public IEnumerable<MyClass?>? EnumerableNullableClass { get; set; }

        public IList<int>? IListInt { get; set; }
        public IList<int?>? IListNullableInt { get; set; }
        public IList<string>? IListString { get; set; }
        public IList<string?>? IListNullableString { get; set; }
        public IList<MyStruct>? IListStruct { get; set; }
        public IList<MyStruct?>? IListNullableStruct { get; set; }
        public IList<MyClass>? IListClass { get; set; }
        public IList<MyClass?>? IListNullableClass { get; set; }

        public List<int>? ListInt { get; set; }
        public List<int?>? ListNullableInt { get; set; }
        public List<string>? ListString { get; set; }
        public List<string?>? ListNullableString { get; set; }
        public List<MyStruct>? ListStruct { get; set; }
        public List<MyStruct?>? ListNullableStruct { get; set; }
        public List<MyClass>? ListClass { get; set; }
        public List<MyClass?>? ListNullableClass { get; set; }

        public ICollection<int>? ICollectionInt { get; set; }
        public ICollection<int?>? ICollectionNullableInt { get; set; }
        public ICollection<string>? ICollectionString { get; set; }
        public ICollection<string?>? ICollectionNullableString { get; set; }
        public ICollection<MyStruct>? ICollectionStruct { get; set; }
        public ICollection<MyStruct?>? ICollectionNullableStruct { get; set; }
        public ICollection<MyClass>? ICollectionClass { get; set; }
        public ICollection<MyClass?>? ICollectionNullableClass { get; set; }

        public Collection<int>? CollectionInt { get; set; }
        public Collection<int?>? CollectionNullableInt { get; set; }
        public Collection<string>? CollectionString { get; set; }
        public Collection<string?>? CollectionNullableString { get; set; }
        public Collection<MyStruct>? CollectionStruct { get; set; }
        public Collection<MyStruct?>? CollectionNullableStruct { get; set; }
        public Collection<MyClass>? CollectionClass { get; set; }
        public Collection<MyClass?>? CollectionNullableClass { get; set; }

        public ObservableCollection<int>? ObservableCollectionInt { get; set; }
        public ObservableCollection<int?>? ObservableCollectionNullableInt { get; set; }
        public ObservableCollection<string>? ObservableCollectionString { get; set; }
        public ObservableCollection<string?>? ObservableCollectionNullableString { get; set; }
        public ObservableCollection<MyStruct>? ObservableCollectionStruct { get; set; }
        public ObservableCollection<MyStruct?>? ObservableCollectionNullableStruct { get; set; }
        public ObservableCollection<MyClass>? ObservableCollectionClass { get; set; }
        public ObservableCollection<MyClass?>? ObservableCollectionNullableClass { get; set; }
    }

    private struct MyStruct(string value)
    {
        public string Value { get; set; } = value;
    }

    private class MyClass(string value)
    {
        public string Value { get; set; } = value;
    }

    private class SomeLists : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(SomeLists));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Voidbringer>(
                b =>
                {
                    b.PrimitiveCollection(e => e.ArrayStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ArrayNullableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ArrayClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();
                    b.PrimitiveCollection(e => e.ArrayNullableClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();

                    b.PrimitiveCollection(e => e.EnumerableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.EnumerableNullableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.EnumerableClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();
                    b.PrimitiveCollection(e => e.EnumerableNullableClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();

                    b.PrimitiveCollection(e => e.IListStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.IListNullableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.IListClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();
                    b.PrimitiveCollection(e => e.IListNullableClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();

                    b.PrimitiveCollection(e => e.ListStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ListNullableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ListClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();
                    b.PrimitiveCollection(e => e.ListNullableClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();

                    b.PrimitiveCollection(e => e.ICollectionStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ICollectionNullableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ICollectionClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();
                    b.PrimitiveCollection(e => e.ICollectionNullableClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();

                    b.PrimitiveCollection(e => e.CollectionStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.CollectionNullableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.CollectionClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();
                    b.PrimitiveCollection(e => e.CollectionNullableClass).ElementType().HasConversion<MyClassConverter, MyClassComparer>();

                    b.PrimitiveCollection(e => e.ObservableCollectionStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ObservableCollectionNullableStruct).ElementType().HasConversion<MyStructConverter>();
                    b.PrimitiveCollection(e => e.ObservableCollectionClass).ElementType()
                        .HasConversion<MyClassConverter, MyClassComparer>();
                    b.PrimitiveCollection(e => e.ObservableCollectionNullableClass).ElementType()
                        .HasConversion<MyClassConverter, MyClassComparer>();

                    // var myStructListComparer = new ListComparer<MyStruct>(new ValueComparer<MyStruct>(favorStructuralComparisons: false));
                    // var nullableMyStructListComparer =
                    //     new NullableValueTypeListComparer<MyStruct>(new ValueComparer<MyStruct>(favorStructuralComparisons: false));
                    // var myClassListComparer = new ListComparer<MyClass>(new MyClassComparer());
                    // var intListComparer = new ListComparer<int>(new ValueComparer<int>(favorStructuralComparisons: false));
                    // var nullableIntListComparer =
                    //     new NullableValueTypeListComparer<int>(new ValueComparer<int>(favorStructuralComparisons: false));
                    // var stringListComparer = new ListComparer<string>(new ValueComparer<string>(favorStructuralComparisons: false));
                });
    }

    private class MyClassConverter() : ValueConverter<MyClass, string>(v => v.Value, v => new MyClass(v));

    private class MyStructConverter() : ValueConverter<MyStruct, string>(v => v.Value, v => new MyStruct { Value = v });

    private class MyClassComparer() : ValueComparer<MyClass>(
        (l, r) => l!.Value == r!.Value, v => v.Value.GetHashCode(), v => new MyClass(v.Value));
}
