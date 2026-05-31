# QueryPack.RestApi

Automatically generates a full CRUD REST API from an EF Core `DbContext`. Every `DbSet<T>` in your context gets its own set of endpoints — filtering, ordering, eager loading, and pagination included — with no controller code required.

## Installation

```
dotnet add package QueryPack.RestApi
```

## Quick start (code-first)

### 1. Define your models and context

```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public Category Category { get; set; }
}

public class Category
{
    public Guid Id { get; set; }
    public string Title { get; set; }
}

public class StoreContext : DbContext
{
    public StoreContext(DbContextOptions<StoreContext> options) : base(options) { }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
}
```

Every `DbSet<T>` is automatically exposed. No attributes required.

### 2. Register the service

```csharp
builder.Services.AddRestModel<StoreContext>(options =>
{
    options.GlobalApiPrefix = "/api";
    options.ContextOptionsBuilder = (sp, db) =>
        db.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    options.SerializerOptions = o =>
    {
        o.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    };
});
```

### 3. Add Swagger (optional)

```csharp
builder.Services.AddSwaggerGen(s => s.EnableRestModelAnnotations());
```

### 4. Run

Routes are derived from the type name — pluralized and kebab-cased. `Product` → `/api/products`, `Category` → `/api/categories`.

---

## Endpoints

Each registered type gets these endpoints:

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/{entities}` | Collection with optional filtering, ordering, and includes |
| `GET` | `/api/{entities}/{key}` | Single record by primary key |
| `GET` | `/api/{entities}/single` | Single record by query criteria |
| `GET` | `/api/{entities}/range` | Paginated collection |
| `POST` | `/api/{entities}` | Create |
| `PUT` | `/api/{entities}/{key}` | Update |
| `DELETE` | `/api/{entities}/{key}` | Delete |

---

## Querying

### Filter by field

```
GET /api/products?Name=Widget
GET /api/products?Price=9.99
```

### Multiple values (IN)

Pass the same parameter more than once to match any of the values:

```
GET /api/products?Price=9.99&Price=19.99
```

### Date range

Two values on a date field are interpreted as a `start < x <= end` range:

```
GET /api/products?CreatedAt=2024-01-01&CreatedAt=2024-12-31
```

### Text search

Annotate a `string` property with `[TextSearch]` to enable prefix matching:

```csharp
public class Product
{
    [TextSearch]
    public string Name { get; set; }
}
```

```
GET /api/products?Name=Wid   // matches "Widget", "Widge", etc.
```

Multiple values produce an OR:

```
GET /api/products?Name=Wid&Name=Gadg
```

### Filter through a navigation property

Use dot notation to filter by a field on a related entity. The related type must be a registered `DbSet<T>`:

```
GET /api/products?Category.Title=Electronics
```

Deep navigation is supported to any depth:

```
GET /api/products?Category.Department.Name=Engineering
```

### Eager loading (Include)

```
GET /api/products?Include=Category
GET /api/products?Include=Category&Include=Supplier
```

The value is matched case-insensitively and supports camelCase, snake_case, and kebab-case variants.

### Ordering

```
GET /api/products?OrderBy[Name]=Asc
GET /api/products?OrderBy[Price]=Desc
GET /api/products?OrderBy[Name]=Asc&OrderBy[Price]=Desc
```

### Pagination

Use the `range` endpoint with `First` (inclusive, zero-based) and `Last` (inclusive):

```
GET /api/products/range?First=0&Last=19   // first 20 items
GET /api/products/range?First=20&Last=39  // next 20 items
```

Response:

```json
{
  "first": 0,
  "last": 19,
  "items": [ ... ],
  "total": 142
}
```

---

## Custom route

Use `[RestApi]` to override the auto-generated route for a type:

```csharp
[RestApi("/internal/v2/products")]
public class Product { ... }
```

This also allows exposing types from assemblies other than the `DbContext` assembly.

---

## Save pipeline

Run logic before or after EF Core saves changes by implementing `IPipelineProcessor` and annotating the model:

```csharp
public class AuditProcessor : IPipelineProcessor
{
    public Task ProcessAsync(EntityEntry entry)
    {
        if (entry.Entity is Product p)
            p.UpdatedAt = DateTimeOffset.UtcNow;
        return Task.CompletedTask;
    }
}

[PreSaveProcessor(typeof(AuditProcessor))]
public class Product
{
    public DateTimeOffset UpdatedAt { get; set; }
    ...
}
```

`[PostSaveProcessor]` runs after the save completes. Processors are registered as singletons automatically.

Enable the interceptor when configuring the context:

```csharp
options.ContextOptionsBuilder = (sp, db) =>
    db.UseSqlServer(connectionString)
      .EnableModelPipelineAnnotations(sp);
```

---

## Exception handling

Add structured error responses for any exception type:

```csharp
app.UseCustomExceptionHandler();
```

Implement `IExceptionHandlingResultBuilder` and register it in options to customize the response for specific exception types:

```csharp
options.ExceptionMessageBuilders.Add(new MyValidationErrorBuilder());
```

---

## Db-first setup

Implement `IScaffoldService` to scaffold models at startup from an existing database:

```csharp
builder.Services.AddRestModel(new SqlScaffoldService(new SqlServerServicesOptions(connectionString)), options =>
{
    options.GlobalApiPrefix = "/api";
    options.SerializerOptions = o =>
    {
        o.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    };
});
```

Models and the DbContext are compiled at startup from the scaffolded source; no hand-written model classes are needed.

---

## Key detection

Primary keys are resolved in this order:

1. `[Key]` attribute
2. Property named `Id` (case-insensitive)
3. Property named `{TypeName}Id` (case-insensitive) — e.g., `ProductId` on `Product`
