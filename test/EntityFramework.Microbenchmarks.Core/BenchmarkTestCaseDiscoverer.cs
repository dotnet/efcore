// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkTestCaseDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public BenchmarkTestCaseDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public virtual IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var variations = testMethod.Method
                .GetCustomAttributes(typeof(BenchmarkVariationAttribute))
                .ToDictionary(
                    a => a.GetNamedArgument<string>(nameof(BenchmarkVariationAttribute.VariationName)),
                    a => a.GetNamedArgument<object[]>(nameof(BenchmarkVariationAttribute.Data)));

            if (!variations.Any())
            {
                variations.Add("Default", new object[0]);
            }

            var tests = new List<IXunitTestCase>();
            foreach (var variation in variations)
            {
                if (BenchmarkConfig.Instance.RunIterations)
                {
                    tests.Add(new BenchmarkTestCase(
                        factAttribute.GetNamedArgument<int>(nameof(BenchmarkAttribute.Iterations)),
                        factAttribute.GetNamedArgument<int>(nameof(BenchmarkAttribute.WarmupIterations)),
                        variation.Key,
                        _diagnosticMessageSink,
                        testMethod,
                        variation.Value));
                }
                else
                {
                    // TODO running a single iteration is slow under DNX (see #2574)
                    //      disabling so that we don't add 10min to build.cmd
#if !DNX451 && !DNXCORE50
                    var args = new[] { new MetricCollector() }
                        .Concat(variation.Value)
                        .ToArray();

                    tests.Add(new XunitTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, args));
#endif
                }
            }

            return tests;
        }
    }
}
