// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class PropertyInfoExtensionsTest
    {
        [Fact]
        public void IsStatic_identifies_static_properties()
        {
            Assert.True(typeof(KitKat).GetAnyProperty("Yummy").IsStatic());
            Assert.True(typeof(KitKat).GetAnyProperty("Wafers").IsStatic());
            Assert.True(typeof(KitKat).GetAnyProperty("And").IsStatic());
            Assert.True(typeof(KitKat).GetAnyProperty("Chocolate").IsStatic());
            Assert.True(typeof(KitKat).GetAnyProperty("With").IsStatic());
            Assert.True(typeof(KitKat).GetAnyProperty("No").IsStatic());
            Assert.False(typeof(KitKat).GetAnyProperty("Nuts").IsStatic());
            Assert.False(typeof(KitKat).GetAnyProperty("But").IsStatic());
            Assert.False(typeof(KitKat).GetAnyProperty("May").IsStatic());
            Assert.False(typeof(KitKat).GetAnyProperty("Contain").IsStatic());
            Assert.False(typeof(KitKat).GetAnyProperty("TreeNuts").IsStatic());
            Assert.False(typeof(KitKat).GetAnyProperty("Just").IsStatic());
            Assert.True(typeof(KitKat).GetAnyProperty("Like").IsStatic());
            Assert.False(typeof(KitKat).GetAnyProperty("A").IsStatic());
            Assert.True(typeof(KitKat).GetAnyProperty("Twix").IsStatic());
        }

        public class KitKat
        {
            public static int Yummy { get; set; }
            private static int Wafers { get; set; }
            internal static int And { private get; set; }
            internal static int Chocolate { get; private set; }

            protected internal static int With
            {
                get { return 0; }
            }

            public static int No
            {
                set { }
            }

            public int Nuts { get; set; }
            private int But { get; set; }
            internal int May { private get; set; }
            protected internal int Contain { get; private set; }

            public int TreeNuts
            {
                get { return 0; }
            }

            public int Just
            {
                set { }
            }

            public static int Like { private get; set; }
            public int A { get; private set; }
            public static int Twix { protected internal get; set; }
        }
    }
}
