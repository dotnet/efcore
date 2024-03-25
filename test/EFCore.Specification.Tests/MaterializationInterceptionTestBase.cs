// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class MaterializationInterceptionTestBase<TContext> : SingletonInterceptorsTestBase<TContext>
    where TContext : SingletonInterceptorsTestBase<TContext>.LibraryContext
{
    protected override string StoreName
        => "MaterializationInterception";

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool, bool>))]
    public virtual async Task Binding_interceptors_are_used_by_queries(bool inject, bool usePooling)
    {
        var interceptors = new[]
        {
            new TestBindingInterceptor("1"),
            new TestBindingInterceptor("2"),
            new TestBindingInterceptor("3"),
            new TestBindingInterceptor("4")
        };

        using var context = await CreateContext(interceptors, inject, usePooling);

        context.AddRange(
            new Book { Title = "Amiga ROM Kernel Reference Manual" },
            new Book { Title = "Amiga Hardware Reference Manual" });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await context.Set<Book>().ToListAsync();
        Assert.All(results, e => Assert.Equal("4", e.MaterializedBy));
        Assert.All(interceptors, i => Assert.Equal(1, i.CalledCount));
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool, bool>))]
    public virtual async Task Binding_interceptors_are_used_when_creating_instances(bool inject, bool usePooling)
    {
        var interceptors = new[]
        {
            new TestBindingInterceptor("1"),
            new TestBindingInterceptor("2"),
            new TestBindingInterceptor("3"),
            new TestBindingInterceptor("4")
        };

        using var context = await CreateContext(interceptors, inject, usePooling);

        var materializer = context.GetService<IEntityMaterializerSource>();
        var book = (Book)materializer.GetEmptyMaterializer(context.Model.FindEntityType(typeof(Book))!)(
            new MaterializationContext(ValueBuffer.Empty, context));

        Assert.Equal("4", book.MaterializedBy);
        Assert.All(interceptors, i => Assert.Equal(1, i.CalledCount));
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool, bool>))]
    public virtual async Task Intercept_query_materialization_for_empty_constructor(bool inject, bool usePooling)
    {
        var creatingInstanceCount = 0;
        var createdInstanceCount = 0;
        var initializingInstanceCount = 0;
        var initializedInstanceCount = 0;
        LibraryContext? context = null;
        var ids = new HashSet<Guid>();
        var titles = new HashSet<string?>();
        var authors = new HashSet<string?>();

        var interceptors = new[]
        {
            new ValidatingMaterializationInterceptor(
                (data, instance, method) =>
                {
                    Assert.Same(context, data.Context);
                    Assert.Same(data.Context.Model.FindEntityType(typeof(Book)), data.EntityType);
                    Assert.Equal(QueryTrackingBehavior.TrackAll, data.QueryTrackingBehavior);

                    var idProperty = data.EntityType.FindProperty(nameof(Book.Id))!;
                    var id = data.GetPropertyValue<Guid>(nameof(Book.Id))!;
                    Assert.Equal(id, data.GetPropertyValue(nameof(Book.Id)));
                    Assert.Equal(id, data.GetPropertyValue<Guid>(idProperty));
                    Assert.Equal(id, data.GetPropertyValue(idProperty));
                    ids.Add(id);

                    var titleProperty = data.EntityType.FindProperty(nameof(Book.Title))!;
                    var title = data.GetPropertyValue<string?>(nameof(Book.Title));
                    Assert.Equal(title, data.GetPropertyValue(nameof(Book.Title)));
                    Assert.Equal(title, data.GetPropertyValue<string?>(titleProperty));
                    Assert.Equal(title, data.GetPropertyValue(titleProperty));
                    titles.Add(title);

                    var authorProperty = data.EntityType.FindProperty("Author")!;
                    var author = data.GetPropertyValue<string?>("Author");
                    Assert.Equal(author, data.GetPropertyValue("Author"));
                    Assert.Equal(author, data.GetPropertyValue<string?>(authorProperty));
                    Assert.Equal(author, data.GetPropertyValue(authorProperty));
                    authors.Add(author);

                    switch (method)
                    {
                        case nameof(IMaterializationInterceptor.CreatingInstance):
                            creatingInstanceCount++;
                            Assert.Null(instance);
                            break;
                        case nameof(IMaterializationInterceptor.CreatedInstance):
                            createdInstanceCount++;
                            Assert.IsType<Book>(instance);
                            Assert.Equal(Guid.Empty, ((Book)instance!).Id);
                            Assert.Null(((Book)instance!).Title);
                            break;
                        case nameof(IMaterializationInterceptor.InitializingInstance):
                            initializingInstanceCount++;
                            Assert.IsType<Book>(instance);
                            Assert.Equal(Guid.Empty, ((Book)instance!).Id);
                            Assert.Null(((Book)instance!).Title);
                            break;
                        case nameof(IMaterializationInterceptor.InitializedInstance):
                            initializedInstanceCount++;
                            Assert.IsType<Book>(instance);
                            Assert.Equal(id, ((Book)instance!).Id);
                            Assert.Equal(title, ((Book)instance!).Title);
                            break;
                    }
                })
        };

        using (context = await CreateContext(interceptors, inject, usePooling))
        {
            var books = new[]
            {
                new Book { Title = "Amiga ROM Kernel Reference Manual" }, new Book { Title = "Amiga Hardware Reference Manual" }
            };

            context.AddRange(books);

            context.Entry(books[0]).Property("Author").CurrentValue = "Commodore Business Machines Inc.";
            context.Entry(books[1]).Property("Author").CurrentValue = "Agnes";

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var results = await context.Set<Book>().Where(e => books.Select(e => e.Id).Contains(e.Id)).ToListAsync();
            Assert.Equal(2, results.Count);

            Assert.Equal(2, creatingInstanceCount);
            Assert.Equal(2, createdInstanceCount);
            Assert.Equal(2, initializingInstanceCount);
            Assert.Equal(2, initializedInstanceCount);

            Assert.Equal(2, ids.Count);
            Assert.Equal(2, titles.Count);
            Assert.Equal(2, authors.Count);
            Assert.Contains(ids, t => t == books[0].Id);
            Assert.Contains(ids, t => t == books[1].Id);
            Assert.Contains(titles, t => t == "Amiga ROM Kernel Reference Manual");
            Assert.Contains(titles, t => t == "Amiga Hardware Reference Manual");
            Assert.Contains(authors, t => t == "Commodore Business Machines Inc.");
            Assert.Contains(authors, t => t == "Agnes");
        }
    }

    private static int _id;

    [ConditionalTheory] // Issue #30244
    [ClassData(typeof(DataGenerator<bool, bool>))]
    public virtual async Task Intercept_query_materialization_with_owned_types(bool async, bool usePooling)
    {
        var creatingInstanceCounts = new Dictionary<Type, int>();
        var createdInstanceCounts = new Dictionary<Type, int>();
        var initializingInstanceCounts = new Dictionary<Type, int>();
        var initializedInstanceCounts = new Dictionary<Type, int>();
        LibraryContext? context = null;

        var interceptors = new[]
        {
            new ValidatingMaterializationInterceptor(
                (data, instance, method) =>
                {
                    Assert.Same(context, data.Context);
                    Assert.Equal(QueryTrackingBehavior.TrackAll, data.QueryTrackingBehavior);

                    int count;
                    var clrType = data.EntityType.ClrType;
                    switch (method)
                    {
                        case nameof(IMaterializationInterceptor.CreatingInstance):
                            count = creatingInstanceCounts.GetOrAddNew(clrType);
                            creatingInstanceCounts[clrType] = count + 1;
                            Assert.Null(instance);
                            break;
                        case nameof(IMaterializationInterceptor.CreatedInstance):
                            count = createdInstanceCounts.GetOrAddNew(clrType);
                            createdInstanceCounts[clrType] = count + 1;
                            Assert.Same(clrType, instance!.GetType());
                            break;
                        case nameof(IMaterializationInterceptor.InitializingInstance):
                            count = initializingInstanceCounts.GetOrAddNew(clrType);
                            initializingInstanceCounts[clrType] = count + 1;
                            Assert.Same(clrType, instance!.GetType());
                            break;
                        case nameof(IMaterializationInterceptor.InitializedInstance):
                            count = initializedInstanceCounts.GetOrAddNew(clrType);
                            initializedInstanceCounts[clrType] = count + 1;
                            Assert.Same(clrType, instance!.GetType());
                            break;
                    }
                })
        };

        using (context = await CreateContext(interceptors, inject: true, usePooling))
        {
            context.Add(
                new TestEntity30244
                {
                    Id = _id++,
                    Name = "TestIssue",
                    Settings = { new KeyValueSetting30244("Value1", "1"), new KeyValueSetting30244("Value2", "9") }
                });

            _ = async
                ? await context.SaveChangesAsync()
                : context.SaveChanges();

            context.ChangeTracker.Clear();

            var entity = async
                ? await context.Set<TestEntity30244>().OrderBy(e => e.Id).FirstOrDefaultAsync()
                : context.Set<TestEntity30244>().OrderBy(e => e.Id).FirstOrDefault();

            Assert.NotNull(entity);
            Assert.Contains(("Value1", "1"), entity.Settings.Select(e => (e.Key, e.Value)));
            Assert.Contains(("Value2", "9"), entity.Settings.Select(e => (e.Key, e.Value)));

            Assert.Equal(2, creatingInstanceCounts.Count);
            Assert.Equal(1, creatingInstanceCounts[typeof(TestEntity30244)]);
            Assert.Equal(2, creatingInstanceCounts[typeof(KeyValueSetting30244)]);

            Assert.Equal(2, createdInstanceCounts.Count);
            Assert.Equal(1, createdInstanceCounts[typeof(TestEntity30244)]);
            Assert.Equal(2, createdInstanceCounts[typeof(KeyValueSetting30244)]);

            Assert.Equal(2, initializingInstanceCounts.Count);
            Assert.Equal(1, initializingInstanceCounts[typeof(TestEntity30244)]);
            Assert.Equal(2, initializingInstanceCounts[typeof(KeyValueSetting30244)]);

            Assert.Equal(2, initializedInstanceCounts.Count);
            Assert.Equal(1, initializedInstanceCounts[typeof(TestEntity30244)]);
            Assert.Equal(2, initializedInstanceCounts[typeof(KeyValueSetting30244)]);
        }
    }

    [ConditionalTheory] // Issue #31365
    [ClassData(typeof(DataGenerator<bool, bool>))]
    public virtual async Task Intercept_query_materialization_with_owned_types_projecting_collection(bool async, bool usePooling)
    {
        var creatingInstanceCounts = new Dictionary<Type, int>();
        var createdInstanceCounts = new Dictionary<Type, int>();
        var initializingInstanceCounts = new Dictionary<Type, int>();
        var initializedInstanceCounts = new Dictionary<Type, int>();
        LibraryContext? context = null;

        var interceptors = new[]
        {
            new ValidatingMaterializationInterceptor(
                (data, instance, method) =>
                {
                    Assert.Same(context, data.Context);
                    Assert.Equal(QueryTrackingBehavior.NoTracking, data.QueryTrackingBehavior);

                    int count;
                    var clrType = data.EntityType.ClrType;
                    switch (method)
                    {
                        case nameof(IMaterializationInterceptor.CreatingInstance):
                            count = creatingInstanceCounts.GetOrAddNew(clrType);
                            creatingInstanceCounts[clrType] = count + 1;
                            Assert.Null(instance);
                            break;
                        case nameof(IMaterializationInterceptor.CreatedInstance):
                            count = createdInstanceCounts.GetOrAddNew(clrType);
                            createdInstanceCounts[clrType] = count + 1;
                            Assert.Same(clrType, instance!.GetType());
                            break;
                        case nameof(IMaterializationInterceptor.InitializingInstance):
                            count = initializingInstanceCounts.GetOrAddNew(clrType);
                            initializingInstanceCounts[clrType] = count + 1;
                            Assert.Same(clrType, instance!.GetType());
                            break;
                        case nameof(IMaterializationInterceptor.InitializedInstance):
                            count = initializedInstanceCounts.GetOrAddNew(clrType);
                            initializedInstanceCounts[clrType] = count + 1;
                            Assert.Same(clrType, instance!.GetType());
                            break;
                    }
                })
        };

        using (context = await CreateContext(interceptors, inject: true, usePooling))
        {
            context.Add(
                new TestEntity30244
                {
                    Id = _id++,
                    Name = "TestIssue",
                    Settings = { new KeyValueSetting30244("Value1", "1"), new KeyValueSetting30244("Value2", "9") }
                });

            _ = async
                ? await context.SaveChangesAsync()
                : context.SaveChanges();

            context.ChangeTracker.Clear();

            var query = context.Set<TestEntity30244>()
                .AsNoTracking()
                .OrderBy(e => e.Id)
                .Select(x => x.Settings.Where(s => s.Key != "Foo").ToList());

            var collection = async
                ? await query.FirstOrDefaultAsync()
                : query.FirstOrDefault();

            Assert.NotNull(collection);
            Assert.Equal("Value1", collection[0].Key);
            Assert.Equal("1", collection[0].Value);
            Assert.Contains(("Value2", "9"), collection.Select(x => (x.Key, x.Value)));

            Assert.Equal(1, creatingInstanceCounts.Count);
            Assert.Equal(2, creatingInstanceCounts[typeof(KeyValueSetting30244)]);

            Assert.Equal(1, createdInstanceCounts.Count);
            Assert.Equal(2, createdInstanceCounts[typeof(KeyValueSetting30244)]);

            Assert.Equal(1, initializingInstanceCounts.Count);
            Assert.Equal(2, initializingInstanceCounts[typeof(KeyValueSetting30244)]);

            Assert.Equal(1, initializedInstanceCounts.Count);
            Assert.Equal(2, initializedInstanceCounts[typeof(KeyValueSetting30244)]);
        }
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool, bool>))]
    public virtual async Task Intercept_query_materialization_for_full_constructor(bool inject, bool usePooling)
    {
        var creatingInstanceCount = 0;
        var createdInstanceCount = 0;
        var initializingInstanceCount = 0;
        var initializedInstanceCount = 0;
        LibraryContext? context = null;
        var ids = new HashSet<Guid>();
        var titles = new HashSet<string?>();
        var authors = new HashSet<string?>();

        var interceptors = new[]
        {
            new ValidatingMaterializationInterceptor(
                (data, instance, method) =>
                {
                    Assert.Same(context, data.Context);
                    Assert.Same(data.Context.Model.FindEntityType(typeof(Pamphlet)), data.EntityType);
                    Assert.Equal(QueryTrackingBehavior.TrackAll, data.QueryTrackingBehavior);

                    var idProperty = data.EntityType.FindProperty(nameof(Pamphlet.Id))!;
                    var id = data.GetPropertyValue<Guid>(nameof(Pamphlet.Id))!;
                    Assert.Equal(id, data.GetPropertyValue(nameof(Pamphlet.Id)));
                    Assert.Equal(id, data.GetPropertyValue<Guid>(idProperty));
                    Assert.Equal(id, data.GetPropertyValue(idProperty));
                    ids.Add(id);

                    var titleProperty = data.EntityType.FindProperty(nameof(Pamphlet.Title))!;
                    var title = data.GetPropertyValue<string?>(nameof(Pamphlet.Title));
                    Assert.Equal(title, data.GetPropertyValue(nameof(Pamphlet.Title)));
                    Assert.Equal(title, data.GetPropertyValue<string?>(titleProperty));
                    Assert.Equal(title, data.GetPropertyValue(titleProperty));
                    titles.Add(title);

                    var authorProperty = data.EntityType.FindProperty("Author")!;
                    var author = data.GetPropertyValue<string?>("Author");
                    Assert.Equal(author, data.GetPropertyValue("Author"));
                    Assert.Equal(author, data.GetPropertyValue<string?>(authorProperty));
                    Assert.Equal(author, data.GetPropertyValue(authorProperty));
                    authors.Add(author);

                    switch (method)
                    {
                        case nameof(IMaterializationInterceptor.CreatingInstance):
                            creatingInstanceCount++;
                            Assert.Null(instance);
                            break;
                        case nameof(IMaterializationInterceptor.CreatedInstance):
                            createdInstanceCount++;
                            Assert.IsType<Pamphlet>(instance);
                            Assert.Equal(id, ((Pamphlet)instance!).Id);
                            Assert.Equal(title, ((Pamphlet)instance!).Title);
                            break;
                        case nameof(IMaterializationInterceptor.InitializingInstance):
                            initializingInstanceCount++;
                            Assert.IsType<Pamphlet>(instance);
                            Assert.Equal(id, ((Pamphlet)instance!).Id);
                            Assert.Equal(title, ((Pamphlet)instance!).Title);
                            break;
                        case nameof(IMaterializationInterceptor.InitializedInstance):
                            initializedInstanceCount++;
                            Assert.IsType<Pamphlet>(instance);
                            Assert.Equal(id, ((Pamphlet)instance!).Id);
                            Assert.Equal(title, ((Pamphlet)instance!).Title);
                            break;
                    }
                })
        };

        using (context = await CreateContext(interceptors, inject, usePooling))
        {
            var pamphlets = new[] { new Pamphlet(Guid.Empty, "Rights of Man"), new Pamphlet(Guid.Empty, "Pamphlet des pamphlets") };

            context.AddRange(pamphlets);

            context.Entry(pamphlets[0]).Property("Author").CurrentValue = "Thomas Paine";
            context.Entry(pamphlets[1]).Property("Author").CurrentValue = "Paul-Louis Courier";

            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var results = await context.Set<Pamphlet>().Where(e => pamphlets.Select(e => e.Id).Contains(e.Id)).ToListAsync();
            Assert.Equal(2, results.Count);

            Assert.Equal(2, creatingInstanceCount);
            Assert.Equal(2, createdInstanceCount);
            Assert.Equal(2, initializingInstanceCount);
            Assert.Equal(2, initializedInstanceCount);

            Assert.Equal(2, ids.Count);
            Assert.Equal(2, titles.Count);
            Assert.Equal(2, authors.Count);
            Assert.Contains(ids, t => t == pamphlets[0].Id);
            Assert.Contains(ids, t => t == pamphlets[1].Id);
            Assert.Contains(titles, t => t == "Rights of Man");
            Assert.Contains(titles, t => t == "Pamphlet des pamphlets");
            Assert.Contains(authors, t => t == "Thomas Paine");
            Assert.Contains(authors, t => t == "Paul-Louis Courier");
        }
    }

    [ConditionalTheory]
    [ClassData(typeof(DataGenerator<bool, bool>))]
    public virtual async Task Multiple_materialization_interceptors_can_be_used(bool inject, bool usePooling)
    {
        var interceptors = new ISingletonInterceptor[]
        {
            new CountingMaterializationInterceptor("A"),
            new TestBindingInterceptor("1"),
            new CountingMaterializationInterceptor("B"),
            new TestBindingInterceptor("2"),
            new TestBindingInterceptor("3"),
            new TestBindingInterceptor("4"),
            new CountingMaterializationInterceptor("C")
        };

        using var context = await CreateContext(interceptors, inject, usePooling);

        context.AddRange(
            new Book { Title = "Amiga ROM Kernel Reference Manual" },
            new Book { Title = "Amiga Hardware Reference Manual" });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var results = await context.Set<Book>().ToListAsync();
        Assert.All(results, e => Assert.Equal("4", e.MaterializedBy));
        Assert.All(interceptors.OfType<TestBindingInterceptor>(), i => Assert.Equal(1, i.CalledCount));

        Assert.All(results, e => Assert.Equal("ABC", e.CreatedBy));
        Assert.All(results, e => Assert.Equal("ABC", e.InitializingBy));
        Assert.All(results, e => Assert.Equal("ABC", e.InitializedBy));
    }

    protected class TestBindingInterceptor(string id) : IInstantiationBindingInterceptor
    {
        private readonly string _id = id;

        public int CalledCount { get; private set; }

        protected Book BookFactory()
            => new() { MaterializedBy = _id };

        public InstantiationBinding ModifyBinding(InstantiationBindingInterceptionData interceptionData, InstantiationBinding binding)
        {
            CalledCount++;

            return new FactoryMethodBinding(
                this,
                typeof(TestBindingInterceptor).GetTypeInfo().GetDeclaredMethod(nameof(BookFactory))!,
                new List<ParameterBinding>(),
                interceptionData.TypeBase.ClrType);
        }
    }

    protected class ValidatingMaterializationInterceptor(
        Action<MaterializationInterceptionData, object?, string> validate) : IMaterializationInterceptor
    {
        private readonly Action<MaterializationInterceptionData, object?, string> _validate = validate;

        public InterceptionResult<object> CreatingInstance(
            MaterializationInterceptionData materializationData,
            InterceptionResult<object> result)
        {
            _validate(materializationData, null, nameof(CreatingInstance));

            return result;
        }

        public object CreatedInstance(
            MaterializationInterceptionData materializationData,
            object entity)
        {
            _validate(materializationData, entity, nameof(CreatedInstance));

            return entity;
        }

        public InterceptionResult InitializingInstance(
            MaterializationInterceptionData materializationData,
            object entity,
            InterceptionResult result)
        {
            _validate(materializationData, entity, nameof(InitializingInstance));

            return result;
        }

        public object InitializedInstance(
            MaterializationInterceptionData materializationData,
            object entity)
        {
            _validate(materializationData, entity, nameof(InitializedInstance));

            return entity;
        }
    }

    protected class CountingMaterializationInterceptor(string id) : IMaterializationInterceptor
    {
        private readonly string _id = id;

        public InterceptionResult<object> CreatingInstance(
            MaterializationInterceptionData materializationData,
            InterceptionResult<object> result)
            => result;

        public object CreatedInstance(
            MaterializationInterceptionData materializationData,
            object entity)
        {
            ((Book)entity).CreatedBy += _id;
            return entity;
        }

        public InterceptionResult InitializingInstance(
            MaterializationInterceptionData materializationData,
            object entity,
            InterceptionResult result)
        {
            ((Book)entity).InitializingBy += _id;
            return result;
        }

        public object InitializedInstance(
            MaterializationInterceptionData materializationData,
            object entity)
        {
            ((Book)entity).InitializedBy += _id;
            return entity;
        }
    }
}
