using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Domain.Entities;

namespace Repository.MongoDB
{
    public static class Configuration
    {
        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services)
         where T : Entity =>
         services.AddScoped<IMongoDBRepository<T>, MongoDBRepository<T>>();

        public static IServiceCollection AddMongoRepository(this IServiceCollection services) =>
         services.AddScoped(typeof(IMongoDBRepository<>), typeof(MongoDBRepository<>));

        public static IServiceCollection AddMongoUnitOfWork(this IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            BsonSerializer.RegisterSerializer(typeof(object), new ObjectSerializer(ObjectSerializer.AllAllowedTypes));

            services.AddScoped<IMongoUnitOfWork, MongoDBUnitOfWork>();
            return services;
        }
    }
}