// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IXGJsonPocoTranslator _jsonPocoTranslator;
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;

        protected static readonly MethodInfo[] NewArrayExpressionSupportMethodInfos = Array.Empty<MethodInfo>()
            .Concat(typeof(XGDbFunctionsExtensions).GetRuntimeMethods().Where(m => m.Name is nameof(XGDbFunctionsExtensions.Match)
                                                                                             or nameof(XGDbFunctionsExtensions.IsMatch)))
            .Concat(typeof(string).GetRuntimeMethods().Where(m => m.Name is nameof(string.Concat)
                                                                         or nameof(string.Join)))
            .Where(m => m.GetParameters().Any(p => p.ParameterType.IsArray))
            .ToArray();

        protected static readonly MethodInfo ElementAtMethodInfo = typeof(Enumerable)
            .GetRuntimeMethods()
            .Single(m => m.Name == nameof(Enumerable.ElementAt) &&
                         m.GetParameters()
                             .Select(
                                 p => p.ParameterType.IsGenericType
                                     ? p.ParameterType.GetGenericTypeDefinition()
                                     : p.ParameterType)
                             .SequenceEqual(new[] { typeof(IEnumerable<>), typeof(int) }));

        public XGSqlTranslatingExpressionVisitor(
            RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
            QueryCompilationContext queryCompilationContext,
            QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
            [CanBeNull] IXGJsonPocoTranslator jsonPocoTranslator)
            : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
        {
            _queryCompilationContext = queryCompilationContext;
            _jsonPocoTranslator = jsonPocoTranslator;
            _sqlExpressionFactory = (XGSqlExpressionFactory)Dependencies.SqlExpressionFactory;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                XGBipolarExpression bipolarExpression => VisitXGBipolarExpression(bipolarExpression),
                _ => base.VisitExtension(extensionExpression)
            };

        private Expression VisitXGBipolarExpression(XGBipolarExpression bipolarExpression)
        {
            var defaultExpression = Visit(bipolarExpression.DefaultExpression) ?? QueryCompilationContext.NotTranslatedExpression;
            var alternativeExpression = Visit(bipolarExpression.AlternativeExpression) ?? QueryCompilationContext.NotTranslatedExpression;

            return defaultExpression != QueryCompilationContext.NotTranslatedExpression
                // ? alternativeExpression != QueryCompilationContext.NotTranslatedExpression
                //     // ? new XGBipolarSqlExpression(
                //     //     (SqlExpression)defaultExpression,
                //     //     (SqlExpression)alternativeExpression)
                //     ? QueryCompilationContext.NotTranslatedExpression
                //     : (SqlExpression)defaultExpression
                ? (SqlExpression)defaultExpression
                : alternativeExpression != QueryCompilationContext.NotTranslatedExpression
                    ? (SqlExpression)alternativeExpression
                    : QueryCompilationContext.NotTranslatedExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType == ExpressionType.ArrayLength)
            {
                if (TranslationFailed(unaryExpression.Operand, Visit(unaryExpression.Operand), out var sqlOperand))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (sqlOperand.Type == typeof(byte[]) &&
                    (sqlOperand.TypeMapping is null or XGByteArrayTypeMapping))
                {
                    return _sqlExpressionFactory.NullableFunction(
                        "LENGTH",
                        new[] {sqlOperand},
                        typeof(int));
                }

                return _jsonPocoTranslator?.TranslateArrayLength(sqlOperand) ??
                       QueryCompilationContext.NotTranslatedExpression;
            }

            // Make explicit casts implicit if they are applied to a JSON traversal object.
            // It is pretty common for Newtonsoft.Json objects to be cast to other types (e.g. casting from
            // JToken to JArray to check an arrays length via the JContainer.Count property).
            if (unaryExpression.NodeType == ExpressionType.Convert ||
                unaryExpression.NodeType == ExpressionType.ConvertChecked)
            {
                var visitedOperand = Visit(unaryExpression.Operand);
                if (visitedOperand is XGJsonTraversalExpression traversal)
                {
                    return unaryExpression.Type == typeof(object)
                        ? traversal
                        : traversal.Clone(
                            traversal.ReturnsText,
                            unaryExpression.Type,
                            Dependencies.TypeMappingSource.FindMapping(unaryExpression.Type));
                }

                ResetTranslationErrorDetails();
            }

            return base.VisitUnary(unaryExpression);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
            {
                if (TranslationFailed(binaryExpression.Left, Visit(TryRemoveImplicitConvert(binaryExpression.Left)), out var sqlLeft)
                    || TranslationFailed(binaryExpression.Right, Visit(TryRemoveImplicitConvert(binaryExpression.Right)), out var sqlRight))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                if (binaryExpression.Left.Type == typeof(byte[]))
                {
                    return TranslateByteArrayElementAccess(sqlLeft, sqlRight);
                }

                // Try translating ArrayIndex inside json column
                var expression = _jsonPocoTranslator?.TranslateMemberAccess(
                    sqlLeft,
                    _sqlExpressionFactory.JsonArrayIndex(sqlRight),
                    binaryExpression.Type);

                if (expression is not null)
                {
                    return expression;
                }
            }

            // These are all legal operations in .NET:
            //     DateTime = DateTime + TimeSpan
            //     DateTimeOffset = DateTimeOffset + TimeSpan
            //     TimeSpan = TimeSpan + TimeSpan
            //
            // .NET does currently not support the following operations:
            //     DateOnly = DateOnly + TimeSpan
            //     TimeOnly = TimeOnly + TimeSpan
            //
            // TODO: Add support for DateTime and DateTimeOffset.
            // To support DateTime and DateTimeOffset in Pomelo, we need propert TimeSpan support (representing MICROSECONDS or Ticks stored
            // as BIGINT, not just a simple mapping to TIME, which is very limited in its range).
            // if (binaryExpression.NodeType == ExpressionType.Add &&
            //     Visit(binaryExpression.Left) is SqlExpression addLeftVisited &&
            //     Visit(binaryExpression.Right) is SqlExpression addRightVisited &&
            //     addRightVisited.Type == typeof(TimeSpan) &&
            //     (addLeftVisited.Type == typeof(DateTime) ||
            //      // addLeftVisited.Type == typeof(TimeSpan) || // <-- This should work out-of-the-box for TimeSpan -> TIME mappings.
            //      addLeftVisited.Type == typeof(DateTimeOffset)))
            // {
            //     // Possible avenue for DateTime and DateTimeOffset support:
            //     //
            //     // return _sqlExpressionFactory.Add(
            //     //     addLeftVisited,
            //     //     _sqlExpressionFactory.ComplexFunctionArgument(
            //     //         new SqlExpression[]
            //     //         {
            //     //             _sqlExpressionFactory.Fragment("INTERVAL"), addRightVisited is SqlConstantExpression { Value: TimeSpan timeSpanValue }
            //     //                 ? _sqlExpressionFactory.Constant(timeSpanValue.Ticks / 10)
            //     //                 : _sqlExpressionFactory.NullableFunction(
            //     //                     "TIMESTAMPDIFF",
            //     //                     new[]
            //     //                     {
            //     //                         _sqlExpressionFactory.Fragment("MICROSECOND"),
            //     //                         _sqlExpressionFactory.Constant(DateTime.MinValue), addRightVisited,
            //     //                     },
            //     //                     typeof(TimeSpan),
            //     //                     typeMapping: null,
            //     //                     onlyNullWhenAnyNullPropagatingArgumentIsNull: true,
            //     //                     argumentsPropagateNullability: new[] { false, true, true }),
            //     //             _sqlExpressionFactory.Fragment("MICROSECOND")
            //     //         },
            //     //         " ",
            //     //         typeof(string)),
            //     //     addLeftVisited.TypeMapping);
            // }

            // These are all legal operations in .NET:
            //     TimeSpan = DateTime - DateTime
            //     TimeSpan = DateTimeOffset - DateTimeOffset
            //     TimeSpan = TimeOnly - TimeOnly
            //
            // .NET does currently not support the following operations:
            //     TimeSpan = DateOnly - DateOnly
            //
            // TODO: Add support for DateTime and DateTimeOffset.
            // To support DateTime and DateTimeOffset in Pomelo, we need propert TimeSpan support (representing MICROSECONDS or Ticks stored
            // as BIGINT, not just a simple mapping to TIME, which is very limited in its range).
            if (binaryExpression.NodeType == ExpressionType.Subtract &&
                Visit(binaryExpression.Left) is SqlExpression subtractLeftVisited &&
                Visit(binaryExpression.Right) is SqlExpression subtractRightVisited &&
                (/*subtractLeftVisited.Type == typeof(DateTime) && subtractRightVisited.Type == typeof(DateTime) ||
                 subtractLeftVisited.Type == typeof(DateTimeOffset) && subtractRightVisited.Type == typeof(DateTimeOffset) ||*/
                 subtractLeftVisited.Type == typeof(TimeOnly) && subtractRightVisited.Type == typeof(TimeOnly)))
            {
                return _sqlExpressionFactory.Subtract(
                    subtractLeftVisited,
                    subtractRightVisited,
                    Dependencies.TypeMappingSource.FindMapping(typeof(TimeSpan)));

                // Previous statement is simpler than this:
                //
                // return _sqlExpressionFactory.NullableFunction(
                //     "TIMEDIFF",
                //     new[]
                //     {
                //         left,
                //         right,
                //     },
                //     typeof(TimeSpan));

                // Possible avenue for DateTime and DateTimeOffset support:
                //
                // var left = subtractLeftVisited.Type == typeof(TimeOnly)
                //     ? _sqlExpressionFactory.NullableFunction(
                //         "ADDTIME",
                //         new[] { _sqlExpressionFactory.Constant(_options.NeutralDateTime), subtractLeftVisited },
                //         typeof(DateTime))
                //     : subtractLeftVisited;
                //
                // var right = subtractRightVisited.Type == typeof(TimeOnly)
                //     ? _sqlExpressionFactory.NullableFunction(
                //         "ADDTIME",
                //         new[] { _sqlExpressionFactory.Constant(_options.NeutralDateTime), subtractRightVisited },
                //         typeof(DateTime))
                //     : subtractRightVisited;
                //
                // return _sqlExpressionFactory.NullableFunction(
                //     "TIMESTAMPDIFF",
                //     new[]
                //     {
                //         _sqlExpressionFactory.Fragment("MICROSECOND"),
                //         right,
                //         left,
                //     },
                //     typeof(TimeSpan),
                //     typeMapping: null,
                //     onlyNullWhenAnyNullPropagatingArgumentIsNull: true,
                //     argumentsPropagateNullability: new[] { false, true, true });
            }

            return base.VisitBinary(binaryExpression);
        }

        private Expression TranslateByteArrayElementAccess(Expression array, Expression index)
            => Visit(array) is SqlExpression leftSql &&
               Visit(index) is SqlExpression rightSql
                ? _sqlExpressionFactory.NullableFunction(
                    "ASCII",
                    new[]
                    {
                        _sqlExpressionFactory.NullableFunction(
                            "SUBSTRING",
                            new[]
                            {
                                leftSql, Dependencies.SqlExpressionFactory.Add(
                                    Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(rightSql),
                                    Dependencies.SqlExpressionFactory.Constant(1)),
                                Dependencies.SqlExpressionFactory.Constant(1)
                            },
                            typeof(byte[]))
                    },
                    typeof(byte))
                : QueryCompilationContext.NotTranslatedExpression;

        protected virtual Expression VisitMethodCallNewArray(NewArrayExpression newArrayExpression)
        {
            // Needed for XGDbFunctionsExtensions.Match(), String.Concat() and String.Join() translations.
            if (newArrayExpression.Type == typeof(string[]))
            {
                return _sqlExpressionFactory.ComplexFunctionArgument(
                    newArrayExpression.Expressions.Select(e => (SqlExpression)Visit(e))
                        .ToArray(),
                    ", ",
                    typeof(string[]));
            }

            // Needed for String.Concat() and String.Join() translations.
            if (newArrayExpression.Type == typeof(object[]))
            {
                var typeMapping = ((XGStringTypeMapping)Dependencies.TypeMappingSource.GetMapping(typeof(string))).Clone(forceToString: true);
                return _sqlExpressionFactory.ComplexFunctionArgument(
                    newArrayExpression.Expressions.Select(e => Dependencies.SqlExpressionFactory.ApplyTypeMapping((SqlExpression)Visit(e), typeMapping))
                        .ToArray(),
                    ", ",
                    typeof(object[]),
                    typeMapping);
            }

            return base.VisitNewArray(newArrayExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == ElementAtMethodInfo
                && methodCallExpression.Arguments[0].Type == typeof(byte[]))
            {
                return TranslateByteArrayElementAccess(
                    methodCallExpression.Arguments[0],
                    methodCallExpression.Arguments[1]);
            }

            if (NewArrayExpressionSupportMethodInfos.Contains(methodCallExpression.Method))
            {
                var arguments = new Expression[methodCallExpression.Arguments.Count];
                for (var i = 0; i < arguments.Length; i++)
                {
                    var argument = methodCallExpression.Arguments[i];

                    if (argument is NewArrayExpression newArrayExpression)
                    {
                        if (TranslationFailed(argument, VisitMethodCallNewArray(newArrayExpression), out var sqlExpression))
                        {
                            return QueryCompilationContext.NotTranslatedExpression;
                        }

                        arguments[i] = sqlExpression;
                    }
                    else
                    {
                        arguments[i] = argument;
                    }
                }

                methodCallExpression = methodCallExpression.Update(methodCallExpression.Object, arguments);
            }

            var result = CallBaseVisitMethodCall(methodCallExpression);

            if (result == QueryCompilationContext.NotTranslatedExpression &&
                XGStringComparisonMethodTranslator.StringComparisonMethodInfos.Any(m => m == methodCallExpression.Method))
            {
                var message = XGStrings.QueryUnableToTranslateMethodWithStringComparison(
                    methodCallExpression.Method.DeclaringType.Name,
                    methodCallExpression.Method.Name,
                    nameof(XGDbContextOptionsBuilder.EnableStringComparisonTranslations));

                // EF Core returns an error message on its own, when the string.Equals() methods (static and non-static) are being used with
                // a `StringComparison` parameter.
                // Since we also support other translations, but all of them only when opted in, we will replace the EF Core error message
                // with our own, that is more appropriate for our case.
                if (TranslationErrorDetails.Contains(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison))
                {
                    var translationErrorDetails = TranslationErrorDetails;
                    ResetTranslationErrorDetails();
                    message = translationErrorDetails.Replace(CoreStrings.QueryUnableToTranslateStringEqualsWithStringComparison, message);
                }

                AddTranslationErrorDetails(message);
            }

            return result;
        }

        /// <summary>
        /// EF Core does not forward the current QueryCompilationContext to IMethodCallTranslator implementations.
        /// Our XGMethodCallTranslatorProvider and XGQueryCompilationContextMethodTranslator implementations take care of that.
        /// </summary>
        private Expression CallBaseVisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var xgMethodCallTranslatorProvider = (XGMethodCallTranslatorProvider)Dependencies.MethodCallTranslatorProvider;

            if (xgMethodCallTranslatorProvider.QueryCompilationContext is null)
            {
                xgMethodCallTranslatorProvider.QueryCompilationContext = _queryCompilationContext;

                try
                {
                    return base.VisitMethodCall(methodCallExpression);
                }
                finally
                {
                    xgMethodCallTranslatorProvider.QueryCompilationContext = null;
                }
            }

            if (xgMethodCallTranslatorProvider.QueryCompilationContext == _queryCompilationContext)
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            throw new UnreachableException();
        }

        protected virtual void ResetTranslationErrorDetails()
        {
            // When we try translating an expression and later decide that we want to discard the result, we need to remove any translation
            // error details, or those details might end up more than once in generated exceptions down the stack.
            //
            // We use a workaround here, that will result in the TranslationErrorDetails being set to `null` again.
            // Otherwise, we would need to override TranslationErrorDetails, AddTranslationErrorDetails and Translate, reimplement the
            // TranslationErrorDetails functionality and maintain everything just to support resetting the TranslationErrorDetails property.
            base.Translate(Expression.Constant(0));
        }

        public override SqlExpression GenerateGreatest(IReadOnlyList<SqlExpression> expressions, Type resultType)
            => _sqlExpressionFactory.NullableFunction(
                "GREATEST",
                expressions,
                resultType,
                true);

        public override SqlExpression GenerateLeast(IReadOnlyList<SqlExpression> expressions, Type resultType)
            => _sqlExpressionFactory.NullableFunction(
                "LEAST",
                expressions,
                resultType,
                true);

        #region Copied from RelationalSqlTranslatingExpressionVisitor

        private static Expression TryRemoveImplicitConvert(Expression expression)
        {
            if (expression is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked)
                {
                    var innerType = unaryExpression.Operand.Type.UnwrapNullableType();
                    if (innerType.IsEnum)
                    {
                        innerType = Enum.GetUnderlyingType(innerType);
                    }
                    var convertedType = unaryExpression.Type.UnwrapNullableType();

                    if (innerType == convertedType
                        || (convertedType == typeof(int)
                            && (innerType == typeof(byte)
                                || innerType == typeof(sbyte)
                                || innerType == typeof(char)
                                || innerType == typeof(short)
                                || innerType == typeof(ushort))))
                    {
                        return TryRemoveImplicitConvert(unaryExpression.Operand);
                    }
                }
            }

            return expression;
        }

        [DebuggerStepThrough]
        private bool TranslationFailed(Expression original, Expression translation, out SqlExpression castTranslation)
        {
            if (original != null && translation is not SqlExpression)
            {
                castTranslation = null;
                return true;
            }

            castTranslation = translation as SqlExpression;
            return false;
        }

        #endregion Copied from RelationalSqlTranslatingExpressionVisitor
    }
}
