// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.

// @TODO: What to do on null keys? -> Missing
public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed partial class ShaperProcessingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo StartTrackingMethodInfo
            = typeof(QueryContext).GetMethod(
                nameof(QueryContext.StartTracking), [typeof(IEntityType), typeof(object), typeof(ISnapshot).MakeByRefType()])!;

        private static readonly MethodInfo CollectionAccessorCreateMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Create))!;

        private static readonly MethodInfo CollectionAccessorGetOrCreateMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate))!;

        private static readonly MethodInfo CollectionAccessorAddStandaloneMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.AddStandalone)) ?? throw new UnreachableException();

        private static readonly ConstructorInfo JsonReaderDataConstructor
            = typeof(JsonReaderData).GetConstructor([typeof(ReadOnlyMemory<byte>)])!;

        private static readonly PropertyInfo JsonReaderDataBytesConsumedProperty
            = typeof(JsonReaderData).GetProperty(nameof(JsonReaderData.BytesConsumed)) ?? throw new UnreachableException();

        private static readonly ConstructorInfo JsonReaderManagerConstructor
            = typeof(Utf8JsonReaderManager).GetConstructor(
                [typeof(JsonReaderData), typeof(IDiagnosticsLogger<DbLoggerCategory.Query>)])!;

        private static readonly MethodInfo Utf8JsonReaderManagerMoveNextMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.MoveNext), [])!;

        private static readonly MethodInfo Utf8JsonReaderManagerCaptureStateMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.CaptureState), [])!;

        private static readonly FieldInfo Utf8JsonReaderManagerCurrentReaderField
            = typeof(Utf8JsonReaderManager).GetField(nameof(Utf8JsonReaderManager.CurrentReader))!;

        private static readonly MethodInfo Utf8JsonReaderManagerSkipMethod
            = typeof(Utf8JsonReaderManager).GetMethod(nameof(Utf8JsonReaderManager.Skip), [])!;

        private static readonly MethodInfo Utf8JsonReaderValueTextEqualsMethod
            = typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.ValueTextEquals), [typeof(ReadOnlySpan<byte>)])!;

        private static readonly ConstructorInfo Utf8JsonReaderConstructor
            = typeof(Utf8JsonReader).GetConstructor([typeof(ReadOnlySpan<byte>), typeof(bool), typeof(JsonReaderState)]) ?? throw new UnreachableException();

        private static readonly MethodInfo Utf8JsonReaderReadMethod
            = typeof(Utf8JsonReader).GetMethod(nameof(Utf8JsonReader.Read), [])!;

        private static readonly PropertyInfo Utf8JsonReaderBytesConsumedProperty
            = typeof(Utf8JsonReader).GetProperty(nameof(Utf8JsonReader.BytesConsumed)) ?? throw new UnreachableException();

        private static readonly PropertyInfo Utf8JsonReaderStateProperty
            = typeof(Utf8JsonReader).GetProperty(nameof(Utf8JsonReader.CurrentState)) ?? throw new UnreachableException();

        private static readonly PropertyInfo ReadOnlyMemorySpanProperty
            = typeof(ReadOnlyMemory<byte>).GetProperty(nameof(ReadOnlyMemory<>.Span)) ?? throw new UnreachableException();

        private static readonly MethodInfo ReadOnlyMemorySliceMethod
            = typeof(ReadOnlyMemory<byte>).GetMethod(nameof(ReadOnlyMemory<>.Slice), [typeof(int)]) ?? throw new UnreachableException();

        private static FieldInfo MaterializerTupleEntityTypeField(Type tupleType) => tupleType.GetField(nameof(ValueTuple<,,,>.Item1))!;
        private static FieldInfo MaterializerTupleInstanceField(Type tupleType) => tupleType.GetField(nameof(ValueTuple<,,,>.Item2))!;
        private static FieldInfo MaterializerTupleShadowSnapshotField(Type tupleType) => tupleType.GetField(nameof(ValueTuple<,,,>.Item3))!;
        private static FieldInfo MaterializerTupleTrackingActionsField(Type tupleType) => tupleType.GetField(nameof(ValueTuple<,,,>.Item4))!;

        private static readonly ConstructorInfo InvalidOperationExceptionConstructor
            = typeof(InvalidOperationException).GetConstructor([typeof(string)]) ?? throw new UnreachableException();

        private static readonly MethodInfo ByteArrayAsSpanMethod = typeof(MemoryExtensions).GetMethods()
            .Where(x => x.Name == nameof(MemoryExtensions.AsSpan) && x.GetGenericArguments().Count() == 1)
            .Select(x => new { x, prms = x.GetParameters() })
            .Single(x => x.prms.Count() == 1 && x.prms[0].ParameterType.IsArray).x.MakeGenericMethod(typeof(byte));

        private static readonly PropertyInfo Utf8JsonReaderTokenTypeProperty
            = typeof(Utf8JsonReader).GetProperty(nameof(Utf8JsonReader.TokenType))!;

        private static readonly MethodInfo PropertyGetJsonValueReaderWriterMethod =
            typeof(IReadOnlyProperty).GetMethod(nameof(IReadOnlyProperty.GetJsonValueReaderWriter), [])!;

        private static readonly MethodInfo PropertyGetTypeMappingMethod =
            typeof(IReadOnlyProperty).GetMethod(nameof(IReadOnlyProperty.GetTypeMapping), [])!;

        private static readonly PropertyInfo QueryContextQueryLoggerProperty =
            typeof(QueryContext).GetProperty(nameof(QueryContext.QueryLogger))!;

        private static readonly MethodInfo TryGetEntryMethod =
            typeof(QueryContext).GetMethod(nameof(QueryContext.TryGetEntry)) ?? throw new UnreachableException();

        private readonly Dictionary<ITypeBase, LambdaExpression>
            _structuralTypeJsonShaperLambdaMapping = [];

        private readonly Dictionary<ITypeBase, LambdaExpression>
            _structuralTypeJsonMaterializerLambdaMapping = [];

        private readonly Dictionary<ProjectionBindingExpression, (ParameterExpression Variable, Expression Shaper)>
            _deferredProjectionBindings = [];

        private readonly Dictionary<IEntityType, (
            ParameterExpression InstanceVariable,
            ParameterExpression? ShadowSnapshotVariable,
            ParameterExpression TrackingActionsVariable,
            BinaryExpression TryGetEntryAssignment)> _entityTypeMaterializerExpressionsMapping = [];

        private readonly ParameterExpression _jsonReaderDataParameter = Parameter(typeof(JsonReaderData), "jsonReaderData");
        private readonly ParameterExpression _jsonReaderManagerVariable = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
        private readonly ParameterExpression _innerShaperBytesConsumedVariable = Variable(typeof(int), "innerShaperBytesConsumed");
        private readonly ParameterExpression _ordinalParameter = Variable(typeof(int), "ordinal");

        private readonly CosmosShapedQueryCompilingExpressionVisitor _parentVisitor;
        private readonly SelectExpression _selectExpression;
        private readonly ParameterExpression _dataParameter;
        private readonly ParameterExpression _bytesConsumedParameter;

        private readonly bool _isTracking;
        private readonly bool _queryStateManager;

        public ShaperProcessingExpressionVisitor(
            CosmosShapedQueryCompilingExpressionVisitor parentVisitor,
            SelectExpression selectExpression,
            ParameterExpression dataParameter,
            ParameterExpression bytesConsumedParameter)
        {
            _parentVisitor = parentVisitor;
            _selectExpression = selectExpression;
            _dataParameter = dataParameter;
            _bytesConsumedParameter = bytesConsumedParameter;

            _isTracking = parentVisitor.QueryCompilationContext.QueryTrackingBehavior == QueryTrackingBehavior.TrackAll;
            _queryStateManager = parentVisitor.QueryCompilationContext.QueryTrackingBehavior is QueryTrackingBehavior.TrackAll
                or QueryTrackingBehavior.NoTrackingWithIdentityResolution;
        }

        public LambdaExpression ProcessShaper(Expression shaperExpression)
        {
            var processedShaperExpression = Visit(shaperExpression);

            if (_deferredProjectionBindings.Count == 0)
            {
                var resultVariable = Variable(processedShaperExpression.Type, "result");
                processedShaperExpression = Block(
                    [_jsonReaderDataParameter, resultVariable, _innerShaperBytesConsumedVariable],
                    // bytesConsumed = 0
                    [Assign(_bytesConsumedParameter, Constant(0)),
                    // result = shaper
                    Assign(resultVariable, processedShaperExpression),
                    // bytesConsumed += innerShaperBytesConsumed
                    AddBytesConsumedExpressions(_innerShaperBytesConsumedVariable)[0],
                    // return result
                    resultVariable]);
            }
            else
            {
                // We read the projections in the order they are defined in from the document and pass the sub data to the shaper
                var tokenTypeVariable = Parameter(typeof(JsonTokenType), "tokenType");
                var jsonReaderVariable = Parameter(typeof(Utf8JsonReader), "jsonReader");
                var afterStartObjectReaderStateVariable = Parameter(typeof(JsonReaderState), "afterStartObjectReaderState");

                var shaperBlockVariables = new List<ParameterExpression>(_deferredProjectionBindings.Values.Select(x => x.Variable))
                {
                    _jsonReaderDataParameter,
                    tokenTypeVariable,
                    jsonReaderVariable,
                    afterStartObjectReaderStateVariable,
                    _innerShaperBytesConsumedVariable
                };
                var shaperBlockExpressions = new List<Expression>()
                {
                    // bytesConsumed = 0
                    Assign(_bytesConsumedParameter, Constant(0)),
                    // jsonReader = new Utf8JsonReader(data.Span, isFinalBlock: true, state: default)
                    AssignJsonReaderVariableExpression(),
                    // jsonReader.Read() // Reads StartObject (always present)
                    Call(jsonReaderVariable, Utf8JsonReaderReadMethod),
                    // if (jsonReader.TokenType != JsonTokenType.StartObject) throw new InvalidOperationException(InvalidTokenType);
                    IfThen(
                        NotEqual(Property(jsonReaderVariable, Utf8JsonReaderTokenTypeProperty), Constant(JsonTokenType.StartObject)),
                        Throw(Call(NewJsonReaderInvalidTokenTypeExceptionMethodInfo, Property(jsonReaderVariable, Utf8JsonReaderTokenTypeProperty)))
                    ),
                    // afterStartObjectReaderState = jsonReader.CurrentState // Store the state of after start object to be able to continue reading properties later on
                    Assign(afterStartObjectReaderStateVariable, Property(jsonReaderVariable, Utf8JsonReaderStateProperty))
                };

                BinaryExpression AssignJsonReaderVariableExpression(Expression? jsonReaderState = null)
                    => Assign(jsonReaderVariable, New(Utf8JsonReaderConstructor, Property(_dataParameter, ReadOnlyMemorySpanProperty), Constant(true), jsonReaderState ?? Default(typeof(JsonReaderState))));

                var groups = _deferredProjectionBindings.GroupBy(x => GetProjectionIndex(x.Key));

                foreach (var group in groups.OrderBy(x => x.Key))
                {
                    var projectionReadExpressions = new List<Expression>();

                    projectionReadExpressions.AddRange([
                        // bytesConsumed += jsonReader.BytesConsumed
                        // data = data.Slice((int)jsonReader.BytesConsumed) // Slice the data to the start json value being deserialized
                        .. AddBytesConsumedExpressions(Convert(Property(jsonReaderVariable, Utf8JsonReaderBytesConsumedProperty), typeof(int))),
                    ]);

                    string? alias = null;
                    // Deserialize the json value the amount of times there are projections for this json value, and assign the result to the variable for each projection.
                    foreach (var (projectionBindingExpression, (variable, innerShaper)) in group)
                    {
                        alias ??= GetProjection(projectionBindingExpression).Alias;
                        projectionReadExpressions.Add(
                            // variable = innerShaper
                            Assign(variable, innerShaper));
                    }
                    Debug.Assert(alias != null, "There is always one item in a group");

                    var nextItemBytesConsumedVariable = Variable(typeof(int), "nextItemBytesConsumed");
                    projectionReadExpressions.AddRange([
                        // bytesConsumed += innerShaperBytesConsumed
                        // data = data.Slice(innerShaperBytesConsumed) // innerShaperBytesConsumed is set by every inner shaper, slice the data to the end of the json value that was deserialzed
                        ..AddBytesConsumedExpressions(_innerShaperBytesConsumedVariable),
                        // Slice the possible ',' after the last json value
                        Block(
                            [nextItemBytesConsumedVariable],
                            // data = SliceNextItemTokenMethodInfo(data, out nextItemBytesConsumed)
                            [Assign(_dataParameter, Call(SliceNextItemTokenMethodInfo, _dataParameter, nextItemBytesConsumedVariable)),
                            // nextItemBytesConsumed += nextItemBytesConsumedVariable
                            AddAssignChecked(_bytesConsumedParameter, nextItemBytesConsumedVariable)]),
                    ]);

                    // Only read the property if the property name matches the alias of the projection, otherwise the property is undefined and we will continue to the next property.
                    shaperBlockExpressions.AddRange(
                            // jsonReader.Read()
                            Call(jsonReaderVariable, Utf8JsonReaderReadMethod),
                            // if (jsonReader.TokenType == JsonTokenType.PropertyName)
                            IfThen(AndAlso(Equal(Property(jsonReaderVariable, Utf8JsonReaderTokenTypeProperty), Constant(JsonTokenType.PropertyName)), JsonReaderValueTextEquals(jsonReaderVariable, alias)),
                                Block(projectionReadExpressions)),
                        // jsonReader = new Utf8JsonReader(data.Span, isFinalBlock: true, state: afterStartObjectReaderState) // Create a new json reader to continue reading the document,
                        // using the state from when we started reading the first property
                        AssignJsonReaderVariableExpression(afterStartObjectReaderStateVariable));
                }

                shaperBlockExpressions.AddRange(
                    // jsonReader.Read() // Reads EndObject
                    Call(jsonReaderVariable, Utf8JsonReaderReadMethod),
                    // bytesConsumed += jsonReader.BytesConsumed
                    AddBytesConsumedExpressions(Convert(Property(jsonReaderVariable, Utf8JsonReaderBytesConsumedProperty), typeof(int)))[0]);

                shaperBlockExpressions.Add(processedShaperExpression);
                processedShaperExpression = Block(shaperBlockVariables, shaperBlockExpressions);
            }

            Expression[] AddBytesConsumedExpressions(Expression bytesConsumedExpression) =>
            [
                AddAssignChecked(_bytesConsumedParameter, bytesConsumedExpression),
                Assign(_dataParameter, Call(_dataParameter, ReadOnlyMemorySliceMethod, bytesConsumedExpression))
            ];


            var shaperLambda = Lambda(
                typeof(Shaper<>).MakeGenericType(shaperExpression.Type),
                processedShaperExpression,
                "Shaper",
                [QueryCompilationContext.QueryContextParameter,
                _dataParameter,
                _ordinalParameter,
                _bytesConsumedParameter]);

            return shaperLambda;
        }
        
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            // Every deserialize action sets innerShaperBytesConsumed, so it can be used by the parent shaper to slice the data.
            var resultVariable = Variable(extensionExpression.Type, "result");
            switch (extensionExpression)
            {
                case StructuralTypeShaperExpression structuralTypeShaperExpression:
                {
                    var shaperLambda = StructuralTypeJsonShaperLambda(structuralTypeShaperExpression);

                    var shaper = Block(
                        [resultVariable],
                        [Assign(_jsonReaderDataParameter, New(JsonReaderDataConstructor, _dataParameter)),
                        Assign(resultVariable, Invoke(shaperLambda, GetParametersForLambda(structuralTypeShaperExpression.StructuralType))),
                        Assign(_innerShaperBytesConsumedVariable, Property(_jsonReaderDataParameter, JsonReaderDataBytesConsumedProperty)),
                        resultVariable]);

                    if (structuralTypeShaperExpression.ValueBufferExpression is ProjectionBindingExpression projectionBinding) // Otherwise this is an inner shaper of a CollectionShaperExpression,
                    {
                        return CheckDeferProjectionBinding(projectionBinding, shaper);
                    }

                    return shaper;
                }

                case ProjectionBindingExpression projectionBindingExpression:
                {
                    var projection = GetProjection(projectionBindingExpression);
                    var typeMapping = ((SqlExpression)projection.Expression).TypeMapping!;
                    var valueJsonReaderManager = Variable(typeof(Utf8JsonReaderManager), "valueJsonReaderManager");

                    var shaper = Block(
                        [resultVariable, valueJsonReaderManager],
                        [
                            Assign(_jsonReaderDataParameter, New(JsonReaderDataConstructor, _dataParameter)),
                            Assign(valueJsonReaderManager, NewJsonReaderManager()),
                            Call(valueJsonReaderManager, Utf8JsonReaderManagerMoveNextMethod),
                            Assign(
                                resultVariable,
                                CreateReadJsonValueExpression(valueJsonReaderManager, typeMapping, resultVariable.Type)),
                            Call(valueJsonReaderManager, Utf8JsonReaderManagerCaptureStateMethod),
                            Assign(_innerShaperBytesConsumedVariable, Property(_jsonReaderDataParameter, JsonReaderDataBytesConsumedProperty)),
                            resultVariable]);

                    return CheckDeferProjectionBinding(projectionBindingExpression, shaper);
                }

                case CollectionShaperExpression collectionShaperExpression:
                {
                    var innerShaperLambda = ProcessShaper(collectionShaperExpression.InnerShaper);
                    var collectionBytesConsumedVariable = Variable(typeof(int), "collectionBytesConsumed");

                    var shaper = Block(
                        [resultVariable, collectionBytesConsumedVariable],
                        [Assign(resultVariable,
                            Call(
                                ShapeCollectionMethodInfo.MakeGenericMethod(collectionShaperExpression.ElementType, collectionShaperExpression.Type),
                                QueryCompilationContext.QueryContextParameter,
                                _dataParameter,
                                Constant(collectionShaperExpression.CollectionCreator),
                                innerShaperLambda,
                                collectionBytesConsumedVariable)),
                        Assign(_innerShaperBytesConsumedVariable, collectionBytesConsumedVariable),
                        resultVariable]);

                    var projectionBinding = (ProjectionBindingExpression)collectionShaperExpression.Projection;
                    return CheckDeferProjectionBinding(projectionBinding, shaper);
                }

                case IncludeExpression includeExpression:
                    return Visit(includeExpression.EntityExpression);
            }

            Expression CheckDeferProjectionBinding(ProjectionBindingExpression projectionBinding, Expression shaper)
            {
                var projection = GetProjection(projectionBinding);
                if (!projection.IsValueProjection && projection.Alias != null) // There are multiple projections in the query result, so we have to defer the shaper to a variable assignment and run it when we are reading the projection from the result. See: ProcessShaper
                {
                    if (!_deferredProjectionBindings.TryGetValue(projectionBinding, out var deferredBinding))
                    {
                        deferredBinding = (Variable(shaper.Type, projection.Alias), shaper);
                        _deferredProjectionBindings[projectionBinding] = deferredBinding;
                    }
                    
                    return deferredBinding.Variable;
                }

                return shaper;
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            // Converts valueBuffer.TryReadValue to jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
            if (IsValueBufferTryReadValueMethodCall(methodCallExpression, out var property))
            {
                var jsonReadPropertyValueExpression = ReadJsonPropertyValue(property);

                return ConvertIfNotMatch(jsonReadPropertyValueExpression, methodCallExpression.Type);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
            => memberExpression.Member == typeof(MaterializationContext).GetProperty(nameof(MaterializationContext.Context))
                ? MakeMemberAccess(QueryCompilationContext.QueryContextParameter, typeof(QueryContext).GetProperty(nameof(QueryContext.Context))!)
                : base.VisitMember(memberExpression);

        private LambdaExpression StructuralTypeJsonShaperLambda(StructuralTypeShaperExpression shaper)
        {
            if (_structuralTypeJsonShaperLambdaMapping.TryGetValue(shaper.StructuralType, out var lambda))
            {
                return lambda;
            }

            var materializer = StructuralTypeJsonMaterializerLambda(
                shaper.StructuralType,
                shaper.IsNullable);

            if (!RequiresTracking(shaper.StructuralType, out var entityType))
            {
                return materializer;
            }

            var tupleVariable = Variable(materializer.Body.Type);
            var shaperVariables = new List<ParameterExpression>
            {
                tupleVariable
            };

            var shaperExpressions = new List<Expression>()
            {
                Assign(tupleVariable, Invoke(materializer, GetParametersForLambda(entityType)))
            };

            // var (entityType, instance, shadowSnapshot, trackingActions) = MaterializeRootEntity(queryContext, jsonReaderData);
            var entityTypeVariable = Field(tupleVariable, MaterializerTupleEntityTypeField(materializer.Body.Type));
            var instanceVariable = Field(tupleVariable, MaterializerTupleInstanceField(materializer.Body.Type));
            var shadowSnapshotVariable = Field(tupleVariable, MaterializerTupleShadowSnapshotField(materializer.Body.Type));
            var trackingActionsVariable = Field(tupleVariable, MaterializerTupleTrackingActionsField(materializer.Body.Type));

            var tryGetEntryAssignment = GetEntityTypeMaterializerExpressions(entityType).TryGetEntryAssignment;
            var entryVariable = (ParameterExpression)tryGetEntryAssignment.Left;
            var hasNullKeyVariable = (ParameterExpression)((MethodCallExpression)tryGetEntryAssignment.Right).Arguments[3];
            shaperVariables.Add(entryVariable);
            shaperVariables.Add(hasNullKeyVariable);

            var tryGetEntryPropertyMap = entityType.FindPrimaryKey()!.Properties
                .ToDictionary<IProperty, IProperty, Expression>(
                    p => p,
                    property =>
                    {
                        if (entityType.IsOwned() && property.IsOrdinalKeyProperty())
                        {
                            return _ordinalParameter;
                        }

                        // @TODO: AsNoTrackingWithIdentityResolution...

                        if (property.IsShadowProperty())
                        {
                            // shadowSnapshotVariable.GetValue<T>(1)
                            return Call(shadowSnapshotVariable, Snapshot.GetValueMethod.MakeGenericMethod(property.ClrType), Constant(property.GetShadowIndex()));
                        }

                        return instanceVariable.MakeMemberAccess(property.GetMemberInfo(true, false));
                    });

            // var entry = queryContext.TryGetEntry(entityType, new object[] { instance.Id }, true, out var _);
            tryGetEntryAssignment = (BinaryExpression)new ValueBufferTryReadValueMethodsReplacer(tryGetEntryPropertyMap)
                .Visit(tryGetEntryAssignment);

            shaperExpressions.Add(tryGetEntryAssignment);

            //if (entry != default)
            //{
            //    instance = entry.Entity;
            //}
            //else
            //{
            //    foreach (var action in trackingActions)
            //    {
            //        action();
            //    }
            //    queryContext.StartTracking(entityType, instance, shadowSnapshot);
            //}
            shaperExpressions.Add(IfThenElse(
                NotEqual(entryVariable, Default(entryVariable.Type)),
                Assign(instanceVariable, Convert(MakeMemberAccess(entryVariable, typeof(InternalEntityEntry).GetProperty(nameof(InternalEntityEntry.Entity))!), instanceVariable.Type)),
                Block(
                    ForEach(trackingActionsVariable, (trackingAction) => Invoke(trackingAction)),
                    Call(QueryCompilationContext.QueryContextParameter, StartTrackingMethodInfo, entityTypeVariable, instanceVariable, shadowSnapshotVariable))));

            // return instance
            shaperExpressions.Add(instanceVariable);

            lambda = Lambda(
                Block(shaperVariables, shaperExpressions),
                entityType.Name + "_Shaper",
                GetParametersForLambda(entityType));
            _structuralTypeJsonShaperLambdaMapping.Add(entityType, lambda);

            return lambda;
        }

        private LambdaExpression StructuralTypeJsonMaterializerLambda(
            ITypeBase structuralType,
            bool nullable)
        {
            if (_structuralTypeJsonMaterializerLambdaMapping.TryGetValue(structuralType, out var lambda))
            {
                return lambda;
            }

            var materializerVariables = new List<ParameterExpression>() { _jsonReaderManagerVariable };
            var materializerExpressions = new List<Expression>();

            var structuralTypeShaperExpression = new StructuralTypeShaperExpression(
                structuralType,
                Default(typeof(ValueBuffer)),
                nullable);

            var materializerBlock =
                (BlockExpression)_parentVisitor.InjectStructuralTypeMaterializers(structuralTypeShaperExpression);

            //  Rewrite ordinal key properties
            if (structuralType.TryGetOrdinalKey(out var ordinalKeyProperty))
            {
                materializerBlock = new ValueBufferTryReadValueMethodsReplacer(new Dictionary<IProperty, Expression>
                {
                    { ordinalKeyProperty, _ordinalParameter }
                }).Rewrite(materializerBlock);
            }

            // We can't know owned principal properties until we have fully deserialized the parent.
            // We rewrite assignments to principal properties here, to be assigned after the parent is fully deserialized and tracked.
            // See GenerateJsonPropertyReadLoop nested structural properties for the replacement of these values.
            if (structuralType is IEntityType ownedEntityType && ownedEntityType.IsOwned())
            {
                var principalPropertyDefaultReplacements = ownedEntityType.GetDerivedTypesInclusive()
                    .SelectMany(x => x.GetProperties().Where(p => p.FindFirstPrincipal() != null && !p.IsPersisted())).Distinct()
                    .ToDictionary(x => x, p => (Expression)Default(p.ClrType));
                materializerBlock = new ValueBufferTryReadValueMethodsReplacer(principalPropertyDefaultReplacements).Rewrite(materializerBlock);
            }

            var discriminatorProperty = structuralType.FindDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                // @TODO: Optimize for only 1 possible value, do discriminator value correct check when finding property instead of scanning document.
                //if (structuralType.GetDerivedTypesInclusive().Count() == 1)
                //{
                //}

                // We have to read the json document to find the discriminator value before we can know how to deserialize the document.
                var discriminatorValueVariable = Variable(discriminatorProperty.ClrType.MakeNullable(), "discriminatorValue"); // Not a local variable, but defined by the parent block.
                materializerVariables.Add(discriminatorValueVariable);
                var discriminatorRead = ReadDiscriminator(structuralType, discriminatorProperty, discriminatorValueVariable);
                materializerExpressions.Add(discriminatorRead);

                // Replace calls for ValueBufferTryReadValue for the discriminator property
                materializerBlock = new ValueBufferTryReadValueMethodsReplacer(new Dictionary<IProperty, Expression>
                {
                    { discriminatorProperty, discriminatorValueVariable }
                }).Rewrite(materializerBlock);
            }

            var instanceVariable = (ParameterExpression)materializerBlock.Expressions[^1];
            var entityTypeVariable = materializerBlock.Variables.Single(x => x.Type.IsAssignableTo(typeof(ITypeBase)));
            materializerVariables.AddRange(instanceVariable, entityTypeVariable);

            var trackingActions = Variable(typeof(List<Action>), "trackingActions");
            if (RequiresTracking(structuralType, out var entityType))
            {
                materializerVariables.Add(trackingActions);
                materializerExpressions.Add(Assign(trackingActions, New(typeof(List<Action>))));

                // We can't do tracking till after the entity is materialized
                // So we remove the tracking code from the materializer block and store it for later use
                var entryVariable = materializerBlock.Variables.Single(x => x.Type == typeof(InternalEntityEntry));
                var hasNullKeyVariable = materializerBlock.Variables.Single(x => x.Type == typeof(bool));

                var entryAssignment = materializerBlock.Expressions.OfType<BinaryExpression>()
                    .Single(x => x.NodeType == ExpressionType.Assign && x.Left == entryVariable);

                var hasNullKeyCheck = materializerBlock.Expressions.OfType<ConditionalExpression>()
                    .Single(x => x.Test is UnaryExpression { NodeType: ExpressionType.Not } ue && ue.Operand == hasNullKeyVariable);
                var entryNotNullCheck = (ConditionalExpression)hasNullKeyCheck.IfTrue;
                var entryNotNullBlock = (BlockExpression)entryNotNullCheck.IfTrue;
                var readValuesBlock = (BlockExpression)entryNotNullCheck.IfFalse;

                var shadowSnapshotVariable = readValuesBlock.Variables.SingleOrDefault(x => x.Type == typeof(ISnapshot));

                _entityTypeMaterializerExpressionsMapping[entityType] = (
                    instanceVariable,
                    shadowSnapshotVariable,
                    trackingActions,
                    entryAssignment);

                // Remove the start tracking call...
                var startTrackingAssignment = readValuesBlock.Expressions.OfType<BinaryExpression>()
                    .Single(x => x.NodeType == ExpressionType.Assign && x.Left == entryVariable);
                readValuesBlock = readValuesBlock.Update(readValuesBlock.Variables, readValuesBlock.Expressions.Where(x => x != startTrackingAssignment));

                materializerBlock = (BlockExpression)Visit(readValuesBlock);

                materializerVariables.AddRange(materializerBlock.Variables);
                materializerExpressions.AddRange(materializerBlock.Expressions);

                // Because of tracking, this returns (IEntityType, RootEntity, ISnapshot, List<Action>)
                // Else this only returns RootEntity (clrType)
                materializerExpressions.Add(
                    New(
                        typeof(ValueTuple<,,,>).MakeGenericType(typeof(IEntityType), entityType.ClrType, typeof(ISnapshot), typeof(List<Action>))
                            .GetConstructor([typeof(IEntityType), entityType.ClrType, typeof(ISnapshot), typeof(List<Action>)])!,
                        entityTypeVariable,
                        instanceVariable,
                        (Expression?)shadowSnapshotVariable ?? Default(typeof(ISnapshot)),
                        trackingActions));
            }
            else
            {
                if (structuralType is IEntityType et && et.FindPrimaryKey() != null)
                {
                    var nullKeyCheck = materializerBlock.Expressions.OfType<ConditionalExpression>().Single();
                    var readValuesBlock = (BlockExpression)nullKeyCheck.IfTrue;

                    materializerBlock = (BlockExpression)Visit(readValuesBlock);
                }
                else
                {
                    materializerBlock = materializerBlock.Expressions.OfType<BlockExpression>().Single();
                    materializerBlock = (BlockExpression)Visit(materializerBlock);
                }
                
                materializerVariables.AddRange(materializerBlock.Variables);
                materializerExpressions.AddRange(materializerBlock.Expressions);
            }

            materializerBlock = Block(materializerVariables, materializerExpressions);

            var resultType = structuralType.ClrType.IsValueType && nullable ? materializerBlock.Type.MakeNullable() : materializerBlock.Type;

            var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");

            // Check if there is an object to materialize before we start.
            materializerBlock = Block(
                [_jsonReaderManagerVariable, tokenTypeVariable],
                // jsonReaderManager = new Utf8JsonReaderManager(data.Span)
                [Assign(_jsonReaderManagerVariable, NewJsonReaderManager()),
                // tokenType = jsonReaderManager.MoveNext()
                Assign(tokenTypeVariable, Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                // switch (tokenType)
                Switch(
                    tokenTypeVariable,
                    // default: throw new InvalidOperationException(InvalidTokenType)
                    Throw(Call(NewJsonReaderInvalidTokenTypeExceptionMethodInfo, tokenTypeVariable), resultType),
                    // case JsonTokenType.Null: return default
                    SwitchCase(
                        Block(
                            Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerCaptureStateMethod),
                            Default(resultType)),
                        Constant(JsonTokenType.Null)),
                    // case JsonTokenType.StartObject: materializerBlock
                    SwitchCase(ConvertIfNotMatch(materializerBlock, resultType),
                        Constant(JsonTokenType.StartObject)))]);

            lambda = Lambda(
                materializerBlock,
                structuralType.Name + "_Materializer",
                GetParametersForLambda(structuralType));

            _structuralTypeJsonMaterializerLambdaMapping.Add(structuralType, lambda);

            return lambda;
        }

        private BlockExpression ReadDiscriminator(ITypeBase structuralType, IProperty discriminatorProperty, ParameterExpression discriminatorValueVariable)
        {
            // @TODO: Change serializer to put discriminator first
            // Generate a read loop to get the discriminator
            // string discriminatorValue = null;
            // while (true)
            //  tokenType = jsonReaderManager.MoveNext();
            //  switch($tokenType) {
            //      case(JsonTokenType.Null):
            //      case(JsonTokenType.EndObject):
            //          goto EndRead;
            //      case(JsonTokenType.PropertyName):
            //          if (jsonReaderManager.CurrentReader.ValueTextEquals("$type"u8))
            //              jsonReaderManager.MoveNext();
            //              discriminatorValue = jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
            //              goto EndRead;
            //          else if (jsonReaderManager.CurrentReader.ValueTextEquals("Id"u8))
            //              jsonReaderManager.Skip();
            //              jsonReaderManager.Skip();
            //          else
            //              throw new InvalidOperationException("Discriminator was not early in the document.");
            //      default:
            //          throw invalid json
            // EndRead:


            var variables = new List<ParameterExpression>();
            var expressions = new List<Expression>();

            var breakLabel = Label("EndRead");

            Expression noMatch;
            //if (_throwOnLateDiscriminator) // @TODO
            //{
            //    var ifNotPropertyMatchThrow = Throw(New(InvalidOperationExceptionConstructor, Constant("Discriminator was not early in the document."))); // @TODO: message: Discriminator was not early in the document.
            //    noMatch = structuralType is IEntityType entityType && entityType.FindPrimaryKey() is { } primaryKey // Allow primary keys to come before discriminator, for backwards compatibility.
            //                                                                                                        // else if (jsonReaderManager.CurrentReader.ValueTextEquals(("Id"u8).Span))
            //            ? IfThenElse(
            //                primaryKey.Properties
            //                    .Select(p => JsonReaderValueTextEquals(_jsonReaderManagerVariable, p.GetJsonPropertyName()))
            //                    .Aggregate<MethodCallExpression, Expression?>(
            //                        null,
            //                        (previous, next) => previous is null ? next : OrElse(previous, next))!,
            //                    // jsonReaderManager.Skip() x2
            //                    Block(Enumerable.Range(0, 2).Select(_ => Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerSkipMethod))),
            //                // else throw new InvalidOperationException("Discriminator was not early in the document.")
            //                ifNotPropertyMatchThrow)
            //            : ifNotPropertyMatchThrow;
            //}
            //else
            {
                // jsonReaderManager.Skip() x2
                noMatch = Block(Enumerable.Range(0, 2).Select(_ => Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerSkipMethod)));
            }

            var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");

            return Block(
                [_jsonReaderManagerVariable, tokenTypeVariable],
                Assign(_jsonReaderManagerVariable, NewJsonReaderManager()),
                // jsonReaderManager.MoveNext();
                Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod),
                // string? discriminatorValue = defalt();
                Assign(discriminatorValueVariable, Default(discriminatorProperty.ClrType.MakeNullable())),
                Loop(
                    Block([], [
                        // tokenType = jsonReaderManager.MoveNext();
                        Assign(tokenTypeVariable, Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                        // switch (tokenType)
                        Switch(tokenTypeVariable,
                            // default: throw
                            Throw(Call(NewJsonReaderInvalidTokenTypeExceptionMethodInfo, tokenTypeVariable)),
                            [
                                // case Null: goto EndRead
                                SwitchCase(Break(breakLabel, typeof(void)), Constant(JsonTokenType.Null)),
                                // case EndObject: goto EndRead
                                SwitchCase(Break(breakLabel, typeof(void)), Constant(JsonTokenType.EndObject)),
                                // case PropertyName:
                                SwitchCase(
                                    // if (jsonReaderManager.CurrentReader.ValueTextEquals(("$type"u8).Span))
                                    IfThenElse(
                                        JsonReaderManagerValueTextEquals(_jsonReaderManagerVariable, discriminatorProperty.GetJsonPropertyName()),
                                        Block(
                                            // jsonReaderManager.MoveNext()
                                            Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod),
                                            // discriminatorValue = jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
                                            Assign(
                                                discriminatorValueVariable,
                                                CheckMakeNullableValueType(
                                                    ReadJsonPropertyValue(discriminatorProperty))),
                                            // goto EndRead
                                            Break(breakLabel, typeof(void))),
                                        noMatch
                                        ),
                                    Constant(JsonTokenType.PropertyName))])]),
                    breakLabel));
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase switchCase)
            => switchCase switch
            {
                {
                    Body: BlockExpression { Expressions.Count: > 0 } body,
                    TestValues: [ConstantExpression { Value: ITypeBase structuralType }]
                } => switchCase.Update(switchCase.TestValues, RewriteStructuralTypeCase(body, structuralType)),
                _ => base.VisitSwitchCase(switchCase)
            };

        private BlockExpression RewriteStructuralTypeCase(BlockExpression body, ITypeBase structuralType)
        {
            // keep track which variable corresponds to which navigation - we need that info for fixup
            // which happens at the end (after we read everything to guarantee that we can instantiate the entity
            var navigationVariableMap = new Dictionary<IPropertyBase, ParameterExpression>();

            var valueBufferTryReadValueMethodsToProcess =
                new ValueBufferTryReadValueMethodsFinder(structuralType).FindValueBufferTryReadValueMethods(body);

            BlockExpression jsonEntityTypeInitializerBlock;
            // sometimes we have shadow snapshot and sometimes not, but type initializer always comes last
            switch (body.Expressions[^1])
            {
                case UnaryExpression
                {
                    Operand: BlockExpression innerBlock,
                    NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked
                } jsonEntityTypeInitializerUnary:
                {
                    // in case of proxies, the entity initializer block is wrapped around Convert node
                    // that converts from the proxy type to the actual entity type.
                    // We normalize that into a block by pushing the convert inside the inner block. Rather than:
                    //
                    // return (MyEntity)
                    // {
                    //     ProxyEntity instance;
                    //     (...)
                    //     return instance;
                    // }
                    //
                    // we produce:
                    // return
                    // {
                    //     ProxyEntity instance;
                    //     MyEntity actualInstance;
                    //     (...)
                    //     actualInstance = (MyEntity)instance;
                    //     return actualInstance;
                    // }
                    var newVariables = innerBlock.Variables.ToList();
                    var proxyConversionVariable = Variable(jsonEntityTypeInitializerUnary.Type);
                    newVariables.Add(proxyConversionVariable);
                    var newExpressions = innerBlock.Expressions.ToList()[..^1];
                    newExpressions.Add(
                        Assign(proxyConversionVariable, jsonEntityTypeInitializerUnary.Update(innerBlock.Expressions[^1])));
                    newExpressions.Add(proxyConversionVariable);
                    jsonEntityTypeInitializerBlock = Block(newVariables, newExpressions);
                    break;
                }

                case BlockExpression b:
                    jsonEntityTypeInitializerBlock = b;
                    break;
                // case where we don't use block but rather return construction directly, as in:
                // return new MyEntity(...)
                //
                // rather than:
                // return
                // {
                //    MyEntity instance;
                //    instance = new MyEntity(...)
                //    (...)
                // }
                // we normalize this into block, since we are going to be adding extra statements (i.e. loop extracting JSON
                // property values) there anyway
                case NewExpression jsonEntityTypeInitializerCtor:
                    var newInstanceVariable = Variable(jsonEntityTypeInitializerCtor.Type, "instance");
                    jsonEntityTypeInitializerBlock = Block(
                        [newInstanceVariable],
                        Assign(newInstanceVariable, jsonEntityTypeInitializerCtor),
                        newInstanceVariable);
                    break;
                default:
                    throw new UnreachableException();
            }

            var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");
            var instanceVariable = (ParameterExpression)jsonEntityTypeInitializerBlock.Expressions[^1];

            Debug.Assert(instanceVariable.Type.IsAssignableFrom(structuralType.ClrType));

            var finalBlockVariables = new List<ParameterExpression>
            {
                _jsonReaderManagerVariable, tokenTypeVariable,
            };

            finalBlockVariables.AddRange(jsonEntityTypeInitializerBlock.Variables);

            var finalBlockExpressions = new List<Expression>
            {
                // jsonReaderManager = new Utf8JsonReaderManager(jsonReaderData))
                Assign(_jsonReaderManagerVariable, NewJsonReaderManager()),
                // jsonReaderManager.MoveNext();
                Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod),
            };

            IEnumerable<IPropertyBase> nestedStructuralProperties = structuralType.GetComplexProperties();

            if (structuralType is IEntityType et)
            {
                nestedStructuralProperties = nestedStructuralProperties.Concat(
                    et.GetNavigations()
                        .Where(n => n.ForeignKey.IsOwnership
                                 && n == n.ForeignKey.PrincipalToDependent));
            }

            var (loop, propertyAssignmentMap) = GenerateJsonPropertyReadLoop(
                tokenTypeVariable,
                finalBlockVariables,
                valueBufferTryReadValueMethodsToProcess);

            finalBlockExpressions.Add(loop);

            var finalCaptureState = Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerCaptureStateMethod);
            finalBlockExpressions.Add(finalCaptureState);

            // we have the loop, now we can add code that generate the entity instance
            // will have to replace ValueBufferTryReadValue method calls with the parameters that store the value
            // we can't use simple ExpressionReplacingVisitor, because there could be multiple instances of MethodCallExpression for given property
            // using dedicated mini-visitor that looks for MCEs with a given shape and compare the IProperty inside
            // order is:
            // - shadow snapshot (if there was one)
            // - entity construction / property assignments
            // - manual fixups
            // - entity instance variable that is returned as end result
            var valueBufferTryReadValueReplacer = new ValueBufferTryReadValueMethodsReplacer(propertyAssignmentMap);

            if (body.Expressions[0] is BinaryExpression
                {
                    NodeType: ExpressionType.Assign,
                    Right: UnaryExpression
                    {
                        NodeType: ExpressionType.Convert,
                        Operand: NewExpression
                    }
                } shadowSnapshotAssignment
#pragma warning disable EF1001 // Internal EF Core API usage.
                && shadowSnapshotAssignment.Type == typeof(ISnapshot))
#pragma warning restore EF1001 // Internal EF Core API usage.
            {
                finalBlockExpressions.Add(valueBufferTryReadValueReplacer.Visit(shadowSnapshotAssignment));
            }

            foreach (var jsonEntityTypeInitializerBlockExpression in jsonEntityTypeInitializerBlock.Expressions.ToArray()[..^1])
            {
                finalBlockExpressions.Add(Visit(valueBufferTryReadValueReplacer.Visit(jsonEntityTypeInitializerBlockExpression)));
            }

            foreach (var (property, variable) in navigationVariableMap)
            {
                finalBlockExpressions.Add(
                    MakeMemberAccess(ConvertIfNotMatch(instanceVariable, property.DeclaringType.ClrType), property.GetMemberInfo(true, true)).Assign(variable));
            }

            // Empty collections have not been initialized, so we double check all collection properties here
            if (!structuralType.ClrType.IsValueType) // @TODO: How does this work for value types...?
            {
                foreach (var collectionProperty in nestedStructuralProperties.Where(x => x.IsCollection))
                {
                    finalBlockExpressions.Add(Call(Constant(collectionProperty.GetCollectionAccessor()), CollectionAccessorGetOrCreateMethodInfo, instanceVariable, Constant(true)));
                }
            }

            finalBlockExpressions.Add(instanceVariable);

            return Block(
                finalBlockVariables,
                finalBlockExpressions);

            // builds a loop that extracts values of JSON properties and assigns them into variables
            // also creates materializers for child navigations
            // returns the loop expression and mappings for properties (so we know which calls to replace with variables)
            (LoopExpression, Dictionary<IProperty, Expression>) GenerateJsonPropertyReadLoop(
                ParameterExpression tokenTypeVariable,
                List<ParameterExpression> finalBlockVariables,
                List<MethodCallExpression> valueBufferTryReadValueMethodsToProcess)
            {
                var breakLabel = Label("done");
                var testExpressions = new List<Expression>();
                var readExpressions = new List<Expression>();
                var propertyAssignmentMap = new Dictionary<IProperty, Expression>();

                foreach (var valueBufferTryReadValueMethodToProcess in valueBufferTryReadValueMethodsToProcess)
                {
                    var property = valueBufferTryReadValueMethodToProcess.Arguments[2].GetConstantValue<IProperty>();
                    var jsonPropertyName = property.GetJsonPropertyName();

                    if (!property.IsPersisted())
                    {
                        continue;
                    }

                    testExpressions.Add(
                        Call(
                            Field(
                                _jsonReaderManagerVariable,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderValueTextEqualsMethod,
                            Convert(
                                Call(
                                    ByteArrayAsSpanMethod,
                                    Constant(Encoding.UTF8.GetBytes(jsonPropertyName))),
                                typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                    var propertyVariable = Variable(valueBufferTryReadValueMethodToProcess.Type, property.Name);

                    finalBlockVariables.Add(propertyVariable);

                    var moveNext = Call(
                        _jsonReaderManagerVariable,
                        Utf8JsonReaderManagerMoveNextMethod);

                    var assignment = Assign(
                        propertyVariable,
                        Visit(valueBufferTryReadValueMethodToProcess));

                    readExpressions.Add(
                        Block(
                            moveNext,
                            assignment,
                            Empty()));

                    propertyAssignmentMap[property] = propertyVariable;
                }

                // Go over all structural properties (complex properties and navigations - if we're an (owned) entity), which represent JSON
                // nested types; generate materializers and fixup to wire the materialized related instance into the parent's property.
                // Note that we need to build entity materializers and tracking fixup separately; we don't know the order in which data comes, so
                // we need to read through everything before we can do tracking fixup safely
                foreach (var nestedStructuralProperty in nestedStructuralProperties)
                {
                    Check.DebugAssert(
                        nestedStructuralProperty is not INavigation ownedNavigation || !ownedNavigation.IsOnDependent,
                        "JSON navigations should always be from principal do dependent");

                    var (nestedStructuralType, inverseNavigation, isStructuralPropertyNullable, jsonPropertyName) = nestedStructuralProperty switch
                    {
                        INavigation n => ((ITypeBase)n.TargetEntityType, n.Inverse, !n.ForeignKey.IsRequiredDependent, n.TargetEntityType.GetContainingPropertyName()!),
                        IComplexProperty cp => (cp.ComplexType, null, cp.IsNullable, cp.GetJsonPropertyName()),

                        _ => throw new UnreachableException()
                    };

                    testExpressions.Add(JsonReaderManagerValueTextEquals(_jsonReaderManagerVariable, jsonPropertyName));

                    var nestedMaterializerLambda =
                        StructuralTypeJsonMaterializerLambda(
                            nestedStructuralType,
                            isStructuralPropertyNullable);

                    // Builds the expression tree for collection properties. This is a while loop around the materializer for the nested structural type
                    var nestedCollectionReadVariables = new List<ParameterExpression>()
                    {
                        _ordinalParameter
                    };
                    var nestedCollectionReadExpressions = new List<Expression>()
                    {
                        Assign(_ordinalParameter, Constant(-1)),
                    };

                    var nestedMaterializer = Invoke(nestedMaterializerLambda, GetParametersForLambda(nestedStructuralType));

                    // Builds the expression tree for materializing an instance of the nested structural type
                    var nestedReadExpressions = new List<Expression>();
                    var nestedReadVariables = new List<ParameterExpression>();

                    if (!nestedStructuralProperty.IsCollection)
                    {
                        nestedReadExpressions.Add(Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerCaptureStateMethod));
                    }
                    else
                    {
                        nestedReadExpressions.Add(PostIncrementAssign(_ordinalParameter));
                    }

                    if (RequiresTracking(nestedStructuralType, out var nestedEntityType))
                    {
                        // Change tracker will do fixup
                        // We also need to set any non persisted principal properties on the nested entity to the values from the parent entity, because we can only do this once the parent entity is fully materialized
                        //var (nestedEntityType, nestedInstance, nestedShadowSnapshot, nestedTrackingActions) = MaterializeAssociate(queryContext, jsonReaderData);
                        //if (nestedInstance != default)
                        //{
                        //  trackingActions.Add(() =>
                        //  {
                        //      var entry = queryContext.TryGetEntry(nestedEntityType, new object[] { instance.Id, nestedInstance.Id }, false, out var _);
                        //      if (entry != default)
                        //      {
                        //          nestedShadowSnapshotVariable.SetValue<T>(0, instance.Id);
                        //          foreach (var nestedTrackingAction in nestedTrackingActions)
                        //          {
                        //              nestedTrackingAction();
                        //          }
                        //          queryContext.StartTracking(nestedEntityType, nestedInstance, nestedShadowSnapshot);
                        //    }
                        //});
                        //}

                        var tupleType = nestedMaterializerLambda.Body.Type;

                        var tupleVariable = Variable(tupleType);
                        nestedReadVariables.Add(tupleVariable);

                        var nestedEntityTypeVariable = Field(tupleVariable, MaterializerTupleEntityTypeField(tupleType));
                        var nestedInstanceVariable = Field(tupleVariable, MaterializerTupleInstanceField(tupleType));
                        var nestedShadowSnapshotVariable = Field(tupleVariable, MaterializerTupleShadowSnapshotField(tupleType));
                        var nestedTrackingActionsVariable = Field(tupleVariable, MaterializerTupleTrackingActionsField(tupleType));

                        var (parentInstanceVariable, parentShadowSnapshotVariable, parentTrackingActionsVariable, _) = GetEntityTypeMaterializerExpressions((IEntityType)structuralType);
                        var tryGetEntryAssignment = GetEntityTypeMaterializerExpressions(nestedEntityType).TryGetEntryAssignment;
                        var entryVariable = (ParameterExpression)tryGetEntryAssignment.Left;
                        var hasNullKeyVariable = (ParameterExpression)((MethodCallExpression)tryGetEntryAssignment.Right).Arguments[3];

                        // Map key values to the correct source for the try get entry call.
                        var tryGetEntryPropertyMap = nestedEntityType.FindPrimaryKey()!.Properties
                            .ToDictionary(property => property, property =>
                            {
                                return nestedEntityType.IsOwned() && property.IsOrdinalKeyProperty()
                                        ? _ordinalParameter
                                        : property.FindFirstPrincipal() is { } principalProperty
                                            ? principalProperty.IsShadowProperty()
                                                ? GetSnapshotValue(parentShadowSnapshotVariable!, principalProperty)
                                                : parentInstanceVariable.MakeMemberAccess(principalProperty.GetMemberInfo(forMaterialization: true, forSet: false))
                                            : property.IsShadowProperty()
                                                ? GetSnapshotValue(nestedShadowSnapshotVariable, property)
                                                : nestedInstanceVariable.MakeMemberAccess(property.GetMemberInfo(forMaterialization: true, forSet: false));

                                static Expression GetSnapshotValue(Expression snapshotVariable, IProperty property)
                                    => Call(
                                        snapshotVariable,
                                        Snapshot.GetValueMethod.MakeGenericMethod(property.ClrType),
                                        Constant(property.GetShadowIndex()));
                            });

                        tryGetEntryAssignment = (BinaryExpression)new ValueBufferTryReadValueMethodsReplacer(tryGetEntryPropertyMap)
                            .Visit(tryGetEntryAssignment);

                        nestedReadExpressions.AddRange(
                            Assign(tupleVariable, nestedMaterializer),
                            // if (nestedInstance != default)
                            IfThen(NotEqual(nestedInstanceVariable, Default(nestedInstanceVariable.Type)),
                                // parentTrackingActions.Add(() =>
                                Call(parentTrackingActionsVariable, parentTrackingActionsVariable.Type.GetMethod(nameof(List<>.Add))!,
                                    Lambda(
                                        Block(
                                            [entryVariable, hasNullKeyVariable],
                                            [
                                                // var entry = queryContext.TryGetEntry(entityType, new object[] { instance.Id, nestedInstance.Id }, false, out var _);
                                                tryGetEntryAssignment,
                                                // if (entry == default)
                                                IfThen(Equal(entryVariable, Default(entryVariable.Type)),
                                                    Block([
                                                        // nestedShadowSnapshotVariable.SetValue<T>(0, instance.Id)
                                                        ..nestedEntityType.GetProperties().Where(x => x.IsShadowProperty() && !x.IsPersisted()).Select(p => new { self = p, principal = p.FindFirstPrincipal()! }).Where(x => x.principal != null).Select(p =>
                                                            Call(nestedShadowSnapshotVariable, Snapshot.SetValueMethod.MakeGenericMethod(p.self.ClrType),
                                                                Constant(p.self.GetShadowIndex()),
                                                                ConvertIfNotMatch(
                                                                    p.principal.IsShadowProperty()
                                                                        ? Call(parentShadowSnapshotVariable, Snapshot.GetValueMethod.MakeGenericMethod(p.principal.ClrType), Constant(p.principal.GetShadowIndex()))
                                                                        : parentInstanceVariable.MakeMemberAccess(p.principal.GetMemberInfo(true, false)),
                                                                p.self.ClrType))),
                                                        // nestedShadowSnapshotVariable.SetValue<T>(1, ordinal)
                                                        ..nestedEntityType.GetProperties().Where(x => x.IsOrdinalKeyProperty()).Select(p => ConvertIfNotMatch(_ordinalParameter, p.ClrType)),
                                                        // nestedTrackingActions.ForEach(nestedTrackingAction => nestedTrackingAction())
                                                        ForEach(nestedTrackingActionsVariable, nestedTrackingAction => Invoke(nestedTrackingAction)),
                                                        // queryContext.StartTracking(nestedEntityType, nestedInstance, nestedShadowSnapshot)
                                                        Call(QueryCompilationContext.QueryContextParameter, StartTrackingMethodInfo, nestedEntityTypeVariable, nestedInstanceVariable, nestedShadowSnapshotVariable)]))])))));
                    }
                    else
                    {
                        // We have to do manual fixup for this structural property
                        var propertyVariable = Variable(nestedStructuralProperty.ClrType, nestedStructuralProperty.Name);
                        finalBlockVariables.Add(propertyVariable);
                        navigationVariableMap[nestedStructuralProperty] = propertyVariable;
                        if (!nestedStructuralProperty.IsCollection)
                        {
                            nestedReadExpressions.Add(Assign(propertyVariable, nestedMaterializer));
                        }
                        else
                        {
                            nestedCollectionReadExpressions.Add(Assign(propertyVariable, Convert(Call(Constant(nestedStructuralProperty.GetCollectionAccessor(), typeof(IClrCollectionAccessor)), CollectionAccessorCreateMethodInfo), propertyVariable.Type)));
                            nestedReadExpressions.Add(Call(Constant(nestedStructuralProperty.GetCollectionAccessor()), CollectionAccessorAddStandaloneMethodInfo, propertyVariable, nestedMaterializer));
                        }
                    }

                    nestedReadExpressions.Add(Assign(_jsonReaderManagerVariable, NewJsonReaderManager()));
                    nestedReadExpressions.Add(Empty());

                    var nestedReadBlock = Block(nestedReadVariables, nestedReadExpressions);

                    if (nestedStructuralProperty.IsCollection)
                    {
                        var collectionBreakLabel = Label("EndCollection");
                        readExpressions.Add(
                            Block(nestedCollectionReadVariables,
                                // tokenType = jsonReaderManager.MoveNext()
                                [Assign(tokenTypeVariable, Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                                // switch (tokenType)
                                Switch(tokenTypeVariable,
                                    // default: throw new InvalidOperationException(InvalidTokenType)
                                    Throw(Call(NewJsonReaderInvalidTokenTypeExceptionMethodInfo, tokenTypeVariable)),
                                    // case Null: jsonReaderManager.CaptureState()
                                    SwitchCase(Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerCaptureStateMethod), Constant(JsonTokenType.Null)),
                                    // case StartArray
                                    SwitchCase(
                                        Block([
                                            ..nestedCollectionReadExpressions,
                                            // jsonReaderManager.CaptureState()
                                            Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerCaptureStateMethod),
                                            // while (true)
                                            Loop(Block(
                                                // tokenType = jsonReaderManager.MoveNext()
                                                Assign(tokenTypeVariable, Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                                                // switch (tokenType)
                                                Switch(tokenTypeVariable,
                                                    // default: materializer
                                                    nestedReadBlock,
                                                    // case Null: throw new InvalidOperationException("Null item in collection.")
                                                    SwitchCase(Throw(New(typeof(InvalidOperationException))), Constant(JsonTokenType.Null)), // @TODO: message? Null item in collection.
                                                    // case EndArray: goto collectionBreakLabel
                                                    SwitchCase(Break(collectionBreakLabel), Constant(JsonTokenType.EndArray)))),
                                                collectionBreakLabel)]),
                                        Constant(JsonTokenType.StartArray)))]));
                    }
                    else
                    {
                        readExpressions.Add(nestedReadBlock);
                    }
                }

                var switchCases = new List<SwitchCase>
                {
                    SwitchCase(Break(breakLabel, typeof(void)), Constant(JsonTokenType.Null)),
                    SwitchCase(
                        Break(breakLabel),
                        Constant(JsonTokenType.EndObject))
                };

                var testsCount = testExpressions.Count;

                // generate PropertyName switch-case code
                if (testsCount > 0)
                {
                    var testExpression = IfThenElse(
                        testExpressions[testsCount - 1],
                        readExpressions[testsCount - 1],
                        // jsonReaderManager.Skip() x2
                        Block(Enumerable.Range(0, 2).Select(_ => Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerSkipMethod))));

                    for (var i = testsCount - 2; i >= 0; i--)
                    {
                        testExpression = IfThenElse(
                            testExpressions[i],
                            readExpressions[i],
                            testExpression);
                    }

                    switchCases.Add(
                        SwitchCase(
                            testExpression,
                            Constant(JsonTokenType.PropertyName)));
                }

                var loopBody = Block(
                    Assign(tokenTypeVariable, Call(_jsonReaderManagerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                    Switch(
                        tokenTypeVariable,
                        Block(
                            Throw(Call(NewJsonReaderInvalidTokenTypeExceptionMethodInfo, tokenTypeVariable)),
                            Default(typeof(void))),
                        switchCases.ToArray()));

                return (Loop(loopBody, breakLabel), propertyAssignmentMap);
            }
        }

        private (
            ParameterExpression InstanceVariable,
            ParameterExpression? ShadowSnapshotVariable,
            ParameterExpression TrackingActionsVariable,
            BinaryExpression TryGetEntryAssignment)
            GetEntityTypeMaterializerExpressions(IEntityType entityType)
        {
            do
            {
                if (_entityTypeMaterializerExpressionsMapping.TryGetValue(entityType, out var materializerExpressions))
                {
                    return materializerExpressions;
                }
            } while ((entityType = entityType.BaseType!) != null);

            throw new UnreachableException();
        }

        private ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            => ((SelectExpression)projectionBindingExpression.QueryExpression).Projection[GetProjectionIndex(projectionBindingExpression)];

        private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember != null
                ? ((SelectExpression)projectionBindingExpression.QueryExpression).GetMappedProjection(projectionBindingExpression.ProjectionMember).GetConstantValue<int>()
                : (projectionBindingExpression.Index
                    ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print())));

        private NewExpression NewJsonReaderManager()
            => New(JsonReaderManagerConstructor, _jsonReaderDataParameter, MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty));

        private ParameterExpression[] GetParametersForLambda(ITypeBase structuralType)
            => structuralType.TryGetOrdinalKey(out _)
             ? [QueryCompilationContext.QueryContextParameter, _jsonReaderDataParameter, _ordinalParameter]
             : [QueryCompilationContext.QueryContextParameter, _jsonReaderDataParameter];

        private bool RequiresTracking(ITypeBase structuralType, [NotNullWhen(true)] out IEntityType? entityType)
        {
            entityType = structuralType as IEntityType;
            return _queryStateManager && entityType != null && entityType.FindPrimaryKey() != null;
        }

        private Expression ReadJsonPropertyValue(IProperty property)
        {
            var jsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
            Debug.Assert(jsonValueReaderWriter != null, "JsonValueReaderWriter should not be null since we are in Cosmos provider and all types should have JsonValueReaderWriter");
            return CreateReadJsonValueExpression(_jsonReaderManagerVariable, jsonValueReaderWriter, property.ClrType);
        }

        private static Expression CreateReadJsonValueExpression(ParameterExpression jsonReaderManagerParameter, CoreTypeMapping typeMapping, Type clrType)
        {
            var jsonValueReaderWriter = typeMapping.JsonValueReaderWriter;
            Debug.Assert(jsonValueReaderWriter != null, "JsonValueReaderWriter should not be null since we are in Cosmos provider and all types should have JsonValueReaderWriter");
            return CreateReadJsonValueExpression(jsonReaderManagerParameter, jsonValueReaderWriter, clrType);
        }

        private static Expression CreateReadJsonValueExpression(ParameterExpression jsonReaderManagerParameter, JsonValueReaderWriter jsonValueReaderWriter, Type clrType)
        {
            var jsonValueReaderWriterConstant = Constant(jsonValueReaderWriter);

            var fromJsonMethod = jsonValueReaderWriterConstant.Type.GetMethod(
                nameof(JsonValueReaderWriter<>.FromJsonTyped),
                [typeof(Utf8JsonReaderManager).MakeByRefType(), typeof(object)])!;

            Expression resultExpression = Call(jsonValueReaderWriterConstant, fromJsonMethod, jsonReaderManagerParameter, Default(typeof(object)));

            if (resultExpression.Type != clrType)
            {
                resultExpression = Convert(resultExpression, clrType);
            }

            if (clrType.IsNullableType())
            {
                // in case of null value we can't just use the JsonReader method, but rather check the current token type
                // if it's JsonTokenType.Null means value is null, only if it's not we are safe to read the value
                resultExpression = Condition(
                    Equal(
                        Property(
                            Field(
                                jsonReaderManagerParameter,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderTokenTypeProperty),
                        Constant(JsonTokenType.Null)),
                    Default(clrType),
                    Convert(resultExpression, clrType));
            }

            return resultExpression;
        }

        private static BlockExpression ForEach(Expression list, Func<ParameterExpression, Expression> bodyFactory)
        {
            var itemType = list.Type.GetGenericArguments()[0];

            var listVar = Variable(list.Type, "list");
            var i = Variable(typeof(int), "i");
            var count = Variable(typeof(int), "count");
            var item = Variable(itemType, "item");

            var breakLabel = Label("break");

            var countProperty = list.Type.GetProperty(nameof(List<int>.Count))!;
            var indexerProperty = list.Type.GetProperty("Item")!;

            var body = bodyFactory(item);

            if (body.Type != typeof(void))
            {
                body = Block(body, Empty());
            }

            return Block(
                [listVar, i, count, item],

                Assign(listVar, list),
                Assign(i, Constant(0)),
                Assign(count, Property(listVar, countProperty)),

                Loop(
                    IfThenElse(
                        LessThan(i, count),

                        Block(
                            Assign(
                                item,
                                MakeIndex(
                                    listVar,
                                    indexerProperty,
                                    [i]
                                )
                            ),

                            body,

                            PostIncrementAssign(i)
                        ),

                        Break(breakLabel)
                    ),
                    breakLabel
                )
            );
        }

        private static MemberExpression StringConstantSpan(string value)
            => Property(Constant((ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(value)), typeof(ReadOnlyMemory<byte>).GetProperty(nameof(ReadOnlyMemory<>.Span))!);

        private static Expression ConvertIfNotMatch(Expression expression, Type targetType)
            => expression.Type != targetType
                ? Convert(expression, targetType)
                : expression;

        private static Expression CheckMakeNullableValueType(Expression expression)
            => expression.Type.IsValueType && !expression.Type.IsNullableValueType()
                ? Convert(expression, expression.Type.MakeNullable())
                : expression;

        private static MethodCallExpression JsonReaderManagerValueTextEquals(Expression jsonReaderManager, string text)
            => JsonReaderValueTextEquals(
                Field(
                    jsonReaderManager,
                    Utf8JsonReaderManagerCurrentReaderField),
                text);

        private static MethodCallExpression JsonReaderValueTextEquals(Expression jsonReader, string text)
            => Call(
                jsonReader,
                Utf8JsonReaderValueTextEqualsMethod,
                StringConstantSpan(text));

        private static bool IsValueBufferTryReadValueMethodCall(Expression expression, [NotNullWhen(true)] out IProperty? property)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition()
                == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty p)
            {
                property = p;
                return true;
            }

            property = null;
            return false;
        }

        private sealed class ValueBufferTryReadValueMethodsFinder : ExpressionVisitor
        {
            private readonly List<IProperty> _properties;
            private readonly List<MethodCallExpression> _valueBufferTryReadValueMethods = [];

            public ValueBufferTryReadValueMethodsFinder(ITypeBase structuralType)
                => _properties = structuralType.GetProperties().ToList();

            public List<MethodCallExpression> FindValueBufferTryReadValueMethods(Expression expression)
            {
                _valueBufferTryReadValueMethods.Clear();

                Visit(expression);

                return _valueBufferTryReadValueMethods;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (IsValueBufferTryReadValueMethodCall(methodCallExpression, out var property)
                 && _properties.Contains(property))
                {
                    _valueBufferTryReadValueMethods.Add(methodCallExpression);
                    _properties.Remove(property);

                    return methodCallExpression;
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        private sealed class ValueBufferTryReadValueMethodsReplacer(IReadOnlyDictionary<IProperty, Expression> mappedProperties) : ExpressionVisitor // @TODO: Can we simply bring back instance?
        {
            private readonly Dictionary<IProperty, Expression> _propertyInstanceMap = [];

            public BlockExpression Rewrite(BlockExpression materializerExpression)
            {
                _propertyInstanceMap.Clear();
                return (BlockExpression)VisitBlock(materializerExpression);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (IsValueBufferTryReadValueMethodCall(node.Right, out var property)
                 && mappedProperties.TryGetValue(property, out var parameter))
                {
                    parameter = ConvertIfNotMatch(parameter, node.Right.Type);

                    if (node.Left is MemberExpression memberExpression)
                    {
                        _propertyInstanceMap[property] = memberExpression.Expression ?? throw new UnreachableException();
                    }

                    if (property!.IsPrimitiveCollection
                        && !property.ClrType.IsArray)
                    {
                        var instance = _propertyInstanceMap[property];

                        var genericMethod = StructuralTypeMaterializerSource.PopulateListMethod.MakeGenericMethod(
                            property.ClrType.TryGetElementType(typeof(IEnumerable<>))!);

                        var currentVariable = Variable(parameter!.Type);
                        var convertedVariable = genericMethod.GetParameters()[1].ParameterType.IsAssignableFrom(currentVariable.Type)
                            ? (Expression)currentVariable
                            : Convert(currentVariable, genericMethod.GetParameters()[1].ParameterType);
                        return Block(
                            [currentVariable],
                            MakeMemberAccess(instance, property.GetMemberInfo(forMaterialization: true, forSet: false))
                                .Assign(currentVariable),
                            IfThenElse(
                                OrElse(
                                    ReferenceEqual(currentVariable, Constant(null)),
                                    ReferenceEqual(parameter, Constant(null))),
                                node is { NodeType: ExpressionType.Assign, Left: MemberExpression leftMemberExpression }
                                    ? leftMemberExpression.Assign(parameter)
                                    : MakeBinary(node.NodeType, node.Left, parameter),
                                Call(
                                    genericMethod,
                                    parameter,
                                    convertedVariable)
                            ));
                    }

                    var visitedLeft = Visit(node.Left);
                    return node.NodeType == ExpressionType.Assign
                        && visitedLeft is MemberExpression me
                            ? me.Assign(parameter!)
                            : MakeBinary(node.NodeType, visitedLeft, parameter!);
                }

                return base.VisitBinary(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
                => IsValueBufferTryReadValueMethodCall(methodCallExpression, out var property)
                && mappedProperties.TryGetValue(property, out var parameter)
                    ? ConvertIfNotMatch(parameter, methodCallExpression.Type)
                    : base.VisitMethodCall(methodCallExpression);
        }
    }
}
