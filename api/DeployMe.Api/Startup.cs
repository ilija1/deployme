using System.Collections.Generic;
using DeployMe.Extensions.Json.Converters;
using DeployMe.Http.WebApiExtensions.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Newtonsoft;

namespace DeployMe.Api
{
    public class Startup
    {
        public const string Component = "agents-api";

        public Startup(IConfiguration configuration)
        {
            JsonConvert.DefaultSettings = () => GetDefaultJsonSerializerSettings();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        public void ConfigureServices(IServiceCollection services)
        {
//            services.AddCors(
//                options =>
//                {
//                    options.AddPolicy(
//                        "AllowOrigin",
//                        builder =>
//                            builder
//                                .AllowAnyMethod()
//                                .AllowAnyHeader()
//                                .AllowAnyOrigin());
//                });

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options => GetDefaultJsonSerializerSettings(options.SerializerSettings));

            // Inject cache client
            services.AddSingleton(_ => Configuration.GetSection(nameof(RedisConfiguration)).Get<RedisConfiguration>());
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
            services.AddSingleton<ISerializer, NewtonsoftSerializer>();
            services.AddSingleton(i => i.GetRequiredService<IRedisCacheClient>().GetDbFromConfiguration());

            // Inject log delegate
            services.RegisterLogDelegate(Component);
        }

        public static JsonSerializerSettings GetDefaultJsonSerializerSettings(JsonSerializerSettings existingSettings = null)
        {
            if (existingSettings == null)
            {
                existingSettings = new JsonSerializerSettings();
            }

            existingSettings.ContractResolver = new DefaultContractResolver();
            existingSettings.Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
                new ExceptionConverter()
            };
            return existingSettings;
        }
    }
}
