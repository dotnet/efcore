// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryCompilationContext : QueryCompilationContext
    {
        public RelationalQueryCompilationContext([NotNull] IModel model)
            : base(Check.NotNull(model, "model"))
        {
        }

        public override EntityQueryModelVisitor CreateVisitor()
        {
            return new RelationalQueryModelVisitor(this, new EnumerableMethodProvider());
        }

        public override IResultOperatorHandler ResultOperatorHandler
        {
            get { return new RelationalResultOperatorHandler(base.ResultOperatorHandler); }
        }
    }
}
