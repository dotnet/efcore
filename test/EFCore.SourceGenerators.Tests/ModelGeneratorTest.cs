// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SourceGenerators.Tests
{
    public class ModelGeneratorTest
    {
        [ConditionalFact]
        public void Can_get_compiled_model()
        {
            var model = new MyRuntimeDbContext().GetCompiledModel();

            Assert.Equal(typeof(int), model.GetEntityTypes().Single().FindProperty("ShadowProp").ClrType);
        }
    }
}
