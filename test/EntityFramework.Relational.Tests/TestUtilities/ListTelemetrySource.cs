// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace Microsoft.Data.Entity.TestUtilities
{
    public class ListTelemetrySource : TelemetrySource
    {
        public ListTelemetrySource(List<Tuple<string, object>> telemetryList)
        {
            TelemetryList = telemetryList;
        }

        public List<Tuple<string, object>> TelemetryList { get; }

        public override void WriteTelemetry(string telemetryName, object parameters)
        {
            TelemetryList?.Add(new Tuple<string, object>(telemetryName, parameters));
        }

        public override bool IsEnabled(string telemetryName) => true;
    }
}
