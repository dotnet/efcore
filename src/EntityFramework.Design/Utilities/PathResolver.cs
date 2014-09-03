// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
#if NET451 || ASPNETCORE50
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Infrastructure;
#endif

namespace Microsoft.Data.Entity.Design.Utilities
{
    internal static class PathResolver
    {
        private static string ApplicationBaseDirectory
        {
            get
            {
#if NET451 || ASPNETCORE50
                try
                {
                    var applicationBasePath = TryGetApplicationBasePath();
                    if (applicationBasePath != null)
                    {
                        return applicationBasePath;
                    }
                }
                catch (FileNotFoundException)
                {
                    // Ignore. Running outside of Project K
                }
#endif

#if ASPNETCORE50
                return ApplicationContext.BaseDirectory;
#else
                return AppDomain.CurrentDomain.BaseDirectory;
#endif
            }
        }

#if NET451 || ASPNETCORE50
        private static string TryGetApplicationBasePath()
        {
            var locator = CallContextServiceLocator.Locator;

            if (locator != null)
            {
                var appEnv = (IApplicationEnvironment)locator.ServiceProvider.GetService(typeof(IApplicationEnvironment));
                return appEnv.ApplicationBasePath;
            }

            return null;
        }
#endif

        public static string ResolveAppRelativePath(string path)
        {
            return Path.Combine(ApplicationBaseDirectory, path);
        }
    }
}
