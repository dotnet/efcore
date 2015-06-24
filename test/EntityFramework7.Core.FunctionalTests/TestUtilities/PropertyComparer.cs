// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests.TestUtilities
{
    public class PropertyComparer : IEqualityComparer<IProperty>, IComparer<IProperty>
    {
        public static readonly PropertyComparer Instance = new PropertyComparer();

        private PropertyComparer()
        {
        }

        public int Compare(IProperty x, IProperty y)
        {
            return StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

        public bool Equals(IProperty x, IProperty y)
        {
            if (x == null)
            {
                return y == null;
            }

            if (y == null)
            {
                return false;
            }

            return x.Name == y.Name
                   && x.ClrType == y.ClrType
                   && x.IsNullable == y.IsNullable
                   && x.IsConcurrencyToken == y.IsConcurrencyToken
                   && x.RequiresValueGenerator == y.RequiresValueGenerator
                   && x.ValueGenerated == y.ValueGenerated
                   && x.IsReadOnlyBeforeSave == y.IsReadOnlyBeforeSave
                   && x.IsReadOnlyAfterSave == y.IsReadOnlyAfterSave
                   && x.SentinelValue == y.SentinelValue;
        }

        public int GetHashCode(IProperty obj) => obj.Name.GetHashCode();
    }
}
