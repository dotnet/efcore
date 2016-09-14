// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public static class DbContextHelpers
    {
        public static void ExecuteWithStrategyInTransaction<TContext>(
            Func<TContext> createContext,
            Action<DatabaseFacade, IDbContextTransaction> useTransaction,
            Action<TContext> testOperation,
            Action<TContext> nestedTestOperation1 = null,
            Action<TContext> nestedTestOperation2 = null,
            Action<TContext> nestedTestOperation3 = null)
            where TContext : DbContext
        {
            using (var c = createContext())
            {
                c.Database.CreateExecutionStrategy().Execute(context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            testOperation(context);

                            if (nestedTestOperation1 == null)
                            {
                                return;
                            }
                            using (var innerContext1 = createContext())
                            {
                                useTransaction(innerContext1.Database, transaction);
                                nestedTestOperation1(innerContext1);

                                if (nestedTestOperation2 == null)
                                {
                                    return;
                                }
                                using (var innerContext2 = createContext())
                                {
                                    useTransaction(innerContext2.Database, transaction);
                                    nestedTestOperation2(innerContext2);

                                    if (nestedTestOperation3 == null)
                                    {
                                        return;
                                    }
                                    using (var innerContext3 = createContext())
                                    {
                                        useTransaction(innerContext3.Database, transaction);
                                        nestedTestOperation3(innerContext3);
                                    }
                                }
                            }
                        }
                    }, c);
            }
        }

        public static async Task ExecuteWithStrategyInTransactionAsync<TContext>(
            Func<TContext> createContext,
            Action<DatabaseFacade, IDbContextTransaction> useTransaction,
            Func<TContext, Task> testOperation,
            Func<TContext, Task> nestedTestOperation1 = null,
            Func<TContext, Task> nestedTestOperation2 = null,
            Func<TContext, Task> nestedTestOperation3 = null)
            where TContext : DbContext
        {
            using (var c = createContext())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(async context =>
                    {
                        using (var transaction = await context.Database.BeginTransactionAsync())
                        {
                            await testOperation(context);

                            if (nestedTestOperation1 == null)
                            {
                                return;
                            }
                            using (var innerContext1 = createContext())
                            {
                                useTransaction(innerContext1.Database, transaction);
                                await nestedTestOperation1(innerContext1);

                                if (nestedTestOperation2 == null)
                                {
                                    return;
                                }
                                using (var innerContext2 = createContext())
                                {
                                    useTransaction(innerContext2.Database, transaction);
                                    await nestedTestOperation2(innerContext2);

                                    if (nestedTestOperation3 == null)
                                    {
                                        return;
                                    }
                                    using (var innerContext3 = createContext())
                                    {
                                        useTransaction(innerContext3.Database, transaction);
                                        await nestedTestOperation3(innerContext3);
                                    }
                                }
                            }
                        }
                    }, c);
            }
        }
    }
}
