// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships
{
    public class InheritanceRelationshipsContext : PoolableDbContext
    {
        public static readonly string StoreName = "InheritanceRelationships";

        public InheritanceRelationshipsContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<BaseInheritanceRelationshipEntity> BaseEntities { get; set; }
        public DbSet<DerivedInheritanceRelationshipEntity> DerivedEntities { get; set; }

        public DbSet<BaseReferenceOnBase> BaseReferencesOnBase { get; set; }
        public DbSet<BaseReferenceOnDerived> BaseReferencesOnDerived { get; set; }
        public DbSet<ReferenceOnBase> ReferencesOnBase { get; set; }
        public DbSet<ReferenceOnDerived> ReferencesOnDerived { get; set; }
        public DbSet<NestedReferenceBase> NestedReferences { get; set; }

        public DbSet<BaseCollectionOnBase> BaseCollectionsOnBase { get; set; }
        public DbSet<BaseCollectionOnDerived> BaseCollectionsOnDerived { get; set; }
        public DbSet<CollectionOnBase> CollectionsOnBase { get; set; }
        public DbSet<CollectionOnDerived> CollectionsOnDerived { get; set; }
        public DbSet<NestedCollectionBase> NestedCollections { get; set; }

        public DbSet<PrincipalEntity> PrincipalEntities { get; set; }
        public DbSet<ReferencedEntity> ReferencedEntities { get; set; }

        public static void Seed(InheritanceRelationshipsContext context)
        {
            var nrb1 = new NestedReferenceBase
            {
                Name = "NRB1"
            };
            var nrb2 = new NestedReferenceBase
            {
                Name = "NRB2"
            };
            var nrb3 = new NestedReferenceBase
            {
                Name = "NRB3"
            };
            var nrb4 = new NestedReferenceBase
            {
                Name = "NRB4 (dangling)"
            };

            context.NestedReferences.AddRange(nrb1, nrb2, nrb3, nrb4);

            var nrd1 = new NestedReferenceDerived
            {
                Name = "NRD1"
            };
            var nrd2 = new NestedReferenceDerived
            {
                Name = "NRD2"
            };
            var nrd3 = new NestedReferenceDerived
            {
                Name = "NRD3"
            };
            var nrd4 = new NestedReferenceDerived
            {
                Name = "NRD4"
            };
            var nrd5 = new NestedReferenceDerived
            {
                Name = "NRD4 (dangling)"
            };

            context.NestedReferences.AddRange(nrd1, nrd2, nrd3, nrd5);

            var ncb11 = new NestedCollectionBase
            {
                Name = "NCB11"
            };
            var ncb21 = new NestedCollectionBase
            {
                Name = "NCB21"
            };
            var ncb22 = new NestedCollectionBase
            {
                Name = "NCB22"
            };
            var ncb31 = new NestedCollectionBase
            {
                Name = "NCB31"
            };
            var ncb41 = new NestedCollectionBase
            {
                Name = "NCB41 (dangling)"
            };

            context.NestedCollections.AddRange(ncb11, ncb21, ncb22, ncb31, ncb41);

            var ncd11 = new NestedCollectionDerived
            {
                Name = "NCD11"
            };
            var ncd21 = new NestedCollectionDerived
            {
                Name = "NCD21"
            };
            var ncd31 = new NestedCollectionDerived
            {
                Name = "NCD21"
            };
            var ncd32 = new NestedCollectionDerived
            {
                Name = "NCD32"
            };
            var ncd41 = new NestedCollectionDerived
            {
                Name = "NCD41"
            };
            var ncd42 = new NestedCollectionDerived
            {
                Name = "NCD42"
            };
            var ncd51 = new NestedCollectionDerived
            {
                Name = "NCD52 (dangling)"
            };
            var ncd52 = new NestedCollectionDerived
            {
                Name = "NCD52 (dangling)"
            };

            context.NestedCollections.AddRange(ncd11, ncd21, ncd31, ncd32, ncd41, ncd42, ncd51, ncd52);

            var brob1 = new BaseReferenceOnBase
            {
                Name = "BROB1",
                NestedReference = nrb1,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncb11
                }
            };
            var brob2 = new BaseReferenceOnBase
            {
                Name = "BROB2",
                NestedReference = nrd1,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncd11
                }
            };
            var brob3 = new BaseReferenceOnBase
            {
                Name = "BROB3 (dangling)"
            };

            context.BaseReferencesOnBase.AddRange(brob1, brob2, brob3);

            var drob1 = new DerivedReferenceOnBase
            {
                Name = "DROB1",
                NestedReference = nrb2,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncb21,
                    ncb22
                }
            };
            var drob2 = new DerivedReferenceOnBase
            {
                Name = "DROB2",
                NestedReference = nrd2,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncd21
                }
            };
            var drob3 = new DerivedReferenceOnBase
            {
                Name = "DROB3"
            };
            var drob4 = new DerivedReferenceOnBase
            {
                Name = "DROB4 (half-dangling)",
                NestedReference = nrd3,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncd31,
                    ncd32
                }
            };
            var drob5 = new DerivedReferenceOnBase
            {
                Name = "DROB5 (dangling)"
            };

            context.BaseReferencesOnBase.AddRange(drob1, drob2, drob3, drob4, drob5);

            var rob1 = new ReferenceOnBase
            {
                Name = "ROB1"
            };
            var rob2 = new ReferenceOnBase
            {
                Name = "ROB2"
            };
            var rob3 = new ReferenceOnBase
            {
                Name = "ROB3"
            };
            var rob4 = new ReferenceOnBase
            {
                Name = "ROB4"
            };

            context.ReferencesOnBase.AddRange(rob1, rob2, rob3, rob4);

            var bcob11 = new BaseCollectionOnBase
            {
                Name = "BCOB11",
                NestedReference = nrb1,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncb11
                }
            };
            var bcob12 = new BaseCollectionOnBase
            {
                Name = "BCOB12",
                NestedReference = nrd1,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncd11
                }
            };
            var bcob21 = new BaseCollectionOnBase
            {
                Name = "BCOB21"
            };
            var bcob31 = new BaseCollectionOnBase
            {
                Name = "BCOB31 (dangling)",
                NestedReference = nrb2,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncb21,
                    ncb22
                }
            };
            var bcob32 = new BaseCollectionOnBase
            {
                Name = "BCOB32 (dangling)"
            };

            context.BaseCollectionsOnBase.AddRange(bcob11, bcob12, bcob21, bcob31, bcob32);

            var dcob11 = new DerivedCollectionOnBase
            {
                Name = "DCOB11",
                NestedReference = nrd2,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncd21
                },
                DerivedProperty = 1
            };
            var dcob12 = new DerivedCollectionOnBase
            {
                Name = "DCOB12",
                NestedReference = nrb3,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncb31
                },
                DerivedProperty = 2
            };
            var dcob21 = new DerivedCollectionOnBase
            {
                Name = "DCOB21",
                DerivedProperty = 3
            };
            var dcob31 = new DerivedCollectionOnBase
            {
                Name = "DCOB31",
                NestedReference = nrd3,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncd31,
                    ncd32
                },
                DerivedProperty = 4
            };
            var dcob32 = new DerivedCollectionOnBase
            {
                Name = "DCOB32",
                DerivedProperty = 5
            };
            var dcob41 = new DerivedCollectionOnBase
            {
                Name = "DCOB41",
                DerivedProperty = 6
            };
            var dcob51 = new DerivedCollectionOnBase
            {
                Name = "DCOB51 (dangling)",
                NestedReference = nrd4,
                NestedCollection = new List<NestedCollectionBase>
                {
                    ncd41,
                    ncd42
                },
                DerivedProperty = 7
            };
            var dcob52 = new DerivedCollectionOnBase
            {
                Name = "DCOB52 (dangling)",
                DerivedProperty = 8
            };

            context.BaseCollectionsOnBase.AddRange(dcob11, dcob12, dcob21, dcob31, dcob32, dcob41, dcob51, dcob52);

            var cob11 = new CollectionOnBase
            {
                Name = "COB11"
            };
            var cob12 = new CollectionOnBase
            {
                Name = "COB12"
            };
            var cob21 = new CollectionOnBase
            {
                Name = "COB21"
            };
            var cob31 = new CollectionOnBase
            {
                Name = "COB31"
            };
            var cob32 = new CollectionOnBase
            {
                Name = "COB32"
            };
            var cob33 = new CollectionOnBase
            {
                Name = "COB33"
            };
            var cob41 = new CollectionOnBase
            {
                Name = "COB41"
            };
            var cob51 = new CollectionOnBase
            {
                Name = "COB51 (dangling)"
            };
            var cob52 = new CollectionOnBase
            {
                Name = "COB52 (dangling)"
            };

            context.CollectionsOnBase.AddRange(cob11, cob12, cob21, cob31, cob32, cob33, cob41, cob51, cob52);

            var brod1 = new BaseReferenceOnDerived
            {
                Name = "BROD1"
            };
            var brod2 = new BaseReferenceOnDerived
            {
                Name = "BROD2 (dangling)"
            };
            var brod3 = new BaseReferenceOnDerived
            {
                Name = "BROD3 (dangling)"
            };

            context.BaseReferencesOnDerived.AddRange(brod1, brod2, brod3);

            var drod1 = new DerivedReferenceOnDerived
            {
                Name = "DROD1"
            };
            var drod2 = new DerivedReferenceOnDerived
            {
                Name = "DROD2"
            };
            var drod3 = new DerivedReferenceOnDerived
            {
                Name = "DROD3 (dangling)"
            };

            context.BaseReferencesOnDerived.AddRange(drod1, drod2, drod3);

            var rod1 = new ReferenceOnDerived
            {
                Name = "ROD1"
            };
            var rod2 = new ReferenceOnDerived
            {
                Name = "ROD2"
            };
            var rod3 = new ReferenceOnDerived
            {
                Name = "ROD3 (dangling)"
            };

            context.ReferencesOnDerived.AddRange(rod1, rod2, rod3);

            var bcod11 = new BaseCollectionOnDerived
            {
                Name = "BCOD11"
            };
            var bcod21 = new BaseCollectionOnDerived
            {
                Name = "BCOD21 (dangling)"
            };
            var bcod22 = new BaseCollectionOnDerived
            {
                Name = "BCOD22 (dangling)"
            };

            context.BaseCollectionsOnDerived.AddRange(bcod11, bcod21, bcod22);

            var dcod11 = new DerivedCollectionOnDerived
            {
                Name = "DCOD11"
            };
            var dcod12 = new DerivedCollectionOnDerived
            {
                Name = "DCOD12"
            };
            var dcod21 = new DerivedCollectionOnDerived
            {
                Name = "DCOD21"
            };
            var dcod31 = new DerivedCollectionOnDerived
            {
                Name = "DCOD31 (dangling)"
            };

            context.BaseCollectionsOnDerived.AddRange(dcod11, dcod12, dcod21, dcod31);

            var cod11 = new CollectionOnDerived
            {
                Name = "COD11"
            };
            var cod21 = new CollectionOnDerived
            {
                Name = "COD21"
            };
            var cod22 = new CollectionOnDerived
            {
                Name = "COD22"
            };
            var cod31 = new CollectionOnDerived
            {
                Name = "COD31 (dangling)"
            };

            context.CollectionsOnDerived.AddRange(cod11, cod21, cod22, cod31);

            var baseEntity1 = new BaseInheritanceRelationshipEntity
            {
                Name = "Base1",
                BaseReferenceOnBase = brob1,
                ReferenceOnBase = rob1,
                BaseCollectionOnBase = new List<BaseCollectionOnBase>
                {
                    bcob11
                },
                CollectionOnBase = new List<CollectionOnBase>
                {
                    cob11,
                    cob12
                }
            };

            var baseEntity2 = new BaseInheritanceRelationshipEntity
            {
                Name = "Base2",
                BaseReferenceOnBase = drob2,
                ReferenceOnBase = rob2,
                CollectionOnBase = new List<CollectionOnBase>
                {
                    cob21
                }
            };

            var baseEntity3 = new BaseInheritanceRelationshipEntity
            {
                Name = "Base3",
                BaseCollectionOnBase = new List<BaseCollectionOnBase>
                {
                    dcob21
                }
            };

            context.BaseEntities.AddRange(baseEntity1, baseEntity2, baseEntity3);

            var derivedEntity1 = new DerivedInheritanceRelationshipEntity
            {
                Name = "Derived1(4)",
                BaseSelfRerefenceOnDerived = baseEntity1,
                BaseReferenceOnBase = drob1,
                ReferenceOnBase = rob3,
                BaseCollectionOnBase = new List<BaseCollectionOnBase>
                {
                    dcob11,
                    dcob12
                },
                CollectionOnBase = new List<CollectionOnBase>
                {
                    cob31,
                    cob32
                },
                BaseReferenceOnDerived = brod1,
                DerivedReferenceOnDerived = drod1,
                ReferenceOnDerived = rod1,
                BaseCollectionOnDerived = new List<BaseCollectionOnDerived>
                {
                    bcod11
                },
                DerivedCollectionOnDerived = new List<DerivedCollectionOnDerived>
                {
                    dcod11,
                    dcod12
                },
                CollectionOnDerived = new List<CollectionOnDerived>
                {
                    cod11
                }
            };

            var derivedEntity2 = new DerivedInheritanceRelationshipEntity
            {
                Name = "Derived2(5)",
                BaseSelfRerefenceOnDerived = baseEntity2,
                ReferenceOnBase = rob4,
                BaseReferenceOnBase = brob2,
                CollectionOnBase = new List<CollectionOnBase>
                {
                    cob41
                },
                BaseReferenceOnDerived = drod2,
                ReferenceOnDerived = rod2,
                CollectionOnDerived = new List<CollectionOnDerived>
                {
                    cod21,
                    cod22
                }
            };

            var derivedEntity3 = new DerivedInheritanceRelationshipEntity
            {
                Name = "Derived3(6)",
                BaseSelfRerefenceOnDerived = baseEntity3,
                BaseCollectionOnBase = new List<BaseCollectionOnBase>
                {
                    bcob21
                },
                DerivedReferenceOnDerived = drod2,
                BaseCollectionOnDerived = new List<BaseCollectionOnDerived>
                {
                    dcod11,
                    dcod12
                },
                DerivedCollectionOnDerived = new List<DerivedCollectionOnDerived>
                {
                    dcod21
                }
            };

            context.BaseEntities.AddRange(derivedEntity1, derivedEntity2, derivedEntity3);

            baseEntity1.DerivedSefReferenceOnBase = derivedEntity1;
            baseEntity2.DerivedSefReferenceOnBase = derivedEntity2;
            baseEntity3.DerivedSefReferenceOnBase = derivedEntity3;

            var principalEntity1 = new PrincipalEntity
            {
                Name = "PE1"
            };
            var principalEntity2 = new PrincipalEntity
            {
                Name = "PE2"
            };

            context.PrincipalEntities.AddRange(principalEntity1, principalEntity2);

            var referencedEntity1 = new ReferencedEntity
            {
                Name = "RE1",
                Principals = new List<PrincipalEntity>
                {
                    principalEntity1
                }
            };
            var referencedEntity2 = new ReferencedEntity
            {
                Name = "RE2",
                Principals = new List<PrincipalEntity>
                {
                    principalEntity2
                }
            };

            context.ReferencedEntities.AddRange(referencedEntity1, referencedEntity2);

            principalEntity2.Reference = referencedEntity1;
            principalEntity1.Reference = referencedEntity2;

            context.SaveChanges();
        }
    }
}
