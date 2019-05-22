// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class UpdateAdapter : IUpdateAdapter
    {
        private readonly IStateManager _stateManager;
        private readonly IChangeDetector _changeDetector;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public UpdateAdapter(
            [NotNull] IStateManager stateManager,
            [NotNull] IChangeDetector changeDetector)
        {
            _stateManager = stateManager;
            _changeDetector = changeDetector;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IUpdateEntry FindPrincipal(IUpdateEntry dependentEntry, IForeignKey foreignKey)
            => _stateManager.FindPrincipal((InternalEntityEntry)dependentEntry, foreignKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IUpdateEntry> GetDependents(IUpdateEntry principalEntry, IForeignKey foreignKey)
            => _stateManager.GetDependents((InternalEntityEntry)principalEntry, foreignKey);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IUpdateEntry TryGetEntry(IKey key, object[] keyValues)
            => _stateManager.TryGetEntry(key, keyValues);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IUpdateEntry> Entries
            => _stateManager.Entries;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void DetectChanges()
            => _changeDetector.DetectChanges(_stateManager);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IList<IUpdateEntry> GetEntriesToSave()
            => _stateManager.GetEntriesToSave();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IUpdateEntry CreateEntry(
            IDictionary<string, object> values,
            IEntityType entityType)
            => _stateManager.CreateEntry(values, entityType);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IModel Model
            => _stateManager.Model;
    }
}
