// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public abstract class BindingInterceptionTestBase : SingletonInterceptorsTestBase
{
    protected BindingInterceptionTestBase(SingletonInterceptorsFixtureBase fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Binding_interceptors_are_used_by_queries(bool inject)
    {
        var interceptors = new[]
        {
            new TestBindingInterceptor("1"),
            new TestBindingInterceptor("2"),
            new TestBindingInterceptor("3"),
            new TestBindingInterceptor("4")
        };

        using var context = CreateContext(interceptors, inject);

        context.AddRange(
            new Book { Id = inject ? 77 : 87, Title = "Amiga ROM Kernel Reference Manual" },
            new Book { Id = inject ? 78 : 88, Title = "Amiga Hardware Reference Manual" });

        context.SaveChanges();
        context.ChangeTracker.Clear();

        var results = context.Set<Book>().ToList();
        Assert.All(results, e => Assert.Equal("4", e.MaterializedBy));
        Assert.All(interceptors, i => Assert.Equal(1, i.CalledCount));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Binding_interceptors_are_used_when_creating_instances(bool inject)
    {
        var interceptors = new[]
        {
            new TestBindingInterceptor("1"),
            new TestBindingInterceptor("2"),
            new TestBindingInterceptor("3"),
            new TestBindingInterceptor("4")
        };

        using var context = CreateContext(interceptors, inject);

        var materializer = context.GetService<IEntityMaterializerSource>();
        var book = (Book)materializer.GetEmptyMaterializer(context.Model.FindEntityType(typeof(Book))!)(
            new MaterializationContext(ValueBuffer.Empty, context));

        Assert.Equal("4", book.MaterializedBy);
        Assert.All(interceptors, i => Assert.Equal(1, i.CalledCount));
    }

    protected class TestBindingInterceptor : IInstantiationBindingInterceptor
    {
        private readonly string _id;

        public TestBindingInterceptor(string id)
        {
            _id = id;
        }

        public int CalledCount { get; private set; }

        protected Book BookFactory()
            => new() { MaterializedBy = _id };

        public InstantiationBinding ModifyBinding(IEntityType entityType, string entityInstanceName, InstantiationBinding binding)
        {
            CalledCount++;

            return new FactoryMethodBinding(
                this,
                typeof(TestBindingInterceptor).GetTypeInfo().GetDeclaredMethod(nameof(BookFactory))!,
                new List<ParameterBinding>(),
                entityType.ClrType);
        }
    }
}
