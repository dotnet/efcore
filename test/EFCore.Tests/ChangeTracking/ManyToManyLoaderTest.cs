// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class ManyToManyLoaderTest
{
    [Fact]
    public void Provider_can_replace_many_to_many_loader_factory()
    {
        using var context = new ReplacedFactoryContext();

        var left = new Left();
        var right = new Right();
        left.Rights.Add(right);
        context.AddRange(left, right);
        context.SaveChanges();
        context.ChangeTracker.Clear();

        var tracked = context.Set<Left>().Single();

        // Accessing the collection loader must route through the replaced factory.
        Assert.Throws<CustomLoaderSentinelException>(
            () => context.Entry(tracked).Collection(e => e.Rights).Load());
    }

    [Fact]
    public void Generated_loader_factory_delegate_routes_through_the_runtime_factory()
    {
        using var context = new DelegatePathContext();

        var skipNavigation = (RuntimeSkipNavigation)context.Model
            .FindEntityType(typeof(Left))!
            .FindSkipNavigation(nameof(Left.Rights))!;

        // Simulate what the compiled-model generator emits for native AOT: a static delegate
        // that carries the concrete generic types but defers creation to the runtime factory.
        skipNavigation.SetManyToManyLoaderFactory(
            static (factory, navigation) => factory.Create<Right, Left>(navigation));

        var loader = ((IRuntimeSkipNavigation)skipNavigation)
            .GetManyToManyLoader(new CustomManyToManyLoaderFactory());

        // The replaced factory governs creation even via the generated delegate.
        Assert.IsType<CustomLoader>(loader);
    }

    private class ReplacedFactoryContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(ReplacedFactoryContext))
                .ReplaceService<IManyToManyLoaderFactory, CustomManyToManyLoaderFactory>();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Left>().HasMany(e => e.Rights).WithMany(e => e.Lefts);
    }

    private class DelegatePathContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(DelegatePathContext));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Left>().HasMany(e => e.Rights).WithMany(e => e.Lefts);
    }

    private class CustomManyToManyLoaderFactory : IManyToManyLoaderFactory
    {
        public ICollectionLoader Create(ISkipNavigation skipNavigation)
            => new CustomLoader();

        public ICollectionLoader Create<TEntity, TSourceEntity>(ISkipNavigation skipNavigation)
            where TEntity : class
            where TSourceEntity : class
            => new CustomLoader();
    }

    private class CustomLoader : ICollectionLoader
    {
        public void Load(InternalEntityEntry entry, LoadOptions options)
            => throw new CustomLoaderSentinelException();

        public Task LoadAsync(InternalEntityEntry entry, LoadOptions options, CancellationToken cancellationToken = default)
            => throw new CustomLoaderSentinelException();

        public IQueryable Query(InternalEntityEntry entry)
            => throw new CustomLoaderSentinelException();
    }

    private sealed class CustomLoaderSentinelException : Exception { }

    private class Left
    {
        public int Id { get; set; }
        public List<Right> Rights { get; } = new();
    }

    private class Right
    {
        public int Id { get; set; }
        public List<Left> Lefts { get; } = new();
    }
}
