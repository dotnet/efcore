// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.Utilities;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.

namespace Microsoft.Framework.DependencyInjection
{
    public static class RelationalEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddRelational([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddSingleton<DatabaseBuilder, DatabaseBuilder>()
                .AddSingleton<RelationalObjectArrayValueReaderFactory, RelationalObjectArrayValueReaderFactory>()
                .AddSingleton<RelationalTypedValueReaderFactory, RelationalTypedValueReaderFactory>()
                .AddSingleton<ParameterNameGeneratorFactory, ParameterNameGeneratorFactory>()
                .AddSingleton<CommandBatchPreparer, CommandBatchPreparer>();

            return builder;
        }
    }
}
