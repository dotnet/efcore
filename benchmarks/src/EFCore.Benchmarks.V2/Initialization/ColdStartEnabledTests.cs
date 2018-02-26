// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Benchmarks.Models.AdventureWorks;

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Benchmarks.Initialization
{
    public abstract class ColdStartEnabledTests : MarshalByRefObject
    {
        protected abstract AdventureWorksContextBase CreateContext();

        public void CreateAndDisposeUnusedContext(int count)
        {
            for (var i = 0; i < count; i++)
            {
                // ReSharper disable once UnusedVariable
                using (var context = CreateContext())
                {
                }
            }
        }

        public void InitializeAndQuery_AdventureWorks(int count)
        {
            for (var i = 0; i < count; i++)
            {
                using (var context = CreateContext())
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
                using (var context = CreateContext())
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
