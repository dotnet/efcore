// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class InternalEntityEntryNotifier
    {
        private readonly IEntityStateListener[] _entityStateListeners;
        private readonly IPropertyListener[] _propertyListeners;
        private readonly IForeignKeyListener[] _fkListeners;
        private readonly INavigationListener[] _navigationListeners;
        private readonly IKeyListener[] _keyListeners;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected InternalEntityEntryNotifier()
        {
        }

        public InternalEntityEntryNotifier(
            [CanBeNull] IEnumerable<IEntityStateListener> entityStateListeners,
            [CanBeNull] IEnumerable<IPropertyListener> propertyListeners,
            [CanBeNull] IEnumerable<IForeignKeyListener> fkListeners,
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

            if (fkListeners != null)
            {
                var listeners = fkListeners.ToArray();
                _fkListeners = listeners.Length == 0 ? null : listeners;
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

        public virtual void StateChanging([NotNull] InternalEntityEntry entry, EntityState newState) 
            => Dispatch(l => l.StateChanging(entry, newState));

        public virtual void StateChanged([NotNull] InternalEntityEntry entry, EntityState oldState) 
            => Dispatch(l => l.StateChanged(entry, oldState));

        public virtual void ForeignKeyPropertyChanged(
            [NotNull] InternalEntityEntry entry, [NotNull] IProperty property, [CanBeNull] object oldValue, [CanBeNull] object newValue) 
            => Dispatch(l => l.ForeignKeyPropertyChanged(entry, property, oldValue, newValue));

        public virtual void NavigationReferenceChanged(
            [NotNull] InternalEntityEntry entry, [NotNull] INavigation navigation, [CanBeNull] object oldValue, [CanBeNull] object newValue) 
            => Dispatch(l => l.NavigationReferenceChanged(entry, navigation, oldValue, newValue));

        public virtual void NavigationCollectionChanged(
            [NotNull] InternalEntityEntry entry, [NotNull] INavigation navigation, [NotNull] ISet<object> added, [NotNull] ISet<object> removed) 
            => Dispatch(l => l.NavigationCollectionChanged(entry, navigation, added, removed));

        public virtual void PrincipalKeyPropertyChanged(
            [NotNull] InternalEntityEntry entry, [NotNull] IProperty property, [CanBeNull] object oldValue, [CanBeNull] object newValue)
            => Dispatch(l => l.KeyPropertyChanged(entry, property, oldValue, newValue));

        public virtual void PropertyChanged([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property)
            => Dispatch(l => l.PropertyChanged(entry, property));

        public virtual void PropertyChanging([NotNull] InternalEntityEntry entry, [NotNull] IPropertyBase property)
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

        private void Dispatch(Action<IForeignKeyListener> action)
        {
            if (_fkListeners == null)
            {
                return;
            }

            foreach (var listener in _fkListeners)
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
