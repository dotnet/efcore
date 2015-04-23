// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class TypeNameHelperTest
    {
        [Fact]
        public void Can_pretty_print_CLR_full_name()
        {
            // Predefined Types
            Assert.Equal("int",
                TypeNameHelper.GetTypeDisplayName(typeof(int)));
            Assert.Equal("System.Collections.Generic.List<int>",
                TypeNameHelper.GetTypeDisplayName(typeof(List<int>)));
            Assert.Equal("System.Collections.Generic.Dictionary<int, string>",
                TypeNameHelper.GetTypeDisplayName(typeof(Dictionary<int, string>)));
            Assert.Equal("System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(Dictionary<int, List<string>>)));
            Assert.Equal("System.Collections.Generic.List<System.Collections.Generic.List<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(List<List<string>>)));

            // Classes inside NonGeneric class
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+A",
                TypeNameHelper.GetTypeDisplayName(typeof(A)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+B<int>",
                TypeNameHelper.GetTypeDisplayName(typeof(B<int>)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+C<int, string>",
                TypeNameHelper.GetTypeDisplayName(typeof(C<int, string>)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+C<int, Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+B<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(C<int, B<string>>)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+B<Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+B<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(B<B<string>>)));

            // Classes inside Generic class
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+Outer<int>+D",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.D)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<int>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.E<int>)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+Outer<int>+F<int, string>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.F<int, string>)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+Outer<int>+F<int, Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.F<int, Outer<int>.E<string>>)));
            Assert.Equal("Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<Microsoft.Data.Entity.Tests.Utilities.TypeNameHelperTest+Outer<int>+E<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.E<Outer<int>.E<string>>)));
        }

        [Fact]
        public void Can_pretty_print_CLR_name()
        {
            // Predefined Types
            Assert.Equal("int",
                TypeNameHelper.GetTypeDisplayName(typeof(int), false));
            Assert.Equal("List<int>",
                TypeNameHelper.GetTypeDisplayName(typeof(List<int>), false));
            Assert.Equal("Dictionary<int, string>",
                TypeNameHelper.GetTypeDisplayName(typeof(Dictionary<int, string>), false));
            Assert.Equal("Dictionary<int, List<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(Dictionary<int, List<string>>), false));
            Assert.Equal("List<List<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(List<List<string>>), false));

            // Classes inside NonGeneric class
            Assert.Equal("A",
                TypeNameHelper.GetTypeDisplayName(typeof(A), false));
            Assert.Equal("B<int>",
                TypeNameHelper.GetTypeDisplayName(typeof(B<int>), false));
            Assert.Equal("C<int, string>",
                TypeNameHelper.GetTypeDisplayName(typeof(C<int, string>), false));
            Assert.Equal("C<int, B<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(C<int, B<string>>), false));
            Assert.Equal("B<B<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(B<B<string>>), false));

            // Classes inside Generic class
            Assert.Equal("D",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.D), false));
            Assert.Equal("E<int>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.E<int>), false));
            Assert.Equal("F<int, string>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.F<int, string>), false));
            Assert.Equal("F<int, E<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.F<int, Outer<int>.E<string>>), false));
            Assert.Equal("E<E<string>>",
                TypeNameHelper.GetTypeDisplayName(typeof(Outer<int>.E<Outer<int>.E<string>>), false));
        }

        [Fact]
        public void Returns_common_name_for_built_in_types()
        {
            Assert.Equal("bool", TypeNameHelper.GetTypeDisplayName(typeof(bool)));
            Assert.Equal("byte", TypeNameHelper.GetTypeDisplayName(typeof(byte)));
            Assert.Equal("char", TypeNameHelper.GetTypeDisplayName(typeof(char)));
            Assert.Equal("decimal", TypeNameHelper.GetTypeDisplayName(typeof(decimal)));
            Assert.Equal("double", TypeNameHelper.GetTypeDisplayName(typeof(double)));
            Assert.Equal("float", TypeNameHelper.GetTypeDisplayName(typeof(float)));
            Assert.Equal("int", TypeNameHelper.GetTypeDisplayName(typeof(int)));
            Assert.Equal("long", TypeNameHelper.GetTypeDisplayName(typeof(long)));
            Assert.Equal("object", TypeNameHelper.GetTypeDisplayName(typeof(object)));
            Assert.Equal("sbyte", TypeNameHelper.GetTypeDisplayName(typeof(sbyte)));
            Assert.Equal("short", TypeNameHelper.GetTypeDisplayName(typeof(short)));
            Assert.Equal("string", TypeNameHelper.GetTypeDisplayName(typeof(string)));
            Assert.Equal("uint", TypeNameHelper.GetTypeDisplayName(typeof(uint)));
            Assert.Equal("ulong", TypeNameHelper.GetTypeDisplayName(typeof(ulong)));
            Assert.Equal("ushort", TypeNameHelper.GetTypeDisplayName(typeof(ushort)));
        }
        private class A { }

        private class B<T> { }

        private class C<T1, T2> { }

        private class Outer<T>
        {
            public class D { }

            public class E<T1> { }

            public class F<T1, T2> { }
        }

    }
}
