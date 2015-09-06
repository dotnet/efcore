// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace Microsoft.Data.Entity.Migrations.Internal
{
    public class MigrationsIdGenerator : IMigrationsIdGenerator
    {
        private const string Format = "yyyyMMddHHmmss";

        private DateTime _lastTimestamp = DateTime.MinValue;

        public virtual string GetName(string id) => id.Substring(Format.Length + 1);
        public virtual bool IsValidId(string value)
            => Regex.IsMatch(value, $"[0-9]{{{Format.Length}}}_.+", default(RegexOptions), TimeSpan.FromMilliseconds(1000.0));

        public virtual string GenerateId(string name)
        {
            var now = DateTime.UtcNow;
            var timestamp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            lock (this)
            {
                if (timestamp <= _lastTimestamp)
                {
                    timestamp = _lastTimestamp.AddSeconds(1);
                }

                _lastTimestamp = timestamp;
            }

            return timestamp.ToString(Format) + "_" + name;
        }
    }
}
