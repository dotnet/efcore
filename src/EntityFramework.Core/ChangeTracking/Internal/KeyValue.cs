// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct KeyValue<T> : IKeyValue
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly IEqualityComparer _equalityComparer = CreateComparer();

        private static IEqualityComparer CreateComparer()
        {
            if (typeof(T) == typeof(byte[]))
            {
                return StructuralComparisons.StructuralEqualityComparer;
            }

            return typeof(T) == typeof(object[])
                ? CompositeKeyComparisons.CompositeKeyEqualityComparer
                : EqualityComparer<T>.Default;
        }

        private readonly T _value;

        public KeyValue([CanBeNull] IKey key, [CanBeNull] T value)
        {
            Key = key;
            _value = value;
        }

        public bool IsInvalid => Key == null;

        public IKey Key { get; }

        public object Value => _value;

        public override bool Equals(object obj)
        {
            var entityKey = (KeyValue<T>)obj;

            return
                ReferenceEquals(Key, entityKey.Key)
                && _equalityComparer.Equals(_value, entityKey._value);
        }

        public override int GetHashCode()
        {
            if (Key == null)
            {
                return 0;
            }

            return (Key.GetHashCode() * 397)
                   ^ _equalityComparer.GetHashCode(_value);
        }

        [UsedImplicitly]
        private string DebuggerDisplay
            => $"{string.Join(", ", Key.Properties.Select(p => p.DeclaringEntityType.Name + "." + p.Name))}"
               + $".({string.Join(", ", (_value as object[]) ?? new object[] { _value })})";
    }

    public static class CompositeKeyComparisons
    {
        public static readonly IEqualityComparer CompositeKeyEqualityComparer = new CompositeComparer();

        private sealed class CompositeComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object x, object y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                var xs = (object[])x;
                var ys = (object[])y;

                if (xs.Length != ys.Length)
                {
                    return false;
                }

                var structuralEqualityComparer
                    = StructuralComparisons.StructuralEqualityComparer;

                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < xs.Length; i++)
                {
                    if (!structuralEqualityComparer.Equals(xs[i], ys[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                var values = (object[])obj;

                var structuralEqualityComparer
                    = StructuralComparisons.StructuralEqualityComparer;

                var hashCode = 0;

                // ReSharper disable once ForCanBeConvertedToForeach
                // ReSharper disable once LoopCanBeConvertedToQuery
                for (var i = 0; i < values.Length; i++)
                {
                    hashCode
                        = (hashCode * 397)
                          ^ structuralEqualityComparer.GetHashCode(values[i]);
                }

                return hashCode;
            }
        }
    }
}
