// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.Versioning;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.PlatformAbstractions;

namespace Microsoft.EntityFrameworkCore.Design.Internal
{
    public class ApplicationEnvironment : IApplicationEnvironment
    {
        public ApplicationEnvironment([NotNull] IApplicationEnvironment applicationEnvironment)
        {
            Check.NotNull(applicationEnvironment, nameof(applicationEnvironment));

            ApplicationBasePath = applicationEnvironment.ApplicationBasePath;
            ApplicationName = applicationEnvironment.ApplicationName;
            ApplicationVersion = applicationEnvironment.ApplicationVersion;
            RuntimeFramework = applicationEnvironment.RuntimeFramework;
        }

        public virtual string ApplicationBasePath { get; [param: NotNull] set; }
        public virtual string ApplicationName { get; [param: NotNull] set; }
        public virtual string ApplicationVersion { get; [param: NotNull] set; }
        public virtual FrameworkName RuntimeFramework { get; [param: NotNull] set; }
    }
}
