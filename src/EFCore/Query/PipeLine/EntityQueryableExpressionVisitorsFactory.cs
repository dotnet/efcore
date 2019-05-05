// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public abstract class EntityQueryableTranslatorFactory : IEntityQueryableTranslatorFactory
    {
        public abstract EntityQueryableTranslator Create(QueryCompilationContext2 queryCompilationContext);
    }
}
