// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.SourceGenerators.Tests.Models;

namespace Microsoft.EntityFrameworkCore.SourceGenerators
{
    [CompileTimeContext(typeof(MyContext))]
    public partial class MyRuntimeDbContext : RuntimeDbContext
    {
        // The comments below represent the generated code
        //private static readonly IModel CompiledModel = CreateCompiledModel();

        //private static IModel CreateCompiledModel()
        //{
        //    var modelBuilder = new ModelBuilder();
        //    modelBuilder.Entity<MyEntity>(eb =>
        //    {
        //        eb.Property<int>("Id");
        //        eb.Property<int>("ShadowProp");
        //    });

        //    return modelBuilder.Model;
        //}

        //public override IModel GetCompiledModel()
        //{
        //    return CompiledModel;
        //}
    }
}
