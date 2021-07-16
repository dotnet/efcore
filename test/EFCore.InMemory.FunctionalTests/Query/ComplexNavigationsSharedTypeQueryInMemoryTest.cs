// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsSharedTypeQueryInMemoryTest :
        ComplexNavigationsSharedTypeQueryTestBase<ComplexNavigationsSharedTypeQueryInMemoryFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ComplexNavigationsSharedTypeQueryInMemoryTest(
            ComplexNavigationsSharedTypeQueryInMemoryFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }
    }
}
