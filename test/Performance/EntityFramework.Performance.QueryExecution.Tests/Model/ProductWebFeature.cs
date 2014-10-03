// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace QueryExecution.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("DefaultContainerStore.ProductWebFeature")]
    public partial class ProductWebFeature
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FeatureId { get; set; }

        public int? ProductId { get; set; }

        public int? PhotoId { get; set; }

        public int ReviewId { get; set; }

        [Required]
        public string Heading { get; set; }

        public virtual ProductPhoto Photo { get; set; }

        public virtual ProductReview Review { get; set; }
    }
}
