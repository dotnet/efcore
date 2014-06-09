// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Adapters;
using Microsoft.Data.Entity.AzureTableStorage.Interfaces;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class AtsQueryCompilationContext : QueryCompilationContext
    {
        public TableFilterFactory TableFilterFactory { get; private set; }

        public AtsQueryCompilationContext([NotNull] IModel model, [NotNull] TableFilterFactory tableFilterFactory)
            : base(model)
        {
            Check.NotNull(tableFilterFactory, "tableFilterFactory");
            TableFilterFactory = tableFilterFactory;
        }

        public override EntityQueryModelVisitor CreateVisitor()
        {
            return new AtsQueryModelVisitor(this);
        }
    }
}
