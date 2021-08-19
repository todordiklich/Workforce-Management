using System;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

using WFM.DAL.Seeder;
using WFM.DAL.Context;
using WFM.BLL.Services;
using WFM.DAL.Entities;
using WFM.BLL.Interfaces;
using WFM.DAL.Repositories;
using WFM.DAL.Repositories.Interfaces;
using WFM.WEB.CustomAuthorization;
using WFM.BLL.Scheduler.HostedServices;
using WFM.BLL.Scheduler.Factories;
using WFM.BLL.Scheduler.Jobs;
using WFM.BLL.Scheduler;
using Quartz.Impl;
using Quartz;
using Quartz.Spi;
using Microsoft.AspNetCore.Authorization;
using WFM.WEB.CustomExceptionMiddleware;
using Microsoft.AspNetCore.Http;

namespace WFM.WEB
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WFMApp", Version = "v1" });

                // Adds the authorize button in swagger UI 
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                // Uses the token from the authorize input and sends it as a header
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                        Reference = new OpenApiReference
                            {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });

            // EF
            services.AddDbContext<AppEntityContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            // EF Identity
            services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+";
                options.SignIn.RequireConfirmedEmail = false;
            })
                    .AddRoles<IdentityRole<Guid>>()
                    .AddEntityFrameworkStores<AppEntityContext>()
                    .AddDefaultTokenProviders();

            #region Quartz
            //Register Quartz Scheduler
            // https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html#di-aware-job-factories

            var scheduler = StdSchedulerFactory.GetDefaultScheduler().GetAwaiter().GetResult();

            services.AddSingleton(scheduler);
            services.AddHostedService<QuartzHostedService>();
            services.AddSingleton<IJobFactory, QuartzJobFactory>();
            //services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionScopedJobFactory();

                var recalculateDaysOffJobkey = new JobKey("RecalculateDaysOffJob");
                q.AddJob<RecalculateDaysOffJob>(opts => opts.WithIdentity(recalculateDaysOffJobkey));
                q.AddTrigger(opts => opts
                    .ForJob(recalculateDaysOffJobkey)
                    .WithIdentity("RecalculateDaysOffTrigger")
                    //.WithCronSchedule("0 50 10 22 7 ? *")); Uncomment for demo
                    .WithCronSchedule("0 0 0 1 1 ? *"));
            });

            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });

            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });

            //register the cronjobs here
            services.AddTransient<RecalculateDaysOffJob>();
            //services.AddTransient<DeleteTimeOffRequestJob>();
            #endregion

            //Register Email configuration
            services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());

            // BLL - register all dependencies bellow
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<ITeamRepository, TeamRepository>();
            services.AddTransient<ITimeOffRequestRepository, TimeOffRequestRepository>();
            services.AddTransient<IApprovalRepository, ApprovalRepository>();
            services.AddTransient<IDaysOffLimitDefaultRepository, DaysOffLimitDefaultRepository>();

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<ITeamService, TeamService>();
            services.AddTransient<ITimeOffRequestService, TimeOffRequestService>();
            services.AddTransient<IApprovalService, ApprovalService>();
            services.AddTransient<IHolidayService, HolidayService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IDaysOffLimitDefaultService, DaysOffLimitDefaultService>();
            services.AddTransient<IDateTimeProvider, DateTimeProvider>();
            
            services.AddTransient<UserManager<User>>();
            services.AddTransient<RoleManager<IdentityRole<Guid>>>();

            services.AddTransient<IAuthorizationHandler, CreatorHandler>();

            // IdentityServer
            var builder = services.AddIdentityServer((options) =>
            {
                options.EmitStaticAudienceClaim = true;
            })
                                   .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
                                   .AddInMemoryClients(IdentityConfig.Clients);

            builder.AddDeveloperSigningCredential();
            builder.AddResourceOwnerValidator<PasswordValidator>();

            services
                .AddAuthorization(options =>
                {
                    options.AddPolicy("AdminOrRegularRoleName",
                         policy => policy.RequireRole("Admin", "Regular"));

                    options.AddPolicy("CreatorOrAdminOnly",
                                 policy => policy.Requirements.Add(new CreatorRequirement()));
                })
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })

                // Adds the JWT bearer token services that will authenticate each request based on the token in the Authorize header
                // and configures them to validate the token with the options
                .AddJwtBearer(options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.Audience = "https://localhost:5001/resources";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            DatabaseSeeder.Seed(app.ApplicationServices);               

            //Adds the Identityserver Middleware that will handle 
            app.UseIdentityServer();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WFMApp v1"));
            }

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseRouting();

            // Adds the auth middlewares
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
