using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Verifiers
{
    public static class AdditionalMetadataReferences
    {
        public static ReferenceAssemblies Default { get; } = CreateDefaultReferenceAssemblies();

        private static ReferenceAssemblies CreateDefaultReferenceAssemblies()
        {
            var referenceAssemblies = ReferenceAssemblies.Default;

            referenceAssemblies = referenceAssemblies.AddPackages(ImmutableArray.Create(
                new PackageIdentity("Microsoft.EntityFrameworkCore", "5.0.3")
                ));

            return referenceAssemblies;
        }
    }

}
