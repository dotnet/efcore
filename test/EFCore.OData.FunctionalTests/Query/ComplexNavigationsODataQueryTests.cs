﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsODataQueryTests : ODataQueryTestBase, IClassFixture<ComplexNavigationsODataQueryTestFixture>
    {
        public ComplexNavigationsODataQueryTests(ComplexNavigationsODataQueryTestFixture fixture)
            : base(fixture)
        {
        }

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
    }
}
