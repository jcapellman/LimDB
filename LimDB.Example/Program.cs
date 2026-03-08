using LimDB.lib;
using LimDB.Example.Models;

Console.WriteLine("=== LimDB Example Application ===\n");

var dbFileName = "products.json";

// Create initial sample data if the file doesn't exist
if (!File.Exists(dbFileName))
{
    Console.WriteLine("Creating initial database file...");
    await File.WriteAllTextAsync(dbFileName, "[]");
}

// Create the LimDB context
var db = await LimDbContext<Product>.CreateFromLocalStorageSourceAsync(dbFileName);

Console.WriteLine("Database loaded successfully!\n");

// INSERT - Add new products
Console.WriteLine("--- INSERT Operations ---");
var newProduct1 = new Product { Name = "Laptop", Price = 999.99m, Category = "Electronics" };
var id1 = await db.InsertAsync(newProduct1);
Console.WriteLine($"Inserted Product: {newProduct1.Name} with ID: {id1}");

var newProduct2 = new Product { Name = "Mouse", Price = 29.99m, Category = "Electronics" };
var id2 = await db.InsertAsync(newProduct2);
Console.WriteLine($"Inserted Product: {newProduct2.Name} with ID: {id2}");

var newProduct3 = new Product { Name = "Desk Chair", Price = 199.99m, Category = "Furniture" };
var id3 = await db.InsertAsync(newProduct3);
Console.WriteLine($"Inserted Product: {newProduct3.Name} with ID: {id3}\n");

// READ - Get all products
Console.WriteLine("--- READ Operations ---");
Console.WriteLine("All Products:");
var allProducts = db.GetMany();
if (allProducts != null)
{
    foreach (var product in allProducts)
    {
        Console.WriteLine($"  ID: {product.Id}, Name: {product.Name}, Price: ${product.Price:F2}, Category: {product.Category}");
    }
}
Console.WriteLine();

// READ - Get specific product by ID
Console.WriteLine($"Get Product by ID ({id1}):");
var productById = db.GetOneById(id1);
if (productById != null)
{
    Console.WriteLine($"  Found: {productById.Name} - ${productById.Price:F2}\n");
}

// READ - Filter products
Console.WriteLine("Electronics Products (Price > $50):");
var electronics = db.GetMany(p => p.Category == "Electronics" && p.Price > 50);
if (electronics != null)
{
    foreach (var product in electronics)
    {
        Console.WriteLine($"  {product.Name} - ${product.Price:F2}");
    }
}
Console.WriteLine();

// UPDATE - Modify a product
Console.WriteLine("--- UPDATE Operations ---");
var productToUpdate = db.GetOneById(id2);
if (productToUpdate != null)
{
    Console.WriteLine($"Updating {productToUpdate.Name} price from ${productToUpdate.Price:F2} to $24.99");
    productToUpdate.Price = 24.99m;
    var updateResult = await db.UpdateAsync(productToUpdate);
    Console.WriteLine($"Update successful: {updateResult}\n");
}

// Verify update
var updatedProduct = db.GetOneById(id2);
Console.WriteLine($"Verified: {updatedProduct?.Name} now costs ${updatedProduct?.Price:F2}\n");

// DELETE - Remove a product
Console.WriteLine("--- DELETE Operations ---");
Console.WriteLine($"Deleting product with ID: {id3}");
var deleteResult = await db.DeleteByIdAsync(id3);
Console.WriteLine($"Delete successful: {deleteResult}\n");

// Final state
Console.WriteLine("--- Final Database State ---");
var finalProducts = db.GetMany();
Console.WriteLine($"Total products remaining: {finalProducts?.Count()}");
if (finalProducts != null)
{
    foreach (var product in finalProducts)
    {
        Console.WriteLine($"  {product.Name} - ${product.Price:F2} ({product.Category})");
    }
}

Console.WriteLine("\n=== Example Complete ===");
Console.WriteLine($"Database saved to: {Path.GetFullPath(dbFileName)}");
