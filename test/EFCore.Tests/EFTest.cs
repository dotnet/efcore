// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class EFTest
{
    [ConditionalFact]
    public void Property_throws_when_invoked_outside_of_query()
        => Assert.Equal(
            CoreStrings.PropertyMethodInvoked,
            Assert.Throws<InvalidOperationException>(() => EF.Property<object>(new object(), "")).Message);

    [ConditionalFact]
    public void CompiledQuery_throws_when_used_with_different_models()
    {
        using var context1 = new SwitchContext();
        using var context2 = new SwitchContext();

        var query = EF.CompileQuery((SwitchContext c, Bar p1) => c.Foos.Where(e => e.Bars.Contains(p1)));

        _ = query(context1, new Bar()).ToList();
        _ = query(context1, new Bar()).ToList();

        Assert.Equal(
            CoreStrings.CompiledQueryDifferentModel("(c, p1) => c.Foos .Where(e => e.Bars.Contains(p1))"),
            Assert.Throws<InvalidOperationException>(
                    () => query(context2, new Bar()).ToList())
                .Message.Replace("\r", "").Replace("\n", ""), ignoreWhiteSpaceDifferences: true);

        _ = query(context1, new Bar()).ToList();
    }

    [ConditionalFact]
    public async Task CompiledQueryAsync_throws_when_used_with_different_models()
    {
        using var context1 = new SwitchContext();
        using var context2 = new SwitchContext();

        var query = EF.CompileAsyncQuery((SwitchContext c) => c.Foos);

        _ = await query(context1).ToListAsync();
        _ = await query(context1).ToListAsync();

        Assert.Equal(
            CoreStrings.CompiledQueryDifferentModel("c => c.Foos"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => query(context2).ToListAsync())).Message);

        _ = await query(context1).ToListAsync();
    }

    private class Foo
    {
        public int Id { get; set; }
        public List<Bar> Bars { get; } = [];
    }

    private class Bar
    {
        public int Id { get; set; }
    }

    private class SwitchContext : DbContext
    {
        public DbSet<Foo> Foos
            => Set<Foo>();

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(SwitchContext))
                .ReplaceService<IModelCacheKeyFactory, DegenerateCacheKeyFactory>();
    }

    private class DegenerateCacheKeyFactory : IModelCacheKeyFactory
    {
        private static int _value;

        public object Create(DbContext context, bool designTime)
            => _value++;
    }
}
