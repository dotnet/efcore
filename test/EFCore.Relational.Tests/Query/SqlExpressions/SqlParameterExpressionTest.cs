// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public class SqlParameterExpressionTest
{
    [Theory, InlineData(false, null), InlineData(true, ParameterTranslationMode.Constant)]
    public void Quote_preserves_parameter_identity_and_options(bool nullable, ParameterTranslationMode? translationMode)
    {
        var parameter = new SqlParameterExpression(
            "runtime_p", "p", typeof(int), nullable, translationMode, typeMapping: null);

        var quoted = Expression.Lambda<Func<SqlParameterExpression>>(parameter.Quote()).Compile(preferInterpretation: true)();

        Assert.Equal(parameter.InvariantName, quoted.InvariantName);
        Assert.Equal(parameter.Name, quoted.Name);
        Assert.Equal(parameter.Type, quoted.Type);
        Assert.Equal(parameter.IsNullable, quoted.IsNullable);
        Assert.Equal(parameter.TranslationMode, quoted.TranslationMode);
        Assert.Equal(parameter.TypeMapping, quoted.TypeMapping);
        Assert.Equal(parameter, quoted);
        Assert.Equal(parameter.GetHashCode(), quoted.GetHashCode());
    }

    [Fact]
    public void Different_runtime_parameters_with_same_sql_name_are_not_equal()
    {
        var typeMapping = IntTypeMapping.Default;
        var first = new SqlParameterExpression("first", "p", typeof(int), false, null, typeMapping);
        var second = new SqlParameterExpression("second", "p", typeof(int), false, null, typeMapping);

        Assert.NotEqual(first, second);
    }
}
