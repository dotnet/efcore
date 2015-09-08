// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class Sidecar : IPropertyAccessor
    {
        public static class WellKnownNames
        {
            public const string OriginalValues = "OriginalValues";
            public const string RelationshipsSnapshot = "RelationshipsSnapshot";
            public const string StoreGeneratedValues = "StoreGeneratedValues";
        }

        private readonly InternalEntityEntry _entry;

        protected Sidecar([NotNull] InternalEntityEntry entry)
        {
            _entry = entry;
        }

        public virtual InternalEntityEntry InternalEntityEntry => _entry;

        public abstract bool CanStoreValue([NotNull] IPropertyBase property);
        protected abstract object ReadValue([NotNull] IPropertyBase property);
        protected abstract void WriteValue([NotNull] IPropertyBase property, [CanBeNull] object value);
        public abstract string Name { get; }
        public abstract bool TransparentRead { get; }
        public abstract bool TransparentWrite { get; }
        public abstract bool AutoCommit { get; }

        public virtual bool HasValue([NotNull] IPropertyBase property) => CanStoreValue(property) && ReadValue(property) != null;

        public virtual object this[IPropertyBase property]
        {
            get
            {
                var value = ReadValue(property);

                return value != null
                    ? (ReferenceEquals(value, NullSentinel.Value) ? null : value)
                    : _entry[property];
            }
            set { WriteValue(property, value ?? NullSentinel.Value); }
        }

        public virtual void Commit()
        {
            _entry.RemoveSidecar(Name);

            foreach (var property in _entry.EntityType.GetPropertiesAndNavigations())
            {
                if (HasValue(property))
                {
                    CopyValueToEntry(property, this[property]);
                }
            }
        }

        public virtual void Rollback() => _entry.RemoveSidecar(Name);

        public virtual void TakeSnapshot()
        {
            foreach (var property in _entry.EntityType.GetPropertiesAndNavigations())
            {
                if (CanStoreValue(property))
                {
                    this[property] = CopyValueFromEntry(property);
                }
            }
        }

        public virtual void UpdateSnapshot()
        {
            foreach (var property in _entry.EntityType.GetPropertiesAndNavigations())
            {
                if (HasValue(property))
                {
                    this[property] = CopyValueFromEntry(property);
                }
            }
        }

        public virtual void EnsureSnapshot([NotNull] IPropertyBase property)
        {
            if (CanStoreValue(property)
                && !HasValue(property))
            {
                this[property] = CopyValueFromEntry(property);
            }
        }

        public virtual void TakeSnapshot([NotNull] IPropertyBase property)
        {
            if (CanStoreValue(property))
            {
                this[property] = CopyValueFromEntry(property);
            }
        }

        protected virtual object CopyValueFromEntry(IPropertyBase property) => _entry[property];

        protected virtual void CopyValueToEntry(IPropertyBase property, object value) => _entry[property] = value;

        protected sealed class NullSentinel
        {
            public static readonly NullSentinel Value = new NullSentinel();

            private NullSentinel()
            {
            }
        }
    }
}
