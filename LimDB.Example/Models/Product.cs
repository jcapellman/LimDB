using LimDB.lib.Objects.Base;

namespace LimDB.Example.Models;

public class Product : BaseObject
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
}
