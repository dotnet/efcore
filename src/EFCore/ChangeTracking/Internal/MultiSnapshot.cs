// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public readonly struct MultiSnapshot : ISnapshot
    {
        private readonly ISnapshot[] _snapshots;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MultiSnapshot([NotNull] ISnapshot[] snapshots)
        {
            _snapshots = snapshots;
        }

        internal static readonly ConstructorInfo Constructor
            = typeof(MultiSnapshot).GetDeclaredConstructor(new[] { typeof(ISnapshot[]) });

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public T GetValue<T>(int index)
            => _snapshots[index / Snapshot.MaxGenericTypes].GetValue<T>(index % Snapshot.MaxGenericTypes);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public object this[int index]
        {
            get => _snapshots[index / Snapshot.MaxGenericTypes][index % Snapshot.MaxGenericTypes];
            set => _snapshots[index / Snapshot.MaxGenericTypes][index % Snapshot.MaxGenericTypes] = value;
        }
    }
}
