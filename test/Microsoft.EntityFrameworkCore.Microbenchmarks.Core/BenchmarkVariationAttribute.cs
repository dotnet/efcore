// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Microbenchmarks.Core
{
    public class BenchmarkVariationAttribute : DataAttribute
    {
        public BenchmarkVariationAttribute(string variationName, params object[] data)
        {
            VariationName = variationName;
            Data = data;
        }

        public string VariationName { get; private set; }

        public object[] Data { get; }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod) => new[] { Data };
    }
}
