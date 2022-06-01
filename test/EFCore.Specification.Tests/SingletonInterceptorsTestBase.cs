// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

public abstract class SingletonInterceptorsTestBase
{
    protected SingletonInterceptorsTestBase(SingletonInterceptorsFixtureBase fixture)
    {
        Fixture = fixture;
    }

    protected SingletonInterceptorsFixtureBase Fixture { get; }

    protected class Book
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public string? Title { get; set; }

        [NotMapped]
        public string? MaterializedBy { get; set; }
    }

    public class LibraryContext : PoolableDbContext
    {
        public LibraryContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>();
        }
    }

    public LibraryContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
        => new(Fixture.CreateOptions(interceptors, inject));

    public abstract class SingletonInterceptorsFixtureBase : SharedStoreFixtureBase<LibraryContext>
    {
        public virtual DbContextOptions CreateOptions(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
        {
            var optionsBuilder = inject
                ? new DbContextOptionsBuilder<DbContext>().UseInternalServiceProvider(
                    InjectInterceptors(new ServiceCollection(), interceptors)
                        .BuildServiceProvider(validateScopes: true))
                : new DbContextOptionsBuilder<DbContext>().AddInterceptors(interceptors);

            return AddOptions(TestStore.AddProviderOptions(optionsBuilder)).EnableDetailedErrors().Options;
        }

        protected virtual IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
        {
            foreach (var interceptor in injectedInterceptors)
            {
                serviceCollection.AddSingleton(interceptor);
            }

            return serviceCollection;
        }
    }
}
