// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetTopologySuite.Geometries;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalGeometryTypeMappingTest
    {
        [ConditionalFact]
        public void Comparer_uses_exact_comparison()
        {
            var geometry1 = new GeometryCollection(new[] { new Point(1, 2), new Point(3, 4) });
            var geometry2 = new GeometryCollection(new[] { new Point(3, 4), new Point(1, 2) });

            var comparer = new FakeRelationalGeometryTypeMapping<GeometryCollection>().Comparer;
            Assert.False(comparer.Equals(geometry1, geometry2));
        }

        private class FakeRelationalGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, TGeometry>
        {
            public FakeRelationalGeometryTypeMapping()
                : base(new NullValueConverter(), "geometry")
            {
            }

            private FakeRelationalGeometryTypeMapping(RelationalTypeMappingParameters parameters)
                : base(parameters, new NullValueConverter())
            {
            }

            protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
                => new FakeRelationalGeometryTypeMapping<TGeometry>(parameters);

            protected override Type WKTReaderType { get; }

            protected override string AsText(object value)
                => throw new NotImplementedException();

            protected override int GetSrid(object value)
                => throw new NotImplementedException();

            private class NullValueConverter : ValueConverter<TGeometry, TGeometry>
            {
                public NullValueConverter()
                    : base(t => t, t => t)
                {
                }
            }
        }
    }
}
