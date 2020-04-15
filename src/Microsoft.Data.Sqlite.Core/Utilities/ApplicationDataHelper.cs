// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal class ApplicationDataHelper
    {
        private static object _appData;
        private static string _localFolder;
        private static string _tempFolder;

        public static object CurrentApplicationData
            => _appData ??= LoadAppData();

        public static string TemporaryFolderPath
            => _tempFolder ??= GetFolderPath("TemporaryFolder");

        public static string LocalFolderPath
            => _localFolder ??= GetFolderPath("LocalFolder");

        private static object LoadAppData()
        {
            try
            {
                return Type.GetType("Windows.Storage.ApplicationData, Windows, ContentType=WindowsRuntime")
                    ?.GetRuntimeProperty("Current").GetValue(null);
            }
            catch
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
