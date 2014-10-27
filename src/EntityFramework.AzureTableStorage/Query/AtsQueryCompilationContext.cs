// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsQueryCompilationContext : QueryCompilationContext
    {
        public AtsQueryCompilationContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] EntityMaterializerSource entityMaterializerSource)
            : base(
                Check.NotNull(model, "model"),
                Check.NotNull(logger, "logger"),
                new LinqOperatorProvider(),
                new ResultOperatorHandler(),
                Check.NotNull(entityMaterializerSource, "entityMaterializerSource"))
        {
        }

        public override EntityQueryModelVisitor CreateQueryModelVisitor(
            EntityQueryModelVisitor parentEntityQueryModelVisitor)
        {
            return new AtsQueryModelVisitor(this);
        }
    }
}
