// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

// Mapped as an owned type
public class Site
{
    public string Name { get; set; } = null!;
    public SiteAddress Address { get; set; } = null!;
}

// Mapped as a complex type on the owned type
public class SiteAddress
{
    public string Street { get; set; } = null!;
    public string City { get; set; } = null!;
    public SiteCountry Country { get; set; } = null!;
}

public class SiteCountry
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
}
