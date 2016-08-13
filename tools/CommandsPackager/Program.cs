// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace CommandPackager
{
    public class Program
    {
        private static void PrintUsage()
            => Console.WriteLine(
                @"Usage: <CONFIGURATION>");
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return 2;
            }
            try
            {
                new CommandPackager(Directory.GetCurrentDirectory(), args[0]).Run().GetAwaiter().GetResult();
                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return 1;
            }
        }
    }
}
