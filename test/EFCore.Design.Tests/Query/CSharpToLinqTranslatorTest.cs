// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Query.Internal;

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
            @"""foo"" as String");
    // ReSharper restore BuiltInTypeReferenceStyle

    [Fact]
    public void As_with_predefined_type()
        => AssertExpression(
            () => "foo" as string,
            @"""foo"" as string");

    // TODO
//     [Theory]
//     [InlineData("=")]
//     [InlineData("+=")]
//     [InlineData("&=")]
//     [InlineData("/=")]
//     [InlineData("^=")]
//     [InlineData("<<=")]
//     [InlineData("%=")]
//     [InlineData("*=")]
//     [InlineData("|=")]
//     [InlineData(">>=")]
//     [InlineData("-=")]
//     public void Assignment_not_supported(string op)
//         => Assert.Throws<NotSupportedException>(
//             () => AssertCodeOld(
// $"""
// var i = 3;
// ~~~i {op} 2~~~;
// """,
//                 _ => {}));

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
            Expression.MakeBinary(binaryType, Expression.Constant(1), Expression.Constant(2)),
            code);

    [Theory]
    [InlineData("true && false", ExpressionType.AndAlso)]
    [InlineData("true || false", ExpressionType.OrElse)]
    [InlineData("true ^ false", ExpressionType.ExclusiveOr)]
    public void Binary_bool(string code, ExpressionType binaryType)
        => AssertExpression(
            Expression.Lambda<Func<bool>>(
                Expression.MakeBinary(binaryType, Expression.Constant(true), Expression.Constant(false))),
            code);

    [Fact]
    public void Binary_add_string()
        => AssertExpression(
            () => new[] { "foo", "bar" }.Select(s => s + "foo"),
            @"new[] { ""foo"", ""bar"" }.Select(s => s + ""foo"")");

    [Fact]
    public void Coalesce()
        => AssertExpression(
            () => (object?)"foo" ?? (object)"bar",
            @"(object?)""foo"" ?? (object)""bar""");

    [Fact]
    public void Convert()
        => AssertExpression(
            () => (object)1,
            @"(object)1");

    [Fact]
    public void ElementAccess_over_array()
        => AssertExpression(
            () => new[] { 1, 2, 3 }[1],
            @"new[] { 1, 2, 3 } [1]");

    [Fact]
    public void ElementAccess_over_list()
        => AssertExpression(
            () => new List<int> { 1, 2, 3 }[1],
            @"new List<int> { 1, 2, 3 }[1]");

    [Fact]
    public void IdentifierName_for_lambda_parameter()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.Where(i => i == 2),
            @"new[] { 1, 2, 3 }.Where(i => i == 2);");

    [Fact]
    public void IdentifierName_for_captured_variable()
        => throw new NotImplementedException();

    [Fact]
    public void ImplicitArrayCreation()
        => AssertExpression(
            () => new[] { 1, 2 },
            "new[] { 1, 2 }");

    [Fact]
    public void Invocation_instance_method()
        => AssertExpression(
            () => "foo".Substring(2),
            @"""foo"".Substring(2)");

    [Fact]
    public void Invocation_method_with_optional_parameter()
        => AssertExpression(
            Expression.Call(
                typeof(File).GetMethod(nameof(File.ReadAllTextAsync), new[] { typeof(string), typeof(CancellationToken) })!,
                Expression.Constant("/tmp/foo"),
                Expression.Constant(default(CancellationToken))),
            @"System.IO.File.ReadAllTextAsync(""/tmp/foo"")");

    [Fact]
    public void Invocation_static_method()
        => AssertExpression(
            () => DateTime.Parse("2020-01-01"),
            @"DateTime.Parse(""2020-01-01"")");

    [Fact]
    public void Invocation_extension_method()
        => AssertExpression(
            () => typeof(string).GetTypeInfo(),
            @"typeof(string).GetTypeInfo()");

    // ReSharper disable InvokeAsExtensionMethod
    [Fact]
    public void Invocation_extension_method_with_non_extension_syntax()
        => AssertExpression(
            () => IntrospectionExtensions.GetTypeInfo(typeof(string)),
            @"typeof(string).GetTypeInfo()");
    // ReSharper restore InvokeAsExtensionMethod

    [Fact]
    public void Invocation_generic_method()
        => AssertExpression(
            () => Enumerable.Repeat("foo", 5),
            @"Enumerable.Repeat(""foo"", 5)");

    [Fact]
    public void Invocation_generic_extension_method()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.Where(i => i > 1),
            @"new[] { 1, 2, 3 }.Where(i => i > 1)");

    [Fact]
    public void Invocation_generic_queryable_extension_method()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.AsQueryable().Where(i => i > 1),
            @"new[] { 1, 2, 3 }.AsQueryable().Where(i => i > 1)");

    [Fact]
    public void Invocation_non_generic_method_on_generic_type()
        => AssertExpression(
            () => SomeGenericType<int>.SomeFunction(1),
            @"CSharpToLinqTranslatorTest.SomeGenericType<int>.SomeFunction(1)");

    [Fact]
    public void Invocation_generic_method_on_generic_type()
        => AssertExpression(
            () => SomeGenericType<int>.SomeGenericFunction<string>(1, "foo"),
            @"CSharpToLinqTranslatorTest.SomeGenericType<int>.SomeGenericFunction<string>(1, ""foo"")");

    [Theory]
    [InlineData(@"""hello""", "hello")]
    [InlineData("1", 1)]
    [InlineData("1L", 1L)]
    [InlineData("1U", 1U)]
    [InlineData("1UL", 1UL)]
    [InlineData("1.5D", 1.5)]
    [InlineData("1.5F", 1.5F)]
    [InlineData("true", true)]
    public void Literal(string csharpLiteral, object expectedValue)
        => AssertExpression(
            Expression.Constant(expectedValue),
            csharpLiteral);

    [Fact]
    public void Literal_decimal()
        => AssertExpression(
            () => 1.5m,
            "1.5m");

    // TODO
    // public void Literal_null()
    //     => AssertCodeOld(
    //         "string s = ~~~null~~~;",
    //         actual =>
    //         {
    //             var constantExpression = Assert.IsAssignableFrom<ConstantExpression>(actual);
    //             Assert.Null(constantExpression.Value);
    //             Assert.Same(typeof(string), constantExpression.Type);
    //         });

    [Fact]
    public void Literal_enum()
        => AssertExpression(
            () => SomeEnum.Two,
            "CSharpToLinqTranslatorTest.SomeEnum.Two");

    [Fact]
    public void Literal_enum_with_multiple_values()
        => AssertExpression(
            () => SomeEnum.One | SomeEnum.Two,
            "CSharpToLinqTranslatorTest.SomeEnum.One | CSharpToLinqTranslatorTest.SomeEnum.Two");

    // [Fact]
    // public void Literal_enum_with_unknown_value()
    //     => AssertExpression(Constant((SomeEnum)1000), "(SomeEnum)1000L");

    [Fact]
    public void MemberAccess_array_length()
        => AssertExpression(
            () => new[] { 1, 2, 3 }.Length,
            "new[] { 1, 2, 3 }.Length");

    [Fact]
    public void MemberAccess_instance_property()
        => AssertExpression(
            () => "foo".Length,
            @"""foo"".Length;");

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
            @"(object)new CSharpToLinqTranslatorTest.Blog()");

    [Fact]
    public void Not_boolean()
        => AssertExpression(
            Expression.Not(Expression.Constant(true)),
            "!true");

    [Fact]
    public void ObjectCreation()
        => AssertExpression(
            () => new List<int>(),
            @"new List<int>()");

    [Fact]
    public void ObjectCreation_with_arguments()
        => AssertExpression(
            () => new List<int>(10),
            @"new List<int>(10)");

    [Fact]
    public void ObjectCreation_with_initializers()
        => AssertExpression(
            () => new Blog(8) { Name = "foo" },
            @"new CSharpToLinqTranslatorTest.Blog(8) { Name = ""foo"" }");

    [Fact]
    public void ObjectCreation_with_parameterless_struct_constructor()
        => AssertExpression(
            () => new DateTime(),
            @"new DateTime()");

    [Fact]
    public void Parenthesized()
        => AssertExpression(
            () => 1,
            @"(1)");

    [Theory]
    [InlineData("+8", 8, ExpressionType.UnaryPlus)]
    [InlineData("-8", 8, ExpressionType.Negate)]
    [InlineData("~8", 8, ExpressionType.Not)]
    public void PrefixUnary(string code, object operandValue, ExpressionType expectedNodeType)
        => AssertExpression(
            Expression.MakeUnary(expectedNodeType, Expression.Constant(8), typeof(int)),
            code);

    // ReSharper disable RedundantSuppressNullableWarningExpression
    [Fact]
    public void SuppressNullableWarningExpression()
        => AssertExpression(
            () => "foo"!,
            @"""foo""!");
    // ReSharper restore RedundantSuppressNullableWarningExpression

    [ConditionalFact]
    public void Typeof()
        => AssertExpression(
            () => typeof(string),
            @"typeof(string)");

    protected virtual void AssertExpression<T>(Expression<Func<T>> expected, string code)
        => AssertExpression(
            expected.Body,
            code);

    protected virtual void AssertExpression(Expression expected, string code)
    {
        code =
            "using System;" + Environment.NewLine +
            "using System.Collections.Generic;" + Environment.NewLine +
            "using System.Linq;" + Environment.NewLine +
            "using System.Reflection;" + Environment.NewLine +
            "using Microsoft.EntityFrameworkCore.Query;" + Environment.NewLine +
            Environment.NewLine +
            $"_ = {code};";

        var compilation = Compile(code);

        var syntaxTree = compilation.SyntaxTrees.Single();

        if (syntaxTree.GetRoot() is CompilationUnitSyntax { Members: [GlobalStatementSyntax globalStatement, ..] })
        {
            var expression = globalStatement switch
            {
                { Statement: ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax { Right: var e } } } => e,
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

    private class BlogContext : DbContext
    {
    }

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
}
