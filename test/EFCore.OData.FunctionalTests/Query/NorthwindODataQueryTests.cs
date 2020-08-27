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
    public class NorthwindODataQueryTests : ODataQueryTestBase, IClassFixture<NorthwindODataQueryTestFixture>
    {
        public NorthwindODataQueryTests(NorthwindODataQueryTestFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public async Task Basic_query_customers()
        {
            var requestUri = string.Format("{0}/odata/Customers", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#Customers", result["@odata.context"].ToString());
            var customers = result["value"] as JArray;

            Assert.Equal(91, customers.Count);
        }

        [ConditionalFact]
        public async Task Basic_query_select_single_customer()
        {
            var requestUri = string.Format(@"{0}/odata/Customers('ALFKI')", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#Customers/$entity", result["@odata.context"].ToString());
            Assert.Equal("ALFKI", result["CustomerID"].ToString());
        }

        [ConditionalFact]
        public async Task Query_for_alfki_expand_orders()
        {
            var requestUri = string.Format(@"{0}/odata/Customers?$filter=CustomerID eq 'ALFKI'&$expand=Orders", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#Customers", result["@odata.context"].ToString());
            var customers = result["value"] as JArray;

            Assert.Single(customers);
            Assert.Equal("ALFKI", customers[0]["CustomerID"]);
            var orders = customers[0]["Orders"] as JArray;
            Assert.Equal(6, orders.Count);
        }

        [ConditionalFact]
        public async Task Basic_query_orders()
        {
            var requestUri = string.Format("{0}/odata/Orders", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#Orders", result["@odata.context"].ToString());
            var orders = result["value"] as JArray;

            Assert.Equal(830, orders.Count);
        }

        [ConditionalFact]
        public async Task Query_orders_select_single_property()
        {
            var requestUri = string.Format("{0}/odata/Orders?$select=OrderDate", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#Orders(OrderDate)", result["@odata.context"].ToString());
            var orderDates = result["value"] as JArray;

            Assert.Equal(830, orderDates.Count);
        }

        [ConditionalFact(Skip = "TODO: fix routing")]
        public async Task Basic_query_order_details()
        {
            var requestUri = string.Format("{0}/odata/Order Details", BaseAddress);
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsObject<JObject>();

            Assert.Contains("$metadata#OrderDetails", result["@odata.context"].ToString());
        }
    }
}
