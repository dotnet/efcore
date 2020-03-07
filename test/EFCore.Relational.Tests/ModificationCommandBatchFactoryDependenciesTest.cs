// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ModificationCommandBatchFactoryDependenciesTest
    {
        [ConditionalFact]
        public void Can_use_With_methods_to_clone_and_replace_service()
        {
            RelationalTestHelpers.Instance.TestDependenciesClone<ModificationCommandBatchFactoryDependencies>();
        }
    }
}
