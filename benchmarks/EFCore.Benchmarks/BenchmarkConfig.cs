// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class BenchmarkConfig
    {
        private static readonly Lazy<BenchmarkConfig> _instance = new Lazy<BenchmarkConfig>(
            () =>
                {
                    var config = new ConfigurationBuilder()
                        .AddJsonFile("config.json")
                        .AddEnvironmentVariables()
                        .Build();

                    var resultDatabasesSection = config.GetSection("benchmarks:resultDatabases");

                    return new BenchmarkConfig
                    {
                        RunIterations = bool.Parse(config["benchmarks:runIterations"]),
                        ResultDatabases = resultDatabasesSection.GetChildren().Select(s => s.Value).ToArray(),
                        BenchmarkDatabase = config["benchmarks:benchmarkDatabase"],
                        ProductVersion = config["benchmarks:productVersion"],
                        CustomData = config["benchmarks:customData"]
                    };
                });

        private BenchmarkConfig()
        {
        }

        public static BenchmarkConfig Instance => _instance.Value;

        public bool RunIterations { get; private set; }
        public IEnumerable<string> ResultDatabases { get; private set; }
        public string BenchmarkDatabase { get; private set; }
        public string ProductVersion { get; private set; }
        public string CustomData { get; private set; }
    }
}
