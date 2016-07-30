// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451
using System;
using System.IO;
using System.Reflection;
using Microsoft.Data.Sqlite.Utilities;
using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class NativeLibraryLoaderTest : MarshalByRefObject
    {
        [Fact]
        public void FindsNativeLibraryNextToAssemblyLocation()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(tmpDir);
            File.Copy(GetType().Assembly.Location, Path.Combine(tmpDir, Path.GetFileName(GetType().Assembly.Location)));

            var info = new AppDomainSetup
            {
                ApplicationBase = tmpDir,
                ShadowCopyFiles = "true",
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
            };

            var domain = AppDomain.CreateDomain("TestDomain", null, info);
            var resolver = new AssemblyResolver(GetAppBaseDirectory());
            domain.AssemblyResolve += resolver.ResolveAssembly;

            string expected;
            Assert.True(NativeLibraryLoader.TryFind("sqlite3", out expected));

            try
            {
                var unwrapped = (NativeLibraryLoaderTest)domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName, "Microsoft.Data.Sqlite.NativeLibraryLoaderTest");

                Assert.NotEqual(GetAppBaseDirectory(), unwrapped.GetAppBaseDirectory());
                Assert.Equal(expected, unwrapped.GetSqliteLocation());
            }
            finally
            {
                AppDomain.Unload(domain);
                Directory.Delete(tmpDir, recursive: true);
            }
        }

        [Fact]
        public void FindsNativeLibraryFromPrivateBinPath()
        {
            var tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));
            Directory.CreateDirectory(tmpDir);
            File.Copy(GetType().Assembly.Location, Path.Combine(tmpDir, Path.GetFileName(GetType().Assembly.Location)));

            var testPrivateBinPath = Path.Combine(tmpDir, "private-bin");
            Directory.CreateDirectory(testPrivateBinPath);

            var expectedLocation = Path.Combine(testPrivateBinPath, "sqlite3.dll");
            string dllPath;
            Assert.True(NativeLibraryLoader.TryFind("sqlite3", out dllPath));
            File.Copy(dllPath, expectedLocation);

            var info = new AppDomainSetup
            {
                ApplicationBase = tmpDir,
                ShadowCopyFiles = "true",
                ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
                PrivateBinPath = testPrivateBinPath + ";" + AppDomain.CurrentDomain.BaseDirectory // test the comma-separated parsing
            };

            var domain = AppDomain.CreateDomain("TestDomain2", null, info);
            var resolver = new AssemblyResolver(GetAppBaseDirectory());
            domain.AssemblyResolve += resolver.ResolveAssembly;

            try
            {
                var unwrapped = (NativeLibraryLoaderTest)domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName, "Microsoft.Data.Sqlite.NativeLibraryLoaderTest");

                Assert.NotEqual(GetAppBaseDirectory(), unwrapped.GetAppBaseDirectory());
                Assert.Equal(expectedLocation, unwrapped.GetSqliteLocation());
            }
            finally
            {
                AppDomain.Unload(domain);
                Directory.Delete(tmpDir, recursive: true);
            }
        }

        public string GetAppBaseDirectory()
            => AppDomain.CurrentDomain.BaseDirectory;

        public string GetSqliteLocation()
        {
            string dllPath;
            NativeLibraryLoader.TryFind("sqlite3", out dllPath);
            return dllPath;
        }
    }

    [Serializable]
    public class AssemblyResolver
    {
        private readonly string _baseDir;

        public AssemblyResolver(string baseDir)
        {
            _baseDir = baseDir;
        }

        public Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var shortName = new AssemblyName(args.Name).Name;
            var path = Path.Combine(_baseDir, shortName + ".dll");
            if (File.Exists(path))
            {
                return Assembly.LoadFile(path);
            }
            return null;
        }
    }
}
#endif
