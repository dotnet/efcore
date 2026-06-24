// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed partial class ShaperProcessingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo CollectionAccessorGetOrCreateMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate))!;

        private static readonly MethodInfo CollectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Add))!;

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

        private static readonly PropertyInfo ReadOnlyMemorySpanProperty
            = typeof(ReadOnlyMemory<byte>).GetProperty(nameof(ReadOnlyMemory<>.Span)) ?? throw new UnreachableException();

        private static readonly MethodInfo ReadOnlyMemorySliceMethod
            = typeof(ReadOnlyMemory<byte>).GetMethod(nameof(ReadOnlyMemory<>.Slice), [typeof(int)]) ?? throw new UnreachableException();

        private static readonly MethodInfo ByteArrayAsSpanMethod = typeof(MemoryExtensions).GetMethods()
            .Where(x => x.Name == nameof(MemoryExtensions.AsSpan) && x.GetGenericArguments().Count() == 1)
            .Select(x => new { x, prms = x.GetParameters() })
            .Where(x => x.prms.Count() == 1 && x.prms[0].ParameterType.IsArray)
            .Single().x.MakeGenericMethod(typeof(byte));

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

        /// <summary>
        ///     Structural types their shaper lambda.
        /// </summary>
        private readonly Dictionary<ITypeBase, LambdaExpression>
            _structuralTypeJsonShaperLambdaMapping = new();

        /// <summary>
        ///     Structural types their materializer lambda.
        /// </summary>
        private readonly Dictionary<ITypeBase, LambdaExpression>
            _structuralTypeJsonMaterializerLambdaMapping = new();

        private readonly Dictionary<ProjectionBindingExpression, (ParameterExpression Variable, LambdaExpression Materializer)>
            _deferredProjectionBindings = new();

        // ValueBuffer: JsonReaderManager
        private readonly Dictionary<Expression, ParameterExpression>
            _valueBufferToJsonReaderDataMapping = new(); // @TODO: Remove...

        // MaterializationContext: JsonReaderManager
        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _materializationContextToJsonReaderDataMapping = new();

        // MaterializationContext: KeyValues
        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _jsonMaterializationContextToKeyValuesMapping = new();

        private readonly Dictionary<Expression, ParameterExpression>
            _valueBufferToKeyValuesMapping = new();

        // JsonReaderData: JsonReaderManager
        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _jsonReaderDataToJsonReaderManagerParameterMapping = new();

        private readonly HashSet<ParameterExpression> _hasNullKeys = [];

        private readonly ParameterExpression _jsonReaderDataParameter = Parameter(typeof(JsonReaderData), "jsonReaderData");

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

        public LambdaExpression ProcessShaper(
            Expression shaperExpression)
        {
            var processedShaperExpression = Visit(shaperExpression);

            if (_deferredProjectionBindings.Count != 0)
            {
                var tokenTypeVariable = Parameter(typeof(JsonTokenType), "tokenType");
                var jsonReaderVariable = Parameter(typeof(Utf8JsonReader), "jsonReader");

                var shaperBlockVariables = new List<ParameterExpression>(_deferredProjectionBindings.Values.Select(x => x.Variable))
                {
                    _jsonReaderDataParameter,
                    tokenTypeVariable,
                    jsonReaderVariable
                };
                var shaperBlockExpressions = new List<Expression>()
                {
                    Assign(_bytesConsumedParameter, Constant(0)), // bytesConsumed = 0;
                    AssignJsonReaderVariableExpression(), // jsonReader = new Utf8JsonReader(data.Span, isFinalBlock: true, state: default);
                    Call(jsonReaderVariable, Utf8JsonReaderReadMethod), // jsonReader.Read();
                    Assign(tokenTypeVariable, Property(jsonReaderVariable, Utf8JsonReaderTokenTypeProperty)), // tokenType = jsonReader.TokenType;
                    IfThen(NotEqual(tokenTypeVariable, Constant(JsonTokenType.StartObject)), Throw(New(typeof(InvalidOperationException)))), // @TODO: Invalid json exception
                };

                BinaryExpression AssignJsonReaderVariableExpression()
                    => Assign(jsonReaderVariable, New(Utf8JsonReaderConstructor, Property(_dataParameter, ReadOnlyMemorySpanProperty), Constant(true), Default(typeof(JsonReaderState))));

                var groups = _deferredProjectionBindings.GroupBy(x => GetProjectionIndex(x.Key)); // @TODO: Get projection index?

                foreach (var group in groups)
                {
                    shaperBlockExpressions.AddRange([
                        Call(jsonReaderVariable, Utf8JsonReaderReadMethod), // jsonReader.Read();
                        .. AddBytesConsumedExpressions(Property(jsonReaderVariable, Utf8JsonReaderBytesConsumedProperty)),
                    ]);

                    foreach (var (projectionBindingExpression, (variable, materializer)) in group)
                    {
                        shaperBlockExpressions.AddRange([
                            Assign(_jsonReaderDataParameter, New(JsonReaderDataConstructor, _dataParameter)), // jsonReaderData = new JsonReaderData(data)
                            Assign(variable, Invoke(materializer, QueryCompilationContext.QueryContextParameter, _jsonReaderDataParameter)) // variable = materializer(QueryContext, jsonReaderData)
                            ]);
                    }

                    shaperBlockExpressions.AddRange([
                        ..AddBytesConsumedExpressions(Property(_jsonReaderDataParameter, JsonReaderDataBytesConsumedProperty)),
                        AssignJsonReaderVariableExpression(), // jsonReader = new Utf8JsonReader(data.Span, isFinalBlock: true, state: default);
                    ]);
                }

                Expression[] AddBytesConsumedExpressions(Expression bytesConsumedExpression) =>
                [
                    AddAssignChecked(_bytesConsumedParameter, bytesConsumedExpression),
                    Assign(_dataParameter, Call(_dataParameter, ReadOnlyMemorySliceMethod, bytesConsumedExpression))
                ];

                shaperBlockExpressions.Add(processedShaperExpression);
                processedShaperExpression = Block(shaperBlockVariables, shaperBlockExpressions);
            }

            var shaperLambda = Lambda(
                typeof(Shaper<>).MakeGenericType(shaperExpression.Type),
                processedShaperExpression,
                QueryCompilationContext.QueryContextParameter,
                _dataParameter,
                _bytesConsumedParameter);
            return shaperLambda;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case StructuralTypeShaperExpression shaper:
                {
                    var shaperLambda = StructuralTypeJsonShaper(shaper);

                    if (shaper.ValueBufferExpression is ProjectionBindingExpression projectionBindingExpression) // Otherwise this is an inner shaper of a CollectionShaperExpression,
                    {
                        var projection = GetProjection(projectionBindingExpression);
                        if (!projection.IsValueProjection && projection.Alias != null) // There are multiple projections in the document, so we have to defer the projection to a variable assignment when reading the parent document. See: ProcessShaper
                        {
                            var variable = Variable(shaper.Type, projection.Alias);
                            _deferredProjectionBindings[projectionBindingExpression] = (variable, shaperLambda);
                            return variable;
                        }
                    }

                    return Invoke(shaperLambda, QueryCompilationContext.QueryContextParameter, _jsonReaderDataParameter);
                }

                case ProjectionBindingExpression projectionBindingExpression:
                {
                    var (jsonReaderData, jsonReaderManager, jsonReaderInitializeExpessions) = GenerateJsonReader();

                    var projection = GetProjection(projectionBindingExpression);
                    var typeMapping = ((SqlExpression)projection.Expression).TypeMapping!;
                    var returnValue = Variable(projectionBindingExpression.Type, "returnValue");

                    Expression jsonMaterializeExpression = Block(
                        [jsonReaderData, jsonReaderManager, returnValue],
                        [ ..jsonReaderInitializeExpessions,
                            Assign(
                                returnValue,
                                CreateReadJsonValueExpression(jsonReaderManager, returnValue.Type, typeMapping)),
                            Call(jsonReaderManager, Utf8JsonReaderManagerCaptureStateMethod),
                            returnValue]);

                    if (!projection.IsValueProjection && projection.Alias != null) 
                    {
                        jsonMaterializeExpression = GenerateExtractPath(jsonMaterializeExpression, _dataParameter, [projection.Alias]);
                    }

                    return jsonMaterializeExpression;
                }

                case CollectionShaperExpression collectionShaperExpression:
                {
                    var innerShaper = ProcessShaper(collectionShaperExpression.InnerShaper);

                    Expression jsonMaterializeExpression = Call(
                        ReadShapedCollectionMethod.MakeGenericMethod(collectionShaperExpression.ElementType, collectionShaperExpression.Type),
                        QueryCompilationContext.QueryContextParameter,
                        _dataParameter,
                        Constant(collectionShaperExpression.CollectionCreator),
                        innerShaper);

                    var projection = GetProjection((ProjectionBindingExpression)collectionShaperExpression.Projection);
                    if (!projection.IsValueProjection && projection.Alias != null)
                    {
                        jsonMaterializeExpression = GenerateExtractPath(jsonMaterializeExpression, _dataParameter, [projection.Alias]);
                    }

                    return jsonMaterializeExpression;

                }
                case IncludeExpression includeExpression:
                    return Visit(includeExpression.EntityExpression);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            switch (binaryExpression)
            {
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
                    when parameterExpression.Type == typeof(MaterializationContext):
                {
                    // Rewrites the materialization context instantiation to use an empty value buffer
                    // and adds mapping between materialization context parameter and json reader data parameter,
                    // so when we encounter an expression that tries to read from value buffer we know which json reader data to rewrite the read to instead.
                    var newExpression = (NewExpression)binaryExpression.Right;
                    var valueBufferExpression = newExpression.Arguments[0];
                    _materializationContextToJsonReaderDataMapping[parameterExpression] = _valueBufferToJsonReaderDataMapping[valueBufferExpression];

                    if (_valueBufferToKeyValuesMapping.TryGetValue(valueBufferExpression, out var keyValuesParameter))
                    {
                        _jsonMaterializationContextToKeyValuesMapping[parameterExpression] = keyValuesParameter;
                    }

                    var valueBuffer = Constant(ValueBuffer.Empty);
                    var updatedExpression = newExpression.Update(
                    [
                        valueBuffer,
                        newExpression.Arguments[1]
                    ]);

                    return Assign(binaryExpression.Left, updatedExpression);
                }

                case
                {
                    NodeType: ExpressionType.Assign,
                    Left: MemberExpression { Member: FieldInfo { IsInitOnly: true } } memberExpression
                }:
                {
                    return memberExpression.Assign(Visit(binaryExpression.Right));
                }

                // we only have mapping between MaterializationContext and JsonReaderData, but we use JsonReaderManager to extract JSON
                // values so we need to add mapping between JsonReaderData and JsonReaderManager parameter, so we know which parameter to
                // use when generating actual Get* method
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression jsonReaderManagerParameter }
                    when jsonReaderManagerParameter.Type == typeof(Utf8JsonReaderManager):
                {
                    var jsonReaderDataParameter = (ParameterExpression)((NewExpression)binaryExpression.Right).Arguments[0];
                    _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderDataParameter] = jsonReaderManagerParameter;
                    break;
                }
            }

            return base.VisitBinary(binaryExpression);
        }

        //protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        //{
        //    // Converts valueBuffer.TryReadValue to jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
        //    if (methodCallExpression.Method.IsGenericMethod
        //        && methodCallExpression.Method.GetGenericMethodDefinition()
        //        == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
        //    {
        //        //var materializationContext = (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object!;
        //        //var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty>();

        //        //var jsonReaderData = _materializationContextToJsonReaderDataMapping[materializationContext];
        //        //var jsonReaderManager = _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderData];

        //        //var jsonReadPropertyValueExpression = CreateReadJsonPropertyValueExpression(jsonReaderManager, property);

        //        //return ConvertIfNotMatch(jsonReadPropertyValueExpression, methodCallExpression.Type);
        //    }

        //    return base.VisitMethodCall(methodCallExpression);
        //}

        private LambdaExpression StructuralTypeJsonShaper(StructuralTypeShaperExpression shaper)
        {
            if (_structuralTypeJsonShaperLambdaMapping.TryGetValue(shaper.StructuralType, out var lambda)) // @TODO: We might need to cache per nullable aswell?
            {
                return lambda;
            }

            var materializer = StructuralTypeJsonMaterializer(
                shaper.StructuralType,
                shaper.Type,
                shaper.IsNullable,
                shaper.ValueBufferExpression);

            if (!_isTracking)
            {
                // Materializer will be generated as a simple deserialize for non tracking queries.
                // We can return the materializer directly as the shaper, since we don't need to read any metadata properties from the document.
                return materializer;
            }


            var shaperVariables = new List<ParameterExpression>();
            var shaperExpressions = new List<Expression>();

            // @TODO: Do we want to get this from the injection or rebuild ourselfs?

            // Always returns only the instance

            lambda = Lambda(
                Block(shaperVariables, shaperExpressions),
                QueryCompilationContext.QueryContextParameter,
                _jsonReaderDataParameter);
            _structuralTypeJsonShaperLambdaMapping.Add(shaper.StructuralType, lambda);

            return lambda;
        }

        private LambdaExpression StructuralTypeJsonMaterializer(
            ITypeBase structuralType,
            Type clrType,
            bool nullable,
            Expression valueBufferExpression)
        {
            if (_structuralTypeJsonMaterializerLambdaMapping.TryGetValue(structuralType, out var lambda)) // @TODO: We might need to cache per nullable aswell?
            {
                return lambda;
            }

            var jsonReaderMangagerVariable = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
            var materializerVariables = new List<ParameterExpression>()
            {
                jsonReaderMangagerVariable
            };
            var materializerExpressions = new List<Expression>()
            {
                Assign(jsonReaderMangagerVariable, NewJsonReaderManager()),
            };

            var structuralTypeShaperExpression = new StructuralTypeShaperExpression(
                structuralType,
                valueBufferExpression,
                nullable);

            var materializerBlock =
                (BlockExpression)_parentVisitor.InjectStructuralTypeMaterializers(structuralTypeShaperExpression);

            var discriminatorProperty = structuralType.FindDiscriminatorProperty();
            if (discriminatorProperty != null)
            {
                // We have to read the json document to find the discriminator value before we can know how to deserialize the document.
                var (discriminatorReadLoopExpression, discriminatorValueVariable) = ReadDiscriminator(structuralType, jsonReaderMangagerVariable, discriminatorProperty);

                materializerVariables.Add(discriminatorValueVariable);
                materializerExpressions.Add(discriminatorReadLoopExpression);
                materializerExpressions.Add(Assign(jsonReaderMangagerVariable, NewJsonReaderManager())); // Start reading from the beginning for the actual materializer block.

                // Replace calls for ValueBufferTryReadValue for the discriminator property
                materializerBlock = new ValueBufferTryReadValueRewriter(new Dictionary<IProperty, ParameterExpression>
                {
                    { discriminatorProperty, discriminatorValueVariable }
                }).Rewrite(materializerBlock);
            } // @TODO: Optimize for only 1 possible value, do check when finding property...

            var requiresTracking = _queryStateManager && structuralType is IEntityType entityType;
            var trackingActions = Variable(typeof(List<Action>), "trackingActions");
            if (requiresTracking) // @TODO: How does this work for complex types again?
            {
                materializerVariables.Add(trackingActions);

                // We can't do tracking till after the entity is materialized
                // So we remove the tracking code from the materializer block and store it for later use

                // @TODO: we want to store these somewhere in a mapping, and also trackingActions...?

                var entryVariable = materializerBlock.Variables.Single(x => x.Type == typeof(InternalEntityEntry));
                var hasNullKeyVariable = materializerBlock.Variables.Single(x => x.Type == typeof(bool));

                var entryAssignment = materializerBlock.Expressions.OfType<BinaryExpression>()
                    .Single(x => x.NodeType == ExpressionType.Assign && x.Left == entryVariable);
                var tryGetEntryCall = (MethodCallExpression)entryAssignment.Right;

                var hasNullKeyCheck = materializerBlock.Expressions.OfType<ConditionalExpression>()
                    .Single(x => x.Test is UnaryExpression { NodeType: ExpressionType.Not } ue && ue.Operand == hasNullKeyVariable);
                var entryNotNullCheck = (ConditionalExpression)hasNullKeyCheck.IfTrue;
                var entryNotNullBlock = (BlockExpression)entryNotNullCheck.IfTrue;
                var readValuesBlock = (BlockExpression)entryNotNullCheck.IfFalse;

                var shadowSnapshotVariable = readValuesBlock.Variables.SingleOrDefault(x => x.Type == typeof(ISnapshot));

                materializerBlock = (BlockExpression)Visit(readValuesBlock); // @TODO: Visit here? How do we rewrite correctly?
            }
            else
            {
                // @TODO: Test if we can just visit the materializer block directly here or if we need some modifications as well.
                materializerBlock = (BlockExpression)Visit(materializerBlock);
            }

            materializerVariables.AddRange(materializerBlock.Variables);
            materializerExpressions.AddRange(materializerBlock.Expressions);

            // If tracking, this returns (IEntityType, RootEntity, ISnapshot, List<Action>)
            // Else this only returns RootEntity (clrType)
            if (requiresTracking)
            {
                var instanceVariable = (ParameterExpression)materializerBlock.Expressions[^1];
                var snapshotVariable = materializerBlock.Variables.Single(x => x.Type == typeof(ISnapshot));
                var entityTypeVariable = materializerBlock.Variables.Single(x => x.Type == typeof(IEntityType));

                materializerExpressions.Add(
                    New(
                        typeof(ValueTuple<,,,>).MakeGenericType(typeof(IEntityType), clrType, typeof(ISnapshot), typeof(List<Action>))
                            .GetConstructor([typeof(IEntityType), clrType, typeof(ISnapshot), typeof(List<Action>)]),
                        entityTypeVariable,
                        instanceVariable,
                        snapshotVariable,
                        trackingActions));
            }

            lambda = Lambda(
                Block(materializerVariables, materializerExpressions),
                QueryCompilationContext.QueryContextParameter,
                _jsonReaderDataParameter);

            _structuralTypeJsonMaterializerLambdaMapping.Add(structuralType, lambda);

            return lambda;
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
            //sometimes we have shadow snapshot and sometimes not, but type initializer always comes last
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

            var managerVariable = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
            var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");
            var jsonStructuralTypeVariable = (ParameterExpression)jsonEntityTypeInitializerBlock.Expressions[^1];

            Debug.Assert(jsonStructuralTypeVariable.Type.IsAssignableFrom(structuralType.ClrType));

            var finalBlockVariables = new List<ParameterExpression>
                {
                    managerVariable, tokenTypeVariable,
                };

            finalBlockVariables.AddRange(jsonEntityTypeInitializerBlock.Variables);

            var finalBlockExpressions = new List<Expression>
                {
                    // jsonReaderManager = new Utf8JsonReaderManager(jsonReaderData))
                    Assign(
                        managerVariable,
                        New(
                            JsonReaderManagerConstructor,
                            jsonReaderDataParameter,
                            MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty))),
                    // tokenType = jsonReaderManager.CurrentReader.TokenType
                    Assign(
                        tokenTypeVariable,
                        Property(
                            Field(
                                managerVariable,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderTokenTypeProperty)),
                };

            var (loop, propertyAssignmentMap) = GenerateJsonPropertyReadLoop(
                managerVariable,
                tokenTypeVariable,
                finalBlockVariables,
                valueBufferTryReadValueMethodsToProcess);

            finalBlockExpressions.Add(loop);

            var finalCaptureState = Call(managerVariable, Utf8JsonReaderManagerCaptureStateMethod);
            finalBlockExpressions.Add(finalCaptureState);

            // we have the loop, now we can add code that generate the entity instance
            // will have to replace ValueBufferTryReadValue method calls with the parameters that store the value
            // we can't use simple ExpressionReplacingVisitor, because there could be multiple instances of MethodCallExpression for given property
            // using dedicated mini-visitor that looks for MCEs with a given shape and compare the IProperty inside
            // order is:
            // - shadow snapshot (if there was one)
            // - entity construction / property assignments
            // - navigation fixups
            // - entity instance variable that is returned as end result
            var propertyAssignmentReplacer = new ValueBufferTryReadValueMethodsReplacer(
                jsonStructuralTypeVariable, propertyAssignmentMap);

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
                finalBlockExpressions.Add(propertyAssignmentReplacer.Visit(shadowSnapshotAssignment));
            }

            foreach (var jsonEntityTypeInitializerBlockExpression in jsonEntityTypeInitializerBlock.Expressions.ToArray()[..^1])
            {
                finalBlockExpressions.Add(propertyAssignmentReplacer.Visit(jsonEntityTypeInitializerBlockExpression));
            }

            // Fixup is only needed for non-tracking queries, in case of tracking (or NoTrackingWithIdentityResolution) - ChangeTracker does the job
            // or for empty/null collections of a tracking queries.
            ProcessFixup(queryStateManager ? trackingInnerFixupMap : innerFixupMap);

            finalBlockExpressions.Add(jsonStructuralTypeVariable);

            return Block(
                finalBlockVariables,
                finalBlockExpressions);

            void ProcessFixup(IDictionary<IPropertyBase, LambdaExpression> fixupMap)
            {
                foreach (var fixup in fixupMap)
                {
                    if (!navigationVariableMap.TryGetValue(fixup.Key, out var navigationEntityParameter))
                    {
                        // The navigation was not used in this materializer, it might be used by another type.
                        continue;
                    }

                    // Inject the fixup code for each property; we have this as a set of lambdas in the fixup map.
                    // In the normal case, simply Invoke the lambda, passing it the structural type to be fixed up as a parameter.
                    // This unfortunately doesn't work on value types (where a copy would be mutated), so for them,
                    // we unwrap the lambda and integrate its body directly.
                    // We should ideally do this for all cases (no need for the extra lambda Invoke), but there are some issues around us writing
                    // to readonly fields.
                    if (jsonStructuralTypeVariable.Type.IsValueType /*&& Nullable.GetUnderlyingType(fixup.Value.Parameters[1].Type) is null*/)
                    {
                        // No convert because it is a value type and inheritance is not supported for complex properties / value types.
                        var fixupBody = ReplacingExpressionVisitor.Replace(
                            originals: [fixup.Value.Parameters[0], fixup.Value.Parameters[1]],
                            replacements: [jsonStructuralTypeVariable, navigationEntityParameter],
                            fixup.Value.Body);

                        finalBlockExpressions.Add(fixupBody);
                    }
                    else
                    {
                        // Need to convert because fixup expects declaring type
                        var convertedJsonStructuralTypeVariable = Convert(jsonStructuralTypeVariable, fixup.Value.Parameters[0].Type);

                        // If the structural type being fixed up is nullable, then we need to add null checks before we run fixup logic.
                        // For regular entities, whose fixup is done as part of the "Materialize*" method, the checks are done there
                        // (the same will be done for the "optimized" scenario, where we populate properties directly rather than store in variables).
                        // But in this case fixups are standalone, so the null safety must be added here.
                        finalBlockExpressions.Add(
                            IfThen(
                                NotEqual(convertedJsonStructuralTypeVariable, Constant(null, convertedJsonStructuralTypeVariable.Type)),
                                Invoke(
                                    fixup.Value,
                                    convertedJsonStructuralTypeVariable,
                                    navigationEntityParameter)));
                    }
                }
            }

            // builds a loop that extracts values of JSON properties and assigns them into variables
            // also injects entity shapers (generated earlier) for child navigations
            // returns the loop expression and mappings for properties (so we know which calls to replace with variables)
            (LoopExpression, Dictionary<IProperty, ParameterExpression>) GenerateJsonPropertyReadLoop(
                ParameterExpression managerVariable,
                ParameterExpression tokenTypeVariable,
                List<ParameterExpression> finalBlockVariables,
                List<MethodCallExpression> valueBufferTryReadValueMethodsToProcess)
            {
                var breakLabel = Label("done");
                var testExpressions = new List<Expression>();
                var readExpressions = new List<Expression>();
                var propertyAssignmentMap = new Dictionary<IProperty, ParameterExpression>();

                foreach (var valueBufferTryReadValueMethodToProcess in valueBufferTryReadValueMethodsToProcess)
                {
                    var property = valueBufferTryReadValueMethodToProcess.Arguments[2].GetConstantValue<IProperty>();
                    var jsonPropertyName = property.GetJsonPropertyName();
                    if (jsonPropertyName == string.Empty) // non persisted property
                    {
                        continue;
                    }
                    testExpressions.Add(
                        Call(
                            Field(
                                managerVariable,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderValueTextEqualsMethod,
                            Convert(
                                Call(
                                    ByteArrayAsSpanMethod,
                                    Constant(Encoding.UTF8.GetBytes(jsonPropertyName))),
                                typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                    var propertyVariable = Variable(valueBufferTryReadValueMethodToProcess.Type);

                    finalBlockVariables.Add(propertyVariable);

                    var moveNext = Call(
                        managerVariable,
                        Utf8JsonReaderManagerMoveNextMethod);

                    var assignment = Assign(
                        propertyVariable,
                        valueBufferTryReadValueMethodToProcess);

                    readExpressions.Add(
                        Block(
                            moveNext,
                            assignment,
                            Empty()));

                    propertyAssignmentMap[property] = propertyVariable;
                }

                foreach (var innerShaperMapElement in innerShapersMap)
                {
                    if (!innerShaperMapElement.Key.DeclaringType.IsAssignableFrom(structuralType))
                    {
                        continue;
                    }

                    var propertyName = innerShaperMapElement.Key switch
                    {
                        IComplexProperty complexProperty => complexProperty.GetJsonPropertyName(),
                        INavigation navigation => navigation.TargetEntityType.GetContainingPropertyName()!,
                        _ => throw new UnreachableException()
                    };
                    testExpressions.Add(
                        Call(
                            Field(
                                managerVariable,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderValueTextEqualsMethod,
                            Convert(
                                Call(
                                    ByteArrayAsSpanMethod,
                                    Constant(Encoding.UTF8.GetBytes(propertyName))),
                                typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                    var propertyVariable = Variable(innerShaperMapElement.Value.Type);
                    finalBlockVariables.Add(propertyVariable);

                    navigationVariableMap[innerShaperMapElement.Key] = propertyVariable;

                    var moveNext = Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod);
                    var captureState = Call(managerVariable, Utf8JsonReaderManagerCaptureStateMethod);
                    var assignment = Assign(propertyVariable, innerShaperMapElement.Value);
                    var managerRecreation = Assign(
                        managerVariable,
                        New(
                            JsonReaderManagerConstructor,
                            jsonReaderDataParameter,
                            MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty)));

                    readExpressions.Add(
                        Block(
                            moveNext,
                            captureState,
                            assignment,
                            managerRecreation,
                            Empty()));
                }

                var switchCases = new List<SwitchCase>();
                var testsCount = testExpressions.Count;

                // generate PropertyName switch-case code
                if (testsCount > 0)
                {
                    var testExpression = IfThen(
                        testExpressions[testsCount - 1],
                        readExpressions[testsCount - 1]);

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

                switchCases.Add(
                    SwitchCase(
                        Break(breakLabel),
                        Constant(JsonTokenType.EndObject)));

                var loopBody = Block(
                    Assign(tokenTypeVariable, Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                    Switch(
                        tokenTypeVariable,
                        Block(
                            Call(managerVariable, Utf8JsonReaderManagerSkipMethod),
                            Default(typeof(void))),
                        switchCases.ToArray()));

                return (Loop(loopBody, breakLabel), propertyAssignmentMap);
            }
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
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition()
                    == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                    && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty property
                    && _properties.Contains(property))
                {
                    _valueBufferTryReadValueMethods.Add(methodCallExpression);
                    _properties.Remove(property);

                    return methodCallExpression;
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        private sealed class ValueBufferTryReadValueMethodsReplacer(
            Expression instance,
            Dictionary<IProperty, ParameterExpression> propertyAssignmentMap)
            : ExpressionVisitor
        {
            protected override Expression VisitBinary(BinaryExpression node)
            {
                if (node.Right is MethodCallExpression methodCallExpression
                    && IsPropertyAssignment(methodCallExpression, out var property, out var parameter))
                {
                    if (parameter == null)
                    {
                        return Empty();
                    }

                    if (property!.IsPrimitiveCollection
                        && !property.ClrType.IsArray)
                    {
#pragma warning disable EF1001 // Internal EF Core API usage.
                        var genericMethod = StructuralTypeMaterializerSource.PopulateListMethod.MakeGenericMethod(
                            property.ClrType.TryGetElementType(typeof(IEnumerable<>))!);
#pragma warning restore EF1001 // Internal EF Core API usage.
                        var currentVariable = Variable(parameter.Type);
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
                        && visitedLeft is MemberExpression memberExpression
                            ? memberExpression.Assign(parameter!)
                            : MakeBinary(node.NodeType, visitedLeft, parameter!);
                }

                return base.VisitBinary(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
                => IsPropertyAssignment(methodCallExpression, out _, out var parameter)
                    ? parameter ?? Default(methodCallExpression.Type)
                    : base.VisitMethodCall(methodCallExpression);

            private bool IsPropertyAssignment(
                MethodCallExpression methodCallExpression,
                [NotNullWhen(true)] out IProperty? property,
                out Expression? parameter)
            {
                if (methodCallExpression.Method.IsGenericMethod
                    && methodCallExpression.Method.GetGenericMethodDefinition()
                    == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                    && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty prop)
                {
                    property = prop;
                    parameter = propertyAssignmentMap.TryGetValue(prop, out var param) ? param : null;
                    return true;
                }

                property = null;
                parameter = null;
                return false;
            }
        }

        //private BlockExpression Rewrite(BlockExpression materializeExpression)
        //{
        //    if (!_queryStateManager)
        //    {
        //        return (BlockExpression)Visit(materializeExpression); // @TODO: test...
        //    }

        //    var variables = new List<ParameterExpression>(materializeExpression.Variables);

        //    var entryVariable = variables.Single(x => x.Type == typeof(InternalEntityEntry));
        //    var hasNullKeyVariable = variables.Single(x => x.Type == typeof(bool));

        //    variables.Remove(entryVariable);
        //    variables.Remove(hasNullKeyVariable);

        //    // @TODO: Do we want to store these somewhere?
        //    var entryAssignment = materializeExpression.Expressions.OfType<BinaryExpression>()
        //        .Single(x => x.NodeType == ExpressionType.Assign && x.Left == entryVariable);
        //    var tryGetEntryCall = (MethodCallExpression)entryAssignment.Right;

        //    var hasNullKeyCheck = materializeExpression.Expressions.OfType<ConditionalExpression>()
        //        .Single(x => x.Test is UnaryExpression { NodeType: ExpressionType.Not } ue && ue.Operand == hasNullKeyVariable);
        //    var entryNotNullCheck = (ConditionalExpression)hasNullKeyCheck.IfTrue;
        //    var entryNotNullBlock = (BlockExpression)entryNotNullCheck.IfTrue;
        //    var readValuesBlock = (BlockExpression)entryNotNullCheck.IfFalse;

        //    var rewrittenReadValuesBlock = (BlockExpression)Visit(readValuesBlock);

        //    return rewrittenReadValuesBlock;
        //}

        private (Expression, ParameterExpression) ReadDiscriminator(ITypeBase structuralType, ParameterExpression jsonReaderMangagerVariable, IProperty discriminatorProperty)
        {
            // @TODO: Change serializer to put discriminator first
            // Generate a read loop to get the discriminator
            // string discriminatorValue = null;
            // while (true)
            //  tokenType = jsonReaderManager.MoveNext();
            //  switch($tokenType) {
            //      case(JsonTokenType.EndObject):
            //          goto EndRead;
            //      case(JsonTokenType.PropertyName):
            //          if (jsonReaderManager.CurrentReader.ValueTextEquals("$type"u8))
            //              jsonReaderManager.MoveNext();
            //              discriminatorValue = jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
            //              goto EndRead;
            //          else if (jsonReaderManager.CurrentReader.ValueTextEquals("Id"u8))
            //              jsonReaderManager.Skip(); // @TODO: was it 1 or 2 skips?
            //              jsonReaderManager.Skip();
            //          else
            //              throw new InvalidOperationException("Discriminator was not early in the document.");
            //      default:
            //          throw invalid json
            // EndRead:


            var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");
            var discriminatorBlockVariables = new List<ParameterExpression>()
            {
                tokenTypeVariable
            };
            
            var discriminatorValueVariable = Variable(discriminatorProperty.ClrType.MakeNullable(), "discriminatorValue"); // Not a local variable, but defined by the parent block.

            var metadataBlockExpressions = new List<Expression>
            {
                 // string? discriminatorValue = defalt();
                Assign(discriminatorValueVariable, Default(discriminatorProperty.ClrType.MakeNullable()))
            };

            var breakLabel = Label("EndRead");

            var propertyJsonValueReaderWriter = discriminatorProperty.GetJsonValueReaderWriter() ?? discriminatorProperty.GetTypeMapping().JsonValueReaderWriter;
            Debug.Assert(propertyJsonValueReaderWriter != null, "Cosmos provider should always provide a JsonValueReaderWriter for all scalar properties");
            var propertyJsonValueReaderWriterConstant = Constant(propertyJsonValueReaderWriter);

            var fromJsonMethod = propertyJsonValueReaderWriterConstant.Type.GetMethod(
                nameof(JsonValueReaderWriter<>.FromJsonTyped),
                [typeof(Utf8JsonReaderManager).MakeByRefType(), typeof(object)])!;

            // @TODO: throwOnLateType ? Throw(New(typeof(InvalidOperationException))) : Empty();
            var ifNotPropertyMatchThrow = Throw(New(typeof(InvalidOperationException))); // @TODO: message: Discriminator was not early in the document.

            return (
                Loop(
                    Block([], [
                        // tokenType = jsonReaderManager.MoveNext();
                        Assign(tokenTypeVariable, Call(jsonReaderMangagerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                        // switch (tokenType)
                        Switch(tokenTypeVariable,
                            // default: throw @todo: invalid json
                            Throw(New(typeof(InvalidOperationException)), typeof(void)),
                            [
                                // case EndObject: goto EndRead
                                SwitchCase(Break(breakLabel, typeof(void)), Constant(JsonTokenType.EndObject)),
                                // case PropertyName:
                                SwitchCase(
                                    // if (jsonReaderManager.CurrentReader.ValueTextEquals(("$type"u8).Span))
                                    IfThenElse(
                                        JsonReaderValueTextEquals(jsonReaderMangagerVariable, discriminatorProperty.GetJsonPropertyName()),
                                        Block(
                                            // jsonReaderManager.MoveNext()
                                            Call(jsonReaderMangagerVariable, Utf8JsonReaderManagerMoveNextMethod),
                                            // discriminatorValue = jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
                                            Assign(
                                                discriminatorValueVariable,
                                                CheckMakeNullableValueType(
                                                    Call(propertyJsonValueReaderWriterConstant, fromJsonMethod, jsonReaderMangagerVariable, Default(typeof(object))))),
                                            // goto EndRead
                                            Break(breakLabel, typeof(void))),
                                        structuralType is IEntityType entityType && entityType.FindPrimaryKey() is { } primaryKey // Allow primary keys to come before discriminator, for backwards compatibility.
                                            // else if (jsonReaderManager.CurrentReader.ValueTextEquals(("Id"u8).Span))
                                            ? IfThenElse(
                                                primaryKey.Properties
                                                    .Select(p => JsonReaderValueTextEquals(jsonReaderMangagerVariable, p.GetJsonPropertyName()))
                                                    .Aggregate<MethodCallExpression, Expression?>(
                                                        null,
                                                        (previous, next) => previous is null ? next : OrElse(previous, next))!,
                                                    // jsonReaderManager.Skip() x2
                                                    Block(Enumerable.Range(0, 2).Select(_ => Call(jsonReaderMangagerVariable, Utf8JsonReaderManagerSkipMethod))),
                                                // else throw new InvalidOperationException("Discriminator was not early in the document.")
                                                ifNotPropertyMatchThrow)
                                            : ifNotPropertyMatchThrow),
                                    Constant(JsonTokenType.PropertyName))])]),
                    breakLabel),
                discriminatorValueVariable);
        }

        private NewExpression NewJsonReaderManager()
            => New(JsonReaderManagerConstructor, _jsonReaderDataParameter, MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty));

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

        private static MethodCallExpression JsonReaderValueTextEquals(ParameterExpression jsonReaderManagerVariable, string text)
            => Call(
                Field(
                    jsonReaderManagerVariable,
                    Utf8JsonReaderManagerCurrentReaderField),
                Utf8JsonReaderValueTextEqualsMethod,
                StringConstantSpan(text));


        private class JsonStructuralTypeMaterializerRewriter(ShaperProcessingExpressionVisitor parentVisitor) : ExpressionVisitor
        {
            
        }


















        private (ParameterExpression jsonReaderData, ParameterExpression jsonReaderManager, Expression[] jsonReaderInitializeExpessions) GenerateJsonReader()
        {
            var jsonReaderData = Variable(typeof(JsonReaderData), "jsonReaderData");
            var jsonReaderManager = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
            var jsonReaderInitializeExpessions = new Expression[]
            {
                Assign(
                    jsonReaderData,
                    New(JsonReaderDataConstructor,
                        _dataParameter)),
                Assign(
                    jsonReaderManager,
                    New(JsonReaderManagerConstructor,
                        jsonReaderData,
                        MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty))),
                Call(jsonReaderManager, Utf8JsonReaderManagerMoveNextMethod),
            };

            return (jsonReaderData, jsonReaderManager, jsonReaderInitializeExpessions);
        }

        private Expression CreateJsonShapers(
            ITypeBase structuralType,
            Type clrType,
            bool nullable,
            Expression valueBufferExpression,
            (ParameterExpression Parameter, Type Type) keyValues = default)
        {
            var shaperBlockVariables = new List<ParameterExpression>();
            var shaperBlockExpressions = new List<Expression>();
            var jsonReaderDataShaperLambdaParameter = _valueBufferToJsonReaderDataMapping[valueBufferExpression];

            var noKeyValues = keyValues.Equals(default);
            if (noKeyValues)
            {
                keyValues = (Parameter(typeof(ISnapshot), "keyValues"), typeof(ISnapshot));
            }
            else
            {
                Check.DebugAssert(keyValues.Parameter != null && keyValues.Type != null);
                _valueBufferToKeyValuesMapping[valueBufferExpression] = keyValues.Parameter!;
            }

            Expression structuralTypeShaperExpression = new StructuralTypeShaperExpression(
                structuralType,
                valueBufferExpression,
                nullable);

            var structuralTypeShaperMaterializer =
                (BlockExpression)_parentVisitor.InjectStructuralTypeMaterializers(structuralTypeShaperExpression);

            // Read metadata properties from the document (if needed)
            // We only need to read primary key values if this is the root entity, since owned entities their identity are defined by the owner identity, or the latter combined with their index in the collection.
            var isDocumentRoot = structuralType is IEntityType et && et.IsDocumentRoot();
            var extractKeyValues = _queryStateManager && isDocumentRoot;
            var discriminatorProperty = structuralType.FindDiscriminatorProperty(); // @TODO: Optimize, if there is only 1 discriminator value possible we don't need to read this..? We would need to check the discriminator value in the shaper later then tho to make sure something else isn't wrong (read wrong entity)
            if (discriminatorProperty != null
             || extractKeyValues)
            {
                // Generate a read loop to extract the metadata properties.
                var managerVariable = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
                var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");

                var metadataBlockExpressions = new List<Expression>
                {
                    // jsonReaderManager = new Utf8JsonReaderManager(jsonReaderData))
                    Assign(
                        managerVariable,
                        New(
                            JsonReaderManagerConstructor,
                            jsonReaderDataShaperLambdaParameter,
                            MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty))),
                    // tokenType = jsonReaderManager.CurrentReader.TokenType
                    Assign(
                        tokenTypeVariable,
                        Property(
                            Field(
                                managerVariable,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderTokenTypeProperty)),
                };

                // Generate a loop to get the metadata
                // string discriminatorValue = null;
                // int? idValue = null;
                // while (true)
                //  tokenType = jsonReaderManager.MoveNext();
                //  switch($tokenType) {
                //      case(JsonTokenType.PropertyName):
                //          if (jsonReaderManager.CurrentReader.ValueTextEquals("$type"u8))
                //              jsonReaderManager.MoveNext();
                //              discriminatorValue = jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
                //              if (idValue != null && discriminatorValue != null)
                //                  goto done
                //          else if (jsonReaderManager.CurrentReader.ValueTextEquals("Id"u8))
                //              jsonReaderManager.MoveNext();
                //              idValue = jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
                //              if (idValue != null && discriminatorValue != null)
                //                  goto done
                //          else if // @TODO: NESTED??
                //          else
                //              jsonReaderManager.Skip(); // @TODO: was it 1 or 2 skips?
                //              jsonReaderManager.Skip();
                //
                //      case (JsonTokenType.EndObject):
                //          goto done
                //      default:
                //          throw invalid json
                // label: done

                var propertiesToRead = new Dictionary<IProperty, ParameterExpression>();
                if (discriminatorProperty != null)
                {
                    propertiesToRead.Add(discriminatorProperty, CreateMetadataVariable(discriminatorProperty));
                }
                if (extractKeyValues)
                {
                    foreach (var property in ((IEntityType)structuralType).FindPrimaryKey()!.Properties)
                    {
                        propertiesToRead.Add(property, CreateMetadataVariable(property));
                    }
                }

                static ParameterExpression CreateMetadataVariable(IPropertyBase property)
                    => Variable(property.ClrType.MakeNullable(), property.Name + "Value");

                foreach (var (property, propertyValueVariable) in propertiesToRead)
                {
                    shaperBlockVariables.Add(propertyValueVariable);
                    metadataBlockExpressions.Add(Assign(propertyValueVariable, Default(property.ClrType.MakeNullable())));
                }

                var breakLabel = Label("MetadataRead");

                var ifDoneGotoBreak = IfThen(propertiesToRead.Select(p => p.Value).Select(p => NotEqual(p, Default(p.Type))).Aggregate(AndAlso), Goto(breakLabel)); // if (idValue != null && discriminatorValue != null) goto break

                var ifElseIfTree = propertiesToRead.Aggregate(
                    (Expression)Block(Enumerable.Repeat(Call(managerVariable, Utf8JsonReaderManagerSkipMethod), 2)), // @TODO: Or do we only need to skip once here?
                    (previous, propertyTuple) =>
                    {
                        var (property, propertyValueVariable) = propertyTuple;
                        var propertyJsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
                        Debug.Assert(propertyJsonValueReaderWriter != null, "Cosmos provider should always provide a JsonValueReaderWriter for all scalar properties");
                        var propertyJsonValueReaderWriterConstant = Constant(propertyJsonValueReaderWriter);

                        var fromJsonMethod = propertyJsonValueReaderWriterConstant.Type.GetMethod(
                            nameof(JsonValueReaderWriter<>.FromJsonTyped),
                            [typeof(Utf8JsonReaderManager).MakeByRefType(), typeof(object)])!;

                        ReadOnlyMemory<byte> propertyNameBytes = Encoding.UTF8.GetBytes(property.GetJsonPropertyName());
                        return IfThenElse( // if (jsonReaderManager.CurrentReader.ValueTextEquals("property".Span))
                            Call(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderValueTextEqualsMethod,
                                Property(Constant(propertyNameBytes), typeof(ReadOnlyMemory<byte>).GetProperty(nameof(ReadOnlyMemory<>.Span))!)),
                            Block(
                                Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod), // jsonReaderManager.MoveNext()
                                Assign(propertyValueVariable, CheckMakeNullableValueType(Call(propertyJsonValueReaderWriterConstant, fromJsonMethod, managerVariable, Default(typeof(object))))), // propertyValue = propertyjsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
                                ifDoneGotoBreak
                            ),
                            previous);

                        static Expression CheckMakeNullableValueType(Expression expression)
                            => expression.Type.IsValueType && !expression.Type.IsNullableValueType()
                                ? Convert(expression, expression.Type.MakeNullable())
                                : expression;
                    });

                metadataBlockExpressions.Add(
                    Loop(Block(
                        Assign(tokenTypeVariable, Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                        Switch( // switch(tokenType)
                            tokenTypeVariable,
                            Throw(New(typeof(Exception)), typeof(void)), // default throw @TODO: invalidjson exception
                            SwitchCase( // case JsonTokenType.PropertyName:
                                ifElseIfTree,
                                Constant(JsonTokenType.PropertyName)),
                            SwitchCase(Break(breakLabel, typeof(void)), Constant(JsonTokenType.EndObject)))), // case JsonTokenType.EndObject: break
                        breakLabel));

                shaperBlockExpressions.Add(
                    Block(
                        [managerVariable, tokenTypeVariable],
                        metadataBlockExpressions));

                structuralTypeShaperMaterializer = new ValueBufferTryReadValueRewriter(propertiesToRead).Rewrite(structuralTypeShaperMaterializer);

                if (isDocumentRoot)
                {
                    // Add assignment for keyValuesParameter
                    var primaryKeyProperties = ((IEntityType)structuralType).FindPrimaryKey()!.Properties;
                    keyValues = (keyValues.Parameter, Snapshot.CreateSnapshotType(primaryKeyProperties.Select(x => x.ClrType).ToArray()));

                    // This way we don't break any other visitor, since we don't touch the materializer much, just appending.
                    var newExpressions = structuralTypeShaperMaterializer.Expressions.ToList();
                    var entryIndex = newExpressions.FindIndex(ex => ex is BinaryExpression { NodeType: ExpressionType.Assign, Right: MethodCallExpression { Method.Name: nameof(QueryContext.TryGetEntry) } });

                    Check.DebugAssert(entryIndex != -1, "a TryGetEntry call should be in the materializer");

                    var entryExpression = (BinaryExpression)newExpressions[entryIndex];
                    var hasNullKey = ((MethodCallExpression)entryExpression.Right).Arguments.Last();
                    newExpressions.Insert(
                        entryIndex + 1,
                        IfThen(Not(hasNullKey),
                            Assign(keyValues.Parameter,
                                New(keyValues.Type.GetConstructors().Single(),
                                    primaryKeyProperties.Select(x => Convert(propertiesToRead[x], x.ClrType))))));
                    structuralTypeShaperMaterializer = structuralTypeShaperMaterializer.Update(structuralTypeShaperMaterializer.Variables, newExpressions);
                }
            }

            structuralTypeShaperMaterializer = new JsonEntityMaterializerKeyValuesSnapshotRewriter(keyValues.Parameter).Rewrite(structuralTypeShaperMaterializer);

            var innerShapersMap = new Dictionary<IPropertyBase, Expression>();
            var innerFixupMap = new Dictionary<IPropertyBase, LambdaExpression>();
            var trackingInnerFixupMap = new Dictionary<IPropertyBase, LambdaExpression>();

            // Go over all structural properties (complex properties and navigations - if we're an (owned) entity), which represent JSON
            // nested types; generate shapers and fixup to wire the materialized related instance into the parent's property.
            // Note that we need to build entity shapers and fixup separately; we don't know the order in which data comes, so
            // we need to read through everything before we can do fixup safely
            IEnumerable<IPropertyBase> nestedStructuralProperties = ((ITypeBase)structuralType.GetRootType()).GetDerivedTypesInclusive().SelectMany(x => x.GetDeclaredComplexProperties());

            if (structuralType is IEntityType entityType)
            {
                nestedStructuralProperties = nestedStructuralProperties.Concat(
                    entityType.GetRootType().GetDerivedTypesInclusive().SelectMany(x => x.GetDeclaredNavigations()
                        .Where(n => n.ForeignKey.IsOwnership
                            && n == n.ForeignKey.PrincipalToDependent)));
            }

            foreach (var nestedStructuralProperty in nestedStructuralProperties)
            {
                Check.DebugAssert(
                    nestedStructuralProperty is not INavigation ownedNavigation || !ownedNavigation.IsOnDependent,
                    "JSON navigations should always be from principal do dependent");

                var (relatedStructuralType, inverseNavigation, isStructuralPropertyNullable) = nestedStructuralProperty switch
                {
                    INavigation n => ((ITypeBase)n.TargetEntityType, n.Inverse, !n.ForeignKey.IsRequiredDependent),
                    IComplexProperty cp => (cp.ComplexType, null, cp.IsNullable),

                    _ => throw new UnreachableException()
                };

                var indexBasedSnapshotType = Snapshot.CreateSnapshotType([.. keyValues.Type.GenericTypeArguments, typeof(int)]);

                var innerShaper = CreateJsonShapers(
                    relatedStructuralType,
                    nestedStructuralProperty.ClrType,
                    nullable || isStructuralPropertyNullable,
                    valueBufferExpression,
                    nestedStructuralProperty.IsCollection ? (keyValues.Parameter, indexBasedSnapshotType) : keyValues);

                if (nestedStructuralProperty.IsCollection)
                {
                    // Build a snapshot factory that uses the collection index as last argument.
                    var indexParameter = Parameter(typeof(int), "index");
                    var indexBasedSnapshotFactory = Lambda(
                        New(
                            indexBasedSnapshotType.GetConstructors().Single(),
                            [.. keyValues.Type.GenericTypeArguments.Select((type, i) => Call(keyValues.Parameter, Snapshot.GetValueMethod.MakeGenericMethod(type), Constant(i))), indexParameter]),
                        keyValues.Parameter,
                        indexParameter);

                    var collectionClrType = nestedStructuralProperty.GetMemberInfo(forMaterialization: true, forSet: true).GetMemberType();
                    innerShaper = Call(
                        MaterializeJsonEntityCollectionMethodInfo.MakeGenericMethod(
                            nestedStructuralProperty switch
                            {
                                INavigation n => n.TargetEntityType.ClrType,
                                IComplexProperty cp => cp.ComplexType.ClrType,
                                _ => throw new UnreachableException()
                            },
                            collectionClrType),
                        QueryCompilationContext.QueryContextParameter,
                        keyValues.Parameter,
                        jsonReaderDataShaperLambdaParameter,
                        Constant(nestedStructuralProperty),
                        Lambda(
                            innerShaper,
                            QueryCompilationContext.QueryContextParameter,
                            keyValues.Parameter,
                            jsonReaderDataShaperLambdaParameter),
                        indexBasedSnapshotFactory);

                    var shaperEntityParameter = Parameter(nestedStructuralProperty.DeclaringType.ClrType);
                    var ownedNavigationType = nestedStructuralProperty.GetMemberInfo(forMaterialization: true, forSet: true).GetMemberType();
                    var shaperCollectionParameter = Parameter(ownedNavigationType);
                    var expressionsForFixup = new List<Expression>();
                    var expressionsForTracking = new List<Expression>();

                    if (!nestedStructuralProperty.IsShadowProperty())
                    {
                        expressionsForFixup.Add(
                            shaperEntityParameter.MakeMemberAccess(nestedStructuralProperty.GetMemberInfo(forMaterialization: true, forSet: true))
                                .Assign(shaperCollectionParameter));

                        expressionsForTracking.Add(
                            IfThen(
                                OrElse(
                                    ReferenceEqual(Constant(null), shaperCollectionParameter),
                                    IsFalse(
                                        Call(
                                            typeof(ShaperProcessingExpressionVisitor).GetMethod(nameof(Any))!,
                                            shaperCollectionParameter))),
                                shaperEntityParameter
                                    .MakeMemberAccess(nestedStructuralProperty.GetMemberInfo(forMaterialization: true, forSet: true))
                                    .Assign(shaperCollectionParameter)));
                    }

                    if (inverseNavigation is not null && !inverseNavigation.IsShadowProperty())
                    {
                        var innerFixupCollectionElementParameter = Parameter(inverseNavigation.DeclaringEntityType.ClrType);
                        var innerFixupParentParameter = Parameter(inverseNavigation.TargetEntityType.ClrType);

                        var elementFixup = Lambda(
                            Block(
                                typeof(void),
                                AssignStructuralProperty(
                                    innerFixupCollectionElementParameter,
                                    innerFixupParentParameter,
                                    inverseNavigation)),
                            innerFixupCollectionElementParameter,
                            innerFixupParentParameter);

                        expressionsForFixup.Add(
                            Call(
                                InverseCollectionFixupMethod.MakeGenericMethod(
                                    inverseNavigation.DeclaringEntityType.ClrType,
                                    inverseNavigation.TargetEntityType.ClrType),
                                shaperCollectionParameter,
                                shaperEntityParameter,
                                elementFixup));
                    }

                    var fixup = Lambda(
                        Block(typeof(void), expressionsForFixup),
                        shaperEntityParameter,
                        shaperCollectionParameter);

                    innerFixupMap[nestedStructuralProperty] = fixup;

                    var trackedFixup = Lambda(
                        Block(typeof(void), expressionsForTracking),
                        shaperEntityParameter,
                        shaperCollectionParameter);

                    // With tracking queries, the change tracker performs entity fixup, so we only need to handle fixup in the shaper for
                    // non-tracking queries; however, complex types always need to be fixed up in the shaper.
                    trackingInnerFixupMap[nestedStructuralProperty] = relatedStructuralType is IComplexType ? fixup : trackedFixup;
                }
                else
                {
                    var fixup = GenerateReferenceFixupForJson(
                        nestedStructuralProperty.DeclaringType.ClrType,
                        nestedStructuralProperty.ClrType,
                        nestedStructuralProperty,
                        inverseNavigation);

                    // With tracking queries, the change tracker performs entity fixup, so we only need to handle fixup in the shaper for
                    // non-tracking queries; however, complex types always need to be fixed up in the shaper.
                    innerFixupMap[nestedStructuralProperty] = fixup;

                    if (relatedStructuralType is IComplexType)
                    {
                        innerFixupMap[nestedStructuralProperty] = GenerateReferenceFixupForJson(
                            nestedStructuralProperty.DeclaringType.ClrType,
                            nestedStructuralProperty.ClrType,
                            nestedStructuralProperty,
                            inverseNavigation);
                    }
                }

                innerShapersMap[nestedStructuralProperty] = innerShaper;
            }

            var jsonMaterializerExpression = new JsonEntityMaterializerRewriter(
                    structuralType,
                    _queryStateManager,
                    jsonReaderDataShaperLambdaParameter,
                    innerShapersMap,
                    innerFixupMap,
                    trackingInnerFixupMap)
                .Rewrite(structuralTypeShaperMaterializer);

            shaperBlockVariables.AddRange(jsonMaterializerExpression.Variables);
            shaperBlockExpressions.AddRange(jsonMaterializerExpression.Expressions);

            jsonMaterializerExpression = Block(shaperBlockVariables, shaperBlockExpressions);

            var shaperLambda = Lambda(
                jsonMaterializerExpression,
                QueryCompilationContext.QueryContextParameter,
                keyValues.Parameter,
                jsonReaderDataShaperLambdaParameter);

            MethodInfo method;
            if (Nullable.GetUnderlyingType(clrType) is { } underlyingType)
            {
                // We need to project out a nullable value type. Note that the shaperLambda that we pass itself always returns a
                // non-nullable value (the null checks are outside of it.))
                Check.DebugAssert(nullable, "On non-nullable structural property but the property's ClrType is Nullable<T>");
                Check.DebugAssert(underlyingType == structuralType.ClrType);

                method = MaterializeJsonNullableValueStructuralTypeMethodInfo.MakeGenericMethod(structuralType.ClrType);
            }
            else
            {
                method = MaterializeJsonStructuralTypeMethodInfo.MakeGenericMethod(structuralType.ClrType);
            }

            return Call(
                method,
                QueryCompilationContext.QueryContextParameter,
                noKeyValues ? Constant(null, typeof(ISnapshot)) : keyValues.Parameter,
                jsonReaderDataShaperLambdaParameter,
                Constant(nullable),
                shaperLambda);
        }

        private Expression CheckGenerateExtractPath(Expression materializeExpression,
            Expression projectionExpressionObject,
            Expression data) // @TODO: Optimize signature?
        {
            var jsonPropertyPath = new List<string>();

            while (projectionExpressionObject is not ObjectReferenceExpression)
            {
                switch (projectionExpressionObject)
                {
                    case ObjectAccessExpression accessExpression:
                        jsonPropertyPath.Add(accessExpression.PropertyName);
                        projectionExpressionObject = accessExpression.Object;
                        break;
                    case ObjectArrayAccessExpression arrayAccessExpression:
                        jsonPropertyPath.Add(arrayAccessExpression.PropertyName);
                        projectionExpressionObject = arrayAccessExpression.Object;
                        break;
                    //case ObjectArrayIndexExpression objectArrayIndexExpression:
                    //    if (objectArrayIndexExpression.Index is ConstantExpression constantIndex)
                    //    {
                    //        path.Add(constantIndex.GetConstantValue<int>().ToString());
                    //        obj = objectArrayIndexExpression.Array;
                    //        break;
                    //    }
                    //    throw new InvalidOperationException(
                    //        CoreStrings.TranslationFailed(objectArrayIndexExpression.Index.Print()));
                    default:
                        throw new InvalidOperationException(
                            CoreStrings.TranslationFailed(projectionExpressionObject.Print()));
                }
            }

            // If propertyPath countis 0, which means the path being read is the query root, so we don't need to extract an embedded document path
            if (jsonPropertyPath.Count == 0)
            {
                return materializeExpression;
            }

            return GenerateExtractPath(materializeExpression, data, jsonPropertyPath);
        }

        private Expression GenerateExtractPath(Expression materializeExpression, Expression data, IEnumerable<string> jsonPropertyPath)
        {
            // var dataSubset = ReadPath(data, jsonPropertyPath);
            // materializeExpression(data -> dataSubset)

            var dataSubset = Parameter(data.Type, "dataSubset");
            var jsonPropertyPathBytes = new LinkedList<byte[]>(jsonPropertyPath.Select(Encoding.UTF8.GetBytes).Reverse());

            return Block(
                [dataSubset],
                Assign(dataSubset, Call(ReadPathMethod, data, Constant(jsonPropertyPathBytes))),
                ReplacingExpressionVisitor.Replace(data, dataSubset, materializeExpression));
        } // @TODO: We could do this in a switch read loop aswell..

        private Expression CreateReadJsonValueExpression(ParameterExpression jsonReaderManagerParameter, Type clrType, CoreTypeMapping typeMapping)
        {
            var jsonValueReaderWriter = typeMapping.JsonValueReaderWriter;
            Debug.Assert(jsonValueReaderWriter != null, "JsonValueReaderWriter should not be null since we are in Cosmos provider and all types should have JsonValueReaderWriter");
            var jsonValueReaderWriterConstant = Constant(jsonValueReaderWriter);

            var fromJsonMethod = jsonValueReaderWriterConstant.Type.GetMethod(
                nameof(JsonValueReaderWriter<>.FromJsonTyped),
                [typeof(Utf8JsonReaderManager).MakeByRefType(), typeof(object)])!;

            Expression resultExpression = Call(jsonValueReaderWriterConstant, fromJsonMethod, jsonReaderManagerParameter, Default(typeof(object)));

            if (resultExpression.Type != typeMapping.ClrType)
            {
                resultExpression = Convert(resultExpression, typeMapping.ClrType);
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

        // Almost 1-1 copy from relational, but no liftable constants..
        private Expression CreateReadJsonPropertyValueExpression(
            ParameterExpression jsonReaderManagerParameter,
            IProperty property)
        {
            // jsonReaderManager.MoveNext();
            // jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
            var jsonValueReaderWriter = property.GetJsonValueReaderWriter() ?? property.GetTypeMapping().JsonValueReaderWriter;
            Debug.Assert(jsonValueReaderWriter != null, "JsonValueReaderWriter should not be null since we are in Cosmos provider and all types should have JsonValueReaderWriter");
            var jsonValueReaderWriterConstant = Constant(jsonValueReaderWriter);

            var fromJsonMethod = jsonValueReaderWriterConstant.Type.GetMethod(
                nameof(JsonValueReaderWriter<>.FromJsonTyped),
                [typeof(Utf8JsonReaderManager).MakeByRefType(), typeof(object)])!;

            Expression resultExpression = Convert(
                Call(jsonValueReaderWriterConstant, fromJsonMethod, jsonReaderManagerParameter, Default(typeof(object))),
                property.GetTypeMapping().ClrType);

            if (property.IsNullable)
            {
                // in case of null value we can't just use the JsonReader method, but rather check the current token type
                // if it's JsonTokenType.Null means value is null, only if it's not we are safe to read the value
                if (resultExpression.Type != property.ClrType)
                {
                    resultExpression = Convert(resultExpression, property.ClrType);
                }

                resultExpression = Condition(
                    Equal(
                        Property(
                            Field(
                                jsonReaderManagerParameter,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderTokenTypeProperty),
                        Constant(JsonTokenType.Null)),
                    Default(property.ClrType),
                    resultExpression);
            }

            //if (_detailedErrorsEnabled)
            //{
            //    var exceptionParameter = Parameter(typeof(Exception), name: "e");
            //    var catchBlock = Catch(
            //        exceptionParameter,
            //        Call(
            //            ThrowExtractJsonPropertyExceptionMethod.MakeGenericMethod(resultExpression.Type),
            //            exceptionParameter,
            //            Constant(property, typeof(IProperty))));

            //    resultExpression = TryCatch(resultExpression, catchBlock);
            //}

            return resultExpression;
        }

        private ProjectionExpression GetProjection(ProjectionBindingExpression projectionBindingExpression)
            => ((SelectExpression)projectionBindingExpression.QueryExpression).Projection[GetProjectionIndex(projectionBindingExpression)];

        private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember != null
                ? ((SelectExpression)projectionBindingExpression.QueryExpression).GetMappedProjection(projectionBindingExpression.ProjectionMember).GetConstantValue<int>()
                : (projectionBindingExpression.Index
                    ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print())));

        private sealed class JsonEntityMaterializerKeyValuesSnapshotRewriter(ParameterExpression keyValuesParameter) : ExpressionVisitor
        {
            public BlockExpression Rewrite(BlockExpression jsonEntityShaperMaterializer)
                => (BlockExpression)VisitBlock(jsonEntityShaperMaterializer);

            protected override Expression VisitNew(NewExpression newExpression)
            {
                if (newExpression.Type.IsAssignableTo(typeof(ISnapshot)))
                {
                    var newArgs = new Expression[newExpression.Arguments.Count];
                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        newArgs[i] = TryRewriteValueBufferTryReadValue(newExpression.Arguments[i]);
                    }

                    return newExpression.Update(newArgs);
                }

                return base.VisitNew(newExpression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method == TryGetEntryMethod)
                {
                    var keyValuesArray = (NewArrayExpression)methodCallExpression.Arguments[1];
                    var newExpressions = new Expression[keyValuesArray.Expressions.Count];
                    for (var i = 0; i < keyValuesArray.Expressions.Count; i++)
                    {
                        newExpressions[i] = TryRewriteValueBufferTryReadValue(keyValuesArray.Expressions[i]);
                    }

                    return methodCallExpression.Update(
                        methodCallExpression.Object,
                        [methodCallExpression.Arguments[0], keyValuesArray.Update(newExpressions), ..methodCallExpression.Arguments.Skip(2)]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            private Expression TryRewriteValueBufferTryReadValue(Expression methodCallExpression)
                => methodCallExpression is MethodCallExpression methodCall
                && methodCall.Method.IsGenericMethod
                && methodCall.Method.GetGenericMethodDefinition() == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                && methodCall.Arguments[2].GetConstantValue<object>() is IProperty property
                && property.IsKey()
                    ? ConvertIfNotMatch(
                            Call(
                                keyValuesParameter,
                                Snapshot.GetValueMethod.MakeGenericMethod(property.ClrType),
                                Constant(property.GetIndex())),
                            methodCall.Type)
                    : methodCallExpression;
        }

        private sealed class ValueBufferTryReadValueRewriter(Dictionary<IProperty, ParameterExpression> mappedProperties) : ExpressionVisitor
        {
            public BlockExpression Rewrite(BlockExpression materializerExpression)
                => (BlockExpression)VisitBlock(materializerExpression);

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.IsGenericMethod
                        && methodCallExpression.Method.GetGenericMethodDefinition()
                        == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                        && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty property)
                {
                    if (mappedProperties.TryGetValue(property, out var parameter))
                    {
                        return methodCallExpression.Type != parameter.Type
                            ? Convert(parameter, methodCallExpression.Type)
                            : parameter;
                    }
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        // This is 1-1 copy from relational, except filtering out "" json properties, and using cosmos GetJsonPropertyName instead of relational.
        // And also: Allow inheritance @TODO: Improve


        private sealed class JsonEntityMaterializerRewriter(
            ITypeBase structuralBaseType,
            bool queryStateManager,
            ParameterExpression jsonReaderDataParameter,
            IDictionary<IPropertyBase, Expression> innerShapersMap,
            IDictionary<IPropertyBase, LambdaExpression> innerFixupMap,
            IDictionary<IPropertyBase, LambdaExpression> trackingInnerFixupMap)
            : ExpressionVisitor
        {
            private static readonly PropertyInfo JsonEncodedTextEncodedUtf8BytesProperty
                = typeof(JsonEncodedText).GetProperty(nameof(JsonEncodedText.EncodedUtf8Bytes))!;

            private static readonly MethodInfo JsonEncodedTextEncodeMethod
                = typeof(JsonEncodedText).GetMethod(nameof(JsonEncodedText.Encode), [typeof(string), typeof(JavaScriptEncoder)])!;

            public BlockExpression Rewrite(BlockExpression jsonEntityShaperMaterializer)
                => (BlockExpression)VisitBlock(jsonEntityShaperMaterializer);

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
                //sometimes we have shadow snapshot and sometimes not, but type initializer always comes last
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

                var managerVariable = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
                var tokenTypeVariable = Variable(typeof(JsonTokenType), "tokenType");
                var jsonStructuralTypeVariable = (ParameterExpression)jsonEntityTypeInitializerBlock.Expressions[^1];

                Debug.Assert(jsonStructuralTypeVariable.Type.IsAssignableFrom(structuralType.ClrType));

                var finalBlockVariables = new List<ParameterExpression>
                {
                    managerVariable, tokenTypeVariable,
                };

                finalBlockVariables.AddRange(jsonEntityTypeInitializerBlock.Variables);

                var finalBlockExpressions = new List<Expression>
                {
                    // jsonReaderManager = new Utf8JsonReaderManager(jsonReaderData))
                    Assign(
                        managerVariable,
                        New(
                            JsonReaderManagerConstructor,
                            jsonReaderDataParameter,
                            MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty))),
                    // tokenType = jsonReaderManager.CurrentReader.TokenType
                    Assign(
                        tokenTypeVariable,
                        Property(
                            Field(
                                managerVariable,
                                Utf8JsonReaderManagerCurrentReaderField),
                            Utf8JsonReaderTokenTypeProperty)),
                };

                var (loop, propertyAssignmentMap) = GenerateJsonPropertyReadLoop(
                    managerVariable,
                    tokenTypeVariable,
                    finalBlockVariables,
                    valueBufferTryReadValueMethodsToProcess);

                finalBlockExpressions.Add(loop);

                var finalCaptureState = Call(managerVariable, Utf8JsonReaderManagerCaptureStateMethod);
                finalBlockExpressions.Add(finalCaptureState);

                // we have the loop, now we can add code that generate the entity instance
                // will have to replace ValueBufferTryReadValue method calls with the parameters that store the value
                // we can't use simple ExpressionReplacingVisitor, because there could be multiple instances of MethodCallExpression for given property
                // using dedicated mini-visitor that looks for MCEs with a given shape and compare the IProperty inside
                // order is:
                // - shadow snapshot (if there was one)
                // - entity construction / property assignments
                // - navigation fixups
                // - entity instance variable that is returned as end result
                var propertyAssignmentReplacer = new ValueBufferTryReadValueMethodsReplacer(
                    jsonStructuralTypeVariable, propertyAssignmentMap);

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
                    finalBlockExpressions.Add(propertyAssignmentReplacer.Visit(shadowSnapshotAssignment));
                }

                foreach (var jsonEntityTypeInitializerBlockExpression in jsonEntityTypeInitializerBlock.Expressions.ToArray()[..^1])
                {
                    finalBlockExpressions.Add(propertyAssignmentReplacer.Visit(jsonEntityTypeInitializerBlockExpression));
                }

                // Fixup is only needed for non-tracking queries, in case of tracking (or NoTrackingWithIdentityResolution) - ChangeTracker does the job
                // or for empty/null collections of a tracking queries.
                ProcessFixup(queryStateManager ? trackingInnerFixupMap : innerFixupMap);

                finalBlockExpressions.Add(jsonStructuralTypeVariable);

                return Block(
                    finalBlockVariables,
                    finalBlockExpressions);

                void ProcessFixup(IDictionary<IPropertyBase, LambdaExpression> fixupMap)
                {
                    foreach (var fixup in fixupMap)
                    {
                        if (!navigationVariableMap.TryGetValue(fixup.Key, out var navigationEntityParameter))
                        {
                            // The navigation was not used in this materializer, it might be used by another type.
                            continue;
                        }

                        // Inject the fixup code for each property; we have this as a set of lambdas in the fixup map.
                        // In the normal case, simply Invoke the lambda, passing it the structural type to be fixed up as a parameter.
                        // This unfortunately doesn't work on value types (where a copy would be mutated), so for them,
                        // we unwrap the lambda and integrate its body directly.
                        // We should ideally do this for all cases (no need for the extra lambda Invoke), but there are some issues around us writing
                        // to readonly fields.
                        if (jsonStructuralTypeVariable.Type.IsValueType /*&& Nullable.GetUnderlyingType(fixup.Value.Parameters[1].Type) is null*/)
                        {
                            // No convert because it is a value type and inheritance is not supported for complex properties / value types.
                            var fixupBody = ReplacingExpressionVisitor.Replace(
                                originals: [fixup.Value.Parameters[0], fixup.Value.Parameters[1]],
                                replacements: [jsonStructuralTypeVariable, navigationEntityParameter],
                                fixup.Value.Body);

                            finalBlockExpressions.Add(fixupBody);
                        }
                        else
                        {
                            // Need to convert because fixup expects declaring type
                            var convertedJsonStructuralTypeVariable = Convert(jsonStructuralTypeVariable, fixup.Value.Parameters[0].Type);

                            // If the structural type being fixed up is nullable, then we need to add null checks before we run fixup logic.
                            // For regular entities, whose fixup is done as part of the "Materialize*" method, the checks are done there
                            // (the same will be done for the "optimized" scenario, where we populate properties directly rather than store in variables).
                            // But in this case fixups are standalone, so the null safety must be added here.
                            finalBlockExpressions.Add(
                                IfThen(
                                    NotEqual(convertedJsonStructuralTypeVariable, Constant(null, convertedJsonStructuralTypeVariable.Type)),
                                    Invoke(
                                        fixup.Value,
                                        convertedJsonStructuralTypeVariable,
                                        navigationEntityParameter)));
                        }
                    }
                }

                // builds a loop that extracts values of JSON properties and assigns them into variables
                // also injects entity shapers (generated earlier) for child navigations
                // returns the loop expression and mappings for properties (so we know which calls to replace with variables)
                (LoopExpression, Dictionary<IProperty, ParameterExpression>) GenerateJsonPropertyReadLoop(
                    ParameterExpression managerVariable,
                    ParameterExpression tokenTypeVariable,
                    List<ParameterExpression> finalBlockVariables,
                    List<MethodCallExpression> valueBufferTryReadValueMethodsToProcess)
                {
                    var breakLabel = Label("done");
                    var testExpressions = new List<Expression>();
                    var readExpressions = new List<Expression>();
                    var propertyAssignmentMap = new Dictionary<IProperty, ParameterExpression>();

                    foreach (var valueBufferTryReadValueMethodToProcess in valueBufferTryReadValueMethodsToProcess)
                    {
                        var property = valueBufferTryReadValueMethodToProcess.Arguments[2].GetConstantValue<IProperty>();
                        var jsonPropertyName = property.GetJsonPropertyName();
                        if (jsonPropertyName == string.Empty) // non persisted property
                        {
                            continue;
                        }
                        testExpressions.Add(
                            Call(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderValueTextEqualsMethod,
                                Convert(
                                    Call(
                                        ByteArrayAsSpanMethod,
                                        Constant(Encoding.UTF8.GetBytes(jsonPropertyName))),
                                    typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                        var propertyVariable = Variable(valueBufferTryReadValueMethodToProcess.Type);

                        finalBlockVariables.Add(propertyVariable);

                        var moveNext = Call(
                            managerVariable,
                            Utf8JsonReaderManagerMoveNextMethod);

                        var assignment = Assign(
                            propertyVariable,
                            valueBufferTryReadValueMethodToProcess);

                        readExpressions.Add(
                            Block(
                                moveNext,
                                assignment,
                                Empty()));

                        propertyAssignmentMap[property] = propertyVariable;
                    }

                    foreach (var innerShaperMapElement in innerShapersMap)
                    {
                        if (!innerShaperMapElement.Key.DeclaringType.IsAssignableFrom(structuralType))
                        {
                            continue;
                        }

                        var propertyName = innerShaperMapElement.Key switch
                        {
                            IComplexProperty complexProperty => complexProperty.GetJsonPropertyName(),
                            INavigation navigation => navigation.TargetEntityType.GetContainingPropertyName()!,
                            _ => throw new UnreachableException()
                        };
                        testExpressions.Add(
                            Call(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderValueTextEqualsMethod,
                                Convert(
                                    Call(
                                        ByteArrayAsSpanMethod,
                                        Constant(Encoding.UTF8.GetBytes(propertyName))),
                                    typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                        var propertyVariable = Variable(innerShaperMapElement.Value.Type);
                        finalBlockVariables.Add(propertyVariable);

                        navigationVariableMap[innerShaperMapElement.Key] = propertyVariable;

                        var moveNext = Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod);
                        var captureState = Call(managerVariable, Utf8JsonReaderManagerCaptureStateMethod);
                        var assignment = Assign(propertyVariable, innerShaperMapElement.Value);
                        var managerRecreation = Assign(
                            managerVariable,
                            New(
                                JsonReaderManagerConstructor,
                                jsonReaderDataParameter,
                                MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty)));

                        readExpressions.Add(
                            Block(
                                moveNext,
                                captureState,
                                assignment,
                                managerRecreation,
                                Empty()));
                    }

                    var switchCases = new List<SwitchCase>();
                    var testsCount = testExpressions.Count;

                    // generate PropertyName switch-case code
                    if (testsCount > 0)
                    {
                        var testExpression = IfThen(
                            testExpressions[testsCount - 1],
                            readExpressions[testsCount - 1]);

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

                    switchCases.Add(
                        SwitchCase(
                            Break(breakLabel),
                            Constant(JsonTokenType.EndObject)));

                    var loopBody = Block(
                        Assign(tokenTypeVariable, Call(managerVariable, Utf8JsonReaderManagerMoveNextMethod)),
                        Switch(
                            tokenTypeVariable,
                            Block(
                                Call(managerVariable, Utf8JsonReaderManagerSkipMethod),
                                Default(typeof(void))),
                            switchCases.ToArray()));

                    return (Loop(loopBody, breakLabel), propertyAssignmentMap);
                }
            }

            protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
            {
                var visited = base.VisitConditional(conditionalExpression);

                // this code compensates for differences between regular entities and JSON entities for tracking queries
                // for regular entities we preserve all the includes, so shaper for each entity is visited regardless
                // because of that, the original entity materializer code short-circuits if we find entity in change tracker
                //
                // for JSON entities that is incorrect, because all includes are part of the parent's shaper
                // so if we short circuit the parent, we never process the children
                // this is a problem when someone modifies child entity in the database directly - we would never pick up those changes
                // if we are tracking the parent
                // the code here re-arranges the existing materializer so that even if we find parent in the change tracker
                // we still process all the child navigations, it's just that we use the parent instance from change tracker, rather than create new one
#pragma warning disable EF1001 // Internal EF Core API usage.
                if (queryStateManager
                    && visited is ConditionalExpression
                    {
                        Test: BinaryExpression
                        {
                            NodeType: ExpressionType.NotEqual,
                            Left: ParameterExpression,
                            Right: DefaultExpression rightDefault
                        } testBinaryExpression,
                        IfTrue: BlockExpression ifTrueBlock,
                        IfFalse: BlockExpression ifFalseBlock
                    }
                    && rightDefault.Type == typeof(InternalEntityEntry))
                {
                    var entityAlreadyTrackedVariable = Variable(typeof(bool), "entityAlreadyTracked");

                    var resultBlockVariables = new List<ParameterExpression> { entityAlreadyTrackedVariable };
                    var resultBlockExpressions = new List<Expression>
                    {
                        Assign(entityAlreadyTrackedVariable, Constant(false)),

                        // shadowSnapshot = Snapshot.Empty;
                        ifFalseBlock.Expressions[0],

                        // entityType = EntityType;
                        ifFalseBlock.Expressions[1],
                        IfThen(
                            testBinaryExpression,
                            Block(
                                ifTrueBlock.Variables,
                                ifTrueBlock.Expressions.Concat(
                                    [Assign(entityAlreadyTrackedVariable, Constant(true)), Default(typeof(void))])))
                    };

                    resultBlockVariables.AddRange(ifFalseBlock.Variables.ToList());

                    var instanceAssignment = ifFalseBlock.Expressions.OfType<BinaryExpression>().Single(e
                        => e is { NodeType: ExpressionType.Assign, Left: ParameterExpression instance, Right: BlockExpression or SwitchExpression }
                        && structuralBaseType.ClrType.IsAssignableFrom(instance.Type));

                    var newInstanceAssignment = instanceAssignment.Right switch
                    {
                        BlockExpression block => RemapBody(block),
                        SwitchExpression switchExpr => switchExpr.Update(switchExpr.SwitchValue, switchExpr.Cases.Select(c => c.Update(c.TestValues, RemapBody((BlockExpression)c.Body))), switchExpr.DefaultBody),
                        _ => throw new UnreachableException()
                    };

                    resultBlockExpressions.Add(
                            Assign(instanceAssignment.Left, newInstanceAssignment));

                    var startTrackingAssignment = ifFalseBlock.Expressions
                        .OfType<BinaryExpression>()
                        .Single(e => e is
                        { NodeType: ExpressionType.Assign, Left: ParameterExpression instance, Right: ConditionalExpression }
                            && instance.Type == typeof(InternalEntityEntry));

                    var startTrackingExpression =
                        IfThen(
                            Not(
                                OrElse(
                                    entityAlreadyTrackedVariable,
                                    ((ConditionalExpression)startTrackingAssignment.Right).Test)),
                            Block(
                                ((ConditionalExpression)startTrackingAssignment.Right).IfFalse,
                                Default(typeof(void))));

                    resultBlockExpressions.Add(startTrackingExpression);
                    resultBlockExpressions.Add(Default(typeof(void)));
                    var resultBlock = Block(resultBlockVariables, resultBlockExpressions);
                    return resultBlock;

                    Expression RemapBody(BlockExpression instanceAssignmentBody)
                    {
                        var newInstanceAssignmentVariables = instanceAssignmentBody.Variables.ToList();
                        var newInstanceAssignmentExpressions = new List<Expression>();

                        // we only need to generate shadowSnapshot if the entity isn't already tracked
                        // shadow snapshot can be generated early in the block (default)
                        // or after we read all the values from JSON (case when the entity has some shadow properties)
                        // so we loop through the existing expressions and add the condition to snapshot assignment when we find it
                        // expressions processed here:
                        // shadowSnapshot = new Snapshot(...)
                        // jsonManagerPrm = new Utf8JsonReaderManager(jsonReaderDataPrm);
                        // tokenType = jsonManagerPrm.TokenType;
                        // property_reading_loop(...)
                        // jsonManagerPrm.CaptureState();
                        for (var i = 0; i < 5; i++)
                        {
                            newInstanceAssignmentExpressions.Add(
                                instanceAssignmentBody.Expressions[i].Type == typeof(ISnapshot)
                                    ? IfThen(
                                        Not(entityAlreadyTrackedVariable),
                                        instanceAssignmentBody.Expressions[i])
                                    : instanceAssignmentBody.Expressions[i]);
                        }

                        // from now on we have entity construction and property assignments
                        // then navigation fixup and then returning the final product
                        // entity construction could vary in length (e.g. when we have custom materializer)
                        // but we know how many navigation fixups there are and that instance is returned as last statement
                        var innerInstanceVariable = instanceAssignmentBody.Expressions[^1];

                        var createAndPopulateInstanceIfTrueBlock = Block(
                            Assign(innerInstanceVariable, instanceAssignment.Left),
                            Default(typeof(void)));

                        // all expressions except first 5 (that we already added)
                        // final variable being returned is also omitted but we generate Express.Default(typeof(void)) instead
                        var createAndPopulateInstanceIfFalseBlockExpressionsCount = instanceAssignmentBody.Expressions.Count - 5;
                        var createAndPopulateInstanceIfFalseBlockExpressions =
                            new Expression[createAndPopulateInstanceIfFalseBlockExpressionsCount];

                        Array.Copy(
                            instanceAssignmentBody.Expressions.ToArray()[5..^1],
                            createAndPopulateInstanceIfFalseBlockExpressions,
                            createAndPopulateInstanceIfFalseBlockExpressionsCount - 1);

                        createAndPopulateInstanceIfFalseBlockExpressions[^1] = Default(typeof(void));

                        var createAndPopulateInstanceExpression = IfThenElse(
                            entityAlreadyTrackedVariable,
                            createAndPopulateInstanceIfTrueBlock,
                            Block(createAndPopulateInstanceIfFalseBlockExpressions));

                        newInstanceAssignmentExpressions.Add(createAndPopulateInstanceExpression);
                        newInstanceAssignmentExpressions.Add(innerInstanceVariable);

                        return Block(newInstanceAssignmentVariables, newInstanceAssignmentExpressions);
                    }
                }
#pragma warning restore EF1001 // Internal EF Core API usage.

                return visited;
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
                    if (methodCallExpression.Method.IsGenericMethod
                        && methodCallExpression.Method.GetGenericMethodDefinition()
                        == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                        && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty property
                        && _properties.Contains(property))
                    {
                        _valueBufferTryReadValueMethods.Add(methodCallExpression);
                        _properties.Remove(property);

                        return methodCallExpression;
                    }

                    return base.VisitMethodCall(methodCallExpression);
                }
            }

            private sealed class ValueBufferTryReadValueMethodsReplacer(
                Expression instance,
                Dictionary<IProperty, ParameterExpression> propertyAssignmentMap)
                : ExpressionVisitor
            {
                protected override Expression VisitBinary(BinaryExpression node)
                {
                    if (node.Right is MethodCallExpression methodCallExpression
                        && IsPropertyAssignment(methodCallExpression, out var property, out var parameter))
                    {
                        if (parameter == null)
                        {
                            return Empty();
                        }

                        if (property!.IsPrimitiveCollection
                            && !property.ClrType.IsArray)
                        {
#pragma warning disable EF1001 // Internal EF Core API usage.
                            var genericMethod = StructuralTypeMaterializerSource.PopulateListMethod.MakeGenericMethod(
                                property.ClrType.TryGetElementType(typeof(IEnumerable<>))!);
#pragma warning restore EF1001 // Internal EF Core API usage.
                            var currentVariable = Variable(parameter.Type);
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
                            && visitedLeft is MemberExpression memberExpression
                                ? memberExpression.Assign(parameter!)
                                : MakeBinary(node.NodeType, visitedLeft, parameter!);
                    }

                    return base.VisitBinary(node);
                }

                protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
                    => IsPropertyAssignment(methodCallExpression, out _, out var parameter)
                        ? parameter ?? Default(methodCallExpression.Type)
                        : base.VisitMethodCall(methodCallExpression);

                private bool IsPropertyAssignment(
                    MethodCallExpression methodCallExpression,
                    [NotNullWhen(true)] out IProperty? property,
                    out Expression? parameter)
                {
                    if (methodCallExpression.Method.IsGenericMethod
                        && methodCallExpression.Method.GetGenericMethodDefinition()
                        == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                        && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty prop)
                    {
                        property = prop;
                        parameter = propertyAssignmentMap.TryGetValue(prop, out var param) ? param : null;
                        return true;
                    }

                    property = null;
                    parameter = null;
                    return false;
                }
            }
        }

        // Below is 1-1 (or very close) copy from relational
        private static LambdaExpression GenerateReferenceFixupForJson(
            Type clrType,
            Type relatedClrType,
            IPropertyBase structuralProperty,
            INavigationBase? inverseNavigation)
        {
            var entityParameter = Parameter(clrType);
            var relatedEntityParameter = Parameter(relatedClrType);
            var expressions = new List<Expression>();

            if (!structuralProperty.IsShadowProperty())
            {
                expressions.Add(
                    AssignStructuralProperty(
                        entityParameter,
                        relatedEntityParameter,
                        structuralProperty));
            }

            if (inverseNavigation != null
                && !inverseNavigation.IsShadowProperty())
            {
                expressions.Add(
                    AssignStructuralProperty(
                        relatedEntityParameter,
                        entityParameter,
                        inverseNavigation));
            }

            return Lambda(Block(typeof(void), expressions), entityParameter, relatedEntityParameter);
        }

        private static Expression AssignStructuralProperty(
            ParameterExpression structuralType,
            ParameterExpression relatedStructuralType,
            IPropertyBase structuralProperty)
        {
            var setter = structuralProperty.GetMemberInfo(forMaterialization: true, forSet: true);

            // If we're assigning a value complex type to a nullable complex property, add an upcast for typing
            var assignee = structuralProperty.ClrType.IsNullableValueType()
                && structuralProperty.ClrType.UnwrapNullableType() == relatedStructuralType.Type
                ? Convert(relatedStructuralType, structuralProperty.ClrType)
                : (Expression)relatedStructuralType;

            return structuralType.MakeMemberAccess(setter).Assign(assignee);
        }
    }
}
