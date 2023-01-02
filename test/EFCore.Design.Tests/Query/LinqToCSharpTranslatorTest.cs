// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Xunit.Sdk;
using static System.Linq.Expressions.Expression;
using Assert = Xunit.Assert;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query;

public class LinqToCSharpTranslatorTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public LinqToCSharpTranslatorTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _outputExpressionTrees = true;
    }

    [Theory]
    [InlineData("hello", @"""hello""")]
    [InlineData(1, "1")]
    [InlineData(1L, "1L")]
    [InlineData((short)1, "1")]
    [InlineData((sbyte)1, "1")]
    [InlineData(1U, "1U")]
    [InlineData(1UL, "1UL")]
    [InlineData((ushort)1, "1")]
    [InlineData((byte)1, "1")]
    [InlineData(1.5, "1.5D")]
    [InlineData(1.5F, "1.5F")]
    [InlineData(true, "true")]
    [InlineData(typeof(string), "typeof(string)")]
    public void Constant_values(object constantValue, string literalRepresentation)
        => AssertExpression(
            Constant(constantValue),
            literalRepresentation);

    [Fact]
    public void Constant_decimal()
        => AssertExpression(Constant(1.5m), "1.5M");

    [Fact]
    public void Constant_null()
        => AssertExpression(Constant(null, typeof(string)), "null");

    [Fact]
    public void Constant_throws_on_unsupported_type()
        => Assert.Throws<NotSupportedException>(() => AssertExpression(Constant(default(DateTime)), ""));

    [Fact]
    public void Enum()
        => AssertExpression(Constant(SomeEnum.One), "SomeEnum.One");

    [Fact]
    public void Enum_with_multiple_values()
        => AssertExpression(Constant(SomeEnum.One | SomeEnum.Two), "SomeEnum.One | SomeEnum.Two");

    [Fact]
    public void Enum_with_unknown_value()
        => AssertExpression(Constant((SomeEnum)1000), "(SomeEnum)1000L");

    [Theory]
    [InlineData(ExpressionType.Add, "+")]
    [InlineData(ExpressionType.Subtract, "-")]
    // TODO: Complete
    public void Binary_numeric(ExpressionType expressionType, string op)
        => AssertExpression(
            MakeBinary(expressionType, Constant(2), Constant(3)),
            $"2 {op} 3");

    [Fact]
    public void Binary_ArrayIndex()
        => AssertExpression(
            ArrayIndex(Parameter(typeof(int[]), "i"), Constant(2)),
            "i[2]");

    [Fact]
    public void Binary_Power()
        => AssertExpression(
            Power(Constant(2.0), Constant(3.0)),
            "Math.Pow(2D, 3D)");

    [Fact]
    public void Binary_PowerAssign()
        => AssertExpression(
            PowerAssign(Parameter(typeof(double), "d"), Constant(3.0)),
            "d = Math.Pow(d, 3D)");

    [Theory]
    [InlineData(ExpressionType.Negate, "-i")]
    [InlineData(ExpressionType.NegateChecked, "-i")]
    [InlineData(ExpressionType.Not, "~i")]
    [InlineData(ExpressionType.OnesComplement, "~i")]
    [InlineData(ExpressionType.UnaryPlus, "+i")]
    [InlineData(ExpressionType.Increment, "i + 1")]
    [InlineData(ExpressionType.Decrement, "i - 1")]
    public void Unary_expression_int(ExpressionType expressionType, string expected)
        => AssertExpression(
            MakeUnary(expressionType, Parameter(typeof(int), "i"), typeof(int)),
            expected);

    [Theory]
    [InlineData(ExpressionType.Not, "!b")]
    [InlineData(ExpressionType.IsFalse, "!b")]
    [InlineData(ExpressionType.IsTrue, "b")]
    public void Unary_expression_bool(ExpressionType expressionType, string expected)
        => AssertExpression(
            MakeUnary(expressionType, Parameter(typeof(bool), "b"), typeof(bool)),
            expected);

    [Theory]
    [InlineData(ExpressionType.PostIncrementAssign, "i++")]
    [InlineData(ExpressionType.PostDecrementAssign, "i--")]
    [InlineData(ExpressionType.PreIncrementAssign, "++i")]
    [InlineData(ExpressionType.PreDecrementAssign, "--i")]
    public void Unary_statement(ExpressionType expressionType, string expected)
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
            MakeUnary(expressionType, i, typeof(int))),
$$"""
{
    int i;
    {{expected}};
}
""");
    }

    [Fact]
    public void Unary_ArrayLength()
        => AssertExpression(
            ArrayLength(Parameter(typeof(int[]), "i")),
            "i.Length");

    [Fact]
    public void Unary_Convert()
        => AssertExpression(
            Convert(
                Parameter(typeof(object), "i"),
                typeof(string)),
            "(string)i");

    [Fact]
    public void Unary_Throw()
        => AssertStatement(
            Throw(New(typeof(Exception))),
            "throw new Exception();");

    [Fact]
    public void Unary_Unbox()
        => AssertExpression(
            Unbox(Parameter(typeof(object), "i"), typeof(int)),
            "i");

    [Fact]
    public void Unary_Quote()
    {
        AssertExpression(
            Quote((Expression<Func<string, int>>)(s => s.Length)),
            "s => s.Length");
    }

    [Fact]
    public void Unary_TypeAs()
        => AssertExpression(
            TypeAs(Parameter(typeof(object), "i"), typeof(string)),
            "i as string");

    [Fact]
    public void Instance_property()
        => AssertExpression(
            Property(
                Constant("hello"),
                typeof(string).GetProperty(nameof(string.Length))!),
            @"""hello"".Length");

    [Fact]
    public void Static_property()
        => AssertExpression(
            Property(
                null,
                typeof(DateTime).GetProperty(nameof(DateTime.Now))!),
            "DateTime.Now");

    [Fact]
    public void Private_instance_field_read()
        => AssertExpression(
            Field(Parameter(typeof(Blog), "blog"), "_privateField"),
            @"typeof(Blog).GetField(""_privateField"", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(blog)");

    [Fact]
    public void Private_instance_field_write()
        => AssertStatement(
            Assign(
                Field(Parameter(typeof(Blog), "blog"), "_privateField"),
                Constant(8)),
            @"typeof(Blog).GetField(""_privateField"", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(blog, 8)");

    [Fact]
    public void Internal_instance_field_read()
        => AssertExpression(
            Field(Parameter(typeof(Blog), "blog"), "_internalField"),
            @"typeof(Blog).GetField(""_internalField"", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(blog)");

    [Fact]
    public void Not()
        => AssertExpression(
            Expression.Not(Constant(true)),
            "!true");

    [Fact]
    public void Method_call_instance()
    {
        var blog = Parameter(typeof(Blog), "blog");

        AssertStatement(
            Block(
                variables: new[] { blog },
                Assign(blog, New(Blog.Constructor)),
                Call(
                    blog,
                    typeof(Blog).GetMethod(nameof(Blog.SomeInstanceMethod))!)),
"""
{
    var blog = new Blog();
    blog.SomeInstanceMethod();
}
""");
    }

    [Fact]
    public void Method_call_static()
        => AssertExpression(
            Call(ReturnsIntWithParamMethod, Constant(8)),
            "LinqToCSharpTranslatorTest.ReturnsIntWithParam(8)");

    [Fact]
    public void Method_call_static_on_nested_type()
        => AssertExpression(
            Call(typeof(Blog).GetMethod(nameof(Blog.Static_method_on_nested_type))!),
            "LinqToCSharpTranslatorTest.Blog.Static_method_on_nested_type()");

    [Fact]
    public void Method_call_extension()
    {
        var blog = Parameter(typeof(LinqExpressionToRoslynTranslatorExtensionType), "someType");

        AssertStatement(
            Block(
                variables: new[] { blog },
                Assign(blog, New(LinqExpressionToRoslynTranslatorExtensionType.Constructor)),
                Call(LinqExpressionToRoslynTranslatorExtensions.SomeExtensionMethod, blog)),
"""
{
    var someType = new LinqExpressionToRoslynTranslatorExtensionType();
    someType.SomeExtension();
}
""");
    }

    [Fact]
    public void Method_call_extension_with_null_this()
        => AssertExpression(
            Call(
                LinqExpressionToRoslynTranslatorExtensions.SomeExtensionMethod,
                Constant(null, typeof(LinqExpressionToRoslynTranslatorExtensionType))),
            "LinqExpressionToRoslynTranslatorExtensions.SomeExtension(null)");

    [Fact]
    public void Method_call_generic()
    {
        var blog = Parameter(typeof(Blog), "blog");

        AssertStatement(
            Block(
                variables: new[] { blog },
                Assign(blog, New(Blog.Constructor)),
                Call(
                    GenericMethod.MakeGenericMethod(typeof(Blog)),
                    blog)),
"""
{
    var blog = new Blog();
    LinqToCSharpTranslatorTest.GenericMethodImplementation<Blog>(blog);
}
""");
    }

    [Fact]
    public void Method_call_namespace_is_collected()
    {
        var (translator, _) = CreateTranslator();
        var namespaces = new HashSet<string>();
        _ = translator.TranslateExpression(Call(FooMethod), namespaces);
        Assert.Collection(namespaces,
            ns => Assert.Equal(typeof(LinqToCSharpTranslatorTest).Namespace, ns));
    }

    [Fact]
    public void Method_call_with_in_out_ref_parameters()
    {
        var inParam = Parameter(typeof(int), "inParam");
        var outParam = Parameter(typeof(int), "outParam");
        var refParam = Parameter(typeof(int), "refParam");

        AssertStatement(
            Block(
                variables: new[] { inParam, outParam, refParam },
                Call(WithInOutRefParameterMethod, new[] { inParam, outParam, refParam })),
"""
{
    int inParam;
    int outParam;
    int refParam;
    LinqToCSharpTranslatorTest.WithInOutRefParameter(in inParam, out outParam, ref refParam);
}
""");
    }

    [Fact]
    public void Instantiation()
        => AssertExpression(
            New(
                typeof(Blog).GetConstructor(new[] { typeof(string) })!,
                Constant("foo")),
            @"new Blog(""foo"")");

    [Fact]
    public void Instantiation_with_required_properties_and_parameterless_constructor()
        => AssertExpression(
            New(
                typeof(BlogWithRequiredProperties).GetConstructor(Array.Empty<Type>())!),
"""
Activator.CreateInstance<BlogWithRequiredProperties>()
""");

    [Fact]
    public void Instantiation_with_required_properties_and_non_parameterless_constructor()
        => Assert.Throws<NotImplementedException>(() => AssertExpression(
            New(
                typeof(BlogWithRequiredProperties).GetConstructor(new[] { typeof(string) })!,
                Constant("foo")), ""));

    [Fact]
    public void Instantiation_with_required_properties_with_SetsRequiredMembers()
        => AssertExpression(
            New(
                typeof(BlogWithRequiredProperties).GetConstructor(new[] { typeof(string), typeof(int) })!,
                Constant("foo"), Constant(8)),
            @"new BlogWithRequiredProperties(""foo"", 8)");

    [Fact]
    public void Lambda_with_expression_body()
        => AssertExpression(
            Lambda<Func<bool>>(Constant(true)),
            "() => true");

    [Fact]
    public void Lambda_with_block_body()
    {
        var i = Parameter(typeof(int), "i");
        var block = Block(
            variables: new[] { i },
            Assign(i, Constant(8)),
            i);

        AssertExpression(
            Lambda<Func<int>>(block),
"""
() =>
{
    var i = 8;
    return i;
}
""");
    }

    [Fact]
    public void Lambda_with_no_parameters()
        => AssertExpression(
            Lambda<Func<bool>>(Constant(true)),
            "() => true");

    [Fact]
    public void Lambda_with_one_parameter()
    {
        var i = Parameter(typeof(int), "i");

        AssertExpression(
            Lambda<Func<int, bool>>(Constant(true), i),
            "i => true");
    }

    [Fact]
    public void Lambda_with_two_parameters()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertExpression(
            Lambda<Func<int, int, int>>(Add(i, j), i, j),
            "(i, j) => i + j");
    }

    [Fact]
    public void Invocation_with_literal_argument()
        => AssertExpression(
            AndAlso(
                Constant(true),
                Invoke((Expression<Func<int, bool>>)(f => f > 5), Constant(8))),
            "true && 8 > 5");

    [Fact]
    public void Invocation_with_argument_that_has_side_effects()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(
                    i,
                    Add(
                        Constant(5),
                        Invoke((Expression<Func<int, int>>)(f => f + f), Call(FooMethod))))),
"""
{
    var f = LinqToCSharpTranslatorTest.Foo();
    var i = 5 + f + f;
}
""");
    }

    [Fact]
    public void Condition_expression()
        => AssertExpression(
            Condition(Constant(true), Constant(1), Constant(2)),
            "true ? 1 : 2");

    [Fact]
    public void Conditional_without_false_value_fails()
        => Assert.Throws<NotSupportedException>(
            () => AssertExpression(
                IfThen(Constant(true), Constant(8)),
                "true ? 1 : 2"));

    [Fact]
    public void Condition_statement()
        => AssertStatement(
            Block(
                Condition(Constant(true), Call(FooMethod), Call(BarMethod)),
                Constant(8)),
"""
{
    if (true)
    {
        LinqToCSharpTranslatorTest.Foo();
    }
    else
    {
        LinqToCSharpTranslatorTest.Bar();
    }
}
""");

    [Fact]
    public void IfThen_statement()
    {
        var parameter = Parameter(typeof(int), "i");
        var block = Block(
            variables: new[] { parameter },
            expressions: Assign(parameter, Constant(8)));

        AssertStatement(
            Block(IfThen(Constant(true), block)),
"""
{
    if (true)
    {
        var i = 8;
    }
}
""");
    }

    [Fact]
    public void IfThenElse_statement()
    {
        var parameter1 = Parameter(typeof(int), "i");
        var block1 = Block(
            variables: new[] { parameter1 },
            expressions: Assign(parameter1, Constant(8)));

        var parameter2 = Parameter(typeof(int), "j");
        var block2 = Block(
            variables: new[] { parameter2 },
            expressions: Assign(parameter2, Constant(9)));

        AssertStatement(
            Block(IfThenElse(Constant(true), block1, block2)),
"""
{
    if (true)
    {
        var i = 8;
    }
    else
    {
        var j = 9;
    }
}
""");
    }

    [Fact]
    public void IfThenElse_nested()
    {
        var variable = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { variable },
                expressions: IfThenElse(
                    Constant(true),
                    Block(Assign(variable, Constant(1))),
                    IfThenElse(
                        Constant(false),
                        Block(Assign(variable, Constant(2))),
                        Block(Assign(variable, Constant(3)))))),
"""
{
    int i;
    if (true)
    {
        i = 1;
    }
    else if (false)
    {
        i = 2;
    }
    else
    {
        i = 3;
    }
}
""");
    }

    [Fact]
    public void Switch_expression()
        => AssertExpression(
            Switch(
                Constant(8),
                Constant(0),
                SwitchCase(Constant(-9), Constant(9)),
                SwitchCase(Constant(-10), Constant(10))),
"""
8 switch
{
    9 => -9,
    10 => -10,
    _ => 0
}
""");

    [Fact]
    public void Switch_expression_nested()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");
        var k = Parameter(typeof(int), "k");

        AssertStatement(
            Block(
                variables: new[] { i, j, k },
                Assign(j, Constant(8)),
                Assign(
                    i,
                    Switch(
                        j,
                        defaultBody: Constant(0),
                        SwitchCase(Constant(1), Constant(100)),
                        SwitchCase(
                            Switch(
                                k,
                                defaultBody: Constant(0),
                                SwitchCase(Constant(2), Constant(200)),
                                SwitchCase(Constant(3), Constant(300))),
                            Constant(200))))),
"""
{
    int k;
    var j = 8;
    var i = j switch
    {
        100 => 1,
        200 => k switch
        {
            200 => 2,
            300 => 3,
            _ => 0
        },
        _ => 0
    };
}
""");
    }

    [Fact]
    public void Switch_expression_non_constant_arm()
        => AssertExpression(
            Switch(
                Parameter(typeof(Blog), "blog1"),
                Constant(0),
                SwitchCase(Constant(2), Parameter(typeof(Blog), "blog2")),
                SwitchCase(Constant(3), Parameter(typeof(Blog), "blog3"))),
            "blog1 == blog2 ? 2 : blog1 == blog3 ? 3 : 0");

    [Fact]
    public void Switch_statement_with_non_constant_label()
    {
        AssertStatement(
            Switch(
                Parameter(typeof(Blog), "blog1"),
                Constant(0),
                SwitchCase(Constant(2), Parameter(typeof(Blog), "blog2")),
                SwitchCase(Constant(3), Parameter(typeof(Blog), "blog3"))),
"""
if (blog1 == blog2)
{
    2;
}
else if (blog1 == blog3)
{
    3;
}
else
{
    0;
}
""");
    }

    [Fact]
    public void Switch_statement_without_default()
    {
        var parameter = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { parameter },
                expressions: Switch(
                    Constant(7),
                    SwitchCase(Block(typeof(void), Assign(parameter, Constant(9))), Constant(-9)),
                    SwitchCase(Block(typeof(void), Assign(parameter, Constant(10))), Constant(-10)))),
"""
{
    int i;
    switch (7)
    {
        case -9:
        {
            i = 9;
            break;
        }

        case -10:
        {
            i = 10;
            break;
        }
    }
}
""");
    }

    [Fact]
    public void Switch_statement_with_default()
    {
        var parameter = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { parameter },
                expressions: Switch(
                    Constant(7),
                    Assign(parameter, Constant(0)),
                    SwitchCase(Assign(parameter, Constant(9)), Constant(-9)),
                    SwitchCase(Assign(parameter, Constant(10)), Constant(-10)))),
"""
{
    int i;
    switch (7)
    {
        case -9:
            i = 9;
            break;
        case -10:
            i = 10;
            break;
        default:
            i = 0;
            break;
    }
}
""");
    }

    [Fact]
    public void Switch_statement_with_multiple_labels()
    {
        var parameter = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { parameter },
                expressions: Switch(
                    Constant(7),
                    Assign(parameter, Constant(0)),
                    SwitchCase(Assign(parameter, Constant(9)), Constant(-9), Constant(-8)),
                    SwitchCase(Assign(parameter, Constant(10)), Constant(-10)))),
"""
{
    int i;
    switch (7)
    {
        case -9:
        case -8:
            i = 9;
            break;
        case -10:
            i = 10;
            break;
        default:
            i = 0;
            break;
    }
}
""");
    }

    [Fact]
    public void Variable_assignment_uses_var()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(i, Constant(8))),
"""
{
    var i = 8;
}
""");
    }

    [Fact]
    public void Variable_assignment_to_null_does_not_use_var()
    {
        var s = Parameter(typeof(string), "s");

        AssertStatement(
            Block(
                variables: new[] { s },
                Assign(s, Constant(null, typeof(string)))),
"""
{
    string s = null;
}
""");
    }

    [Fact]
    public void Variables_with_same_name_in_sibling_blocks_do_not_get_renamed()
    {
        var i1 = Parameter(typeof(int), "i");
        var i2 = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                Block(
                    variables: new[] { i1 },
                    Assign(i1, Constant(8)),
                    Call(ReturnsIntWithParamMethod, i1)),
                Block(
                    variables: new[] { i2 },
                    Assign(i2, Constant(8)),
                    Call(ReturnsIntWithParamMethod, i2))),
"""
{
    {
        var i = 8;
        LinqToCSharpTranslatorTest.ReturnsIntWithParam(i);
    }

    {
        var i = 8;
        LinqToCSharpTranslatorTest.ReturnsIntWithParam(i);
    }
}
""");
    }

    [Fact]
    public void Variable_with_same_name_in_child_block_gets_renamed()
    {
        var i1 = Parameter(typeof(int), "i");
        var i2 = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i1 },
                Assign(i1, Constant(8)),
                Call(ReturnsIntWithParamMethod, i1),
                Block(
                    variables: new[] { i2 },
                    Assign(i2, Constant(8)),
                    Call(ReturnsIntWithParamMethod, i2),
                    Call(ReturnsIntWithParamMethod, i1))),
"""
{
    var i = 8;
    LinqToCSharpTranslatorTest.ReturnsIntWithParam(i);
    {
        var i0 = 8;
        LinqToCSharpTranslatorTest.ReturnsIntWithParam(i0);
        LinqToCSharpTranslatorTest.ReturnsIntWithParam(i);
    }
}
""");
    }

    [Fact]
    public void Variable_with_same_name_in_lambda_does_not_get_renamed()
    {
        var i1 = Parameter(typeof(int), "i");
        var i2 = Parameter(typeof(int), "i");
        var f = Parameter(typeof(Func<int, bool>), "f");

        AssertStatement(
            Block(
                variables: new[] { i1 },
                Assign(i1, Constant(8)),
                Assign(
                    f, Lambda<Func<int, bool>>(
                        Equal(i2, Constant(5)),
                        i2))),
"""
{
    var i = 8;
    f = i => i == 5;
}
""");
    }

    [Fact]
    public void Same_parameter_instance_is_used_twice_in_nested_lambdas()
    {
        var f1 = Parameter(typeof(Func<int, bool>), "f1");
        var f2 = Parameter(typeof(Func<int, bool>), "f2");
        var i = Parameter(typeof(int), "i");

        AssertExpression(
            Assign(
                f1,
                Lambda<Func<int, bool>>(
                    Block(
                        Assign(
                            f2,
                            Lambda<Func<int, bool>>(
                                Equal(i, Constant(5)),
                                i)),
                        Constant(true)),
                    i)),
"""
f1 = i =>
{
    f2 = i => i == 5;
    return true;
}
""");
    }

    [Fact]
    public void Block_with_non_standalone_expression_as_statement()
        => AssertStatement(
            Block(Add(Constant(1), Constant(2))),
"""
{
    _ = 1 + 2;
}
""");

    [Fact]
    public void Statement_condition_block_inside_expression_block_with_lifted_statements()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(
                    i, Block(
                        // We're in expression context. Do anything that will get lifted.
                        Call(FooMethod),
                        // Statement condition
                        IfThen(
                            Constant(true),
                            Block(
                                Call(BarMethod),
                                Call(BazMethod))),
                        // Last expression (to make the block above evaluate as statement
                        Constant(8)))),
"""
{
    LinqToCSharpTranslatorTest.Foo();
    if (true)
    {
        LinqToCSharpTranslatorTest.Bar();
        LinqToCSharpTranslatorTest.Baz();
    }

    var i = 8;
}
""");
    }

    [Fact]
    public void Lift_block_in_assignment_context()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(i, Block(
                    variables: new[] { j },
                    Assign(j, Call(FooMethod)),
                    Call(ReturnsIntWithParamMethod, j)))),
"""
{
    var j = LinqToCSharpTranslatorTest.Foo();
    var i = LinqToCSharpTranslatorTest.ReturnsIntWithParam(j);
}
""");
    }

    [Fact]
    public void Lift_block_in_method_call_context()
    {
        AssertStatement(
            Block(
                Call(
                    ReturnsIntWithParamMethod,
                    Block(
                        Call(FooMethod),
                        Call(BarMethod)))),
"""
{
    LinqToCSharpTranslatorTest.Foo();
    LinqToCSharpTranslatorTest.ReturnsIntWithParam(LinqToCSharpTranslatorTest.Bar());
}
""");
    }

    [Fact]
    public void Lift_nested_block()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertStatement(
            Block(variables: new[] { i },
            Assign(
                i,
                Block(
                    variables: new[] { j },
                    Assign(j, Call(FooMethod)),
                    Block(
                        Call(BarMethod),
                        Call(ReturnsIntWithParamMethod, j))))),
"""
{
    var j = LinqToCSharpTranslatorTest.Foo();
    LinqToCSharpTranslatorTest.Bar();
    var i = LinqToCSharpTranslatorTest.ReturnsIntWithParam(j);
}
""");
    }

    [Fact]
    public void Binary_lifts_left_side_if_right_is_lifted()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(i,
                    Add(
                        Call(FooMethod),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))),
"""
{
    var lifted = LinqToCSharpTranslatorTest.Foo();
    LinqToCSharpTranslatorTest.Bar();
    var i = lifted + LinqToCSharpTranslatorTest.Baz();
}
""");
    }

    [Fact]
    public void Binary_does_not_lift_left_side_if_it_has_no_side_effects()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(i,
                    Add(
                        Constant(5),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))),
"""
{
    LinqToCSharpTranslatorTest.Bar();
    var i = 5 + LinqToCSharpTranslatorTest.Baz();
}
""");
    }

    [Fact]
    public void Method_lifts_earlier_args_if_later_arg_is_lifted()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(i,
                    Call(
                        typeof(LinqToCSharpTranslatorTest).GetMethod(nameof(MethodWithSixParams))!,
                        Call(FooMethod),
                        Constant(5),
                        Block(Call(BarMethod), Call(BazMethod)),
                        Call(FooMethod),
                        Block(Call(BazMethod), Call(BarMethod)),
                        Call(FooMethod)))),
"""
{
    var liftedArg = LinqToCSharpTranslatorTest.Foo();
    LinqToCSharpTranslatorTest.Bar();
    var liftedArg0 = LinqToCSharpTranslatorTest.Baz();
    var liftedArg1 = LinqToCSharpTranslatorTest.Foo();
    LinqToCSharpTranslatorTest.Baz();
    var i = LinqToCSharpTranslatorTest.MethodWithSixParams(liftedArg, 5, liftedArg0, liftedArg1, LinqToCSharpTranslatorTest.Bar(), LinqToCSharpTranslatorTest.Foo());
}
""");
    }

    [Fact]
    public void New_lifts_earlier_args_if_later_arg_is_lifted()
    {
        var b = Parameter(typeof(Blog), "b");

        AssertStatement(
            Block(
                variables: new[] { b },
                Assign(b,
                    New(
                        typeof(Blog).GetConstructor(new[] { typeof(int), typeof(int) })!,
                        Call(FooMethod),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))),
"""
{
    var liftedArg = LinqToCSharpTranslatorTest.Foo();
    LinqToCSharpTranslatorTest.Bar();
    var b = new Blog(liftedArg, LinqToCSharpTranslatorTest.Baz());
}
""");
    }

    [Fact]
    public void Index_lifts_earlier_args_if_later_arg_is_lifted()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void ListInit_lifts_earlier_args_if_later_arg_is_lifted()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void New_array()
        => AssertExpression(
            NewArrayInit(typeof(int)),
            "new int[]{}");

    [Fact]
    public void New_array_with_bounds()
        => AssertExpression(
            NewArrayBounds(typeof(int), Constant(3)),
            "new int[3]");

    [Fact]
    public void New_array_with_initializers()
        => AssertExpression(
            NewArrayInit(typeof(int), Constant(3), Constant(4)),
            "new int[]{3, 4}");

    [Fact]
    public void New_array_lifts_earlier_args_if_later_arg_is_lifted()
    {
        var a = Parameter(typeof(int[]), "a");

        // a = new[] { Foo(), { Bar(); Baz(); } }
        AssertStatement(
            Block(
                variables: new[] { a },
                Assign(a,
                    NewArrayInit(
                        typeof(int),
                        Call(FooMethod),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))),
"""
{
    var liftedArg = LinqToCSharpTranslatorTest.Foo();
    LinqToCSharpTranslatorTest.Bar();
    var a = new int[]{liftedArg, LinqToCSharpTranslatorTest.Baz()};
}
""");
    }

    [Fact]
    public void Lift_variable_in_expression_block()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(i, Block(
                    variables: new[] { j },
                    Block(
                        Call(FooMethod),
                        Assign(j, Constant(8)),
                        Constant(9))))),
"""
{
    int j;
    LinqToCSharpTranslatorTest.Foo();
    j = 8;
    var i = 9;
}
""");
    }

    [Fact]
    public void Lift_block_in_lambda_body_expression()
    {
        AssertExpression(
            Lambda<Func<int>>(
                Call(
                    ReturnsIntWithParamMethod,
                    Block(
                        Call(FooMethod),
                        Call(BarMethod))),
                Array.Empty<ParameterExpression>()),
"""
() =>
{
    LinqToCSharpTranslatorTest.Foo();
    return LinqToCSharpTranslatorTest.ReturnsIntWithParam(LinqToCSharpTranslatorTest.Bar());
}
""");
    }

    [Fact]
    public void Do_not_lift_block_in_lambda_body()
    {
        AssertExpression(
            Lambda<Func<int>>(
                Block(Block(Constant(8))),
                Array.Empty<ParameterExpression>()),
"""
() =>
{
    {
        return 8;
    }
}
""");
    }

    [Fact]
    public void Simplify_block_with_single_expression()
        => AssertExpression(
            Assign(Parameter(typeof(int), "i"), Block(Constant(8))),
            "i = 8");

    [Fact]
    public void Cannot_lift_out_of_expression_context()
        => Assert.Throws<NotSupportedException>(
            () => AssertExpression(
                Assign(
                    Parameter(typeof(int), "i"),
                    Block(
                        Call(FooMethod),
                        Constant(8))),
                ""));

    [Fact]
    public void Lift_switch_expression()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");
        var k = Parameter(typeof(int), "k");

        AssertStatement(
            Block(
                variables: new[] { i, j },
                Assign(j, Constant(8)),
                Assign(
                    i,
                    Switch(
                        j,
                        defaultBody: Block(Constant(0)),
                        SwitchCase(
                            Block(
                                Block(
                                    Assign(k, Call(FooMethod)),
                                    Call(ReturnsIntWithParamMethod, k))),
                            Constant(8)),
                        SwitchCase(Constant(2), Constant(9))))),
"""
{
    int i;
    var j = 8;
    switch (j)
    {
        case 8:
        {
            k = LinqToCSharpTranslatorTest.Foo();
            i = LinqToCSharpTranslatorTest.ReturnsIntWithParam(k);
            break;
        }

        case 9:
            i = 2;
            break;
        default:
            i = 0;
            break;
    }
}
""");
    }

    [Fact]
    public void Lift_nested_switch_expression()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");
        var k = Parameter(typeof(int), "k");
        var l = Parameter(typeof(int), "l");

        AssertStatement(
            Block(
                variables: new[] { i, j, k },
                Assign(j, Constant(8)),
                Assign(
                    i,
                    Switch(
                        j,
                        defaultBody: Constant(0),
                        SwitchCase(Constant(1), Constant(100)),
                        SwitchCase(
                            Switch(
                                k,
                                defaultBody: Constant(0),
                                SwitchCase(
                                    Block(
                                        variables: new[] { l },
                                        Assign(l, Call(FooMethod)),
                                        Call(ReturnsIntWithParamMethod, l)),
                                    Constant(200)),
                                SwitchCase(Constant(3), Constant(300))),
                            Constant(200))))),
"""
{
    int i;
    int k;
    var j = 8;
    switch (j)
    {
        case 100:
            i = 1;
            break;
        case 200:
        {
            switch (k)
            {
                case 200:
                {
                    var l = LinqToCSharpTranslatorTest.Foo();
                    i = LinqToCSharpTranslatorTest.ReturnsIntWithParam(l);
                    break;
                }

                case 300:
                    i = 3;
                    break;
                default:
                    i = 0;
                    break;
            }

            break;
        }

        default:
            i = 0;
            break;
    }
}
""");
    }

    [Fact]
    public void Lift_non_literal_switch_expression()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(
                    i,
                    Switch(
                        Parameter(typeof(Blog), "blog1"),
                        defaultBody: Block(Constant(0)),
                        SwitchCase(
                            Block(
                                Call(ReturnsIntWithParamMethod, Constant(8)),
                                Constant(1)),
                            Parameter(typeof(Blog), "blog2")),
                        SwitchCase(
                            Block(
                                Call(ReturnsIntWithParamMethod, Constant(9)),
                                Constant(2)),
                            Parameter(typeof(Blog), "blog3")),
                        SwitchCase(Constant(3), Parameter(typeof(Blog), "blog4"))))),
"""
{
    int i;
    if (blog1 == blog2)
    {
        LinqToCSharpTranslatorTest.ReturnsIntWithParam(8);
        i = 1;
    }
    else
    {
        if (blog1 == blog3)
        {
            LinqToCSharpTranslatorTest.ReturnsIntWithParam(9);
            i = 2;
        }
        else
        {
            i = blog1 == blog4 ? 3 : 0;
        }
    }
}
""");
    }

    [Fact]
    public void TypeEqual_node()
        => AssertExpression(
            TypeEqual(Parameter(typeof(object), "p"), typeof(int)),
            "p == typeof(int)");

    [Fact]
    public void TypeIs_node()
        => AssertExpression(
            TypeIs(Parameter(typeof(object), "p"), typeof(int)),
            "p is int");

    [Fact]
    public void Goto_with_named_label()
    {
        var labelTarget = Label("label1");

        AssertStatement(
            Block(
                Goto(labelTarget),
                Label(labelTarget),
                Call(FooMethod)),
"""
{
    goto label1;
    label1:
        LinqToCSharpTranslatorTest.Foo();
}
""");
    }

    [Fact]
    public void Goto_with_label_on_last_line()
    {
        var labelTarget = Label("label1");

        AssertStatement(
            Block(
                Goto(labelTarget),
                Label(labelTarget)),
"""
{
    goto label1;
    label1:
        ;
}
""");
    }

    [Fact]
    public void Goto_outside_label()
    {
        var labelTarget = Label();

        AssertStatement(
            Block(
                IfThen(
                    Constant(true),
                    Block(
                        Call(FooMethod),
                        Goto(labelTarget))),
                Label(labelTarget)),
"""
{
    if (true)
    {
        LinqToCSharpTranslatorTest.Foo();
        goto unnamedLabel;
    }

    unnamedLabel:
        ;
}
""");
    }

    [Fact]
    public void Goto_with_unnamed_labels_in_sibling_blocks()
    {
        var labelTarget1 = Label();
        var labelTarget2 = Label();

        AssertStatement(
            Block(
                Block(
                    Goto(labelTarget1),
                    Label(labelTarget1)),
                Block(
                    Goto(labelTarget2),
                    Label(labelTarget2))),
"""
{
    {
        goto unnamedLabel;
        unnamedLabel:
            ;
    }

    {
        goto unnamedLabel;
        unnamedLabel:
            ;
    }
}
""");
    }

    [Fact]
    public void Loop_statement_infinite()
        => AssertStatement(
            Loop(Call(FooMethod)),
"""
while (true)
{
    LinqToCSharpTranslatorTest.Foo();
}
""");

    [Fact]
    public void Loop_statement_with_break_and_continue()
    {
        var i = Parameter(typeof(int), "i");
        var breakLabel = Label();
        var continueLabel = Label();

        AssertStatement(
            Block(
                variables: new[] { i },
                Assign(i, Constant(0)),
                Loop(
                    Block(
                        IfThen(
                            Equal(i, Constant(100)),
                            Break(breakLabel)),
                        IfThen(
                            Equal(Modulo(i, Constant(2)), Constant(0)),
                            Continue(continueLabel)),
                        PostIncrementAssign(i)),
                    breakLabel,
                    continueLabel)),
"""
{
    var i = 0;
    {
        while (true)
        {
            unnamedLabel0:
                if (i == 100)
                {
                    goto unnamedLabel;
                }

            if (i % 2 == 0)
            {
                goto unnamedLabel0;
            }

            i++;
        }

        unnamedLabel:
            ;
    }
}
""");
    }

    [Fact]
    public void Try_catch_statement()
    {
        var e = Parameter(typeof(InvalidOperationException), "e");

        AssertStatement(
            TryCatch(
                Call(FooMethod),
                Catch(e, Call(BarMethod)),
                Catch(e, Call(BazMethod))),
"""
try
{
    LinqToCSharpTranslatorTest.Foo();
}
catch (InvalidOperationException e)
{
    LinqToCSharpTranslatorTest.Bar();
}
catch (InvalidOperationException e)
{
    LinqToCSharpTranslatorTest.Baz();
}
""");
    }

    [Fact]
    public void Try_finally_statement()
        => AssertStatement(
            TryFinally(
                Call(FooMethod),
                Call(BarMethod)),
"""
try
{
    LinqToCSharpTranslatorTest.Foo();
}
finally
{
    LinqToCSharpTranslatorTest.Bar();
}
""");

    [Fact]
    public void Try_catch_finally_statement()
    {
        var e = Parameter(typeof(InvalidOperationException), "e");

        AssertStatement(
            TryCatchFinally(
                Call(FooMethod),
                Block(
                    Call(BarMethod),
                    Call(BazMethod)),
                Catch(e, Call(BarMethod)),
                Catch(
                    e,
                    Call(BazMethod),
                    Equal(
                        Property(e, nameof(Exception.Message)),
                        Constant("foo")))),
"""
try
{
    LinqToCSharpTranslatorTest.Foo();
}
catch (InvalidOperationException e)
{
    LinqToCSharpTranslatorTest.Bar();
}
catch (InvalidOperationException e)when (e.Message == "foo")
{
    LinqToCSharpTranslatorTest.Baz();
}
finally
{
    LinqToCSharpTranslatorTest.Bar();
    LinqToCSharpTranslatorTest.Baz();
}
""");
    }

    [Fact]
    public void Try_catch_statement_with_filter()
    {
        var e = Parameter(typeof(InvalidOperationException), "e");

        AssertStatement(
            TryCatch(
                Call(FooMethod),
                Catch(
                    e,
                    Call(BarMethod),
                    Equal(
                        Property(e, nameof(Exception.Message)),
                        Constant("foo")))),
"""
try
{
    LinqToCSharpTranslatorTest.Foo();
}
catch (InvalidOperationException e)when (e.Message == "foo")
{
    LinqToCSharpTranslatorTest.Bar();
}
""");
    }

    [Fact]
    public void Try_catch_statement_without_exception_reference()
        => AssertStatement(
            TryCatch(
                Call(FooMethod),
                Catch(
                    typeof(InvalidOperationException),
                    Call(BarMethod))),
"""
try
{
    LinqToCSharpTranslatorTest.Foo();
}
catch (InvalidOperationException)
{
    LinqToCSharpTranslatorTest.Bar();
}
""");

    [Fact]
    public void Try_fault_statement()
        => AssertStatement(
            TryFault(
                Call(FooMethod),
                Call(BarMethod)),
"""
try
{
    LinqToCSharpTranslatorTest.Foo();
}
catch
{
    LinqToCSharpTranslatorTest.Bar();
}
""");

    // TODO: try/catch expressions

    private void AssertStatement(Expression expression, string expected)
        => AssertCore(expression, isStatement: true, expected);

    private void AssertExpression(Expression expression, string expected)
        => AssertCore(expression, isStatement: false, expected);

    private void AssertCore(Expression expression, bool isStatement, string expected)
    {
        var (translator, workspace) = CreateTranslator();
        var namespaces = new HashSet<string>();
        var node = isStatement
            ? translator.TranslateStatement(expression, namespaces)
            : translator.TranslateExpression(expression, namespaces);

        if (_outputExpressionTrees)
        {
            _testOutputHelper.WriteLine("---- Input LINQ expression tree:");
            _testOutputHelper.WriteLine(_expressionPrinter.PrintExpression(expression));
        }

        // TODO: Actually compile the output C# code to make sure it's valid.
        // TODO: For extra credit, execute both code representations and make sure the results are the same
        // Simplifier.ReduceAsync(expression).Result

        var code = node.NormalizeWhitespace().ToFullString();

        var projectId = ProjectId.CreateNewId();
        var versionStamp = VersionStamp.Create();
        var projectInfo = ProjectInfo.Create(projectId, versionStamp, "TestProj", "TestProj", LanguageNames.CSharp);
        workspace.AddProject(projectInfo);
        var document = workspace.AddDocument(projectId, "Test.cs", SourceText.From(code));

        var syntaxRootFoo = document.GetSyntaxRootAsync().Result!;
        var annotatedDocument = document.WithSyntaxRoot(syntaxRootFoo.WithAdditionalAnnotations(Simplifier.Annotation));
        document = Simplifier.ReduceAsync(annotatedDocument).Result;

        var actual = document.GetTextAsync().Result.ToString();

        try
        {
            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);

            if (_outputExpressionTrees)
            {
                _testOutputHelper.WriteLine("---- Output Roslyn syntax tree:");
                _testOutputHelper.WriteLine(actual);
            }
        }
        catch (EqualException)
        {
            _testOutputHelper.WriteLine("---- Output Roslyn syntax tree:");
            _testOutputHelper.WriteLine(actual);

            throw;
        }
    }

    private (LinqToCSharpTranslator, AdhocWorkspace) CreateTranslator()
    {
        var workspace = new AdhocWorkspace();
        var syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);
        return (new LinqToCSharpTranslator(syntaxGenerator), workspace);
    }

    // ReSharper disable UnusedMember.Local
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper disable UnusedParameter.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    // ReSharper disable MemberCanBePrivate.Local

    private static readonly MethodInfo ReturnsIntWithParamMethod
        = typeof(LinqToCSharpTranslatorTest).GetMethod(nameof(ReturnsIntWithParam))!;

    public static int ReturnsIntWithParam(int i)
        => i + 1;

    private static readonly MethodInfo WithInOutRefParameterMethod
        = typeof(LinqToCSharpTranslatorTest).GetMethod(nameof(WithInOutRefParameter))!;

    public static void WithInOutRefParameter(in int inParam, out int outParam, ref int refParam)
    {
        outParam = 8;
    }

    private static readonly MethodInfo GenericMethod
        = typeof(LinqToCSharpTranslatorTest).GetMethods().Single(m => m.Name == nameof(GenericMethodImplementation));

    public static int GenericMethodImplementation<T>(T t)
        => 0;

    private static readonly MethodInfo FooMethod
        = typeof(LinqToCSharpTranslatorTest).GetMethod(nameof(Foo))!;

    public static int Foo()
        => 1;

    private static readonly MethodInfo BarMethod
        = typeof(LinqToCSharpTranslatorTest).GetMethod(nameof(Bar))!;

    public static int Bar()
        => 1;

    private static readonly MethodInfo BazMethod
        = typeof(LinqToCSharpTranslatorTest).GetMethod(nameof(Baz))!;

    public static int Baz()
        => 1;

    public static int MethodWithSixParams(int a, int b, int c, int d, int e, int f)
        => a + b + c + d + e + f;

    private class Blog
    {
#pragma warning disable CS0169
        private int _privateField;
        private int _internalField;
#pragma warning restore CS0169
        private int PrivateProperty { get; set; }
        private int InternalProperty { get; set; }

        public Blog() {}
        public Blog(string name) {}
        public Blog(int foo, int bar) {}

        public int SomeInstanceMethod()
            => 3;

        public static readonly ConstructorInfo Constructor
            = typeof(Blog).GetConstructor(Array.Empty<Type>())!;

        public static int Static_method_on_nested_type()
            => 3;
    }

    private class BlogWithRequiredProperties
    {
        public required string Name { get; set; }
        public required int Rating { get; set; }

        public BlogWithRequiredProperties() {}

        public BlogWithRequiredProperties(string name)
        {
            Name = name;
        }

        [SetsRequiredMembers]
        public BlogWithRequiredProperties(string name, int rating)
        {
            Name = name;
            Rating = rating;
        }
    }

    [Flags]
    public enum SomeEnum
    {
        One = 1,
        Two = 2
    }

    // ReSharper restore UnusedMember.Local
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper restore UnusedParameter.Local
    // ReSharper restore UnusedAutoPropertyAccessor.Local
    // ReSharper restore MemberCanBePrivate.Local

    private readonly ExpressionPrinter _expressionPrinter = new();
    private readonly bool _outputExpressionTrees;
}

internal class LinqExpressionToRoslynTranslatorExtensionType
{
    public static readonly ConstructorInfo Constructor
        = typeof(LinqExpressionToRoslynTranslatorExtensionType).GetConstructor(Array.Empty<Type>())!;
}

internal static class LinqExpressionToRoslynTranslatorExtensions
{
    public static readonly MethodInfo SomeExtensionMethod
        = typeof(LinqExpressionToRoslynTranslatorExtensions).GetMethod(
            nameof(SomeExtension), new[] { typeof(LinqExpressionToRoslynTranslatorExtensionType) })!;

    public static int SomeExtension(this LinqExpressionToRoslynTranslatorExtensionType? someType)
        => 3;
}
