// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Storage;

public class RelationalGeometryTypeMappingTest
{
    [ConditionalFact]
    public void Comparer_uses_exact_comparison()
    {
        var geometry1 = new GeometryCollection([new Point(1, 2), new Point(3, 4)]);
        var geometry2 = new GeometryCollection([new Point(3, 4), new Point(1, 2)]);

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

        protected override Type WktReaderType { get; }

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
