// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public class CosmosDbContextOptionsExtensionsTests
    {
        [ConditionalFact]
        public void Can_create_options_with_specified_region()
        {
            var regionName = Regions.EastAsia;
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.Region(regionName); });

            var extension = options
                .Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(regionName, extension.Region);
        }

        [ConditionalFact]
        public void Can_create_options_with_wrong_region()
        {
            var regionName = "FakeRegion";
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.Region(regionName); });

            var extension = options
                .Options.FindExtension<CosmosOptionsExtension>();

            // The region will be validated by the Cosmos SDK, because the region list is not constant
            Assert.Equal(regionName, extension.Region);
        }

        [ConditionalFact]
        public void Can_create_options_with_correct_connection_mode()
        {
            var connectionMode = ConnectionMode.Direct;
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.ConnectionMode(connectionMode); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(connectionMode, extension.ConnectionMode);
        }

        [ConditionalFact]
        public void Throws_if_wrong_connection_mode()
        {
            var connectionMode = (ConnectionMode)958410610;
            var options = Assert.Throws<ArgumentOutOfRangeException>(
                () =>
                    new DbContextOptionsBuilder().UseCosmos(
                        "serviceEndPoint",
                        "authKeyOrResourceToken",
                        "databaseName",
                        o => { o.ConnectionMode(connectionMode); }));
        }

        [ConditionalFact]
        public void Can_create_options_and_limit_to_endpoint()
        {
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.LimitToEndpoint(); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.True(extension.LimitToEndpoint);
        }

        [ConditionalFact]
        public void Can_create_options_with_web_proxy()
        {
            var webProxy = new WebProxy();
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.WebProxy(webProxy); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Same(webProxy, extension.WebProxy);
        }

        [ConditionalFact]
        public void Can_create_options_with_request_timeout()
        {
            var requestTimeout = TimeSpan.FromMinutes(3);
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.RequestTimeout(requestTimeout); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(requestTimeout, extension.RequestTimeout);
        }

        [ConditionalFact]
        public void Can_create_options_with_open_tcp_connection_timeout()
        {
            var timeout = TimeSpan.FromMinutes(3);
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.OpenTcpConnectionTimeout(timeout); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(timeout, extension.OpenTcpConnectionTimeout);
        }

        [ConditionalFact]
        public void Can_create_options_with_idle_tcp_connection_timeout()
        {
            var timeout = TimeSpan.FromMinutes(3);
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.IdleTcpConnectionTimeout(timeout); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(timeout, extension.IdleTcpConnectionTimeout);
        }

        [ConditionalFact]
        public void Can_create_options_with_gateway_mode_max_connection_limit()
        {
            var connectionLimit = 3;
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.GatewayModeMaxConnectionLimit(connectionLimit); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(connectionLimit, extension.GatewayModeMaxConnectionLimit);
        }

        [ConditionalFact]
        public void Can_create_options_with_max_tcp_connections_per_endpoint()
        {
            var connectionLimit = 3;
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.MaxTcpConnectionsPerEndpoint(connectionLimit); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(connectionLimit, extension.MaxTcpConnectionsPerEndpoint);
        }

        [ConditionalFact]
        public void Can_create_options_with_max_requests_per_tcp_connection()
        {
            var requestLimit = 3;
            var options = new DbContextOptionsBuilder().UseCosmos(
                "serviceEndPoint",
                "authKeyOrResourceToken",
                "databaseName",
                o => { o.MaxRequestsPerTcpConnection(requestLimit); });

            var extension = options.Options.FindExtension<CosmosOptionsExtension>();

            Assert.Equal(requestLimit, extension.MaxRequestsPerTcpConnection);
        }
    }
}
