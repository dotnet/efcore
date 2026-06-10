// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

// ReSharper disable once CheckNamespace
namespace Xunit;

[AttributeUsage(AttributeTargets.Method)]
[XunitTestCaseDiscoverer(
    "Microsoft.EntityFrameworkCore.TestUtilities.Xunit.ConditionalTheoryDiscoverer",
    "Microsoft.EntityFrameworkCore.Specification.Tests")]
public sealed class ConditionalTheoryAttribute : TheoryAttribute;
