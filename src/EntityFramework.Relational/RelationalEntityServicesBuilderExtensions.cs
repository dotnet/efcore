// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.

namespace Microsoft.Data.Entity.Relational
{
    public static class RelationalEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddRelational([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddSingleton<DatabaseBuilder>()
                .AddSingleton<RelationalObjectArrayValueReaderFactory>()
                .AddSingleton<RelationalTypedValueReaderFactory>()
                .AddSingleton<ParameterNameGeneratorFactory>()
                .AddSingleton<CommandBatchPreparer>()
                .AddSingleton<ModificationCommandComparer>()
                .AddSingleton<RelationalModelBuilderFactory>()
                .AddSingleton<GraphFactory, BidirectionalAdjacencyListGraphFactory>()
                .AddScoped<RelationalDatabase>()
                // TODO: Is singleton correct here? What is IConfiguration scoped as?
                .AddSingleton<ConnectionStringResolver>();

            return builder;
        }
    }
}
