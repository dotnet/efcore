// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

// ReSharper disable InconsistentNaming
// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    public class InitializationTests
    {
        private ColdStartSandbox _sandbox;
        private ColdStartEnabledTests _testClass;

        [Params(true, false)]
        public bool Cold { get; set; }

        [GlobalSetup]
        public virtual void Initialize()
        {
            if (Cold)
            {
                _sandbox = new ColdStartSandbox();
                _testClass = _sandbox.CreateInstance<ColdStartEnabledTests>();
            }
            else
            {
                _testClass = new ColdStartEnabledTests();
            }
        }

        [GlobalCleanup]
        public virtual void CleanupContext()
        {
            _sandbox?.Dispose();
        }

        [Benchmark]
        public virtual void CreateAndDisposeUnusedContext()
        {
            _testClass.CreateAndDisposeUnusedContext(Cold ? 1 : 10000);
        }

        [Benchmark]
        public virtual void InitializeAndQuery_AdventureWorks()
        {
            _testClass.InitializeAndQuery_AdventureWorks(Cold ? 1 : 1000);
        }

        [Benchmark]
        public virtual void InitializeAndSaveChanges_AdventureWorks()
        {
            _testClass.InitializeAndSaveChanges_AdventureWorks(Cold ? 1 : 100);
        }

        [Benchmark]
        public virtual void BuildModel_AdventureWorks()
        {
            var builder = new DbModelBuilder();
            AdventureWorksContext.ConfigureModel(builder);
            builder.Build(new SqlConnection(AdventureWorksFixture.ConnectionString));
        }

        private class ColdStartEnabledTests : MarshalByRefObject
        {
            public virtual void CreateAndDisposeUnusedContext(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    // ReSharper disable once UnusedVariable
                    using (var context = AdventureWorksFixture.CreateContext())
                    {
                    }
                }
            }

            public virtual void InitializeAndQuery_AdventureWorks(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    using (var context = AdventureWorksFixture.CreateContext())
                    {
                        context.Department.First();
                    }
                }
            }

            public virtual void InitializeAndSaveChanges_AdventureWorks(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    using (var context = AdventureWorksFixture.CreateContext())
                    {
                        context.Currency.Add(
                            new Currency { CurrencyCode = "TMP", Name = "Temporary" });

                        using (context.Database.BeginTransaction())
                        {
                            context.SaveChanges();

                            // TODO: Don't measure transaction rollback
                        }
                    }
                }
            }
        }
    }
}
