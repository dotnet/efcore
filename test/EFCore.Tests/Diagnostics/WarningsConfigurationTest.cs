// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Diagnostics;

public class WarningsConfigurationTest
{
    [Fact]
    public void Same_explicit_behaviors_in_separate_instances_use_same_service_provider()
    {
        var first = new WarningsConfiguration()
            .WithExplicit([CoreEventId.CascadeDelete], WarningBehavior.Throw)
            .WithExplicit([(CoreEventId.ContextDisposed, LogLevel.Warning)]);
        var second = new WarningsConfiguration()
            .WithExplicit([CoreEventId.CascadeDelete], WarningBehavior.Throw)
            .WithExplicit([(CoreEventId.ContextDisposed, LogLevel.Warning)]);

        Assert.True(first.ShouldUseSameServiceProvider(second));
        Assert.True(second.ShouldUseSameServiceProvider(first));
    }

    [Fact]
    public void Same_event_with_different_behavior_uses_different_service_provider()
    {
        var first = new WarningsConfiguration().WithExplicit([CoreEventId.CascadeDelete], WarningBehavior.Throw);
        var second = new WarningsConfiguration().WithExplicit([CoreEventId.CascadeDelete], WarningBehavior.Ignore);

        Assert.False(first.ShouldUseSameServiceProvider(second));
    }

    [Fact]
    public void Different_events_with_same_count_use_different_service_provider()
    {
        var first = new WarningsConfiguration().WithExplicit([CoreEventId.CascadeDelete], WarningBehavior.Throw);
        var second = new WarningsConfiguration().WithExplicit([CoreEventId.CascadeDeleteOrphan], WarningBehavior.Throw);

        Assert.False(first.ShouldUseSameServiceProvider(second));
    }

    [Fact]
    public void Same_event_with_different_level_uses_different_service_provider()
    {
        var first = new WarningsConfiguration().WithExplicit([(CoreEventId.ContextDisposed, LogLevel.Warning)]);
        var second = new WarningsConfiguration().WithExplicit([(CoreEventId.ContextDisposed, LogLevel.Information)]);

        Assert.False(first.ShouldUseSameServiceProvider(second));
    }
}
