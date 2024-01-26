// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore;

public class DbSetInitializerTest
{
    [ConditionalFact]
    public void Initializes_all_entity_set_properties_with_setters()
    {
        var setFinder = new FakeSetFinder();
        var setSource = new DbSetSource();

        var customServices = new ServiceCollection()
            .AddSingleton<IDbSetInitializer>(
                new DbSetInitializer(setFinder, setSource));

        var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider(customServices);

        using var context = new JustAContext(
            new DbContextOptionsBuilder().UseInternalServiceProvider(serviceProvider).Options);
        Assert.NotNull(context.One);
        Assert.NotNull(context.GetTwo());
        Assert.NotNull(context.Three);
        Assert.Null(context.Four);
    }

    private class FakeSetFinder : IDbSetFinder
    {
        public IReadOnlyList<DbSetProperty> FindSets(Type contextType)
        {
            var setterFactory = ClrPropertySetterFactory.Instance;

            return new[]
            {
                new DbSetProperty("One", typeof(string), setterFactory.Create(typeof(JustAContext).GetAnyProperty("One"))),
                new DbSetProperty("Two", typeof(object), setterFactory.Create(typeof(JustAContext).GetAnyProperty("Two"))),
                new DbSetProperty("Three", typeof(string), setterFactory.Create(typeof(JustAContext).GetAnyProperty("Three"))),
                new DbSetProperty("Four", typeof(string), null)
            };
        }
    }

    private class JustAContext(DbContextOptions options) : DbContext(options)
    {

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<string> One { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private DbSet<object> Two { get; set; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<string> Three { get; private set; }

        public DbSet<string> Four
            => null;

        public DbSet<object> GetTwo()
            => Two;
    }
}
