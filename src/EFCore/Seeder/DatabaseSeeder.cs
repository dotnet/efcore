// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Seeder.Attributes;

namespace Microsoft.EntityFrameworkCore.Seeder
{
    /// <summary>
    /// Seeds database.
    /// </summary>
    /// <typeparam name="T">DbContext that models are defined in it</typeparam>
    public class DatabaseSeeder<T> : IDatabaseSeeder
        where T : DbContext
    {
        private T DbContext { get; }
        private string Environment { get; set; }
        private IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Creates a DatabaseSeeder that seeds database.
        /// </summary>
        /// <param name="serviceProvider">IServiceCollection which DbContext is registered in it</param>
        /// <param name="dbContext">DbContext which models are defined in it</param>
        public DatabaseSeeder(IServiceProvider serviceProvider, T dbContext)
        {
            ServiceProvider = serviceProvider;
            DbContext = dbContext;
            Environment = "Development";
        }

        /// <summary>
        /// Sets environment for seeder.
        /// </summary>
        /// <param name="environment">The environment that seeder is run in it</param>
        /// <returns>IDatabaseSeeder that can seed the database.</returns>
        public IDatabaseSeeder SetEnvironment(string environment)
        {
            Environment = environment;
            return this;
        }

        /// <summary>
        /// Runs seeders.
        /// NOTE: Seeders SHOULD NOT run on a dotnet-ef process.
        /// </summary>
        /// <param name="isEfProcess">Determines the process is dotnet-ef process or not.</param>
        public void EnsureSeeded(bool isEfProcess)
        {
            if (isEfProcess)
            {
                return;
            }

            var seeders = GetSeeders();

            seeders.ForEach(
                seederInfo =>
                {
                    if (Environment.ToLower() != "Production" && seederInfo.SeederAttribute.Production)
                    {
                        return;
                    }

                    var seederInstance = CreateSeederInstance(seederInfo);
                    if (seederInstance == null)
                    {
                        return;
                    }

                    var dbSetMethod = GetDbSetMethod(seederInfo);
                    if (dbSetMethod == null)
                    {
                        return;
                    }

                    var dbSet = dbSetMethod.Invoke(DbContext, parameters: null);
                    if (dbSet == null)
                    {
                        return;
                    }

                    var countMethod = GetCountMethod(seederInfo);
                    if (countMethod == null)
                    {
                        return;
                    }

                    var dataCount = (int)countMethod.Invoke(dbSet, new[] { dbSet });

                    if (dataCount != 0 && !seederInfo.SeederAttribute.Force)
                    {
                        return;
                    }

                    if (seederInfo.IsAsync())
                    {
                        Console.WriteLine($"{seederInfo.SeederAttribute.Type.Name}: Async seeders are not supported");
                    }
                    else
                    {
                        InvokeSeeder(seederInfo, seederInstance);
                    }

                    Console.WriteLine(seederInfo.SeederAttribute.Type.Name + " seeded");
                });
        }

        /// <summary>
        /// Invokes an async seeder method.
        /// </summary>
        /// <param name="seederInfo">The seeder method information.</param>
        /// <param name="seederInstance">An instance of the class which seeder is defined in it.</param>
        protected virtual void InvokeAsyncSeeder(SeederInfo seederInfo, object seederInstance)
        {
            ((Task)seederInfo.MethodInfo.Invoke(seederInstance, null))?.Wait();
        }

        /// <summary>
        /// Invokes a non-async seeder method.
        /// </summary>
        /// <param name="seederInfo">The seeder method information.</param>
        /// <param name="seederInstance">An instance of the class which seeder is defined in it.</param>
        protected virtual void InvokeSeeder(SeederInfo seederInfo, object seederInstance)
        {
            seederInfo.MethodInfo.Invoke(seederInstance, null);
        }

        /// <summary>
        /// Creates an instance from the class which seeder is defined in it.
        /// </summary>
        /// <param name="seederInfo">The seeder method information.</param>
        /// <returns>An instance of the class which seeder is defined in it.</returns>
        protected virtual object CreateSeederInstance(SeederInfo seederInfo)
        {
            var seederConstructorInfo = GetConstructorInfo(seederInfo.Type);
            var seederConstructorParameters = CreateConstructorParameters(seederConstructorInfo);
            return Activator.CreateInstance(seederInfo.Type, seederConstructorParameters);
        }

        /// <summary>
        /// Gets list of all seeders methods in AppDomain.CurrentDomain assembly
        /// </summary>
        /// <returns>List of seeders which are define in AppDomain.CurrentDomain assembly</returns>
        protected virtual List<SeederInfo> GetSeeders()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsClass)
                .SelectMany(x => x.GetMethods())
                .Where(x => x.GetCustomAttributes(typeof(SeederAttribute), false).FirstOrDefault() != null)
                .Select(
                    x => new SeederInfo(
                        x.DeclaringType,
                        x,
                        x.GetCustomAttribute(typeof(SeederAttribute)) as SeederAttribute
                    )).OrderBy(x => x.SeederAttribute.Priority)
                .ToList();
        }

        /// <summary>
        /// Gets DbSet method of seeder model from DbContext.
        /// </summary>
        /// <param name="seederInfo">The seeder method information.</param>
        /// <returns>DbSet method of seeder model from DbContext.</returns>
        protected internal virtual MethodInfo GetDbSetMethod(SeederInfo seederInfo)
        {
            return DbContext.GetType().GetMethods()
                .FirstOrDefault(x => x.Name == "Set" && x.GetParameters().Length == 0)?
                .MakeGenericMethod(seederInfo.SeederAttribute.Type);
        }

        /// <summary>
        /// Gets Count method from Queryable that should run with DbSet method to get count of a model in database.
        /// </summary>
        /// <param name="seederInfo">The seeder method information.</param>
        /// <returns>Count method from Queryable for the model that should seed.</returns>
        protected internal virtual MethodInfo GetCountMethod(SeederInfo seederInfo)
        {
            return typeof(Queryable)
                .GetMethods()
                .FirstOrDefault(x => x.Name == "Count" && x.GetParameters().Length == 1)?
                .MakeGenericMethod(seederInfo.SeederAttribute.Type);
        }

        /// <summary>
        /// Gets ConstructorInfo of the class which seeder method is defined in it.
        /// </summary>
        /// <param name="type">The class which seeder method is defined in it.</param>
        /// <returns>ConstructorInfo of the class which seeder method is defined in it.</returns>
        protected virtual ConstructorInfo GetConstructorInfo(Type type)
        {
            return type.GetConstructors().Length < 1 ? null : type.GetConstructors()[0];
        }

        /// <summary>
        /// Creates parameters from Dependency Injection for constructor of the class which seeder method is defined in it.
        /// </summary>
        /// <param name="constructor">Constructor of the class which seeder method is defined in it.</param>
        /// <returns>Parameters for constructor of the class which seeder method is defined in it.</returns>
        protected virtual object[] CreateConstructorParameters(MethodBase constructor)
        {
            return constructor.GetParameters()
                .Select(parameter => ServiceProvider.GetService(parameter.ParameterType))
                .ToArray();
        }
    }
}
