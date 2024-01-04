// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class LazyLoadProxyInMemoryTest(LazyLoadProxyInMemoryTest.LoadInMemoryFixture fixture) : LazyLoadProxyTestBase<LazyLoadProxyInMemoryTest.LoadInMemoryFixture>(fixture)
{
    protected override string SerializedBlogs2
        => """
{
  "$id": "1",
  "$values": [
    {
      "$id": "2",
      "Id": 1,
      "Writer": {
        "$id": "3",
        "FirstName": "firstNameWriter0",
        "LastName": "lastNameWriter0",
        "Alive": false,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Reader": {
        "$id": "4",
        "FirstName": "firstNameReader0",
        "LastName": "lastNameReader0",
        "Alive": false,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Host": {
        "$id": "5",
        "HostName": "127.0.0.1",
        "Rating": 0,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null
    },
    {
      "$id": "6",
      "Id": 2,
      "Writer": {
        "$id": "7",
        "FirstName": "firstNameWriter1",
        "LastName": "lastNameWriter1",
        "Alive": false,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Reader": {
        "$id": "8",
        "FirstName": "firstNameReader1",
        "LastName": "lastNameReader1",
        "Alive": false,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Host": {
        "$id": "9",
        "HostName": "127.0.0.2",
        "Rating": 0,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null
    },
    {
      "$id": "10",
      "Id": 3,
      "Writer": {
        "$id": "11",
        "FirstName": "firstNameWriter2",
        "LastName": "lastNameWriter2",
        "Alive": false,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Reader": {
        "$id": "12",
        "FirstName": "firstNameReader2",
        "LastName": "lastNameReader2",
        "Alive": false,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Host": {
        "$id": "13",
        "HostName": "127.0.0.3",
        "Rating": 0,
        "Culture": {
          "Species": null,
          "Subspecies": null,
          "Rating": 0,
          "Validation": null,
          "Manufacturer": null,
          "License": {
            "Title": null,
            "Charge": 0,
            "Tag": null,
            "Tog": {
              "Text": null
            }
          }
        },
        "Milk": null
      },
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null
    }
  ]
}
""";

    protected override string SerializedBlogs1
        => """
[
  {
    "Writer": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "FirstName": "firstNameWriter0",
      "LastName": "lastNameWriter0",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "FirstName": "firstNameReader0",
      "LastName": "lastNameReader0",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "HostName": "127.0.0.1",
      "Rating": 0.0
    },
    "Culture": {
      "Species": null,
      "Subspecies": null,
      "Rating": 0,
      "Validation": null,
      "Manufacturer": null,
      "License": {
        "Title": null,
        "Charge": 0.0,
        "Tag": null,
        "Tog": {
          "Text": null
        }
      }
    },
    "Milk": null,
    "Id": 1
  },
  {
    "Writer": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "FirstName": "firstNameWriter1",
      "LastName": "lastNameWriter1",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "FirstName": "firstNameReader1",
      "LastName": "lastNameReader1",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "HostName": "127.0.0.2",
      "Rating": 0.0
    },
    "Culture": {
      "Species": null,
      "Subspecies": null,
      "Rating": 0,
      "Validation": null,
      "Manufacturer": null,
      "License": {
        "Title": null,
        "Charge": 0.0,
        "Tag": null,
        "Tog": {
          "Text": null
        }
      }
    },
    "Milk": null,
    "Id": 2
  },
  {
    "Writer": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "FirstName": "firstNameWriter2",
      "LastName": "lastNameWriter2",
      "Alive": false
    },
    "Reader": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "FirstName": "firstNameReader2",
      "LastName": "lastNameReader2",
      "Alive": false
    },
    "Host": {
      "Culture": {
        "Species": null,
        "Subspecies": null,
        "Rating": 0,
        "Validation": null,
        "Manufacturer": null,
        "License": {
          "Title": null,
          "Charge": 0.0,
          "Tag": null,
          "Tog": {
            "Text": null
          }
        }
      },
      "Milk": null,
      "HostName": "127.0.0.3",
      "Rating": 0.0
    },
    "Culture": {
      "Species": null,
      "Subspecies": null,
      "Rating": 0,
      "Validation": null,
      "Manufacturer": null,
      "License": {
        "Title": null,
        "Charge": 0.0,
        "Tag": null,
        "Tog": {
          "Text": null
        }
      }
    },
    "Milk": null,
    "Id": 3
  }
]
""";

    public class LoadInMemoryFixture : LoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Called>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Quest>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Entity>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Company>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Parson>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<SingleShadowFk>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Mother>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Father>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Address>(
                builder =>
                {
                    builder.Ignore(e => e.Milk);
                    builder.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Pyrson>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<NonVirtualOneToOneOwner>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<VirtualOneToOneOwner>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<NonVirtualParent>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Blog>(
                e =>
                {
                    e.Ignore(e => e.Milk);
                    e.Ignore(e => e.Culture);
                    e.OwnsOne(
                        x => x.Writer, b =>
                        {
                            b.Ignore(e => e.Milk);
                            b.Ignore(e => e.Culture);
                        });
                    e.OwnsOne(
                        x => x.Reader, b =>
                        {
                            b.Ignore(e => e.Milk);
                            b.Ignore(e => e.Culture);
                        });
                    e.OwnsOne(
                        x => x.Host, b =>
                        {
                            b.Ignore(e => e.Milk);
                            b.Ignore(e => e.Culture);
                        });
                });

            modelBuilder.Entity<Single>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<VirtualOneToManyOwner>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<NonVirtualOneToManyOwner>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<ExplicitLazyLoadVirtualOneToManyOwner>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<ExplicitLazyLoadNonVirtualOneToManyOwner>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Child>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<NonVirtualChild>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<ChildAk>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<ChildShadowFk>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<ChildCompositeKey>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Single>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<SinglePkToPk>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<SingleShadowFk>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<SingleAk>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<SingleCompositeKey>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<Nose>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });

            modelBuilder.Entity<NonVirtualParent>(
                b =>
                {
                    b.Ignore(e => e.Milk);
                    b.Ignore(e => e.Culture);
                });
        }
    }
}
