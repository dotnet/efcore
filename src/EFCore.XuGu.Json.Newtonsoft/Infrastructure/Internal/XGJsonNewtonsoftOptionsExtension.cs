// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Json.Newtonsoft.Infrastructure.Internal
{
    public class XGJsonNewtonsoftOptionsExtension : XGJsonOptionsExtension
    {
        public XGJsonNewtonsoftOptionsExtension()
        {
        }

        public XGJsonNewtonsoftOptionsExtension([NotNull] XGJsonOptionsExtension copyFrom)
            : base(copyFrom)
        {
        }

        protected override XGJsonOptionsExtension Clone()
            => new XGJsonNewtonsoftOptionsExtension(this);

        public override string UseJsonOptionName => nameof(XGJsonNewtonsoftDbContextOptionsBuilderExtensions.UseNewtonsoftJson);
        public override string AddEntityFrameworkName => nameof(XGJsonNewtonsoftServiceCollectionExtensions.AddEntityFrameworkXGJsonNewtonsoft);
        public override Type TypeMappingSourcePluginType => typeof(XGJsonNewtonsoftTypeMappingSourcePlugin);

        /// <summary>
        ///     Adds the services required to make the selected options work. This is used when there
        ///     is no external <see cref="IServiceProvider" /> and EF is maintaining its own service
        ///     provider internally. This allows database providers (and other extensions) to register their
        ///     required services when EF is creating an service provider.
        /// </summary>
        /// <param name="services"> The collection to add services to. </param>
        public override void ApplyServices(IServiceCollection services)
            => services.AddEntityFrameworkXGJsonNewtonsoft();
    }
}
