// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.TestUtilities
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

            return y == null
                ? false
                : x.Name == y.Name
                && x.ClrType == y.ClrType
                && x.IsShadowProperty() == y.IsShadowProperty()
                && x.IsNullable == y.IsNullable
                && x.IsConcurrencyToken == y.IsConcurrencyToken
                && x.ValueGenerated == y.ValueGenerated
                && x.GetBeforeSaveBehavior() == y.GetBeforeSaveBehavior()
                && x.GetAfterSaveBehavior() == y.GetAfterSaveBehavior()
                && (!_compareAnnotations || x.GetAnnotations().SequenceEqual(y.GetAnnotations(), AnnotationComparer.Instance));
        }

        public int GetHashCode(IProperty obj) => obj.Name.GetHashCode();
    }
}
