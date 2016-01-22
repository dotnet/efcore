// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    public struct MultiSnapshot : ISnapshot
    {
        private readonly ISnapshot[] _snapshots;

        public MultiSnapshot([NotNull] ISnapshot[] snapshots)
        {
            _snapshots = snapshots;
        }

        internal static readonly ConstructorInfo Constructor 
            = typeof(MultiSnapshot).GetDeclaredConstructor(new[] { typeof(ISnapshot[]) });

        public T GetValue<T>(int index) 
            => _snapshots[index / Snapshot.MaxGenericTypes].GetValue<T>(index % Snapshot.MaxGenericTypes);

        public object this[int index]
        {
            get { return _snapshots[index / Snapshot.MaxGenericTypes][index % Snapshot.MaxGenericTypes]; }
            set { _snapshots[index / Snapshot.MaxGenericTypes][index % Snapshot.MaxGenericTypes] = value; }
        }
    }
}
