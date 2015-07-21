// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Migrations.Infrastructure
{
    public class MigrationIdGenerator : IMigrationIdGenerator
    {
        private const string Format = "yyyyMMddHHmmss";

        private DateTime _lastTimestamp = DateTime.MinValue;

        public virtual string CreateId(string name) => NextTimestamp() + "_" + name;
        public virtual string GetName(string id) => id.Substring(Format.Length + 1);
        public virtual bool IsValidId(string value)
            => Regex.IsMatch(value, $"[0-9]{{{Format.Length}}}_.+", default(RegexOptions), TimeSpan.FromMilliseconds(1000.0));

        public virtual string ResolveId(string nameOrId, IReadOnlyList<Migration> migrations)
        {
            Check.NotEmpty(nameOrId, nameof(nameOrId));
            Check.NotNull(migrations, nameof(migrations));

            var candidates = IsValidId(nameOrId)
                ? migrations.Where(m => m.Id == nameOrId)
                    .Concat(migrations.Where(m => string.Equals(m.Id, nameOrId, StringComparison.OrdinalIgnoreCase)))
                : migrations.Where(m => GetName(m.Id) == nameOrId)
                    .Concat(migrations.Where(m => string.Equals(GetName(m.Id), nameOrId, StringComparison.OrdinalIgnoreCase)));

            var candidate = candidates.Select(m => m.Id).FirstOrDefault();
            if (candidate == null)
            {
                throw new InvalidOperationException(Strings.MigrationNotFound(nameOrId));
            }

            return candidate;
        }

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
