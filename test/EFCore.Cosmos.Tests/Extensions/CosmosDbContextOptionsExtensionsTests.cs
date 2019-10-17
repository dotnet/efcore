// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
    }
}
