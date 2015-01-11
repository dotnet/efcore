// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ReverseEngineering;

namespace EntityFramework.ReverseEngineering.TestProvider
{
    public class TestProviderContextTemplateHelper : ContextTemplatingHelper
    {
        public TestProviderContextTemplateHelper(ContextTemplateModel model) : base(model) { }

        public override string OnConfiguringCode(string indent)
        {
            return indent + "options.UseTestProvider(\"" + ContextTemplateModel.ConnectionString + "\");";
        }

        public override string OnModelCreatingCode(string indent)
        {
            return indent + "builder.UseTestProvider(\"" + ContextTemplateModel.ConnectionString + "\");";
        }
    }
}