using System.Linq.Expressions;
using MongoDB.Driver;
using Domain.Entities;

namespace Repository.MongoDB
{
    public static class MongoDbCommonMethods
    {
        public static UpdateDefinition<T> BuildUpdateDefinition<T>(object obj)
        {
            var b = new UpdateDefinitionBuilder<T>();
            return obj.GetType()
                .GetProperties()
                .Aggregate((UpdateDefinition<T>)null, (current, next) =>
                    current?.Set(next.Name, next.GetValue(obj)) ??
                    b.Set(next.Name, next.GetValue(obj)));
        }
    }

    internal sealed class MongoDBRepository<T> : IMongoDBRepository<T>
        where T : Entity
    {
        private readonly IMongoUnitOfWork _mongoUnitOfWork;
        private readonly IClientSessionHandle _session;
        private readonly IMongoCollection<T> _collection;

        public MongoDBRepository(IMongoDatabase database,
                               IClientSessionHandle session,
                               IMongoUnitOfWork mongoUnitOfWork,
                               MongoCollectionSettings cfg = null)
        {
            _mongoUnitOfWork = mongoUnitOfWork;
            _session = session;
            _collection = database.GetCollection<T>(typeof(T).Name.ToLower(), cfg);
        }

        public void Add(T entity) =>
            _mongoUnitOfWork.Add(() =>
                _collection.InsertOne(_session, entity));

        public void Add(IEnumerable<T> entities) =>
            _mongoUnitOfWork.Add(() =>
                _collection.InsertMany(_session, entities));

        public void Delete(T entity) =>
            _mongoUnitOfWork.Add(() =>
                _collection.DeleteOne(_session, x => x.Id == entity.Id));

        public void Delete(IEnumerable<T> entities) =>
            _mongoUnitOfWork.Add(() =>
                _collection.DeleteMany(_session,
                    new FilterDefinitionBuilder<T>().In(x => x.Id, entities.Select(y => y.Id))));

        public IQueryable<T> GetAll(Expression<Func<T, bool>> predicate) =>
            _collection.Find(predicate)
                       .ToList()
                       .AsQueryable();

        public IQueryable<R> Aggregate<R>(PipelineDefinition<T, R> pipeline) =>
            _collection.Aggregate(pipeline)
                       .ToList()
                       .AsQueryable();

        public IQueryable<T> GetAll() =>
            _collection.Find(FilterDefinition<T>.Empty)
                       .ToList()
                       .AsQueryable();

        public T GetFirstOrDefault(Expression<Func<T, bool>> predicate) =>
            _collection.Find(predicate).FirstOrDefault();

        public void Update(T entity) =>
            _mongoUnitOfWork.Add(() =>
                _collection.ReplaceOne(_session, x => x.Id == entity.Id, entity, new ReplaceOptions { IsUpsert = true }));

        public void Update(IEnumerable<T> entities) =>
            _mongoUnitOfWork.Add(() =>
                entities.Select(e =>
                    _collection.ReplaceOne(_session, x => x.Id == e.Id, e)));

        public bool AtomicUpdateMany(Expression<Func<T, bool>> predicate,
                                     object obj) =>
            _collection.UpdateMany(predicate,
                MongoDbCommonMethods.BuildUpdateDefinition<T>(obj)).ModifiedCount > 0;

        public bool AtomicUpdateMany(Expression<Func<T, bool>> predicate,
                                     UpdateDefinition<T> d) =>
            _collection.UpdateMany(predicate, d).ModifiedCount > 0;

        public T AtomicGetFirstAndUpdate(Expression<Func<T, bool>> predicate,
                                         object obj) =>
             _collection.FindOneAndUpdate(predicate,
                                          MongoDbCommonMethods.BuildUpdateDefinition<T>(obj));

        public long Count(Expression<Func<T, bool>> predicate) =>
            _collection.CountDocuments(predicate);

        public R AggregateFirstOrDefault<R>(PipelineDefinition<T, R> pipeline) =>
            _collection.Aggregate(pipeline).FirstOrDefault();

        public T AtomicGetFirstAndUpdate(Expression<Func<T, bool>> predicate, UpdateDefinition<T> d) =>
            _collection.FindOneAndUpdate(predicate, d);

        public bool AtomicUpdateOne(Expression<Func<T, bool>> predicate,
                                    UpdateDefinition<T> d) =>
            _collection.UpdateOne(predicate, d).ModifiedCount > 0;

        public void AtomicReplaceUpsert(Expression<Func<T, bool>> predicate, T entity) =>
            _collection.ReplaceOne(predicate, entity, new ReplaceOptions { IsUpsert = true });

        public void AtomicUpdateOrCreate(Expression<Func<T, bool>> predicate, object obj, T entity)
        {
            if (_collection.UpdateOne(predicate,
                    MongoDbCommonMethods.BuildUpdateDefinition<T>(obj)).ModifiedCount == 0)
                _collection.InsertOne(entity);
        }

        public bool AtomicUpdateOne(Expression<Func<T, bool>> predicate,
                                    object obj) =>
            _collection.UpdateOne(predicate,
                MongoDbCommonMethods.BuildUpdateDefinition<T>(obj)).ModifiedCount > 0;

        public bool AtomicDeleteMany(Expression<Func<T, bool>> predicate) =>
            _collection.DeleteMany(predicate).DeletedCount > 0;

        public void AtomicInsertOne(T entity) =>
            _collection.InsertOne(entity);
    }
} 