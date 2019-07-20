// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
#pragma warning disable RCS1102 // Make class static.
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class DbFunctionMetadataTests
    {
        public class MyNonDbContext
        {
            public int NonStatic()
            {
                throw new Exception();
            }

            public static int DuplicateNameTest()
            {
                throw new Exception();
            }
        }

        public class MyBaseContext : DbContext
        {
            public static readonly string[] FunctionNames =
            {
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
            };

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
            public static int StaticPublicBase() => throw new Exception();

            [DbFunction]
            protected static int StaticProtectedBase() => throw new Exception();

            [DbFunction]
            private static int StaticPrivateBase() => throw new Exception();

            [DbFunction]
            internal static int StaticInternalBase() => throw new Exception();

            [DbFunction]
            protected internal static int StaticProtectedInternalBase() => throw new Exception();

            [DbFunction]
            public int InstancePublicBase() => throw new Exception();

            [DbFunction]
            protected int InstanceProtectedBase() => throw new Exception();

            [DbFunction]
            private int InstancePrivateBase() => throw new Exception();

            [DbFunction]
            internal int InstanceInternalBase() => throw new Exception();

            [DbFunction]
            protected internal int InstanceProtectedInternalBase() => throw new Exception();

            [DbFunction]
            public virtual int VirtualBase() => throw new Exception();
        }

        public class MyDerivedContext : MyBaseContext
        {
            public static new readonly string[] FunctionNames =
            {
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
            };

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
            {
                throw new Exception();
            }

            [DbFunction]
            public static int StaticPublicDerived() => throw new Exception();

            [DbFunction]
            protected static int StaticProtectedDerived() => throw new Exception();

            [DbFunction]
            private static int StaticPrivateDerived() => throw new Exception();

            [DbFunction]
            internal static int StaticInternalDerived() => throw new Exception();

            [DbFunction]
            protected internal static int StaticProtectedInternalDerived() => throw new Exception();

            [DbFunction]
            public int InstancePublicDerived() => throw new Exception();

            [DbFunction]
            protected int InstanceProtectedDerived() => throw new Exception();

            [DbFunction]
            private int InstancePrivateDerived() => throw new Exception();

            [DbFunction]
            internal int InstanceInternalDerived() => throw new Exception();

            [DbFunction]
            protected internal int InstanceProtectedInternalDerived() => throw new Exception();

            [DbFunction]
            public override int VirtualBase() => throw new Exception();
        }

        public static MethodInfo MethodAmi = typeof(TestMethods).GetRuntimeMethod(
            nameof(TestMethods.MethodA), new[] { typeof(string), typeof(int) });

        public static MethodInfo MethodBmi = typeof(TestMethods).GetRuntimeMethod(
            nameof(TestMethods.MethodB), new[] { typeof(string), typeof(int) });

        public static MethodInfo MethodImi = typeof(TestMethods).GetRuntimeMethod(
            nameof(TestMethods.MethodI), new Type[] { });

        public static MethodInfo MethodHmi = typeof(TestMethods).GetTypeInfo().GetDeclaredMethod(nameof(TestMethods.MethodH));

        public class TestMethods
        {
            public static int Foo => 1;

            public static int MethodA(string a, int b)
            {
                throw new NotImplementedException();
            }

            [DbFunction(Schema = "bar", FunctionName = "MethodFoo")]
            public static int MethodB(string c, int d)
            {
                throw new NotImplementedException();
            }

            public static void MethodC()
            {
            }

            public static TestMethods MethodD()
            {
                throw new NotImplementedException();
            }

            public static int MethodF(MyBaseContext context)
            {
                throw new NotImplementedException();
            }

            public static int MethodH<T>(T a, string b)
            {
                throw new Exception();
            }

            public static int MethodI()
            {
                throw new Exception();
            }
        }

        [ConditionalFact]
        public virtual void DbFunctions_with_duplicate_names_and_parameters_on_different_types_dont_collide()
        {
            var modelBuilder = GetModelBuilder();

            var dup1methodInfo
                = typeof(MyDerivedContext)
                    .GetRuntimeMethod(nameof(MyDerivedContext.DuplicateNameTest), Array.Empty<Type>());

            var dup2methodInfo
                = typeof(MyNonDbContext)
                    .GetRuntimeMethod(nameof(MyNonDbContext.DuplicateNameTest), Array.Empty<Type>());

            var dbFunc1 = modelBuilder.HasDbFunction(dup1methodInfo).HasName("Dup1").Metadata;
            var dbFunc2 = modelBuilder.HasDbFunction(dup2methodInfo).HasName("Dup2").Metadata;

            Assert.Equal("Dup1", dbFunc1.FunctionName);
            Assert.Equal("Dup2", dbFunc2.FunctionName);
        }

        [ConditionalFact]
        public virtual void Finds_dbFunctions_on_dbContext()
        {
            var context = new MyDerivedContext();
            var modelBuilder = GetModelBuilder(context);

            foreach (var function in MyBaseContext.FunctionNames)
            {
                Assert.NotNull(
                    modelBuilder.Model.FindDbFunction(
                        typeof(MyBaseContext).GetMethod(
                            function, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)));
            }

            foreach (var function in MyDerivedContext.FunctionNames)
            {
                Assert.NotNull(
                    modelBuilder.Model.FindDbFunction(
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
                    .GetRuntimeMethod(nameof(MyDerivedContext.InstancePublicBase), Array.Empty<Type>());

            var dbFunc = modelBuilder.HasDbFunction(methodInfo).Metadata;

            Assert.Equal("InstancePublicBase", dbFunc.FunctionName);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [ConditionalFact]
        public virtual void Non_static_function_on_non_dbcontext_throws()
        {
            var modelBuilder = GetModelBuilder();

            var methodInfo
                = typeof(MyNonDbContext)
                    .GetRuntimeMethod(nameof(MyNonDbContext.NonStatic), Array.Empty<Type>());

            Assert.Equal(
                RelationalStrings.DbFunctionInvalidInstanceType(methodInfo.DisplayName(), typeof(MyNonDbContext).ShortDisplayName()),
                Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(methodInfo)).Message);
        }

        [ConditionalFact]
        public void Detects_void_return_throws()
        {
            var modelBuilder = GetModelBuilder();

            var methodInfo = typeof(TestMethods).GetRuntimeMethod(nameof(TestMethods.MethodC), Array.Empty<Type>());

            Assert.Equal(
                RelationalStrings.DbFunctionInvalidReturnType(methodInfo.DisplayName(), typeof(void).ShortDisplayName()),
                Assert.Throws<ArgumentException>(() => modelBuilder.HasDbFunction(methodInfo)).Message);
        }

        [ConditionalFact]
        public void Adding_method_fluent_only_convention_defaults()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodA", dbFunc.FunctionName);
            Assert.Null(dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [ConditionalFact]
        public void Adding_method_fluent_only_convention_defaults_fluent_method_info()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(() => TestMethods.MethodA(null, default));
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodA", dbFunc.FunctionName);
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

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [ConditionalFact]
        public void Adding_method_fluent_only_with_builder()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDbFunction(MethodAmi, funcBuilder => funcBuilder.HasName("foo").HasSchema("bar"));

            var dbFunc = modelBuilder.HasDbFunction(MethodAmi).Metadata;

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [ConditionalFact]
        public void Adding_method_with_attribute_only()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("MethodFoo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [ConditionalFact]
        public void Adding_method_with_attribute_and_fluent_api_configuration_source()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi)
                .HasName("foo")
                .HasSchema("bar");

            var dbFunc = dbFuncBuilder.Metadata;

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [ConditionalFact]
        public void Adding_method_with_attribute_and_fluent_configuration_source()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDbFunction(MethodBmi, funcBuilder => funcBuilder.HasName("foo").HasSchema("bar"));

            var dbFunc = modelBuilder.HasDbFunction(MethodBmi).Metadata;

            Assert.Equal("foo", dbFunc.FunctionName);
            Assert.Equal("bar", dbFunc.Schema);
            Assert.Equal(typeof(int), dbFunc.MethodInfo.ReturnType);
        }

        [ConditionalFact]
        public void Adding_method_with_relational_schema()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi);

            Assert.Equal("dbo", dbFuncBuilder.Metadata.Schema);
        }

        [ConditionalFact]
        public void Adding_method_with_store_type()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi).HasStoreType("int(8)");

            Assert.Equal("int(8)", dbFuncBuilder.Metadata.StoreType);
        }

        [ConditionalFact]
        public void Adding_method_with_relational_schema_fluent_overrides()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodAmi).HasSchema("bar");

            Assert.Equal("bar", dbFuncBuilder.Metadata.Schema);
        }

        [ConditionalFact]
        public void Adding_method_with_relational_schema_attribute_overrides()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder.HasDefaultSchema("dbo");

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);

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

            Assert.Equal(0, dbFunc.Parameters.Count);
        }

        [ConditionalFact]
        public void DbParameters_invalid_parameter_name_throws()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);

            Assert.Equal(
                RelationalStrings.DbFunctionInvalidParameterName("q", dbFuncBuilder.Metadata.MethodInfo.DisplayName()),
                Assert.Throws<ArgumentException>(() => dbFuncBuilder.HasParameter("q")).Message);
        }

        [ConditionalFact]
        public void DbParameters_load_with_parameters()
        {
            var modelBuilder = GetModelBuilder();

            var dbFuncBuilder = modelBuilder.HasDbFunction(MethodBmi);
            var dbFunc = dbFuncBuilder.Metadata;

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

            Assert.Equal(2, dbFunc.Parameters.Count);

            Assert.Equal("c", dbFunc.Parameters[0].Name);
            Assert.Equal(typeof(string), dbFunc.Parameters[0].ClrType);
            Assert.Equal("varchar(max)", dbFunc.Parameters[0].StoreType);

            Assert.Equal("d", dbFunc.Parameters[1].Name);
            Assert.Equal(typeof(int), dbFunc.Parameters[1].ClrType);
        }

        private ModelBuilder GetModelBuilder(DbContext dbContext = null)
        {
            var conventionSet = new ConventionSet();

            var dependencies = CreateDependencies().With(new CurrentDbContext(dbContext ?? new DbContext(new DbContextOptions<DbContext>())));
            var relationalDependencies = CreateRelationalDependencies();
            var dbFunctionAttributeConvention = new RelationalDbFunctionAttributeConvention(dependencies, relationalDependencies);
            conventionSet.ModelInitializedConventions.Add(dbFunctionAttributeConvention);
            conventionSet.ModelAnnotationChangedConventions.Add(dbFunctionAttributeConvention);
            conventionSet.ModelFinalizedConventions.Add(new DbFunctionTypeMappingConvention(dependencies, relationalDependencies));

            return new ModelBuilder(conventionSet);
        }

        private ProviderConventionSetBuilderDependencies CreateDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        private RelationalConventionSetBuilderDependencies CreateRelationalDependencies()
            => RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<RelationalConventionSetBuilderDependencies>();
    }
}
