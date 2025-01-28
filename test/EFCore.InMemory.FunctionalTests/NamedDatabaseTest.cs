// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class NamedDatabaseTest
{
    [ConditionalFact]
    public void Transient_databases_are_not_shared()
    {
        using (var context = new PusheenContext())
        {
            context.Add(
                new Pusheen { Activity = "In a box" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext())
        {
            Assert.Empty(context.Pusheens);
        }
    }

    [ConditionalFact]
    public void Database_per_app_domain_is_default_with_internal_service_provider()
    {
        using (var context = new PusheenContext(nameof(PusheenContext)))
        {
            context.Add(
                new Pusheen { Activity = "In a box" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext(nameof(PusheenContext)))
        {
            Assert.Equal("In a box", context.Pusheens.Single().Activity);
        }
    }

    [ConditionalFact]
    public void Database_per_service_provider_is_default()
    {
        var provider1 = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);
        var provider2 = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);

        using (var context = new PusheenContext(nameof(PusheenContext), provider1))
        {
            context.Add(
                new Pusheen { Activity = "In a box" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext(nameof(PusheenContext), provider2))
        {
            Assert.Empty(context.Pusheens);

            context.Add(
                new Pusheen { Activity = "With some yarn" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext(nameof(PusheenContext), provider1))
        {
            Assert.Equal("In a box", context.Pusheens.Single().Activity);
        }

        using (var context = new PusheenContext(nameof(PusheenContext), provider2))
        {
            Assert.Equal("With some yarn", context.Pusheens.Single().Activity);
        }
    }

    [ConditionalFact]
    public void Named_databases_shared_per_app_domain_with_internal_service_provider()
    {
        using (var context = new PusheenContext("Cats"))
        {
            context.Add(
                new Pusheen { Activity = "In a box" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext("Plump"))
        {
            Assert.Empty(context.Pusheens);

            context.Add(
                new Pusheen { Activity = "With some yarn" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext("Cats"))
        {
            Assert.Equal("In a box", context.Pusheens.Single().Activity);
        }

        using (var context = new PusheenContext("Plump"))
        {
            Assert.Equal("With some yarn", context.Pusheens.Single().Activity);
        }
    }

    [ConditionalFact]
    public void Named_databases_shared_per_service_provider()
    {
        var provider1 = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);
        var provider2 = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);

        using (var context = new PusheenContext("Cats", provider1))
        {
            context.Add(
                new Pusheen { Activity = "In a box" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext("Plump", provider1))
        {
            Assert.Empty(context.Pusheens);

            context.Add(
                new Pusheen { Activity = "With some yarn" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext(nameof(PusheenContext), provider1))
        {
            Assert.Empty(context.Pusheens);

            context.Add(
                new Pusheen { Activity = "On a scooter" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext("Cats", provider2))
        {
            Assert.Empty(context.Pusheens);

            context.Add(
                new Pusheen { Activity = "Is a DJ" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext("Plump", provider2))
        {
            Assert.Empty(context.Pusheens);

            context.Add(
                new Pusheen { Activity = "Goes to sleep" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext(nameof(PusheenContext), provider2))
        {
            Assert.Empty(context.Pusheens);

            context.Add(
                new Pusheen { Activity = "Loves magic unicorns" });
            context.SaveChanges();
        }

        using (var context = new PusheenContext("Cats", provider1))
        {
            Assert.Equal("In a box", context.Pusheens.Single().Activity);
        }

        using (var context = new PusheenContext("Plump", provider1))
        {
            Assert.Equal("With some yarn", context.Pusheens.Single().Activity);
        }

        using (var context = new PusheenContext(nameof(PusheenContext), provider1))
        {
            Assert.Equal("On a scooter", context.Pusheens.Single().Activity);
        }

        using (var context = new PusheenContext("Cats", provider2))
        {
            Assert.Equal("Is a DJ", context.Pusheens.Single().Activity);
        }

        using (var context = new PusheenContext("Plump", provider2))
        {
            Assert.Equal("Goes to sleep", context.Pusheens.Single().Activity);
        }

        using (var context = new PusheenContext(nameof(PusheenContext), provider2))
        {
            Assert.Equal("Loves magic unicorns", context.Pusheens.Single().Activity);
        }
    }

    private class PusheenContext(string databaseName, IServiceProvider serviceProvider = null) : DbContext
    {
        private readonly string _databaseName = databaseName;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public PusheenContext(IServiceProvider serviceProvider = null)
            : this(null, serviceProvider)
        {
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<Pusheen> Pusheens { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInternalServiceProvider(_serviceProvider);

            if (_databaseName == null)
            {
                optionsBuilder
                    .EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
            }
            else
            {
                optionsBuilder.UseInMemoryDatabase(_databaseName);
            }
        }
    }

    private class Pusheen
    {
        public int Id { get; set; }
        public string Activity { get; set; }
    }
}
