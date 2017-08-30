// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.EFCore.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Benchmarks.EFCore
{
    public class InitializationTests
    {
#if NET461
        private ColdStartSandbox _sandbox;
#endif
        private ColdStartEnabledTests _testClass;

#if NET461
        [Params(true, false)]
#elif NETCOREAPP2_0
        [Params(false)]
#endif
        public bool Cold { get; set; }

        [GlobalSetup]
        public virtual void Initialize()
        {
            if (Cold)
            {
#if NET461
                _sandbox = new ColdStartSandbox();
                _testClass = _sandbox.CreateInstance<ColdStartEnabledTests>();
#endif
            }
            else
            {
                _testClass = new ColdStartEnabledTests();
            }
        }

#if NET461
        [GlobalCleanup]
        public virtual void CleanupContext()
        {
            _sandbox?.Dispose();
        }
#endif

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
            var builder = new ModelBuilder(SqlServerConventionSetBuilder.Build());
            AdventureWorksContext.ConfigureModel(builder);

            // ReSharper disable once UnusedVariable
            var model = builder.Model;
        }

        private class ColdStartEnabledTests : MarshalByRefObject
        {
            public void CreateAndDisposeUnusedContext(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    // ReSharper disable once UnusedVariable
                    using (var context = AdventureWorksFixture.CreateContext())
                    {
                    }
                }
            }

            public void InitializeAndQuery_AdventureWorks(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    using (var context = AdventureWorksFixture.CreateContext())
                    {
                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        context.Department.First();
                    }
                }
            }

            public void InitializeAndSaveChanges_AdventureWorks(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    using (var context = AdventureWorksFixture.CreateContext())
                    {
                        context.Currency.Add(
                            new Currency
                            {
                                CurrencyCode = "TMP",
                                Name = "Temporary"
                            });

                        using (context.Database.BeginTransaction())
                        {
                            context.SaveChanges();

                            // TODO: Don't mesure transaction rollback
                        }
                    }
                }
            }
        }
    }
}
