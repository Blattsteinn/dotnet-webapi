using FoodOrderingApi.Models.Places;

namespace FoodOrderingApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Places.Any())
            return;

        // ── Place 1: Pizza Palace ──────────────────────────────────────────
        var pizzaPalace = new Place
        {
            Name = "Pizza Palace",
            Latitude = 40.7128,
            Longtitude = -74.0060,
            Description = "Best pizza in town"
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
            Description = "Best burgers in town"
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