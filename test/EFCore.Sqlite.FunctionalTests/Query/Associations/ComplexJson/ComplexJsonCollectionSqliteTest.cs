// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public class ComplexJsonCollectionSqliteTest(ComplexJsonSqliteFixture fixture, ITestOutputHelper testOutputHelper)
    : ComplexJsonCollectionRelationalTestBase<ComplexJsonSqliteFixture>(fixture, testOutputHelper);
