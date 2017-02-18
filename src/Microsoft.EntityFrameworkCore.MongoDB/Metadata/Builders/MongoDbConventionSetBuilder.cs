using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class MongoDbConventionSetBuilder : IConventionSetBuilder
    {
        private readonly ICurrentDbContext _currentDbContext;

        public MongoDbConventionSetBuilder([NotNull] ICurrentDbContext currentDbContext)
        {
            _currentDbContext = Check.NotNull(currentDbContext, nameof(currentDbContext));
        }

        public virtual ConventionSet AddConventions([NotNull] ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            var mongoDatabaseAttributeConvention = new MongoDatabaseAttributeConvention(_currentDbContext.Context);
            PropertyDiscoveryConvention mongoDbPropertyDiscoveryConvention = new MongoDbPropertyDiscoveryConvention();
            RelationshipDiscoveryConvention mongoDbRelationshipDiscoveryConvention = new MongoDbRelationshipDiscoveryConvention();
            PropertyMappingValidationConvention mongoDbPropertyMappingValidationConvention
                = new MongoDbPropertyMappingValidationConvention();
            DatabaseGeneratedAttributeConvention mongoDbDatabaseGeneratedAttributeConvention
                = new MongoDbDatabaseGeneratedAttributeConvention();

            conventionSet.ModelInitializedConventions
                .With(mongoDatabaseAttributeConvention);

            conventionSet.EntityTypeAddedConventions
                .Replace(mongoDbPropertyDiscoveryConvention)
                .Replace(mongoDbRelationshipDiscoveryConvention)
                .With(new MongoDbDerivedTypeAttributeConvention());

            conventionSet.BaseEntityTypeSetConventions
                .Replace(mongoDbPropertyDiscoveryConvention)
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.EntityTypeMemberIgnoredConventions
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.PropertyAddedConventions
                .Replace(mongoDbDatabaseGeneratedAttributeConvention);

            conventionSet.PropertyFieldChangedConventions
                .Replace(mongoDbDatabaseGeneratedAttributeConvention);

            conventionSet.NavigationAddedConventions
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.NavigationRemovedConventions
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.ModelBuiltConventions
                .Replace(mongoDbPropertyMappingValidationConvention);

            return conventionSet;
        }
    }
}