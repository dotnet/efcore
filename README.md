# Database Seeder Pull Request for EFCore:
A new seeder for EFCore to easily seeding database

## Previous EFCore seeder:
EFCore has a seeder that can be found in [this link](https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding).
You have to write your seeds in ModelBuilder and seeds will be a part of your migrations what is not an interesting way for seeding the database.
Also that seeds won't increase the primary key and indexes of your tables so you have to increase them manually!!!

## New Seeder that implemented in this PR:
After my experience while using EFCore seeder, I decided to write a new implementation for EFCore seeder.
You can find my seeder in `Seeder` directory of EFCore source code.
Also a sample project this implementation is available in [this repo](https://github.com/AshkanAbd/efCoreSeederSample)

#### New seeder approach:
##### Define your seeders:
```c#
using System.Linq;
using efCoreSeederSample.Models;
using efCoreSeederSample.Seeder.Attributes;
using Microsoft.EntityFrameworkCore;

namespace efCoreSeederSample.Seed
{
    public class DatabaseSeed
    {
        public SeederSampleDbContext DbContext { get; set; }

        public DatabaseSeed(SeederSampleDbContext dbContext)
        {
            DbContext = dbContext;
        }

        [Seeder(typeof(Category), 1)]
        public void CategorySeeder()
        {
            for (var i = 1; i <= 3; i++) {
                DbContext.Categories.Add(new Category {
                    Name = $"Category {i}"
                });
            }

            DbContext.SaveChanges();
        }
    }
}
```
As you can see, In this new approach we can easily define a seeder method. Also there is a priority in `SeederAttribute` that determines order of seeder methods, this can helps you to seed your database as you defined your relations.
##### Run your seeders:
```c#
services.AddSeeder<SeederSampleDbContext>()
                .SetEnvironment(Configuration["Seeders:Environment"])
                .EnsureSeeded(bool.Parse(Configuration["Seeders:isEfProcess"]));
```

You should add this line of code to `ConfigureServices` method of `Startup` after registering you application `DbContext`.
After adding this line, in start of every run, seeder starts to seed your database.

### `SeederAttribute` Parameters:
#### `Type`:
This is type of the model you want seed.
The model should has a `DBSet<>` in your `DbContext`.
#### `Priority`:
The priority of the seeder.
Assume that because of relations you defined in your database, you can't insert data to table `A` before table `B`. Now for seeding your database you can set `Priority` of `BSeeder` to `1` and `Priority` of `ASeeder` to `2`. Now seeder will run `BSeeder` before `ASeeder` and you have seed in your both tables.
##### `Production`:
When this parameter is `true` means this seeder should only run on your production for seeding your database in production environment. And if the parameter is `false`, the seeder will only run on development environment.
#### `Force`:
Seeder automatically checks your database's tables. If the model's table has some data in it, seeder won't run to prevent duplicating data. But with setting `Force` Parameter to `true`, you can force seeder to insert data again.

## Purpose:
I think this implementation is good start point for a new seeder in EFCore and it's better than previous seeder of EFCore.
I hope you like it.
