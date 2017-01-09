// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities
{
    public class TestRelationalModelSource : RelationalModelSource
    {
        public TestRelationalModelSource(
            IDbSetFinder setFinder,
            ICoreConventionSetBuilder coreConventionSetBuilder,
            IModelCustomizer modelCustomizer,
            IModelCacheKeyFactory modelCacheKeyFactory,
            CoreModelValidator coreModelValidator)
            : base(setFinder, coreConventionSetBuilder, modelCustomizer, modelCacheKeyFactory, coreModelValidator)
        {
        }
    }
}
