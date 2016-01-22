// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class RequiresMaterializationExpressionVisitorFactory : IRequiresMaterializationExpressionVisitorFactory
    {
        private readonly IModel _model;

        public RequiresMaterializationExpressionVisitorFactory([NotNull] IModel model)
        {
            _model = model;
        }

        public virtual RequiresMaterializationExpressionVisitor Create(EntityQueryModelVisitor queryModelVisitor)
            => new RequiresMaterializationExpressionVisitor(_model, queryModelVisitor);
    }
}
