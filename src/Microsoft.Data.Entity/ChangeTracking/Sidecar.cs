// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public abstract class Sidecar
    {
        public static class WellKnownNames
        {
            public const string OriginalValues = "OriginalValues";
            public const string StoreGeneratedValues = "StoreGeneratedValues";
        }

        private readonly StateEntry _stateEntry;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Sidecar()
        {
        }

        protected Sidecar([NotNull] StateEntry stateEntry)
        {
            Check.NotNull(stateEntry, "stateEntry");

            _stateEntry = stateEntry;
        }

        public virtual StateEntry StateEntry
        {
            get { return _stateEntry; }
        }

        public abstract bool CanStoreValue([NotNull] IProperty property);
        protected abstract object ReadValue([NotNull] IProperty property);
        protected abstract void WriteValue([NotNull] IProperty property, [CanBeNull] object value);
        public abstract string Name { get; }
        public abstract bool TransparentRead { get; }
        public abstract bool TransparentWrite { get; }
        public abstract bool AutoCommit { get; }

        public virtual bool HasValue([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return CanStoreValue(property) && ReadValue(property) != null;
        }

        public virtual object this[[param: NotNull] IProperty property]
        {
            get
            {
                Check.NotNull(property, "property");

                var value = ReadValue(property);

                return value != null
                    ? (ReferenceEquals(value, NullSentinel.Value) ? null : value)
                    : _stateEntry[property];
            }
            [param: CanBeNull]
            set
            {
                Check.NotNull(property, "property");

                WriteValue(property, value ?? NullSentinel.Value);
            }
        }

        public virtual void Commit()
        {
            _stateEntry.RemoveSidecar(Name);

            foreach (var property in _stateEntry.EntityType.Properties)
            {
                if (HasValue(property))
                {
                    _stateEntry[property] = this[property];
                }
            }
        }

        public virtual void Rollback()
        {
            _stateEntry.RemoveSidecar(Name);
        }

        public virtual void TakeSnapshot()
        {
            foreach (var property in _stateEntry.EntityType.Properties)
            {
                if (CanStoreValue(property))
                {
                    this[property] = _stateEntry[property];
                }
            }
        }

        public virtual void UpdateSnapshot()
        {
            foreach (var property in _stateEntry.EntityType.Properties)
            {
                if (HasValue(property))
                {
                    this[property] = _stateEntry[property];
                }
            }
        }

        public virtual void EnsureSnapshot([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            if (CanStoreValue(property)
                && !HasValue(property))
            {
                this[property] = _stateEntry[property];
            }
        }

        protected sealed class NullSentinel
        {
            public static readonly NullSentinel Value = new NullSentinel();

            private NullSentinel()
            {
            }
        }
    }
}
