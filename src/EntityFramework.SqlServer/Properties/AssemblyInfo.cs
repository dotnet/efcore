// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Resources;
using Microsoft.Data.Entity.Storage;

[assembly: NeutralResourcesLanguage("en-US")]
[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: ProviderDesignTimeServices(
    typeName: "Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.SqlServerMetadataModelProvider",
    assemblyName: "EntityFramework.SqlServer.Design")]
