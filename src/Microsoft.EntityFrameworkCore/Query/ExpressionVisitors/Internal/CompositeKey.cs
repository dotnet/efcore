// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public struct CompositeKey
    {
        public static readonly ConstructorInfo CompositeKeyCtor
            = typeof(CompositeKey).GetTypeInfo().DeclaredConstructors
                .Single(c => c.GetParameters().Length == 1);

        public static bool operator ==(CompositeKey x, CompositeKey y) => x.Equals(y);
        public static bool operator !=(CompositeKey x, CompositeKey y) => !x.Equals(y);

        private readonly object[] _values;

        [UsedImplicitly]
        public CompositeKey([NotNull] object[] values)
        {
            _values = values;
        }

        public override bool Equals(object obj)
            => _values.SequenceEqual(((CompositeKey)obj)._values);

        public override int GetHashCode() => 0;
    }
}
