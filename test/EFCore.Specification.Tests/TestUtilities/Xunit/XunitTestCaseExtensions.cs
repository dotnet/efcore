// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit
{
    public static class XunitTestCaseExtensions
    {
        private static readonly ConcurrentDictionary<string, List<IAttributeInfo>> _typeAttributes
            = new ConcurrentDictionary<string, List<IAttributeInfo>>();

        private static readonly ConcurrentDictionary<string, List<IAttributeInfo>> _assemblyAttributes
            = new ConcurrentDictionary<string, List<IAttributeInfo>>();

        public static async ValueTask<bool> TrySkipAsync(XunitTestCase testCase, IMessageBus messageBus)
        {
            var method = testCase.Method;
            var type = testCase.TestMethod.TestClass.Class;
            var assembly = type.Assembly;

            var skipReasons = new List<string>();
            var attributes =
                _assemblyAttributes.GetOrAdd(
                        assembly.Name,
                        a => assembly.GetCustomAttributes(typeof(ITestCondition)).ToList())
                    .Concat(
                        _typeAttributes.GetOrAdd(
                            type.Name,
                            t => type.GetCustomAttributes(typeof(ITestCondition)).ToList()))
                    .Concat(method.GetCustomAttributes(typeof(ITestCondition)))
                    .OfType<ReflectionAttributeInfo>()
                    .Select(attributeInfo => (ITestCondition)attributeInfo.Attribute);

            foreach (var attribute in attributes)
            {
                if (!await attribute.IsMetAsync())
                {
                    skipReasons.Add(attribute.SkipReason);
                }
            }

            if (skipReasons.Count > 0)
            {
                messageBus.QueueMessage(
                    new TestSkipped(new XunitTest(testCase, testCase.DisplayName), string.Join(Environment.NewLine, skipReasons)));
                return true;
            }

            return false;
        }
    }
}
