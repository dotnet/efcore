// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable PossibleNullReferenceException
namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    internal class AnnotationComparer : IEqualityComparer<IAnnotation>, IComparer<IAnnotation>
    {
        public static readonly AnnotationComparer Instance = new AnnotationComparer();

        private AnnotationComparer()
        {
        }

        public int Compare(IAnnotation x, IAnnotation y) => StringComparer.Ordinal.Compare(x.Name, y.Name);

        public bool Equals(IAnnotation x, IAnnotation y)
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
                   && (x.Name == CoreAnnotationNames.ValueGeneratorFactoryAnnotation
                       || x.Name == CoreAnnotationNames.TypeMapping
                       || Equals(x.Value, y.Value));
        }

        public int GetHashCode(IAnnotation obj) => obj.Name.GetHashCode() ^ obj.Value?.GetHashCode() ?? 0;
    }
}
