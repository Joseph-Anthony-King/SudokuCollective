using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Hangfire;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerUI;
using SudokuCollective.Api.Filters;
using SudokuCollective.Api.Middleware;
using SudokuCollective.Api.Models;
using SudokuCollective.Cache;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Jobs;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Jobs;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Authentication;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Services;
using SudokuCollective.Heroku;
using SudokuCollective.Repos;
using Role = SudokuCollective.Core.Models.Role;

namespace SudokuCollective.Api
{
    /// <summary>
    /// Startup Class
    /// </summary>
    /// <remarks>
    /// Startup Class Constructor
    /// </remarks>
    /// <param name="configuration"></param>
    /// <param name="environment"></param>
    public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
		private readonly IWebHostEnvironment _environment = environment;
		private ILogger<Startup> _logger;

        /// <summary>
        /// Startup Class Configuration
        /// </summary>
        public IConfiguration Configuration { get; } = configuration;

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
		{
			// Add logger to ConfigureServices to aid in debugging remote hosts...
			using var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Information);
				builder.AddConsole();
				builder.AddEventSourceLogger();
			});

			_logger = loggerFactory.CreateLogger<Startup>();

			_logger.LogInformation(message: string.Format("Initiating service configuration in {0} environment...", _environment.EnvironmentName));

			services.AddDbContext<DatabaseContext>(options =>
			{
				options.UseNpgsql(
					_environment.IsDevelopment() ?
						Configuration.GetConnectionString("DatabaseConnection") :
						HerokuIntegration.GetHerokuPostgresConnectionString(),
					b => b.MigrationsAssembly("SudokuCollective.Api"));
				options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			});

			var swaggerDescription = _environment.IsDevelopment() ?
				Configuration.GetSection("MissionStatement").Value :
				Environment.GetEnvironmentVariable("MISSIONSTATEMENT");

			var sandboxLicense = _environment.IsDevelopment() ?
				Configuration.GetSection("DefaultSandboxApp:License").Value :
				Environment.GetEnvironmentVariable("SANDBOX_APP:LICENSE");

			services.AddSwaggerGen(swagger =>
			{
				swagger.SwaggerDoc(
					"v1",
					new OpenApiInfo
					{
						Version = "v1",
						Title = "SudokuCollective API",
						Description = string.Format("{0} \r\n\r\n For testing purposes please use the Sudoku Collective Sandbox App if you haven't created your own app: \r\n\r\n Id: 3 \r\n\r\n  License: {1}",
							swaggerDescription,
							sandboxLicense)
					});
				swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
				{
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\""
				});
				swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
					{
						{
							new OpenApiSecurityScheme
							{
								Reference = new OpenApiReference
								{
									Type = ReferenceType.SecurityScheme,
									Id = "Bearer"
								}
							},
						Array.Empty<string>()
					}
				});
				swagger.MapType(typeof(JsonElement), () => new OpenApiSchema
				{
					Type = "object",
					Example = new OpenApiString("{}")
				});

				var swashbucklePayloadArray = new OpenApiArray
                {
                    new OpenApiObject()
                };

				swagger.MapType(typeof(List<object>), () => new OpenApiSchema
				{
					Type = "array",
					Example = swashbucklePayloadArray
				});

				swagger.DocumentFilter<ErrorControllerFilter>();
				swagger.DocumentFilter<PathLowercaseDocumentFilter>();

				// Add domain model documentation to Swashbuckler
				swagger.DocumentFilter<CustomModelDocumentFilter<App>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<UserDTO>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<Difficulty>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<GalleryApp>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<Game>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<Role>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<SMTPServerSettings>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<SudokuCell>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<SudokuMatrix>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<SudokuSolution>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<AppPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<CreateDifficultyPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<UpdateDifficultyPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<CreateGamePayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<GamePayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<GamesPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<LicensePayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<CreateRolePayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<UpdateRolePayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<AddSolutionsPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<SolutionPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<PasswordResetPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<RequestPasswordResetPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<UpdateUserPayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<UpdateUserRolePayload>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<AnnonymousGameRequest>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<UpdatePasswordRequest>>();
				swagger.DocumentFilter<CustomModelDocumentFilter<ConfirmEmailResult>>();

				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var filePath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				swagger.IncludeXmlComments(filePath);
			});

			// Add redis cache
			ConfigurationOptions options;

			string cacheConnectionString = "";

			if (_environment.IsDevelopment())
			{
				options = ConfigurationOptions.Parse(Configuration.GetConnectionString("CacheConnection"));

				cacheConnectionString = Configuration.GetConnectionString("CacheConnection");
			}
			else
            {
				options = HerokuIntegration.GetHerokuRedisConfigurationOptions();

				cacheConnectionString = options.ToString();
			}

			services.AddSingleton<Lazy<IConnectionMultiplexer>>(sp =>
				new Lazy<IConnectionMultiplexer>(() =>
				{
					return ConnectionMultiplexer.Connect(options);
				}));

			services.AddStackExchangeRedisCache(redisOptions =>
			{
				redisOptions.InstanceName = "SudokuCollective";
				redisOptions.Configuration = cacheConnectionString;
				redisOptions.ConfigurationOptions = options;
				redisOptions.ConnectionMultiplexerFactory = () =>
				{
					var serviceProvider = services.BuildServiceProvider();
					Lazy<IConnectionMultiplexer> connection = serviceProvider.GetService<Lazy<IConnectionMultiplexer>>();
					return Task.FromResult(connection.Value);
				};
			});

			// Add Hangfire services.
			services.AddHangfire(configuration => configuration
				.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
				.UseSimpleAssemblyNameTypeSerializer()
				.UseRecommendedSerializerSettings()
				.UseInMemoryStorage());

			// Add the processing server as IHostedService
			services.AddHangfireServer();

			// Add JWT Token authentication
			var tokenManagement = _environment.IsDevelopment() ?
					Configuration.GetSection("tokenManagement").Get<TokenManagement>() :
					new TokenManagement
					{
						Secret = Environment.GetEnvironmentVariable("TOKEN:SECRET"),
						Issuer = Environment.GetEnvironmentVariable("TOKEN:ISSUER"),
						Audience = Environment.GetEnvironmentVariable("TOKEN:AUDIENCE"),
						AccessExpiration = Convert.ToInt32(Environment.GetEnvironmentVariable("TOKEN:ACCESS_EXPIRATION")),
						RefreshExpiration = Convert.ToInt32(Environment.GetEnvironmentVariable("TOKEN:REFRESH_EXPIRATION"))
					};

			var secret = Encoding.ASCII.GetBytes(tokenManagement.Secret);

			services.AddSingleton<ITokenManagement>(tokenManagement);

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(x =>
			{
				x.RequireHttpsMetadata = false;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(secret),
					ValidIssuer = tokenManagement.Issuer,
					ValidAudience = tokenManagement.Audience,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					LifetimeValidator = LifetimeValidator,
				};
				x.Events = new JwtBearerEvents
				{
					OnAuthenticationFailed = context =>
					{
						_logger.LogInformation(context.Exception.GetType().ToString());
						if (context.Exception.GetType() == typeof(SecurityTokenInvalidLifetimeException))
						{
							context.Response.Headers.Append("Token-Expired", "true");
                        }

						return Task.CompletedTask;
					}
				};
			});

			services.AddMvc(options => options.EnableEndpointRouting = false);

			services.AddControllers()
					.AddJsonOptions(x =>
					{
						x.JsonSerializerOptions.AllowTrailingCommas = true;
						x.JsonSerializerOptions.IncludeFields = false;
						x.JsonSerializerOptions.IgnoreReadOnlyProperties = false;
						x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
					});
			services.AddSingleton<ICacheKeys, CacheKeys>();
			services.AddSingleton<ICachingStrategy, CachingStrategy>();

			services.AddScoped<IDataJobs, DataJobs>();
			services.AddScoped<IAppsRepository<App>, AppsRepository<App>>();
			services.AddScoped<IUsersRepository<User>, UsersRepository<User>>();
			services.AddScoped<IAppAdminsRepository<AppAdmin>, AppAdminsRepository<AppAdmin>>();
			services.AddScoped<IGamesRepository<Game>, GamesRepository<Game>>();
			services.AddScoped<IDifficultiesRepository<Difficulty>, DifficultiesRepository<Difficulty>>();
			services.AddScoped<IRolesRepository<Role>, RolesRepository<Role>>();
			services.AddScoped<ISolutionsRepository<SudokuSolution>, SolutionsRepository<SudokuSolution>>();
			services.AddScoped<IEmailConfirmationsRepository<EmailConfirmation>, EmailConfirmationsRepository<EmailConfirmation>>();
			services.AddScoped<IPasswordResetsRepository<Core.Models.PasswordReset>, PasswordResetsRepository<Core.Models.PasswordReset>>();
			services.AddScoped<IAuthenticateService, AuthenticateService>();
			services.AddScoped<IUserManagementService, UserManagementService>();
			services.AddScoped<IAppsService, AppsService>();
			services.AddScoped<IUsersService, UsersService>();
			services.AddScoped<IGamesService, GamesService>();
			services.AddScoped<IDifficultiesService, DifficultiesService>();
			services.AddScoped<IRolesService, RolesService>();
			services.AddScoped<ISolutionsService, SolutionsService>();
			services.AddScoped<IEmailService, EmailService>();
			services.AddScoped<ICacheService, CacheService>();
			services.AddScoped<IRequestService, RequestService>();
			services.AddScoped<IValuesService, ValuesService>();

			var emailMetaData = _environment.IsDevelopment() ?
					Configuration.GetSection("emailMetaData").Get<EmailMetaData>() :
					new EmailMetaData
					{
						SmtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER:SERVER"),
						Port = Convert.ToInt32(Environment.GetEnvironmentVariable("SMTP_SERVER:PORT")),
						UserName = Environment.GetEnvironmentVariable("SMTP_SERVER:USERNAME"),
						Password = Environment.GetEnvironmentVariable("SMTP_SERVER:PASSWORD"),
						FromEmail = Environment.GetEnvironmentVariable("SMTP_SERVER:FROM_EMAIL")
					};

			services.AddSingleton<IEmailMetaData>(emailMetaData);

			services.AddHttpContextAccessor();
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline...
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseCors(x => x.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader());

			app.UseHttpsRedirection();

			app.UseExceptionHandler("/error");

			app.UseRouting();

			app.UseAuthentication();

			app.UseMiddleware<ExpiredTokenMiddleware>();

			app.UseSwagger();

			app.UseSwaggerUI(swaggerUI =>
			{
				var swaggerTitle = "SudokuCollective API v1";
				swaggerUI.DocumentTitle = swaggerTitle;
				swaggerUI.SwaggerEndpoint("/swagger/v1/swagger.json", swaggerTitle);
				swaggerUI.DocExpansion(DocExpansion.None);
			});

			// Initialize and set the path for the welcome page saved in wwwroot
			DefaultFilesOptions defaultFile = new DefaultFilesOptions();
			defaultFile.DefaultFileNames.Clear();
			defaultFile.DefaultFileNames.Add("index.html");

			app.UseDefaultFiles(defaultFile);

			app.UseHangfireDashboard();

			app.UseStaticFiles();

			app.UseMvc();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHangfireDashboard();
			});

			app.Use(async (context, next) =>
			{
				context.Request.EnableBuffering();

				await next();
			});

			SeedData.EnsurePopulated(
					app,
					Configuration,
					env);
		}

		private bool LifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken token, TokenValidationParameters @params)
		{
			if (expires != null)
			{
				return expires > DateTime.UtcNow;
			}
			return false;
		}
	}
}
