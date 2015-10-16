// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Metadata
{
    public class RelationalDesignAnnotationNames
    {
        public const string AnnotationPrefix = "RelationalDesign:";
        public const string DependentEndNavPropName = AnnotationPrefix + "DependentEndNavPropName";
        public const string PrincipalEndNavPropName = AnnotationPrefix + "PrincipalEndNavPropName";
        public const string EntityTypeError = AnnotationPrefix + "EntityTypeError";
        public const string ExplicitValueGenerationNever = AnnotationPrefix + "ExplicitValueGeneratedNever";
    }
}
