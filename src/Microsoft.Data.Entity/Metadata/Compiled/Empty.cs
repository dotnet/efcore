// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Metadata.Compiled
{
    internal static class Empty
    {
        public static readonly INavigation[] Navigations = new INavigation[0];
        public static readonly IForeignKey[] ForeignKeys = new IForeignKey[0];
    }
}
