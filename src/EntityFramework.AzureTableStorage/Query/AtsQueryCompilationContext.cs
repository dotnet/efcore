// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsQueryCompilationContext : QueryCompilationContext
    {
        public AtsQueryCompilationContext([NotNull] IModel model)
            : base(model, new LinqOperatorProvider(), new ResultOperatorHandler())
        {
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor()
        {
            return new AtsQueryModelVisitor(this);
        }
    }
}
