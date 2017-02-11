// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.DbFunctions.Internal.Initializers
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqliteStringInitializer : IDbFunctionInitalizer
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(ModelBuilder modelBuilder)
        {
            /* Replace */
            modelBuilder.DbFunction(typeof(string).GetRuntimeMethod(nameof(string.Replace), new[] { typeof(string), typeof(string) }), b =>
            {
                b.HasName("REPLACE");
                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);
            });

            /* Substring - we support both overloads for sqlite */
            modelBuilder.DbFunction(typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Substring)), b =>
            {
                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    var newArgs = args.ToList();
                    newArgs[1] = newArgs[1].NodeType == ExpressionType.Constant
                            ? (Expression)Expression.Constant(
                                (int)((ConstantExpression)newArgs[1]).Value + 1)
                            : Expression.Add(
                                newArgs[1],
                                Expression.Constant(1));

                    return new SqlFunctionExpression(
                        "substr",
                        typeof(string),
                        newArgs);
                });
            });

            /* ToLower */
            modelBuilder.DbFunction(typeof(string).GetRuntimeMethod(nameof(string.ToLower), new Type[] { }), b =>
            {
                b.HasName("lower");
                b.Parameter("stringObject").HasParameterIndex(0).IsObjectParameter(true);
            });

            /* ToUpper */
            modelBuilder.DbFunction(typeof(string).GetRuntimeMethod(nameof(string.ToUpper), new Type[] { }), b =>
            {
                b.HasName("upper");
                b.Parameter("stringObject").HasParameterIndex(0).IsObjectParameter(true);
            });

            /* TrimStart */
            modelBuilder.DbFunction(typeof(string).GetRuntimeMethods().Where(mi => mi.Name == nameof(string.TrimStart)), b =>
            {
                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression("ltrim", dbFunc.ReturnType, args);
                });

                b.AfterInitialization((dbFunc, dbFuncExp) =>
                {
                    if (dbFuncExp.Arguments.Count == 1)
                        return;

                    var args = new List<Expression> { dbFuncExp.Arguments[0] };

                    var constantChars = (dbFuncExp.Arguments[1] as ConstantExpression)?.Value as char[];

                    if (constantChars?.Length > 0)
                        args.Add(Expression.Constant(new string(constantChars), typeof(string)));

                    dbFuncExp.Arguments = new ReadOnlyCollection<Expression>(args);
                });
            });

            /* TrimEnd */
            modelBuilder.DbFunction(typeof(string).GetRuntimeMethods().Where(mi => mi.Name == nameof(string.TrimEnd)), b =>
            {
                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression("rtrim", dbFunc.ReturnType, args);
                });

                b.AfterInitialization((dbFunc, dbFuncExp) =>
                {
                    if (dbFuncExp.Arguments.Count == 1)
                        return;

                    var args = new List<Expression> { dbFuncExp.Arguments[0] };

                    var constantChars = (dbFuncExp.Arguments[1] as ConstantExpression)?.Value as char[];

                    if (constantChars?.Length > 0)
                        args.Add(Expression.Constant(new string(constantChars), typeof(string)));

                    dbFuncExp.Arguments = new ReadOnlyCollection<Expression>(args);
                });
            });

            /* Trim */
            //TODO - this should be two seperate setups - one for empty and one for params
            modelBuilder.DbFunction(typeof(string).GetTypeInfo().GetDeclaredMethods(nameof(string.Trim)), b =>
            {
                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    return new SqlFunctionExpression("trim", dbFunc.ReturnType, args);
                });

                b.AfterInitialization((dbFunc, dbFuncExp) =>
                {
                    if (dbFuncExp.Arguments.Count == 1)
                        return;

                    var args = new List<Expression> { dbFuncExp.Arguments[0] };

                    var constantChars = (dbFuncExp.Arguments[1] as ConstantExpression)?.Value as char[];

                    if (constantChars?.Length > 0)
                        args.Add(Expression.Constant(new string(constantChars), typeof(string)));

                    dbFuncExp.Arguments = new ReadOnlyCollection<Expression>(args);
                });
            });

            modelBuilder.DbFunction(typeof(string).GetRuntimeMethod(nameof(string.StartsWith), new[] { typeof(string) }), b =>
            {
                var concat = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    var patternExpression = args.ElementAt(1);
                    var patternConstantExpression = patternExpression as ConstantExpression;

                    var startsWithExpression = Expression.AndAlso(
                        new LikeExpression(
                            args.ElementAt(0),
                            Expression.Add(patternExpression, Expression.Constant("%", typeof(string)), concat)),
                        Expression.Equal(
                            new SqlFunctionExpression("instr", typeof(int), new[] { args.ElementAt(0), patternExpression }),
                            Expression.Constant(1)));

                    return patternConstantExpression != null
                        ? (string)patternConstantExpression.Value == string.Empty
                            ? (Expression)Expression.Constant(true)
                            : startsWithExpression
                        : Expression.OrElse(
                            startsWithExpression,
                            Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
                });
            });

            modelBuilder.DbFunction(typeof(string).GetRuntimeMethod(nameof(string.EndsWith), new[] { typeof(string) }), b =>
            {
                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    var patternExpression = args.ElementAt(1);
                    var patternConstantExpression = patternExpression as ConstantExpression;

                    var endsWithExpression = Expression.Equal(
                        new SqlFunctionExpression(
                            "substr",
                            typeof(string),
                            new[]
                            {
                                args.ElementAt(0),
                                Expression.Negate(new SqlFunctionExpression("length", typeof(int), new[] { patternExpression }))
                            }),
                        patternExpression);

                    return new NotNullableExpression(
                        patternConstantExpression != null
                            ? (string)patternConstantExpression.Value == string.Empty
                                ? (Expression)Expression.Constant(true)
                                : endsWithExpression
                            : Expression.OrElse(
                                endsWithExpression,
                                Expression.Equal(patternExpression, Expression.Constant(string.Empty))));
                });
            });

            modelBuilder.DbFunction(typeof(string), nameof(string.IsNullOrWhiteSpace), b =>
            {
                b.TranslateWith((args, dbFunc) =>
                {
                    return Expression.MakeBinary(
                        ExpressionType.OrElse,
                        new IsNullExpression(args.ElementAt(0)),
                        Expression.Equal(
                            new SqlFunctionExpression(
                                "trim",
                                typeof(string),
                                new[] { args.ElementAt(0) }),
                            Expression.Constant("", typeof(string))));
                });
            });

            modelBuilder.DbFunction(typeof(string), nameof(string.Contains), b =>
            {
                b.Parameter("stringObject").HasParameterIndex(0, true).IsObjectParameter(true);

                b.TranslateWith((args, dbFunc) =>
                {
                    var patternExpression = args.ElementAt(1);
                    var patternConstantExpression = patternExpression as ConstantExpression;

                    var charIndexExpression = Expression.GreaterThan(
                        new SqlFunctionExpression("instr", typeof(int), new[] { args.ElementAt(0), patternExpression }),
                        Expression.Constant(0));

                    return
                        patternConstantExpression != null
                            ? (string)patternConstantExpression.Value == string.Empty
                                ? (Expression)Expression.Constant(true)
                                : charIndexExpression
                            : Expression.OrElse(
                                charIndexExpression,
                                Expression.Equal(patternExpression, Expression.Constant(string.Empty)));
                });
            });
        }
    }
}
