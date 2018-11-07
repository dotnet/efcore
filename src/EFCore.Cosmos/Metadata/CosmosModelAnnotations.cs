// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata
{
    public class CosmosModelAnnotations : ICosmosModelAnnotations
    {
        public CosmosModelAnnotations(IModel model)
            : this(new CosmosAnnotations(model))
        {
        }

        protected CosmosModelAnnotations(CosmosAnnotations annotations) => Annotations = annotations;

        protected virtual CosmosAnnotations Annotations { get; }

        protected virtual IModel Model => (IModel)Annotations.Metadata;

        public virtual string DefaultContainerName
        {
            get => (string)Annotations.Metadata[CosmosAnnotationNames.ContainerName];

            [param: CanBeNull]
            set => SetDefaultContainerName(value);
        }

        protected virtual bool SetDefaultContainerName([CanBeNull] string value)
            => Annotations.SetAnnotation(
                CosmosAnnotationNames.ContainerName,
                Check.NullButNotEmpty(value, nameof(value)));
    }
}
