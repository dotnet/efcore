// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static System.Linq.Expressions.Expression;

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
///         and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
///     </para>
/// </remarks>
public abstract class ShapedQueryCompilingExpressionVisitor : ExpressionVisitor
{
    private static readonly PropertyInfo CancellationTokenMemberInfo
        = typeof(QueryContext).GetTypeInfo().GetProperty(nameof(QueryContext.CancellationToken))!;

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
            _cancellationTokenParameter = MakeMemberAccess(
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

            return shapedQueryExpression.ResultCardinality switch
            {
                ResultCardinality.Enumerable => serverEnumerable,

                ResultCardinality.Single => QueryCompilationContext.IsAsync
                    ? Call(
                        SingleAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                        serverEnumerable,
                        _cancellationTokenParameter)
                    : Call(
                        EnumerableMethods.SingleWithoutPredicate.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                        serverEnumerable),

                ResultCardinality.SingleOrDefault => QueryCompilationContext.IsAsync
                    ? Call(
                        SingleOrDefaultAsyncMethodInfo.MakeGenericMethod(serverEnumerable.Type.GetSequenceType()),
                        serverEnumerable,
                        _cancellationTokenParameter)
                    : Call(
                        EnumerableMethods.SingleOrDefaultWithoutPredicate.MakeGenericMethod(
                            serverEnumerable.Type.GetSequenceType()),
                        serverEnumerable),

                _ => base.VisitExtension(extensionExpression)
            };
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
    ///     Inject entity materializers in given shaper expression. <see cref="StructuralTypeShaperExpression" /> is replaced with materializer
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
                || constantExpression.Value is Array { Length: 0 };

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
            => extensionExpression is StructuralTypeShaperExpression or ProjectionBindingExpression
                ? extensionExpression
                : base.VisitExtension(extensionExpression);

        private static Expression? RemoveConvert(Expression? expression)
        {
            while (expression is { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked })
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

        private static readonly PropertyInfo DbContextMemberInfo
            = typeof(QueryContext).GetTypeInfo().GetProperty(nameof(QueryContext.Context))!;

        private static readonly PropertyInfo EntityMemberInfo
            = typeof(InternalEntityEntry).GetTypeInfo().GetProperty(nameof(InternalEntityEntry.Entity))!;

        private static readonly PropertyInfo EntityTypeMemberInfo
            = typeof(InternalEntityEntry).GetTypeInfo().GetProperty(nameof(InternalEntityEntry.EntityType))!;

        private static readonly MethodInfo TryGetEntryMethodInfo
            = typeof(QueryContext).GetTypeInfo().GetDeclaredMethods(nameof(QueryContext.TryGetEntry))
                .Single(mi => mi.GetParameters().Length == 4);

        private static readonly MethodInfo StartTrackingMethodInfo
            = typeof(QueryContext).GetMethod(
                nameof(QueryContext.StartTracking), [typeof(IEntityType), typeof(object), typeof(ISnapshot).MakeByRefType()])!;

        private static readonly MethodInfo CreateNullKeyValueInNoTrackingQueryMethod
            = typeof(EntityMaterializerInjectingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(CreateNullKeyValueInNoTrackingQuery))!;

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
            _queryStateManager =
                queryTrackingBehavior is QueryTrackingBehavior.TrackAll or QueryTrackingBehavior.NoTrackingWithIdentityResolution;
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
            => extensionExpression is StructuralTypeShaperExpression shaper
                ? ProcessEntityShaper(shaper)
                : base.VisitExtension(extensionExpression);

        private Expression ProcessEntityShaper(StructuralTypeShaperExpression shaper)
        {
            _currentEntityIndex++;

            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();

            var typeBase = shaper.StructuralType;
            var clrType = typeBase.ClrType;

            var materializationContextVariable = Variable(
                typeof(MaterializationContext),
                "materializationContext" + _currentEntityIndex);
            variables.Add(materializationContextVariable);
            expressions.Add(
                Assign(
                    materializationContextVariable,
                    New(
                        MaterializationContextConstructor,
                        shaper.ValueBufferExpression,
                        MakeMemberAccess(QueryCompilationContext.QueryContextParameter, DbContextMemberInfo))));

            var valueBufferExpression = Call(materializationContextVariable, MaterializationContext.GetValueBufferMethod);

            var primaryKey = typeBase is IEntityType entityType ? entityType.FindPrimaryKey() : null;

            var concreteEntityTypeVariable = Variable(
                typeBase is IEntityType ? typeof(IEntityType) : typeof(IComplexType),
                "entityType" + _currentEntityIndex);
            variables.Add(concreteEntityTypeVariable);

            var instanceVariable = Variable(clrType, "instance" + _currentEntityIndex);
            variables.Add(instanceVariable);
            expressions.Add(Assign(instanceVariable, Default(clrType)));

            if (_queryStateManager
                && primaryKey != null)
            {
                var entryVariable = Variable(typeof(InternalEntityEntry), "entry" + _currentEntityIndex);
                var hasNullKeyVariable = Variable(typeof(bool), "hasNullKey" + _currentEntityIndex);
                variables.Add(entryVariable);
                variables.Add(hasNullKeyVariable);

                expressions.Add(
                    Assign(
                        entryVariable,
                        Call(
                            QueryCompilationContext.QueryContextParameter,
                            TryGetEntryMethodInfo,
                            Constant(primaryKey),
                            NewArrayInit(
                                typeof(object),
                                primaryKey.Properties
                                    .Select(
                                        p => valueBufferExpression.CreateValueBufferReadValueExpression(
                                            typeof(object),
                                            p.GetIndex(),
                                            p))),
                            Constant(!shaper.IsNullable),
                            hasNullKeyVariable)));

                expressions.Add(
                    IfThen(
                        Not(hasNullKeyVariable),
                        IfThenElse(
                            NotEqual(entryVariable, Default(typeof(InternalEntityEntry))),
                            Block(
                                Assign(concreteEntityTypeVariable, MakeMemberAccess(entryVariable, EntityTypeMemberInfo)),
                                Assign(
                                    instanceVariable, Convert(
                                        MakeMemberAccess(entryVariable, EntityMemberInfo),
                                        clrType))),
                            MaterializeEntity(
                                shaper, materializationContextVariable, concreteEntityTypeVariable, instanceVariable,
                                entryVariable))));
            }
            else
            {
                if (primaryKey != null)
                {
                    if (shaper.IsNullable)
                    {
                        expressions.Add(
                            IfThen(
                                primaryKey.Properties.Select(
                                        p => NotEqual(
                                            valueBufferExpression.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                            Constant(null)))
                                    .Aggregate(AndAlso),
                                MaterializeEntity(
                                    shaper, materializationContextVariable, concreteEntityTypeVariable,
                                    instanceVariable,
                                    null)));
                    }
                    else
                    {
                        var keyValuesVariable = Variable(typeof(object[]), "keyValues" + _currentEntityIndex);
                        expressions.Add(
                            IfThenElse(
                                primaryKey.Properties.Select(
                                        p => NotEqual(
                                            valueBufferExpression.CreateValueBufferReadValueExpression(typeof(object), p.GetIndex(), p),
                                            Constant(null)))
                                    .Aggregate(AndAlso),
                                MaterializeEntity(
                                    shaper, materializationContextVariable, concreteEntityTypeVariable,
                                    instanceVariable,
                                    null),
                                Block(
                                    new[] { keyValuesVariable },
                                    Assign(
                                        keyValuesVariable,
                                        NewArrayInit(
                                            typeof(object),
                                            primaryKey.Properties.Select(
                                                p => valueBufferExpression.CreateValueBufferReadValueExpression(
                                                    typeof(object), p.GetIndex(), p)))),
                                    Call(
                                        CreateNullKeyValueInNoTrackingQueryMethod,
                                        Constant(typeBase),
                                        Constant(primaryKey.Properties),
                                        keyValuesVariable))));
                    }
                }
                else
                {
                    expressions.Add(
                        MaterializeEntity(
                            shaper, materializationContextVariable, concreteEntityTypeVariable, instanceVariable,
                            null));
                }
            }

            expressions.Add(instanceVariable);
            return Block(variables, expressions);
        }

        private Expression MaterializeEntity(
            StructuralTypeShaperExpression shaper,
            ParameterExpression materializationContextVariable,
            ParameterExpression concreteEntityTypeVariable,
            ParameterExpression instanceVariable,
            ParameterExpression? entryVariable)
        {
            var typeBase = shaper.StructuralType;

            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();

            var shadowValuesVariable = Variable(
                typeof(ISnapshot),
                "shadowSnapshot" + _currentEntityIndex);
            variables.Add(shadowValuesVariable);
            expressions.Add(
                Assign(
                    shadowValuesVariable,
                    Constant(Snapshot.Empty)));

            var returnType = typeBase.ClrType;
            var valueBufferExpression = Call(materializationContextVariable, MaterializationContext.GetValueBufferMethod);
            var expressionContext = (returnType, materializationContextVariable, concreteEntityTypeVariable, shadowValuesVariable);
            expressions.Add(
                Assign(
                    concreteEntityTypeVariable,
                    ReplacingExpressionVisitor.Replace(
                        shaper.MaterializationCondition.Parameters[0],
                        valueBufferExpression,
                        shaper.MaterializationCondition.Body)));

            var (primaryKey, concreteEntityTypes) = typeBase is IEntityType entityType
                ? (entityType.FindPrimaryKey(), entityType.GetConcreteDerivedTypesInclusive().Cast<ITypeBase>().ToArray())
                : (null, [typeBase]);

            var switchCases = new SwitchCase[concreteEntityTypes.Length];
            for (var i = 0; i < concreteEntityTypes.Length; i++)
            {
                switchCases[i] = SwitchCase(
                    CreateFullMaterializeExpression(concreteEntityTypes[i], expressionContext),
                    Constant(concreteEntityTypes[i], typeBase is IEntityType ? typeof(IEntityType) : typeof(IComplexType)));
            }

            var materializationExpression = Switch(
                concreteEntityTypeVariable,
                Default(returnType),
                switchCases);

            expressions.Add(Assign(instanceVariable, materializationExpression));

            if (_queryStateManager && primaryKey is not null)
            {
                if (typeBase is IEntityType entityType2)
                {
                    foreach (var et in entityType2.GetAllBaseTypes().Concat(entityType2.GetDerivedTypesInclusive()))
                    {
                        _visitedEntityTypes.Add(et);
                    }
                }

                expressions.Add(
                    Assign(
                        entryVariable!,
                        Condition(
                            Equal(concreteEntityTypeVariable, Default(typeof(IEntityType))),
                            Default(typeof(InternalEntityEntry)),
                            Call(
                                QueryCompilationContext.QueryContextParameter,
                                StartTrackingMethodInfo,
                                concreteEntityTypeVariable,
                                instanceVariable,
                                shadowValuesVariable))));
            }

            expressions.Add(instanceVariable);

            return Block(
                returnType,
                variables,
                expressions);
        }

        private BlockExpression CreateFullMaterializeExpression(
            ITypeBase concreteTypeBase,
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
                .CreateMaterializeExpression(
                    new EntityMaterializerSourceParameters(
                        concreteTypeBase, "instance", _queryTrackingBehavior), materializationContextVariable);

            // TODO: Properly support shadow properties for complex types?
            if (_queryStateManager
                && concreteTypeBase is IRuntimeEntityType { ShadowPropertyCount: > 0 } runtimeEntityType)
            {
                var valueBufferExpression = Call(
                    materializationContextVariable, MaterializationContext.GetValueBufferMethod);

                var shadowProperties = ((IEnumerable<IPropertyBase>)runtimeEntityType.GetProperties())
                    .Concat(runtimeEntityType.GetNavigations())
                    .Concat(runtimeEntityType.GetSkipNavigations())
                    .Where(n => n.IsShadowProperty())
                    .OrderBy(e => e.GetShadowIndex());

                blockExpressions.Add(
                    Assign(
                        shadowValuesVariable,
                        ShadowValuesFactoryFactory.Instance.CreateConstructorExpression(runtimeEntityType,
                            NewArrayInit(
                                typeof(object),
                                shadowProperties.Select(p =>
                                    Convert(valueBufferExpression.CreateValueBufferReadValueExpression(
                                        p.ClrType, p.GetIndex(), p), typeof(object)))))));
            }

            materializer = materializer.Type == returnType
                ? materializer
                : Convert(materializer, returnType);
            blockExpressions.Add(materializer);

            return Block(blockExpressions);
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
