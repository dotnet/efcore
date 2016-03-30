// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public class InternalEntityEntryNotifier : IInternalEntityEntryNotifier
    {
        private readonly IEntityStateListener[] _entityStateListeners;
        private readonly IPropertyListener[] _propertyListeners;
        private readonly INavigationListener[] _navigationListeners;
        private readonly IKeyListener[] _keyListeners;

        public InternalEntityEntryNotifier(
            [CanBeNull] IEnumerable<IEntityStateListener> entityStateListeners,
            [CanBeNull] IEnumerable<IPropertyListener> propertyListeners,
            [CanBeNull] IEnumerable<INavigationListener> navigationListeners,
            [CanBeNull] IEnumerable<IKeyListener> keyListeners)
        {
            if (entityStateListeners != null)
            {
                var listeners = entityStateListeners.ToArray();
                _entityStateListeners = listeners.Length == 0 ? null : listeners;
            }

            if (propertyListeners != null)
            {
                var listeners = propertyListeners.ToArray();
                _propertyListeners = listeners.Length == 0 ? null : listeners;
            }

            if (navigationListeners != null)
            {
                var listeners = navigationListeners.ToArray();
                _navigationListeners = listeners.Length == 0 ? null : listeners;
            }

            if (keyListeners != null)
            {
                var listeners = keyListeners.ToArray();
                _keyListeners = listeners.Length == 0 ? null : listeners;
            }
        }

        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
            => Dispatch(l => l.StateChanging(entry, newState));

        public virtual void StateChanged(InternalEntityEntry entry, EntityState oldState, bool skipInitialFixup, bool fromQuery)
            => Dispatch(l => l.StateChanged(entry, oldState, skipInitialFixup, fromQuery));

        public virtual void NavigationReferenceChanged(InternalEntityEntry entry, INavigation navigation, object oldValue, object newValue)
            => Dispatch(l => l.NavigationReferenceChanged(entry, navigation, oldValue, newValue));

        public virtual void NavigationCollectionChanged(InternalEntityEntry entry, INavigation navigation, ISet<object> added, ISet<object> removed)
            => Dispatch(l => l.NavigationCollectionChanged(entry, navigation, added, removed));

        public virtual void KeyPropertyChanged(
            InternalEntityEntry entry,
            IProperty property,
            IReadOnlyList<IKey> keys,
            IReadOnlyList<IForeignKey> foreignKeys,
            object oldValue,
            object newValue)
            => Dispatch(l => l.KeyPropertyChanged(entry, property, keys, foreignKeys, oldValue, newValue));

        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModified)
            => Dispatch(l => l.PropertyChanged(entry, property, setModified));

        public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
            => Dispatch(l => l.PropertyChanging(entry, property));

        private void Dispatch(Action<IEntityStateListener> action)
        {
            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _entityStateListeners)
            {
                action(listener);
            }
        }

        private void Dispatch(Action<IPropertyListener> action)
        {
            if (_propertyListeners == null)
            {
                return;
            }

            foreach (var listener in _propertyListeners)
            {
                action(listener);
            }
        }

        private void Dispatch(Action<INavigationListener> action)
        {
            if (_navigationListeners == null)
            {
                return;
            }

            foreach (var listener in _navigationListeners)
            {
                action(listener);
            }
        }

        private void Dispatch(Action<IKeyListener> action)
        {
            if (_keyListeners == null)
            {
                return;
            }

            foreach (var listener in _keyListeners)
            {
                action(listener);
            }
        }
    }
}
