// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

public static class XunitTestCaseExtensions
{
    private static readonly ConcurrentDictionary<string, List<IAttributeInfo>> _typeAttributes = new();
    private static readonly ConcurrentDictionary<string, List<IAttributeInfo>> _assemblyAttributes = new();

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

        if (!assembly.Name.StartsWith("Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests"))
        {
            skipReasons.Add("HACK: skip all non-sqlite tests");
        }

        foreach (var attribute in attributes)
        {
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                skipReasons.Add("HACK: skip all non-mac os tests");
            }

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
