// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Redis.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class BuiltInDataTypesTest : BuiltInDataTypesTestBase<RedisTestStore, BuiltInDataTypesFixture>
    {
        protected BuiltInDataTypesTest(BuiltInDataTypesFixture fixture) :base(fixture)
        {
        }
    }
}
