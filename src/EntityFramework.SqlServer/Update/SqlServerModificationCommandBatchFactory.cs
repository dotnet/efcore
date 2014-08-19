// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.SqlServer.Update
{
    public class SqlServerModificationCommandBatchFactory : ModificationCommandBatchFactory
    {
        private static readonly string MaxBatchSizeConfigurationKey = "Data:SqlServer:MaxBatchSize";
        private int? _maxBatchSize;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected SqlServerModificationCommandBatchFactory()
        {
        }

        public SqlServerModificationCommandBatchFactory(
            [NotNull] SqlServerSqlGenerator sqlGenerator,
            [CanBeNull] IEnumerable<IConfiguration> configurations)
            : base(sqlGenerator)
        {
            var _configuration = (configurations == null ? null : configurations.FirstOrDefault());

            string maxBatchSizeString = null;

            if (_configuration != null
                && _configuration.TryGet(MaxBatchSizeConfigurationKey, out maxBatchSizeString))
            {
                int maxBatchSize;
                if (!Int32.TryParse(maxBatchSizeString, out maxBatchSize))
                {
                    throw new InvalidOperationException(Strings.FormatIntegerConfigurationValueFormatError(MaxBatchSizeConfigurationKey, maxBatchSizeString));
                }
                _maxBatchSize = maxBatchSize;
            }   
        }

        public override ModificationCommandBatch Create()
        {
            return new SqlServerModificationCommandBatch(_maxBatchSize);
        }
    }
}
