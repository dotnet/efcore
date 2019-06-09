// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly ConcurrentDictionary<string, string> _resolvedConditions
            = new ConcurrentDictionary<string, string>();

        public static bool TrySkip(XunitTestCase testCase, IMessageBus messageBus)
        {
            var method = testCase.Method;
            var type = testCase.TestMethod.TestClass.Class;
            var assembly = type.Assembly;
            var key = $"{method.Name}<<{type.Name}<<{assembly.Name}";

            var skipReason = _resolvedConditions.GetOrAdd(
                key,
                k =>
                {
                    var skipReasons = method
                        .GetCustomAttributes(typeof(ITestCondition))
                        .Concat(
                            _typeAttributes.GetOrAdd(
                                type.Name,
                                t => type.GetCustomAttributes(typeof(ITestCondition)).ToList()))
                        .Concat(
                            _assemblyAttributes.GetOrAdd(
                                assembly.Name,
                                a => assembly.GetCustomAttributes(typeof(ITestCondition)).ToList()))
                        .OfType<ReflectionAttributeInfo>()
                        .Select(attributeInfo => (ITestCondition)attributeInfo.Attribute)
                        .Where(condition => !condition.IsMet)
                        .Select(condition => condition.SkipReason)
                        .ToList();

                    return skipReasons.Count > 0 ? string.Join(Environment.NewLine, skipReasons) : null;
                });

            if (skipReason != null)
            {
                messageBus.QueueMessage(new TestSkipped(new XunitTest(testCase, testCase.DisplayName), skipReason));
                return true;
            }

            return false;
        }
    }
}
