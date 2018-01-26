// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentDbEntityQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;

        public DocumentDbEntityQueryModelVisitorFactory([NotNull] EntityQueryModelVisitorDependencies dependencies,
            ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory)
            : base(dependencies)
        {
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
        }

        public override EntityQueryModelVisitor Create(
            QueryCompilationContext queryCompilationContext, EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            return new DocumentDbQueryModelVisitor(
                Dependencies,
                queryCompilationContext,
                _sqlTranslatingExpressionVisitorFactory,
                (DocumentDbQueryModelVisitor)parentEntityQueryModelVisitor);
        }
    }
}
