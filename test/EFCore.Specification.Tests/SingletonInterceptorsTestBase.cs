// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

public abstract class SingletonInterceptorsTestBase<TContext> : NonSharedModelTestBase
    where TContext : SingletonInterceptorsTestBase<TContext>.LibraryContext
{
    protected class Book
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }

        [NotMapped]
        public string? MaterializedBy { get; set; }

        [NotMapped]
        public string? CreatedBy { get; set; }

        [NotMapped]
        public string? InitializingBy { get; set; }

        [NotMapped]
        public string? InitializedBy { get; set; }
    }

    protected class Pamphlet
    {
        public Pamphlet(Guid id, string? title)
        {
            Id = id;
            Title = title;
        }

        public Guid Id { get; set; }
        public string? Title { get; set; }
    }

    public class TestEntity30244
    {
        [DatabaseGenerated((DatabaseGeneratedOption.None))]
        public int Id { get; set; }

        public string? Name { get; set; }
        public List<KeyValueSetting30244> Settings { get; } = new();
    }

    public class KeyValueSetting30244
    {
        public KeyValueSetting30244(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }

    public abstract class LibraryContext : PoolableDbContext
    {
        protected LibraryContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(
                b =>
                {
                    b.Property<string?>("Author");
                });

            modelBuilder.Entity<Pamphlet>(
                b =>
                {
                    b.Property<string?>("Author");
                });
        }
    }

    public TContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject, bool usePooling)
    {
        var contextFactory = base.Initialize<TContext>(
            onConfiguring: inject ? null : o => o.AddInterceptors(interceptors),
            addServices: inject ? s => InjectInterceptors(s, interceptors) : null,
            usePooling: usePooling,
            useServiceProvider: inject);

        return contextFactory.CreateContext();
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
