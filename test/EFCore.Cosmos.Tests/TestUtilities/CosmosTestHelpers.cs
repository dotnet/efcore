// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Cosmos.TestUtilities
{
    public class CosmosTestHelpers : TestHelpers
    {
        protected CosmosTestHelpers()
        {
        }

        public static CosmosTestHelpers Instance { get; } = new CosmosTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
        {
            return services.AddEntityFrameworkCosmos();
        }

        public override IModelValidator CreateModelValidator()
        {
            var typeMappingSource = new CosmosTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>());

            return new ModelValidator(
                new ModelValidatorDependencies(
                    typeMappingSource,
                    new MemberClassifier(
                        typeMappingSource,
                        TestServiceFactory.Instance.Create<IParameterBindingFactories>())));
        }

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseCosmos(
                TestEnvironment.DefaultConnection,
                TestEnvironment.AuthToken,
                "UnitTests");
        }
    }
}
