// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics.Entity.Utilities;
using Microsoft.AspNet.Http;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Diagnostics.Entity
{
    public class MigrationsEndPointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MigrationsEndPointOptions _options;
        private readonly bool _isDevMode;

        public MigrationsEndPointMiddleware([NotNull] RequestDelegate next, [NotNull] MigrationsEndPointOptions options, bool isDevMode)
        {
            Check.NotNull(next, "next");
            Check.NotNull(options, "options");

            _next = next;
            _options = options;
            _isDevMode = isDevMode;
        }

        public virtual async Task Invoke([NotNull] HttpContext context)
        {
            Check.NotNull(context, "context");

            if (_isDevMode || !_options.DevModeOnly)
            {
                if (context.Request.Path.Equals(_options.Path))
                {
                    var form = context.Request.GetFormAsync().Result;
                    var contextTypeName = form["context"];
                    var contextType = Type.GetType(contextTypeName);

                    // TODO Handle context not being registered in DI
                    using (var db = (DbContext)context.ApplicationServices.GetService(contextType))
                    {
                        var migrator = db.Configuration.Services.ServiceProvider.GetService<DbMigrator>();
                        migrator.UpdateDatabase();

                        context.Response.StatusCode = 204;
                        context.Response.Headers.Add("Pragma", new[] { "no-cache" });
                        context.Response.Headers.Add("Cache-Control", new[] { "no-cache" });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
