// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels.ConcurrencyModel
{
    public class Chassis
    {
        public byte[] Version { get; set; }

        public int TeamId { get; set; }

        public string Name { get; set; }

        public virtual Team Team { get; set; }
    }
}
