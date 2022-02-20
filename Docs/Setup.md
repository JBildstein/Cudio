# Setup

Cudio is using dependency injection extensively and uses reflection to discover things like command and query handlers or read table builders.

## Core Library

To help you find all the services and handlers you need, you can use the `CudioServiceDiscovery` class.
Simply create an instance with the appropriate constructor depending on where you have defined your commands, queries, handlers etc.
Unless you have custom logic or implementations, you can call `AddAll(services)` to let Cudio handle type discovery and add everything to the provided DI framework.

These types are added:
 - `CommandBus` as `ICommandBus`
 - `QueryBus` as `IQueryBus`
 - any type implementing `ICommandHandler<T>`
 - any type implementing `ICommandAuthorizer<T>`
 - any type implementing `ICommandValidator<T>`
 - any type implementing `IQueryHandler<T>`
 - any type implementing `IQueryAuthorizer<T>`
 - any type implementing `IQueryValidator<T>`
 - any type implementing `IReadModelBuilder<T>`
 - any type implementing `IHydrator<T>`

You still have to regiser your own implementation of the following interfaces:

### IClaimsPrincipalProvider

Needed to do command and query authorization.\
It provides the `ClaimsPrincipal` of the user that wants to execute a command or query.
How you implement this depends entirely on how you authenticate/authorize your users or if you even have a concept like that.
If you don't have users or a need for authorization, just implement the interface by returning an empty `ClaimsPrincipal` (e.g. `new ClaimsPrincipal()`)

### ITransactionFactory

Needed to ensure the read and write optimized tables are consistent and updated within a single transaction. It should be registered with DI as scoped.\
Typically you'd just open a transaction with your DB provider and return that.
Cudio contains types to wrap DB transaction types to Cudios `ITransaction` type:
 - `System.Transaction.CommittableTransaction` -> `SystemCommitableTransaction`
 - `System.Data.Common.DbTransaction` -> `SystemDataTransaction`

If your DB provider uses different types you'll have to create your own implementation of `ITransaction` which should be very simple.

If you have different read and write DBs, a single transaction (usually) won't work and you'll have to implement your own strategy of keeping your data consistent.

## ASP.NET Core

The `Cudio.AspNetCore` package contains extension methods to register Cudio with the `IServiceCollection`.
Simply call the appropriate `AddCudio` overload to add all necessary Cudio services and your commands, queries, read table builders etc.

The `IClaimsPrincipalProvider` is already provided by using the `HttpContext` with `IHttpContextAccessor` which is registered as well.

You still have to register a `ITransactionFactory` yourself, because that depends on what DB you use and how your DB setup looks like in general.
See the [ITransactionFactory](#itransactionfactory) topic above for details.

Example if you use the minimal hosting model: 
```csharp
using Cudio.AspNetCore;
...
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
...
builder.Services.AddCudio();
builder.Services.AddScoped<ITransactionFactory, MyTransactionFactory>();
...
```

or the classic hosting model with `Startup.cs`:
```csharp
using Cudio.AspNetCore;
...

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
...
        services.AddCudio();
        services.AddScoped<ITransactionFactory, MyTransactionFactory>();
    }

    public void Configure(IApplicationBuilder app)
    {
...
    }
}
```

The `...` is used to denote omitted code not relevant to setting up Cudio.


