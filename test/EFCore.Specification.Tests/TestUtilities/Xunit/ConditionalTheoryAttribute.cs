// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit.Sdk;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    [AttributeUsage(AttributeTargets.Method)]
    [XunitTestCaseDiscoverer(
        "Microsoft.EntityFrameworkCore.TestUtilities.Xunit.ConditionalTheoryDiscoverer",
        "Microsoft.EntityFrameworkCore.Specification.Tests")]
    public sealed class ConditionalTheoryAttribute : TheoryAttribute
    {
    }
}
