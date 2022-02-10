using System.Threading.Tasks;

namespace Cudio
{
    public sealed class DummyTransactionFactory : ITransactionFactory
    {
        public Task<ITransaction> OpenTransaction()
        {
            return Task.FromResult<ITransaction>(default(DummyTransaction));
        }
    }
}
