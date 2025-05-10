using System;
using System.Text.Json.Serialization;
using API.Application.Users;
using API.Application.Users.Ports;
using Confluent.Kafka;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Domain.Events;
using API.Application.Users.Consumers;
using MongoDB.Driver;
using Repository.MongoDB;
using Repository.Redis;
using API.Middleware;
using API.Application.Adapters.Ports;
using API.Application.Adapters.HttpClients;
using API.Application.Adapters.HttpClient.Configuration;
using StackExchange.Redis;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Domain.Entities;
using Common;

namespace API.Application;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services,
                                                       IConfiguration configuration) =>
        services.ConfigureCommonServices()
                .ConfigureSettings(configuration)
                .ConfigureHttpClient()
                .ConfigureRepositories(configuration)
                .ConfigureMasstransit(configuration)
                .ConfigureHangfire();

    private static IServiceCollection ConfigureCommonServices(this IServiceCollection services)
    {
        services.AddControllers()
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        services.AddScoped<IUserService, UserService>();
        services.AddSessionObservable();
        return services.AddContextUser();
    }

    private static IServiceCollection ConfigureSettings(this IServiceCollection services,
                                                              IConfiguration configuration) =>
        services
                .Configure<KeycloakSettings>(configuration.GetSection(nameof(KeycloakSettings)))
                .Configure<ViaCepSetting>(configuration.GetSection(nameof(ViaCepSetting)));

    private static readonly MongoCollectionSettings _mongoCollectionSettings = new()
    {
        ReadConcern = ReadConcern.Majority,
        WriteConcern = WriteConcern.WMajority
    };

    private static IServiceCollection ConfigureHttpClient(this IServiceCollection services) =>
        services.AddScoped<IAuthHttpClient, AuthHttpClient>()
                .AddScoped<IViaCepHttpClient, ViaCepHttpClient>();

    private static IServiceCollection ConfigureRepositories(this IServiceCollection services,
                                                           IConfiguration configuration)
    {
        services.AddSingleton<IMongoClient>(x => new MongoClient(configuration.GetValue<string>("ConnectionStrings:Mongo")))
                .AddSingleton(x => x.GetRequiredService<IMongoClient>().GetDatabase("net_api"))
                .AddScoped(x => x.GetRequiredService<IMongoClient>().StartSession())
                .AddSingleton(_mongoCollectionSettings)
                .AddMongoUnitOfWork()
                .AddMongoRepository<User>();

        services.AddSingleton<IConnectionMultiplexer>(x =>
            ConnectionMultiplexer.Connect(configuration.GetValue<string>("ConnectionStrings:Redis")))
                .AddScoped<IRedisRepository, RedisRepository>();

        return services;
    }


    private static IServiceCollection ConfigureHangfire(this IServiceCollection services)
    {
        services.AddHangfire((provider, config) =>
        {
            config.UseColouredConsoleLogProvider();
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseFilter(new AutomaticRetryAttribute
                  {
                      Attempts = 2,
                      DelayInSecondsByAttemptFunc = _ => 5,
                      OnAttemptsExceeded = AttemptsExceededAction.Delete
                  })
                  .UseMongoStorage(provider.GetRequiredService<IMongoClient>(), "hangfire", new MongoStorageOptions
                  {
                      MigrationOptions = new MongoMigrationOptions
                      {
                          MigrationStrategy = new MigrateMongoMigrationStrategy(),
                          BackupStrategy = new CollectionMongoBackupStrategy(),
                      },
                      CheckQueuedJobsStrategy = CheckQueuedJobsStrategy.Poll,
                      Prefix = "job",
                      CheckConnection = false
                  });
        });

        services.AddHangfireServer(options =>
        {
            options.ServerName = $"ProviderPortal {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")} Hangfire Server";
        });

        return services;
    }
    private static IServiceCollection ConfigureMasstransit(this IServiceCollection services,
                                                              IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserConsumer>();

            x.UsingInMemory((context, config) => config.ConfigureEndpoints(context));

            x.AddRider(rider =>
            {
                rider.AddProducer<UserCreated>(configuration);
                rider.AddProducer<UserChanged>(configuration);
                rider.AddProducer<UserStatusChanged>(configuration);

                rider.UsingKafka((context, k) =>
                {
                    k.Host(configuration.GetValue<string>("Kafka:Host"));

                    k.RegisterConsumer<UserConsumer, UserCreated>(context, configuration, "user-service");
                    k.RegisterConsumer<UserConsumer, UserChanged>(context, configuration, "user-service");
                    k.RegisterConsumer<UserConsumer, UserStatusChanged>(context, configuration, "user-service");
                });
            });
        });

        return services;
    }

    private static IRiderRegistrationConfigurator AddProducer<TMessage>(this IRiderRegistrationConfigurator rider,
                                                       IConfiguration configuration) where TMessage : class
    {
        rider.AddProducer<TMessage>(configuration[$"Kafka:{typeof(TMessage).Name}:Topic"]);
        return rider;
    }

    private static IRiderRegistrationConfigurator AddProducer<TKey, TMessage>(this IRiderRegistrationConfigurator rider,
                                                                                       IConfiguration configuration) where TMessage : class
    {
        rider.AddProducer<TKey, TMessage>(configuration[$"Kafka:{typeof(TMessage).Name}:Topic"]);
        return rider;
    }

    private static IKafkaFactoryConfigurator RegisterConsumer<TConsumer, TMessage>(this IKafkaFactoryConfigurator k,
                                                  IRiderRegistrationContext context,
                                                  IConfiguration configuration,
                                                  string groupId,
                                                  AutoOffsetReset autoOffsetReset = AutoOffsetReset.Latest)
        where TMessage : class
        where TConsumer : class, IConsumer<TMessage>
    {
        k.TopicEndpoint<TMessage>(configuration[$"Kafka:{typeof(TMessage).Name}:Topic"],
                                  groupId,
        e =>
        {
            e.ConfigureConsumer<TConsumer>(context);
            e.AutoOffsetReset = autoOffsetReset;
            e.ConcurrentConsumerLimit = 5;
            e.ConcurrentMessageLimit = 10;

            e.UseRawJsonSerializer();
            e.UseMessageRetry(r => r.Interval(10, TimeSpan.FromSeconds(5)));
            e.PartitionAssignmentStrategy = PartitionAssignmentStrategy.RoundRobin;

            e.CreateIfMissing(c =>
            {
                c.NumPartitions = configuration.GetValue<ushort>($"Kafka:{typeof(TMessage).Name}:NumPartitions");
                c.ReplicationFactor = configuration.GetValue<short>($"Kafka:{typeof(TMessage).Name}:ReplicationFactor");
            });
        });

        return k;
    }

}