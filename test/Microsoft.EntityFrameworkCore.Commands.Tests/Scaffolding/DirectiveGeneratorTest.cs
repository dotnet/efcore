// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Xunit;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class DirectiveGeneratorTest
    {
        [Fact]
        public void GeneratesTypeInstantiation_closed_generic()
        {
            var generator = new DirectiveGenerator();
            var typeInfo = typeof(A.B<A.C>).GetTypeInfo();
            Assert.Contains("<TypeInstantiation Name=\"A.B\" Arguments=\"A.C\" Dynamic=\"Required All\" />", generator.GenerateXml(new[] { typeInfo }));
        }

        [Fact]
        public void GeneratesType()
        {
            var generator = new DirectiveGenerator();
            var typeInfo = typeof(A.C).GetTypeInfo();
            Assert.Contains("<Type Name=\"A.C\" Dynamic=\"Required All\" />", generator.GenerateXml(new[] { typeInfo }));
        }

        [Fact]
        public void GeneratesType_open_generic()
        {
            var generator = new DirectiveGenerator();
            var typeInfo = typeof(A.B<>).GetTypeInfo();
            Assert.Contains("<Type Name=\"A.B{TChild}\" Dynamic=\"Required All\" />", generator.GenerateXml(new[] { typeInfo }));
        }

        [Fact]
        public void GeneratesMethodInstantiation()
        {
            var generator = new DirectiveGenerator();
            var methodInfo = typeof(A.C).GetTypeInfo().GetDeclaredMethod(nameof(A.C.Func)).MakeGenericMethod(typeof(A.B<A.D>));

            var xml = generator.GenerateXml(new[] { methodInfo });

            Assert.Contains("<Type Name=\"A.C\" Dynamic=\"Required All\">", xml);
            Assert.Contains("<MethodInstantiation Name=\"Func\" Arguments=\"A.B{A.D}\" Dynamic=\"Required\" />", xml);
        }

        [Fact]
        public void DoesNotDuplicateMethodInstantiations()
        {
            var generator = new DirectiveGenerator();
            var methodInfo = typeof(A.C).GetTypeInfo().GetDeclaredMethod(nameof(A.C.Func)).MakeGenericMethod(typeof(A.D));

            var xml = generator.GenerateXml(new[] { methodInfo, methodInfo });

            var expectedDirective = "<MethodInstantiation Name=\"Func\" Arguments=\"A.D\" Dynamic=\"Required\" />";
            Assert.Equal(xml.LastIndexOf(expectedDirective), xml.IndexOf(expectedDirective));
        }
    }
}

namespace A
{
    public class B<TChild> { }
    public class C
    {
        public void Func<T>() { }
    }
    public class D { }
}
