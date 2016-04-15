// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities
{
    public class PropertyComparer : IEqualityComparer<IProperty>, IComparer<IProperty>
    {
        private readonly bool _compareAnnotations;

        public PropertyComparer(bool compareAnnotations = true)
        {
            _compareAnnotations = compareAnnotations;
        }

        public int Compare(IProperty x, IProperty y) => StringComparer.Ordinal.Compare(x.Name, y.Name);

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
                   && x.IsShadowProperty == y.IsShadowProperty
                   && x.IsNullable == y.IsNullable
                   && x.IsConcurrencyToken == y.IsConcurrencyToken
                   && x.RequiresValueGenerator == y.RequiresValueGenerator
                   && x.ValueGenerated == y.ValueGenerated
                   && x.IsReadOnlyBeforeSave == y.IsReadOnlyBeforeSave
                   && x.IsReadOnlyAfterSave == y.IsReadOnlyAfterSave
                   && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(IProperty obj) => obj.Name.GetHashCode();
    }
}
