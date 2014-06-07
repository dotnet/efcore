// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class StateEntryNotifier
    {
        private readonly IEntityStateListener[] _entityStateListeners;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected StateEntryNotifier()
        {
        }

        public StateEntryNotifier([CanBeNull] IEnumerable<IEntityStateListener> entityStateListeners)
        {
            if (entityStateListeners != null)
            {
                var stateListeners = entityStateListeners.ToArray();
                _entityStateListeners = stateListeners.Length == 0 ? null : stateListeners;
            }
        }

        public virtual void StateChanging([NotNull] StateEntry entry, EntityState newState)
        {
            Check.NotNull(entry, "entry");
            Check.IsDefined(newState, "newState");

            Dispatch(l => l.StateChanging(entry, newState));
        }

        public virtual void StateChanged([NotNull] StateEntry entry, EntityState oldState)
        {
            Check.NotNull(entry, "entry");
            Check.IsDefined(oldState, "oldState");

            Dispatch(l => l.StateChanged(entry, oldState));
        }

        public virtual void ForeignKeyPropertyChanged(
            [NotNull] StateEntry entry, [NotNull] IProperty property, [CanBeNull] object oldValue, [CanBeNull] object newValue)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(property, "property");

            Dispatch(l => l.ForeignKeyPropertyChanged(entry, property, oldValue, newValue));
        }

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
    }
}
