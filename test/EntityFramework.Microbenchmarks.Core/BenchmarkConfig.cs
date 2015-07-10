// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Configuration;
using System;

namespace EntityFramework.Microbenchmarks.Core
{
    public class BenchmarkConfig
    {
        private static Lazy<BenchmarkConfig> _instance = new Lazy<BenchmarkConfig>(() =>
        {
            var config = new ConfigurationBuilder(".")
                .AddJsonFile("config.json")
                .AddJsonFile("config.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return new BenchmarkConfig
            {
                RunIterations = bool.Parse(config.Get("benchmarks:runIterations")),
                ResultsDatabase = config.Get("benchmarks:resultsDatabase"),
                BenchmarkDatabaseInstance = config.Get("benchmarks:benchmarkDatabaseInstance"),
                ProductReportingVersion = config.Get("benchmarks:productReportingVersion"),
                CustomData = config.Get("benchmarks:customData")
            };
        });

        private BenchmarkConfig()
        { }

        public static BenchmarkConfig Instance
        {
            get { return _instance.Value; }
        }

        public bool RunIterations { get; private set; }
        public string ResultsDatabase { get; private set; }
        public string BenchmarkDatabaseInstance { get; private set; }
        public string ProductReportingVersion { get; private set; }
        public string CustomData { get; private set; }
    }
}
