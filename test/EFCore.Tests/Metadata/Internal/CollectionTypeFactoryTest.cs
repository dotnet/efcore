// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class CollectionTypeFactoryTest
    {
        [ConditionalFact]
        public void Returns_given_type_if_public_parameterless_constructor_available()
        {
            var factory = new CollectionTypeFactory();

            Assert.Same(typeof(CustomHashSet), factory.TryFindTypeToInstantiate(typeof(object), typeof(CustomHashSet), false));
            Assert.Same(typeof(CustomList), factory.TryFindTypeToInstantiate(typeof(object), typeof(CustomList), false));
            Assert.Same(typeof(HashSet<Random>), factory.TryFindTypeToInstantiate(typeof(object), typeof(HashSet<Random>), false));
            Assert.Same(typeof(List<Random>), factory.TryFindTypeToInstantiate(typeof(object), typeof(List<Random>), false));
            Assert.Same(
                typeof(ObservableCollection<Random>),
                factory.TryFindTypeToInstantiate(typeof(object), typeof(ObservableCollection<Random>), false));
            Assert.Same(
                typeof(ObservableHashSet<Random>),
                factory.TryFindTypeToInstantiate(typeof(object), typeof(ObservableHashSet<Random>), false));
        }

        [ConditionalFact]
        public void Returns_ObservableHashSet_if_notifying_and_assignable()
        {
            Assert.Same(
                typeof(ObservableHashSet<Random>),
                new CollectionTypeFactory().TryFindTypeToInstantiate(typeof(DummyNotifying), typeof(ICollection<Random>), false));
        }

        [ConditionalFact]
        public void Returns_ObservableHashSet_if_full_notification_required()
        {
            Assert.Same(
                typeof(ObservableHashSet<Random>),
                new CollectionTypeFactory().TryFindTypeToInstantiate(typeof(object), typeof(ICollection<Random>), true));
        }

        [ConditionalFact]
        public void Returns_HashSet_if_assignable()
        {
            var factory = new CollectionTypeFactory();

            Assert.Same(typeof(HashSet<Random>), factory.TryFindTypeToInstantiate(typeof(object), typeof(ICollection<Random>), false));
            Assert.Same(typeof(HashSet<Random>), factory.TryFindTypeToInstantiate(typeof(object), typeof(ISet<Random>), false));
            Assert.Same(typeof(HashSet<Random>), factory.TryFindTypeToInstantiate(typeof(object), typeof(IEnumerable<Random>), false));
        }

        [ConditionalFact]
        public void Returns_List_if_assignable()
        {
            Assert.Same(
                typeof(List<Random>),
                new CollectionTypeFactory().TryFindTypeToInstantiate(typeof(object), typeof(IList<Random>), false));
        }

        [ConditionalFact]
        public void Returns_null_when_no_usable_concrete_type_found()
        {
            var factory = new CollectionTypeFactory();

            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object), typeof(PrivateConstructor), false));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object), typeof(InternalConstructor), false));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object), typeof(ProtectedConstructor), false));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object), typeof(NoParameterlessConstructor), false));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object), typeof(Abstract), false));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object), typeof(object), false));
            Assert.Null(factory.TryFindTypeToInstantiate(typeof(object), typeof(Random), false));
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

        private class DummyNotifying : INotifyPropertyChanged
        {
#pragma warning disable 67
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }
    }
}
