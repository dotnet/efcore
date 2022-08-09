// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore;

public class DbSetSourceTest
{
    [ConditionalFact]
    public void Can_create_new_generic_DbSet()
    {
        var context = InMemoryTestHelpers.Instance.CreateContext();

        var factorySource = new DbSetSource();

        var set = factorySource.Create(context, typeof(Random));

        Assert.IsType<InternalDbSet<Random>>(set);
    }

    [ConditionalFact]
    public void Always_creates_a_new_DbSet_instance()
    {
        var context = InMemoryTestHelpers.Instance.CreateContext();

        var factorySource = new DbSetSource();

        Assert.NotSame(factorySource.Create(context, typeof(Random)), factorySource.Create(context, typeof(Random)));
    }

    [ConditionalFact]
    public void Can_create_new_generic_DbSet_for_shared_type()
    {
        var context = InMemoryTestHelpers.Instance.CreateContext();

        var factorySource = new DbSetSource();

        var set = factorySource.Create(context, nameof(Random), typeof(Random));

        Assert.IsType<InternalDbSet<Random>>(set);
    }

    [ConditionalFact]
    public void Always_creates_a_new_DbSet_instance_for_shared_type()
    {
        var context = InMemoryTestHelpers.Instance.CreateContext();

        var factorySource = new DbSetSource();

        Assert.NotSame(
            factorySource.Create(context, nameof(Random), typeof(Random)),
            factorySource.Create(context, nameof(Random), typeof(Random)));
    }
}
