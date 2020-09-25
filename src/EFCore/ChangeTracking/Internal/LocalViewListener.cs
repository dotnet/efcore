// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <inheritdoc />
    public class LocalViewListener : ILocalViewListener
    {
        private readonly IList<Action<InternalEntityEntry, EntityState>> _viewActions
            = new List<Action<InternalEntityEntry, EntityState>>();

        /// <inheritdoc />
        public virtual void RegisterView(Action<InternalEntityEntry, EntityState> viewAction)
            => _viewActions.Add(viewAction);

        /// <inheritdoc />
        public virtual void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _viewActions.Count; i++)
            {
                _viewActions[i](entry, oldState);
            }
        }

        /// <inheritdoc />
        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
        }
    }
}
