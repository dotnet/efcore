// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Scaffolding.Metadata
{
    public class ScaffoldingAnnotationNames
    {
        public const string AnnotationPrefix = "Scaffolding:";
        public const string UseProviderMethodName = AnnotationPrefix + "UseProviderMethodName";
        public const string DependentEndNavigation = AnnotationPrefix + "DependentEndNavigation";
        public const string PrincipalEndNavigation = AnnotationPrefix + "PrincipalEndNavigation";

        // TODO remove. Change how entity type errors are handles
        public const string EntityTypeError = AnnotationPrefix + "EntityTypeError";

        // TODO remove when configuration source is no longer hidden internally
        public const string ExplicitValueGenerationNever = AnnotationPrefix + "ExplicitValueGeneratedNever";
    }
}
