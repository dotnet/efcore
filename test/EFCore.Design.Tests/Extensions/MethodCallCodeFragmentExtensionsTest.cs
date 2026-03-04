// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Extensions.Namespace1;
using Microsoft.EntityFrameworkCore.Extensions.Namespace2;
using Microsoft.EntityFrameworkCore.Extensions.Namespace3;

namespace Microsoft.EntityFrameworkCore.Extensions
{
    public class MethodCallCodeFragmentExtensionsTest
    {
        [ConditionalFact]
        public void GetRequiredUsings_works()
        {
            var methodCall = new MethodCallCodeFragment(
                typeof(TestExtensions1)
                    .GetRuntimeMethod(
                        nameof(TestExtensions1.Extension1),
                        [typeof(MethodCallCodeFragmentExtensionsTest), typeof(Action<MethodCallCodeFragmentExtensionsTest>)]),
                new NestedClosureCodeFragment(
                    "x",
                    new MethodCallCodeFragment(
                        typeof(TestExtensions2)
                            .GetRuntimeMethod(
                                nameof(TestExtensions2.Extension2),
                                [typeof(MethodCallCodeFragmentExtensionsTest), typeof(TestArgument)]),
                        new TestArgument())));

            var usings = methodCall.GetRequiredUsings();

            Assert.Equal(
                new[]
                {
                    "Microsoft.EntityFrameworkCore.Extensions.Namespace1",
                    "Microsoft.EntityFrameworkCore.Extensions.Namespace2",
                    "Microsoft.EntityFrameworkCore.Extensions.Namespace3"
                },
                usings);
        }
    }

    namespace Namespace1
    {
        internal static class TestExtensions1
        {
            public static void Extension1(
                this MethodCallCodeFragmentExtensionsTest extendedObject,
                Action<MethodCallCodeFragmentExtensionsTest> closure)
                => throw new NotImplementedException();
        }
    }

    namespace Namespace2
    {
        internal static class TestExtensions2
        {
            public static void Extension2(
                this MethodCallCodeFragmentExtensionsTest extendedObject,
                TestArgument argument)
                => throw new NotImplementedException();
        }
    }

    namespace Namespace3
    {
        internal class TestArgument;
    }
}
