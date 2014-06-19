// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class AtsTestFramework : XunitTestFramework
    {
        protected override ITestFrameworkDiscoverer CreateDiscoverer(IAssemblyInfo assemblyInfo)
        {
            return new AtsTestDiscoverer(assemblyInfo, SourceInformationProvider);
        }
    }

    public class AtsTestDiscoverer : XunitTestFrameworkDiscoverer
    {
        public AtsTestDiscoverer(IAssemblyInfo assemblyInfo, ISourceInformationProvider sourceProvider)
            : base(assemblyInfo, sourceProvider)
        {
        }

        protected override bool FindTestsForType(ITypeInfo type, bool includeSourceInformation, IMessageBus messageBus)
        {
            if (type.GetCustomAttributes(typeof(RunIfConfiguredAttribute)) == null
                || TestConfig.Instance.IsConfigured)
            {
                return base.FindTestsForType(type, includeSourceInformation, messageBus);
            }
            throw new Exception(String.Format("Warning: could not run tests for {0} because configuration is missing", type.Name));
        }
    }

    public class RunIfConfiguredAttribute : Attribute
    {
    }
}
