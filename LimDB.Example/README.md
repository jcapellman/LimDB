# LimDB Example Application

A simple console application demonstrating LimDB's core CRUD operations.

## What This Example Demonstrates

- **Creating a database context** from a local JSON file
- **INSERT**: Adding new items to the database
- **READ**: Querying all items, finding by ID, and filtering with expressions
- **UPDATE**: Modifying existing items
- **DELETE**: Removing items from the database

## Running the Example

```bash
cd LimDB.Example
dotnet run
```

The application will:
1. Create a `products.json` database file (if it doesn't exist)
2. Insert sample products
3. Query and filter products
4. Update product prices
5. Delete a product
6. Display the final database state

## Key Concepts

### Creating a Context
```csharp
var db = await LimDbContext<Product>.CreateFromLocalStorageSourceAsync("products.json");
```

### Insert
```csharp
var newProduct = new Product { Name = "Laptop", Price = 999.99m };
var id = await db.InsertAsync(newProduct);
```

### Read
```csharp
// Get all
var all = db.GetMany();

// Get by ID (O(1) performance)
var product = db.GetOneById(id);

// Filter with expression
var filtered = db.GetMany(p => p.Price > 100);
```

### Update
```csharp
product.Price = 24.99m;
await db.UpdateAsync(product);
```

### Delete
```csharp
await db.DeleteByIdAsync(id);
```

## Custom Models

All models must inherit from `BaseObject`:

```csharp
public class Product : BaseObject
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
}
```

`BaseObject` provides:
- `Id` - Unique identifier (auto-generated)
- `Active` - Status flag
- `Created` - Timestamp (auto-generated)
- `Modified` - Timestamp (auto-updated)
