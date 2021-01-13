// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    [DisplayName("InitializationTests")]
    public abstract class InitializationTests
    {
        protected abstract AdventureWorksContextBase CreateContext();
        protected abstract ConventionSet CreateConventionSet();

        [Benchmark]
        public virtual void CreateAndDisposeUnusedContext()
        {
            for (var i = 0; i < 10000; i++)
            {
                // ReSharper disable once UnusedVariable
                using (var context = CreateContext())
                {
                }
            }
        }

        [Benchmark]
        public virtual void InitializeAndQuery_AdventureWorks()
        {
            for (var i = 0; i < 1000; i++)
            {
                using (var context = CreateContext())
                {
                    _ = context.Department.First();
                }
            }
        }

        [Benchmark]
        public virtual void InitializeAndSaveChanges_AdventureWorks()
        {
            for (var i = 0; i < 100; i++)
            {
                using (var context = CreateContext())
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

        [Benchmark]
        public virtual void BuildModel_AdventureWorks()
        {
            var builder = new ModelBuilder(CreateConventionSet());
            AdventureWorksContextBase.ConfigureModel(builder);

            // ReSharper disable once UnusedVariable
            var model = builder.Model;
        }
    }
}
