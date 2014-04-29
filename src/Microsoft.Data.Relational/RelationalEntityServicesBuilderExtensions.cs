// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Relational.Update;
using Microsoft.Data.Relational.Utilities;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.

namespace Microsoft.Data.Relational
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
                .AddSingleton<CommandBatchPreparer, CommandBatchPreparer>();

            return builder;
        }
    }
}
