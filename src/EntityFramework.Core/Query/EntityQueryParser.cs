// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ResultOperators;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryParser : IQueryParser
    {
        private static CompoundNodeTypeProvider CreateNodeTypeProvider()
        {
            var searchedTypes
                = typeof(MethodInfoBasedNodeTypeRegistry)
                    .GetTypeInfo()
                    .Assembly
                    .DefinedTypes
                    .Select(ti => ti.AsType())
                    .ToList();

            var methodInfoBasedNodeTypeRegistry
                = MethodInfoBasedNodeTypeRegistry.CreateFromTypes(searchedTypes);

            methodInfoBasedNodeTypeRegistry
                .Register(AsNoTrackingExpressionNode.SupportedMethods, typeof(AsNoTrackingExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(IncludeExpressionNode.SupportedMethods, typeof(IncludeExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(ThenIncludeExpressionNode.SupportedMethods, typeof(ThenIncludeExpressionNode));

            var innerProviders
                = new INodeTypeProvider[]
                    {
                        methodInfoBasedNodeTypeRegistry,
                        MethodNameBasedNodeTypeRegistry.CreateFromTypes(searchedTypes)
                    };

            return new CompoundNodeTypeProvider(innerProviders);
        }

        private readonly ThreadSafeDictionaryCache<Expression, QueryModel> _cache
            = new ThreadSafeDictionaryCache<Expression, QueryModel>(new ExpressionComparer());

        public virtual QueryModel GetParsedQuery([NotNull] Expression expressionTreeRoot)
        {
            Check.NotNull(expressionTreeRoot, "expressionTreeRoot");

            return _cache.GetOrAdd(expressionTreeRoot, e =>
                {
                    var queryParser
                        = new QueryParser(
                            new ExpressionTreeParser(
                                CreateNodeTypeProvider(),
                                ExpressionTreeParser.CreateDefaultProcessor(
                                    ExpressionTransformerRegistry.CreateDefault())));

                    return queryParser.GetParsedQuery(e);
                });
        }
    }
}
