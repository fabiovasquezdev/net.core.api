using System.Linq.Expressions;
using Domain.Entities;
using MongoDB.Driver;

namespace Repository.MongoDB
{
    public interface IMongoDBRepository<T>
        where T : Entity
    {
        IQueryable<T> GetAll();
        IQueryable<T> GetAll(Expression<Func<T, bool>> predicate);
        T GetFirstOrDefault(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Add(IEnumerable<T> entities);
        void Update(T entity);
        void Update(IEnumerable<T> entities);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);
        bool AtomicUpdateMany(Expression<Func<T, bool>> predicate, object obj);
        bool AtomicUpdateMany(Expression<Func<T, bool>> predicate, UpdateDefinition<T> d);
        bool AtomicUpdateOne(Expression<Func<T, bool>> predicate, UpdateDefinition<T> d);
        bool AtomicUpdateOne(Expression<Func<T, bool>> predicate, object obj);
        void AtomicUpdateOrCreate(Expression<Func<T, bool>> predicate, object obj, T entity);
        void AtomicReplaceUpsert(Expression<Func<T, bool>> predicate, T entity);
        void AtomicInsertOne(T entity);
        bool AtomicDeleteMany(Expression<Func<T, bool>> predicate);
        T AtomicGetFirstAndUpdate(Expression<Func<T, bool>> predicate, object obj);
        T AtomicGetFirstAndUpdate(Expression<Func<T, bool>> predicate, UpdateDefinition<T> d);
        IQueryable<R> Aggregate<R>(PipelineDefinition<T, R> pipeline);
        R AggregateFirstOrDefault<R>(PipelineDefinition<T, R> pipeline);
        long Count(Expression<Func<T, bool>> predicate);
    }
} 