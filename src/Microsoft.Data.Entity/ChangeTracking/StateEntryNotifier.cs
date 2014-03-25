// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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

        public StateEntryNotifier([NotNull] IEnumerable<IEntityStateListener> entityStateListeners)
        {
            Check.NotNull(entityStateListeners, "entityStateListeners");

            var stateListeners = entityStateListeners.ToArray();
            _entityStateListeners = stateListeners.Length == 0 ? null : stateListeners;
        }

        public virtual void StateChanging([NotNull] StateEntry entry, EntityState newState)
        {
            Check.NotNull(entry, "entry");

            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _entityStateListeners)
            {
                listener.StateChanging(entry, newState);
            }
        }

        public virtual void StateChanged([NotNull] StateEntry entry, EntityState oldState)
        {
            Check.NotNull(entry, "entry");

            if (_entityStateListeners == null)
            {
                return;
            }

            foreach (var listener in _entityStateListeners)
            {
                listener.StateChanged(entry, oldState);
            }
        }
    }
}
