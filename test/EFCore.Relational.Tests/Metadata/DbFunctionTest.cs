// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
#pragma warning disable RCS1102 // Make class static.
namespace Microsoft.EntityFrameworkCore.Metadata;

public class DbFunctionTest
{
    protected class Foo
    {
        public int I { get; set; }
        public int J { get; set; }
    }

    protected class MyNonDbContext
    {
        public int NonStatic()
            => throw new Exception();

        public static int DuplicateNameTest()
            => throw new Exception();
    }

    protected class MyBaseContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseFakeRelational();

        public static readonly string[] FunctionNames =
        [
            nameof(StaticPublicBase),
            nameof(StaticProtectedBase),
            nameof(StaticPrivateBase),
            nameof(StaticInternalBase),
            nameof(StaticProtectedInternalBase),
            nameof(InstancePublicBase),
            nameof(InstanceProtectedBase),
            nameof(InstancePrivateBase),
            nameof(InstanceInternalBase),
            nameof(InstanceProtectedInternalBase)
        ];

        public static void Foo()
        {
        }

        public static void Skip2()
        {
        }

        private static void Skip()
        {
        }

        [DbFunction]
        public static int StaticPublicBase()
            => throw new Exception();

        [DbFunction]
        protected static int StaticProtectedBase()
            => throw new Exception();

        [DbFunction]
        private static int StaticPrivateBase()
            => throw new Exception();

        [DbFunction]
        internal static int StaticInternalBase()
            => throw new Exception();

        [DbFunction]
        protected internal static int StaticProtectedInternalBase()
            => throw new Exception();

        [DbFunction]
        public int InstancePublicBase()
            => throw new Exception();

        [DbFunction]
        protected int InstanceProtectedBase()
            => throw new Exception();

        [DbFunction]
        private int InstancePrivateBase()
            => throw new Exception();

        [DbFunction]
        internal int InstanceInternalBase()
            => throw new Exception();

        [DbFunction]
        protected internal int InstanceProtectedInternalBase()
            => throw new Exception();

        [DbFunction]
        public virtual int VirtualBase()
            => throw new Exception();
    }

    protected class MyDerivedContext : MyBaseContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Foo>().HasNoKey();

        public static new readonly string[] FunctionNames =
        [
            nameof(StaticPublicDerived),
            nameof(StaticProtectedDerived),
            nameof(StaticPrivateDerived),
            nameof(StaticInternalDerived),
            nameof(StaticProtectedInternalDerived),
            nameof(InstancePublicDerived),
            nameof(InstanceProtectedDerived),
            nameof(InstancePrivateDerived),
            nameof(InstanceInternalDerived),
            nameof(InstanceProtectedInternalDerived)
        ];

        public static void Bar()
        {
        }

        public static void Skip3()
        {
        }

        private static void Skip4()
        {
        }

        public static int DuplicateNameTest()
            => throw new Exception();

        [DbFunction]
        public static int StaticPublicDerived()
            => throw new Exception();

        [DbFunction]
        protected static int StaticProtectedDerived()
            => throw new Exception();

        [DbFunction]
        private static int StaticPrivateDerived()
            => throw new Exception();

        [DbFunction]
        internal static int StaticInternalDerived()
            => throw new Exception();

        [DbFunction]
        protected internal static int StaticProtectedInternalDerived()
            => throw new Exception();

        [DbFunction]
        public int InstancePublicDerived()
            => throw new Exception();

        [DbFunction]
        protected int InstanceProtectedDerived()
            => throw new Exception();

        [DbFunction]
        private int InstancePrivateDerived()
            => throw new Exception();

        [DbFunction]
        internal int InstanceInternalDerived()
            => throw new Exception();

        [DbFunction]
        protected internal int InstanceProtectedInternalDerived()
            => throw new Exception();

        [DbFunction]
        public override int VirtualBase()
            => throw new Exception();

        [DbFunction]
        public IQueryable<Foo> QueryableNoParams()
            => throw new Exception();

        [DbFunction]
        public IQueryable<Foo> QueryableSingleParam(int i)
            => throw new Exception();

        public IQueryable<Foo> QueryableSingleParam(Expression<Func<int>> i)
            => throw new Exception();

        [DbFunction]
        public IQueryable<Foo> QueryableMultiParam(int i, double j)
            => throw new Exception();

        public IQueryable<Foo> QueryableMultiParam(Expression<Func<int>> i, double j)
            => throw new Exception();

        public IQueryable<Foo> QueryableMultiParam(Expression<Func<int>> i, Expression<Func<double>> j)
            => throw new Exception();
    }

    private static readonly MethodInfo MethodAmi = typeof(TestMethods).GetRuntimeMethod(
        nameof(TestMethods.MethodA), [typeof(string), typeof(int)]);

    private static readonly MethodInfo MethodBmi = typeof(TestMethods).GetRuntimeMethod(
        nameof(TestMethods.MethodB), [typeof(string), typeof(int)]);

    private static readonly MethodInfo MethodImi = typeof(TestMethods).GetRuntimeMethod(
        nameof(TestMethods.MethodI), []);

    private static readonly MethodInfo MethodHmi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(TestMethods.MethodH));

    private static readonly MethodInfo MethodJmi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(TestMethods.MethodJ));

    private class TestMethods
    {
        public static int Foo
            => 1;

        public static int MethodA(string a, int b)
            => throw new NotImplementedException();

        [DbFunction("MethodFoo", "bar")]
        public static int MethodB(string c, int d)
            => throw new NotImplementedException();

        public static void MethodC()
        {
        }

        public static TestMethods MethodD()
            => throw new NotImplementedException();

        public static int MethodF(MyBaseContext context)
            => throw new NotImplementedException();

        public static int MethodH<T>(T a, string b)
            => throw new Exception();

        public static int MethodI()
            => throw new Exception();

        public static IQueryable<TestMethods> MethodJ()
            => throw new Exception();

        public static IQueryable<TestMethods> MethodK(int id)
            => throw new Exception();
    }

    private static class OuterA
    {
        public static class Inner
        {
            public static decimal? Min(decimal? a, decimal? b)
                => throw new Exception();
        }
    }

    private static class OuterB
    {
        public static class Inner
        {
            public static decimal? Min(decimal? a, decimal? b)
                => throw new Exception();
        }
    }

    [ConditionalFact]
    public virtual void DbFunctions_with_duplicate_names_and_parameters_on_different_types_dont_collide()
    {
        var modelBuilder = GetModelBuilder();

        var dup1methodInfo
            = typeof(MyDerivedContext)
                .GetRuntimeMethod(nameof(MyDerivedContext.DuplicateNameTest), []);

        var dup2methodInfo
            = typeof(MyNonDbContext)
                .GetRuntimeMethod(nameof(MyNonDbContext.DuplicateNameTest), []);

        var dbFunc1 = modelBuilder.HasDbFunction(dup1methodInfo).HasName("Dup1").Metadata;
        var dbFunc2 = modelBuilder.HasDbFunction(dup2methodInfo).HasName("Dup2").Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal("Dup1", dbFunc1.Name);
        Assert.Equal("Dup2", dbFunc2.Name);
    }

    [ConditionalFact]
    public virtual void Finds_DbFunctions_on_DbContext()
    {
        var model = new MyDerivedContext().Model;

        foreach (var function in MyBaseContext.FunctionNames)
        {
            Assert.NotNull(
                model.FindDbFunction(
                    typeof(MyBaseContext).GetMethod(
                        function, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)));
        }

        foreach (var function in MyDerivedContext.FunctionNames)
        {
            Assert.NotNull(
                model.FindDbFunction(
                    typeof(MyDerivedContext).GetMethod(
                        function, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)));
        }
    }

    [ConditionalFact]
    public virtual void Non_static_function_on_dbcontext_does_not_throw()
    {
        var modelBuilder = GetModelBuilder();

        var methodInfo
            = typeof(MyDerivedContext)
                .GetRuntimeMethod(nameof(MyDerivedContext.InstancePublicBase), []);

        var dbFunc = modelBuilder.HasDbFunction(methodInfo).Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal("InstancePublicBase", dbFunc.Name);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        Assert.Equal(typeof(int), dbFunc.ReturnType);
    }

    [ConditionalFact]
    public virtual void Non_static_function_on_non_dbcontext_throws()
    {
        var modelBuilder = GetModelBuilder();

        var methodInfo
            = typeof(MyNonDbContext)
                .GetRuntimeMethod(nameof(MyNonDbContext.NonStatic), []);

        Assert.Equal(
            RelationalStrings.DbFunctionInvalidInstanceType(methodInfo.DisplayName(), typeof(MyNonDbContext).ShortDisplayName()),
            Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(methodInfo)).Message);
    }

    [ConditionalFact]
    public void Detects_void_return_throws()
    {
        var modelBuilder = GetModelBuilder();

        var methodInfo = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodC), []);

        Assert.Equal(
            RelationalStrings.DbFunctionInvalidReturnType(nameof(TestMethods.MethodC), typeof(void).ShortDisplayName()),
            Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(methodInfo)).Message);
    }

    [ConditionalFact]
    public void Adding_method_fluent_only_convention_defaults()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);
        var dbFunc = dbFuncBuilder.Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal("MethodA", dbFunc.Name);
        Assert.Null(dbFunc.Schema);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
    }

    [ConditionalFact]
    public void Adding_method_fluent_only_convention_defaults_fluent_method_info()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(() => TestMethods.MethodA(null, default));
        var dbFunc = dbFuncBuilder.Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal("MethodA", dbFunc.Name);
        Assert.Null(dbFunc.Schema);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
    }

    [ConditionalFact]
    public void Adding_method_fluent_only_convention_defaults_non_method_call_throws()
    {
        var modelBuilder = GetModelBuilder();

        Expression<Func<int>> expression = () => 1;

        Assert.Equal(
            RelationalStrings.DbFunctionExpressionIsNotMethodCall(expression),
            Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(expression)).Message);
    }

    [ConditionalFact]
    public void Adding_method_fluent_only_convention_defaults_property_call_throws()
    {
        var modelBuilder = GetModelBuilder();

        Expression<Func<int>> expression = () => TestMethods.Foo;

        Assert.Equal(
            RelationalStrings.DbFunctionExpressionIsNotMethodCall(expression),
            Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(expression)).Message);
    }

    [ConditionalFact]
    public void Adding_method_fluent_only_with_name_schema()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi)
            .HasName("foo")
            .HasSchema("bar");

        var dbFunc = dbFuncBuilder.Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal("foo", dbFunc.Name);
        Assert.Equal("bar", dbFunc.Schema);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
    }

    [ConditionalFact]
    public void Adding_method_fluent_only_with_builder()
    {
        var modelBuilder = GetModelBuilder();

        modelBuilder.HasDbFunction(MethodAmi, funcBuilder => funcBuilder.HasName("foo").HasSchema("bar"));

        var dbFunc = modelBuilder.HasDbFunction(MethodAmi).Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal("foo", dbFunc.Name);
        Assert.Equal("bar", dbFunc.Schema);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
    }

    [ConditionalFact]
    public void Adding_method_with_attribute_only()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
        var dbFunc = dbFuncBuilder.Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal("MethodFoo", dbFunc.Name);
        Assert.Equal("bar", dbFunc.Schema);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
    }

    [ConditionalFact]
    public void Adding_method_with_attribute_and_fluent_api_configuration_source()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi)
            .HasName(null)
            .HasSchema(null);

        var dbFunc = dbFuncBuilder.Metadata;

        Assert.Equal(MethodBmi.Name, dbFunc.Name);
        Assert.Null(dbFunc.Schema);

        dbFuncBuilder.HasName("foo");
        dbFuncBuilder.HasSchema("BAR");

        modelBuilder.FinalizeModel();

        Assert.Equal("foo", dbFunc.Name);
        Assert.Equal("BAR", dbFunc.Schema);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
    }

    [ConditionalFact]
    public void Adding_method_with_attribute_and_fluent_configuration_source()
    {
        var modelBuilder = GetModelBuilder();

        modelBuilder.HasDbFunction(MethodBmi, funcBuilder => funcBuilder.HasName(null).HasSchema(null));

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
        var dbFunc = dbFuncBuilder.Metadata;

        Assert.Equal(MethodBmi.Name, dbFunc.Name);
        Assert.Null(dbFunc.Schema);

        dbFuncBuilder.HasName("foo");
        dbFuncBuilder.HasSchema("BAR");

        modelBuilder.FinalizeModel();

        Assert.Equal("foo", dbFunc.Name);
        Assert.Equal("BAR", dbFunc.Schema);
        Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
    }

    [ConditionalFact]
    public void Adding_method_with_relational_schema()
    {
        var modelBuilder = GetModelBuilder();

        modelBuilder.HasDefaultSchema("dbo");

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);

        modelBuilder.FinalizeModel();

        Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);
    }

    [ConditionalFact]
    public void Adding_method_with_store_type()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi).HasStoreType("int(8)");

        modelBuilder.FinalizeModel(skipValidation: true);

        Assert.Equal("int(8)", dbFuncBuilder.Metadata.StoreType);
    }

    [ConditionalFact]
    public void Adding_method_with_relational_schema_fluent_overrides()
    {
        var modelBuilder = GetModelBuilder();

        modelBuilder.HasDefaultSchema("dbo");

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi).HasSchema("bar");

        modelBuilder.FinalizeModel();

        Assert.Equal("bar", dbFuncBuilder.Metadata.Schema);
    }

    [ConditionalFact]
    public void Adding_method_with_relational_schema_attribute_overrides()
    {
        var modelBuilder = GetModelBuilder();

        modelBuilder.HasDefaultSchema("dbo");

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);

        modelBuilder.FinalizeModel();

        Assert.Equal("bar", dbFuncBuilder.Metadata.Schema);
    }

    [ConditionalFact]
    public void Changing_default_schema_is_detected_by_dbfunction()
    {
        var modelBuilder = GetModelBuilder();

        modelBuilder.HasDefaultSchema("abc");

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);

        Assert.Equal("abc", dbFuncBuilder.Metadata.Schema);

        modelBuilder.HasDefaultSchema("xyz");

        modelBuilder.FinalizeModel();

        Assert.Equal("xyz", dbFuncBuilder.Metadata.Schema);
    }

    [ConditionalFact]
    public void Add_method_generic_not_supported_throws()
    {
        var modelBuilder = GetModelBuilder();

        Assert.Equal(
            RelationalStrings.DbFunctionGenericMethodNotSupported(MethodHmi.DisplayName()),
            Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(MethodHmi)).Message);
    }

    [ConditionalFact]
    public void DbFunction_HasName()
    {
        var modelBuilder = GetModelBuilder();

        var methodA = typeof(OuterA.Inner).GetMethod(nameof(OuterA.Inner.Min));
        var methodB = typeof(OuterB.Inner).GetMethod(nameof(OuterB.Inner.Min));

        var funcA = modelBuilder.HasDbFunction(methodA);
        var funcB = modelBuilder.HasDbFunction(methodB);

        funcA.HasName("MinA");

        modelBuilder.FinalizeModel();

        Assert.Equal("MinA", funcA.Metadata.Name);
        Assert.Equal("Min", funcB.Metadata.Name);
        Assert.NotEqual(funcA.Metadata.Name, funcB.Metadata.Name);
    }

    [ConditionalFact]
    public void DbFunction_IsBuiltIn()
    {
        var modelBuilder = GetModelBuilder();

        var methodA = typeof(OuterA.Inner).GetMethod(nameof(OuterA.Inner.Min));

        var funcA = modelBuilder.HasDbFunction(methodA);

        Assert.False(funcA.Metadata.IsBuiltIn);

        funcA.IsBuiltIn();

        Assert.True(funcA.Metadata.IsBuiltIn);
    }

    [ConditionalFact]
    public virtual void Set_empty_function_name_throws()
    {
        var modelBuilder = GetModelBuilder();

        var expectedMessage = AbstractionsStrings.ArgumentIsEmpty("name");

        Assert.Equal(
            expectedMessage, Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(MethodAmi).HasName("")).Message);
    }

    [ConditionalFact]
    public void DbParameters_load_no_parameters()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodImi);
        var dbFunc = dbFuncBuilder.Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal(0, dbFunc.Parameters.Count);
    }

    [ConditionalFact]
    public void DbFunction_IsQueryable()
    {
        var modelBuilder = GetModelBuilder();

        var queryableNoParams
            = typeof(MyDerivedContext)
                .GetRuntimeMethod(nameof(MyDerivedContext.QueryableNoParams), []);

        var functionName = modelBuilder.HasDbFunction(queryableNoParams).Metadata.ModelName;

        var model = modelBuilder.FinalizeModel(skipValidation: true);

        var function = model.FindDbFunction(functionName);
        var entityType = model.FindEntityType(typeof(Foo));

        Assert.False(function.IsScalar);
        Assert.False(function.IsAggregate);
        var mapping = function.StoreFunction.EntityTypeMappings.Single();
        Assert.False(mapping.IsDefaultFunctionMapping);
        Assert.Same(entityType, mapping.TypeBase);
    }

    [ConditionalFact]
    public void IsNullable_throws_for_nonScalar()
    {
        var modelBuilder = GetModelBuilder();

        var queryableNoParams
            = typeof(MyDerivedContext)
                .GetRuntimeMethod(nameof(MyDerivedContext.QueryableNoParams), []);

        Assert.Equal(
            RelationalStrings.NonScalarFunctionCannotBeNullable(nameof(MyDerivedContext.QueryableNoParams)),
            Assert.Throws<InvalidOperationException>(() => modelBuilder.HasDbFunction(queryableNoParams).IsNullable()).Message);
    }

    [ConditionalFact]
    public void PropagatesNullability_throws_for_nonScalar()
    {
        var modelBuilder = GetModelBuilder();

        var queryableSingleParam = typeof(MyDerivedContext)
            .GetRuntimeMethod(nameof(MyDerivedContext.QueryableSingleParam), [typeof(int)]);

        var function = modelBuilder.HasDbFunction(queryableSingleParam);
        var parameter = function.HasParameter("i");

        Assert.Equal(
            RelationalStrings.NonScalarFunctionParameterCannotPropagatesNullability("i", nameof(MyDerivedContext.QueryableSingleParam)),
            Assert.Throws<InvalidOperationException>(() => parameter.PropagatesNullability()).Message);
    }

    [ConditionalFact]
    public void DbParameters_invalid_parameter_name_throws()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);

        Assert.Equal(
            RelationalStrings.DbFunctionInvalidParameterName(dbFuncBuilder.Metadata.MethodInfo.DisplayName(), "q"),
            Assert.Throws<ArgumentException>(() => dbFuncBuilder.HasParameter("q")).Message);
    }

    [ConditionalFact]
    public void DbParameters_load_with_parameters()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
        var dbFunc = dbFuncBuilder.Metadata;

        modelBuilder.FinalizeModel();

        Assert.Equal(2, dbFunc.Parameters.Count);

        Assert.Equal("c", dbFunc.Parameters[0].Name);
        Assert.Equal(typeof(string), dbFunc.Parameters[0].ClrType);

        Assert.Equal("d", dbFunc.Parameters[1].Name);
        Assert.Equal(typeof(int), dbFunc.Parameters[1].ClrType);
    }

    [ConditionalFact]
    public void DbParameters_dbfunctionType()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
        var dbFunc = dbFuncBuilder.Metadata;

        dbFuncBuilder.HasParameter("c");

        modelBuilder.FinalizeModel();

        Assert.Equal(2, dbFunc.Parameters.Count);

        Assert.Equal("c", dbFunc.Parameters[0].Name);
        Assert.Equal(typeof(string), dbFunc.Parameters[0].ClrType);

        Assert.Equal("d", dbFunc.Parameters[1].Name);
        Assert.Equal(typeof(int), dbFunc.Parameters[1].ClrType);
    }

    [ConditionalFact]
    public void DbParameters_name()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
        var dbFunc = dbFuncBuilder.Metadata;

        dbFuncBuilder.HasParameter("c");

        modelBuilder.FinalizeModel();

        Assert.Equal(2, dbFunc.Parameters.Count);

        Assert.Equal("c", dbFunc.Parameters[0].Name);
        Assert.Equal(typeof(string), dbFunc.Parameters[0].ClrType);

        Assert.Equal("d", dbFunc.Parameters[1].Name);
        Assert.Equal(typeof(int), dbFunc.Parameters[1].ClrType);
    }

    [ConditionalFact]
    public void DbParameters_StoreType()
    {
        var modelBuilder = GetModelBuilder();

        var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
        var dbFunc = dbFuncBuilder.Metadata;

        dbFuncBuilder.HasParameter("c").HasStoreType("varchar(max)");

        modelBuilder.FinalizeModel(skipValidation: true);

        Assert.Equal(2, dbFunc.Parameters.Count);

        Assert.Equal("c", dbFunc.Parameters[0].Name);
        Assert.Equal(typeof(string), dbFunc.Parameters[0].ClrType);
        Assert.Equal("varchar(max)", dbFunc.Parameters[0].StoreType);

        Assert.Equal("d", dbFunc.Parameters[1].Name);
        Assert.Equal(typeof(int), dbFunc.Parameters[1].ClrType);
    }

    [ConditionalFact]
    public void DbFunction_Queryable_custom_translation()
    {
        var modelBuilder = GetModelBuilder();
        var methodInfo = typeof(TestMethods).GetMethod(nameof(TestMethods.MethodJ));
        var dbFunctionBuilder = modelBuilder.HasDbFunction(methodInfo);

        Assert.False(
            dbFunctionBuilder.GetInfrastructure()
                .CanSetTranslation(args => new SqlFragmentExpression("Empty"), fromDataAnnotation: true));
        Assert.Null(dbFunctionBuilder.Metadata.Translation);

        dbFunctionBuilder.GetInfrastructure().HasTranslation(args => new SqlFragmentExpression("Empty"));
        Assert.Null(dbFunctionBuilder.Metadata.Translation);

        dbFunctionBuilder.GetInfrastructure()
            .HasTranslation(args => new SqlFragmentExpression("Empty"), fromDataAnnotation: true);
        Assert.Null(dbFunctionBuilder.Metadata.Translation);

        Assert.Equal(
            RelationalStrings.DbFunctionNonScalarCustomTranslation(methodInfo.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => dbFunctionBuilder.HasTranslation(args => new SqlFragmentExpression("Empty"))).Message);

        var dbFunction = dbFunctionBuilder.Metadata;

        Assert.Equal(
            RelationalStrings.DbFunctionNonScalarCustomTranslation(methodInfo.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => ((IConventionDbFunction)dbFunction).SetTranslation(args => new SqlFragmentExpression("Empty"))).Message);

        Assert.Equal(
            RelationalStrings.DbFunctionNonScalarCustomTranslation(methodInfo.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => ((IConventionDbFunction)dbFunction)
                    .SetTranslation(args => new SqlFragmentExpression("Empty"), fromDataAnnotation: true)).Message);

        Assert.Equal(
            RelationalStrings.DbFunctionNonScalarCustomTranslation(methodInfo.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => dbFunction.Translation = args => new SqlFragmentExpression("Empty")).Message);
    }

    private TestHelpers.TestModelBuilder GetModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
}
