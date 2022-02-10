using System.Threading.Tasks;

namespace Cudio
{
    public readonly struct DummyTransaction : ITransaction
    {
        public Task Commit()
        {
            return Task.CompletedTask;
        }

        public Task Rollback()
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
