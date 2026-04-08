// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    /// <summary>
    /// Rewrites an expression that projects a structural type that is not the document root to parse the JSON document to extract the part of the document that corresponds to the structural type, and then apply the shaper on that extracted JSON fragment.
    /// #34067
    /// </summary>
    private sealed class CosmosEmbeddedDocumentExtractingExpressionVisitor(ParameterExpression readerData) : ExpressionVisitor  // @TODO: Improve?
    {
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

        private static readonly PropertyInfo QueryContextQueryLoggerProperty =
            typeof(QueryContext).GetProperty(nameof(QueryContext.QueryLogger))!;

        private static readonly PropertyInfo Utf8JsonReaderTokenTypeProperty
            = typeof(Utf8JsonReader).GetProperty(nameof(Utf8JsonReader.TokenType))!;

        private static readonly MethodInfo ReadPathMethod
            = typeof(CosmosEmbeddedDocumentExtractingExpressionVisitor).GetMethod(nameof(ReadPath))!;

        private static readonly MethodInfo FinishObjectMethod
            = typeof(CosmosEmbeddedDocumentExtractingExpressionVisitor).GetMethod(nameof(FinishObject))!;

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case StructuralTypeShaperExpression structuralTypeShaperExpression
                    when structuralTypeShaperExpression.ValueBufferExpression.UnwrapTypeConversion(out _) is StructuralTypeProjectionExpression structuralTypeProjection:

                    var jsonPropertyPath = new List<string>();

                    var obj = structuralTypeProjection.Object;
                    while (obj is not ObjectReferenceExpression)
                    {
                        switch (obj)
                        {
                            case ObjectAccessExpression accessExpression:
                                jsonPropertyPath.Add(accessExpression.PropertyName);
                                obj = accessExpression.Object;
                                break;
                            case ObjectArrayAccessExpression arrayAccessExpression:
                                jsonPropertyPath.Add(arrayAccessExpression.PropertyName);
                                obj = arrayAccessExpression.Object;
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
                                    CoreStrings.TranslationFailed(obj.Print()));
                        }
                    }

                    if (jsonPropertyPath.Count == 0)
                    {
                        // This means the structural type is the document root, so we don't need to extract an embedded document
                        break;
                    }

                    // Generate an expression that uses Utf8JsonReaderManager to parse the JSON document and extract the part of the document that corresponds to the structural type

                    //var jsonReaderManager = new Utf8JsonReaderManager(readerData, QueryCompilationContext.QueryContextParameter.QueryLogger)
                    //ReadPath(jsonReaderManager)
                    //var result = StructuralTypeShaperExpression
                    //jsonReaderManager = new Utf8JsonReaderManager(readerData, QueryCompilationContext.QueryContextParameter.QueryLogger)
                    //FinishObject(jsonReaderManager)
                    //return result

                    var jsonPropertyPathBytes = new LinkedList<byte[]>(jsonPropertyPath.Select(x => Encoding.UTF8.GetBytes(x)).Reverse());

                    var jsonReaderManager = Variable(typeof(Utf8JsonReaderManager), "jsonReaderManager");
                    var result = Variable(structuralTypeShaperExpression.Type, "result");
                    return Block(
                        [jsonReaderManager, result],
                        Assign(
                            jsonReaderManager,
                            New(JsonReaderManagerConstructor, readerData, MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty))),
                        Call(ReadPathMethod, jsonReaderManager, Constant(jsonPropertyPathBytes)),
                        Assign(result, structuralTypeShaperExpression),
                        Assign(
                            jsonReaderManager,
                            New(JsonReaderManagerConstructor, readerData, MakeMemberAccess(QueryCompilationContext.QueryContextParameter, QueryContextQueryLoggerProperty))),
                        Call(FinishObjectMethod, jsonReaderManager, Constant(jsonPropertyPathBytes)),
                        result);

            }

            return base.VisitExtension(node);
        }

        public static void ReadPath(Utf8JsonReaderManager jsonReaderManager, LinkedList<byte[]> jsonPropertyPath)
        {
            var prop = jsonPropertyPath.First;
            var tokenType = jsonReaderManager.MoveNext();
            if (tokenType != JsonTokenType.StartObject)
            {
                throw new InvalidOperationException(
                    CoreStrings.JsonReaderInvalidTokenType(tokenType));
            }

            while (prop != null)
            {
                tokenType = jsonReaderManager.MoveNext();
                switch (tokenType)
                {
                    case JsonTokenType.StartObject:
                        break;
                    case JsonTokenType.PropertyName:
                        if (jsonReaderManager.CurrentReader.ValueTextEquals(prop.Value))
                        {
                            prop = prop.Next;
                        }
                        else
                        {
                            jsonReaderManager.Skip();
                        }
                        break;
                    case JsonTokenType.EndObject:
                    case JsonTokenType.Null:
                        throw new NullReferenceException(); // This is what 10.0 threw
                    default:
                        throw new InvalidOperationException(CoreStrings.JsonReaderInvalidTokenType(tokenType));
                }
            }
            jsonReaderManager.CaptureState();
        }

        public static void FinishObject(Utf8JsonReaderManager jsonReaderManager, LinkedList<byte[]> jsonPropertyPath) // @TODO: Improve?
        {
            var count = jsonPropertyPath.Count;
            for (var i = 0; i < count; i++)
            {
                while (jsonReaderManager.MoveNext() is not JsonTokenType.EndObject)
                {
                    jsonReaderManager.Skip();
                }
            }
            jsonReaderManager.CaptureState();
        }
    }
}
