// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Utilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable NotAccessedField.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ReturnValueOfPureMethodIsNotUsed
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace Microsoft.EntityFrameworkCore.ApplicationInsights
{
    public class DiagnosticEventForwarderTest : IClassFixture<NorthwindQuerySqlServerFixture>
    {
        private readonly NorthwindQuerySqlServerFixture _fixture;

        private readonly TestTelemetryChannel _testTelemetryChannel = new TestTelemetryChannel();
        private readonly TelemetryClient _telemetryClient;

        public DiagnosticEventForwarderTest(NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            //_testTelemetryChannel.SetTestOutputHelper(testOutputHelper);

            _telemetryClient = new TelemetryClient(
                new TelemetryConfiguration
                {
                    InstrumentationKey = Guid.NewGuid().ToString(),
                    TelemetryChannel = _testTelemetryChannel
                });

            new DiagnosticEventForwarder(_telemetryClient).Start();

            Assert.True(_telemetryClient.IsEnabled());
        }

        [ConditionalFact]
        [SqlServerConfiguredCondition]
        public void Forwards_events_as_dependencies()
        {
            using (var context = _fixture.CreateContext())
            {
                context.Customers.ToList();
            }

            Assert.Equal(4, _testTelemetryChannel.Items.Count);
            Assert.True(_testTelemetryChannel.Items.All(it => it is DependencyTelemetry));
            Assert.True(_testTelemetryChannel.Items.All(it => it.Context.Properties.ContainsKey("ConnectionId")));

            var dependencyTelemetry = (DependencyTelemetry)_testTelemetryChannel.Items[0];

            Assert.Equal("SQL", dependencyTelemetry.Type);
            Assert.Equal("Northwind", dependencyTelemetry.Target);
            Assert.Equal(RelationalEventId.ConnectionOpened.Name, dependencyTelemetry.Name);
        }

        #region TestTelemetryChannel

        private sealed class TestTelemetryChannel : ITelemetryChannel
        {
            private ITestOutputHelper _testOutputHelper;

            public void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
            {
                _testOutputHelper = testOutputHelper;
            }

            public List<ITelemetry> Items { get; } = new List<ITelemetry>();

            void ITelemetryChannel.Send(ITelemetry item)
            {
                Items.Add(item);

                _testOutputHelper?.WriteLine(PrintTelemetry((dynamic)item));
            }

            void IDisposable.Dispose()
            {
            }

            private static string PrintTelemetry(OperationTelemetry operationTelemetry)
            {
                return $"{operationTelemetry.GetType().ShortDisplayName()}: [Id='{operationTelemetry.Id}', Name='{operationTelemetry.Name}']";
            }

            private static string PrintTelemetry(DependencyTelemetry dependencyTelemetry)
            {
                return $"{dependencyTelemetry.GetType().ShortDisplayName()}: "
                       + $"[Type='{dependencyTelemetry.Type}', "
                       + $"Target='{dependencyTelemetry.Target}', "
                       + $"Name='{dependencyTelemetry.Name}', " 
                       + $"Data='{dependencyTelemetry.Data}', " 
                       + $"Duration='{dependencyTelemetry.Duration.Milliseconds}ms']";
            }

            private static string PrintTelemetry(ITelemetry item)
            {
                return item.ToString();
            }

            void ITelemetryChannel.Flush()
            {
            }

            bool? ITelemetryChannel.DeveloperMode { get; set; }
            string ITelemetryChannel.EndpointAddress { get; set; }
        }

        #endregion
    }
}
