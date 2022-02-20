# Command and Query Bus

A bus is responsible for executing a command or a query and ensuring that everything is in order.
Both the `ICommandBus` and `IQueryBus` basically work the same way from the outside.

You can call `ICommandBus.Execute<TCommand>(TCommand command)` or `IQueryBus.Execute<TResult>(IQuery<TResult> query)` and get a `CommandResult` or `QueryResult<TResult>` back.
The result will tell you if authorization and validation succeeded and contain validation errors if any.
The `QueryResult<TResult>` will also contain the actual result value of the query if it was successful.

If you call `ExecuteDirect` on either bus, the authorization and validation is skipped and the command or query is directly executed.
Because there is no authorization or validation that could fail, no return value other than the actual query result is needed.

Any uncaught exceptions that may happen during the execution of a command or query is propagated back to the initial call of `Execute` or `ExecuteDirect`.

## Command Bus Flow

```
Request (e.g. HTTP, user interaction, CRON job, etc.)
   |
Create command instance (e.g. CreateBookCommand)
   |
Send command to command bus
ICommandBus.Execute<T>(T)
   |
  Within the bus, this happens:
    \
    Authorization of command (e.g. is user allowed to do this)
    ICommandAuthorizer<T>.Authorize(AuthorizationContext, T)
      |
    Validation of command (e.g. is the value supplied valid)
    ICommandValidator<T>.Validate(ValidationContext, T)
      |
    Begin DB transaction (if applicable)
    ITransactionFactory.OpenTransaction()
      |
    Execute command/store data in write table
    ICommandHandler<T>.Execute(ExecutionContext, T)
      |
    Every ready model builder for the changed entities is called
    IReadModelBuilder<T>.Create/Update/Delete
      |
    Commit DB transaction (if applicable)
    ITransaction.Commit()
     /
  Bus returns info of that chain (i.e. did auth and validation succeed)
   |
Handle command result (e.g. show any errors or a success message)
```


## Query Bus Flow

```
Request (e.g. HTTP, user interaction, CRON job, etc.)
   |
Create query instance (e.g. GetBooksByAuthorQuery)
   |
Send query to query bus
IQueryBus.Execute<T>(IQuery<T>)
   |
  Within the bus, this happens:
    \
    Authorization of query (e.g. is user allowed to read this)
    IQueryAuthorizer<T>.Authorize(AuthorizationContext, T)
      |
    Validation of query (e.g. is the value supplied valid)
    IQueryValidator<T>.Validate(ValidationContext, T)
      |
    Execute query
    IQueryHandler<TQuery, TResult>.Execute(TQuery)
     /
  Bus returns info of that chain (i.e. did auth and validation succeed)
   |
Handle query result (e.g. show any errors or return the result)
```