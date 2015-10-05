// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: DesignTimeProviderServices(
    typeName: "Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.SqlServerDesignTimeMetadataProviderFactory",
    assemblyName: "EntityFramework.MicrosoftSqlServer.Design")]
