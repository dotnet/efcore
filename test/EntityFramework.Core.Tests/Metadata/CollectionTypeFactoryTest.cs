// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class CollectionTypeFactoryTest
    {
        [Fact]
        public void Returns_given_type_if_public_parameterless_constructor_available()
        {
            var factory = new CollectionTypeFactory();

            Assert.Same(typeof(CustomHashSet), factory.TryFindTypeToInstantiate(typeof(CustomHashSet)));
            Assert.Same(typeof(CustomList), factory.TryFindTypeToInstantiate(typeof(CustomList)));
            Assert.Same(typeof(HashSet<Random>), factory.TryFindTypeToInstantiate(typeof(HashSet<Random>)));
            Assert.Same(typeof(List<Random>), factory.TryFindTypeToInstantiate(typeof(List<Random>)));
            Assert.Same(typeof(ObservableCollection<Random>), factory.TryFindTypeToInstantiate(typeof(ObservableCollection<Random>)));
        }

        [Fact]
        public void Returns_HashSet_if_assignable()
        {
            var factory = new CollectionTypeFactory();

            Assert.Same(typeof(HashSet<Random>), factory.TryFindTypeToInstantiate(typeof(ICollection<Random>)));

            Assert.Same(typeof(HashSet<Random>), factory.TryFindTypeToInstantiate(typeof(ISet<Random>)));
        }

        [Fact]
        public void Returns_List_if_assignable()
        {
            Assert.Same(typeof(List<Random>), new CollectionTypeFactory().TryFindTypeToInstantiate(typeof(IList<Random>)));
        }

        [Fact]
        public void Returns_null_when_no_usable_concrete_type_found()
        {
            var factory = new CollectionTypeFactory();

            Assert.Null(factory.TryFindTypeToInstantiate(typeof(PrivateConstructor)));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(InternalConstructor)));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(ProtectedConstructor)));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(NoParameterlessConstructor)));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(Abstract)));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object)));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(Random)));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(IEnumerable<Random>)));
        }

        private class CustomHashSet : HashSet<Random>
        {
        }

        private class CustomList : List<Random>
        {
        }

        private class PrivateConstructor : List<Random>
        {
            private PrivateConstructor()
            {
            }
        }

        private class InternalConstructor : List<Random>
        {
            // ReSharper disable once EmptyConstructor
            internal InternalConstructor()
            {
            }
        }

        private class ProtectedConstructor : List<Random>
        {
            protected ProtectedConstructor()
            {
            }
        }

        private class NoParameterlessConstructor : List<Random>
        {
            public NoParameterlessConstructor(bool _)
            {
            }
        }

        private abstract class Abstract : List<Random>
        {
        }
    }
}
