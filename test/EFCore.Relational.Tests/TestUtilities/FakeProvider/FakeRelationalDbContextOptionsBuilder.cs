// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeRelationalDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    : RelationalDbContextOptionsBuilder<FakeRelationalDbContextOptionsBuilder, FakeRelationalOptionsExtension>(optionsBuilder);
