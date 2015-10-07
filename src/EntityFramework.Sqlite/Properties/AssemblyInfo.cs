// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using Microsoft.Data.Entity.Infrastructure;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: DesignTimeProviderServices(
    typeName: "Microsoft.Data.Entity.Sqlite.Design.SqliteDesignTimeServices",
    assemblyName: "EntityFramework.Sqlite.Design")]
