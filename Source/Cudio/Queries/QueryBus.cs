using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Cudio
{
    /// <summary>
    /// A bus to handle queries.
    /// </summary>
    public class QueryBus : IQueryBus
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IClaimsPrincipleProvider claimsPrincipalProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryBus"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider for DI.</param>
        /// <param name="claimsPrincipalProvider">The claims principal provider to authorize commands.</param>
        public QueryBus(IServiceProvider serviceProvider, IClaimsPrincipleProvider claimsPrincipalProvider)
        {
            this.serviceProvider = serviceProvider;
            this.claimsPrincipalProvider = claimsPrincipalProvider;
        }

        /// <inheritdoc/>
        public async Task<QueryResult<T>> Execute<T>(IQuery<T> query)
        {
            // workaround for inflexible generic type resolution:
            // A method with a signature of
            // TResult Foo<TQuery, TResult>(TQuery) where TQuery : IQuery<TResult>
            // cannot be resolved automatically by the compiler so both generic
            // arguments would have to be supplied for each call, e.g.:
            // var bar = Foo<MyQuery, string>(query);
            // This gets annoying really fast but it's even worse if the return type
            // of the query changes in the future and it would have to be changed
            // for each call instead of just the query definition and the handler.
            // By using dynamic we can make the runtime take care of the problem
            // with a little help of the default(T) for the return type.
            return await ExecuteBase((dynamic)query, default(T));
        }

        /// <inheritdoc/>
        public async Task<T> ExecuteDirect<T>(IQuery<T> query)
        {
            // re: dynamic -> see Execute<T>(IQuery<T>)
            return await ExecuteDirectBase((dynamic)query, default(T));
        }

        private async Task<QueryResult<TResult>> ExecuteBase<TQuery, TResult>(TQuery query, TResult dummy)
            where TQuery : IQuery<TResult>
        {
            var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            var authorizor = serviceProvider.GetService<IQueryAuthorizer<TQuery>>();
            var validator = serviceProvider.GetService<IQueryValidator<TQuery>>();

            var authContext = new AuthorizationContext(claimsPrincipalProvider.GetClaimsPrincipal());
            var validationContext = new ValidationContext();

            if (authorizor != null) { await authorizor.Authorize(authContext, query); }
            else { authContext.Succeed(); }

            var result = default(TResult);
            if (authContext.HasSucceeded)
            {
                if (validator != null) { await validator.Validate(validationContext, query); }

                if (!validationContext.HasErrors) { result = await handler.Execute(query); }
            }

            return new QueryResult<TResult>(query, result, validationContext, authContext);
        }

        private async Task<TResult> ExecuteDirectBase<TQuery, TResult>(TQuery query, TResult dummy)
            where TQuery : IQuery<TResult>
        {
            var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
            return await handler.Execute(query);
        }
    }
}
