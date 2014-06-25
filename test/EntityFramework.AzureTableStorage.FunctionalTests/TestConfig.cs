// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class TestConfig
    {
        private TestConfig()
        {
            _cstr = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_TestAccount");
            const string cliConfigPath = "App.config";
            const string vsConfigPath = "..\\..\\App.config";
            if (_cstr == null)
            {
                var configuration = new Configuration();
                if (File.Exists(cliConfigPath))
                {
                    configuration.AddXmlFile(cliConfigPath);
                }
                else if (File.Exists(vsConfigPath))
                {
                    configuration.AddXmlFile(vsConfigPath);
                }

                if (configuration.TryGet("TestAccount:ConnectionString", out _cstr))
                {
                    _cstr = _cstr.Trim();
                }
            }
        }

        private static TestConfig _instance;
        private readonly string _cstr;

        public static TestConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TestConfig();
                }
                return _instance;
            }
        }

        public bool IsConfigured
        {
            get { return !String.IsNullOrEmpty(ConnectionString); }
        }

        public string ConnectionString
        {
            get { return _cstr; }
        }
    }
}
