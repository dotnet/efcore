// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json.Linq;
using static System.Linq.Expressions.Expression;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private sealed class JObjectInjectingExpressionVisitor : ExpressionVisitor
    {
        private int _currentEntityIndex;

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case StructuralTypeShaperExpression shaperExpression:
                {
                    _currentEntityIndex++;

                    var valueBufferExpression = shaperExpression.ValueBufferExpression;

                    var jObjectVariable = Variable(
                        typeof(JObject),
                        "jObject" + _currentEntityIndex);
                    var variables = new List<ParameterExpression> { jObjectVariable };

                    var expressions = new List<Expression>
                    {
                        Assign(
                            jObjectVariable,
                            TypeAs(
                                valueBufferExpression,
                                typeof(JObject))),
                        Condition(
                            Equal(jObjectVariable, Constant(null, jObjectVariable.Type)),
                            Constant(null, shaperExpression.Type),
                            shaperExpression)
                    };

                    return Block(
                        shaperExpression.Type,
                        variables,
                        expressions);
                }

                case CollectionShaperExpression collectionShaperExpression:
                {
                    _currentEntityIndex++;

                    var jArrayVariable = Variable(
                        typeof(JArray),
                        "jArray" + _currentEntityIndex);
                    var variables = new List<ParameterExpression> { jArrayVariable };

                    var expressions = new List<Expression>
                    {
                        Assign(
                            jArrayVariable,
                            TypeAs(
                                collectionShaperExpression.Projection,
                                typeof(JArray))),
                        Condition(
                            Equal(jArrayVariable, Constant(null, jArrayVariable.Type)),
                            Constant(null, collectionShaperExpression.Type),
                            collectionShaperExpression)
                    };

                    return Block(
                        collectionShaperExpression.Type,
                        variables,
                        expressions);
                }
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
