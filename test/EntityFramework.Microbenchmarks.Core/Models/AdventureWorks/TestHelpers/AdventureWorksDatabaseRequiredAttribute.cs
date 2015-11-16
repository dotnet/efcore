// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Xunit.Sdk;

namespace EntityFramework.Microbenchmarks.Core.Models.AdventureWorks.TestHelpers
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AdventureWorksDatabaseRequiredAttribute : Attribute, ITestCondition
    {
        private static readonly Lazy<bool> _databaseExists = new Lazy<bool>(() =>
            {
                using (var connection = new SqlConnection(AdventureWorksFixtureBase.ConnectionString))
                {
                    try
                    {
                        connection.Open();
                        connection.Close();
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            });
        
        public bool IsMet => _databaseExists.Value;
        
        public string SkipReason => $"AdventureWorks2014 database does not exist on {BenchmarkConfig.Instance.BenchmarkDatabaseInstance}. Download the AdventureWorks backup from https://msftdbprodsamples.codeplex.com/downloads/get/880661 and restore it to {BenchmarkConfig.Instance.BenchmarkDatabaseInstance} to enable these tests.";
    }
}
