// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Diagnostics.Entity.Utilities;

namespace Microsoft.AspNet.Builder
{
    public static class MigrationsEndPointExtensions
    {
        public static IApplicationBuilder UseMigrationsEndPoint([NotNull] this IApplicationBuilder builder)
        {
            Check.NotNull(builder, "builder");

            return builder.UseMigrationsEndPoint(new MigrationsEndPointOptions());
        }

        public static IApplicationBuilder UseMigrationsEndPoint([NotNull] this IApplicationBuilder builder, [NotNull] MigrationsEndPointOptions options)
        {
            Check.NotNull(builder, "builder");
            Check.NotNull(options, "options");

            /* TODO: Development, Staging, or Production
            string appMode = new AppProperties(builder.Properties).Get<string>(Constants.HostAppMode);
            bool isDevMode = string.Equals(Constants.DevMode, appMode, StringComparison.Ordinal);*/
            var isDevMode = true;
            return builder.Use(next => new MigrationsEndPointMiddleware(next, options, isDevMode).Invoke);
        }
    }
}
