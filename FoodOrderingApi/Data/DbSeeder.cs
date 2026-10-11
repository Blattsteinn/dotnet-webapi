using Microsoft.EntityFrameworkCore;

using FoodOrderingApi.Models.Places;

namespace FoodOrderingApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        // The demo catalogue is written once, into an empty database.
        if (!db.Places.Any())
            SeedCatalogue(db);

        // Opening hours (and the 24h place that belongs with them) are filled in
        // whenever they are missing, so a database created before this feature
        // still ends up with the test fixtures. Gaps are only ever filled: an
        // owner's later edits are never overwritten.
        SeedFixtures(db);
    }

    // ── Fixtures: opening hours and visibility ─────────────────────────────
    //
    // Four demo places, each with one job:
    //   Pizza Palace   — a normal week plus an overnight leg (a place that has
    //                    a schedule to look at)
    //   Burger Joint   — no rows at all, so it is permanently closed (proves
    //                    "no hours means closed")
    //   All Night Kebab— 24h every day, so it is always open and can be ordered
    //                    from at any hour. The Development owner (Program.cs)
    //                    owns it, so the /owner routes have something to edit.
    //                    Its menu also carries the visibility cases: a hidden
    //                    product inside a visible category, and a hidden
    //                    category.
    //
    // "OpeningHours.http" and "Products.http" drive these fixtures.
    private static void SeedFixtures(AppDbContext db)
    {
        Place? pizzaPalace = db.Places
            .Include(p => p.OpeningHours)
            .FirstOrDefault(p => p.Name == "Pizza Palace");

        if (pizzaPalace != null && pizzaPalace.OpeningHours.Count == 0)
        {
            pizzaPalace.OpeningHours.AddRange(
            [
                Hour(DayOfWeek.Monday,    11, 0, 22, 0),
                Hour(DayOfWeek.Tuesday,   11, 0, 22, 0),
                Hour(DayOfWeek.Wednesday, 11, 0, 22, 0),
                Hour(DayOfWeek.Thursday,  11, 0, 22, 0),
                // Friday and Saturday run past midnight: the row belongs to the
                // day it opens.
                Hour(DayOfWeek.Friday,    11, 0,  2, 0),
                Hour(DayOfWeek.Saturday,  12, 0,  2, 0),
                Hour(DayOfWeek.Sunday,    12, 0, 21, 0),
            ]);
        }

        // Burger Joint is deliberately left without hours.

        Place? kebab = db.Places
            .Include(p => p.OpeningHours)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
            .FirstOrDefault(p => p.Name == "All Night Kebab");

        if (kebab == null)
        {
            kebab = BuildAllNightKebab();
            db.Places.Add(kebab);
        }
        else if (kebab.OpeningHours.Count == 0)
        {
            kebab.OpeningHours.AddRange(Open24HoursEveryDay());
        }

        EnsureVisibilityFixtures(kebab);

        db.SaveChanges();
    }

    // The menu cases the public product endpoints have to filter. Filled in
    // per item, so an existing place picks them up without its menu being
    // rewritten.
    private static void EnsureVisibilityFixtures(Place kebab)
    {
        ProductCategory? kebabs =
            kebab.ProductCategories.FirstOrDefault(c => c.Name == "Kebabs");

        if (kebabs == null)
        {
            kebabs = new ProductCategory { Name = "Kebabs", Hidden = false };
            kebab.ProductCategories.Add(kebabs);
        }

        // Visible product — the one the public endpoints must still serve.
        if (kebabs.Products.All(p => p.Name != "Kebab"))
        {
            kebabs.Products.Add(new Product
            {
                Name = "Kebab",
                Description = "Your 3am friend",
                Price = 6.50m,
                Hidden = false,
                ProductCategory = kebabs
            });
        }

        // Hidden product inside a visible category: absent from the public
        // product endpoints, present in the owner/admin read model.
        if (kebabs.Products.All(p => p.Name != "Off-menu Special"))
        {
            kebabs.Products.Add(new Product
            {
                Name = "Off-menu Special",
                Description = "Hidden product — must not appear publicly",
                Price = 5.00m,
                Hidden = true,
                ProductCategory = kebabs
            });
        }

        // Hidden category: its products are unreachable publicly, so the
        // category id answers 404.
        if (kebab.ProductCategories.All(c => c.Name != "Staff Only"))
        {
            var staffOnly = new ProductCategory { Name = "Staff Only", Hidden = true };
            staffOnly.Products.Add(new Product
            {
                Name = "Chef's Special",
                Description = "Lives under a hidden category",
                Price = 4.00m,
                Hidden = false,
                ProductCategory = staffOnly
            });
            kebab.ProductCategories.Add(staffOnly);
        }
    }

    private static Place BuildAllNightKebab()
    {
        var place = new Place
        {
            Name = "All Night Kebab",
            Latitude = 54.6872,
            Longtitude = 25.2797,
            Description = "Open 24 hours — the always-open ordering fixture",
            Address = "3 Kebab Lane, Vilnius"
        };

        place.OpeningHours.AddRange(Open24HoursEveryDay());

        return place;
    }

    // 00:00-00:00 is the "open 24 hours" interval, one row per day.
    private static IEnumerable<PlaceOpeningHour> Open24HoursEveryDay() =>
        Enum.GetValues<DayOfWeek>().Select(day => Hour(day, 0, 0, 0, 0));

    private static PlaceOpeningHour Hour(
        DayOfWeek day, int opensHour, int opensMinute, int closesHour, int closesMinute) =>
        new()
        {
            Day = day,
            OpensAt = new TimeOnly(opensHour, opensMinute),
            ClosesAt = new TimeOnly(closesHour, closesMinute)
        };

    private static void SeedCatalogue(AppDbContext db)
    {
        // ── Place 1: Pizza Palace ──────────────────────────────────────────
        var pizzaPalace = new Place
        {
            Name = "Pizza Palace",
            Latitude = 40.7128,
            Longtitude = -74.0060,
            Description = "Best pizza in town",
            Address = "1 Pizza Plaza, New York, NY"
        };

        var pizzasCategory = new ProductCategory
        {
            Name = "Pizzas",
            Hidden = false,
            Place = pizzaPalace
        };
        var drinksCategory = new ProductCategory
        {
            Name = "Drinks",
            Hidden = false,
            Place = pizzaPalace
        };
        var sidesCategory = new ProductCategory
        {
            Name = "Sides",
            Hidden = false,
            Place = pizzaPalace
        };

        pizzaPalace.ProductCategories.Add(pizzasCategory);
        pizzaPalace.ProductCategories.Add(drinksCategory);
        pizzaPalace.ProductCategories.Add(sidesCategory);

        // Products under Pizzas
        var margherita = new Product
        {
            Name = "Margherita",
            Description = "Tomato and cheese",
            Price = 9.99m,
            Hidden = false,
            ProductCategory = pizzasCategory
        };
        var pepperoni = new Product
        {
            Name = "Pepperoni",
            Description = "Pepperoni and cheese",
            Price = 11.99m,
            Hidden = false,
            ProductCategory = pizzasCategory
        };
        var vegetarian = new Product
        {
            Name = "Vegetarian",
            Description = "Veggies and cheese",
            Price = 10.99m,
            Hidden = false,
            ProductCategory = pizzasCategory
        };

        pizzasCategory.Products.Add(margherita);
        pizzasCategory.Products.Add(pepperoni);
        pizzasCategory.Products.Add(vegetarian);

        // AddOnCategories for Margherita
        var extraToppings = new AddOnCategory
        {
            Name = "Extra Toppings",
            Multichoice = true,
            Required = false,
            Product = margherita
        };
        var crustType = new AddOnCategory
        {
            Name = "Crust Type",
            Multichoice = false,
            Required = true,
            Product = margherita
        };

        margherita.AddOnCategories.Add(extraToppings);
        margherita.AddOnCategories.Add(crustType);

        // AddOns for Extra Toppings
        extraToppings.AddOns.Add(new AddOn
        {
            Name = "Extra Cheese",
            Price = 1.50m,
            Hidden = false,
            AddOnCategory = extraToppings
        });
        extraToppings.AddOns.Add(new AddOn
        {
            Name = "Mushrooms",
            Price = 1.00m,
            Hidden = false,
            AddOnCategory = extraToppings
        });
        extraToppings.AddOns.Add(new AddOn
        {
            Name = "Olives",
            Price = 0.75m,
            Hidden = false,
            AddOnCategory = extraToppings
        });

        // AddOns for Crust Type
        crustType.AddOns.Add(new AddOn
        {
            Name = "Regular",
            Price = 0.00m,
            Hidden = false,
            AddOnCategory = crustType
        });
        crustType.AddOns.Add(new AddOn
        {
            Name = "Thin",
            Price = 1.00m,
            Hidden = false,
            AddOnCategory = crustType
        });
        crustType.AddOns.Add(new AddOn
        {
            Name = "Thick",
            Price = 1.50m,
            Hidden = false,
            AddOnCategory = crustType
        });

        // Products under Drinks
        var coke = new Product
        {
            Name = "Coke",
            Description = "Coca-Cola",
            Price = 2.00m,
            Hidden = false,
            ProductCategory = drinksCategory
        };
        var water = new Product
        {
            Name = "Water",
            Description = "Bottled water",
            Price = 1.50m,
            Hidden = false,
            ProductCategory = drinksCategory
        };
        drinksCategory.Products.Add(coke);
        drinksCategory.Products.Add(water);

        // Products under Sides
        var fries = new Product
        {
            Name = "Fries",
            Description = "French fries",
            Price = 3.00m,
            Hidden = false,
            ProductCategory = sidesCategory
        };
        var garlicBread = new Product
        {
            Name = "Garlic Bread",
            Description = "Toasted bread with garlic",
            Price = 4.00m,
            Hidden = false,
            ProductCategory = sidesCategory
        };
        sidesCategory.Products.Add(fries);
        sidesCategory.Products.Add(garlicBread);

        // ── Place 2: Burger Joint ──────────────────────────────────────────
        var burgerJoint = new Place
        {
            Name = "Burger Joint",
            Latitude = 34.0522,
            Longtitude = -118.2437,
            Description = "Best burgers in town",
            Address = "2 Burger Boulevard, Los Angeles, CA"
        };

        var burgersCategory = new ProductCategory
        {
            Name = "Burgers",
            Hidden = false,
            Place = burgerJoint
        };
        burgerJoint.ProductCategories.Add(burgersCategory);

        var cheeseburger = new Product
        {
            Name = "Cheeseburger",
            Description = "Beef patty with cheese",
            Price = 8.99m,
            Hidden = false,
            ProductCategory = burgersCategory
        };
        burgersCategory.Products.Add(cheeseburger);

        // Add a simple AddOnCategory for Cheeseburger
        var burgerExtras = new AddOnCategory
        {
            Name = "Extras",
            Multichoice = true,
            Required = false,
            Product = cheeseburger
        };
        cheeseburger.AddOnCategories.Add(burgerExtras);

        burgerExtras.AddOns.Add(new AddOn
        {
            Name = "Bacon",
            Price = 1.50m,
            Hidden = false,
            AddOnCategory = burgerExtras
        });
        burgerExtras.AddOns.Add(new AddOn
        {
            Name = "Avocado",
            Price = 1.00m,
            Hidden = false,
            AddOnCategory = burgerExtras
        });

        // ── Persist to database ────────────────────────────────────────────
        db.Places.Add(pizzaPalace);
        db.Places.Add(burgerJoint);

        db.SaveChanges();
    }
}