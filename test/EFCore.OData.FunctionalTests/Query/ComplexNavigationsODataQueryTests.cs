// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.EntityFrameworkCore.Extensions;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Query;

public class ComplexNavigationsODataQueryTests(ComplexNavigationsODataQueryTestFixture fixture) : ODataQueryTestBase(fixture), IClassFixture<ComplexNavigationsODataQueryTestFixture>
{
    [ConditionalFact]
    public async Task Query_level_ones()
    {
        var requestUri = $"{BaseAddress}/odata/LevelOne";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#LevelOne", result["@odata.context"].ToString());
        var levelOnes = result["value"] as JArray;

        Assert.Equal(13, levelOnes.Count);
    }

    [ConditionalFact]
    public async Task Query_level_twos()
    {
        var requestUri = $"{BaseAddress}/odata/LevelTwo";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#LevelTwo", result["@odata.context"].ToString());
        var levelTwos = result["value"] as JArray;

        Assert.Equal(11, levelTwos.Count);
    }

    [ConditionalFact]
    public async Task Query_level_threes()
    {
        var requestUri = $"{BaseAddress}/odata/LevelThree";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#LevelThree", result["@odata.context"].ToString());
        var levelThrees = result["value"] as JArray;

        Assert.Equal(10, levelThrees.Count);
    }

    [ConditionalFact]
    public async Task Query_level_four()
    {
        var requestUri = $"{BaseAddress}/odata/LevelFour";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#LevelFour", result["@odata.context"].ToString());
        var levelFours = result["value"] as JArray;

        Assert.Equal(10, levelFours.Count);
    }

    [ConditionalFact]
    public async Task Query_count_expand_with_filter_contains()
    {
        var requestUri =
            $"{BaseAddress}/odata/LevelOne?$count=true&$expand=OneToOne_Required_FK1&$filter=OneToOne_Required_FK1/Id in (1)";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#LevelOne(OneToOne_Required_FK1())", result["@odata.context"].ToString());
        Assert.Equal(1, result["@odata.count"]);
        var projection = result["value"] as JArray;
        Assert.Single(projection);
    }
}
