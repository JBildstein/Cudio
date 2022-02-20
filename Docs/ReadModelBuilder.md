# Read Model Builders

A read model builder takes changes made to the write optimized data and transforms them into read optimized data.
The changes you register with the `ExecutionContext` during `Execute` in a command handler will automatically be sent to any `IReadModelBuilder<T>` that implements a handler method for the registered type.

Allowed method signatures are:

 - `void Create(Model value)`
 - `void Update(Model newValue)`
 - `void Update(Model oldValue, Model newValue)`
 - `void Delete(Model value)`
 - `Task Create(Model value)`
 - `Task Update(Model newValue)`
 - `Task Update(Model oldValue, Model newValue)`
 - `Task Delete(Model value)`

Where `Model` is whichever type you want to get notified on about changes.
The parameter names can be chosen freely and are not taken into consideration during handler discovery.

## Read Model Builder Example

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
