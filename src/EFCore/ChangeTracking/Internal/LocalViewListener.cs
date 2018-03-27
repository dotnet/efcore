// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LocalViewListener : ILocalViewListener
    {
        private readonly IList<Action<InternalEntityEntry, EntityState>> _viewActions
            = new List<Action<InternalEntityEntry, EntityState>>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RegisterView(Action<InternalEntityEntry, EntityState> viewAction)
            => _viewActions.Add(viewAction);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _viewActions.Count; i++)
            {
                _viewActions[i](entry, oldState);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
        }
    }
}
