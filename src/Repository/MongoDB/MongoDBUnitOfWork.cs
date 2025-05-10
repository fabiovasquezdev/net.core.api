using MongoDB.Driver;

namespace Repository.MongoDB
{
    internal sealed class MongoDBUnitOfWork : IMongoUnitOfWork
    {
        private readonly IMongoClient _client;
        private readonly List<Action> _actions;
        private readonly IClientSessionHandle _session;

        private static readonly TransactionOptions _transactionOptions = new(
            ReadConcern.Majority, 
            default,
            WriteConcern.WMajority);

        public MongoDBUnitOfWork(IMongoClient client, IClientSessionHandle session)
        {
            _client = client;
            _session = session;
            _actions = new List<Action>();
        }

        public async Task WithTransactionAsync(Func<Task> fn, CancellationToken cancellationToken)
        {
            using (_session)
            {
                await _session.WithTransactionAsync(async (session, _) => 
                {
                    await fn();
                    return true;
                }, _transactionOptions, cancellationToken);
            }
        }

        public void SaveChanges()
        {
            foreach (var a in _actions)
                a();
            _actions.Clear();
        }

        public async Task SaveChangesAsync()
        {
            await Task.Run(() => {
                foreach (var a in _actions)
                    a();
                _actions.Clear();
            });
        }

        void IMongoUnitOfWork.Add(Action a) =>
            _actions.Add(a);
    }
} 