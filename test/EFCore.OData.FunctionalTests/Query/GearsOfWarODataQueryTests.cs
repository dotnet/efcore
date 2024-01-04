// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.EntityFrameworkCore.Extensions;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Query;

public class GearsOfWarODataQueryTests(GearsOfWarODataQueryTestFixture fixture) : ODataQueryTestBase(fixture), IClassFixture<GearsOfWarODataQueryTestFixture>
{
    [ConditionalFact]
    public async Task Basic_query_gears()
    {
        var requestUri = $"{BaseAddress}/odata/Gears";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#Gears", result["@odata.context"].ToString());
        var gears = result["value"] as JArray;

        Assert.Equal(5, gears.Count);
    }

    [ConditionalFact]
    public async Task Basic_query_inheritance()
    {
        var requestUri = $"{BaseAddress}/odata/Gears/Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel.Officer";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains(
            "$metadata#Gears/Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel.Officer", result["@odata.context"].ToString());
        var gears = result["value"] as JArray;

        Assert.Equal(2, gears.Count);
    }

    [ConditionalFact]
    public async Task Basic_query_single_element_from_set_composite_key()
    {
        var requestUri = $"{BaseAddress}/odata/Gears(Nickname='Marcus',SquadId=1)";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains(
            "$metadata#Gears/Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel.Officer/$entity",
            result["@odata.context"].ToString());
        Assert.Equal("Marcus", result["Nickname"].ToString());
    }

    [ConditionalFact]
    public async Task Complex_query_with_any_on_collection_navigation()
    {
        var requestUri = string.Format(@"{0}/odata/Gears?$filter=Weapons/any(w: w/Id gt 4)", BaseAddress);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#Gears", result["@odata.context"].ToString());
        var officers = result["value"] as JArray;

        Assert.Equal(3, officers.Count);
    }

    [ConditionalFact]
    public async Task Query_with_expand_and_key_projection()
    {
        var requestUri = string.Format(@"{0}/odata/Gears?$select=SquadId&$expand=Tag($select=Id)", BaseAddress);
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#Gears(SquadId,Tag(Id))", result["@odata.context"].ToString());
        var projections = result["value"] as JArray;

        Assert.Equal(5, projections.Count);
    }
}
