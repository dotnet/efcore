// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Utilities
{
    public class TypeNameHelperTest
    {
        [Fact]
        public void Can_pretty_print_CLR_full_name()
        {
            // Predefined Types
            Assert.Equal("int",
                typeof(int).DisplayName());
            Assert.Equal("System.Collections.Generic.List<int>",
                typeof(List<int>).DisplayName());
            Assert.Equal("System.Collections.Generic.Dictionary<int, string>",
                typeof(Dictionary<int, string>).DisplayName());
            Assert.Equal("System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>",
                typeof(Dictionary<int, List<string>>).DisplayName());
            Assert.Equal("System.Collections.Generic.List<System.Collections.Generic.List<string>>",
                typeof(List<List<string>>).DisplayName());

            // Classes inside NonGeneric class
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+A",
                typeof(A).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+B<int>",
                typeof(B<int>).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+C<int, string>",
                typeof(C<int, string>).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+C<int, Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+B<string>>",
                typeof(C<int, B<string>>).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+B<Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+B<string>>",
                typeof(B<B<string>>).DisplayName());

            // Classes inside Generic class
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+Outer<int>+D",
                typeof(Outer<int>.D).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<int>",
                typeof(Outer<int>.E<int>).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+Outer<int>+F<int, string>",
                typeof(Outer<int>.F<int, string>).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+Outer<int>+F<int, Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<string>>",
                typeof(Outer<int>.F<int, Outer<int>.E<string>>).DisplayName());
            Assert.Equal("Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<Microsoft.EntityFrameworkCore.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<string>>",
                typeof(Outer<int>.E<Outer<int>.E<string>>).DisplayName());
        }

        [Fact]
        public void Can_pretty_print_CLR_name()
        {
            // Predefined Types
            Assert.Equal("int",
                typeof(int).DisplayName(false));
            Assert.Equal("List<int>",
                typeof(List<int>).DisplayName(false));
            Assert.Equal("Dictionary<int, string>",
                typeof(Dictionary<int, string>).DisplayName(false));
            Assert.Equal("Dictionary<int, List<string>>",
                typeof(Dictionary<int, List<string>>).DisplayName(false));
            Assert.Equal("List<List<string>>",
                typeof(List<List<string>>).DisplayName(false));

            // Classes inside NonGeneric class
            Assert.Equal("A",
                typeof(A).DisplayName(false));
            Assert.Equal("B<int>",
                typeof(B<int>).DisplayName(false));
            Assert.Equal("C<int, string>",
                typeof(C<int, string>).DisplayName(false));
            Assert.Equal("C<int, B<string>>",
                typeof(C<int, B<string>>).DisplayName(false));
            Assert.Equal("B<B<string>>",
                typeof(B<B<string>>).DisplayName(false));

            // Classes inside Generic class
            Assert.Equal("D",
                typeof(Outer<int>.D).DisplayName(false));
            Assert.Equal("E<int>",
                typeof(Outer<int>.E<int>).DisplayName(false));
            Assert.Equal("F<int, string>",
                typeof(Outer<int>.F<int, string>).DisplayName(false));
            Assert.Equal("F<int, E<string>>",
                typeof(Outer<int>.F<int, Outer<int>.E<string>>).DisplayName(false));
            Assert.Equal("E<E<string>>",
                typeof(Outer<int>.E<Outer<int>.E<string>>).DisplayName(false));
        }

        [Fact]
        public void Returns_common_name_for_built_in_types()
        {
            Assert.Equal("bool", typeof(bool).DisplayName());
            Assert.Equal("byte", typeof(byte).DisplayName());
            Assert.Equal("char", typeof(char).DisplayName());
            Assert.Equal("decimal", typeof(decimal).DisplayName());
            Assert.Equal("double", typeof(double).DisplayName());
            Assert.Equal("float", typeof(float).DisplayName());
            Assert.Equal("int", typeof(int).DisplayName());
            Assert.Equal("long", typeof(long).DisplayName());
            Assert.Equal("object", typeof(object).DisplayName());
            Assert.Equal("sbyte", typeof(sbyte).DisplayName());
            Assert.Equal("short", typeof(short).DisplayName());
            Assert.Equal("string", typeof(string).DisplayName());
            Assert.Equal("uint", typeof(uint).DisplayName());
            Assert.Equal("ulong", typeof(ulong).DisplayName());
            Assert.Equal("ushort", typeof(ushort).DisplayName());
        }

        private class A
        {
        }

        private class B<T>
        {
        }

        private class C<T1, T2>
        {
        }

        private class Outer<T>
        {
            public class D
            {
            }

            public class E<T1>
            {
            }

            public class F<T1, T2>
            {
            }
        }
    }
}
