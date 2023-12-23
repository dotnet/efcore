// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class that translates queryable methods in a query.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public abstract class QueryableMethodTranslatingExpressionVisitor : ExpressionVisitor
{
    private readonly bool _subquery;
    private readonly EntityShaperNullableMarkingExpressionVisitor _entityShaperNullableMarkingExpressionVisitor;

    /// <summary>
    ///     Creates a new instance of the <see cref="QueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    /// <param name="subquery">A bool value indicating whether it is for a subquery translation.</param>
    protected QueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        bool subquery)
    {
        Dependencies = dependencies;
        QueryCompilationContext = queryCompilationContext;
        _subquery = subquery;
        _entityShaperNullableMarkingExpressionVisitor = new EntityShaperNullableMarkingExpressionVisitor();
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; }

    private Expression? _untranslatedExpression;

    /// <summary>
    ///     Detailed information about errors encountered during translation.
    /// </summary>
    public virtual string? TranslationErrorDetails { get; private set; }

    /// <summary>
    ///     Translates an expression to an equivalent SQL representation.
    /// </summary>
    /// <param name="expression">An expression to translate.</param>
    /// <returns>A SQL translation of the given expression.</returns>
    public virtual Expression Translate(Expression expression)
    {
        var translated = Visit(expression);

        // Note that we only throw if a specific node is recognized as untranslatable; we need to otherwise not throw in order to allow
        // for client evaluation.
        if (translated == QueryCompilationContext.NotTranslatedExpression && _untranslatedExpression is not null)
        {
            if (_untranslatedExpression is QueryRootExpression)
            {
                throw new InvalidOperationException(
                    TranslationErrorDetails is null
                        ? CoreStrings.QueryUnhandledQueryRootExpression(_untranslatedExpression.GetType().ShortDisplayName())
                        : CoreStrings.TranslationFailedWithDetails(_untranslatedExpression, TranslationErrorDetails));
            }

            throw new InvalidOperationException(
                TranslationErrorDetails is null
                    ? CoreStrings.TranslationFailed(_untranslatedExpression.Print())
                    : CoreStrings.TranslationFailedWithDetails(_untranslatedExpression.Print(), TranslationErrorDetails));
        }

        return translated;
    }

    /// <summary>
    ///     Adds detailed information about errors encountered during translation.
    /// </summary>
    /// <param name="details">Error encountered during translation.</param>
    protected virtual void AddTranslationErrorDetails(string details)
    {
        if (TranslationErrorDetails == null)
        {
            TranslationErrorDetails = details;
        }
        else
        {
            TranslationErrorDetails += Environment.NewLine + details;
        }
    }

    /// <summary>
    ///     The query compilation context object for current compilation.
    /// </summary>
    protected virtual QueryCompilationContext QueryCompilationContext { get; }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case InlineQueryRootExpression inlineQueryRootExpression:
                return TranslateInlineQueryRoot(inlineQueryRootExpression) ?? base.VisitExtension(extensionExpression);

            case ParameterQueryRootExpression parameterQueryRootExpression:
                return TranslateParameterQueryRoot(parameterQueryRootExpression) ?? base.VisitExtension(extensionExpression);

            case QueryRootExpression queryRootExpression:
                // This requires exact type match on query root to avoid processing query roots derived from EntityQueryRootExpression, e.g.
                // SQL Server TemporalQueryRootExpression.
                if (queryRootExpression.GetType() == typeof(EntityQueryRootExpression))
                {
                    return CreateShapedQueryExpression(((EntityQueryRootExpression)extensionExpression).EntityType);
                }

                _untranslatedExpression = queryRootExpression;
                return QueryCompilationContext.NotTranslatedExpression;

            default:
                return base.VisitExtension(extensionExpression);
        }
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;
        if (method.DeclaringType == typeof(Queryable)
            || method.DeclaringType == typeof(QueryableExtensions))
        {
            var source = Visit(methodCallExpression.Arguments[0]);
            if (source is ShapedQueryExpression shapedQueryExpression)
            {
                var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
                switch (method.Name)
                {
                    case nameof(Queryable.All)
                        when genericMethod == QueryableMethods.All:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateAll(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateAny(shapedQueryExpression, null));

                    case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateAny(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.AsQueryable)
                        when genericMethod == QueryableMethods.AsQueryable:
                        return source;

                    case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithoutSelector(method):
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateAverage(shapedQueryExpression, null, methodCallExpression.Type));

                    case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithSelector(method):
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateAverage(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                    case nameof(Queryable.Cast)
                        when genericMethod == QueryableMethods.Cast:
                        return CheckTranslated(TranslateCast(shapedQueryExpression, method.GetGenericArguments()[0]));

                    case nameof(Queryable.Concat)
                        when genericMethod == QueryableMethods.Concat:
                    {
                        var source2 = Visit(methodCallExpression.Arguments[1]);
                        if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                        {
                            return CheckTranslated(TranslateConcat(shapedQueryExpression, innerShapedQueryExpression));
                        }

                        break;
                    }

                    case nameof(Queryable.Contains)
                        when genericMethod == QueryableMethods.Contains:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateContains(shapedQueryExpression, methodCallExpression.Arguments[1]));

                    case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateCount(shapedQueryExpression, null));

                    case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateCount(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.DefaultIfEmpty)
                        when genericMethod == QueryableMethods.DefaultIfEmptyWithoutArgument:
                        return CheckTranslated(TranslateDefaultIfEmpty(shapedQueryExpression, null));

                    case nameof(Queryable.DefaultIfEmpty)
                        when genericMethod == QueryableMethods.DefaultIfEmptyWithArgument:
                        return CheckTranslated(TranslateDefaultIfEmpty(shapedQueryExpression, methodCallExpression.Arguments[1]));

                    case nameof(Queryable.Distinct)
                        when genericMethod == QueryableMethods.Distinct:
                        return CheckTranslated(TranslateDistinct(shapedQueryExpression));

                    case nameof(Queryable.ElementAt)
                        when genericMethod == QueryableMethods.ElementAt:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], false));

                    case nameof(Queryable.ElementAtOrDefault)
                        when genericMethod == QueryableMethods.ElementAtOrDefault:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(
                            TranslateElementAtOrDefault(shapedQueryExpression, methodCallExpression.Arguments[1], true));

                    case nameof(Queryable.Except)
                        when genericMethod == QueryableMethods.Except:
                    {
                        var source2 = Visit(methodCallExpression.Arguments[1]);
                        if (source2 is ShapedQueryExpression innerShapedQueryExpression)
                        {
                            return CheckTranslated(TranslateExcept(shapedQueryExpression, innerShapedQueryExpression));
                        }

                        break;
                    }

                    case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                    case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateFirstOrDefault(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                    case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(TranslateFirstOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                    case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(
                            TranslateFirstOrDefault(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                    case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeySelector:
                        return CheckTranslated(TranslateGroupBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, null));

                    case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyElementSelector:
                        return CheckTranslated(
                            TranslateGroupBy(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2), null));

                    case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyElementResultSelector:
                        return CheckTranslated(
                            TranslateGroupBy(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2),
                                GetLambdaExpressionFromArgument(3)));

                    case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyResultSelector:
                        return CheckTranslated(
                            TranslateGroupBy(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), null, GetLambdaExpressionFromArgument(2)));

                    case nameof(Queryable.GroupJoin)
                        when genericMethod == QueryableMethods.GroupJoin:
                    {
                        if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                        {
                            return CheckTranslated(
                                TranslateGroupJoin(
                                    shapedQueryExpression,
                                    innerShapedQueryExpression,
                                    GetLambdaExpressionFromArgument(2),
                                    GetLambdaExpressionFromArgument(3),
                                    GetLambdaExpressionFromArgument(4)));
                        }

                        break;
                    }

                    case nameof(Queryable.Intersect)
                        when genericMethod == QueryableMethods.Intersect:
                    {
                        if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                        {
                            return CheckTranslated(TranslateIntersect(shapedQueryExpression, innerShapedQueryExpression));
                        }

                        break;
                    }

                    case nameof(Queryable.Join)
                        when genericMethod == QueryableMethods.Join:
                    {
                        if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                        {
                            return CheckTranslated(
                                TranslateJoin(
                                    shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2),
                                    GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4)));
                        }

                        break;
                    }

                    case nameof(QueryableExtensions.LeftJoin)
                        when genericMethod == QueryableExtensions.LeftJoinMethodInfo:
                    {
                        if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                        {
                            return CheckTranslated(
                                TranslateLeftJoin(
                                    shapedQueryExpression, innerShapedQueryExpression, GetLambdaExpressionFromArgument(2),
                                    GetLambdaExpressionFromArgument(3), GetLambdaExpressionFromArgument(4)));
                        }

                        break;
                    }

                    case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                    case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateLastOrDefault(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                    case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(TranslateLastOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                    case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(
                            TranslateLastOrDefault(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                    case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateLongCount(shapedQueryExpression, null));

                    case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateLongCount(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithoutSelector:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateMax(shapedQueryExpression, null, methodCallExpression.Type));

                    case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithSelector:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateMax(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                    case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithoutSelector:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateMin(shapedQueryExpression, null, methodCallExpression.Type));

                    case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithSelector:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateMin(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                    case nameof(Queryable.OfType)
                        when genericMethod == QueryableMethods.OfType:
                        return CheckTranslated(TranslateOfType(shapedQueryExpression, method.GetGenericArguments()[0]));

                    case nameof(Queryable.OrderBy)
                        when genericMethod == QueryableMethods.OrderBy:
                        return CheckTranslated(TranslateOrderBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), true));

                    case nameof(Queryable.OrderByDescending)
                        when genericMethod == QueryableMethods.OrderByDescending:
                        return CheckTranslated(TranslateOrderBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), false));

                    case nameof(Queryable.Reverse)
                        when genericMethod == QueryableMethods.Reverse:
                        return CheckTranslated(TranslateReverse(shapedQueryExpression));

                    case nameof(Queryable.Select)
                        when genericMethod == QueryableMethods.Select:
                        return CheckTranslated(TranslateSelect(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithoutCollectionSelector:
                        return CheckTranslated(TranslateSelectMany(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithCollectionSelector:
                        return CheckTranslated(
                            TranslateSelectMany(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), GetLambdaExpressionFromArgument(2)));

                    case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, false));

                    case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateSingleOrDefault(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, false));

                    case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithoutPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(TranslateSingleOrDefault(shapedQueryExpression, null, methodCallExpression.Type, true));

                    case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithPredicate:
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.SingleOrDefault);
                        return CheckTranslated(
                            TranslateSingleOrDefault(
                                shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type, true));

                    case nameof(Queryable.Skip)
                        when genericMethod == QueryableMethods.Skip:
                        return CheckTranslated(TranslateSkip(shapedQueryExpression, methodCallExpression.Arguments[1]));

                    case nameof(Queryable.SkipWhile)
                        when genericMethod == QueryableMethods.SkipWhile:
                        return CheckTranslated(TranslateSkipWhile(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithoutSelector(method):
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(TranslateSum(shapedQueryExpression, null, methodCallExpression.Type));

                    case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithSelector(method):
                        shapedQueryExpression = shapedQueryExpression.UpdateResultCardinality(ResultCardinality.Single);
                        return CheckTranslated(
                            TranslateSum(shapedQueryExpression, GetLambdaExpressionFromArgument(1), methodCallExpression.Type));

                    case nameof(Queryable.Take)
                        when genericMethod == QueryableMethods.Take:
                        return CheckTranslated(TranslateTake(shapedQueryExpression, methodCallExpression.Arguments[1]));

                    case nameof(Queryable.TakeWhile)
                        when genericMethod == QueryableMethods.TakeWhile:
                        return CheckTranslated(TranslateTakeWhile(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                    case nameof(Queryable.ThenBy)
                        when genericMethod == QueryableMethods.ThenBy:
                        return CheckTranslated(TranslateThenBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), true));

                    case nameof(Queryable.ThenByDescending)
                        when genericMethod == QueryableMethods.ThenByDescending:
                        return CheckTranslated(TranslateThenBy(shapedQueryExpression, GetLambdaExpressionFromArgument(1), false));

                    case nameof(Queryable.Union)
                        when genericMethod == QueryableMethods.Union:
                    {
                        if (Visit(methodCallExpression.Arguments[1]) is ShapedQueryExpression innerShapedQueryExpression)
                        {
                            return CheckTranslated(TranslateUnion(shapedQueryExpression, innerShapedQueryExpression));
                        }

                        break;
                    }

                    case nameof(Queryable.Where)
                        when genericMethod == QueryableMethods.Where:
                        return CheckTranslated(TranslateWhere(shapedQueryExpression, GetLambdaExpressionFromArgument(1)));

                        LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex)
                            => methodCallExpression.Arguments[argumentIndex].UnwrapLambdaFromQuote();

                        Expression CheckTranslated(ShapedQueryExpression? translated)
                        {
                            if (translated is not null)
                            {
                                return translated;
                            }

                            _untranslatedExpression ??= methodCallExpression;

                            return QueryCompilationContext.NotTranslatedExpression;
                        }
                }
            }
            else if (source == QueryCompilationContext.NotTranslatedExpression)
            {
                return source;
            }
        }

        // The method isn't a LINQ operator on Queryable/QueryableExtensions.

        // Identify property access, e.g. primitive collection property (context.Blogs.Where(b => b.Tags.Contains(...)))
        if ((methodCallExpression.TryGetEFPropertyArguments(out var propertyAccessSource, out var propertyName)
            || methodCallExpression.TryGetIndexerArguments(QueryCompilationContext.Model, out propertyAccessSource, out propertyName))
            && TranslateMemberAccess(propertyAccessSource, MemberIdentity.Create(propertyName)) is ShapedQueryExpression translation)
        {
            return translation;
        }

        return _subquery
            ? QueryCompilationContext.NotTranslatedExpression
            : throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
    }

    private sealed class EntityShaperNullableMarkingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is StructuralTypeShaperExpression shaper
                ? shaper.MakeNullable()
                : base.VisitExtension(extensionExpression);
    }

    /// <summary>
    ///     Marks the entity shaper in the given shaper expression as nullable.
    /// </summary>
    /// <param name="shaperExpression">The shaper expression to process.</param>
    /// <returns>New shaper expression in which all entity shapers are nullable.</returns>
    protected virtual Expression MarkShaperNullable(Expression shaperExpression)
        => _entityShaperNullableMarkingExpressionVisitor.Visit(shaperExpression);

    /// <summary>
    ///     Translates the given subquery.
    /// </summary>
    /// <param name="expression">The subquery expression to translate.</param>
    /// <returns>The translation of the given subquery.</returns>
    public virtual ShapedQueryExpression? TranslateSubquery(Expression expression)
    {
        var subqueryVisitor = CreateSubqueryVisitor();
        var translation = subqueryVisitor.Translate(expression) as ShapedQueryExpression;
        if (translation == null && subqueryVisitor.TranslationErrorDetails != null)
        {
            AddTranslationErrorDetails(subqueryVisitor.TranslationErrorDetails);
        }

        return translation;
    }

    /// <summary>
    ///     Creates a visitor customized to translate a subquery through <see cref="TranslateSubquery(Expression)" />.
    /// </summary>
    /// <returns>A visitor to translate subquery.</returns>
    protected abstract QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor();

    /// <summary>
    ///     Creates a <see cref="ShapedQueryExpression" /> for the given entity type.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>A shaped query expression for the given entity type.</returns>
    protected abstract ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType);

    /// <summary>
    ///     Translates <see cref="Queryable.All{TSource}(IQueryable{TSource}, Expression{Func{TSource,bool}})" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateAll(ShapedQueryExpression source, LambdaExpression predicate);

    /// <summary>
    ///     Translates <see cref="Queryable.Any{TSource}(IQueryable{TSource})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateAny(ShapedQueryExpression source, LambdaExpression? predicate);

    /// <summary>
    ///     Translates <see cref="Queryable.Average(IQueryable{decimal})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="selector">The selector supplied in the call.</param>
    /// <param name="resultType">The result type after the operation.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateAverage(ShapedQueryExpression source, LambdaExpression? selector, Type resultType);

    /// <summary>
    ///     Translates <see cref="Queryable.Cast{TResult}(IQueryable)" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="castType">The type result is being casted to.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateCast(ShapedQueryExpression source, Type castType);

    /// <summary>
    ///     Translates <see cref="Queryable.Concat{TSource}(IQueryable{TSource}, IEnumerable{TSource})" /> method over the given source.
    /// </summary>
    /// <param name="source1">The shaped query on which the operator is applied.</param>
    /// <param name="source2">The other source to perform concat.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateConcat(ShapedQueryExpression source1, ShapedQueryExpression source2);

    /// <summary>
    ///     Translates <see cref="Queryable.Contains{TSource}(IQueryable{TSource}, TSource)" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="item">The item to search for.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item);

    /// <summary>
    ///     Translates <see cref="Queryable.Count{TSource}(IQueryable{TSource})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateCount(ShapedQueryExpression source,        LambdaExpression? predicate);

    /// <summary>
    ///     Translates <see cref="Queryable.DefaultIfEmpty{TSource}(IQueryable{TSource})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="defaultValue">The default value to use.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateDefaultIfEmpty(ShapedQueryExpression source, Expression? defaultValue);

    /// <summary>
    ///     Translates <see cref="Queryable.Distinct{TSource}(IQueryable{TSource})" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateDistinct(ShapedQueryExpression source);

    /// <summary>
    ///     Translates <see cref="Queryable.ElementAt{TSource}(IQueryable{TSource}, int)" /> method or
    ///     <see cref="Queryable.ElementAtOrDefault{TSource}(IQueryable{TSource}, int)" /> over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="index">The index of the element.</param>
    /// <param name="returnDefault">A value indicating whether default should be returned or throw.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault);

    /// <summary>
    ///     Translates <see cref="Queryable.Except{TSource}(IQueryable{TSource}, IEnumerable{TSource})" /> method over the given source.
    /// </summary>
    /// <param name="source1">The shaped query on which the operator is applied.</param>
    /// <param name="source2">The other source to perform except with.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateExcept(ShapedQueryExpression source1, ShapedQueryExpression source2);

    /// <summary>
    ///     Translates <see cref="Queryable.First{TSource}(IQueryable{TSource})" /> method or
    ///     <see cref="Queryable.FirstOrDefault{TSource}(IQueryable{TSource})" /> and their other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <param name="returnType">The return type of result.</param>
    /// <param name="returnDefault">A value indicating whether default should be returned or throw.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateFirstOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault);

    /// <summary>
    ///     Translates <see cref="Queryable.GroupBy{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})" /> method and
    ///     other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="keySelector">The key selector supplied in the call.</param>
    /// <param name="elementSelector">The element selector supplied in the call.</param>
    /// <param name="resultSelector">The result selector supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateGroupBy(
        ShapedQueryExpression source,
        LambdaExpression keySelector,
        LambdaExpression? elementSelector,
        LambdaExpression? resultSelector);

    /// <summary>
    ///     Translates
    ///     <see
    ///         cref="Queryable.GroupJoin{TOuter, TInner, TKey, TResult}(IQueryable{TOuter}, IEnumerable{TInner}, Expression{Func{TOuter, TKey}}, Expression{Func{TInner, TKey}}, Expression{Func{TOuter, IEnumerable{TInner}, TResult}})" />
    ///     method over the given source.
    /// </summary>
    /// <param name="outer">The shaped query on which the operator is applied.</param>
    /// <param name="inner">The inner shaped query to perform join with.</param>
    /// <param name="outerKeySelector">The key selector for the outer source.</param>
    /// <param name="innerKeySelector">The key selector for the inner source.</param>
    /// <param name="resultSelector">The result selector supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateGroupJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector);

    /// <summary>
    ///     Translates <see cref="Queryable.Intersect{TSource}(IQueryable{TSource}, IEnumerable{TSource})" /> method over the given source.
    /// </summary>
    /// <param name="source1">The shaped query on which the operator is applied.</param>
    /// <param name="source2">The other source to perform intersect with.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateIntersect(ShapedQueryExpression source1, ShapedQueryExpression source2);

    /// <summary>
    ///     Translates
    ///     <see
    ///         cref="Queryable.Join{TOuter, TInner, TKey, TResult}(IQueryable{TOuter}, IEnumerable{TInner}, Expression{Func{TOuter, TKey}}, Expression{Func{TInner, TKey}}, Expression{Func{TOuter, TInner, TResult}})" />
    ///     method over the given source.
    /// </summary>
    /// <param name="outer">The shaped query on which the operator is applied.</param>
    /// <param name="inner">The inner shaped query to perform join with.</param>
    /// <param name="outerKeySelector">The key selector for the outer source.</param>
    /// <param name="innerKeySelector">The key selector for the inner source.</param>
    /// <param name="resultSelector">The result selector supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector);

    /// <summary>
    ///     Translates LeftJoin over the given source.
    /// </summary>
    /// <remarks>
    ///     Certain patterns of GroupJoin-DefaultIfEmpty-SelectMany represents a left join in database. We identify such pattern
    ///     in advance and convert it to join like syntax.
    /// </remarks>
    /// <param name="outer">The shaped query on which the operator is applied.</param>
    /// <param name="inner">The inner shaped query to perform join with.</param>
    /// <param name="outerKeySelector">The key selector for the outer source.</param>
    /// <param name="innerKeySelector">The key selector for the inner source.</param>
    /// <param name="resultSelector">The result selector supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateLeftJoin(
        ShapedQueryExpression outer,
        ShapedQueryExpression inner,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector);

    /// <summary>
    ///     Translates <see cref="Queryable.Last{TSource}(IQueryable{TSource})" /> method or
    ///     <see cref="Queryable.LastOrDefault{TSource}(IQueryable{TSource})" /> and their other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <param name="returnType">The return type of result.</param>
    /// <param name="returnDefault">A value indicating whether default should be returned or throw.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateLastOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault);

    /// <summary>
    ///     Translates <see cref="Queryable.LongCount{TSource}(IQueryable{TSource})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source, LambdaExpression? predicate);

    /// <summary>
    ///     Translates <see cref="Queryable.Max{TSource}(IQueryable{TSource})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="selector">The selector supplied in the call.</param>
    /// <param name="resultType">The result type after the operation.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector, Type resultType);

    /// <summary>
    ///     Translates <see cref="Queryable.Min{TSource}(IQueryable{TSource})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="selector">The selector supplied in the call.</param>
    /// <param name="resultType">The result type after the operation.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector, Type resultType);

    /// <summary>
    ///     Translates <see cref="Queryable.OfType{TResult}(IQueryable)" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="resultType">The type of result which is being filtered with.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateOfType(ShapedQueryExpression source, Type resultType);

    /// <summary>
    ///     Translates <see cref="Queryable.OrderBy{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})" /> or
    ///     <see cref="Queryable.OrderByDescending{TSource, TKey}(IQueryable{TSource}, Expression{Func{TSource, TKey}})" /> method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="keySelector">The key selector supplied in the call.</param>
    /// <param name="ascending">A value indicating whether the ordering is ascending or not.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateOrderBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending);

    /// <summary>
    ///     Translates <see cref="Queryable.Reverse{TSource}(IQueryable{TSource})" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateReverse(ShapedQueryExpression source);

    /// <summary>
    ///     Translates <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, TResult}})" /> method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="selector">The selector supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression TranslateSelect(
        ShapedQueryExpression source,
        LambdaExpression selector);

    /// <summary>
    ///     Translates
    ///     <see
    ///         cref="Queryable.SelectMany{TSource, TCollection, TResult}(IQueryable{TSource}, Expression{Func{TSource, IEnumerable{TCollection}}}, Expression{Func{TSource, TCollection, TResult}})" />
    ///     method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="collectionSelector">The collection selector supplied in the call.</param>
    /// <param name="resultSelector">The result selector supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateSelectMany(
        ShapedQueryExpression source,
        LambdaExpression collectionSelector,
        LambdaExpression resultSelector);

    /// <summary>
    ///     Translates <see cref="Queryable.SelectMany{TSource, TResult}(IQueryable{TSource}, Expression{Func{TSource, IEnumerable{TResult}}})" />
    ///     method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="selector">The selector supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateSelectMany(ShapedQueryExpression source, LambdaExpression selector);

    /// <summary>
    ///     Translates <see cref="Queryable.Single{TSource}(IQueryable{TSource})" /> method or
    ///     <see cref="Queryable.SingleOrDefault{TSource}(IQueryable{TSource})" /> and their other
    ///     overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <param name="returnType">The return type of result.</param>
    /// <param name="returnDefault">A value indicating whether default should be returned or throw.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateSingleOrDefault(
        ShapedQueryExpression source,
        LambdaExpression? predicate,
        Type returnType,
        bool returnDefault);

    /// <summary>
    ///     Translates <see cref="Queryable.Skip{TSource}(IQueryable{TSource}, int)" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="count">The count supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateSkip(ShapedQueryExpression source, Expression count);

    /// <summary>
    ///     Translates <see cref="Queryable.SkipWhile{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})" /> method over the given
    ///     source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateSkipWhile(ShapedQueryExpression source, LambdaExpression predicate);

    /// <summary>
    ///     Translates <see cref="Queryable.Sum(IQueryable{decimal})" /> method and other overloads over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="selector">The selector supplied in the call.</param>
    /// <param name="resultType">The result type after the operation.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector, Type resultType);

    /// <summary>
    ///     Translates <see cref="Queryable.Take{TSource}(IQueryable{TSource}, int)" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="count">The count supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateTake(ShapedQueryExpression source, Expression count);

    /// <summary>
    ///     Translates <see cref="Queryable.TakeWhile{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})" /> method over the given
    ///     source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateTakeWhile(ShapedQueryExpression source, LambdaExpression predicate);

    /// <summary>
    ///     Translates <see cref="Queryable.ThenBy{TSource, TKey}(IOrderedQueryable{TSource}, Expression{Func{TSource, TKey}})" /> or
    ///     <see cref="Queryable.ThenByDescending{TSource, TKey}(IOrderedQueryable{TSource}, Expression{Func{TSource, TKey}})" /> method
    ///     over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="keySelector">The key selector supplied in the call.</param>
    /// <param name="ascending">A value indicating whether the ordering is ascending or not.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateThenBy(ShapedQueryExpression source, LambdaExpression keySelector, bool ascending);

    /// <summary>
    ///     Translates <see cref="Queryable.Union{TSource}(IQueryable{TSource}, IEnumerable{TSource})" /> method over the given source.
    /// </summary>
    /// <param name="source1">The shaped query on which the operator is applied.</param>
    /// <param name="source2">The other source to perform union with.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateUnion(ShapedQueryExpression source1, ShapedQueryExpression source2);

    /// <summary>
    ///     Translates <see cref="Queryable.Where{TSource}(IQueryable{TSource}, Expression{Func{TSource, bool}})" /> method over the given source.
    /// </summary>
    /// <param name="source">The shaped query on which the operator is applied.</param>
    /// <param name="predicate">The predicate supplied in the call.</param>
    /// <returns>The shaped query after translation.</returns>
    protected abstract ShapedQueryExpression? TranslateWhere(ShapedQueryExpression source, LambdaExpression predicate);

    #region Queryable collection support

    /// <summary>
    ///     Translates a member access. Used when a property on an entity type represents a collection on which queryable LINQ operators
    ///     may be composed.
    /// </summary>
    /// <param name="source">The shaped query on which the property access is applied.</param>
    /// <param name="member">The member being accessed.</param>
    /// <returns>The shaped query after translation.</returns>
    protected virtual ShapedQueryExpression? TranslateMemberAccess(Expression source, MemberIdentity member)
        => null;

    /// <summary>
    ///     Translates an <see cref="InlineQueryRootExpression" />, which represents a queryable collection expressed inline within the
    ///     query.
    /// </summary>
    /// <param name="inlineQueryRootExpression">The inline query root expression to be translated.</param>
    /// <returns>The shaped query after translation.</returns>
    protected virtual ShapedQueryExpression? TranslateInlineQueryRoot(InlineQueryRootExpression inlineQueryRootExpression)
        => null;

    /// <summary>
    ///     Translates a <see cref="ParameterQueryRootExpression" />, which represents a queryable collection referenced as a parameter
    ///     within the query.
    /// </summary>
    /// <param name="parameterQueryRootExpression">The parameter query root expression to be translated.</param>
    /// <returns>The shaped query after translation.</returns>
    protected virtual ShapedQueryExpression? TranslateParameterQueryRoot(ParameterQueryRootExpression parameterQueryRootExpression)
        => null;

    #endregion Queryable collection support
}
