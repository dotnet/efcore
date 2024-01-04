// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.EntityFrameworkCore.Extensions;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindODataQueryTests(NorthwindODataQueryTestFixture fixture) : ODataQueryTestBase(fixture), IClassFixture<NorthwindODataQueryTestFixture>
{
    [ConditionalFact]
    public async Task Basic_query_customers()
    {
        var requestUri = $"{BaseAddress}/odata/Customers";
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
        var requestUri = $"{BaseAddress}/odata/Orders";
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
        var requestUri = $"{BaseAddress}/odata/Orders?$select=OrderDate";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#Orders(OrderDate)", result["@odata.context"].ToString());
        var orderDates = result["value"] as JArray;

        Assert.Equal(830, orderDates.Count);
    }

    [ConditionalFact]
    public async Task Basic_query_order_details()
    {
        var requestUri = $"{BaseAddress}/odata/Order Details";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#Order%20Details", result["@odata.context"].ToString());
    }

    [ConditionalFact]
    public async Task Basic_query_order_details_single_element_composite_key()
    {
        var requestUri = $"{BaseAddress}/odata/Order Details(OrderID=10248,ProductID=11)";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadAsObject<JObject>();

        Assert.Contains("$metadata#Order%20Details", result["@odata.context"].ToString());
    }
}
