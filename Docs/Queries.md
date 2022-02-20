# Queries

The purpose of a query is to query some values without modifying and data.
There are two parts to a query:
 - the query definition containing all the infos needed to execute it and
 - the query handler that contains the logic to actually execute it.

## Query Definition

In Cudio, anything that implements the `IQuery<T>` interface is considered a query.
Usually when you create a query you'd inherit from `QueryBase<T>` so you don't have to implement the interface yourself.

Example of a query:
```csharp
public class SearchDocumentsQuery : QueryBase<IEnumerable<DocumentModel>>
{
    public string Search { get; }

    public SearchDocumentsQuery(string search)
    {
        Search = search;
    }
}
```

## Query Handler

To create a query handler, the `IQueryHandler<TQuery, TResult>` interface has to be implemented.
Optionally you can also implement `IQueryAuthorizer<T>` and `IQueryValidator<T>` to first authorize and validate a query before executing it.
To make things a bit more concise, you can choose to implement a single combined handler interface:

| Interface | Authorized | Validated |
|-|:-:|:-:|
| `IQueryHandler<T>` | ❌ | ❌ |
| `IAuthorizedQueryHandler<T>` | ✔ | ❌ |
| `IValidatedQueryHandler<T>` | ❌ | ✔ |
| `IFullQueryHandler<T>` | ✔ | ✔ |

Typically you'd use one of the combined interface so that all the logic is together but you can just as well have different classes implementing each of the base interfaces (Handle, Authorize, Validate).

### Authorization

The authorization method has two parameters, the `AuthorizationContext` and the query to be authorized.
The `AuthorizationContext` contains the user as a `System.Security.Claims.ClaimsPrincipal` which was provided by the `IClaimsPrincipalProvider`.
If you are using `Cudio.AspNetCore`, the `IClaimsPrincipalProvider` is registered by default using the `HttpContext`.
Otherwise you'll have to create your own depending on where user auth info comes from.

Once you have done your authorization checks you can call `Succeed()` or `Fail()` on the `AuthorizationContext` depending on your result.
Note that once `Fail()` has been called, the authorization result will **always** be failed even if you have called `Succeed()` before or afterwards.
If neither `Succeed()` or `Fail()` has been called, the authorization result is considered failed since `HasSucceeded` is false.

### Validation

The validation method has two parameters, the `ValidationContext` and the command to be validated.
The `ValidationContext` is basically a dictionary with an error key and a list of error messages for that key.
Use the `AddError(key, error)` method to add an error to the list and with that, fail the validation.

With `Cudio.AspNetCore` you can transfer the errors to the `ModelState` of a controller with the `ToModelState(modelState)` extension method.

### Execution

The execution method only has the query to be executed as a parameter.
So there's not much else to do other than executing your query and returning the result.

### Example Handler for Query

```csharp
// the return type has to be stated again here because the C# compiler cannot infer it from the query type
public class SearchDocumentsQueryHandler : IFullQueryHandler<SearchDocumentsQuery, IEnumerable<DocumentModel>>
{
    private readonly IMyDb db;

    public SearchDocumentsQueryHandler(IMyDb db)
    {
        this.db = db;
    }

    // check if the calling user (e.g. web request) is allowed to execute this query
    public Task Authorize(AuthorizationContext context, SearchDocumentsQuery query)
    {
        // simple check for the users role in  this example.
        // you may have to check the DB or a cache to see if a user has access to a specific resource
        // e.g. to check the user ID claim against a document ID from a query
        if (context.User.IsInRole("Editor")) { context.Succeed(); }
        else { context.Fail(); }

        return Task.CompletedTask;
    }

    // check if the provided value is valid
    public Task Validate(ValidationContext context, SearchDocumentsQuery query)
    {
        if (string.IsNullOrEmpty(query.Search))
        {
            context.AddError("Search", "A search text must be given");
        }

        return Task.CompletedTask;
    }

    // execute the actual query logic (if Authorize and Validate were both successful)
    public async Task<IEnumerable<DocumentModel>> Execute(SearchDocumentsQuery query)
    {
        return await db.Documents.Where(t => t.Title.Like(query.Search));
    }
}
```
