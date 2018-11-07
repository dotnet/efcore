// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    [DisplayName("InitializationTests")]
    public abstract class InitializationTests<T>
        where T : ColdStartEnabledTests, new()
    {
#if NET461
        private ColdStartSandbox _sandbox;
#endif
        private ColdStartEnabledTests _testClass;

#if NET461
        [Params(true, false)]
#elif NETCOREAPP2_0 || NETCOREAPP2_1
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
                _testClass = _sandbox.CreateInstance<T>();
#endif
            }
            else
            {
                _testClass = new T();
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
            var builder = new ModelBuilder(CreateConventionSet());
            AdventureWorksContextBase.ConfigureModel(builder);

            // ReSharper disable once UnusedVariable
            var model = builder.Model;
        }

        protected abstract ConventionSet CreateConventionSet();
    }
}
