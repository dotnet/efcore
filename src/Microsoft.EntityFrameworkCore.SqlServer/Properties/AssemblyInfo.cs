// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using Microsoft.EntityFrameworkCore.Infrastructure;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: DesignTimeProviderServices(
               typeName: "Microsoft.EntityFrameworkCore.Scaffolding.Internal.SqlServerDesignTimeServices",
               assemblyName: "Microsoft.EntityFrameworkCore.SqlServer.Design, Version=1.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60",
               packageName: "Microsoft.EntityFrameworkCore.SqlServer.Design")]
[assembly: AssemblyCompany("Microsoft Corporation.")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyProduct("Microsoft EntityFramework Core")]
