// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestDbContextOperations(
    IOperationReporter reporter,
    Assembly assembly,
    Assembly startupAssembly,
    string projectDir,
    string rootNamespace,
    string language,
    bool nullable,
    string[] args,
    AppServiceProviderFactory appServicesFactory) : DbContextOperations(reporter, assembly, startupAssembly, projectDir, rootNamespace, language, nullable, args, appServicesFactory);
