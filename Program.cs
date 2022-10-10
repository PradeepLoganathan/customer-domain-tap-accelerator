using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CustomerDb>(opt => opt.UseInMemoryDatabase("CustomerTable"));
var app = builder.Build();

app.MapGet("/", () => {
});

app.MapGet("/customers", async (CustomerDb db) =>
    await db.Customers.ToListAsync());

app.MapGet("/customers/premier", async (CustomerDb db) =>
    await db.Customers.Where(t => t.IsPremier).ToListAsync());

app.MapGet("/customers/{id}", async (int id, CustomerDb db) =>
    await db.Customers.FindAsync(id)
        is Customer customer
            ? Results.Ok(customer)
            : Results.NotFound());

app.MapPost("/customers", async (Customer customer, CustomerDb db) =>
{
    db.Customers.Add(customer);
    await db.SaveChangesAsync();

    return Results.Created($"/customers/{customer.Id}", customer);
});

app.MapPut("/customers/{id}", async (int id, Customer inputCustomer, CustomerDb db) =>
{
    var customer = await db.Customers.FindAsync(id);

    if (customer is null) return Results.NotFound();

    customer.Name = inputCustomer.Name;
    customer.IsPremier = inputCustomer.IsPremier;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/customers/{id}", async (int id, CustomerDb db) =>
{
    if (await db.Customers.FindAsync(id) is Customer customer)
    {
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
        return Results.Ok(customer);
    }

    return Results.NotFound();
});

app.Run();

class Customer
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsPremier { get; set; }
}

class CustomerDb : DbContext
{
    public CustomerDb(DbContextOptions<CustomerDb> options)
        : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
}