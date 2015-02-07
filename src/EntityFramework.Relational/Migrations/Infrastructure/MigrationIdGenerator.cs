// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Migrations.Infrastructure
{
    public class MigrationIdGenerator
    {
        private const string Format = "yyyyMMddHHmmss";

        private DateTime _lastTimestamp = DateTime.MinValue;

        public virtual string CreateId([NotNull] string name) => NextTimestamp() + "_" + name;
        public virtual string GetName([NotNull] string id) => id.Substring(Format.Length + 1);
        public virtual bool IsValidId([NotNull] string value) => Regex.IsMatch(value, "[0-9]{" + Format.Length + "}_.+");

        protected virtual string NextTimestamp()
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

            return timestamp.ToString(Format);
        }
    }
}
