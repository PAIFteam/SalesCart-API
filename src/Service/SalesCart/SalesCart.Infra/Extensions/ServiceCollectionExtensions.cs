using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SalesCart.Core.Domain.Interfaces;
using SalesCart.Infra.Data.Repositories;
using SalesCart.Core.Application.UseCases.SalesCartUser;
using StackExchange.Redis;
using SalesCart.Infra.Cache;
using SalesCart.Infra.Services;

namespace SalesCart.Infra.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfraestructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Registro do MediaR
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(
                    Assembly.GetExecutingAssembly(),
                    Assembly.GetAssembly(typeof(SalesCartUserUseCase))!
                    );

            });

            //Registro do Redis
            var redisConnectionString = configuration.GetConnectionString("Redis") 
                ?? throw new InvalidOperationException("Redis connection string not found in configuration.");
            var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisOptions));
            services.AddScoped<IRedisService, RedisService>();

            //Registro dos Repositorios
            services.AddScoped<ISalesCartUserRepository, SalesCartRepository>();
            services.AddScoped<ICuponRepository, CuponRepository>();

            //Registro dos Serviços de Validação
            services.AddScoped<ICuponValidationService, CuponValidationService>();

            //Registro dos UseCases
            services.AddScoped<SalesCartUserUseCase>();

            return services;
        }
    }
}
