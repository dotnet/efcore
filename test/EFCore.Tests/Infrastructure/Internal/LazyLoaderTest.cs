// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal;

public class LazyLoaderTest
{
    [Theory, InlineData(false), InlineData(true)]
    public async Task Loaded_states_are_thread_safe(bool initialize)
    {
        using var context = new DbContext(new DbContextOptionsBuilder().UseInMemoryDatabase(nameof(LazyLoaderTest)).Options);
        var currentContext = context.GetService<ICurrentDbContext>();
        var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>();
        var entity = new object();
        var loaders = Enumerable.Range(0, 1000).Select(
            _ =>
            {
                var loader = new LazyLoader(currentContext, logger);
                if (initialize)
                {
                    loader.SetLoaded(entity, "Initial");
                }

                return loader;
            }).ToArray();

        using var barrier = new Barrier(2);
        await Task.WhenAll(
            Enumerable.Range(0, 2).Select(
                worker => Task.Factory.StartNew(
                    () =>
                    {
                        var navigationName = "Navigation" + worker;
                        try
                        {
                            foreach (var loader in loaders)
                            {
                                barrier.SignalAndWait();
                                loader.SetLoaded(entity, navigationName);
                                Assert.True(loader.IsLoaded(entity, navigationName));
                                Assert.Equal(initialize, loader.IsLoaded(entity, "Initial"));
                                barrier.SignalAndWait();
                            }
                        }
                        finally
                        {
                            barrier.RemoveParticipant();
                        }
                    },
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default)));

        foreach (var loader in loaders)
        {
            Assert.True(loader.IsLoaded(entity, "Navigation0"));
            Assert.True(loader.IsLoaded(entity, "Navigation1"));
            if (initialize)
            {
                Assert.True(loader.IsLoaded(entity, "Initial"));
            }
        }
    }
}
