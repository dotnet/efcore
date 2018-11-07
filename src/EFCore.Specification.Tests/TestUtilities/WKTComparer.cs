// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class WKTComparer : IEqualityComparer<string>
    {
        private static readonly WKTReader _reader = new WKTReader();

        public static WKTComparer Instance { get; } = new WKTComparer();

        private WKTComparer()
        {
        }

        public bool Equals(string x, string y)
            => x == y
                || Normalize(x) == Normalize(y);

        public static string Normalize(string text)
            => text != null
                ? _reader.Read(text).AsText()
                : null;

        public int GetHashCode(string obj)
            => throw new NotImplementedException();
    }
}
