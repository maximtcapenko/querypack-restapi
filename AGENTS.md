# Agent Guidelines for QueryPack.RestApi

This document provides guidance for agents working on the `QueryPack.RestApi` project.

## Project Overview

`QueryPack.RestApi` is a .NET library that automatically generates CRUD REST APIs from Entity Framework Core `DbContext`. It aims to eliminate boilerplate controller code by exposing `DbSet<T>` properties as RESTful endpoints with built-in support for filtering, ordering, eager loading, and pagination.

## Technology Stack

- **Framework:** .NET 10.0
- **ORM:** Entity Framework Core 10.0
- **API Documentation:** Swashbuckle (Swagger)
- **Dynamic Compilation:** Microsoft.CodeAnalysis.CSharp (Roslyn) for DB-first scaffolding.
- **Testing:** xUnit, FluentAssertions, AutoFixture, Microsoft.AspNetCore.Mvc.Testing.

## Core Architecture

### 1. API Generation
The library uses a generic `RestModelController<TModel>` to handle CRUD operations. These controllers are registered dynamically at startup based on the `DbSet<T>` properties found in the registered `DbContext`.

### 2. Criteria & Binding
Filtering and ordering are handled through `ICriteria<TModel>`. The library includes custom model binders (`ICriteriaBinder`) that translate query string parameters into expressions that can be applied to `IQueryable<TModel>`.

### 3. Save Pipeline
Interceptors can be registered using `[PreSaveProcessor]` and `[PostSaveProcessor]` attributes on model classes. These processors must implement `IPipelineProcessor`.

### 4. DB-First Scaffolding
The `IScaffoldService` allows for dynamic model generation from an existing database schema. This uses Roslyn to compile model classes and a `DbContext` at runtime.

## Coding Conventions

- **C# Features:** Use modern C# features such as file-scoped namespaces, primary constructors, and required members where appropriate.
- **Asynchrony:** Always use `Async` suffix for asynchronous methods and accept a `CancellationToken`.
- **Visibility:** Use `internal` for implementation details that shouldn't be exposed in the public API.
- **Naming:** Follow standard .NET naming conventions (PascalCase for classes/methods/properties, camelCase for local variables/parameters).
- **DI:** Use constructor injection (prefer primary constructors in new code).

## Testing Strategy

- **Integration Tests:** Located in `test/QueryPack.RestApi.Tests`. These use `WebApplicationFactory` to run the API in-memory against an `InMemoryDatabase`.
- **Setup:** See `WebApplicationFactoryExtensions.cs` for how the test environment is configured.
- **Assertions:** Use `FluentAssertions` for readable test assertions.
- **Data Generation:** Use `AutoFixture` for generating test data.

## Project Structure

- `src/QueryPack.RestApi/`: The core library.
    - `Mvc/`: Generic controller and MVC-related components.
    - `Model/`: Core abstractions and metadata.
    - `Internal/`: Utility classes and internal implementations.
    - `Extensions/`: Service collection and app builder extensions.
- `src/*.Example/`: Example projects demonstrating different usage scenarios.
- `test/QueryPack.RestApi.Tests/`: Integration and unit tests.

## Common Tasks

- **Adding a new Filter/Criteria:**
    - Look at `src/QueryPack.RestApi/Mvc/Model/Binders/` for existing binders (e.g., `QueryCriteriaBinder`).
    - Look at `src/QueryPack.RestApi/Model/Internal/Criterias/` for criteria implementations (e.g., `QueryCriteria`).
    - Register new binders in `RuntimeCriteriaBinderProvider`.
- **Modifying API behavior:** The core CRUD logic resides in `RestModelController<TModel>`.
- **Updating Metadata:** `IModelMetadataProvider` and `PropertyMetadata` handle how models are inspected for API generation.
- **Customizing API Routes:** Use the `[RestApi]` attribute on model classes.

## How-to Guides

### Adding a Custom Search Annotation

If you want to add a new way to filter a specific property (e.g., `[ContainsSearch]`), follow these steps:

1.  **Create the Attribute:**
    Create a class that inherits from `Attribute` and implements `IAnnotation`.
    ```csharp
    [AttributeUsage(AttributeTargets.Property)]
    public class MySearchAttribute : Attribute, IAnnotation
    {
        public void Apply(IAnnotationContext context)
        {
            // Build your LINQ Expression using context.PropertyExpression and context.Input
            var expression = Expression.Call(context.PropertyExpression, ...);
            context.SetResult(expression);
        }
    }
    ```
2.  **Use the Attribute:**
    Apply the attribute to a property in your model.
    ```csharp
    public class Product
    {
        [MySearch]
        public string Description { get; set; }
    }
    ```
3.  **Processing:**
    The `QueryCriteria<TModel>` will automatically detect the `IAnnotation` and call its `Apply` method during query binding.

### Adding a New Global Search Feature

To add a new global search parameter (e.g., `?search=term` that searches across multiple fields):

1.  **Define a new ICriteria implementation** in `Model/Internal/Criterias/` that performs the multi-field search logic.
2.  **Create a new ICriteriaBinder** in `Mvc/Model/Binders/` to extract the `search` parameter from the query string and create your new criteria.
3.  **Register the binder** in `RuntimeCriteriaBinderProvider`.
