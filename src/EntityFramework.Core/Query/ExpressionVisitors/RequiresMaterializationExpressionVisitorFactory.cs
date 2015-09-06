// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class RequiresMaterializationExpressionVisitorFactory : IRequiresMaterializationExpressionVisitorFactory
    {
        private readonly IModel _model;

        public RequiresMaterializationExpressionVisitorFactory([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        public virtual RequiresMaterializationExpressionVisitor Create([NotNull] EntityQueryModelVisitor queryModelVisitor)
            => new RequiresMaterializationExpressionVisitor(
                _model,
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)));
    }
}
