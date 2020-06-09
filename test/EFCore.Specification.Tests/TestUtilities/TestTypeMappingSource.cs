// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestTypeMappingSource : TypeMappingSource
    {
        public TestTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
        {
            return null;
        }
    }
}
