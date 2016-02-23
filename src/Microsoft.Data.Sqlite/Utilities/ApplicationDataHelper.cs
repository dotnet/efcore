// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Data.Sqlite.Utilities
{
    public class ApplicationDataHelper
    {
        private static Lazy<object> _appData = new Lazy<object>(() => LoadAppData());
        private static Lazy<string> _localFolder = new Lazy<string>(() => GetFolderPath("LocalFolder"));
        private static Lazy<string> _tempFolder = new Lazy<string>(() => GetFolderPath("TemporaryFolder"));
        public static object CurrentApplicationData => _appData.Value;
        public static string TemporaryFolderPath => _tempFolder.Value;
        public static string LocalFolderPath => _localFolder.Value;

        private static object LoadAppData()
        {
            try
            {
                return Type.GetType("Windows.Storage.ApplicationData, Windows, ContentType=WindowsRuntime")
                    ?.GetRuntimeProperty("Current").GetValue(null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException?.HResult == -2147009196)
            {
                // Ignore "The process has no package identity."
                return null;
            }
        }

        private static string GetFolderPath(string propertyName)
        {
            var appDataType = CurrentApplicationData?.GetType();
            var temporaryFolder = appDataType?.GetRuntimeProperty(propertyName).GetValue(CurrentApplicationData);
            return temporaryFolder?.GetType().GetRuntimeProperty("Path").GetValue(temporaryFolder) as string;
        }
    }
}