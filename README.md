# Cudio

[![CI](https://github.com/JBildstein/Cudio/actions/workflows/ci.yml/badge.svg)](https://github.com/JBildstein/Cudio/actions/workflows/ci.yml)

Cudio is a C#/.NET library to help you write clean applications in a CQ(R)S style.
It integrates well with ASP.NET Core but it can be used anywhere you like.
Cudio is short for "CUD it out" (where CUD is CRUD without read) which is a play on the phrase "cut it out", i.e. stop doing plain CRUD and separate it into CUD and R(ead).

[CQS](https://en.wikipedia.org/wiki/Command%E2%80%93query_separation "CQS description on Wikipedia") stands for Command Query Separation and is the simple principle of separating methods that modify data and ones that return data.

[CQRS](https://martinfowler.com/bliki/CQRS.html "CQRS description by Martin Fowler") stands for Command Query Responsibility Segregation and applies the CQS principle as an architecture (follow the link for an in-depth description).

Cudio is not a full blown CQRS framework but rather a library that applies the CQS principle and borrows some concepts from CQRS without inheriting all the complexities.
The goal is to keep your application clean and as simple as possible by allowing you to opt into more complicated use cases as needed.

You can start out with simple commands and queries mimicking a CRUD style application but already benefiting from clean separation of logic.
Very soon you'll likely want to create more queries that do more than just read a single entity.
Later you may want more than one command for creating a single model, e.g. because your business logic differs depending on who created it.

Then, if you want to go a step further, you can also separate the tables (and even DB) for writing and reading to optimize each use case.
When storing data you typically want it normalized and easily updatable. For reading, denormalized data is typically far quicker to access and queries are simpler to write.

With Cudio it's very easy to do this, even if you only need it for some tables and not all of them.
When executing a command, you register all changes that were made (this may be integrated with an ORM) and provide a read table builder that reacts to those changes.
The nice thing about this is that it doesn't matter which command made a change, in fact, no command needs to (or even should) have knowledge of the read optimized tables at all.
So whenever you add a new command that changes any data, all the read optimized tables will just continue to work and you can't forget to add logic to update them.

Another use case for a read table builder would be some kind of data change log.
e.g. every time the UserRights table is updated by a command, the read table builder can store the old and new value for auditing purposes.

To some extent this can be done within a DB as well (e.g. with triggers or materialized views) but with Cudio it's DB agnostic and you can execute complex business logic on the data before storing it.
This means that applications that primarily write data and read very little or only simple data won't be suited well for this approach.
The drawback to the whole read optimized tables concept is that data writes will be slower because multiple tables (or potentially DBs) have to be written to and a small overhead for the change tracking is added.

Read optimized tables normally only contain data that can be recreated from the write optimized data.
This means you can just drop a read optimized table and recreated it again from existing data (hydrate).
Sometimes that's just easier after a bugfix or when the schema or business logic was changed.
That's completely up to you though, Cudio won't stand in the way of either choice.

To tie everything together, there is a command and a query bus to which you pass an instance of your command or query.
The bus then looks for the correct command or query handler, checks optional authorization and validation and then executes it.
After a command has made changes, the bus will also make sure that the read optimized tables are updated.
For more details check out the [Command and Query Bus documentation](Docs/Bus.md).

## Examples

Well, that was a lot of text and explanations, but how would all of this look like?

**Command**
```csharp
public class CreateBookCommand : CommandBase
{
    public Book Value { get; }

    public CreateBookCommand(Book value)
    {
        Value = value;
    }
}
```
**Command Handler**
```csharp
public class CreateBookCommandHandler : IFullCommandHandler<CreateBookCommand>
{
    private readonly IMyDb db;

    public CreateBookCommandHandler(IMyDb db)
    {
        this.db = db;
    }

    // check if the calling user (e.g. web request) is allowed to execute this command (optional)
    public Task Authorize(AuthorizationContext context, CreateBookCommand command)
    {
        if (context.User.IsInRole("Librarian")) { context.Succeed(); }
        else { context.Fail(); }

        return Task.CompletedTask;
    }

    // check if the provided value is valid (optional)
    public Task Validate(ValidationContext context, CreateBookCommand command)
    {
        if (string.IsNullOrEmpty(command.Value.Title))
        {
            context.AddError("Title", "No book title given");
        }

        return Task.CompletedTask;
    }

    // execute the actual command logic (if Authorize and Validate were both successful)
    public async Task Execute(ExecutionContext context, CreateBookCommand command)
    {
        // persist the value in a DB
        var stored = await db.Books.Add(command.Value);

        // register the created value with Cudio for potential read optimized table creation
        // this call might be combined with the above call to the ORM
        context.RegisterCreate(stored);
    }
}
```
If you don't need the `Authorize` or `Validate` methods you don't have to use them, just implement a different handler interface.
Check out the [Command Documentation](Docs/Commands.md) for more details.

**Query**

A query follows the exact same schema, you just use different base types/interfaces:
```csharp
public class GetBookQuery : QueryBase<Book>
{
    public int Id { get; }

    public GetBookQuery(int id)
    {
        Id = id;
    }
}
```

**Query Handler**
```csharp
public class GetBookQueryHandler : IQueryHandler<GetBookQuery, Book>
{
    private readonly IMyDb db;

    public GetBookQueryHandler(IMyDb db)
    {
        this.db = db;
    }

    public async Task<Book> Execute(GetBookQuery query)
    {
        return await db.Books.Get(query.Id);
    }
}
```
In this example the query handler doesn't need validation or authorization but it can be added just like with the command handler above.
Check out the [Query Documentation](Docs/Queries.md) for more details.

**Using it**

Assuming this is a web app and we have a controller:
```csharp
public class BookController
{
    private readonly ICommandBus commandBus;
    private readonly IQueryBus queryBus;

    public BookController(ICommandBus commandBus, IQueryBus queryBus)
    {
        this.commandBus = commandBus;
        this.queryBus = queryBus;
    }

    public async Task<IActionResult> Get(int id)
    {
        var query = new GetBookQuery(id);

        // since there is no validation or authorization in GetBookQueryHandler,
        // we will simply use ExecuteDirect to get the result directly
        Book result = await queryBus.ExecuteDirect(query);

        if (result == null) { return NotFound(); }
        
        return Ok(result);
    }

    public async Task<IActionResult> Post(Book value)
    {
        var command = new CreateBookCommand(value);
        var result = await commandBus.Execute(command);

        // checking the result for errors can usually be delegated to a
        // common method and doesn't have to be repeated every time
        if (result.Unauthorized) { return Unauthorized(); }
        else if (result.ValidationFailed) { return BadRequest(); }
        
        return Ok();
    }
}
```
The `ICommandBus` and `IQueryBus` are part of Cudio and would typically be registered with a DI framework.
For more infos on busses, check out the [Command and Query Bus documentation](Docs/Bus.md).

**Read optimized table builder**

This example is very simple and wouldn't make a lot of sense in reality but shows the basic concept well enough.
It simply populates a table with values from the Books table and the Authors table so that a query doesn't have to do a join.
```csharp
public class BookAuthorReadModelBuilder : IReadModelBuilder<BookAuthor>
{
    private readonly IMyDb db;

    public CreateBookCommandHandler(IMyDb db)
    {
        this.db = db;
    }

    public async Task Create(Book value)
    {
        var author = await db.Authors.Get(value.AuthorId);
        await db.BookAuthors.Add(new
        {
            BookId = value.Id,
            AuthorId = author.Id,
            Title = value.Title,
            AuthorName = $"{author.FirstName} {author.LastName}"
        });
    }

    // the update method can be defined with both old and new value...
    public async Task Update(Book oldValue, Book newValue)
    {
        await db.BookAuthors
            .Update(t => t.Title = newValue.Title)
            .Where(t => t.BookId == newValue.Id);
    }

    // ...or just the new value
    public async Task Update(Author value)
    {
        await db.BookAuthors
            .Update(t => t.AuthorName = $"{value.FirstName} {value.LastName}")
            .Where(t => t.AuthorId == value.Id);
    }

    public async Task Delete(Book value)
    {
        await db.BookAuthors
            .Delete()
            .Where(t => t.BookId == value.Id);
    }

    public async Task Delete(Author value)
    {
        await db.BookAuthors
            .Delete()
            .Where(t => t.AuthorId == value.Id);
    }
}
```
Whenever a command changes a value, Cudio makes sure that the appropriate `Create`, `Update` or `Delete` method of any registered `IReadModelBuilder` is called.
If multiple values were changed, the handlers are simply called repeatedly for each value (in no guaranteed order).
If no method for a value exists (e.g. Author create in the example above), it's simply ignored and nothing happens.

## Usage Infos

Cudio consists of the following NuGet packages:

| Package Id | Description | NuGet |
|---|---|---|
| Cudio | Core library | [![NuGet](https://img.shields.io/nuget/v/Bildstein.Cudio.svg)](https://www.nuget.org/packages/Bildstein.Cudio/) |
| Cudio.AspNetCore | Integration with ASP.NET Core | [![NuGet](https://img.shields.io/nuget/v/Bildstein.Cudio.AspNetCore.svg)](https://www.nuget.org/packages/Bildstein.Cudio.AspNetCore/) |

Cudio is made to work with dependency injection and uses `Microsoft.Extensions.DependencyInjection.Abstractions` as a basis, so it should integrate well with many DI frameworks out there.

If you use app trimming you have to be careful to exclude any types/assemblies that Cudio uses (like command and query handlers).
Cudio relies on reflection to discover handlers and methods to invoke.

## Limitations

To keep Cudio simple, the whole command and query bus flow has to be done in the same process and cannot be outsourced.
This means that
 - you can have more than one instance of your application running as long as they all use the same DB (or you have your DB set up in a way that keeps all data in sync).
 - you can also have some instances only handling commands and some instances only queries (this can help you to scale more fine grained)
 - but it's not possible to split the updates for read and write tables/DBs into separate instances
 - any issued command or query is always executed in the same process and cannot be sent to a different instance

So if you intend to build a bigger setup with microservices and event streaming and the like your are probably better of with something else.
If you intend to grow your application slowly and maybe want to scale to microservices and event streaming sometime in the future you could consider starting out with Cudio and then replace it.
Cudio follows a similar mindset to those architectures and if you implement your application logic with your future goal in mind it should be fairly straightforward (though not quite trivial) to do it.

## Building
Cudio is a standard C# project without any special dependencies currently targeting .NET 6.\
You can build it with your favorite IDE (like Visual Studio, Visual Studio Code, etc.) or just plain old dotnet CLI.\
There's also no platform requirement, so if .NET works, this'll too.

## Contributing
Thank you for your interest in contributing to Cudio.

### Questions
If you have any question, head over to the [discussions Q&A category](https://github.com/JBildstein/Cudio/discussions/categories/q-a).\
Please check first if your question has already been answered before opening a new one.

### Bugs
Please check if there is already and open [issue](https://github.com/JBildstein/Cudio/issues) for your bug.\
If there is an existing issue, upvote it or add more information there.\
Otherwise create a new issue and make sure to fill out the template.

### Feature Requests
If you have and idea or a feature request, head over to the [discussions idea category](https://github.com/JBildstein/Cudio/discussions/categories/ideas).\
Please check first if your idea or feature request already exists and upvote it if so.\
Otherwise start a new discussion and describe what you are expecting of the feature and why you need it.

### Pull Requests
Before you create a pull request, please start a [discussion in the idea category](https://github.com/JBildstein/Cudio/discussions/categories/ideas) first.\
This is important so you don't waste your time working on something that is already done by someone else or does not fit the scope of this project.\
Once you get the all clear, make sure that you add or update unit test if appropriate for the change and adhere to the code style rules (no warnings should show up).
