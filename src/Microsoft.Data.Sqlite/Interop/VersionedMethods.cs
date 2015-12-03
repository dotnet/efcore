// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Sqlite.Interop
{
    internal class VersionedMethods
    {
        private static readonly BaseStrategy _strategy;

        static VersionedMethods()
        {
            var version = new Version(NativeMethods.sqlite3_libversion());
            if (version >= new Version(3, 7, 15))
            {
                _strategy = new Strategy3_7_15();
            }
            else if (version >= new Version(3, 7, 14))
            {
                _strategy = new Strategy3_7_14();
            }
            else if (version >= new Version(3, 7, 10))
            {
                _strategy = new Strategy3_7_10();
            }
            else
            {
                _strategy = new BaseStrategy();
            }
        }

        public static string GetErrorString(int rc)
            => _strategy.GetErrorString(rc);

        public static int Close(IntPtr db)
           => _strategy.Close(db);

        public static string GetFilename(Sqlite3Handle db, string zDbName)
            => _strategy.GetFilename(db, zDbName);

        private class Strategy3_7_15 : Strategy3_7_14
        {
            public override string GetErrorString(int rc)
                => NativeMethods.sqlite3_errstr(rc) + " " + base.GetErrorString(rc);
        }

        private class Strategy3_7_14 : Strategy3_7_10
        {
            public override int Close(IntPtr db)
                => NativeMethods.sqlite3_close_v2(db);
        }

        private class Strategy3_7_10 : BaseStrategy
        {
            public override string GetFilename(Sqlite3Handle db, string zDbName)
                => NativeMethods.sqlite3_db_filename(db, zDbName);
        }

        private class BaseStrategy
        {
            public virtual string GetErrorString(int rc)
                => Strings.DefaultNativeError;

            public virtual int Close(IntPtr db)
                => NativeMethods.sqlite3_close(db);

            public virtual string GetFilename(Sqlite3Handle db, string zDbName)
                => null;
        }
    }
}