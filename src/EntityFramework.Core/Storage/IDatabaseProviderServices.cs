// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Storage
{
    public interface IDatabaseProviderServices
    {
        string InvariantName { get; }
        IDatabase Database { get; }
        IDatabaseCreator Creator { get; }
        IValueGeneratorSelector ValueGeneratorSelector { get; }
        IConventionSetBuilder ConventionSetBuilder { get; }
        IModelSource ModelSource { get; }
        IModelValidator ModelValidator { get; }
        IValueGeneratorCache ValueGeneratorCache { get; }
        IQueryContextFactory QueryContextFactory { get; }
        IQueryCompilationContextFactory QueryCompilationContextFactory { get; }
        ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator { get; }
    }
}
