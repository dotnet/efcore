// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         A class that compiles the shaper expression for given shaped query expression.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Materializer is a code which creates entity instance from the given property values.
///         It takes into account constructor bindings, fields, property access mode configured in the model when creating the instance.
///     </para>
///     <para>
///         Shaper is a code which generate result for the query from given scalar values based on the structure of projection.
///         A shaper can contain zero or more materializers inside it.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information and examples.
///     </para>
/// </remarks>
public abstract class ShapedQueryCompilingExpressionVisitor : ExpressionVisitor
{
    private static readonly PropertyInfo CancellationTokenMemberInfo
        = typeof(QueryContext).GetRequiredProperty(nameof(QueryContext.CancellationToken));

    private readonly Expression _cancellationTokenParameter;
    private readonly EntityMaterializerInjectingExpressionVisitor _entityMaterializerInjectingExpressionVisitor;
    private readonly ConstantVerifyingExpressionVisitor _constantVerifyingExpressionVisitor;

    /// <summary>
    ///     Creates a new instance of the <see cref="ShapedQueryCompilingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this class.</param>
    /// <param name="queryCompilationContext">The query compilation context object to use.</param>
    protected ShapedQueryCompilingExpressionVisitor(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext)
    {
        Dependencies = dependencies;
        QueryCompilationContext = queryCompilationContext;

        _entityMaterializerInjectingExpressionVisitor =
            new EntityMaterializerInjectingExpressionVisitor(
                dependencies.EntityMaterializerSource,
                queryCompilationContext.QueryTrackingBehavior);

        _constantVerifyingExpressionVisitor = new ConstantVerifyingExpressionVisitor(dependencies.TypeMappingSource);

        if (queryCompilationContext.IsAsync)
        {
            _cancellationTokenParameter = Expression.MakeMemberAccess(
                QueryCompilationContext.QueryContextParameter,
                CancellationTokenMemberInfo);
        }
        else
        {
            _cancellationTokenParameter = null!;
        }
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ShapedQueryCompilingExpressionVisitorDependencies Dependencies { get; }

    /// <summary>
    ///     The query compilation context object for current compilation.
    /// </summary>
    protected virtual QueryCompilationContext QueryCompilationContext { get; }

    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
        {
            var serverEnumerable = VisitShapedQuery(shapedQueryExpression);
            switch (shapedQueryExpression.ResultCardinality)
            {
                case ResultCardinality.Enumerable:
                    return serverEnumerable;

                case ResultCardinality.Single:
                    return QueryCompilationContext.IsAsync
                        ? Expression.Call(
                            SingleAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable,
                            _cancellationTokenParameter)
                        : Expression.Call(
                            EnumerableMethods.SingleWithoutPredicate.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable);

                case ResultCardinality.SingleOrDefault:
                    return QueryCompilationContext.IsAsync
                        ? Expression.Call(
                            SingleOrDefaultAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable,
                            _cancellationTokenParameter)
                        : Expression.Call(
                            EnumerableMethods.SingleOrDefaultWithoutPredicate.MakeGenericMethod(
                                serverEnumerable.Type.GetSequenceType()),
                            serverEnumerable);
            }
        }

        return base.VisitExtension(extensionExpression);
    }

    private static readonly MethodInfo SingleAsyncMethodInfo
        = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(SingleAsync))
            .Single(mi => mi.GetParameters().Length == 2);

    private static readonly MethodInfo SingleOrDefaultAsyncMethodInfo
        = typeof(ShapedQueryCompilingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethods(nameof(SingleOrDefaultAsync))
            .Single(mi => mi.GetParameters().Length == 2);

    private static async Task<TSource> SingleAsync<TSource>(
        IAsyncEnumerable<TSource> asyncEnumerable,
        CancellationToken cancellationToken = default)
    {
        var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
        await using var _ = enumerator.ConfigureAwait(false);

        if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException(CoreStrings.SequenceContainsNoElements);
        }

        var result = enumerator.Current;

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException(CoreStrings.SequenceContainsMoreThanOneElement);
        }

        return result;
    }

    private static async Task<TSource?> SingleOrDefaultAsync<TSource>(
        IAsyncEnumerable<TSource> asyncEnumerable,
        CancellationToken cancellationToken = default)
    {
        var enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
        await using var _ = enumerator.ConfigureAwait(false);

        if (!(await enumerator.MoveNextAsync().ConfigureAwait(false)))
        {
            return default;
        }

        var result = enumerator.Current;

        if (await enumerator.MoveNextAsync().ConfigureAwait(false))
        {
            throw new InvalidOperationException(CoreStrings.SequenceContainsMoreThanOneElement);
        }

        return result;
    }

    /// <summary>
    ///     Visits given shaped query expression to create an expression of enumerable.
    /// </summary>
    /// <param name="shapedQueryExpression">The shaped query expression to compile.</param>
    /// <returns>An expression of enumerable.</returns>
    protected abstract Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression);

    /// <summary>
    ///     Inject entity materializers in given shaper expression. <see cref="EntityShaperExpression" /> is replaced with materializer
    ///     expression for given entity.
    /// </summary>
    /// <param name="expression">The expression to inject entity materializers.</param>
    /// <returns>A expression with entity materializers injected.</returns>
    protected virtual Expression InjectEntityMaterializers(Expression expression)
    {
        VerifyNoClientConstant(expression);

        return _entityMaterializerInjectingExpressionVisitor.Inject(expression);
    }

    /// <summary>
    ///     Verifies that the given shaper expression does not contain client side constant which could cause memory leak.
    /// </summary>
    /// <param name="expression">An expression to verify.</param>
    protected virtual void VerifyNoClientConstant(Expression expression)
        => _constantVerifyingExpressionVisitor.Visit(expression);

    private sealed class ConstantVerifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly ITypeMappingSource _typeMappingSource;

        public ConstantVerifyingExpressionVisitor(ITypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
        }

        private bool ValidConstant(ConstantExpression constantExpression)
            => constantExpression.Value == null
                || _typeMappingSource.FindMapping(constantExpression.Type) != null
                || constantExpression.Value is Array array
                && array.Length == 0;

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (!ValidConstant(constantExpression))
            {
                throw new InvalidOperationException(
                    CoreStrings.ClientProjectionCapturingConstantInTree(constantExpression.Type.DisplayName()));
            }

            return constantExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (RemoveConvert(methodCallExpression.Object) is ConstantExpression constantInstance
                && !ValidConstant(constantInstance))
            {
                throw new InvalidOperationException(
                    CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                        constantInstance.Type.DisplayName(),
                        methodCallExpression.Method.Name));
            }

            foreach (var argument in methodCallExpression.Arguments)
            {
                if (RemoveConvert(argument) is ConstantExpression constantArgument
                    && !ValidConstant(constantArgument))
                {
                    throw new InvalidOperationException(
                        CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                            constantArgument.Type.DisplayName(),
                            methodCallExpression.Method.Name));
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is EntityShaperExpression
                || extensionExpression is ProjectionBindingExpression
                    ? extensionExpression
                    : base.VisitExtension(extensionExpression);

        private static Expression? RemoveConvert(Expression? expression)
        {
            while (expression != null
                   && (expression.NodeType == ExpressionType.Convert
                       || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }
    }

    private sealed class EntityMaterializerInjectingExpressionVisitor : ExpressionVisitor
    {
        private static readonly ConstructorInfo MaterializationContextConstructor
            = typeof(MaterializationContext).GetConstructors().Single(ci => ci.GetParameters().Length == 2);

        private static readonly ConstructorInfo ValueBufferConstructor
            = typeof(ValueBuffer).GetTypeInfo().DeclaredConstructors.Single(ci => ci.GetParameters().Length == 1);

        private static readonly PropertyInfo DbContextMemberInfo
            = typeof(QueryContext).GetRequiredProperty(nameof(QueryContext.Context));

        private static readonly PropertyInfo EntityMemberInfo
            = typeof(InternalEntityEntry).GetRequiredProperty(nameof(InternalEntityEntry.Entity));

        private static readonly PropertyInfo EntityTypeMemberInfo
            = typeof(InternalEntityEntry).GetRequiredProperty(nameof(InternalEntityEntry.EntityType));

        private static readonly MethodInfo TryGetEntryMethodInfo
            = typeof(QueryContext).GetTypeInfo().GetDeclaredMethods(nameof(QueryContext.TryGetEntry))
                .Single(mi => mi.GetParameters().Length == 4);

        private static readonly MethodInfo StartTrackingMethodInfo
            = typeof(QueryContext).GetRequiredMethod(
                nameof(QueryContext.StartTracking), typeof(IEntityType), typeof(object), typeof(ValueBuffer));

        private static readonly MethodInfo CreateNullKeyValueInNoTrackingQueryMethod
            = typeof(EntityMaterializerInjectingExpressionVisitor).GetRequiredDeclaredMethod(
                nameof(CreateNullKeyValueInNoTrackingQuery));

        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly QueryTrackingBehavior _queryTrackingBehavior;
        private readonly bool _queryStateManager;
        private readonly ISet<IEntityType> _visitedEntityTypes = new HashSet<IEntityType>();
        private int _currentEntityIndex;

        public EntityMaterializerInjectingExpressionVisitor(
            IEntityMaterializerSource entityMaterializerSource,
            QueryTrackingBehavior queryTrackingBehavior)
        {
            _entityMaterializerSource = entityMaterializerSource;
            _queryTrackingBehavior = queryTrackingBehavior;
            _queryStateManager = queryTrackingBehavior == QueryTrackingBehavior.TrackAll
                || queryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution;
        }

        public Expression Inject(Expression expression)
        {
            var result = Visit(expression);
            if (_queryTrackingBehavior == QueryTrackingBehavior.TrackAll)
            {
                foreach (var entityType in _visitedEntityTypes)
                {
                    if (entityType.FindOwnership() is IForeignKey ownership
                        && !ContainsOwner(ownership.PrincipalEntityType))
                    {
                        throw new InvalidOperationException(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner);
                    }
                }

                bool ContainsOwner(IEntityType? owner)
                    => owner != null && (_visitedEntityTypes.Contains(owner) || ContainsOwner(owner.BaseType));
            }

            return result;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression is EntityShaperExpression entityShaperExpression
                ? ProcessEntityShaper(entityShaperExpression)
                : base.VisitExtension(extensionExpression);

        private Expression ProcessEntityShaper(EntityShaperExpression entityShaperExpression)
        {
            _currentEntityIndex++;

            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();

            var entityType = entityShaperExpression.EntityType;

            var materializationContextVariable = Expression.Variable(
                typeof(MaterializationContext),
                "materializationContext" + _currentEntityIndex);
            variables.Add(materializationContextVariable);
            expressions.Add(
                Expression.Assign(
                    materializationContextVariable,
                    Expression.New(
                        MaterializationContextConstructor,
                        entityShaperExpression.ValueBufferExpression,
                        Expression.MakeMemberAccess(
                            QueryCompilationContext.QueryContextParameter,
                            DbContextMemberInfo))));

            var valueBufferExpression = Expression.Call(materializationContextVariable, MaterializationContext.GetValueBufferMethod);

            var primaryKey = entityType.FindPrimaryKey();

            var concreteEntityTypeVariable = Expression.Variable(
                typeof(IEntityType),
                "entityType" + _currentEntityIndex);
            variables.Add(concreteEntityTypeVariable);

            var instanceVariable = Expression.Variable(entityType.ClrType, "instance" + _currentEntityIndex);
            variables.Add(instanceVariable);
            expressions.Add(
                Expression.Assign(
                    instanceVariable,
                    Expression.Constant(null, entityType.ClrType)));

            if (_queryStateManager
                && primaryKey != null)
            {
                var entryVariable = Expression.Variable(typeof(InternalEntityEntry), "entry" + _currentEntityIndex);
                var hasNullKeyVariable = Expression.Variable(typeof(bool), "hasNullKey" + _currentEntityIndex);
                variables.Add(entryVariable);
                variables.Add(hasNullKeyVariable);

                expressions.Add(
                    Expression.Assign(
                        entryVariable,
                        Expression.Call(
                            QueryCompilationContext.QueryContextParameter,
                            TryGetEntryMethodInfo,
                            Expression.Constant(primaryKey),
                            Expression.NewArrayInit(
                                typeof(object),
                                primaryKey.Properties
                                    .Select(
                                        p => valueBufferExpression.CreateValueBufferReadValueExpression(
                                            typeof(object),
                                            p.GetIndex(),
                                            p))),
                            Expression.Constant(!entityShaperExpression.IsNullable),
                            hasNullKeyVariable)));

                expressions.Add(
                    Expression.IfThen(
                        Expression.Not(hasNullKeyVariable),
                        Expression.IfThenElse(
                            Expression.NotEqual(
                                entryVariable,
                                Expression.Default(typeof(InternalEntityEntry))),
                            Expression.Block(
                                Expression.Assign(
                                    concreteEntityTypeVariable,
                                    Expression.MakeMemberAccess(entryVariable, EntityTypeMemberInfo)),
                                Expression.Assign(
                                    instanceVariable, Expression.Convert(
                                        Expression.MakeMemberAccess(entryVariable, EntityMemberInfo),
                                        entityType.ClrType))),
                            MaterializeEntity(
                                entityShaperExpression, materializationContextVariable, concreteEntityTypeVariable, instanceVariable,
                                entryVariable))));
            }
            else
            {
                if (primaryKey != null)
                {
                    if (entityShaperExpression.IsNullable)
                    {
                        expressions.Add(
                            Expression.IfThen(
                                primaryKey.Properties.Select(
                                        p => Expression.NotEqual(
                                            valueBufferExpression.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                            Expression.Constant(null)))
                                    .Aggregate((a, b) => Expression.AndAlso(a, b)),
                                MaterializeEntity(
                                    entityShaperExpression, materializationContextVariable, concreteEntityTypeVariable,
                                    instanceVariable,
                                    null)));
                    }
                    else
                    {
                        var keyValuesVariable = Expression.Variable(typeof(object[]), "keyValues" + _currentEntityIndex);
                        expressions.Add(
                            Expression.IfThenElse(
                                primaryKey.Properties.Select(
                                        p => Expression.NotEqual(
                                            valueBufferExpression.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                            Expression.Constant(null)))
                                    .Aggregate((a, b) => Expression.AndAlso(a, b)),
                                MaterializeEntity(
                                    entityShaperExpression, materializationContextVariable, concreteEntityTypeVariable,
                                    instanceVariable,
                                    null),
                                Expression.Block(
                                    new[] { keyValuesVariable },
                                    Expression.Assign(
                                        keyValuesVariable,
                                        Expression.NewArrayInit(
                                            typeof(object),
                                            primaryKey.Properties.Select(
                                                p => valueBufferExpression.CreateValueBufferReadValueExpression(
                                                    typeof(object), p.GetIndex(), p)))),
                                    Expression.Call(
                                        CreateNullKeyValueInNoTrackingQueryMethod,
                                        Expression.Constant(entityType),
                                        Expression.Constant(primaryKey.Properties),
                                        keyValuesVariable))));
                    }
                }
                else
                {
                    expressions.Add(
                        MaterializeEntity(
                            entityShaperExpression, materializationContextVariable, concreteEntityTypeVariable, instanceVariable,
                            null));
                }
            }

            expressions.Add(instanceVariable);
            return Expression.Block(variables, expressions);
        }

        private Expression MaterializeEntity(
            EntityShaperExpression entityShaperExpression,
            ParameterExpression materializationContextVariable,
            ParameterExpression concreteEntityTypeVariable,
            ParameterExpression instanceVariable,
            ParameterExpression? entryVariable)
        {
            var entityType = entityShaperExpression.EntityType;

            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();

            var shadowValuesVariable = Expression.Variable(
                typeof(ValueBuffer),
                "shadowValueBuffer" + _currentEntityIndex);
            variables.Add(shadowValuesVariable);
            expressions.Add(
                Expression.Assign(
                    shadowValuesVariable,
                    Expression.Constant(ValueBuffer.Empty)));

            var returnType = entityType.ClrType;
            var valueBufferExpression = Expression.Call(materializationContextVariable, MaterializationContext.GetValueBufferMethod);
            var expressionContext = (returnType, materializationContextVariable, concreteEntityTypeVariable, shadowValuesVariable);
            expressions.Add(
                Expression.Assign(
                    concreteEntityTypeVariable,
                    ReplacingExpressionVisitor.Replace(
                        entityShaperExpression.MaterializationCondition.Parameters[0],
                        valueBufferExpression,
                        entityShaperExpression.MaterializationCondition.Body)));

            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToArray();

            var switchCases = new SwitchCase[concreteEntityTypes.Length];
            for (var i = 0; i < concreteEntityTypes.Length; i++)
            {
                switchCases[i] = Expression.SwitchCase(
                    CreateFullMaterializeExpression(concreteEntityTypes[i], expressionContext),
                    Expression.Constant(concreteEntityTypes[i], typeof(IEntityType)));
            }

            var materializationExpression = Expression.Switch(
                concreteEntityTypeVariable,
                Expression.Constant(null, returnType),
                switchCases);

            expressions.Add(Expression.Assign(instanceVariable, materializationExpression));

            if (_queryStateManager
                && entityType.FindPrimaryKey() != null)
            {
                foreach (var et in entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive()))
                {
                    _visitedEntityTypes.Add(et);
                }

                expressions.Add(
                    Expression.Assign(
                        entryVariable!,
                        Expression.Condition(
                            Expression.Equal(concreteEntityTypeVariable, Expression.Default(typeof(IEntityType))),
                            Expression.Default(typeof(InternalEntityEntry)),
                            Expression.Call(
                                QueryCompilationContext.QueryContextParameter,
                                StartTrackingMethodInfo,
                                concreteEntityTypeVariable,
                                instanceVariable,
                                shadowValuesVariable))));
            }

            expressions.Add(instanceVariable);

            return Expression.Block(
                returnType,
                variables,
                expressions);
        }

        private BlockExpression CreateFullMaterializeExpression(
            IEntityType concreteEntityType,
            (Type ReturnType,
                ParameterExpression MaterializationContextVariable,
                ParameterExpression ConcreteEntityTypeVariable,
                ParameterExpression ShadowValuesVariable) materializeExpressionContext)
        {
            var (returnType,
                materializationContextVariable,
                _,
                shadowValuesVariable) = materializeExpressionContext;

            var blockExpressions = new List<Expression>(2);

            var materializer = _entityMaterializerSource
                .CreateMaterializeExpression(concreteEntityType, "instance", materializationContextVariable);

            if (_queryStateManager
                && concreteEntityType.ShadowPropertyCount() > 0)
            {
                var valueBufferExpression = Expression.Call(
                    materializationContextVariable, MaterializationContext.GetValueBufferMethod);
                var shadowProperties = concreteEntityType.GetProperties().Where(p => p.IsShadowProperty());

                blockExpressions.Add(
                    Expression.Assign(
                        shadowValuesVariable,
                        Expression.New(
                            ValueBufferConstructor,
                            Expression.NewArrayInit(
                                typeof(object),
                                shadowProperties.Select(
                                    p => valueBufferExpression.CreateValueBufferReadValueExpression(
                                        typeof(object), p.GetIndex(), p))))));
            }

            materializer = materializer.Type == returnType
                ? materializer
                : Expression.Convert(materializer, returnType);
            blockExpressions.Add(materializer);

            return Expression.Block(blockExpressions);
        }

        [UsedImplicitly]
        private static Exception CreateNullKeyValueInNoTrackingQuery(
            IEntityType entityType,
            IReadOnlyList<IProperty> properties,
            object?[] keyValues)
        {
            var index = -1;
            for (var i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i] == null)
                {
                    index = i;
                    break;
                }
            }

            var property = properties[index];

            throw new InvalidOperationException(
                CoreStrings.InvalidKeyValue(entityType.DisplayName(), property.Name));
        }
    }
}
