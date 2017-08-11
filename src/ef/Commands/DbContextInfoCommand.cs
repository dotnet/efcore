// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using Microsoft.EntityFrameworkCore.Tools.Properties;

namespace Microsoft.EntityFrameworkCore.Tools.Commands
{
    // ReSharper disable once ArrangeTypeModifiers
    partial class DbContextInfoCommand
    {
        protected override int Execute()
        {
            var result = CreateExecutor().GetContextInfo(Context.Value());

            if (_json.HasValue())
            {
                ReportJsonResult(result);
            }
            else
            {
                ReportResult(result);
            }

            return base.Execute();
        }

        private static void ReportJsonResult(IDictionary result)
        {
            Reporter.WriteData("{");
            Reporter.WriteData("  \"providerName\": \"" + Json.Escape(result["ProviderName"] as string) + "\",");
            Reporter.WriteData("  \"databaseName\": \"" + Json.Escape(result["DatabaseName"] as string) + "\",");
            Reporter.WriteData("  \"dataSource\": \"" + Json.Escape(result["DataSource"] as string) + "\",");
            Reporter.WriteData("  \"options\": \"" + Json.Escape(result["Options"] as string) + "\"");
            Reporter.WriteData("}");
        }

        private static void ReportResult(IDictionary result)
        {
            Reporter.WriteData(Resources.ProviderName(result["ProviderName"]));
            Reporter.WriteData(Resources.DatabaseName(result["DatabaseName"]));
            Reporter.WriteData(Resources.DataSource(result["DataSource"]));
            Reporter.WriteData(Resources.Options(result["Options"]));
        }
    }
}
