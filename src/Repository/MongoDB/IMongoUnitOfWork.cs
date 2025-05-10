namespace Repository.MongoDB
{
    public interface IMongoUnitOfWork
    {
        Task WithTransactionAsync(Func<Task> fn, CancellationToken cancellationToken);
        void SaveChanges();
        Task SaveChangesAsync();
        internal void Add(Action a);

        public static IMongoUnitOfWork operator + (IMongoUnitOfWork u, Action a) 
        {
            u.Add(a);
            return u;
        }
    }
} 