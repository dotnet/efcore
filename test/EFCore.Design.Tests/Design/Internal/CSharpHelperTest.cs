// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Internal;

#nullable enable

public class CSharpHelperTest
{
    private static readonly string EOL = Environment.NewLine;

    [ConditionalTheory]
    [InlineData(
        "single-line string with \"",
        "\"single-line string with \\\"\"")]
    [InlineData(
        true,
        "true")]
    [InlineData(
        false,
        "false")]
    [InlineData(
        (byte)42,
        "(byte)42")]
    [InlineData(
        'A',
        "'A'")]
    [InlineData(
        '\'',
        @"'\''")]
    [InlineData(
        4.2,
        "4.2000000000000002")]
    [InlineData(
        double.NegativeInfinity,
        "double.NegativeInfinity")]
    [InlineData(
        double.PositiveInfinity,
        "double.PositiveInfinity")]
    [InlineData(
        double.NaN,
        "double.NaN")]
    [InlineData(
        0.84551240822557006,
        "0.84551240822557006")]
    [InlineData(
        6E-14,
        "5.9999999999999997E-14")]
    [InlineData(
        -1.7976931348623157E+308, // Double MinValue
        "-1.7976931348623157E+308")]
    [InlineData(
        1.7976931348623157E+308, // Double MaxValue
        "1.7976931348623157E+308")]
    [InlineData(
        4.2f,
        "4.2f")]
    [InlineData(
        -3.402823E+38f, // Single MinValue
        "-3.402823E+38f")]
    [InlineData(
        3.402823E+38f, // Single MaxValue
        "3.402823E+38f")]
    [InlineData(
        42,
        "42")]
    [InlineData(
        42L,
        "42L")]
    [InlineData(
        9000000000000000000L, // Ensure not printed as exponent
        "9000000000000000000L")]
    [InlineData(
        (sbyte)42,
        "(sbyte)42")]
    [InlineData(
        (short)42,
        "(short)42")]
    [InlineData(
        42u,
        "42u")]
    [InlineData(
        42ul,
        "42ul")]
    [InlineData(
        18000000000000000000ul, // Ensure not printed as exponent
        "18000000000000000000ul")]
    [InlineData(
        (ushort)42,
        "(ushort)42")]
    [InlineData(
        "",
        "\"\"")]
    [InlineData(
        SomeEnum.Default,
        "CSharpHelperTest.SomeEnum.Default")]
    public void Literal_works(object value, string expected)
    {
        var literal = new CSharpHelper(TypeMappingSource).UnknownLiteral(value);
        Assert.Equal(expected, literal);
    }

    [ConditionalFact]
    public void Literal_works_when_empty_ByteArray()
        => Literal_works(
            Array.Empty<byte>(),
            "new byte[0]");

    [ConditionalFact]
    public void Literal_works_when_single_ByteArray()
        => Literal_works(
            new byte[] { 1 },
            "new byte[] { 1 }");

    [ConditionalFact]
    public void Literal_works_when_many_ByteArray()
        => Literal_works(
            new byte[] { 1, 2 },
            "new byte[] { 1, 2 }");

    [ConditionalFact]
    public void Literal_works_when_empty_list()
        => Literal_works(
            new List<string>(),
            @"new List<string>()");

    [ConditionalFact]
    public void Literal_works_when_list_with_single_element()
        => Literal_works(
            new List<string> { "one" },
            @"new List<string> { ""one"" }");

    [ConditionalFact]
    public void Literal_works_when_list_of_mixed_objects()
        => Literal_works(
            new List<object> { 1, "two" },
            @"new List<object> { 1, ""two"" }");

    [ConditionalFact]
    public void Literal_works_when_list_vertical()
        => Assert.Equal(
            @"new List<object>
{
    1,
    ""two""
}".ReplaceLineEndings(), new CSharpHelper(TypeMappingSource).Literal(
                new List<object> { 1, "two" }, true));

    [ConditionalFact]
    public void Literal_works_when_empty_dictionary()
        => Literal_works(
            new Dictionary<string, int>(),
            @"new Dictionary<string, int>()");

    [ConditionalFact]
    public void Literal_works_when_dictionary_with_single_element()
        => Literal_works(
            new Dictionary<string, string> { ["one"] = "value" },
            @"new Dictionary<string, string> { [""one""] = ""value"" }");

    [ConditionalFact]
    public void Literal_works_when_dictionary_of_mixed_objects()
        => Literal_works(
            new Dictionary<string, object> { ["one"] = 1, ["two"] = "Two" },
            @"new Dictionary<string, object> { [""one""] = 1, [""two""] = ""Two"" }");

    [ConditionalFact]
    public void Literal_works_when_dictionary_vertical()
        => Assert.Equal(
            @"new Dictionary<int, object>
{
    [1] = 1,
    [2] = ""Two""
}".ReplaceLineEndings(), new CSharpHelper(TypeMappingSource).Literal(
                new Dictionary<int, object> { [1] = 1, [2] = "Two" }, true));

    [ConditionalFact]
    public void Literal_works_when_multiline_string()
        => Literal_works(
            "multi-line\r\nstring\nwith\r\"",
            "\"multi-line\\r\\nstring\\nwith\\r\\\"\"");

    [ConditionalFact]
    public void Literal_works_when_value_tuple()
        => Literal_works((1, "hello"), "(1, \"hello\")");

    [ConditionalFact]
    public void Literal_works_when_value_tuple_with_null_value_type()
        => Literal_works((1, (int?)null, "hello"), "(1, (int?)null, \"hello\")");

    [ConditionalFact]
    public void Literal_works_when_value_tuple_with_null_reference_type()
        => Literal_works((1, (string?)null, "hello"), "(1, (string)null, \"hello\")");

    [ConditionalFact]
    public void Literal_works_when_value_tuple_of_length_1()
        => Literal_works(ValueTuple.Create(1), "ValueTuple.Create(1)");

    [ConditionalFact]
    public void Literal_works_when_value_tuple_of_length_9()
        => Literal_works((1, 2, 3, 4, 5, 6, 7, 8, 9), "(1, 2, 3, 4, 5, 6, 7, 8, 9)");

    [ConditionalFact]
    [UseCulture("de-DE")]
    public void Literal_works_when_DateTime()
        => Literal_works(
            new DateTime(2015, 3, 15, 20, 45, 17, 300, DateTimeKind.Local),
            "new DateTime(2015, 3, 15, 20, 45, 17, 300, DateTimeKind.Local)");

    [ConditionalFact]
    [UseCulture("de-DE")]
    public void Literal_works_when_DateTimeOffset()
        => Literal_works(
            new DateTimeOffset(new DateTime(2015, 3, 15, 19, 43, 47, 500), new TimeSpan(-7, 0, 0)),
            "new DateTimeOffset(new DateTime(2015, 3, 15, 19, 43, 47, 500, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0))");

    [ConditionalFact]
    public void Literal_works_when_decimal()
        => Literal_works(
            4.2m,
            "4.2m");

    [ConditionalFact]
    public void Literal_works_when_decimal_max_value()
        => Literal_works(
            79228162514264337593543950335m, // Decimal MaxValue
            "79228162514264337593543950335m");

    [ConditionalFact]
    public void Literal_works_when_decimal_min_value()
        => Literal_works(
            -79228162514264337593543950335m, // Decimal MinValue
            "-79228162514264337593543950335m");

    [ConditionalFact]
    public void Literal_works_when_Guid()
        => Literal_works(
            new Guid("fad4f3c3-9501-4b3a-af99-afeb496f7664"),
            "new Guid(\"fad4f3c3-9501-4b3a-af99-afeb496f7664\")");

    [ConditionalFact]
    public void Literal_works_when_TimeSpan()
        => Literal_works(
            new TimeSpan(17, 21, 42, 37, 250),
            "new TimeSpan(17, 21, 42, 37, 250)");

    [ConditionalFact]
    public void Literal_works_when_NullableInt()
        => Literal_works(
            (int?)42,
            "42");

    [ConditionalFact]
    public void Literal_works_when_StringArray()
    {
        var literal = new CSharpHelper(TypeMappingSource).Literal(new[] { "A", "B" });
        Assert.Equal("new[] { \"A\", \"B\" }", literal);
    }

    [ConditionalFact]
    public void Literal_works_when_empty_StringArray()
    {
        var literal = new CSharpHelper(TypeMappingSource).Literal(new string[] { });
        Assert.Equal("new string[0]", literal);
    }

    [ConditionalFact]
    public void Literal_works_when_ObjectArray()
    {
        var literal = new CSharpHelper(TypeMappingSource).Literal(new object[] { 'A', 1 });
        Assert.Equal("new object[] { 'A', 1 }", literal);
    }

    [ConditionalFact]
    public void Literal_works_when_MultidimensionalArray()
    {
        var value = new object[,] { { 'A', 1 }, { 'B', 2 } };

        var result = new CSharpHelper(TypeMappingSource).Literal(value);

        Assert.Equal(
            "new object[,]" + EOL + "{" + EOL + "    { 'A', 1 }," + EOL + "    { 'B', 2 }" + EOL + "}",
            result);
    }

    [ConditionalFact]
    public void Literal_works_when_BigInteger()
        => Literal_works(
            new BigInteger(42),
            "BigInteger.Parse(\"42\", NumberFormatInfo.InvariantInfo)");

    [ConditionalFact]
    public void UnknownLiteral_throws_when_unknown()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => new CSharpHelper(TypeMappingSource).UnknownLiteral(new object()));
        Assert.Equal(DesignStrings.UnknownLiteral(typeof(object)), ex.Message);
    }

    [ConditionalTheory]
    [InlineData(typeof(int), "int")]
    [InlineData(typeof(int?), "int?")]
    [InlineData(typeof(int[]), "int[]")]
    [InlineData(typeof(int[,]), "int[,]")]
    [InlineData(typeof(int[][]), "int[][]")]
    [InlineData(typeof(Generic<int>), "Generic<int>")]
    [InlineData(typeof(Nested), "CSharpHelperTest.Nested")]
    [InlineData(typeof(Generic<Generic<int>>), "Generic<Generic<int>>")]
    [InlineData(typeof(MultiGeneric<int, int>), "MultiGeneric<int, int>")]
    [InlineData(typeof(NestedGeneric<int>), "CSharpHelperTest.NestedGeneric<int>")]
    [InlineData(typeof(Nested.DoubleNested), "CSharpHelperTest.Nested.DoubleNested")]
    [InlineData(typeof(NestedGeneric<Nested.DoubleNested>), "CSharpHelperTest.NestedGeneric<CSharpHelperTest.Nested.DoubleNested>")]
    public void Reference_works(Type type, string expected)
        => Assert.Equal(expected, new CSharpHelper(TypeMappingSource).Reference(type));

    private static class Nested
    {
        public class DoubleNested;
    }

    internal class NestedGeneric<T>;

    private enum SomeEnum
    {
        Default
    }

    [ConditionalTheory]
    [InlineData("dash-er", "dasher")]
    [InlineData("params", "@params")]
    [InlineData("true", "@true")]
    [InlineData("yield", "yield")]
    [InlineData("spac ed", "spaced")]
    [InlineData("1nders", "_1nders")]
    [InlineData("name.space", "@namespace")]
    [InlineData("$", "_")]
    public void Identifier_works(string input, string expected)
        => Assert.Equal(expected, new CSharpHelper(TypeMappingSource).Identifier(input));

    [ConditionalTheory]
    [InlineData(new[] { "WebApplication1", "Migration" }, "WebApplication1.Migration")]
    [InlineData(new[] { "WebApplication1.Migration" }, "WebApplication1.Migration")]
    [InlineData(new[] { "ef-xplat.namespace" }, "efxplat.@namespace")]
    [InlineData(new[] { "#", "$" }, "_._")]
    [InlineData(new[] { "" }, "_")]
    [InlineData(new string[] { }, "_")]
    [InlineData(new string?[] { null }, "_")]
    public void Namespace_works(string[] input, string excepted)
        => Assert.Equal(excepted, new CSharpHelper(TypeMappingSource).Namespace(input));

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, true, 42);

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(".TestFunc(true, 42)", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_with_arrays()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, new byte[] { 1, 2 }, new[] { 3, 4 }, new[] { "foo", "bar" });

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(".TestFunc(new byte[] { 1, 2 }, new[] { 3, 4 }, new[] { \"foo\", \"bar\" })", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_when_niladic()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo);

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(".TestFunc()", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_when_chaining()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo)
            .Chain(_testFuncMethodInfo);

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal($"{EOL}.TestFunc(){EOL}.TestFunc()", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_when_chaining_on_chain()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, "One")
            .Chain(new MethodCallCodeFragment(_testFuncMethodInfo, "Two"))
            .Chain(_testFuncMethodInfo, "Three");

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(@$"{EOL}.TestFunc(""One""){EOL}.TestFunc(""Two""){EOL}.TestFunc(""Three"")", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_when_chaining_on_chain_with_call()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, "One")
            .Chain(new MethodCallCodeFragment(_testFuncMethodInfo, "Two"))
            .Chain(
                new MethodCallCodeFragment(_testFuncMethodInfo, "Three").Chain(
                    new MethodCallCodeFragment(_testFuncMethodInfo, "Four")));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(@$"{EOL}.TestFunc(""One""){EOL}.TestFunc(""Two""){EOL}.TestFunc(""Three""){EOL}.TestFunc(""Four"")", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_when_nested_closure()
    {
        var method = new MethodCallCodeFragment(
            _testFuncMethodInfo,
            new NestedClosureCodeFragment("x", new MethodCallCodeFragment(_testFuncMethodInfo)));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(".TestFunc(x => x.TestFunc())", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_when_nested_closure_with_chain()
    {
        var method = new MethodCallCodeFragment(
            _testFuncMethodInfo,
            new NestedClosureCodeFragment(
                "x",
                new MethodCallCodeFragment(_testFuncMethodInfo, "One")
                    .Chain(new MethodCallCodeFragment(_testFuncMethodInfo, "Two"))));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(
            @".TestFunc(x => x
    .TestFunc(""One"")
    .TestFunc(""Two""))",
            result,
            ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_with_indent_works_when_nested_closure_with_chain()
    {
        var method = new MethodCallCodeFragment(
            _testFuncMethodInfo,
            new NestedClosureCodeFragment(
                "x",
                new MethodCallCodeFragment(_testFuncMethodInfo, "One")
                    .Chain(new MethodCallCodeFragment(_testFuncMethodInfo, "Two"))));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method, indent: 1);

        Assert.Equal(
            @".TestFunc(x => x
        .TestFunc(""One"")
        .TestFunc(""Two""))",
            result,
            ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_with_indent_works_when_chain_and_nested_closure_with_chain()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, "One")
            .Chain(
                new MethodCallCodeFragment(
                    _testFuncMethodInfo,
                    new NestedClosureCodeFragment(
                        "x",
                        new MethodCallCodeFragment(_testFuncMethodInfo, "Two")
                            .Chain(new MethodCallCodeFragment(_testFuncMethodInfo, "Three")))));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method, indent: 1);

        Assert.Equal(
            @"
    .TestFunc(""One"")
    .TestFunc(x => x
        .TestFunc(""Two"")
        .TestFunc(""Three""))",
            result,
            ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_when_nested_closure_with_two_calls()
    {
        var method = new MethodCallCodeFragment(
            _testFuncMethodInfo,
            new NestedClosureCodeFragment(
                "x",
                new[] { new MethodCallCodeFragment(_testFuncMethodInfo, "One"), new MethodCallCodeFragment(_testFuncMethodInfo, "Two") }));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method);

        Assert.Equal(
            @".TestFunc(x =>
{
    x.TestFunc(""One"");
    x.TestFunc(""Two"");
})",
            result,
            ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_with_indent_works_when_chain_and_nested_closure()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, "One")
            .Chain(
                new MethodCallCodeFragment(
                    _testFuncMethodInfo,
                    new NestedClosureCodeFragment(
                        "x",
                        new[]
                        {
                            new MethodCallCodeFragment(_testFuncMethodInfo, "Two"),
                            new MethodCallCodeFragment(_testFuncMethodInfo, "Three")
                        })));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method, indent: 1);

        Assert.Equal(
            @"
    .TestFunc(""One"")
    .TestFunc(x =>
    {
        x.TestFunc(""Two"");
        x.TestFunc(""Three"");
    })",
            result,
            ignoreLineEndingDifferences: true);
    }

#pragma warning disable CS0618
    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_with_identifier()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, true, 42);

        var result = new CSharpHelper(TypeMappingSource).Fragment(method, instanceIdentifier: "builder", typeQualified: false);

        Assert.Equal("builder.TestFunc(true, 42)", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_with_identifier_chained()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, "One").Chain(new MethodCallCodeFragment(_testFuncMethodInfo));

        var result = new CSharpHelper(TypeMappingSource).Fragment(method, instanceIdentifier: "builder", typeQualified: false);

        Assert.Equal($@"builder{EOL}    .TestFunc(""One""){EOL}    .TestFunc()", result);
    }

    [ConditionalFact]
    public void Fragment_MethodCallCodeFragment_works_with_type_qualified()
    {
        var method = new MethodCallCodeFragment(_testFuncMethodInfo, true, 42);

        var result = new CSharpHelper(TypeMappingSource).Fragment(method, instanceIdentifier: "builder", typeQualified: false);

        Assert.Equal("builder.TestFunc(true, 42)", result);
    }
#pragma warning restore CS0618

    [ConditionalFact]
    public void Really_unknown_literal_with_no_mapping_support()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(null);

        Assert.Equal(
            CoreStrings.LiteralGenerationNotSupported(nameof(SimpleTestType)),
            Assert.Throws<NotSupportedException>(
                () => new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType())).Message);
    }

    [ConditionalFact]
    public void Literal_with_parameterless_constructor()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.New(typeof(SimpleTestType)));

        Assert.Equal(
            "new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType()",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType()));
    }

    [ConditionalFact]
    public void Literal_with_one_parameter_constructor()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.New(
                typeof(SimpleTestType).GetConstructor([typeof(string)])!,
                Expression.Constant(v.Arg1, typeof(string))));

        Assert.Equal(
            "new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType(\"Jerry\")",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType("Jerry")));
    }

    [ConditionalFact]
    public void Literal_with_two_parameter_constructor()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.New(
                typeof(SimpleTestType).GetConstructor([typeof(string), typeof(int?)])!,
                Expression.Constant(v.Arg1, typeof(string)),
                Expression.Constant(v.Arg2, typeof(int?))));

        Assert.Equal(
            "new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType(\"Jerry\", 77)",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType("Jerry", 77)));
    }

    [ConditionalFact]
    public void Literal_with_parameterless_static_factory()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Call(
                typeof(SimpleTestTypeFactory).GetMethod(
                    nameof(SimpleTestTypeFactory.StaticCreate),
                    Type.EmptyTypes)!));

        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestTypeFactory.StaticCreate()",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType()));
    }

    [ConditionalFact]
    public void Literal_with_one_parameter_static_factory()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Call(
                typeof(SimpleTestTypeFactory).GetMethod(
                    nameof(SimpleTestTypeFactory.StaticCreate),
                    [typeof(string)])!,
                Expression.Constant(v.Arg1, typeof(string))));

        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestTypeFactory.StaticCreate(\"Jerry\")",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType("Jerry")));
    }

    [ConditionalFact]
    public void Literal_with_two_parameter_static_factory()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Call(
                typeof(SimpleTestTypeFactory).GetMethod(
                    nameof(SimpleTestTypeFactory.StaticCreate),
                    [typeof(string), typeof(int?)])!,
                Expression.Constant(v.Arg1, typeof(string)),
                Expression.Constant(v.Arg2, typeof(int?))));

        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestTypeFactory.StaticCreate(\"Jerry\", 77)",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType("Jerry", 77)));
    }

    [ConditionalFact]
    public void Literal_with_parameterless_instance_factory()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Call(
                Expression.New(typeof(SimpleTestTypeFactory)),
                typeof(SimpleTestTypeFactory).GetMethod(
                    nameof(SimpleTestTypeFactory.Create),
                    new Type[0])!));

        Assert.Equal(
            "new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestTypeFactory().Create()",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType()));
    }

    [ConditionalFact]
    public void Literal_with_one_parameter_instance_factory()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Convert(
                Expression.Call(
                    Expression.New(typeof(SimpleTestTypeFactory)),
                    typeof(SimpleTestTypeFactory).GetMethod(
                        nameof(SimpleTestTypeFactory.Create),
                        [typeof(string)])!,
                    Expression.Constant(v.Arg1, typeof(string))),
                typeof(SimpleTestType)));

        Assert.Equal(
            "(Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType)new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestTypeFactory().Create(\"Jerry\")",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType("Jerry", 77)));
    }

    [ConditionalFact]
    public void Literal_with_two_parameter_instance_factory()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Convert(
                Expression.Call(
                    Expression.New(
                        typeof(SimpleTestTypeFactory).GetConstructor([typeof(string)])!,
                        Expression.Constant("4096", typeof(string))),
                    typeof(SimpleTestTypeFactory).GetMethod(
                        nameof(SimpleTestTypeFactory.Create),
                        [typeof(string), typeof(int?)])!,
                    Expression.Constant(v.Arg1, typeof(string)),
                    Expression.Constant(v.Arg2, typeof(int?))),
                typeof(SimpleTestType)));

        Assert.Equal(
            "(Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType)new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestTypeFactory(\"4096\").Create(\"Jerry\", 77)",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType("Jerry", 77)));
    }

    [ConditionalFact]
    public void Literal_with_two_parameter_instance_factory_and_internal_cast()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Convert(
                Expression.Call(
                    Expression.New(
                        typeof(SimpleTestTypeFactory).GetConstructor([typeof(string)])!,
                        Expression.Constant("4096", typeof(string))),
                    typeof(SimpleTestTypeFactory).GetMethod(
                        nameof(SimpleTestTypeFactory.Create),
                        [typeof(string), typeof(int?)])!,
                    Expression.Constant(v.Arg1, typeof(string)),
                    Expression.Convert(
                        Expression.Constant(v.Arg2, typeof(int)),
                        typeof(int?))),
                typeof(SimpleTestType)));

        Assert.Equal(
            "(Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType)new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestTypeFactory(\"4096\").Create(\"Jerry\", (int?)77)",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType("Jerry", 77)));
    }

    [ConditionalFact]
    public void Literal_with_static_field()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Field(null, typeof(SimpleTestType).GetField(nameof(SimpleTestType.SomeStaticField))!));

        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType.SomeStaticField",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType()));
    }

    [ConditionalFact]
    public void Literal_with_static_property()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Property(null, typeof(SimpleTestType).GetProperty(nameof(SimpleTestType.SomeStaticProperty))!));

        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType.SomeStaticProperty",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType()));
    }

    [ConditionalFact]
    public void Literal_with_instance_property()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Property(
                Expression.New(typeof(SimpleTestType)),
                typeof(SimpleTestType).GetProperty(nameof(SimpleTestType.SomeInstanceProperty))!));

        Assert.Equal(
            "new Microsoft.EntityFrameworkCore.Design.Internal.SimpleTestType().SomeInstanceProperty",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType()));
    }

    [ConditionalFact]
    public void Literal_with_add()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Add(
                Expression.Constant(10),
                Expression.Constant(10)));

        Assert.Equal(
            "10 + 10",
            new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType()));
    }

    [ConditionalFact]
    public void Literal_with_unsupported_node_throws()
    {
        var typeMapping = CreateTypeMappingSource<SimpleTestType>(
            v => Expression.Multiply(
                Expression.Constant(10),
                Expression.Constant(10)));

        Assert.Equal(
            DesignStrings.LiteralExpressionNotSupported(
                "(10 * 10)",
                nameof(SimpleTestType)),
            Assert.Throws<NotSupportedException>(
                () => new CSharpHelper(typeMapping).UnknownLiteral(new SimpleTestType())).Message);
    }

    private IRelationalTypeMappingSource TypeMappingSource { get; } = CreateTypeMappingSource();

    private static SqlServerTypeMappingSource CreateTypeMappingSource<T>(Func<T, Expression>? literalExpressionFunc)
        => CreateTypeMappingSource(new TestTypeMappingPlugin<T>(literalExpressionFunc));

    private static SqlServerTypeMappingSource CreateTypeMappingSource(
        params IRelationalTypeMappingSourcePlugin[] plugins)
        => new(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            new RelationalTypeMappingSourceDependencies(plugins));

    private class TestTypeMappingPlugin<T>(Func<T, Expression>? literalExpressionFunc) : IRelationalTypeMappingSourcePlugin
    {
        private readonly Func<T, Expression>? _literalExpressionFunc = literalExpressionFunc;

        public RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => _literalExpressionFunc == null
                ? new SimpleTestNonImplementedTypeMapping()
                : new SimpleTestTypeMapping<T>(_literalExpressionFunc);
    }

    private class SimpleTestTypeMapping<T>(
        Func<T, Expression> literalExpressionFunc) : RelationalTypeMapping("storeType", typeof(SimpleTestType))
    {
        private readonly Func<T, Expression> _literalExpressionFunc = literalExpressionFunc;

        public override Expression GenerateCodeLiteral(object value)
            => _literalExpressionFunc((T)value);

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => throw new NotSupportedException();
    }

    private class SimpleTestNonImplementedTypeMapping : RelationalTypeMapping
    {
        public SimpleTestNonImplementedTypeMapping()
            : base("storeType", typeof(SimpleTestType))
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => throw new NotSupportedException();
    }

    private static readonly MethodInfo _testFuncMethodInfo
        = typeof(CSharpHelperTest).GetRuntimeMethod(
            nameof(TestFunc),
            [typeof(object), typeof(object), typeof(object), typeof(object)])!;

    public static void TestFunc(object builder, object o1, object o2, object o3)
        => throw new NotSupportedException();
}

internal class SimpleTestType
{
    public static readonly int SomeStaticField = 8;
    public readonly int SomeField = 8;
    public static int SomeStaticProperty { get; } = 8;
    public int SomeInstanceProperty { get; } = 8;

    public SimpleTestType()
    {
    }

    public SimpleTestType(string arg1)
        : this(arg1, null)
    {
    }

    public SimpleTestType(string arg1, int? arg2)
    {
        Arg1 = arg1;
        Arg2 = arg2;
    }

    public string Arg1 { get; } = null!;
    public int? Arg2 { get; }
}

internal class SimpleTestTypeFactory
{
    public SimpleTestTypeFactory()
    {
    }

    public SimpleTestTypeFactory(string factoryArg)
    {
        FactoryArg = factoryArg;
    }

    public string FactoryArg { get; } = null!;

    public SimpleTestType Create()
        => new();

    public object Create(string arg1)
        => new SimpleTestType(arg1);

    public object Create(string arg1, int? arg2)
        => new SimpleTestType(arg1, arg2);

    public static SimpleTestType StaticCreate()
        => new();

    public static object StaticCreate(string arg1)
        => new SimpleTestType(arg1);

    public static object StaticCreate(string arg1, int? arg2)
        => new SimpleTestType(arg1, arg2);
}

internal class Generic<T>;

internal class MultiGeneric<T1, T2>;
