// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query;

public class SpatialQuerySqlServerGeographyFixture : SpatialQuerySqlServerFixture
{
    private NtsGeometryServices _geometryServices;
    private GeometryFactory _geometryFactory;

    public NtsGeometryServices GeometryServices
        => LazyInitializer.EnsureInitialized(
            ref _geometryServices,
            () => CreateGeometryServices());

    protected static NtsGeometryServices CreateGeometryServices()
        => new(
            NtsGeometryServices.Instance.DefaultPrecisionModel,
            4326);

    public override GeometryFactory GeometryFactory
        => LazyInitializer.EnsureInitialized(
            ref _geometryFactory,
            () => GeometryServices.CreateGeometryFactory());

    protected override string StoreName
        => "SpatialQueryGeographyTest";

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection.AddSingleton(GeometryServices))
            .AddSingleton<IRelationalTypeMappingSource, ReplacementTypeMappingSource>();

    protected class ReplacementTypeMappingSource : SqlServerTypeMappingSource
    {
        public ReplacementTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => mappingInfo.ClrType == typeof(GeoPoint)
                ? ((RelationalTypeMapping)base.FindMapping(typeof(Point))
                    .Clone(new GeoPointConverter(CreateGeometryServices().CreateGeometryFactory())))
                .Clone("geography", null)
                : base.FindMapping(mappingInfo);
    }
}
