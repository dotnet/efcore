// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkConfig
    {
        private static Lazy<BenchmarkConfig> _instance = new Lazy<BenchmarkConfig>(() =>
        {
            var config = new ConfigurationBuilder(".")
                .AddJsonFile("config.json")
                .AddEnvironmentVariables()
                .Build();

            var resultDatabasesSection = config.GetSection("benchmarks:resultDatabases");

            return new BenchmarkConfig
            {
                RunIterations = bool.Parse(config["benchmarks:runIterations"]),
                ResultDatabases = resultDatabasesSection.GetChildren().Select(s => s.Value).ToArray(),
                BenchmarkDatabaseInstance = config["benchmarks:benchmarkDatabaseInstance"],
                ProductReportingVersion = config["benchmarks:productReportingVersion"],
                CustomData = config["benchmarks:customData"]
            };
        });

        private BenchmarkConfig()
        { }

        public static BenchmarkConfig Instance
        {
            get { return _instance.Value; }
        }

        public bool RunIterations { get; private set; }
        public IEnumerable<string> ResultDatabases { get; private set; }
        public string BenchmarkDatabaseInstance { get; private set; }
        public string ProductReportingVersion { get; private set; }
        public string CustomData { get; private set; }
    }
}
