// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class ExeTest
    {
        [Fact]
        public void ToArguments_works()
        {
            var result = ToArguments(
                new[]
                {
                    "Good",
                    "Good\\",
                    "Needs quotes",
                    "Needs escaping\\",
                    "Needs escaping\\\\",
                    "Needs \"escaping\"",
                    "Needs \\\"escaping\"",
                    "Needs escaping\\\\too"
                });

            Assert.Equal(
                "Good "
                + "Good\\ "
                + "\"Needs quotes\" "
                + "\"Needs escaping\\\\\" "
                + "\"Needs escaping\\\\\\\\\" "
                + "\"Needs \\\"escaping\\\"\" "
                + "\"Needs \\\\\\\"escaping\\\"\" "
                + "\"Needs escaping\\\\\\\\too\"",
                result);
        }

        private static string ToArguments(IReadOnlyList<string> args)
            => (string)typeof(Exe).GetTypeInfo().GetMethod("ToArguments", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { args });
    }
}
