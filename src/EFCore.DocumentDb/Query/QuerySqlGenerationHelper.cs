// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QuerySqlGenerationHelper
    {
        public virtual string GenerateLiteral(object value)
        {
            if (value != null)
            {
                return GenerateLiteralValue((dynamic)value);
            }
            return "NULL";
        }

        private string GenerateLiteralValue(string s)
        {
            return "\"" + EscapeDoubleQuotes(s) + "\"";
        }

        public string EscapeDoubleQuotes(string s)
        {
            return s.Replace("\"", "\\\"");
        }

        private string GenerateLiteralValue(uint i)
        {
            return i.ToString();
        }

        private string GenerateLiteralValue(int i)
        {
            return i.ToString();
        }

        private string GenerateLiteralValue(double i)
        {
            return i.ToString();
        }

        private string GenerateLiteralValue(bool b)
        {
            return b ? "true" : "false";
        }

        private string GenerateLiteralValue(DateTime d)
        {
            return GenerateLiteralValue(string.Format(CultureInfo.InvariantCulture, @"{0:yyyy-MM-ddTHH:mm:ss.fffK}", d));
        }
    }
}
