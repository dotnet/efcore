// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.ReverseEngineering;

namespace EntityFramework.SqlServer.ReverseEngineering
{
    public class SqlServerEntityTypeTemplatingHelper : EntityTypeTemplatingHelper
    {
        public SqlServerEntityTypeTemplatingHelper(EntityTypeTemplateModel model) : base(model) { }
    }
}