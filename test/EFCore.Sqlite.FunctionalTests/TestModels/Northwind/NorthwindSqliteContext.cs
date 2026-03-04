// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.Northwind;

#nullable disable

public class NorthwindSqliteContext(DbContextOptions options) : NorthwindRelationalContext(options);
