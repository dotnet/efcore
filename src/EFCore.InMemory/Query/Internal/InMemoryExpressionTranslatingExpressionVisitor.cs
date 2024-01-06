// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryExpressionTranslatingExpressionVisitor : ExpressionVisitor
{
    private const string RuntimeParameterPrefix = QueryCompilationContext.QueryParameterPrefix + "entity_equality_";

    private static readonly List<MethodInfo> SingleResultMethodInfos =
    [
        QueryableMethods.FirstWithPredicate,
        QueryableMethods.FirstWithoutPredicate,
        QueryableMethods.FirstOrDefaultWithPredicate,
        QueryableMethods.FirstOrDefaultWithoutPredicate,
        QueryableMethods.SingleWithPredicate,
        QueryableMethods.SingleWithoutPredicate,
        QueryableMethods.SingleOrDefaultWithPredicate,
        QueryableMethods.SingleOrDefaultWithoutPredicate,
        QueryableMethods.LastWithPredicate,
        QueryableMethods.LastWithoutPredicate,
        QueryableMethods.LastOrDefaultWithPredicate,
        QueryableMethods.LastOrDefaultWithoutPredicate
    ];

    private static readonly MemberInfo ValueBufferIsEmpty = typeof(ValueBuffer).GetMember(nameof(ValueBuffer.IsEmpty))[0];

    private static readonly MethodInfo ParameterValueExtractorMethod =
        typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterValueExtractor))!;

    private static readonly MethodInfo ParameterListValueExtractorMethod =
        typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ParameterListValueExtractor))!;

    private static readonly MethodInfo GetParameterValueMethodInfo =
        typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue))!;

    private static readonly MethodInfo LikeMethodInfo = typeof(DbFunctionsExtensions).GetRuntimeMethod(
        nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string)])!;

    private static readonly MethodInfo LikeMethodInfoWithEscape = typeof(DbFunctionsExtensions).GetRuntimeMethod(
        nameof(DbFunctionsExtensions.Like), [typeof(DbFunctions), typeof(string), typeof(string), typeof(string)])!;

    private static readonly MethodInfo RandomMethodInfo = typeof(DbFunctionsExtensions).GetRuntimeMethod(
        nameof(DbFunctionsExtensions.Random), [typeof(DbFunctions)])!;

    private static readonly MethodInfo RandomNextDoubleMethodInfo = typeof(Random).GetRuntimeMethod(
        nameof(Random.NextDouble), Type.EmptyTypes)!;

    private static readonly MethodInfo InMemoryLikeMethodInfo =
        typeof(InMemoryExpressionTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(InMemoryLike))!;

    private static readonly MethodInfo GetTypeMethodInfo = typeof(object).GetTypeInfo().GetDeclaredMethod(nameof(GetType))!;

    // Regex special chars defined here:
    // https://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx
    private static readonly char[] RegexSpecialChars
        = ['.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\'];

    private static readonly string DefaultEscapeRegexCharsPattern = BuildEscapeRegexCharsPattern(RegexSpecialChars);

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(value: 1000.0);

    private static string BuildEscapeRegexCharsPattern(IEnumerable<char> regexSpecialChars)
        => string.Join("|", regexSpecialChars.Select(c => @"\" + c));

    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly QueryableMethodTranslatingExpressionVisitor _queryableMethodTranslatingExpressionVisitor;
    private readonly EntityReferenceFindingExpressionVisitor _entityReferenceFindingExpressionVisitor;
    private readonly IModel _model;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryExpressionTranslatingExpressionVisitor(
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
    {
        _queryCompilationContext = queryCompilationContext;
        _queryableMethodTranslatingExpressionVisitor = queryableMethodTranslatingExpressionVisitor;
        _entityReferenceFindingExpressionVisitor = new EntityReferenceFindingExpressionVisitor();
        _model = queryCompilationContext.Model;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? TranslationErrorDetails { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression? Translate(Expression expression)
    {
        TranslationErrorDetails = null;

        return TranslateInternal(expression);
    }

    private Expression? TranslateInternal(Expression expression)
    {
        var result = Visit(expression);

        return result == QueryCompilationContext.NotTranslatedExpression
            || _entityReferenceFindingExpressionVisitor.Find(result)
                ? null
                : result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.Left.Type == typeof(object[])
            && binaryExpression is { Left: NewArrayExpression, NodeType: ExpressionType.Equal })
        {
            return Visit(ConvertObjectArrayEqualityComparison(binaryExpression.Left, binaryExpression.Right));
        }

        if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
            && (binaryExpression.Left.IsNullConstantExpression() || binaryExpression.Right.IsNullConstantExpression()))
        {
            var nonNullExpression = binaryExpression.Left.IsNullConstantExpression() ? binaryExpression.Right : binaryExpression.Left;
            if (nonNullExpression is MethodCallExpression nonNullMethodCallExpression
                && nonNullMethodCallExpression.Method.DeclaringType == typeof(Queryable)
                && nonNullMethodCallExpression.Method.IsGenericMethod
                && SingleResultMethodInfos.Contains(nonNullMethodCallExpression.Method.GetGenericMethodDefinition()))
            {
                var source = nonNullMethodCallExpression.Arguments[0];
                if (nonNullMethodCallExpression.Arguments.Count == 2)
                {
                    source = Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(source.Type.GetSequenceType()),
                        source,
                        nonNullMethodCallExpression.Arguments[1]);
                }

                var translatedSubquery = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(source);
                if (translatedSubquery != null)
                {
                    var projection = translatedSubquery.ShaperExpression;
                    if (projection is NewExpression
                        || RemoveConvert(projection) is StructuralTypeShaperExpression { IsNullable: false }
                        || RemoveConvert(projection) is CollectionResultShaperExpression)
                    {
                        var anySubquery = Expression.Call(
                            QueryableMethods.AnyWithoutPredicate.MakeGenericMethod(translatedSubquery.Type.GetSequenceType()),
                            translatedSubquery);

                        return Visit(
                            binaryExpression.NodeType == ExpressionType.Equal
                                ? Expression.Not(anySubquery)
                                : anySubquery);
                    }

                    static Expression RemoveConvert(Expression e)
                        => e is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked } unary
                            ? RemoveConvert(unary.Operand)
                            : e;
                }
            }
        }

        if (binaryExpression.NodeType == ExpressionType.Equal
            || binaryExpression.NodeType == ExpressionType.NotEqual
            && binaryExpression.Left.Type == typeof(Type))
        {
            if (IsGetTypeMethodCall(binaryExpression.Left, out var entityReference1)
                && IsTypeConstant(binaryExpression.Right, out var type1))
            {
                return ProcessGetType(entityReference1!, type1!, binaryExpression.NodeType == ExpressionType.Equal);
            }

            if (IsGetTypeMethodCall(binaryExpression.Right, out var entityReference2)
                && IsTypeConstant(binaryExpression.Left, out var type2))
            {
                return ProcessGetType(entityReference2!, type2!, binaryExpression.NodeType == ExpressionType.Equal);
            }
        }

        var newLeft = Visit(binaryExpression.Left);
        var newRight = Visit(binaryExpression.Right);

        if (newLeft == QueryCompilationContext.NotTranslatedExpression
            || newRight == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
            // Visited expression could be null, We need to pass MemberInitExpression
            && TryRewriteEntityEquality(
                binaryExpression.NodeType,
                newLeft,
                newRight,
                equalsMethod: false,
                out var result))
        {
            return result;
        }

        if (IsConvertedToNullable(newLeft, binaryExpression.Left)
            || IsConvertedToNullable(newRight, binaryExpression.Right))
        {
            newLeft = ConvertToNullable(newLeft);
            newRight = ConvertToNullable(newRight);
        }

        if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
            && TryUseComparer(newLeft, newRight, out var updatedExpression))
        {
            if (binaryExpression.NodeType == ExpressionType.NotEqual)
            {
                updatedExpression = Expression.IsFalse(updatedExpression!);
            }

            return updatedExpression!;
        }

        return Expression.MakeBinary(
            binaryExpression.NodeType,
            newLeft,
            newRight,
            binaryExpression.IsLiftedToNull,
            binaryExpression.Method,
            binaryExpression.Conversion);

        Expression ProcessGetType(StructuralTypeReferenceExpression typeReference, Type comparisonType, bool match)
        {
            if (typeReference.StructuralType is not IEntityType entityType
                || (entityType.BaseType == null
                    && !entityType.GetDirectlyDerivedTypes().Any()))
            {
                // No hierarchy
                return Expression.Constant((typeReference.StructuralType.ClrType == comparisonType) == match);
            }

            if (entityType.GetAllBaseTypes().Any(e => e.ClrType == comparisonType))
            {
                // EntitySet will never contain a type of base type
                return Expression.Constant(!match);
            }

            var derivedType = entityType.GetDerivedTypesInclusive().SingleOrDefault(et => et.ClrType == comparisonType);
            // If no derived type matches then fail the translation
            if (derivedType != null)
            {
                // If the derived type is abstract type then predicate will always be false
                if (derivedType.IsAbstract())
                {
                    return Expression.Constant(!match);
                }

                // Or add predicate for matching that particular type discriminator value
                // All hierarchies have discriminator property
                var discriminatorProperty = entityType.FindDiscriminatorProperty()!;
                var boundProperty = BindProperty(typeReference, discriminatorProperty, discriminatorProperty.ClrType);
                // KeyValueComparer is not null at runtime
                var valueComparer = discriminatorProperty.GetKeyValueComparer();

                var result = valueComparer.ExtractEqualsBody(
                    boundProperty!,
                    Expression.Constant(derivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType));

                return match ? result : Expression.Not(result);
            }

            return QueryCompilationContext.NotTranslatedExpression;
        }

        bool IsGetTypeMethodCall(Expression expression, out StructuralTypeReferenceExpression? typeReference)
        {
            typeReference = null;
            if (expression is not MethodCallExpression methodCallExpression
                || methodCallExpression.Method != GetTypeMethodInfo)
            {
                return false;
            }

            typeReference = Visit(methodCallExpression.Object) as StructuralTypeReferenceExpression;
            return typeReference != null;
        }

        static bool IsTypeConstant(Expression expression, out Type? type)
        {
            type = null;
            if (expression is not UnaryExpression
                {
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked,
                    Operand: ConstantExpression constantExpression
                })
            {
                return false;
            }

            type = constantExpression.Value as Type;
            return type != null;
        }
    }

    private static bool TryUseComparer(
        Expression? newLeft,
        Expression? newRight,
        out Expression? updatedExpression)
    {
        updatedExpression = null;

        if (newLeft == null
            || newRight == null)
        {
            return false;
        }

        var property = FindProperty(newLeft) ?? FindProperty(newRight);
        var comparer = property?.GetValueComparer();

        if (comparer == null)
        {
            return false;
        }

        MethodInfo? objectEquals = null;
        MethodInfo? exactMatch = null;

        var converter = property?.GetValueConverter();
        foreach (var candidate in comparer
                     .GetType()
                     .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                     .Where(
                         m => m.Name == "Equals" && m.GetParameters().Length == 2)
                     .ToList())
        {
            var parameters = candidate.GetParameters();
            var leftType = parameters[0].ParameterType;
            var rightType = parameters[1].ParameterType;

            if (leftType == typeof(object)
                && rightType == typeof(object))
            {
                objectEquals = candidate;
                continue;
            }

            var matchingLeft = leftType.IsAssignableFrom(newLeft.Type)
                ? newLeft
                : converter != null
                && leftType.IsAssignableFrom(converter.ModelClrType)
                && converter.ProviderClrType.IsAssignableFrom(newLeft.Type)
                    ? ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        newLeft,
                        converter.ConvertFromProviderExpression.Body)
                    : null;

            var matchingRight = rightType.IsAssignableFrom(newRight.Type)
                ? newRight
                : converter != null
                && rightType.IsAssignableFrom(converter.ModelClrType)
                && converter.ProviderClrType.IsAssignableFrom(newRight.Type)
                    ? ReplacingExpressionVisitor.Replace(
                        converter.ConvertFromProviderExpression.Parameters.Single(),
                        newRight,
                        converter.ConvertFromProviderExpression.Body)
                    : null;

            if (matchingLeft != null && matchingRight != null)
            {
                exactMatch = candidate;
                newLeft = matchingLeft;
                newRight = matchingRight;
                break;
            }
        }

        if (exactMatch == null
            && (!property!.ClrType.IsAssignableFrom(newLeft.Type))
            || !property!.ClrType.IsAssignableFrom(newRight.Type))
        {
            return false;
        }

        updatedExpression =
            exactMatch != null
                ? Expression.Call(
                    Expression.Constant(comparer, comparer.GetType()),
                    exactMatch,
                    newLeft,
                    newRight)
                : Expression.Call(
                    Expression.Constant(comparer, comparer.GetType()),
                    objectEquals!,
                    Expression.Convert(newLeft, typeof(object)),
                    Expression.Convert(newRight, typeof(object)));

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
    {
        var test = Visit(conditionalExpression.Test);
        var ifTrue = Visit(conditionalExpression.IfTrue);
        var ifFalse = Visit(conditionalExpression.IfFalse);

        if (test == QueryCompilationContext.NotTranslatedExpression
            || ifTrue == QueryCompilationContext.NotTranslatedExpression
            || ifFalse == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (test.Type == typeof(bool?))
        {
            test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
        }

        if (IsConvertedToNullable(ifTrue, conditionalExpression.IfTrue)
            || IsConvertedToNullable(ifFalse, conditionalExpression.IfFalse))
        {
            ifTrue = ConvertToNullable(ifTrue);
            ifFalse = ConvertToNullable(ifFalse);
        }

        return Expression.Condition(test, ifTrue, ifFalse);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case EntityProjectionExpression:
            case StructuralTypeReferenceExpression:
                return extensionExpression;

            case StructuralTypeShaperExpression shaper:
                return new StructuralTypeReferenceExpression(shaper);

            case ProjectionBindingExpression projectionBindingExpression:
                return ((InMemoryQueryExpression)projectionBindingExpression.QueryExpression)
                    .GetProjection(projectionBindingExpression);

            default:
                return QueryCompilationContext.NotTranslatedExpression;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitInvocation(InvocationExpression invocationExpression)
        => QueryCompilationContext.NotTranslatedExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        => throw new InvalidOperationException(CoreStrings.TranslationFailed(lambdaExpression.Print()));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitListInit(ListInitExpression listInitExpression)
        => QueryCompilationContext.NotTranslatedExpression;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var innerExpression = Visit(memberExpression.Expression);

        // when visiting unary we remove converts from nullable to non-nullable
        // however if this happens for memberExpression.Expression we are unable to bind
        if (innerExpression != null
            && memberExpression.Expression != null
            && innerExpression.Type != memberExpression.Expression.Type
            && innerExpression.Type.IsNullableType()
            && innerExpression.Type.UnwrapNullableType() == memberExpression.Expression.Type)
        {
            innerExpression = Expression.Convert(innerExpression, memberExpression.Expression.Type);
        }

        if (memberExpression.Expression != null
            && innerExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (TryBindMember(innerExpression, MemberIdentity.Create(memberExpression.Member), memberExpression.Type) is Expression result)
        {
            return result;
        }

        var updatedMemberExpression = (Expression)memberExpression.Update(innerExpression);
        if (innerExpression != null
            && innerExpression.Type.IsNullableType()
            && ShouldApplyNullProtectionForMemberAccess(innerExpression.Type, memberExpression.Member.Name))
        {
            updatedMemberExpression = ConvertToNullable(updatedMemberExpression);

            return Expression.Condition(
                // Since inner is nullable type this is fine.
                Expression.Equal(innerExpression, Expression.Default(innerExpression.Type)),
                Expression.Default(updatedMemberExpression.Type),
                updatedMemberExpression);
        }

        return updatedMemberExpression;

        static bool ShouldApplyNullProtectionForMemberAccess(Type callerType, string memberName)
            => !(callerType.IsGenericType
                && callerType.GetGenericTypeDefinition() == typeof(Nullable<>)
                && memberName is nameof(Nullable<int>.Value) or nameof(Nullable<int>.HasValue));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
    {
        var expression = Visit(memberAssignment.Expression);
        if (expression == QueryCompilationContext.NotTranslatedExpression)
        {
            return memberAssignment.Update(Expression.Convert(expression, memberAssignment.Expression.Type));
        }

        if (IsConvertedToNullable(expression, memberAssignment.Expression))
        {
            expression = ConvertToNonNullable(expression);
        }

        return memberAssignment.Update(expression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
    {
        var newExpression = Visit(memberInitExpression.NewExpression);
        if (newExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
        for (var i = 0; i < newBindings.Length; i++)
        {
            if (memberInitExpression.Bindings[i].BindingType != MemberBindingType.Assignment)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            newBindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);
            if (((MemberAssignment)newBindings[i]).Expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                && unaryExpression.Operand == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        return memberInitExpression.Update((NewExpression)newExpression, newBindings);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Method.IsGenericMethod
            && methodCallExpression.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod)
        {
            return methodCallExpression;
        }

        // EF.Property case
        if (methodCallExpression.TryGetEFPropertyArguments(out var source, out var propertyName))
        {
            return TryBindMember(Visit(source), MemberIdentity.Create(propertyName), methodCallExpression.Type)
                ?? throw new InvalidOperationException(CoreStrings.QueryUnableToTranslateEFProperty(methodCallExpression.Print()));
        }

        // EF Indexer property
        if (methodCallExpression.TryGetIndexerArguments(_model, out source, out propertyName))
        {
            return TryBindMember(Visit(source), MemberIdentity.Create(propertyName), methodCallExpression.Type)
                ?? QueryCompilationContext.NotTranslatedExpression;
        }

        // Subquery case
        var subqueryTranslation = _queryableMethodTranslatingExpressionVisitor.TranslateSubquery(methodCallExpression);
        if (subqueryTranslation != null)
        {
            var subquery = (InMemoryQueryExpression)subqueryTranslation.QueryExpression;
            if (subqueryTranslation.ResultCardinality == ResultCardinality.Enumerable)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var shaperExpression = subqueryTranslation.ShaperExpression;
            var innerExpression = shaperExpression;
            Type? convertedType = null;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
            {
                convertedType = unaryExpression.Type;
                innerExpression = unaryExpression.Operand;
            }

            if (innerExpression is StructuralTypeShaperExpression shaper
                && (convertedType == null
                    || convertedType.IsAssignableFrom(shaper.Type)))
            {
                return new StructuralTypeReferenceExpression(subqueryTranslation.UpdateShaperExpression(innerExpression));
            }

            if (!(innerExpression is ProjectionBindingExpression projectionBindingExpression
                    && (convertedType == null
                        || convertedType.MakeNullable() == innerExpression.Type)))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            if (projectionBindingExpression.ProjectionMember == null)
            {
                // We don't lift scalar subquery with client eval
                return QueryCompilationContext.NotTranslatedExpression;
            }

            return ProcessSingleResultScalar(
                subquery,
                subquery.GetProjection(projectionBindingExpression),
                methodCallExpression.Type);
        }

        if (methodCallExpression.Method == LikeMethodInfo
            || methodCallExpression.Method == LikeMethodInfoWithEscape)
        {
            // EF.Functions.Like
            var visitedArguments = new Expression[3];
            visitedArguments[2] = Expression.Constant(null, typeof(string));
            // Skip first DbFunctions argument
            for (var i = 1; i < methodCallExpression.Arguments.Count; i++)
            {
                var argument = Visit(methodCallExpression.Arguments[i]);
                if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                visitedArguments[i - 1] = argument;
            }

            return Expression.Call(InMemoryLikeMethodInfo, visitedArguments);
        }

        if (methodCallExpression.Method == RandomMethodInfo)
        {
            return Expression.Call(Expression.New(typeof(Random)), RandomNextDoubleMethodInfo);
        }

        Expression? @object = null;
        Expression[] arguments;
        var method = methodCallExpression.Method;

        if (method.Name == nameof(object.Equals)
            && methodCallExpression is { Object: not null, Arguments.Count: 1 })
        {
            var left = Visit(methodCallExpression.Object);
            var right = Visit(methodCallExpression.Arguments[0]);

            if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Object : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : right,
                    equalsMethod: true,
                    out var result))
            {
                return result;
            }

            if (TranslationFailed(methodCallExpression.Object, left)
                || TranslationFailed(methodCallExpression.Arguments[0], right))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            @object = left;
            arguments = [right];
        }
        else if (method.Name == nameof(object.Equals)
                 && methodCallExpression.Object == null
                 && methodCallExpression.Arguments.Count == 2)
        {
            if (methodCallExpression.Arguments[0].Type == typeof(object[])
                && methodCallExpression.Arguments[0] is NewArrayExpression)
            {
                return Visit(
                    ConvertObjectArrayEqualityComparison(
                        methodCallExpression.Arguments[0], methodCallExpression.Arguments[1]));
            }

            var left = Visit(methodCallExpression.Arguments[0]);
            var right = Visit(methodCallExpression.Arguments[1]);

            if (TryUseComparer(left, right, out var updatedExpression))
            {
                return updatedExpression!;
            }

            if (TryRewriteEntityEquality(
                    ExpressionType.Equal,
                    left == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : left,
                    right == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[1] : right,
                    equalsMethod: true,
                    out var result))
            {
                return result;
            }

            if (TranslationFailed(methodCallExpression.Arguments[0], left)
                || TranslationFailed(methodCallExpression.Arguments[1], right))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            arguments = [left, right];
        }
        else if (method.IsGenericMethod
                 && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains))
        {
            var enumerable = Visit(methodCallExpression.Arguments[0]);
            var item = Visit(methodCallExpression.Arguments[1]);

            if (TryRewriteContainsEntity(
                    enumerable,
                    item == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[1] : item,
                    out var result))
            {
                return result;
            }

            if (TranslationFailed(methodCallExpression.Arguments[0], enumerable)
                || TranslationFailed(methodCallExpression.Arguments[1], item))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            arguments = [enumerable, item];
        }
        else if (methodCallExpression.Arguments.Count == 1
                 && method.IsContainsMethod())
        {
            var enumerable = Visit(methodCallExpression.Object);
            var item = Visit(methodCallExpression.Arguments[0]);

            if (TryRewriteContainsEntity(
                    enumerable,
                    item == QueryCompilationContext.NotTranslatedExpression ? methodCallExpression.Arguments[0] : item,
                    out var result))
            {
                return result;
            }

            if (TranslationFailed(methodCallExpression.Object, enumerable)
                || TranslationFailed(methodCallExpression.Arguments[0], item))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            @object = enumerable;
            arguments = [item];
        }
        else
        {
            @object = Visit(methodCallExpression.Object);
            if (TranslationFailed(methodCallExpression.Object, @object))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            arguments = new Expression[methodCallExpression.Arguments.Count];
            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = Visit(methodCallExpression.Arguments[i]);
                if (TranslationFailed(methodCallExpression.Arguments[i], argument))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                arguments[i] = argument;
            }
        }

        // if the nullability of arguments change, we have no easy/reliable way to adjust the actual methodInfo to match the new type,
        // so we are forced to cast back to the original type
        var parameterTypes = methodCallExpression.Method.GetParameters().Select(p => p.ParameterType).ToArray();
        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = arguments[i];
            if (IsConvertedToNullable(argument, methodCallExpression.Arguments[i])
                && !parameterTypes[i].IsAssignableFrom(argument.Type))
            {
                argument = ConvertToNonNullable(argument);
            }

            arguments[i] = argument;
        }

        // if object is nullable, add null safeguard before calling the function
        // we special-case Nullable<>.GetValueOrDefault, which doesn't need the safeguard
        if (methodCallExpression.Object != null
            && @object!.Type.IsNullableType()
            && methodCallExpression.Method.Name != nameof(Nullable<int>.GetValueOrDefault))
        {
            var result = (Expression)methodCallExpression.Update(
                Expression.Convert(@object, methodCallExpression.Object.Type),
                arguments);

            result = ConvertToNullable(result);
            var objectNullCheck = Expression.Equal(@object, Expression.Constant(null, @object.Type));
            // instance.Equals(argument) should translate to
            // instance == null ? argument == null : instance.Equals(argument)
            if (method.Name == nameof(object.Equals))
            {
                var argument = arguments[0];
                if (argument.NodeType == ExpressionType.Convert
                    && argument is UnaryExpression unaryExpression
                    && argument.Type == unaryExpression.Operand.Type.UnwrapNullableType())
                {
                    argument = unaryExpression.Operand;
                }

                if (!argument.Type.IsNullableType())
                {
                    argument = Expression.Convert(argument, argument.Type.MakeNullable());
                }

                return Expression.Condition(
                    objectNullCheck,
                    ConvertToNullable(Expression.Equal(argument, Expression.Constant(null, argument.Type))),
                    result);
            }

            return Expression.Condition(objectNullCheck, Expression.Constant(null, result.Type), result);
        }

        return methodCallExpression.Update(@object, arguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNew(NewExpression newExpression)
    {
        var newArguments = new List<Expression>();
        foreach (var argument in newExpression.Arguments)
        {
            var newArgument = Visit(argument);
            if (newArgument == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            if (IsConvertedToNullable(newArgument, argument))
            {
                newArgument = ConvertToNonNullable(newArgument);
            }

            newArguments.Add(newArgument);
        }

        return newExpression.Update(newArguments);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitNewArray(NewArrayExpression newArrayExpression)
    {
        var newExpressions = new List<Expression>();
        foreach (var expression in newArrayExpression.Expressions)
        {
            var newExpression = Visit(expression);
            if (newExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            if (IsConvertedToNullable(newExpression, expression))
            {
                newExpression = ConvertToNonNullable(newExpression);
            }

            newExpressions.Add(newExpression);
        }

        return newArrayExpression.Update(newExpressions);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitParameter(ParameterExpression parameterExpression)
    {
        if (parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true)
        {
            return Expression.Call(
                GetParameterValueMethodInfo.MakeGenericMethod(parameterExpression.Type),
                QueryCompilationContext.QueryContextParameter,
                Expression.Constant(parameterExpression.Name));
        }

        throw new InvalidOperationException(CoreStrings.TranslationFailed(parameterExpression.Print()));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitTypeBinary(TypeBinaryExpression typeBinaryExpression)
    {
        if (typeBinaryExpression.NodeType == ExpressionType.TypeIs
            && Visit(typeBinaryExpression.Expression) is StructuralTypeReferenceExpression typeReference)
        {
            if (typeReference.StructuralType is not IEntityType entityType)
            {
                return Expression.Constant(typeReference.StructuralType.ClrType == typeBinaryExpression.TypeOperand);
            }

            if (entityType.GetAllBaseTypesInclusive().Any(et => et.ClrType == typeBinaryExpression.TypeOperand))
            {
                return Expression.Constant(true);
            }

            var derivedType = entityType.GetDerivedTypes().SingleOrDefault(et => et.ClrType == typeBinaryExpression.TypeOperand);
            if (derivedType != null)
            {
                // All hierarchies have discriminator property
                var discriminatorProperty = entityType.FindDiscriminatorProperty()!;
                var boundProperty = BindProperty(typeReference, discriminatorProperty, discriminatorProperty.ClrType);
                // KeyValueComparer is not null at runtime
                var valueComparer = discriminatorProperty.GetKeyValueComparer();

                var equals = valueComparer.ExtractEqualsBody(
                    boundProperty!,
                    Expression.Constant(derivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType));

                foreach (var derivedDerivedType in derivedType.GetDerivedTypes())
                {
                    equals = Expression.OrElse(
                        equals,
                        valueComparer.ExtractEqualsBody(
                            boundProperty!,
                            Expression.Constant(derivedDerivedType.GetDiscriminatorValue(), discriminatorProperty.ClrType)));
                }

                return equals;
            }
        }

        return QueryCompilationContext.NotTranslatedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        var newOperand = Visit(unaryExpression.Operand);
        if (newOperand == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (newOperand is StructuralTypeReferenceExpression typeReference
            && unaryExpression.NodeType is ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs)
        {
            return typeReference.Convert(unaryExpression.Type);
        }

        if (unaryExpression.NodeType == ExpressionType.Convert
            && newOperand.Type == unaryExpression.Type)
        {
            return newOperand;
        }

        if (unaryExpression.NodeType == ExpressionType.Convert
            && IsConvertedToNullable(newOperand, unaryExpression))
        {
            return newOperand;
        }

        var result = (Expression)Expression.MakeUnary(unaryExpression.NodeType, newOperand, unaryExpression.Type);
        if (result is UnaryExpression
            {
                NodeType: ExpressionType.Convert,
                Operand: UnaryExpression { NodeType: ExpressionType.Convert } innerUnary
            } outerUnary)
        {
            var innerMostType = innerUnary.Operand.Type;
            var intermediateType = innerUnary.Type;
            var outerMostType = outerUnary.Type;

            if (outerMostType == innerMostType
                && intermediateType == innerMostType.UnwrapNullableType())
            {
                result = innerUnary.Operand;
            }
            else if (outerMostType == typeof(object)
                     && intermediateType == innerMostType.UnwrapNullableType())
            {
                result = Expression.Convert(innerUnary.Operand, typeof(object));
            }
        }

        return result;
    }

    private Expression? TryBindMember(Expression? source, MemberIdentity member, Type type)
    {
        if (source is not StructuralTypeReferenceExpression typeReference)
        {
            return null;
        }

        var entityType = typeReference.StructuralType;

        var property = member.MemberInfo != null
            ? entityType.FindProperty(member.MemberInfo)
            : entityType.FindProperty(member.Name!);

        if (property != null)
        {
            return BindProperty(typeReference, property, type);
        }

        AddTranslationErrorDetails(
            CoreStrings.QueryUnableToTranslateMember(
                member.Name,
                typeReference.StructuralType.DisplayName()));

        return null;
    }

    private Expression? BindProperty(StructuralTypeReferenceExpression typeReference, IProperty property, Type type)
    {
        if (typeReference.Parameter != null)
        {
            var valueBufferExpression = Visit(typeReference.Parameter.ValueBufferExpression);
            if (valueBufferExpression == QueryCompilationContext.NotTranslatedExpression)
            {
                return null;
            }

            var result = ((EntityProjectionExpression)valueBufferExpression).BindProperty(property);

            // if the result type change was just nullability change e.g from int to int?
            // we want to preserve the new type for null propagation
            return result.Type != type
                && !(result.Type.IsNullableType()
                    && !type.IsNullableType()
                    && result.Type.UnwrapNullableType() == type)
                    ? Expression.Convert(result, type)
                    : result;
        }

        if (typeReference.Subquery != null)
        {
            var entityShaper = (StructuralTypeShaperExpression)typeReference.Subquery.ShaperExpression;
            var inMemoryQueryExpression = (InMemoryQueryExpression)typeReference.Subquery.QueryExpression;

            var projectionBindingExpression = (ProjectionBindingExpression)entityShaper.ValueBufferExpression;
            var entityProjectionExpression = (EntityProjectionExpression)inMemoryQueryExpression.GetProjection(
                projectionBindingExpression);
            var readValueExpression = entityProjectionExpression.BindProperty(property);

            return ProcessSingleResultScalar(
                inMemoryQueryExpression,
                readValueExpression,
                type);
        }

        return null;
    }

    private static Expression ProcessSingleResultScalar(
        InMemoryQueryExpression inMemoryQueryExpression,
        Expression readValueExpression,
        Type type)
    {
        if (inMemoryQueryExpression.ServerQueryExpression is not NewExpression)
        {
            // The terminating operator is not applied
            // It is of FirstOrDefault kind
            // So we change to single column projection and then apply it.
            inMemoryQueryExpression.ReplaceProjection(
                new Dictionary<ProjectionMember, Expression> { { new ProjectionMember(), readValueExpression } });
            inMemoryQueryExpression.ApplyProjection();
        }

        var serverQuery = inMemoryQueryExpression.ServerQueryExpression;
        serverQuery = ((LambdaExpression)((NewExpression)serverQuery).Arguments[0]).Body;
        if (serverQuery is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Type == typeof(object))
        {
            serverQuery = unaryExpression.Operand;
        }

        var valueBufferVariable = Expression.Variable(typeof(ValueBuffer));
        var readExpression = valueBufferVariable.CreateValueBufferReadValueExpression(type, index: 0, property: null);
        return Expression.Block(
            variables: new[] { valueBufferVariable },
            Expression.Assign(valueBufferVariable, serverQuery),
            Expression.Condition(
                Expression.MakeMemberAccess(valueBufferVariable, ValueBufferIsEmpty),
                Expression.Default(type),
                readExpression));
    }

    [UsedImplicitly]
    private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
        => (T)queryContext.ParameterValues[parameterName]!;

    private static bool IsConvertedToNullable(Expression result, Expression original)
        => result.Type.IsNullableType()
            && !original.Type.IsNullableType()
            && result.Type.UnwrapNullableType() == original.Type;

    private static Expression ConvertToNullable(Expression expression)
        => !expression.Type.IsNullableType()
            ? Expression.Convert(expression, expression.Type.MakeNullable())
            : expression;

    private static Expression ConvertToNonNullable(Expression expression)
        => expression.Type.IsNullableType()
            ? Expression.Convert(expression, expression.Type.UnwrapNullableType())
            : expression;

    private static IProperty? FindProperty(Expression? expression)
    {
        if (expression?.NodeType == ExpressionType.Convert
            && expression.Type == typeof(object))
        {
            expression = ((UnaryExpression)expression).Operand;
        }

        if (expression?.NodeType == ExpressionType.Convert
            && expression.Type.IsNullableType()
            && expression is UnaryExpression unaryExpression
            && (expression.Type.UnwrapNullableType() == unaryExpression.Type
                || expression.Type == unaryExpression.Type))
        {
            expression = unaryExpression.Operand;
        }

        if (expression is MethodCallExpression { Method.IsGenericMethod: true } readValueMethodCall
            && readValueMethodCall.Method.GetGenericMethodDefinition() == ExpressionExtensions.ValueBufferTryReadValueMethod)
        {
            return readValueMethodCall.Arguments[2].GetConstantValue<IProperty>();
        }

        return null;
    }

    private bool TryRewriteContainsEntity(Expression? source, Expression item, [NotNullWhen(true)] out Expression? result)
    {
        result = null;

        if (item is not StructuralTypeReferenceExpression { StructuralType: IEntityType entityType })
        {
            return false;
        }

        var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
        if (primaryKeyProperties == null)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nameof(Queryable.Contains), entityType.DisplayName()));
        }

        if (primaryKeyProperties.Count > 1)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported(
                    nameof(Queryable.Contains), entityType.DisplayName()));
        }

        var property = primaryKeyProperties[0];
        Expression rewrittenSource;
        switch (source)
        {
            case ConstantExpression constantExpression:
                var values = constantExpression.GetConstantValue<IEnumerable>();
                var propertyValueList =
                    (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.ClrType.MakeNullable()))!;
                var propertyGetter = property.GetGetter();
                foreach (var value in values)
                {
                    propertyValueList.Add(propertyGetter.GetClrValue(value));
                }

                rewrittenSource = Expression.Constant(propertyValueList);
                break;

            case MethodCallExpression { Method.IsGenericMethod: true } methodCallExpression
                when methodCallExpression.Method.GetGenericMethodDefinition() == GetParameterValueMethodInfo:
                var parameterName = methodCallExpression.Arguments[1].GetConstantValue<string>();
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterListValueExtractorMethod.MakeGenericMethod(entityType.ClrType, property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(parameterName, typeof(string)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter
                );

                var newParameterName =
                    $"{RuntimeParameterPrefix}"
                    + $"{parameterName[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

                rewrittenSource = _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);
                break;

            default:
                return false;
        }

        result = Visit(
            Expression.Call(
                EnumerableMethods.Contains.MakeGenericMethod(property.ClrType.MakeNullable()),
                rewrittenSource,
                CreatePropertyAccessExpression(item, property)));

        return true;
    }

    private bool TryRewriteEntityEquality(
        ExpressionType nodeType,
        Expression left,
        Expression right,
        bool equalsMethod,
        [NotNullWhen(true)] out Expression? result)
    {
        var leftEntityReference = left is StructuralTypeReferenceExpression { StructuralType: IEntityType } l ? l : null;
        var rightEntityReference = right is StructuralTypeReferenceExpression { StructuralType: IEntityType } r ? r : null;

        if (leftEntityReference == null
            && rightEntityReference == null)
        {
            result = null;
            return false;
        }

        if (IsNullConstantExpression(left)
            || IsNullConstantExpression(right))
        {
            var nonNullEntityReference = (IsNullConstantExpression(left) ? rightEntityReference : leftEntityReference)!;
            var entityType1 = (IEntityType)nonNullEntityReference.StructuralType;
            var primaryKeyProperties1 = entityType1.FindPrimaryKey()?.Properties;
            if (primaryKeyProperties1 == null)
            {
                throw new InvalidOperationException(
                    CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                        nodeType == ExpressionType.Equal
                            ? equalsMethod ? nameof(object.Equals) : "=="
                            : equalsMethod
                                ? "!" + nameof(object.Equals)
                                : "!=",
                        entityType1.DisplayName()));
            }

            result = Visit(
                primaryKeyProperties1.Select(
                        p =>
                            Expression.MakeBinary(
                                nodeType, CreatePropertyAccessExpression(nonNullEntityReference, p),
                                Expression.Constant(null, p.ClrType.MakeNullable())))
                    .Aggregate((l, r) => nodeType == ExpressionType.Equal ? Expression.OrElse(l, r) : Expression.AndAlso(l, r)));

            return true;
        }

        var leftEntityType = (IEntityType?)leftEntityReference?.StructuralType;
        var rightEntityType = (IEntityType?)rightEntityReference?.StructuralType;
        var entityType = leftEntityType ?? rightEntityType;

        Check.DebugAssert(entityType != null, "At least either side should be entityReference so entityType should be non-null.");

        if (leftEntityType != null
            && rightEntityType != null
            && leftEntityType.GetRootType() != rightEntityType.GetRootType())
        {
            result = Expression.Constant(false);
            return true;
        }

        var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;
        if (primaryKeyProperties == null)
        {
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnKeylessEntityNotSupported(
                    nodeType == ExpressionType.Equal
                        ? equalsMethod ? nameof(object.Equals) : "=="
                        : equalsMethod
                            ? "!" + nameof(object.Equals)
                            : "!=",
                    entityType.DisplayName()));
        }

        if (primaryKeyProperties.Count > 1
            && (leftEntityReference?.Subquery != null
                || rightEntityReference?.Subquery != null))
        {
            throw new InvalidOperationException(
                CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported(
                    nodeType == ExpressionType.Equal
                        ? equalsMethod ? nameof(object.Equals) : "=="
                        : equalsMethod
                            ? "!" + nameof(object.Equals)
                            : "!=",
                    entityType.DisplayName()));
        }

        result = Visit(
            primaryKeyProperties.Select(
                    p =>
                        Expression.MakeBinary(
                            nodeType,
                            CreatePropertyAccessExpression(left, p),
                            CreatePropertyAccessExpression(right, p)))
                .Aggregate(
                    (l, r) => nodeType == ExpressionType.Equal
                        ? Expression.AndAlso(l, r)
                        : Expression.OrElse(l, r)));

        return true;
    }

    private Expression CreatePropertyAccessExpression(Expression target, IProperty property)
    {
        switch (target)
        {
            case ConstantExpression constantExpression:
                return Expression.Constant(
                    constantExpression.Value is null
                        ? null
                        : property.GetGetter().GetClrValue(constantExpression.Value),
                    property.ClrType.MakeNullable());

            case MethodCallExpression { Method.IsGenericMethod: true } methodCallExpression
                when methodCallExpression.Method.GetGenericMethodDefinition() == GetParameterValueMethodInfo:
                var parameterName = methodCallExpression.Arguments[1].GetConstantValue<string>();
                var lambda = Expression.Lambda(
                    Expression.Call(
                        ParameterValueExtractorMethod.MakeGenericMethod(property.ClrType.MakeNullable()),
                        QueryCompilationContext.QueryContextParameter,
                        Expression.Constant(parameterName, typeof(string)),
                        Expression.Constant(property, typeof(IProperty))),
                    QueryCompilationContext.QueryContextParameter);

                var newParameterName =
                    $"{RuntimeParameterPrefix}"
                    + $"{parameterName[QueryCompilationContext.QueryParameterPrefix.Length..]}_{property.Name}";

                return _queryCompilationContext.RegisterRuntimeParameter(newParameterName, lambda);

            case MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.SingleOrDefault(
                    mb => mb.Member.Name == property.Name) is MemberAssignment memberAssignment:
                return memberAssignment.Expression.Type.IsNullableType()
                    ? memberAssignment.Expression
                    : Expression.Convert(memberAssignment.Expression, property.ClrType.MakeNullable());

            case NewExpression newExpression
                when CanEvaluate(newExpression):
                return CreatePropertyAccessExpression(GetValue(newExpression), property);

            case MemberInitExpression memberInitExpression
                when CanEvaluate(memberInitExpression):
                return CreatePropertyAccessExpression(GetValue(memberInitExpression), property);

            default:
                return target.CreateEFPropertyExpression(property);
        }
    }

    private static T? ParameterValueExtractor<T>(QueryContext context, string baseParameterName, IProperty property)
    {
        var baseParameter = context.ParameterValues[baseParameterName];
        return baseParameter == null ? (T?)(object?)null : (T?)property.GetGetter().GetClrValue(baseParameter);
    }

    private static List<TProperty?>? ParameterListValueExtractor<TEntity, TProperty>(
        QueryContext context,
        string baseParameterName,
        IProperty property)
    {
        if (!(context.ParameterValues[baseParameterName] is IEnumerable<TEntity> baseListParameter))
        {
            return null;
        }

        var getter = property.GetGetter();
        return baseListParameter.Select(e => e != null ? (TProperty?)getter.GetClrValue(e) : (TProperty?)(object?)null).ToList();
    }

    private static ConstantExpression GetValue(Expression expression)
        => Expression.Constant(
            Expression.Lambda<Func<object>>(Expression.Convert(expression, typeof(object)))
                .Compile(preferInterpretation: true)
                .Invoke(),
            expression.Type);

    private static bool CanEvaluate(Expression expression)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (expression)
#pragma warning restore IDE0066 // Convert switch statement to expression
        {
            case ConstantExpression:
                return true;

            case NewExpression newExpression:
                return newExpression.Arguments.All(CanEvaluate);

            case MemberInitExpression memberInitExpression:
                return CanEvaluate(memberInitExpression.NewExpression)
                    && memberInitExpression.Bindings.All(
                        mb => mb is MemberAssignment memberAssignment && CanEvaluate(memberAssignment.Expression));

            default:
                return false;
        }
    }

    private static Expression ConvertObjectArrayEqualityComparison(Expression left, Expression right)
    {
        var leftExpressions = ((NewArrayExpression)left).Expressions;
        var rightExpressions = ((NewArrayExpression)right).Expressions;

        return leftExpressions.Zip(
                rightExpressions,
                (l, r) =>
                {
                    l = RemoveObjectConvert(l);
                    r = RemoveObjectConvert(r);
                    if (l.Type.IsNullableType())
                    {
                        r = r.Type.IsNullableType() ? r : Expression.Convert(r, l.Type);
                    }
                    else if (r.Type.IsNullableType())
                    {
                        l = l.Type.IsNullableType() ? l : Expression.Convert(l, r.Type);
                    }

                    return ExpressionExtensions.CreateEqualsExpression(l, r);
                })
            .Aggregate((a, b) => Expression.AndAlso(a, b));

        static Expression RemoveObjectConvert(Expression expression)
            => expression is UnaryExpression unaryExpression
                && expression.Type == typeof(object)
                && expression.NodeType == ExpressionType.Convert
                    ? unaryExpression.Operand
                    : expression;
    }

    private static bool IsNullConstantExpression(Expression expression)
        => expression is ConstantExpression { Value: null };

    [DebuggerStepThrough]
    private static bool TranslationFailed(Expression? original, Expression? translation)
        => original != null
            && (translation == QueryCompilationContext.NotTranslatedExpression || translation is StructuralTypeReferenceExpression);

    private static bool InMemoryLike(string matchExpression, string pattern, string escapeCharacter)
    {
        //TODO: this fixes https://github.com/aspnet/EntityFramework/issues/8656 by insisting that
        // the "escape character" is a string but just using the first character of that string,
        // but we may later want to allow the complete string as the "escape character"
        // in which case we need to change the way we construct the regex below.
        var singleEscapeCharacter =
            (escapeCharacter == null || escapeCharacter.Length == 0)
                ? (char?)null
                : escapeCharacter.First();

        if (matchExpression == null
            || pattern == null)
        {
            return false;
        }

        if (matchExpression.Equals(pattern, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (matchExpression.Length == 0
            || pattern.Length == 0)
        {
            return false;
        }

        var escapeRegexCharsPattern
            = singleEscapeCharacter == null
                ? DefaultEscapeRegexCharsPattern
                : BuildEscapeRegexCharsPattern(RegexSpecialChars.Where(c => c != singleEscapeCharacter));

        var regexPattern
            = Regex.Replace(
                pattern,
                escapeRegexCharsPattern,
                c => @"\" + c,
                default,
                RegexTimeout);

        var stringBuilder = new StringBuilder();

        for (var i = 0; i < regexPattern.Length; i++)
        {
            var c = regexPattern[i];
            var escaped = i > 0 && regexPattern[i - 1] == singleEscapeCharacter;

            switch (c)
            {
                case '_':
                {
                    stringBuilder.Append(escaped ? '_' : '.');
                    break;
                }
                case '%':
                {
                    stringBuilder.Append(escaped ? "%" : ".*");
                    break;
                }
                default:
                {
                    if (c != singleEscapeCharacter)
                    {
                        stringBuilder.Append(c);
                    }

                    break;
                }
            }
        }

        regexPattern = stringBuilder.ToString();

        return Regex.IsMatch(
            matchExpression,
            @"\A" + regexPattern + @"\s*\z",
            RegexOptions.IgnoreCase | RegexOptions.Singleline,
            RegexTimeout);
    }

    private sealed class EntityReferenceFindingExpressionVisitor : ExpressionVisitor
    {
        private bool _found;

        public bool Find(Expression expression)
        {
            _found = false;

            Visit(expression);

            return _found;
        }

        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            if (_found)
            {
                return expression;
            }

            if (expression is StructuralTypeReferenceExpression)
            {
                _found = true;
                return expression;
            }

            return base.Visit(expression);
        }
    }

    private sealed class StructuralTypeReferenceExpression : Expression
    {
        public StructuralTypeReferenceExpression(StructuralTypeShaperExpression parameter)
        {
            Parameter = parameter;
            StructuralType = parameter.StructuralType;
        }

        public StructuralTypeReferenceExpression(ShapedQueryExpression subquery)
        {
            Subquery = subquery;
            StructuralType = ((StructuralTypeShaperExpression)subquery.ShaperExpression).StructuralType;
        }

        private StructuralTypeReferenceExpression(StructuralTypeReferenceExpression typeReference, IEntityType type)
        {
            Parameter = typeReference.Parameter;
            Subquery = typeReference.Subquery;
            StructuralType = type;
        }

        public new StructuralTypeShaperExpression? Parameter { get; }
        public ShapedQueryExpression? Subquery { get; }
        public ITypeBase StructuralType { get; }

        public override Type Type
            => StructuralType.ClrType;

        public override ExpressionType NodeType
            => ExpressionType.Extension;

        public Expression Convert(Type type)
        {
            if (type == typeof(object) // Ignore object conversion
                || type.IsAssignableFrom(Type)) // Ignore casting to base type/interface
            {
                return this;
            }

            return StructuralType is IEntityType entityType
                && entityType.GetDerivedTypes().FirstOrDefault(et => et.ClrType == type) is IEntityType derivedEntityType
                    ? new StructuralTypeReferenceExpression(this, derivedEntityType)
                    : QueryCompilationContext.NotTranslatedExpression;
        }
    }
}
