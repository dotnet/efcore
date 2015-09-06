// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class SqlTranslatingExpressionVisitorFactory : ISqlTranslatingExpressionVisitorFactory
    {
        private readonly IRelationalMetadataExtensionProvider _relationalMetadataExtensionProvider;
        private readonly IExpressionFragmentTranslator _compositeExpressionFragmentTranslator;
        private readonly IMethodCallTranslator _methodCallTranslator;
        private readonly IMemberTranslator _memberTranslator;

        public SqlTranslatingExpressionVisitorFactory(
            [NotNull] IRelationalMetadataExtensionProvider relationalMetadataExtensionProvider,
            [NotNull] IExpressionFragmentTranslator compositeExpressionFragmentTranslator,
            [NotNull] IMethodCallTranslator methodCallTranslator,
            [NotNull] IMemberTranslator memberTranslator)
        {
            Check.NotNull(relationalMetadataExtensionProvider, nameof(relationalMetadataExtensionProvider));
            Check.NotNull(compositeExpressionFragmentTranslator, nameof(compositeExpressionFragmentTranslator));
            Check.NotNull(methodCallTranslator, nameof(methodCallTranslator));
            Check.NotNull(memberTranslator, nameof(memberTranslator));

            _relationalMetadataExtensionProvider = relationalMetadataExtensionProvider;
            _compositeExpressionFragmentTranslator = compositeExpressionFragmentTranslator;
            _methodCallTranslator = methodCallTranslator;
            _memberTranslator = memberTranslator;
        }

        public virtual SqlTranslatingExpressionVisitor Create(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] SelectExpression targetSelectExpression = null,
            [CanBeNull] Expression topLevelPredicate = null,
            bool bindParentQueries = false,
            bool inProjection = false)
            => new SqlTranslatingExpressionVisitor (
                _relationalMetadataExtensionProvider,
                _compositeExpressionFragmentTranslator,
                _methodCallTranslator,
                _memberTranslator,
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                targetSelectExpression,
                topLevelPredicate,
                bindParentQueries,
                inProjection);
    }
}
