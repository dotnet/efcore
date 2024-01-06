// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindCompiledQueryInMemoryTest(NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture) : NorthwindCompiledQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>(fixture);
