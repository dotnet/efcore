// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class OperationExecutorFactory
    {
        public virtual OperationExecutorBase Create(CommandLineOptions options)
        {
            try
            {
#if NET451
                if (!options.NoAppDomain)
                {
                    return new AppDomainOperationExecutor(
                        configFile: options.AppConfigFile,
                        assembly: options.Assembly,
                        startupAssembly: options.StartupAssembly,
                        projectDir: options.ProjectDirectory ?? Directory.GetCurrentDirectory(),
                        dataDirectory: options.DataDirectory ?? Directory.GetCurrentDirectory(),
                        contentRootPath: options.ContentRootPath,
                        rootNamespace: options.RootNamespace,
                        environment: options.EnvironmentName);
                }
#endif
                return new ReflectionOperationExecutor(
                    assembly: options.Assembly,
                    startupAssembly: options.StartupAssembly,
                    projectDir: options.ProjectDirectory ?? Directory.GetCurrentDirectory(),
                    dataDirectory: options.DataDirectory ?? Directory.GetCurrentDirectory(),
                    contentRootPath: options.ContentRootPath,
                    rootNamespace: options.RootNamespace,
                    environment: options.EnvironmentName);
            }
            catch (TypeLoadException ex) when (ex.TypeName.StartsWith(OperationExecutorBase.DesignAssemblyName, StringComparison.Ordinal))
            {
                throw new OperationErrorException(ToolsStrings.DesignDependencyIncompatible, ex);
            }
            catch (FileNotFoundException ex) when (ex.FileName.StartsWith(OperationExecutorBase.DesignAssemblyName, StringComparison.Ordinal))
            {
                throw new OperationErrorException(ToolsStrings.DesignDependencyNotFound, ex);
            }
        }
    }
}
