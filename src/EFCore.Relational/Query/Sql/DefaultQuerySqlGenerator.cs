// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

// ReSharper disable SwitchStatementMissingSomeCases
// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     The default query SQL generator.
    /// </summary>
    public class DefaultQuerySqlGenerator : ThrowingExpressionVisitor, ISqlExpressionVisitor, IQuerySqlGenerator
    {
        private IRelationalCommandBuilder _relationalCommandBuilder;
        private IReadOnlyDictionary<string, object> _parametersValues;
        private ParameterNameGenerator _parameterNameGenerator;
        private RelationalTypeMapping _typeMapping;
        private NullComparisonTransformingVisitor _nullComparisonTransformingVisitor;
        private RelationalNullsExpandingVisitor _relationalNullsExpandingVisitor;
        private PredicateReductionExpressionOptimizer _predicateReductionExpressionOptimizer;
        private PredicateNegationExpressionOptimizer _predicateNegationExpressionOptimizer;
        private ReducingExpressionVisitor _reducingExpressionVisitor;
        private BooleanExpressionTranslatingVisitor _booleanExpressionTranslatingVisitor;
        private InExpressionValuesExpandingVisitor _inExpressionValuesExpandingVisitor;

        private bool _valueConverterWarningsEnabled;

        private static readonly Dictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " <> " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },
            { ExpressionType.And, " & " },
            { ExpressionType.Or, " | " }
        };

        /// <summary>
        ///     Creates a new instance of <see cref="DefaultQuerySqlGenerator" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="selectExpression"> The select expression. </param>
        protected DefaultQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression)

        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(selectExpression, nameof(selectExpression));

            Dependencies = dependencies;
            SelectExpression = selectExpression;
        }

        /// <summary>
        ///     Whether or not the generated SQL could have out-of-order projection columns.
        /// </summary>
        public virtual bool RequiresRuntimeProjectionRemapping => false;

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual QuerySqlGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Gets a value indicating whether this SQL query is cacheable.
        /// </summary>
        /// <value>
        ///     true if this SQL query is cacheable, false if not.
        /// </value>
        public virtual bool IsCacheable { get; protected set; }

        /// <summary>
        ///     Gets the select expression.
        /// </summary>
        /// <value>
        ///     The select expression.
        /// </value>
        protected virtual SelectExpression SelectExpression { get; }

        /// <summary>
        ///     Gets the SQL generation helper.
        /// </summary>
        /// <value>
        ///     The SQL generation helper.
        /// </value>
        protected virtual ISqlGenerationHelper SqlGenerator => Dependencies.SqlGenerationHelper;

        /// <summary>
        ///     Gets the parameter values.
        /// </summary>
        /// <value>
        ///     The parameter values.
        /// </value>
        protected virtual IReadOnlyDictionary<string, object> ParameterValues => _parametersValues;

        /// <summary>
        ///     Generates SQL for the given parameter values.
        /// </summary>
        /// <param name="parameterValues"> The parameter values. </param>
        /// <returns>
        ///     A relational command.
        /// </returns>
        public virtual IRelationalCommand GenerateSql(IReadOnlyDictionary<string, object> parameterValues)
        {
            Check.NotNull(parameterValues, nameof(parameterValues));

            _relationalCommandBuilder = Dependencies.CommandBuilderFactory.Create();
            _parameterNameGenerator = Dependencies.ParameterNameGeneratorFactory.Create();

            _parametersValues = parameterValues;
            _nullComparisonTransformingVisitor = new NullComparisonTransformingVisitor(parameterValues);
            _inExpressionValuesExpandingVisitor = new InExpressionValuesExpandingVisitor(parameterValues);

            IsCacheable = true;

            GenerateTagsHeaderComment();

            Visit(SelectExpression);

            return _relationalCommandBuilder.Build();
        }

        /// <summary>
        ///     Generates the tags header comment.
        /// </summary>
        protected virtual void GenerateTagsHeaderComment()
        {
            if (SelectExpression.Tags.Count > 0)
            {
                foreach (var tag in SelectExpression.Tags)
                {
                    using (var reader = new StringReader(tag))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {

                            _relationalCommandBuilder.Append(SingleLineCommentToken).Append(" ").AppendLine(line);
                        }
                    }

                    _relationalCommandBuilder.AppendLine();
                }
            }
        }

        /// <summary>
        ///     Creates a relational value buffer factory.
        /// </summary>
        /// <param name="relationalValueBufferFactoryFactory"> The relational value buffer factory. </param>
        /// <param name="dataReader"> The data reader. </param>
        /// <returns>
        ///     The new value buffer factory.
        /// </returns>
        public virtual IRelationalValueBufferFactory CreateValueBufferFactory(
            IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory, DbDataReader dataReader)
        {
            Check.NotNull(relationalValueBufferFactoryFactory, nameof(relationalValueBufferFactoryFactory));

            return relationalValueBufferFactoryFactory
                .Create(SelectExpression.GetMappedProjectionTypes().ToArray());
        }

        /// <summary>
        ///     Information about the types being projected by this query.
        /// </summary>
        public virtual IReadOnlyList<TypeMaterializationInfo> GetTypeMaterializationInfos()
            => SelectExpression.GetMappedProjectionTypes().ToArray();

        /// <summary>
        ///     The generated SQL.
        /// </summary>
        protected virtual IRelationalCommandBuilder Sql => _relationalCommandBuilder;

        /// <summary>
        ///     The default true literal SQL.
        /// </summary>
        protected virtual string TypedTrueLiteral => "CAST(1 AS BIT)";

        /// <summary>
        ///     The default false literal SQL.
        /// </summary>
        protected virtual string TypedFalseLiteral => "CAST(0 AS BIT)";

        /// <summary>
        ///     The default alias separator.
        /// </summary>
        protected virtual string AliasSeparator { get; } = " AS ";

        /// <summary>
        ///     The default single line comment prefix.
        /// </summary>
        protected virtual string SingleLineCommentToken { get; } = "--";

        /// <summary>
        ///     Visit a top-level SelectExpression.
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            IDisposable subQueryIndent = null;

            if (selectExpression.Alias != null)
            {
                _relationalCommandBuilder.AppendLine("(");

                subQueryIndent = _relationalCommandBuilder.Indent();
            }

            _relationalCommandBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _relationalCommandBuilder.Append("DISTINCT ");
            }

            GenerateTop(selectExpression);

            var projectionAdded = false;

            if (selectExpression.IsProjectStar)
            {
                var tableAlias = selectExpression.ProjectStarTable.Alias;

                _relationalCommandBuilder
                    .Append(SqlGenerator.DelimitIdentifier(tableAlias))
                    .Append(".*");

                projectionAdded = true;
            }

            if (selectExpression.Projection.Count > 0)
            {
                if (selectExpression.IsProjectStar)
                {
                    _relationalCommandBuilder.Append(", ");
                }

                GenerateList(selectExpression.Projection, GenerateProjection);

                projectionAdded = true;
            }

            if (!projectionAdded)
            {
                _relationalCommandBuilder.Append("1");
            }

            var oldValueConverterWarningsEnabled = _valueConverterWarningsEnabled;

            _valueConverterWarningsEnabled = true;

            if (selectExpression.Tables.Count > 0)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("FROM ");

                GenerateList(selectExpression.Tables, sql => sql.AppendLine());
            }
            else
            {
                GeneratePseudoFromClause();
            }

            if (selectExpression.Predicate != null)
            {
                GeneratePredicate(selectExpression.Predicate);
            }

            if (selectExpression.GroupBy.Count > 0)
            {
                _relationalCommandBuilder.AppendLine();

                _relationalCommandBuilder.Append("GROUP BY ");
                GenerateList(selectExpression.GroupBy);
            }

            if (selectExpression.Having != null)
            {
                GenerateHaving(selectExpression.Having);
            }

            if (selectExpression.OrderBy.Count > 0)
            {
                var orderByList = new List<Ordering>(selectExpression.OrderBy);

                // Filter out constant and parameter expressions (SELECT 1) if there is no skip or take #10410
                if (selectExpression.Limit == null && selectExpression.Offset == null)
                { 
                    orderByList.RemoveAll(o => IsOrderByExpressionConstant(ApplyOptimizations(o.Expression, searchCondition: false)));
                }

                if (orderByList.Count > 0)
                { 
                    _relationalCommandBuilder.AppendLine();
                    
                    GenerateOrderBy(orderByList);
                }
            }

            GenerateLimitOffset(selectExpression);

            if (subQueryIndent != null)
            {
                subQueryIndent.Dispose();

                _relationalCommandBuilder.AppendLine()
                    .Append(")");

                if (selectExpression.Alias.Length > 0)
                {
                    _relationalCommandBuilder
                        .Append(AliasSeparator)
                        .Append(SqlGenerator.DelimitIdentifier(selectExpression.Alias));
                }
            }

            _valueConverterWarningsEnabled = oldValueConverterWarningsEnabled;

            selectExpression.DetachContext();

            return selectExpression;
        }

        /// <summary>
        ///     Generates a pseudo FROM clause. Required by some providers
        ///     when a query has no actual FROM clause.
        /// </summary>
        protected virtual void GeneratePseudoFromClause()
        {
        }

        private Expression ApplyOptimizations(Expression expression, bool searchCondition, bool joinCondition = false)
        {
            var newExpression = _nullComparisonTransformingVisitor.Visit(expression);

            if (_relationalNullsExpandingVisitor == null)
            {
                _relationalNullsExpandingVisitor = new RelationalNullsExpandingVisitor();
            }

            if (_predicateReductionExpressionOptimizer == null)
            {
                _predicateReductionExpressionOptimizer = new PredicateReductionExpressionOptimizer();
            }

            if (_predicateNegationExpressionOptimizer == null)
            {
                _predicateNegationExpressionOptimizer = new PredicateNegationExpressionOptimizer();
            }

            if (_reducingExpressionVisitor == null)
            {
                _reducingExpressionVisitor = new ReducingExpressionVisitor();
            }

            if (_booleanExpressionTranslatingVisitor == null)
            {
                _booleanExpressionTranslatingVisitor = new BooleanExpressionTranslatingVisitor();
            }

            if (joinCondition
                && newExpression is BinaryExpression binaryExpression
                && binaryExpression.NodeType == ExpressionType.Equal)
            {
                newExpression = Expression.MakeBinary(
                    binaryExpression.NodeType,
                    ApplyNullSemantics(binaryExpression.Left),
                    ApplyNullSemantics(binaryExpression.Right));
            }
            else
            {
                newExpression = ApplyNullSemantics(newExpression);
            }

            newExpression = _inExpressionValuesExpandingVisitor.Visit(newExpression);
            if (_inExpressionValuesExpandingVisitor.IsParameterDependent)
            {
                IsCacheable = false;
            }

            newExpression = _predicateReductionExpressionOptimizer.Visit(newExpression);
            newExpression = _predicateNegationExpressionOptimizer.Visit(newExpression);
            newExpression = _reducingExpressionVisitor.Visit(newExpression);

            return _booleanExpressionTranslatingVisitor.Translate(newExpression, searchCondition);
        }

        private class InExpressionValuesExpandingVisitor : RelinqExpressionVisitor
        {
            private readonly IReadOnlyDictionary<string, object> _parametersValues;

            public bool IsParameterDependent { get; private set; }

            public InExpressionValuesExpandingVisitor(IReadOnlyDictionary<string, object> parameterValues)
                => _parametersValues = parameterValues;

            protected override Expression VisitExtension(Expression node)
            {
                var updatedExpression = base.VisitExtension(node);

                if (updatedExpression is InExpression inExpression)
                {
                    if (inExpression.Values != null)
                    {
                        var inValues = ProcessInExpressionValues(inExpression.Values);
                        var inValuesNotNull = ExtractNonNullExpressionValues(inValues);

                        var updatedInExpression = inValuesNotNull.Count > 0
                            ? new InExpression(inExpression.Operand, inValuesNotNull)
                            : (Expression)Expression.Constant(false);

                        return inValues.Count != inValuesNotNull.Count
                            ? Expression.OrElse(
                                updatedInExpression,
                                new IsNullExpression(inExpression.Operand))
                            : updatedInExpression;
                    }
                }

                return updatedExpression;
            }

            private IReadOnlyList<Expression> ProcessInExpressionValues(
                [NotNull] IEnumerable<Expression> inExpressionValues)
            {
                Check.NotNull(inExpressionValues, nameof(inExpressionValues));

                var inConstants = new List<Expression>();

                foreach (var inValue in inExpressionValues)
                {
                    switch (inValue)
                    {
                        case ConstantExpression constantExpression:
                            AddInExpressionValues(constantExpression.Value, inConstants, constantExpression);
                            break;

                        case ParameterExpression parameterExpression:
                            if (_parametersValues.TryGetValue(parameterExpression.Name, out var parameterValue))
                            {
                                if (parameterValue == null
                                    && typeof(IEnumerable).IsAssignableFrom(parameterExpression.Type)
                                    && parameterExpression.Type != typeof(string)
                                    && parameterExpression.Type != typeof(byte[]))
                                {
                                    throw new InvalidOperationException(
                                        RelationalStrings.ExpectedNonNullParameter(parameterExpression.Name));
                                }

                                AddInExpressionValues(parameterValue, inConstants, parameterExpression);

                                IsParameterDependent = true;
                            }

                            break;

                        case ListInitExpression listInitExpression:
                            inConstants.AddRange(
                                ProcessInExpressionValues(
                                    listInitExpression.Initializers.SelectMany(i => i.Arguments)));
                            break;

                        case NewArrayExpression newArrayExpression:
                            inConstants.AddRange(ProcessInExpressionValues(newArrayExpression.Expressions));
                            break;
                    }
                }

                return inConstants;
            }

            private static void AddInExpressionValues(
                object value, List<Expression> inConstants, Expression expression)
            {
                if (value is IEnumerable valuesEnumerable
                    && value.GetType() != typeof(string)
                    && value.GetType() != typeof(byte[]))
                {
                    inConstants.AddRange(valuesEnumerable.Cast<object>().Select(Expression.Constant));
                }
                else
                {
                    inConstants.Add(expression);
                }
            }

            private IReadOnlyList<Expression> ExtractNonNullExpressionValues(
                [NotNull] IEnumerable<Expression> inExpressionValues)
            {
                var inValuesNotNull = new List<Expression>();

                foreach (var inValue in inExpressionValues)
                {
                    switch (inValue)
                    {
                        case ConstantExpression constantExpression:
                            if (constantExpression.Value != null)
                            {
                                inValuesNotNull.Add(inValue);
                            }

                            break;

                        case ParameterExpression parameterExpression:
                            if (_parametersValues.TryGetValue(parameterExpression.Name, out var parameterValue))
                            {
                                if (parameterValue != null)
                                {
                                    inValuesNotNull.Add(inValue);
                                }
                            }

                            break;
                    }
                }

                return inValuesNotNull;
            }
        }

        private Expression ApplyNullSemantics(Expression expression)
        {
            var relationalNullsOptimizedExpandingVisitor = new RelationalNullsOptimizedExpandingVisitor();
            var optimizedRightExpression = relationalNullsOptimizedExpandingVisitor.Visit(expression);

            return relationalNullsOptimizedExpandingVisitor.IsOptimalExpansion
                ? optimizedRightExpression
                : _relationalNullsExpandingVisitor.Visit(expression);
        }

        /// <summary>
        ///     Visit a single projection in SQL SELECT clause
        /// </summary>
        /// <param name="projection"> The projection expression. </param>
        protected virtual void GenerateProjection([NotNull] Expression projection)
            => Visit(
                ApplyExplicitCastToBoolInProjectionOptimization(
                    ApplyOptimizations(projection, searchCondition: false)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Expression ApplyExplicitCastToBoolInProjectionOptimization(Expression expression) => expression;

        /// <summary>
        ///     Visit the predicate in SQL WHERE clause
        /// </summary>
        /// <param name="predicate"> The predicate expression. </param>
        protected virtual void GeneratePredicate([NotNull] Expression predicate)
        {
            var optimizedPredicate = ApplyOptimizations(predicate, searchCondition: true);

            if (optimizedPredicate is BinaryExpression binaryExpression)
            {
                var leftBooleanConstant = GetBooleanConstantValue(binaryExpression.Left);
                var rightBooleanConstant = GetBooleanConstantValue(binaryExpression.Right);

                if ((binaryExpression.NodeType == ExpressionType.Equal
                    && leftBooleanConstant == true
                    && rightBooleanConstant == true)
                    || (binaryExpression.NodeType == ExpressionType.NotEqual
                    && leftBooleanConstant == false
                    && rightBooleanConstant == false))
                {
                    return;
                }
            }

            _relationalCommandBuilder.AppendLine()
                .Append("WHERE ");

            Visit(optimizedPredicate);
        }

        /// <summary>
        ///     Visit the predicate in SQL HAVING clause
        /// </summary>
        /// <param name="predicate"> The having predicate expression. </param>
        protected virtual void GenerateHaving([NotNull] Expression predicate)
        {
            var optimizedPredicate = ApplyOptimizations(predicate, searchCondition: true);

            _relationalCommandBuilder.AppendLine()
                .Append("HAVING ");

            Visit(optimizedPredicate);
        }

        private static bool? GetBooleanConstantValue(Expression expression)
            => expression is ConstantExpression constantExpression
               && constantExpression.Type.UnwrapNullableType() == typeof(bool)
                ? (bool?)constantExpression.Value
                : null;
        
        private bool IsOrderByExpressionConstant([NotNull] Expression processedExpression)
        { 
            return processedExpression.RemoveConvert() is ConstantExpression
                || processedExpression.RemoveConvert() is ParameterExpression;
        }

        /// <summary>
        ///     Generates the ORDER BY SQL.
        /// </summary>
        /// <param name="orderings"> The orderings. </param>
        protected virtual void GenerateOrderBy([NotNull] IReadOnlyList<Ordering> orderings)
        {
            _relationalCommandBuilder.Append("ORDER BY ");

            GenerateList(orderings, GenerateOrdering);
        }

        /// <summary>
        ///     Generates a single ordering in an SQL ORDER BY clause.
        /// </summary>
        /// <param name="ordering"> The ordering. </param>
        protected virtual void GenerateOrdering([NotNull] Ordering ordering)
        {
            Check.NotNull(ordering, nameof(ordering));

            var orderingExpression = ordering.Expression;

            if (orderingExpression is AliasExpression aliasExpression)
            {
                _relationalCommandBuilder.Append(SqlGenerator.DelimitIdentifier(aliasExpression.Alias));
            }
            else
            {
                var processedExpression = ApplyOptimizations(orderingExpression, searchCondition: false);
                if (IsOrderByExpressionConstant(processedExpression))
                {
                    _relationalCommandBuilder.Append("(SELECT 1");
                    GeneratePseudoFromClause();
                    _relationalCommandBuilder.Append(")");
                }
                else
                {
                    Visit(processedExpression);
                }
            }

            if (ordering.OrderingDirection == OrderingDirection.Desc)
            {
                _relationalCommandBuilder.Append(" DESC");
            }
        }

        /// <summary>
        ///     Performs generation over a list of items by visiting each item.
        /// </summary>
        /// <param name="items">The list of items.</param>
        /// <param name="joinAction">An optional join action.</param>
        protected virtual void GenerateList(
            [NotNull] IReadOnlyList<Expression> items,
            [CanBeNull] Action<IRelationalCommandBuilder> joinAction)
            => GenerateList(items, joinAction, typeMappings: null);

        /// <summary>
        ///     Performs generation over a list of items by visiting each item.
        /// </summary>
        /// <param name="items">The list of items.</param>
        /// <param name="joinAction">An optional join action.</param>
        /// <param name="typeMappings">Option type mappings for each item.</param>
        protected virtual void GenerateList(
            [NotNull] IReadOnlyList<Expression> items,
            [CanBeNull] Action<IRelationalCommandBuilder> joinAction = null,
            [CanBeNull] IReadOnlyList<RelationalTypeMapping> typeMappings = null)
            => GenerateList(items, e => Visit(e), joinAction, typeMappings);

        /// <summary>
        ///     Perform generation over a list of items using a provided generation action
        ///     and optional join action.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="items">The list of items.</param>
        /// <param name="generationAction">The generation action.</param>
        /// <param name="joinAction">An optional join action.</param>
        protected virtual void GenerateList<T>(
            [NotNull] IReadOnlyList<T> items,
            [NotNull] Action<T> generationAction,
            [CanBeNull] Action<IRelationalCommandBuilder> joinAction)
            => GenerateList(items, generationAction, joinAction, typeMappings: null);

        /// <summary>
        ///     Perform generation over a list of items using a provided generation action
        ///     and optional join action.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="items">The list of items.</param>
        /// <param name="generationAction">The generation action.</param>
        /// <param name="joinAction">An optional join action.</param>
        /// <param name="typeMappings">Option type mappings for each item.</param>
        protected virtual void GenerateList<T>(
            [NotNull] IReadOnlyList<T> items,
            [NotNull] Action<T> generationAction,
            [CanBeNull] Action<IRelationalCommandBuilder> joinAction = null,
            [CanBeNull] IReadOnlyList<RelationalTypeMapping> typeMappings = null)
        {
            Check.NotNull(items, nameof(items));
            Check.NotNull(generationAction, nameof(generationAction));

            joinAction = joinAction ?? (isb => isb.Append(", "));

            var parentTypeMapping = _typeMapping;

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_relationalCommandBuilder);
                }

                _typeMapping = typeMappings?[i] ?? parentTypeMapping;

                generationAction(items[i]);
            }

            _typeMapping = parentTypeMapping;
        }

        /// <summary>
        ///     Visit a FromSqlExpression.
        /// </summary>
        /// <param name="fromSqlExpression"> The FromSql expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            _relationalCommandBuilder.AppendLine("(");

            using (_relationalCommandBuilder.Indent())
            {
                GenerateFromSql(fromSqlExpression.Sql, fromSqlExpression.Arguments, _parametersValues);
            }

            _relationalCommandBuilder.Append(") AS ")
                .Append(SqlGenerator.DelimitIdentifier(fromSqlExpression.Alias));

            return fromSqlExpression;
        }

        /// <summary>
        ///     Generate SQL corresponding to a FromSql query.
        /// </summary>
        /// <param name="sql"> The FromSql SQL query. </param>
        /// <param name="arguments"> The arguments. </param>
        /// <param name="parameters"> The parameters for this query. </param>
        protected virtual void GenerateFromSql(
            [NotNull] string sql,
            [NotNull] Expression arguments,
            [NotNull] IReadOnlyDictionary<string, object> parameters)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(parameters, nameof(parameters));

            string[] substitutions = null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (arguments)
            {
                case ParameterExpression parameterExpression:
                    if (parameters.TryGetValue(parameterExpression.Name, out var parameterValue))
                    {
                        IsCacheable = false;

                        var argumentValuesFromParameter = (object[])parameterValue;

                        substitutions = new string[argumentValuesFromParameter.Length];

                        _relationalCommandBuilder.AddCompositeParameter(
                            parameterExpression.Name,
                            builder =>
                            {
                                for (var i = 0; i < argumentValuesFromParameter.Length; i++)
                                {
                                    var parameterName = _parameterNameGenerator.GenerateNext();

                                    if (argumentValuesFromParameter[i] is DbParameter dbParameter)
                                    {
                                        if (string.IsNullOrEmpty(dbParameter.ParameterName))
                                        {
                                            dbParameter.ParameterName
                                                = SqlGenerator.GenerateParameterName(parameterName);
                                        }
                                        else
                                        {
                                            parameterName = dbParameter.ParameterName;
                                        }

                                        substitutions[i] = dbParameter.ParameterName;
                                    }
                                    else
                                    {
                                        substitutions[i] = SqlGenerator.GenerateParameterName(parameterName);
                                    }

                                    builder.AddParameter(
                                        parameterName,
                                        substitutions[i]);
                                }
                            });
                    }

                    break;

                case ConstantExpression constantExpression:
                    var argumentValues = (object[])constantExpression.Value;

                    substitutions = new string[argumentValues.Length];

                    for (var i = 0; i < argumentValues.Length; i++)
                    {
                        var value = argumentValues[i];

                        if (value is DbParameter dbParameter)
                        {
                            if (string.IsNullOrEmpty(dbParameter.ParameterName))
                            {
                                dbParameter.ParameterName
                                    = SqlGenerator.GenerateParameterName(_parameterNameGenerator.GenerateNext());
                            }

                            substitutions[i] = dbParameter.ParameterName;

                            _relationalCommandBuilder.AddRawParameter(
                                dbParameter.ParameterName,
                                dbParameter);
                        }
                        else
                        {
                            substitutions[i] = GenerateSqlLiteral(value);
                        }
                    }

                    break;

                case NewArrayExpression newArrayExpression
                when newArrayExpression.NodeType == ExpressionType.NewArrayInit:
                    substitutions = new string[newArrayExpression.Expressions.Count];

                    for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
                    {
                        // ReSharper disable once SwitchStatementMissingSomeCases
                        switch (newArrayExpression.Expressions[i].RemoveConvert())
                        {
                            case ConstantExpression constant:
                                var value = constant.Value;
                                substitutions[i] = GenerateSqlLiteral(value);

                                break;

                            case ParameterExpression parameter:
                                if (_parametersValues.ContainsKey(parameter.Name))
                                {
                                    substitutions[i] = SqlGenerator.GenerateParameterName(parameter.Name);

                                    _relationalCommandBuilder.AddParameter(
                                        parameter.Name,
                                        substitutions[i]);
                                }

                                break;
                        }
                    }

                    break;
            }

            if (substitutions != null)
            {
                // ReSharper disable once CoVariantArrayConversion
                // InvariantCulture not needed since substitutions are all strings
                sql = string.Format(sql, substitutions);
            }

            _relationalCommandBuilder.AppendLines(sql);
        }

        private string GenerateSqlLiteral(object value)
        {
            var mapping = _typeMapping;
            var mappingClrType = mapping?.ClrType.UnwrapNullableType();

            if (mappingClrType != null
                && (value == null
                    || mappingClrType.IsInstanceOfType(value)
                    || value.GetType().IsInteger()
                    && (mappingClrType.IsInteger()
                        || mappingClrType.IsEnum)))
            {
                if (value?.GetType().IsInteger() == true
                    && mappingClrType.IsEnum)
                {
                    value = Enum.ToObject(mappingClrType, value);
                }
            }
            else
            {
                mapping = Dependencies.TypeMappingSource.GetMappingForValue(value);
            }

            LogValueConversionWarning(mapping);

            return mapping.GenerateSqlLiteral(value);
        }

        /// <summary>
        ///     Visit a TableExpression.
        /// </summary>
        /// <param name="tableExpression"> The table expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitTable(TableExpression tableExpression)
        {
            Check.NotNull(tableExpression, nameof(tableExpression));

            _relationalCommandBuilder
                .Append(SqlGenerator.DelimitIdentifier(tableExpression.Table, tableExpression.Schema))
                .Append(AliasSeparator)
                .Append(SqlGenerator.DelimitIdentifier(tableExpression.Alias));

            return tableExpression;
        }

        /// <summary>
        ///     Visit a CrossJoin expression.
        /// </summary>
        /// <param name="crossJoinExpression"> The cross join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitCrossJoin(CrossJoinExpression crossJoinExpression)
        {
            Check.NotNull(crossJoinExpression, nameof(crossJoinExpression));

            _relationalCommandBuilder.Append("CROSS JOIN ");

            Visit(crossJoinExpression.TableExpression);

            return crossJoinExpression;
        }

        /// <summary>
        ///     Visit a CrossJoinLateralExpression expression.
        /// </summary>
        /// <param name="crossJoinLateralExpression"> The cross join lateral expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitCrossJoinLateral(CrossJoinLateralExpression crossJoinLateralExpression)
        {
            Check.NotNull(crossJoinLateralExpression, nameof(crossJoinLateralExpression));

            _relationalCommandBuilder.Append("CROSS JOIN LATERAL ");

            Visit(crossJoinLateralExpression.TableExpression);

            return crossJoinLateralExpression;
        }

        /// <summary>
        ///     Visit a SqlFragmentExpression.
        /// </summary>
        /// <param name="sqlFragmentExpression"> The SqlFragmentExpression expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitSqlFragment(SqlFragmentExpression sqlFragmentExpression)
        {
            Check.NotNull(sqlFragmentExpression, nameof(sqlFragmentExpression));

            _relationalCommandBuilder.Append(sqlFragmentExpression.Sql);

            return sqlFragmentExpression;
        }

        /// <summary>
        ///     Visit a StringCompareExpression.
        /// </summary>
        /// <param name="stringCompareExpression"> The string compare expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitStringCompare(StringCompareExpression stringCompareExpression)
        {
            Visit(stringCompareExpression.Left);

            _relationalCommandBuilder.Append(GenerateOperator(stringCompareExpression));

            Visit(stringCompareExpression.Right);

            return stringCompareExpression;
        }

        /// <summary>
        ///     Visit an InExpression.
        /// </summary>
        /// <param name="inExpression"> The in expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitIn(InExpression inExpression)
        {
            var oldValueConverterWarningsEnabled = _valueConverterWarningsEnabled;

            _valueConverterWarningsEnabled = false;

            GenerateIn(inExpression);

            _valueConverterWarningsEnabled = oldValueConverterWarningsEnabled;

            return inExpression;
        }

        /// <summary>
        ///     Visit a negated InExpression.
        /// </summary>
        /// <param name="inExpression"> The in expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        [Obsolete("Override GenerateIn method instead.")]
        protected virtual Expression GenerateNotIn([NotNull] InExpression inExpression)
        {
            GenerateIn(inExpression, negated: true);

            return inExpression;
        }

        /// <summary>
        ///     Generates SQL for an InExpression.
        /// </summary>
        /// <param name="inExpression"> The in expression. </param>
        /// <param name="negated"> Whether the InExpression is negated or not. </param>
        protected virtual void GenerateIn([NotNull] InExpression inExpression, bool negated = false)
        {
            var parentTypeMapping = _typeMapping;
            _typeMapping = InferTypeMappingFromColumn(inExpression.Operand) ?? parentTypeMapping;

            Visit(inExpression.Operand);

            _relationalCommandBuilder.Append(negated ? " NOT IN " : " IN ");

            if (inExpression.Values != null)
            {
                _relationalCommandBuilder.Append("(");

                GenerateList(inExpression.Values);

                _relationalCommandBuilder.Append(")");
            }
            else
            {
                Visit(inExpression.SubQuery);
            }

            _typeMapping = parentTypeMapping;
        }

        /// <summary>
        ///     Process the InExpression values.
        /// </summary>
        /// <param name="inExpressionValues"> The in expression values. </param>
        /// <returns>
        ///     A list of expressions.
        /// </returns>
        [Obsolete("If you need to override this method then raise an issue at https://github.com/aspnet/EntityFrameworkCore")]
        protected virtual IReadOnlyList<Expression> ProcessInExpressionValues(
            [NotNull] IEnumerable<Expression> inExpressionValues)
        {
            Check.NotNull(inExpressionValues, nameof(inExpressionValues));

            var inConstants = new List<Expression>();

            foreach (var inValue in inExpressionValues)
            {
                switch (inValue)
                {
                    case ConstantExpression inConstant:
                        AddInExpressionValues(inConstant.Value, inConstants, inConstant);
                        break;
                    case ParameterExpression inParameter:
                        if (_parametersValues.TryGetValue(inParameter.Name, out var parameterValue))
                        {
                            AddInExpressionValues(parameterValue, inConstants, inParameter);

                            IsCacheable = false;
                        }

                        break;
                    case ListInitExpression inListInit:
                        inConstants.AddRange(
                            ProcessInExpressionValues(
                                inListInit.Initializers.SelectMany(i => i.Arguments)));
                        break;
                    case NewArrayExpression newArray:
                        inConstants.AddRange(ProcessInExpressionValues(newArray.Expressions));
                        break;
                }
            }

            return inConstants;
        }

        private static void AddInExpressionValues(
            object value, List<Expression> inConstants, Expression expression)
        {
            if (value is IEnumerable valuesEnumerable
                && value.GetType() != typeof(string)
                && value.GetType() != typeof(byte[]))
            {
                inConstants.AddRange(valuesEnumerable.Cast<object>().Select(Expression.Constant));
            }
            else
            {
                inConstants.Add(expression);
            }
        }

        /// <summary>
        ///     Extracts the non null expression values from a list of expressions.
        /// </summary>
        /// <param name="inExpressionValues"> The list of expressions. </param>
        /// <returns>
        ///     The extracted non null expression values.
        /// </returns>
        [Obsolete("If you need to override this method then raise an issue at https://github.com/aspnet/EntityFrameworkCore")]
        protected virtual IReadOnlyList<Expression> ExtractNonNullExpressionValues(
            [NotNull] IReadOnlyList<Expression> inExpressionValues)
        {
            var inValuesNotNull = new List<Expression>();

            foreach (var inValue in inExpressionValues)
            {
                var inConstant = inValue as ConstantExpression;

                if (inConstant?.Value != null)
                {
                    inValuesNotNull.Add(inValue);

                    continue;
                }

                if (inValue is ParameterExpression inParameter)
                {
                    if (_parametersValues.TryGetValue(inParameter.Name, out var parameterValue))
                    {
                        if (parameterValue != null)
                        {
                            inValuesNotNull.Add(inValue);
                        }
                    }
                }
            }

            return inValuesNotNull;
        }

        /// <summary>
        ///     Visit an InnerJoinExpression.
        /// </summary>
        /// <param name="innerJoinExpression"> The inner join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitInnerJoin(InnerJoinExpression innerJoinExpression)
        {
            Check.NotNull(innerJoinExpression, nameof(innerJoinExpression));

            _relationalCommandBuilder.Append("INNER JOIN ");

            Visit(innerJoinExpression.TableExpression);

            _relationalCommandBuilder.Append(" ON ");

            Visit(ApplyOptimizations(innerJoinExpression.Predicate, searchCondition: true, joinCondition: true));

            return innerJoinExpression;
        }

        /// <summary>
        ///     Visit an LeftOuterJoinExpression.
        /// </summary>
        /// <param name="leftOuterJoinExpression"> The left outer join expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitLeftOuterJoin(LeftOuterJoinExpression leftOuterJoinExpression)
        {
            Check.NotNull(leftOuterJoinExpression, nameof(leftOuterJoinExpression));

            _relationalCommandBuilder.Append("LEFT JOIN ");

            Visit(leftOuterJoinExpression.TableExpression);

            _relationalCommandBuilder.Append(" ON ");

            Visit(ApplyOptimizations(leftOuterJoinExpression.Predicate, searchCondition: true, joinCondition: true));

            return leftOuterJoinExpression;
        }

        /// <summary>
        ///     Generates the TOP part of the SELECT statement,
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        protected virtual void GenerateTop([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Limit != null
                && selectExpression.Offset == null)
            {
                _relationalCommandBuilder.Append("TOP(");

                Visit(selectExpression.Limit);

                _relationalCommandBuilder.Append(") ");
            }
        }

        /// <summary>
        ///     Generates the LIMIT OFFSET part of the SELECT statement,
        /// </summary>
        /// <param name="selectExpression"> The select expression. </param>
        protected virtual void GenerateLimitOffset([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            if (selectExpression.Offset != null)
            {
                _relationalCommandBuilder.AppendLine()
                    .Append("OFFSET ");

                Visit(selectExpression.Offset);

                _relationalCommandBuilder.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    _relationalCommandBuilder.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    _relationalCommandBuilder.Append(" ROWS ONLY");
                }
            }
        }

        /// <summary>
        ///     Visit a ConditionalExpression.
        /// </summary>
        /// <param name="conditionalExpression"> The conditional expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Check.NotNull(conditionalExpression, nameof(conditionalExpression));

            _relationalCommandBuilder.AppendLine("CASE");

            using (_relationalCommandBuilder.Indent())
            {
                _relationalCommandBuilder.Append("WHEN ");

                var oldValueConverterWarningsEnabled = _valueConverterWarningsEnabled;

                _valueConverterWarningsEnabled = false;

                Visit(conditionalExpression.Test);

                _valueConverterWarningsEnabled = oldValueConverterWarningsEnabled;

                _relationalCommandBuilder.AppendLine();
                _relationalCommandBuilder.Append("THEN ");

                if (conditionalExpression.IfTrue.RemoveConvert() is ConstantExpression constantIfTrue
                    && constantIfTrue.Value != null
                    && constantIfTrue.Type.UnwrapNullableType() == typeof(bool))
                {
                    _relationalCommandBuilder
                        .Append((bool)constantIfTrue.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(conditionalExpression.IfTrue);
                }

                _relationalCommandBuilder.Append(" ELSE ");

                if (conditionalExpression.IfFalse.RemoveConvert() is ConstantExpression constantIfFalse
                    && constantIfFalse.Value != null
                    && constantIfFalse.Type.UnwrapNullableType() == typeof(bool))
                {
                    _relationalCommandBuilder
                        .Append((bool)constantIfFalse.Value ? TypedTrueLiteral : TypedFalseLiteral);
                }
                else
                {
                    Visit(conditionalExpression.IfFalse);
                }

                _relationalCommandBuilder.AppendLine();
            }

            _relationalCommandBuilder.Append("END");

            return conditionalExpression;
        }

        /// <summary>
        ///     Visit an ExistsExpression.
        /// </summary>
        /// <param name="existsExpression"> The exists expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitExists(ExistsExpression existsExpression)
        {
            Check.NotNull(existsExpression, nameof(existsExpression));

            _relationalCommandBuilder.AppendLine("EXISTS (");

            using (_relationalCommandBuilder.Indent())
            {
                Visit(existsExpression.Subquery);
            }

            _relationalCommandBuilder.Append(")");

            return existsExpression;
        }

        /// <summary>
        ///     Visit a BinaryExpression.
        /// </summary>
        /// <param name="binaryExpression"> The binary expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Check.NotNull(binaryExpression, nameof(binaryExpression));

            var oldValueConverterWarningsEnabled = _valueConverterWarningsEnabled;

            _valueConverterWarningsEnabled
                = _valueConverterWarningsEnabled
                  && binaryExpression.NodeType != ExpressionType.Equal
                  && binaryExpression.NodeType != ExpressionType.NotEqual;

            var parentTypeMapping = _typeMapping;

            if (binaryExpression.IsComparisonOperation()
                || binaryExpression.NodeType == ExpressionType.Add
                || binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                _typeMapping
                    = InferTypeMappingFromColumn(binaryExpression.Left)
                      ?? InferTypeMappingFromColumn(binaryExpression.Right)
                      ?? parentTypeMapping;
            }

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Coalesce:
                    _relationalCommandBuilder.Append("COALESCE(");
                    Visit(binaryExpression.Left);
                    _relationalCommandBuilder.Append(", ");
                    Visit(binaryExpression.Right);
                    _relationalCommandBuilder.Append(")");

                    break;

                default:
                    var needParens = binaryExpression.Left.RemoveConvert() is BinaryExpression leftBinaryExpression
                                     && leftBinaryExpression.NodeType != ExpressionType.Coalesce;

                    if (needParens)
                    {
                        _relationalCommandBuilder.Append("(");
                    }

                    Visit(binaryExpression.Left);

                    if (needParens)
                    {
                        _relationalCommandBuilder.Append(")");
                    }

                    _relationalCommandBuilder.Append(GenerateOperator(binaryExpression));

                    needParens = binaryExpression.Right.RemoveConvert() is BinaryExpression rightBinaryExpression
                                 && rightBinaryExpression.NodeType != ExpressionType.Coalesce;

                    if (needParens)
                    {
                        _relationalCommandBuilder.Append("(");
                    }

                    Visit(binaryExpression.Right);

                    if (needParens)
                    {
                        _relationalCommandBuilder.Append(")");
                    }

                    break;
            }

            _typeMapping = parentTypeMapping;
            _valueConverterWarningsEnabled = oldValueConverterWarningsEnabled;

            return binaryExpression;
        }

        /// <summary>
        ///     Visits a ColumnExpression.
        /// </summary>
        /// <param name="columnExpression"> The column expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitColumn(ColumnExpression columnExpression)
        {
            Check.NotNull(columnExpression, nameof(columnExpression));

            _relationalCommandBuilder.Append(SqlGenerator.DelimitIdentifier(columnExpression.Table.Alias))
                .Append(".")
                .Append(SqlGenerator.DelimitIdentifier(columnExpression.Name));

            return columnExpression;
        }

        /// <summary>
        ///     Visits a ColumnReferenceExpression.
        /// </summary>
        /// <param name="columnReferenceExpression"> The column reference expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitColumnReference(ColumnReferenceExpression columnReferenceExpression)
        {
            Check.NotNull(columnReferenceExpression, nameof(columnReferenceExpression));

            _relationalCommandBuilder.Append(SqlGenerator.DelimitIdentifier(columnReferenceExpression.Table.Alias))
                .Append(".")
                .Append(SqlGenerator.DelimitIdentifier(columnReferenceExpression.Name));

            return columnReferenceExpression;
        }

        /// <summary>
        ///     Visits an AliasExpression.
        /// </summary>
        /// <param name="aliasExpression"> The alias expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitAlias(AliasExpression aliasExpression)
        {
            Check.NotNull(aliasExpression, nameof(aliasExpression));

            Visit(aliasExpression.Expression);

            if (aliasExpression.Alias != GetColumnName(aliasExpression.Expression))
            {
                _relationalCommandBuilder.Append(AliasSeparator);
                _relationalCommandBuilder.Append(SqlGenerator.DelimitIdentifier(aliasExpression.Alias));
            }

            return aliasExpression;
        }

        private static string GetColumnName(Expression expression)
        {
            expression = expression.RemoveConvert();
            expression = expression.UnwrapNullableExpression().RemoveConvert();

            return (expression as AliasExpression)?.Alias
                   ?? (expression as ColumnExpression)?.Name
                   ?? (expression as ColumnReferenceExpression)?.Name;
        }

        /// <summary>
        ///     Visits an IsNullExpression.
        /// </summary>
        /// <param name="isNullExpression"> The is null expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitIsNull(IsNullExpression isNullExpression)
        {
            Check.NotNull(isNullExpression, nameof(isNullExpression));

            Visit(isNullExpression.Operand);

            _relationalCommandBuilder.Append(" IS NULL");

            return isNullExpression;
        }

        /// <summary>
        ///     Visits an IsNotNullExpression.
        /// </summary>
        /// <param name="isNotNullExpression"> The is not null expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected virtual Expression GenerateIsNotNull([NotNull] IsNullExpression isNotNullExpression)
        {
            Check.NotNull(isNotNullExpression, nameof(isNotNullExpression));

            Visit(isNotNullExpression.Operand);

            _relationalCommandBuilder.Append(" IS NOT NULL");

            return isNotNullExpression;
        }

        /// <summary>
        ///     Visit a LikeExpression.
        /// </summary>
        /// <param name="likeExpression"> The like expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitLike(LikeExpression likeExpression)
        {
            Check.NotNull(likeExpression, nameof(likeExpression));

            var parentTypeMapping = _typeMapping;
            _typeMapping = InferTypeMappingFromColumn(likeExpression.Match) ?? parentTypeMapping;

            Visit(likeExpression.Match);

            _relationalCommandBuilder.Append(" LIKE ");

            Visit(likeExpression.Pattern);

            if (likeExpression.EscapeChar != null)
            {
                _relationalCommandBuilder.Append(" ESCAPE ");
                Visit(likeExpression.EscapeChar);
            }

            _typeMapping = parentTypeMapping;

            return likeExpression;
        }

        /// <summary>
        ///     Visits a SqlFunctionExpression.
        /// </summary>
        /// <param name="sqlFunctionExpression"> The SQL function expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            if (!string.IsNullOrWhiteSpace(sqlFunctionExpression.Schema))
            {
                _relationalCommandBuilder
                    .Append(SqlGenerator.DelimitIdentifier(sqlFunctionExpression.Schema))
                    .Append(".")
                    .Append(SqlGenerator.DelimitIdentifier(sqlFunctionExpression.FunctionName));
            }
            else
            {
                if (sqlFunctionExpression.Instance != null)
                {
                    var parentTypeMapping = _typeMapping;
                    _typeMapping = sqlFunctionExpression.InstanceTypeMapping ?? parentTypeMapping;

                    Visit(sqlFunctionExpression.Instance);

                    _typeMapping = parentTypeMapping;

                    _relationalCommandBuilder.Append(".");
                }

                _relationalCommandBuilder.Append(sqlFunctionExpression.FunctionName);
            }

            if (!sqlFunctionExpression.IsNiladic)
            {
                _relationalCommandBuilder.Append("(");

                var parentTypeMapping = _typeMapping;
                _typeMapping = null;

                GenerateList(sqlFunctionExpression.Arguments, typeMappings: sqlFunctionExpression.ArgumentTypeMappings);

                _typeMapping = parentTypeMapping;

                _relationalCommandBuilder.Append(")");
            }

            return sqlFunctionExpression;
        }

        /// <summary>
        ///     Generates a SQL function call.
        /// </summary>
        /// <param name="functionName">The function name</param>
        /// <param name="arguments">The function arguments</param>
        /// <param name="schema">The function schema</param>
        [Obsolete("Override VisitSqlFunction method instead.")]
        protected virtual void GenerateFunctionCall(
            [NotNull] string functionName,
            [NotNull] IReadOnlyList<Expression> arguments,
            [CanBeNull] string schema = null)
        {
            Check.NotEmpty(functionName, nameof(functionName));
            Check.NotNull(arguments, nameof(arguments));

            if (!string.IsNullOrWhiteSpace(schema))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                _relationalCommandBuilder.Append(SqlGenerator.DelimitIdentifier(schema))
                    .Append(".");
            }

            var parentTypeMapping = _typeMapping;
            _typeMapping = null;

            _relationalCommandBuilder.Append(functionName);
            _relationalCommandBuilder.Append("(");

            GenerateList(arguments);

            _relationalCommandBuilder.Append(")");

            _typeMapping = parentTypeMapping;
        }

        /// <summary>
        ///     Visit a SQL ExplicitCastExpression.
        /// </summary>
        /// <param name="explicitCastExpression"> The explicit cast expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitExplicitCast(ExplicitCastExpression explicitCastExpression)
        {
            _relationalCommandBuilder.Append("CAST(");

            var parentTypeMapping = _typeMapping;

            _typeMapping = InferTypeMappingFromColumn(explicitCastExpression.Operand);

            Visit(explicitCastExpression.Operand);

            _relationalCommandBuilder.Append(" AS ");

            var typeMapping = Dependencies.TypeMappingSource.FindMapping(explicitCastExpression.Type);

            if (typeMapping == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.UnsupportedType(explicitCastExpression.Type.ShortDisplayName()));
            }

            LogValueConversionWarning(typeMapping);

            _relationalCommandBuilder.Append(typeMapping.StoreType);

            _relationalCommandBuilder.Append(")");

            _typeMapping = parentTypeMapping;

            return explicitCastExpression;
        }

        /// <summary>
        ///     Visits a UnaryExpression.
        /// </summary>
        /// <param name="expression"> The unary expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            switch (expression.NodeType)
            {
                case ExpressionType.Not:
                    if (expression.Operand is InExpression inExpression)
                    {
                        GenerateIn(inExpression, negated: true);

                        return inExpression;
                    }

                    if (expression.Operand is IsNullExpression isNullExpression)
                    {
                        return GenerateIsNotNull(isNullExpression);
                    }

                    if (expression.Operand is ExistsExpression)
                    {
                        _relationalCommandBuilder.Append("NOT ");
                        Visit(expression.Operand);

                        return expression;
                    }

                    _relationalCommandBuilder.Append("NOT (");
                    Visit(expression.Operand);
                    _relationalCommandBuilder.Append(")");

                    return expression;

                case ExpressionType.Convert:
                    Visit(expression.Operand);

                    return expression;

                case ExpressionType.Negate:
                    _relationalCommandBuilder.Append("-");
                    Visit(expression.Operand);

                    return expression;
            }

            return base.VisitUnary(expression);
        }

        /// <summary>
        ///     Visits a ConstantExpression.
        /// </summary>
        /// <param name="constantExpression"> The constant expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            Check.NotNull(constantExpression, nameof(constantExpression));

            _relationalCommandBuilder.Append(GenerateSqlLiteral(constantExpression.Value));

            return constantExpression;
        }

        /// <summary>
        ///     Visits a ParameterExpression.
        /// </summary>
        /// <param name="parameterExpression"> The parameter expression to visit. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            Check.NotNull(parameterExpression, nameof(parameterExpression));

            var parameterName = SqlGenerator.GenerateParameterName(parameterExpression.Name);

            if (_relationalCommandBuilder.ParameterBuilder.Parameters
                .All(p => p.InvariantName != parameterExpression.Name))
            {
                var parameterType = parameterExpression.Type.UnwrapNullableType();

                var typeMapping = _typeMapping;

                if (typeMapping == null
                    || (!typeMapping.ClrType.UnwrapNullableType().IsAssignableFrom(parameterType)
                        && (parameterType.IsEnum
                            || !typeof(IConvertible).IsAssignableFrom(parameterType))))
                {
                    typeMapping = Dependencies.TypeMappingSource.GetMapping(parameterType);
                }

                LogValueConversionWarning(typeMapping);

                _relationalCommandBuilder.AddParameter(
                    parameterExpression.Name,
                    parameterName,
                    typeMapping,
                    parameterExpression.Type.IsNullableType());
            }

            var parameterNamePlaceholder = SqlGenerator.GenerateParameterNamePlaceholder(parameterExpression.Name);

            _relationalCommandBuilder.Append(parameterNamePlaceholder);

            return parameterExpression;
        }

        private void LogValueConversionWarning(CoreTypeMapping typeMapping)
        {
            if (_valueConverterWarningsEnabled
                && typeMapping.Converter != null)
            {
                Dependencies.Logger.ValueConversionSqlLiteralWarning(typeMapping.ClrType, typeMapping.Converter);
            }
        }

        /// <summary>
        ///     Visits a PropertyParameterExpression.
        /// </summary>
        /// <param name="propertyParameterExpression"> The property parameter expression. </param>
        /// <returns>
        ///     An Expression.
        /// </returns>
        public virtual Expression VisitPropertyParameter(PropertyParameterExpression propertyParameterExpression)
        {
            var parameterName
                = SqlGenerator.GenerateParameterName(
                    propertyParameterExpression.PropertyParameterName);

            if (_relationalCommandBuilder.ParameterBuilder.Parameters
                .All(p => p.InvariantName != propertyParameterExpression.PropertyParameterName))
            {
                _relationalCommandBuilder.AddPropertyParameter(
                    propertyParameterExpression.Name,
                    parameterName,
                    propertyParameterExpression.Property);
            }

            var parameterNamePlaceholder
                = SqlGenerator.GenerateParameterNamePlaceholder(
                    propertyParameterExpression.PropertyParameterName);

            _relationalCommandBuilder.Append(parameterNamePlaceholder);

            return propertyParameterExpression;
        }

        /// <summary>
        ///     Visits a case expression.
        /// </summary>
        /// <param name="caseExpression"> The case expression. </param>
        /// <returns> An expression. </returns>
        public virtual Expression VisitCase(CaseExpression caseExpression)
        {
            Check.NotNull(caseExpression, nameof(caseExpression));

            _relationalCommandBuilder.Append("CASE");

            if (caseExpression.Operand != null)
            {
                _relationalCommandBuilder.Append(" ");
                Visit(caseExpression.Operand);
            }

            using (_relationalCommandBuilder.Indent())
            {
                foreach (var whenClause in caseExpression.WhenClauses)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("WHEN ");
                    Visit(whenClause.Test);
                    _relationalCommandBuilder.Append(" THEN ");
                    Visit(whenClause.Result);
                }

                if (caseExpression.ElseResult != null)
                {
                    _relationalCommandBuilder
                        .AppendLine()
                        .Append("ELSE ");
                    Visit(caseExpression.ElseResult);
                }
            }

            _relationalCommandBuilder
                .AppendLine()
                .Append("END");

            return caseExpression;
        }

        /// <summary>
        ///     Infers a type mapping from a column expression.
        /// </summary>
        /// <param name="expression"> The expression to infer a type mapping for. </param>
        /// <returns>
        ///     A RelationalTypeMapping.
        /// </returns>
        protected virtual RelationalTypeMapping InferTypeMappingFromColumn([NotNull] Expression expression)
            => expression.FindProperty(expression.Type)?.FindRelationalMapping();

        /// <summary>
        ///     Attempts to generate binary operator for a given expression type.
        /// </summary>
        /// <param name="op"> The operation. </param>
        /// <param name="result"> [out] The SQL binary operator. </param>
        /// <returns>
        ///     true if it succeeds, false if it fails.
        /// </returns>
        [Obsolete("Override GenerateOperator method instead.")]
        protected virtual bool TryGenerateBinaryOperator(ExpressionType op, [NotNull] out string result)
            => _operatorMap.TryGetValue(op, out result);

        /// <summary>
        ///     Generates SQL for a given binary operation type.
        /// </summary>
        /// <param name="op"> The operation. </param>
        /// <returns>
        ///     The binary operator.
        /// </returns>
        [Obsolete("Override GenerateOperator method instead.")]
        protected virtual string GenerateBinaryOperator(ExpressionType op) => _operatorMap[op];

        /// <summary>
        ///     Generates an SQL operator for a given expression.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <returns>
        ///     The operator.
        /// </returns>
        protected virtual string GenerateOperator([NotNull] Expression expression)
        {
            switch (expression)
            {
                case StringCompareExpression stringCompareExpression:
                    return _operatorMap[stringCompareExpression.Operator];

                default:
                    return _operatorMap[expression.NodeType];
            }
        }

        /// <summary>
        ///     Creates unhandled item exception.
        /// </summary>
        /// <typeparam name="T"> Generic type parameter. </typeparam>
        /// <param name="unhandledItem"> The unhandled item. </param>
        /// <param name="visitMethod"> The visit method. </param>
        /// <returns>
        ///     The new unhandled item exception.
        /// </returns>
        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            => new NotImplementedException(visitMethod);

        private class NullComparisonTransformingVisitor : RelinqExpressionVisitor
        {
            private readonly IReadOnlyDictionary<string, object> _parameterValues;

            public NullComparisonTransformingVisitor(IReadOnlyDictionary<string, object> parameterValues)
                => _parameterValues = parameterValues;

            protected override Expression VisitBinary(BinaryExpression expression)
            {
                if (expression.NodeType == ExpressionType.Equal
                    || expression.NodeType == ExpressionType.NotEqual)
                {
                    var leftExpression = expression.Left.RemoveConvert();
                    var rightExpression = expression.Right.RemoveConvert();

                    var parameterExpression = leftExpression as ParameterExpression
                                              ?? rightExpression as ParameterExpression;

                    if (parameterExpression != null
                        && _parameterValues.TryGetValue(parameterExpression.Name, out var parameterValue))
                    {
                        var nonParameterExpression
                            = leftExpression is ParameterExpression
                                ? rightExpression
                                : leftExpression;

                        if (nonParameterExpression is ConstantExpression constantExpression)
                        {
                            if (parameterValue == null
                                && constantExpression.Value == null)
                            {
                                return
                                    expression.NodeType == ExpressionType.Equal
                                        ? Expression.Constant(true)
                                        : Expression.Constant(false);
                            }

                            if ((parameterValue == null && constantExpression.Value != null)
                                || (parameterValue != null && constantExpression.Value == null))
                            {
                                return
                                    expression.NodeType == ExpressionType.Equal
                                        ? Expression.Constant(false)
                                        : Expression.Constant(true);
                            }
                        }

                        if (parameterValue == null)
                        {
                            return
                                expression.NodeType == ExpressionType.Equal
                                    ? (Expression)new IsNullExpression(nonParameterExpression)
                                    : Expression.Not(new IsNullExpression(nonParameterExpression));
                        }
                    }
                }

                return base.VisitBinary(expression);
            }

            protected override Expression VisitExtension(Expression node)
                => node is NullCompensatedExpression
                    ? node
                    : base.VisitExtension(node);
        }

        private class BooleanExpressionTranslatingVisitor : RelinqExpressionVisitor
        {
            private bool _isSearchCondition;

            /// <summary>
            ///     Translates given expression to either boolean condition or value
            /// </summary>
            /// <param name="expression">The expression to translate</param>
            /// <param name="searchCondition">Specifies if the returned value should be condition or value</param>
            /// <returns>The translated expression</returns>
            /// General flow of overridden methods
            /// 1. Inspect expression type and set _isSearchCondition flag
            /// 2. Visit the children
            /// 3. Restore _isSearchCondition
            /// 4. Update the expression
            /// 5. Convert to value/search condition as per _isSearchConditionFlag
            public Expression Translate(Expression expression, bool searchCondition)
            {
                _isSearchCondition = searchCondition;

                return Visit(expression);
            }

            protected override Expression VisitBinary(BinaryExpression binaryExpression)
            {
                var parentIsSearchCondition = _isSearchCondition;

                switch (binaryExpression.NodeType)
                {
                    // Only logical operations need conditions on both sides
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        _isSearchCondition = true;
                        break;
                    default:
                        _isSearchCondition = false;
                        break;
                }

                var newLeft = Visit(binaryExpression.Left);
                var newRight = Visit(binaryExpression.Right);

                _isSearchCondition = parentIsSearchCondition;

                binaryExpression = binaryExpression.Update(newLeft, binaryExpression.Conversion, newRight);

                return ApplyConversion(binaryExpression);
            }

            protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
            {
                var parentIsSearchCondition = _isSearchCondition;

                // Test is always a condition
                _isSearchCondition = true;
                var test = Visit(conditionalExpression.Test);
                // Results are always values
                _isSearchCondition = false;
                var ifTrue = Visit(conditionalExpression.IfTrue);
                var ifFalse = Visit(conditionalExpression.IfFalse);

                _isSearchCondition = parentIsSearchCondition;

                conditionalExpression = conditionalExpression.Update(test, ifTrue, ifFalse);

                return ApplyConversion(conditionalExpression);
            }

            protected override Expression VisitConstant(ConstantExpression constantExpression)
                => ApplyConversion(constantExpression);

            protected override Expression VisitUnary(UnaryExpression unaryExpression)
            {
                // Special optimization
                // NOT(A) => A == false
                if (unaryExpression.NodeType == ExpressionType.Not
                    && unaryExpression.Operand.IsSimpleExpression())
                {
                    return Visit(BuildCompareToExpression(unaryExpression.Operand, compareTo: false));
                }

                var parentIsSearchCondition = _isSearchCondition;

                switch (unaryExpression.NodeType)
                {
                    // For convert preserve the flag since they are transparent to SQL
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                        break;

                    // For Not operand must be search condition
                    case ExpressionType.Not:
                        _isSearchCondition = true;
                        break;

                    // For rest, operand must be value
                    default:
                        _isSearchCondition = false;
                        break;
                }

                var operand = Visit(unaryExpression.Operand);

                _isSearchCondition = parentIsSearchCondition;

                unaryExpression = unaryExpression.Update(operand);

                // Convert nodes are transparent to SQL hence no conversion needed
                return unaryExpression.NodeType == ExpressionType.Convert
                    || unaryExpression.NodeType == ExpressionType.ConvertChecked
                    ? unaryExpression
                    : ApplyConversion(unaryExpression);
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                var parentIsSearchCondition = _isSearchCondition;

                Expression newExpression;
                switch (extensionExpression)
                {
                    case SelectExpression selectExpression:
                        // We skip visiting SelectExpression here because it will be processed by outer visitor when
                        // generating SQL
                        newExpression = selectExpression;
                        break;

                    case CaseExpression caseExpression:
                        _isSearchCondition = false;
                        var newOperand = Visit(caseExpression.Operand);

                        var whenThenListChanged = false;
                        var newWhenThenList = new List<CaseWhenClause>();
                        foreach (var whenClause in caseExpression.WhenClauses)
                        {
                            _isSearchCondition = caseExpression.Operand == null;
                            var newTest = Visit(whenClause.Test);

                            _isSearchCondition = false;
                            var newResult = Visit(whenClause.Result);
                            var newWhenThen = newTest != whenClause.Test || newResult != whenClause.Result
                                ? new CaseWhenClause(newTest, newResult)
                                : whenClause;

                            newWhenThenList.Add(newWhenThen);
                            whenThenListChanged |= newWhenThen != whenClause;
                        }

                        _isSearchCondition = false;
                        var newElseResult = Visit(caseExpression.ElseResult);

                        newExpression = newOperand != caseExpression.Operand
                                || whenThenListChanged
                                || newElseResult != caseExpression.ElseResult
                            ? new CaseExpression(newOperand, newWhenThenList, newElseResult)
                            : caseExpression;
                        break;

                    default:
                        // All other Extension expressions have value type children
                        _isSearchCondition = false;
                        newExpression = base.VisitExtension(extensionExpression);
                        break;
                }

                _isSearchCondition = parentIsSearchCondition;

                return ApplyConversion(newExpression);
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
                => ApplyConversion(parameterExpression);

            private Expression ApplyConversion(Expression expression)
                => _isSearchCondition
                    ? ConvertToSearchCondition(expression)
                    : ConvertToValue(expression);

            private static bool IsSearchCondition(Expression expression)
            {
                expression = expression.RemoveConvert();

                return !(expression is BinaryExpression)
                    && expression.NodeType != ExpressionType.Not
                    && expression.NodeType != ExpressionType.Extension
                    ? false
                    : expression.IsComparisonOperation()
                       || expression.IsLogicalOperation()
                       || expression.NodeType == ExpressionType.Not
                       || expression is ExistsExpression
                       || expression is InExpression
                       || expression is IsNullExpression
                       || expression is LikeExpression
                       || expression is StringCompareExpression;
            }

            private static Expression BuildCompareToExpression(Expression expression, bool compareTo)
            {
                var equalExpression = Expression.Equal(
                    expression,
                    Expression.Constant(compareTo, expression.Type));

                // Compensate for type change since Expression.Equal always returns expression of boolean type
                return expression.Type == typeof(bool)
                    ? (Expression)equalExpression
                    : Expression.Convert(equalExpression, expression.Type);
            }

            private static Expression ConvertToSearchCondition(Expression expression)
                => IsSearchCondition(expression)
                    ? expression
                    : BuildCompareToExpression(expression, compareTo: true);

            private static Expression ConvertToValue(Expression expression)
                => IsSearchCondition(expression)
                    ? Expression.Condition(
                        expression.Type == typeof(bool)
                            ? expression
                            : Expression.Convert(expression, typeof(bool)),
                        Expression.Constant(true, expression.Type),
                        Expression.Constant(false, expression.Type))
                    : expression;
        }
    }
}
