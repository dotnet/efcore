// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestTypeMappingSource : TypeMappingSource
    {
        public TestTypeMappingSource(TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
            => null;
    }
}
