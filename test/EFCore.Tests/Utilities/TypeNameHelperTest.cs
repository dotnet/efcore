// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class TypeNameHelperTest
    {
        [ConditionalTheory]
        // Predefined Types
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(List<int>), "System.Collections.Generic.List<int>")]
        [InlineData(typeof(Dictionary<int, string>), "System.Collections.Generic.Dictionary<int, string>")]
        [InlineData(
            typeof(Dictionary<int, List<string>>), "System.Collections.Generic.Dictionary<int, System.Collections.Generic.List<string>>")]
        [InlineData(typeof(List<List<string>>), "System.Collections.Generic.List<System.Collections.Generic.List<string>>")]
        // Classes inside NonGeneric class
        [InlineData(
            typeof(A),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+A")]
        [InlineData(
            typeof(B<int>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+B<int>")]
        [InlineData(
            typeof(C<int, string>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+C<int, string>")]
        [InlineData(
            typeof(B<B<string>>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+B<Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+B<string>>")]
        [InlineData(
            typeof(C<int, B<string>>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+C<int, Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+B<string>>")]
        // Classes inside Generic class
        [InlineData(
            typeof(Outer<int>.D),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Outer<int>+D")]
        [InlineData(
            typeof(Outer<int>.E<int>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Outer<int>+E<int>")]
        [InlineData(
            typeof(Outer<int>.F<int, string>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Outer<int>+F<int, string>")]
        [InlineData(
            typeof(Level1<int>.Level2<bool>.Level3<int>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Level1<int>+Level2<bool>+Level3<int>")]
        [InlineData(
            typeof(Outer<int>.E<Outer<int>.E<string>>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Outer<int>+E<Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Outer<int>+E<string>>")]
        [InlineData(
            typeof(Outer<int>.F<int, Outer<int>.E<string>>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Outer<int>+F<int, Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Outer<int>+E<string>>")]
        [InlineData(
            typeof(OuterGeneric<int>.InnerNonGeneric.InnerGeneric<int, string>.InnerGenericLeafNode<bool>),
            "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+OuterGeneric<int>+InnerNonGeneric+InnerGeneric<int, string>+InnerGenericLeafNode<bool>")]
        public void Can_pretty_print_CLR_full_name(Type type, string expected)
        {
            Assert.Equal(expected, type.DisplayName());
        }

        [ConditionalTheory]
        // Predefined Types
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(List<int>), "List<int>")]
        [InlineData(typeof(Dictionary<int, string>), "Dictionary<int, string>")]
        [InlineData(typeof(Dictionary<int, List<string>>), "Dictionary<int, List<string>>")]
        [InlineData(typeof(List<List<string>>), "List<List<string>>")]
        // Classes inside NonGeneric class
        [InlineData(typeof(A), "A")]
        [InlineData(typeof(B<int>), "B<int>")]
        [InlineData(typeof(C<int, string>), "C<int, string>")]
        [InlineData(typeof(C<int, B<string>>), "C<int, B<string>>")]
        [InlineData(typeof(B<B<string>>), "B<B<string>>")]
        // Classes inside Generic class
        [InlineData(typeof(Outer<int>.D), "D")]
        [InlineData(typeof(Outer<int>.E<int>), "E<int>")]
        [InlineData(typeof(Outer<int>.F<int, string>), "F<int, string>")]
        [InlineData(typeof(Outer<int>.F<int, Outer<int>.E<string>>), "F<int, E<string>>")]
        [InlineData(typeof(Outer<int>.E<Outer<int>.E<string>>), "E<E<string>>")]
        [InlineData(
            typeof(OuterGeneric<int>.InnerNonGeneric.InnerGeneric<int, string>.InnerGenericLeafNode<bool>), "InnerGenericLeafNode<bool>")]
        public void Can_pretty_print_CLR_name(Type type, string expected)
        {
            Assert.Equal(expected, type.ShortDisplayName());
        }

        [ConditionalTheory]
        [InlineData(typeof(bool), "bool")]
        [InlineData(typeof(byte), "byte")]
        [InlineData(typeof(char), "char")]
        [InlineData(typeof(decimal), "decimal")]
        [InlineData(typeof(double), "double")]
        [InlineData(typeof(float), "float")]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(long), "long")]
        [InlineData(typeof(object), "object")]
        [InlineData(typeof(sbyte), "sbyte")]
        [InlineData(typeof(short), "short")]
        [InlineData(typeof(string), "string")]
        [InlineData(typeof(uint), "uint")]
        [InlineData(typeof(ulong), "ulong")]
        [InlineData(typeof(ushort), "ushort")]
        [InlineData(typeof(void), "void")]
        public void Returns_common_name_for_built_in_types(Type type, string expected)
        {
            Assert.Equal(expected, type.DisplayName());
        }

        [ConditionalTheory]
        [InlineData(typeof(int[]), true, "int[]")]
        [InlineData(typeof(string[][]), true, "string[][]")]
        [InlineData(typeof(int[,]), true, "int[,]")]
        [InlineData(typeof(bool[,,,]), true, "bool[,,,]")]
        [InlineData(typeof(A[,][,,]), true, "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+A[,][,,]")]
        [InlineData(typeof(List<int[,][,,]>), true, "System.Collections.Generic.List<int[,][,,]>")]
        [InlineData(typeof(List<int[,,][,]>[,][,,]), false, "List<int[,,][,]>[,][,,]")]
        public void Can_pretty_print_array_name(Type type, bool fullName, string expected)
        {
            Assert.Equal(expected, type.DisplayName(fullName));
        }

        public static TheoryData OpenGenericsTestData { get; } = CreateOpenGenericsTestData();

        public static TheoryData CreateOpenGenericsTestData()
        {
            var openDictionaryType = typeof(Dictionary<,>);
            var genArgsDictionary = openDictionaryType.GetGenericArguments();
            genArgsDictionary[0] = typeof(B<>);
            var closedDictionaryType = openDictionaryType.MakeGenericType(genArgsDictionary);
            var openLevelType = typeof(Level1<>.Level2<>.Level3<>);
            var genArgsLevel = openLevelType.GetGenericArguments();
            genArgsLevel[1] = typeof(string);
            var closedLevelType = openLevelType.MakeGenericType(genArgsLevel);
            var openInnerType = typeof(OuterGeneric<>.InnerNonGeneric.InnerGeneric<,>.InnerGenericLeafNode<>);
            var genArgsInnerType = openInnerType.GetGenericArguments();
            genArgsInnerType[3] = typeof(bool);
            var closedInnerType = openInnerType.MakeGenericType(genArgsInnerType);
            return new TheoryData<Type, bool, string>
            {
                { typeof(List<>), false, "List<>" },
                { typeof(Dictionary<,>), false, "Dictionary<,>" },
                { typeof(List<>), true, "System.Collections.Generic.List<>" },
                { typeof(Dictionary<,>), true, "System.Collections.Generic.Dictionary<,>" },
                {
                    typeof(Level1<>.Level2<>.Level3<>), true,
                    "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Level1<>+Level2<>+Level3<>"
                },
                {
                    typeof(PartiallyClosedGeneric<>).BaseType, true,
                    "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+C<, int>"
                },
                {
                    typeof(OuterGeneric<>.InnerNonGeneric.InnerGeneric<,>.InnerGenericLeafNode<>), true,
                    "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+OuterGeneric<>+InnerNonGeneric+InnerGeneric<,>+InnerGenericLeafNode<>"
                },
                {
                    closedDictionaryType, true,
                    "System.Collections.Generic.Dictionary<Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+B<>,>"
                },
                {
                    closedLevelType, true, "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+Level1<>+Level2<string>+Level3<>"
                },
                {
                    closedInnerType, true,
                    "Microsoft.EntityFrameworkCore.Utilities.TypeNameHelperTest+OuterGeneric<>+InnerNonGeneric+InnerGeneric<,>+InnerGenericLeafNode<bool>"
                }
            };
        }

        [ConditionalFact]
        public void Can_pretty_print_open_generics()
        {
            foreach (var testData in OpenGenericsTestData)
            {
                var type = (Type)testData[0];
                var fullName = (bool)testData[1];
                var expected = (string)testData[2];

                Assert.Equal(expected, type.DisplayName(fullName));
            }
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

        private class PartiallyClosedGeneric<T> : C<T, int>
        {
        }

        private static class Outer<T>
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

        private static class OuterGeneric<T1>
        {
            public static class InnerNonGeneric
            {
                public static class InnerGeneric<T2, T3>
                {
                    public class InnerGenericLeafNode<T4>
                    {
                    }

                    public class InnerLeafNode
                    {
                    }
                }
            }
        }

        private static class Level1<T1>
        {
            public static class Level2<T2>
            {
                public class Level3<T3>
                {
                }
            }
        }
    }
}
