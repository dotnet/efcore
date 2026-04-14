// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Query.Internal;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Query;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantCast
#nullable enable

public class CSharpToLinqTranslatorTest
{
    [Fact]
    public void ArrayCreation()
        => AssertExpression(
            () => new int[3],
            "new int[3]");

    // ReSharper disable RedundantExplicitArrayCreation
    [Fact]
    public void ArrayCreation_with_initializer()
        => AssertExpression(
            () => new int[] { 1, 2 },
            "new int[] { 1, 2 }");
    // ReSharper restore RedundantExplicitArrayCreation

    // ReSharper disable BuiltInTypeReferenceStyle
    [Fact]
    public void As()
        => AssertExpression(
            () => "foo" as String,
            """ "foo" as String""");
    // ReSharper restore BuiltInTypeReferenceStyle

    [Fact]
    public void As_with_predefined_type()
        => AssertExpression(
            () => "foo" as string,
            """ "foo" as string""");

    [Theory]
    [InlineData("1 + 2", ExpressionType.Add)]
    [InlineData("1 - 2", ExpressionType.Subtract)]
    [InlineData("1 * 2", ExpressionType.Multiply)]
    [InlineData("1 / 2", ExpressionType.Divide)]
    [InlineData("1 % 2", ExpressionType.Modulo)]
    [InlineData("1 & 2", ExpressionType.And)]
    [InlineData("1 | 2", ExpressionType.Or)]
    [InlineData("1 ^ 2", ExpressionType.ExclusiveOr)]
    [InlineData("1 >> 2", ExpressionType.RightShift)]
    [InlineData("1 << 2", ExpressionType.LeftShift)]
    [InlineData("1 < 2", ExpressionType.LessThan)]
    [InlineData("1 <= 2", ExpressionType.LessThanOrEqual)]
    [InlineData("1 > 2", ExpressionType.GreaterThan)]
    [InlineData("1 >= 2", ExpressionType.GreaterThanOrEqual)]
    [InlineData("1 == 2", ExpressionType.Equal)]
    [InlineData("1 != 2", ExpressionType.NotEqual)]
    public void Binary_int(string code, ExpressionType binaryType)
        => AssertExpression(
            MakeBinary(binaryType, Constant(1), Constant(2)),
            code);

    [Theory]
    [InlineData("true && false", ExpressionType.AndAlso)]
    [InlineData("true || false", ExpressionType.OrElse)]
    [InlineData("true ^ false", ExpressionType.ExclusiveOr)]
    public void Binary_bool(string code, ExpressionType binaryType)
        => AssertExpression(
            Lambda<Func<bool>>(
                MakeBinary(binaryType, Constant(true), Constant(false))),
            code);

    [Fact]
    public void Binary_add_string()
        => AssertExpression(
            () => new[] { "foo", "bar" }.Select(s => s + "foo"),
            """new[] { "foo", "bar" }.Select(s => s + "foo")""");

    [Fact]
    public void Cast()
        => AssertExpression(
            () => (object)1,
            "(object)1");

    [Fact]
    public void Coalesce()
        => AssertExpression(
            () => (object?)"foo" ?? (object)"bar",
            """(object?)"foo" ?? (object)"bar" """);

    [Fact]
    public void ElementAccess_over_array()
        => AssertExpression(
            () => new[] { 1, 2, 3 }[1],
            "new[] { 1, 2, 3 } [1]");

    [Fact]
    public void ElementAccess_over_list()
        => AssertExpression(
            () => new List<int>
            {
                1,
                2,
                3
            }[1],
            "new List<int> { 1, 2, 3 }[1]");

    [Fact]
    public void IdentifierName_for_lambda_parameter()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.Where(i => i == 2),
            "new[] { 1, 2, 3 }.Where(i => i == 2);");

    [Fact]
    public void ImplicitArrayCreation()
        => AssertExpression(
            () => new[] { 1, 2 },
            "new[] { 1, 2 }");

    [Fact]
    public void Interpolated_string()
        => AssertExpression(
            () => string.Format("Foo: {0}", new[] { (object)8 }),
            """$"Foo: {8}" """);

    [Fact]
    public void Interpolated_string_formattable()
        => AssertExpression(
            () => FormattableStringMethod(FormattableStringFactory.Create("Foo: {0}, {1}", (object)8, (object)9)),
            """CSharpToLinqTranslatorTest.FormattableStringMethod($"Foo: {8}, {9}")""");

    [Fact]
    public void Index_over_array()
        => AssertExpression(
            () => new[] { 1, 2 }[0],
            "new[] { 1, 2 }[0]");

    [Fact]
    public void Index_over_List()
        => AssertExpression(
            () => new List<int> { 1, 2 }[0],
            "new List<int> { 1, 2 }[0]");

    [Fact]
    public void Invocation_instance_method()
        => AssertExpression(
            () => "foo".Substring(2),
            """ "foo".Substring(2)""");

    [Fact]
    public void Invocation_method_with_optional_parameter()
        => AssertExpression(
            Call(
                typeof(CSharpToLinqTranslatorTest).GetMethod(nameof(ParamsAndOptionalMethod), [typeof(int), typeof(int), typeof(int[])])!,
                Constant(1),
                Constant(4),
                NewArrayInit(typeof(int))),
            "CSharpToLinqTranslatorTest.ParamsAndOptionalMethod(1, 4)");

    [Fact]
    public void Invocation_method_with_optional_parameter_missing_argument()
        => AssertExpression(
            Call(
                typeof(CSharpToLinqTranslatorTest).GetMethod(nameof(ParamsAndOptionalMethod), [typeof(int), typeof(int), typeof(int[])])!,
                Constant(1),
                Constant(3),
                NewArrayInit(typeof(int))),
            "CSharpToLinqTranslatorTest.ParamsAndOptionalMethod(1)");

    [Fact]
    public void Invocation_method_with_params_parameter_no_arguments()
        => AssertExpression(
            Call(
                typeof(CSharpToLinqTranslatorTest).GetMethod(nameof(ParamsAndOptionalMethod), [typeof(int), typeof(int), typeof(int[])])!,
                Constant(1),
                Constant(4),
                NewArrayInit(typeof(int))),
            "CSharpToLinqTranslatorTest.ParamsAndOptionalMethod(1, 4)");

    [Fact]
    public void Invocation_method_with_params_parameter_one_argument()
        => AssertExpression(
            Call(
                typeof(CSharpToLinqTranslatorTest).GetMethod(nameof(ParamsAndOptionalMethod), [typeof(int), typeof(int), typeof(int[])])!,
                Constant(1),
                Constant(4),
                NewArrayInit(typeof(int), Constant(5))),
            "CSharpToLinqTranslatorTest.ParamsAndOptionalMethod(1, 4, 5)");

    [Fact]
    public void Invocation_method_with_params_parameter_multiple_arguments()
        => AssertExpression(
            Call(
                typeof(CSharpToLinqTranslatorTest).GetMethod(nameof(ParamsAndOptionalMethod), [typeof(int), typeof(int), typeof(int[])])!,
                Constant(1),
                Constant(4),
                NewArrayInit(typeof(int), Constant(5), Constant(6))),
            "CSharpToLinqTranslatorTest.ParamsAndOptionalMethod(1, 4, 5, 6)");

    [Fact]
    public void Invocation_method_with_params_parameter_missing_argument()
        => AssertExpression(
            Call(
                typeof(CSharpToLinqTranslatorTest).GetMethod(nameof(ParamsAndOptionalMethod), [typeof(int), typeof(int), typeof(int[])])!,
                Constant(1),
                Constant(3),
                NewArrayInit(typeof(int))),
            "CSharpToLinqTranslatorTest.ParamsAndOptionalMethod(1)");

    [Fact]
    public void Invocation_static_method()
        => AssertExpression(
            () => DateTime.Parse("2020-01-01"),
            """DateTime.Parse("2020-01-01")""");

    [Fact]
    public void Invocation_extension_method()
        => AssertExpression(
            () => typeof(string).GetTypeInfo(),
            "typeof(string).GetTypeInfo()");

    // ReSharper disable InvokeAsExtensionMethod
    [Fact]
    public void Invocation_extension_method_with_non_extension_syntax()
        => AssertExpression(
            () => IntrospectionExtensions.GetTypeInfo(typeof(string)),
            "typeof(string).GetTypeInfo()");
    // ReSharper restore InvokeAsExtensionMethod

    [Fact]
    public void Invocation_generic_method()
        => AssertExpression(
            () => Enumerable.Repeat("foo", 5),
            """Enumerable.Repeat("foo", 5)""");

    [Fact]
    public void Invocation_generic_extension_method()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.Where(i => i > 1),
            "new[] { 1, 2, 3 }.Where(i => i > 1)");

    [Fact]
    public void Invocation_generic_queryable_extension_method()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.AsQueryable().Where(i => i > 1),
            "new[] { 1, 2, 3 }.AsQueryable().Where(i => i > 1)");

    [Fact]
    public void Invocation_non_generic_method_on_generic_type()
        => AssertExpression(
            () => SomeGenericType<int>.SomeFunction(1),
            "CSharpToLinqTranslatorTest.SomeGenericType<int>.SomeFunction(1)");

    [Fact]
    public void Invocation_generic_method_on_generic_type()
        => AssertExpression(
            () => SomeGenericType<int>.SomeGenericFunction<string>(1, "foo"),
            """CSharpToLinqTranslatorTest.SomeGenericType<int>.SomeGenericFunction<string>(1, "foo")""");

    [Theory]
    [InlineData(
        """
        "hello"
        """, "hello")]
    [InlineData("1", 1)]
    [InlineData("1L", 1L)]
    [InlineData("1U", 1U)]
    [InlineData("1UL", 1UL)]
    [InlineData("1.5D", 1.5)]
    [InlineData("1.5F", 1.5F)]
    [InlineData("true", true)]
    public void Literal(string csharpLiteral, object expectedValue)
        => AssertExpression(
            Constant(expectedValue),
            csharpLiteral);

    [Fact]
    public void Literal_decimal()
        => AssertExpression(
            () => 1.5m,
            "1.5m");

    [Fact]
    public void Literal_null()
        => AssertExpression(
            Equal(Constant("foo"), Constant(null, typeof(string))),
            """ "foo" == null""");

    [Fact]
    public void Literal_enum()
        => AssertExpression(
            () => SomeEnum.Two,
            "CSharpToLinqTranslatorTest.SomeEnum.Two");

    [Fact]
    public void Literal_enum_with_multiple_values()
        => AssertExpression(
            Convert(
                Or(
                    Convert(Constant(SomeEnum.One), typeof(int)),
                    Convert(Constant(SomeEnum.Two), typeof(int))),
                typeof(SomeEnum)),
            "CSharpToLinqTranslatorTest.SomeEnum.One | CSharpToLinqTranslatorTest.SomeEnum.Two");

    [Fact]
    public void MemberAccess_array_length()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.Length,
            "new[] { 1, 2, 3 }.Length");

    [Fact]
    public void MemberAccess_instance_property()
        => AssertExpression(
            () => "foo".Length,
            """ "foo".Length""");

    [Fact]
    public void MemberAccess_static_property()
        => AssertExpression(
            () => DateTime.Now,
            "DateTime.Now");

    // TODO: MemberAccess on fields

    [Fact]
    public void Nested_type()
        => AssertExpression(
            () => (object)new Blog(),
            "(object)new CSharpToLinqTranslatorTest.Blog()");

    [Fact]
    public void Not_boolean()
        => AssertExpression(
            Not(Constant(true)),
            "!true");

    [Fact]
    public void ObjectCreation()
        => AssertExpression(
            () => new List<int>(),
            "new List<int>()");

    [Fact]
    public void ObjectCreation_with_arguments()
        => AssertExpression(
            () => new List<int>(10),
            "new List<int>(10)");

    [Fact]
    public void ObjectCreation_with_initializers()
        => AssertExpression(
            () => new Blog(8) { Name = "foo" },
            """new CSharpToLinqTranslatorTest.Blog(8) { Name = "foo" }""");

    [Fact]
    public void ObjectCreation_with_parameterless_struct_constructor()
        => AssertExpression(
            () => new DateTime(),
            "new DateTime()");

    [Fact]
    public void Parenthesized()
        => AssertExpression(
            () => 1,
            "(1)");

    [Theory]
    [InlineData("+8", 8, ExpressionType.UnaryPlus)]
    [InlineData("-8", 8, ExpressionType.Negate)]
    [InlineData("~8", 8, ExpressionType.Not)]
    public void PrefixUnary(string code, object operandValue, ExpressionType expectedNodeType)
        => AssertExpression(
            MakeUnary(expectedNodeType, Constant(8), typeof(int)),
            code);

    // ReSharper disable RedundantSuppressNullableWarningExpression
    [Fact]
    public void SuppressNullableWarningExpression()
        => AssertExpression(
            () => "foo"!,
            """ "foo"! """);
    // ReSharper restore RedundantSuppressNullableWarningExpression

    [ConditionalFact]
    public void Typeof()
        => AssertExpression(
            () => typeof(string),
            "typeof(string)");

    [ConditionalFact]
    public void Array_type()
        => AssertExpression(
            () => typeof(ParameterExpression[]),
            "typeof(ParameterExpression[])");

    protected virtual void AssertExpression<T>(Expression<Func<T>> expected, string code)
        => AssertExpression(
            expected.Body,
            code);

    protected virtual void AssertExpression(Expression expected, string code)
    {
        code = $"""
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

_ = {code};
""";

        var compilation = Compile(code);

        var syntaxTree = compilation.SyntaxTrees.Single();

        if (syntaxTree.GetRoot() is CompilationUnitSyntax { Members: [GlobalStatementSyntax globalStatement, ..] })
        {
            var expression = globalStatement switch
            {
                { Statement: ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax { Right: var e } } } => e,
                { Statement: LocalDeclarationStatementSyntax e } => e.Declaration.Variables[0].Initializer!.Value,
                { Statement: ExpressionStatementSyntax { Expression: var e } } => e,

                _ => throw new InvalidOperationException("Could not find expression to assert on")
            };

            var actual = Translate(expression, compilation);

            Assert.Equal(expected, actual, ExpressionEqualityComparer.Instance);
        }
        else
        {
            Assert.Fail("Could not find expression to assert on");
        }
    }

    private Compilation Compile(string code)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var compilation = CSharpCompilation.Create(
            "TestCompilation",
            syntaxTrees: new[] { syntaxTree },
            references: MetadataReferences);

        var diagnostics = compilation.GetDiagnostics()
            .Where(d => d.Severity is DiagnosticSeverity.Error)
            .ToArray();

        if (diagnostics.Any())
        {
            var stringBuilder = new StringBuilder()
                .AppendLine("Compilation errors:");

            foreach (var diagnostic in diagnostics)
            {
                stringBuilder.AppendLine(diagnostic.ToString());
            }

            Assert.Fail(stringBuilder.ToString());
        }

        return compilation;
    }

    private Expression Translate(SyntaxNode node, Compilation compilation)
    {
        var blogContext = new BlogContext();
        var translator = new CSharpToLinqTranslator();
        translator.Load(compilation, blogContext);
        return translator.Translate(node, compilation.GetSemanticModel(node.SyntaxTree));
    }

    private static readonly MetadataReference[] MetadataReferences;

    static CSharpToLinqTranslatorTest()
    {
        var metadataReferences = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Queryable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(DbContext).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(BlogContext).Assembly.Location)
        };

        var netAssemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "mscorlib.dll")));
        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.dll")));
        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Core.dll")));
        metadataReferences.Add(MetadataReference.CreateFromFile(Path.Combine(netAssemblyPath, "System.Runtime.dll")));

        MetadataReferences = metadataReferences.ToArray();
    }

    [Flags]
    public enum SomeEnum
    {
        One = 1,
        Two = 2
    }

    private class BlogContext : DbContext;

    public class Blog
    {
        public Blog()
        {
        }

        public Blog(int id)
            => Id = id;

        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class SomeGenericType<T1>
    {
        public static int SomeFunction(T1 t1)
            => 0;

        public static int SomeGenericFunction<T2>(T1 t1, T2 t2)
            => 0;
    }

    public static int ParamsAndOptionalMethod(int a, int b = 3, params int[] c)
        => throw new NotSupportedException();

    public static int FormattableStringMethod(FormattableString formattableString)
        => throw new NotSupportedException();
}
