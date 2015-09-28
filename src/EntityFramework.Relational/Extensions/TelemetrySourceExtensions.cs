// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Diagnostics.Tracing;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Extensions
{
    internal static class TelemetrySourceExtensions
    {
        public static void WriteCommand(
            [NotNull] this TelemetrySource telemetrySource, 
            [NotNull] string telemetryName, 
            [NotNull] DbCommand command)
        {
            if (telemetrySource.IsEnabled(telemetryName))
            {
                telemetrySource.WriteTelemetry(telemetryName, command);
            }
        }
    }
}
