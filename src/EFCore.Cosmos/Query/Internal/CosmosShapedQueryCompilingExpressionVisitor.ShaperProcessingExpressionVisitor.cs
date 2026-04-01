// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    private sealed partial class ShaperProcessingExpressionVisitor(CosmosShapedQueryCompilingExpressionVisitor parentVisitor,
            SelectExpression selectExpression,
            ParameterExpression readerData,
            bool trackQueryResults) : ExpressionVisitor
    {
        private static readonly MethodInfo QueryContextStartTrackingMethod =
            typeof(QueryContext).GetMethod(nameof(QueryContext.StartTracking))!;

        private static readonly MethodInfo CollectionAccessorGetOrCreateMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.GetOrCreate))!;

        private static readonly MethodInfo CollectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo().GetDeclaredMethod(nameof(IClrCollectionAccessor.Add))!;

        private static readonly PropertyInfo ObjectArrayIndexerPropertyInfo
            = typeof(object[]).GetProperty("Item")!;

        private static readonly ConstructorInfo JsonReaderDataConstructor
            = typeof(JsonReaderData).GetConstructor([typeof(Stream)])!;

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

        private static readonly PropertyInfo EncodingUtf8Property
            = typeof(Encoding).GetProperty(nameof(Encoding.UTF8))!;

        private static readonly MethodInfo Utf8GetBytesMethod
            = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), [typeof(string)])!;

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

        // ValueBuffer: JsonReaderManager
        private readonly Dictionary<Expression, ParameterExpression>
            _valueBufferToJsonReaderDataMapping = new(); // @TODO: Basically only ever 1 entry...? Or will we do includes separatly? Probably not? It's not done in relational

        // MaterializationContext: JsonReaderManager
        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _materializationContextToJsonReaderDataMapping = new(); // @TODO: Basically only ever 1 entry...? Or will we do includes separatly? Probably not? It's not done in relational

        // JsonReaderData: JsonReaderManager
        private readonly Dictionary<ParameterExpression, ParameterExpression>
            _jsonReaderDataToJsonReaderManagerParameterMapping = new();

        private readonly List<MethodCallExpression> _pendingStartTrackingCalls = new();

        public LambdaExpression ProcessShaper(
            Expression shaperExpression)
        {
            var lambda = Visit(shaperExpression);
            return (LambdaExpression)lambda;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case StructuralTypeShaperExpression shaperExpression:
                    _valueBufferToJsonReaderDataMapping[shaperExpression.ValueBufferExpression] = readerData;

                    var jsonMaterializerExpression = Visit(CreateJsonShapers(
                        shaperExpression.StructuralType,
                        shaperExpression.IsNullable,
                        null,
                        shaperExpression.ValueBufferExpression
                    ));

                    // Readd the start tracking calls
                    if (trackQueryResults && shaperExpression.StructuralType is IEntityType entityType2)
                    {
                        // key values are same from top down...

                        // var entity = materializer(...);
                        // var snapshot = new Snapshot(entity.id); // json document id.

                        // for each pending call...
                        // queryContext.StartTracking(entityType, entity.prop?, snapshot); // @TODO: What should entity be?

#pragma warning disable EF1001 // Internal EF Core API usage.
                        var entityVariable = Parameter(entityType2.ClrType, "entity");
                        var snapshotVariable = Parameter(typeof(ISnapshot), "snapshot");

                        var shadowSnapshotValue = Constant(Snapshot.Empty); // var snapshot = new Snapshot(entity.id);

                        // So yeah we need to loop over all navigations again here?

                        jsonMaterializerExpression = Block(
                            [entityVariable, snapshotVariable],
                            Assign(entityVariable, jsonMaterializerExpression),
                            Assign(snapshotVariable, shadowSnapshotValue),
                            Call(QueryCompilationContext.QueryContextParameter,
                                QueryContextStartTrackingMethod,
                                Constant(entityType2),
                                entityVariable,
                                snapshotVariable),
                            entityVariable);
#pragma warning restore EF1001 // Internal EF Core API usage.
                    }

                    var shaperLambda = Lambda(
                        jsonMaterializerExpression,
                        QueryCompilationContext.QueryContextParameter,
                        readerData);

                    return shaperLambda;
                case IncludeExpression includeExpression:
                    return Visit(includeExpression.EntityExpression);
            }

            return base.VisitExtension(node);
        }

        private Expression CreateJsonShapers(
            ITypeBase structuralType,
            bool nullable,
            IPropertyBase? structuralProperty,
            Expression valueBufferExpression)
        {
            var structuralTypeShaperExpression = new StructuralTypeShaperExpression(
                structuralType,
                valueBufferExpression,
                nullable);

            var structuralTypeShaperMaterializer =
                (BlockExpression)parentVisitor.InjectStructuralTypeMaterializers(structuralTypeShaperExpression);

            var innerShapersMap = new Dictionary<string, Expression>();
            var innerFixupMap = new Dictionary<string, LambdaExpression>();
            var trackingInnerFixupMap = new Dictionary<string, LambdaExpression>();

            // Go over all structural properties (complex properties and navigations - if we're an (owned) entity), which represent JSON
            // nested types; generate shapers and fixup to wire the materialized related instance into the parent's property.
            // Note that we need to build entity shapers and fixup separately; we don't know the order in which data comes, so
            // we need to read through everything before we can do fixup safely
            IEnumerable<IPropertyBase> nestedStructuralProperties = structuralType.GetComplexProperties();

            if (structuralType is IEntityType entityType)
            {
                nestedStructuralProperties = nestedStructuralProperties.Concat(
                    entityType.GetNavigations()
                        .Where(n => n.ForeignKey.IsOwnership
                            && n == n.ForeignKey.PrincipalToDependent));
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

                var innerShaper = CreateJsonShapers(
                    relatedStructuralType,
                    nullable || isStructuralPropertyNullable,
                    keyValuesParameter, // ?
                    nestedStructuralProperty,
                    valueBufferExpression);

                var navigationJsonPropertyName = nestedStructuralProperty switch
                {
                    INavigation n => n.TargetEntityType.GetContainingPropertyName()!,
                    IComplexProperty cp => cp.GetJsonPropertyName(),
                    _ => throw new UnreachableException()
                };

                innerShapersMap[navigationJsonPropertyName] = innerShaper;

                if (nestedStructuralProperty.IsCollection)
                {
                    var shaperEntityParameter = Parameter(structuralType.ClrType);
                    var ownedNavigationType = nestedStructuralProperty.GetMemberInfo(forMaterialization: true, forSet: true).GetMemberType();
                    var shaperCollectionParameter = Parameter(ownedNavigationType);
                    var expressions = new List<Expression>();
                    var expressionsForTracking = new List<Expression>();

                    if (!nestedStructuralProperty.IsShadowProperty())
                    {
                        expressions.Add(
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

                        expressions.Add(
                            Call(
                                InverseCollectionFixupMethod.MakeGenericMethod(
                                    inverseNavigation.DeclaringEntityType.ClrType,
                                    inverseNavigation.TargetEntityType.ClrType),
                                shaperCollectionParameter,
                                shaperEntityParameter,
                                elementFixup));
                    }

                    var fixup = Lambda(
                        Block(typeof(void), expressions),
                        shaperEntityParameter,
                        shaperCollectionParameter);

                    innerFixupMap[navigationJsonPropertyName] = fixup;

                    var trackedFixup = Lambda(
                        Block(typeof(void), expressionsForTracking),
                        shaperEntityParameter,
                        shaperCollectionParameter);

                    // With tracking queries, the change tracker performs entity fixup, so we only need to handle fixup in the shaper for
                    // non-tracking queries; however, complex types always need to be fixed up in the shaper.
                    trackingInnerFixupMap[navigationJsonPropertyName] = relatedStructuralType is IComplexType ? fixup : trackedFixup;
                }
                else
                {
                    var fixup = GenerateReferenceFixupForJson(
                        structuralType.ClrType,
                        nestedStructuralProperty.ClrType,
                        nestedStructuralProperty,
                        inverseNavigation);

                    // With tracking queries, the change tracker performs entity fixup, so we only need to handle fixup in the shaper for
                    // non-tracking queries; however, complex types always need to be fixed up in the shaper.
                    innerFixupMap[navigationJsonPropertyName] = fixup;

                    if (relatedStructuralType is IComplexType)
                    {
                        trackingInnerFixupMap[navigationJsonPropertyName] = fixup;
                    }
                }
            }

            var jsonMaterializerExpression = new JsonEntityMaterializerRewriter(structuralType, trackQueryResults, readerData, innerShapersMap, innerFixupMap, trackingInnerFixupMap)
                .Rewrite(structuralTypeShaperMaterializer);

            var shaperLambda = Lambda(
                    jsonMaterializerExpression,
                    QueryCompilationContext.QueryContextParameter,
                    readerData);

            if (structuralProperty is { IsCollection: true })
            {
                var collectionClrType = structuralProperty.GetMemberInfo(forMaterialization: true, forSet: true).GetMemberType();
                var materializeJsonEntityCollectionMethodCall =
                    Call(
                        MaterializeJsonEntityCollectionMethodInfo.MakeGenericMethod(
                            structuralProperty switch
                            {
                                INavigation n => n.TargetEntityType.ClrType,
                                IComplexProperty cp => cp.ComplexType.ClrType,
                                _ => throw new UnreachableException()
                            },
                            collectionClrType),
                        QueryCompilationContext.QueryContextParameter,
                        readerData,
                        Constant(structuralProperty),
                        shaperLambda);

                return materializeJsonEntityCollectionMethodCall;
            }

            MethodInfo method;

            if (Nullable.GetUnderlyingType(structuralType.ClrType) is { } underlyingType) // @TODO: clr type?
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

            var materializedRootJsonEntity = Call(
                method,
                QueryCompilationContext.QueryContextParameter,
                readerData,
                Constant(nullable),
                shaperLambda);

            return materializedRootJsonEntity;
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            // Remove id = null check, an document can not be null, and we can not read the id value before we read the rest of the document
            // @TODO: How will this work for nested stuff.
            //.If(.Call Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValue(
            //    .Call $materializationContext1.get_ValueBuffer(),
            //    0,
            //    .Constant<Microsoft.EntityFrameworkCore.Metadata.IPropertyBase>(Property: RootEntity.Id(int) Required PK AfterSave: Throw))
            //!= null) {

            var test = conditionalExpression.Test;
            while (test is BinaryExpression { NodeType: ExpressionType.AndAlso } binary)
            {
                test = binary.Left; // Composite keys have && conditions
            }

            if (test is BinaryExpression { NodeType: ExpressionType.NotEqual, Left: MethodCallExpression methodCall } && methodCall.Method.IsGenericMethod
             && methodCall.Method.GetGenericMethodDefinition() == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
             && methodCall.Arguments[2] is ConstantExpression { Value: IProperty property } && property.IsKey())
            {
                return Visit(conditionalExpression.IfTrue);
            }

            return base.VisitConditional(conditionalExpression);
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
                    _materializationContextToJsonReaderDataMapping[parameterExpression] = _valueBufferToJsonReaderDataMapping[newExpression.Arguments[0]];

                    var valueBuffer = Constant(ValueBuffer.Empty);
                    var updatedExpression = newExpression.Update(
                    [
                        valueBuffer,
                        newExpression.Arguments[1]
                    ]);


                    return Assign(binaryExpression.Left, updatedExpression);
                }

                // Rewrites entityEntry = queryContext.TryGetEntry(key, object[] keyValues, bool throwOnNullKey, out bool hasNullKey)
                // to entityEntry = default.
                // We can't try to track until we have deserialized the stream to get the key values.
                // We readd this along with the retreiving of the entry instance after we've read the values from the json into variables. @TODO
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
#pragma warning disable EF1001 // Internal EF Core API usage.
                    when parameterExpression.Type == typeof(InternalEntityEntry):
                {
                    var hasNullKeyParameter = (ParameterExpression)((MethodCallExpression)binaryExpression.Right).Arguments[3];
                    return Block([
                        Assign(binaryExpression.Left, Default(typeof(InternalEntityEntry))),
                        Assign(hasNullKeyParameter, Constant(false))
                    ]);
#pragma warning restore EF1001 // Internal EF Core API usage.
                }

                // For embedded documents, we want to remove the shadowSnapShot assignment, and the startTracking call,
                // and then move them to after we have deserialized the parent document fully, so we know the key values to be used in the snapshot.
                // This finds $shadowSnapshotN = new Snapshot(ValueBufferTryReadValue(materializationContext, property, int _));
                // and removes the assignment from the expression tree
                case { NodeType: ExpressionType.Assign, Left: ParameterExpression parameterExpression }
#pragma warning disable EF1001 // Internal EF Core API usage.
                    when parameterExpression.Type == typeof(ISnapshot)
                      && binaryExpression.Left.UnwrapTypeConversion(out _) is NewExpression newExpression:
                {
                    var arguments = newExpression.Arguments.Select(x => x.UnwrapTypeConversion(out _) as MethodCallExpression!).ToArray();

                    // Check if this is the top level document, where Key values are successfully translated by the JsonEntityMaterializerRewriter to $varN
                    if (newExpression.Arguments.Any(x => x == null))
                    {
                        break;
                    }

                    return Empty();
                }
#pragma warning restore EF1001 // Internal EF Core API usage.

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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            // Converts valueBuffer.TryReadValue to jsonValueReaderWriter.FromJsonTyped(jsonReaderManager, null)
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition()
                == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod)
            {
                var property = methodCallExpression.Arguments[2].GetConstantValue<IProperty>();
                var materializationContext = (ParameterExpression)((MethodCallExpression)methodCallExpression.Arguments[0]).Object!;

                var jsonReaderData = _materializationContextToJsonReaderDataMapping[materializationContext];
                var jsonReaderManager = _jsonReaderDataToJsonReaderManagerParameterMapping[jsonReaderData];

                var jsonReadPropertyValueExpression = CreateReadJsonPropertyValueExpression(jsonReaderManager, property);

                return methodCallExpression.Type != jsonReadPropertyValueExpression.Type
                    ? Convert(jsonReadPropertyValueExpression, methodCallExpression.Type)
                    : jsonReadPropertyValueExpression;
            }

            // For embedded documents, there is a possiblity we don't know the shadowSnapshot value yer
            // So we want to remove the shadowSnapShot assignment, and the StartTracking call,
            // and then move them to after we have deserialized the root document fully, so we know the key values to be used in the snapshot.

            // We can't start tracking embedded documents until we have read the key values for the root document from the JSON,
            // so we remove the call to StartTracking here and add it back in after we have materialized the whole entity
            if (methodCallExpression.Method == QueryContextStartTrackingMethod && !methodCallExpression.Arguments[0].GetConstantValue<IEntityType>().IsDocumentRoot())
            {
                _pendingStartTrackingCalls.Add(methodCallExpression);

                return Empty();
            }

            return base.VisitMethodCall(methodCallExpression);
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
            => selectExpression.Projection[GetProjectionIndex(projectionBindingExpression)];

        private int GetProjectionIndex(ProjectionBindingExpression projectionBindingExpression)
            => projectionBindingExpression.ProjectionMember != null
                ? selectExpression.GetMappedProjection(projectionBindingExpression.ProjectionMember).GetConstantValue<int>()
                : (projectionBindingExpression.Index
                    ?? throw new InvalidOperationException(CoreStrings.TranslationFailed(projectionBindingExpression.Print())));

        // This is 1-1 copy from relational, except filtering out "" json properties, and using cosmos GetJsonPropertyName instead of relational
        private sealed class JsonEntityMaterializerRewriter(
            ITypeBase structuralType,
            bool queryStateManager,
            ParameterExpression jsonReaderDataParameter,
            IDictionary<string, Expression> innerShapersMap,
            IDictionary<string, LambdaExpression> innerFixupMap,
            IDictionary<string, LambdaExpression> trackingInnerFixupMap)
            : ExpressionVisitor
        {
            private static readonly PropertyInfo JsonEncodedTextEncodedUtf8BytesProperty
                = typeof(JsonEncodedText).GetProperty(nameof(JsonEncodedText.EncodedUtf8Bytes))!;

            private static readonly MethodInfo JsonEncodedTextEncodeMethod
                = typeof(JsonEncodedText).GetMethod(nameof(JsonEncodedText.Encode), [typeof(string), typeof(JavaScriptEncoder)])!;

            // keep track which variable corresponds to which navigation - we need that info for fixup
            // which happens at the end (after we read everything to guarantee that we can instantiate the entity
            private readonly Dictionary<string, ParameterExpression> _navigationVariableMap = new();

            public BlockExpression Rewrite(BlockExpression jsonEntityShaperMaterializer)
                => (BlockExpression)VisitBlock(jsonEntityShaperMaterializer);

            protected override Expression VisitSwitch(SwitchExpression switchExpression)
            {
                if (switchExpression.SwitchValue.Type.IsAssignableTo(typeof(ITypeBase))
                    && switchExpression is
                    {
                        Cases:
                        [
                        {
                            Body: BlockExpression { Expressions.Count: > 0 } body,
                            TestValues: [{ } onlyValueExpression]
                        }
                        ]
                    }
                    && onlyValueExpression.GetConstantValue<object>() == structuralType)
                {
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

                    Debug.Assert(jsonStructuralTypeVariable.Type == structuralType.ClrType);

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

                    void ProcessFixup(IDictionary<string, LambdaExpression> fixupMap)
                    {
                        foreach (var fixup in fixupMap)
                        {
                            var navigationEntityParameter = _navigationVariableMap[fixup.Key];

                            // Inject the fixup code for each property; we have this as a set of lambdas in the fixup map.
                            // In the normal case, simply Invoke the lambda, passing it the structural type to be fixed up as a parameter.
                            // This unfortunately doesn't work on value types (where a copy would be mutated), so for them,
                            // we unwrap the lambda and integrate its body directly.
                            // We should ideally do this for all cases (no need for the extra lambda Invoke), but there are some issues around us writing
                            // to readonly fields.
                            if (jsonStructuralTypeVariable.Type.IsValueType /*&& Nullable.GetUnderlyingType(jsonStructuralTypeVariable.Type) is null*/)
                            {
                                var fixupBody = ReplacingExpressionVisitor.Replace(
                                    originals: [fixup.Value.Parameters[0], fixup.Value.Parameters[1]],
                                    replacements: [jsonStructuralTypeVariable, _navigationVariableMap[fixup.Key]],
                                    fixup.Value.Body);

                                finalBlockExpressions.Add(fixupBody);
                            }
                            else
                            {
                                // If the structural type being fixed up is nullable, then we need to add null checks before we run fixup logic.
                                // For regular entities, whose fixup is done as part of the "Materialize*" method, the checks are done there
                                // (the same will be done for the "optimized" scenario, where we populate properties directly rather than store in variables).
                                // But in this case fixups are standalone, so the null safety must be added here.
                                finalBlockExpressions.Add(
                                    IfThen(
                                        NotEqual(jsonStructuralTypeVariable, Constant(null, jsonStructuralTypeVariable.Type)),
                                        Invoke(
                                            fixup.Value,
                                            jsonStructuralTypeVariable,
                                            _navigationVariableMap[fixup.Key])));
                            }
                        }
                    }
                }

                return base.VisitSwitch(switchExpression);

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
                        testExpressions.Add(
                            Call(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderValueTextEqualsMethod,
                                Convert(
                                    Call(
                                        ByteArrayAsSpanMethod,
                                        Call(
                                            Property(null, EncodingUtf8Property),
                                            Utf8GetBytesMethod, // @TODO: Why not constant?
                                            Constant(jsonPropertyName))),
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
                        var innerShaperMapElementKey = innerShaperMapElement.Key;
                        testExpressions.Add(
                            Call(
                                Field(
                                    managerVariable,
                                    Utf8JsonReaderManagerCurrentReaderField),
                                Utf8JsonReaderValueTextEqualsMethod,
                                Convert(
                                    Call(
                                        ByteArrayAsSpanMethod,
                                        Call(
                                            Property(null, EncodingUtf8Property),
                                            Utf8GetBytesMethod,
                                            Constant(innerShaperMapElementKey))),
                                    typeof(ReadOnlySpan<>).MakeGenericType(typeof(byte)))));

                        var propertyVariable = Variable(innerShaperMapElement.Value.Type);
                        finalBlockVariables.Add(propertyVariable);

                        _navigationVariableMap[innerShaperMapElement.Key] = propertyVariable;

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
                        => e is { NodeType: ExpressionType.Assign, Left: ParameterExpression instance, Right: BlockExpression }
                        && instance.Type == structuralType.ClrType);
                    var instanceAssignmentBody = (BlockExpression)instanceAssignment.Right;

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

                    var newInstanceAssignmentBlock = Block(newInstanceAssignmentVariables, newInstanceAssignmentExpressions);

                    resultBlockExpressions.Add(
                        Assign(instanceAssignment.Left, newInstanceAssignmentBlock));

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
                }
#pragma warning restore EF1001 // Internal EF Core API usage.

                return visited;
            }

            private sealed class ValueBufferTryReadValueMethodsFinder : ExpressionVisitor
            {
                private readonly List<IProperty> _properties;
                private readonly List<MethodCallExpression> _valueBufferTryReadValueMethods = [];

                public ValueBufferTryReadValueMethodsFinder(ITypeBase structuralType)
                    => _properties = structuralType.GetProperties().Where(x => x.GetJsonPropertyName() != "").ToList(); // Only difference with relational?

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
                        if (property!.IsPrimitiveCollection
                            && !property.ClrType.IsArray)
                        {
#pragma warning disable EF1001 // Internal EF Core API usage.
                            var genericMethod = StructuralTypeMaterializerSource.PopulateListMethod.MakeGenericMethod(
                                property.ClrType.TryGetElementType(typeof(IEnumerable<>))!);
#pragma warning restore EF1001 // Internal EF Core API usage.
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
                            && visitedLeft is MemberExpression memberExpression
                                ? memberExpression.Assign(parameter!)
                                : MakeBinary(node.NodeType, visitedLeft, parameter!);
                    }

                    return base.VisitBinary(node);
                }

                protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
                    => IsPropertyAssignment(methodCallExpression, out _, out var parameter)
                        ? parameter!
                        : base.VisitMethodCall(methodCallExpression);

                private bool IsPropertyAssignment(
                    MethodCallExpression methodCallExpression,
                    out IProperty? property,
                    out Expression? parameter)
                {
                    if (methodCallExpression.Method.IsGenericMethod
                        && methodCallExpression.Method.GetGenericMethodDefinition()
                        == EntityFrameworkCore.Infrastructure.ExpressionExtensions.ValueBufferTryReadValueMethod
                        && methodCallExpression.Arguments[2].GetConstantValue<object>() is IProperty prop
                        && propertyAssignmentMap.TryGetValue(prop, out var param))
                    {
                        property = prop;
                        parameter = param;
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
