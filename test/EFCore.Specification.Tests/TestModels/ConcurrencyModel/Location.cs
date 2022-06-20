// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class Location
{
    public class LocationProxy : Location, IF1Proxy
    {
        public LocationProxy(
            double latitude,
            double longitude)
            : base(latitude, longitude)
        {
        }

        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    public Location()
    {
    }

    private Location(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;

        Assert.IsType<LocationProxy>(this);
    }

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
