// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            var requestUri = string.Format("{0}/odata/LevelOne", BaseAddress);
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
            var requestUri = string.Format("{0}/odata/LevelTwo", BaseAddress);
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
            var requestUri = string.Format("{0}/odata/LevelThree", BaseAddress);
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
            var requestUri = string.Format("{0}/odata/LevelFour", BaseAddress);
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
