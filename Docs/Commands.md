# Commands

A commands purpose is to execute some logic and/or modify data without having a return value.
There are two parts to a command:
 - the command definition containing all the infos needed to execute it and
 - the command handler that contains the logic to actually execute it.

## Command Definition

In Cudio, anything that implements the `ICommand` interface is considered a command.
Usually when you create a command you'd inherit from `CommandBase` so you don't have to implement the interface yourself.

For simple workflows, Cudio has two commands pre-defined:
 - `ValueCommand<T>`: basically makes a command out of a model
 - `CudCommand<T>`: same as `ValueCommand<T>` but with an additional flag of the type of change (Create, Update, Delete)

Those two commands have shortcuts for execution in the `ICommandBus`.

Example of a command:
```csharp
public class ArchiveEntryCommand : CommandBase
{
    public int EntryId { get; }

    public ArchiveEntryCommand(int entryId)
    {
        EntryId = entryId;
    }
}
```

## Command Handler

To create a command handler, the `ICommandHandler<T>` interface has to be implemented.
Optionally you can also implement `ICommandAuthorizer<T>` and `ICommandValidator<T>` to first authorize and validate a command before executing it.
To make things a bit more concise, you can choose to implement a single combined handler interface:

| Interface | Command | Authorized | Validated |
|-|-|:-:|:-:|
| `ICommandHandler<T>` | `T` | ❌ | ❌ |
| `IAuthorizedCommandHandler<T>` | `T` | ✔ | ❌ |
| `IValidatedCommandHandler<T>` | `T` | ❌ | ✔ |
| `IFullCommandHandler<T>` | `T` | ✔ | ✔ |
| `IValueCommandHandler<T>` | `ValueCommand<T>` | ❌ | ❌ |
| `IAuthorizedValueCommandHandler<T>` | `ValueCommand<T>` | ✔ | ❌ |
| `IValidatedValueCommandHandler<T>` | `ValueCommand<T>` | ❌ | ✔ |
| `IFullValueCommandHandler<T>` | `ValueCommand<T>` | ✔ | ✔ |
| `ICudCommandHandler<T>` | `CudCommand<T>` | ❌ | ❌ |
| `IAuthorizedCudCommandHandler<T>` | `CudCommand<T>` | ✔ | ❌ |
| `IValidatedCudCommandHandler<T>` | `CudCommand<T>` | ❌ | ✔ |
| `IFullCudCommandHandler<T>` | `CudCommand<T>` | ✔ | ✔ |

Typically you'd use one of the combined interface so that all the logic is together but you can just as well have different classes implementing each of the base interfaces (Handle, Authorize, Validate).

### Authorization

The authorization method has two parameters, the `AuthorizationContext` and the command to be authorized.
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

The execution method has two parameters, the `ExecutionContext` and the command to be executed.
With the `ExecutionContext` you can register changes of your write optimized models so that the read optimized tables can be updated.
Use `RegisterCreate<T>(value)`, `RegisterUpdate<T>(oldValue, newValue)` and `RegisterDelete<T>(value)` depending on the  change.
Ideally those calls would happen (semi-)automatically by integrating them with whatever ORM you are using.

The `ExecutionContext` also provides the `ExecuteSubcommand<T>(command)` method to execute another command within the same context (and potentially DB transaction).
Note that authorization and validation will **not** be executed for the given subcommand.

### Example Handler for Custom Command

```csharp
public class ArchiveEntryCommandHandler : IFullCommandHandler<ArchiveEntryCommand>
{
    private readonly IMyDb db;

    public ArchiveEntryCommandHandler(IMyDb db)
    {
        this.db = db;
    }

    // check if the calling user (e.g. web request) is allowed to execute this command
    public Task Authorize(AuthorizationContext context, ArchiveEntryCommand command)
    {
        // simple check for the users role in  this example.
        // you may have to check the DB or a cache to see if a user has access to a specific resource
        // e.g. to check the user ID claim against a document ID from a command
        if (context.User.IsInRole("Editor")) { context.Succeed(); }
        else { context.Fail(); }

        return Task.CompletedTask;
    }

    // check if the provided value is valid
    public async Task Validate(ValidationContext context, ArchiveEntryCommand command)
    {
        // checking if the entry exists in the DB may not always be useful.
        // in this example we assume that updating the value is computationally expensive
        // and quick user feedback is important.
        bool exists = await db.Entries.Any(t => t.Id == command.EntryId);
        if (!exists)
        {
            context.AddError("EntryId", $"Entry with ID {command.EntryId} does not exist");
        }
    }

    // execute the actual command logic (if Authorize and Validate were both successful)
    public async Task Execute(ExecutionContext context, ArchiveEntryCommand command)
    {
        // call a fictional ORM to update the archive state and return previous and updated value
        var change = await db.Entries.Update(t => t.Archived = true)
            .Where(t => t.EntryId == command.EntryId)
            .RunWithSingleChange();

        // register the updated value with Cudio for potential read optimized table creation
        // this call might be combined with the above call to the ORM
        context.RegisterUpdate(change.OldValue, change.NewValue);
    }
}
```

### Example Handler for ValueCommand

```csharp
public class CreateBookCommandHandler : IFullValueCommandHandler<CreateBookModel>
{
    private readonly IMyDb db;

    public CreateBookCommandHandler(IMyDb db)
    {
        this.db = db;
    }

    public Task Authorize(AuthorizationContext context, ValueCommand<CreateBookModel> command)
    {
        if (context.User.IsInRole("Librarian")) { context.Succeed(); }
        else { context.Fail(); }

        return Task.CompletedTask;
    }

    public Task Validate(ValidationContext context, ValueCommand<CreateBookModel> command)
    {
        if (string.IsNullOrEmpty(command.Value.Title))
        {
            context.AddError("Title", "Book must have a title");
        }

        return Task.CompletedTask;
    }

    public async Task Execute(ExecutionContext context, ValueCommand<CreateBookModel> command)
    {
        // calling fictional ORM that returns added value
        var created = await db.Books.Add(command.Value);
        context.RegisterCreate(created);
    }
}
```

### Example Handler for CudCommand

```csharp
public class BookCommandHandler : IFullCudCommandHandler<BookModel>
{
    private readonly IMyDb db;

    public BookCommandHandler(IMyDb db)
    {
        this.db = db;
    }

    public Task Authorize(AuthorizationContext context, CudCommand<BookModel> command)
    {
        if (context.User.IsInRole("Librarian")) { context.Succeed(); }
        else { context.Fail(); }

        return Task.CompletedTask;
    }

    public Task Validate(ValidationContext context, CudCommand<BookModel> command)
    {
        if (command.ChangeType == ChangeType.Create || command.ChangeType == ChangeType.Update)
        {
            if (string.IsNullOrEmpty(command.Value.Title))
            {
                context.AddError("Title", "Book must have a title");
            }
        }

        if (command.ChangeType == ChangeType.Update || command.ChangeType == ChangeType.Delete)
        {
            if (command.Value.Id <= 0)
            {
                context.AddError("Id", "Book ID must be greater than zero");
            }
        }

        return Task.CompletedTask;
    }

    public async Task Execute(ExecutionContext context, CudCommand<BookModel> command)
    {
        // calling fictional ORM that returns added, updated and deleted values
        switch (command.ChangeType)
        {
            case ChangeType.Create:
                var created = await db.Books.Add(command.Value);
                context.RegisterCreate(created);
                break;
            case ChangeType.Update:
                var changed = await db.Books.Update(command.Value);
                context.RegisterUpdate(changed.OldValue, changed.NewValue);
                break;
            case ChangeType.Delete:
                var deleted = await db.Books.Delete(command.Value.Id);
                context.RegisterDelete(deleted);
                break;
        }
    }
}
```
