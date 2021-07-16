// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsCollectionsSharedTypeQueryInMemoryTest :
        ComplexNavigationsCollectionsSharedTypeQueryTestBase<ComplexNavigationsSharedTypeQueryInMemoryFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ComplexNavigationsCollectionsSharedTypeQueryInMemoryTest(
            ComplexNavigationsSharedTypeQueryInMemoryFixture fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }
    }
}
