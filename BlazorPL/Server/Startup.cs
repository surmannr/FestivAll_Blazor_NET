using BL.AutoMappers;
using BL.InterfacesForManagers;
using BL.Managers;
using BlazorPL.Client;
using BlazorPL.Server.Areas.Identity;
using DAL;
using DAL.InterfacesForRepos;
using DAL.Models;
using DAL.Repositories;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using SharedLayer.Exceptions;
using Azure.Identity;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;
using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault.Certificates;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BlazorPL.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeSharedTokenCacheCredential = true });
            builder.AddAzureKeyVault(new Uri(@"https://festivall-keyvault.vault.azure.net/"), credential);
            configuration = builder.Build();
            Configuration = configuration;

        }
        public IConfiguration Configuration { get; }
        private string dbConnectionString = null;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //Console.WriteLine(configuration["MySecret"]);

            // Connection string lek�rdez�se a user secrets-b�l (secret.json)
            dbConnectionString = Configuration["ConnectionStrings-festivalldb"];

            // Adatb�zis be�ll�t�sa
            services.AddDbContext<FestivallDb>(options =>
                options.UseSqlServer(dbConnectionString));

            

            // Userkezel�s be�ll�t�sa
            services.AddDefaultIdentity<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
            }).AddRoles<IdentityRole>()
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<FestivallDb>()
              .AddClaimsPrincipalFactory<MyUserClaimsPrincipalFactory>();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.Configure<IdentityOptions>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 8;

                opts.SignIn.RequireConfirmedEmail = true;
            });

            services.AddIdentityServer().AddApiAuthorization<User, FestivallDb>(options =>
            {
                options.IdentityResources["openid"].UserClaims.Add("role");
            });

            services.AddAuthentication().AddIdentityServerJwt();

            services.AddHttpContextAccessor();
            #region Dependency Injection - Repository-khoz
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IOrderItemRepository, OrderItemRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            #endregion
            
            #region Dependency Injection - Manager-ekhez
            services.AddScoped<IEventManager, EventManager>();
            services.AddScoped<IOrderItemManager, OrderItemManager>();
            services.AddScoped<IOrderManager, OrderManager>();
            services.AddScoped<IPostManager, PostManager>();
            services.AddScoped<IReviewManager, ReviewManager>();
            services.AddScoped<ITicketManager, TicketManager>();
            services.AddScoped<IUserManager, UserManager>();
            #endregion

            #region Dependency Injection - AutoMapperekhez
            services.AddAutoMapper(typeof(EventProfile));
            services.AddAutoMapper(typeof(OrderProfile));
            services.AddAutoMapper(typeof(PostProfile));
            services.AddAutoMapper(typeof(TicketProfile));
            services.AddAutoMapper(typeof(ReviewProfile));
            services.AddAutoMapper(typeof(UserProfile));
            #endregion

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);
            #region Swagger
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FestivAll API",
                    Description = "�n�ll� laborat�rium 2021",
                    TermsOfService = new Uri("https://example.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "Surmann Roland",
                        Email = "surmannroland@gmail.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });
                // Set the comments path for the Swagger JSON and UI.
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });
            #endregion

            services.AddProblemDetails(ConfigureProblemDetails).AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);

            services.AddRouting();
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.AddRazorPages();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
           

            app.UseHttpsRedirection();
            app.UseIdentityServer();
            app.UseProblemDetails();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "FestivAll");
                c.RoutePrefix = "swagger";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private void ConfigureProblemDetails(ProblemDetailsOptions options)
        {
            // DbModelNullException -> NotFound : Ha nem tal�lja meg az adott entit�st, akkor NotFound-dal t�r vissza.
            options.MapToStatusCode<DbModelNullException>(StatusCodes.Status404NotFound);
            // DbModelParamsNullException -> BadRequest : Ha a k�telez� param�terek nincsenek kit�ltve (azaz null), akkor BadRequest-tel t�r vissza.
            options.MapToStatusCode<DbModelParamsNullException>(StatusCodes.Status400BadRequest);
            // ApplicationException -> InternalServerError : Ha nem siker�lt l�trehozni az entit�st.
            options.MapToStatusCode<DbUserCreationFailedException>(StatusCodes.Status500InternalServerError);
            // DbModelParamFormatException -> PreconditionFailed : Az adott valid�ci� nem siker�l, a felhaszn�l� �ltal be�rt adat nem megfelel�.
            options.MapToStatusCode<DbModelParamFormatException>(StatusCodes.Status412PreconditionFailed);
            options.MapToStatusCode<DbRequirementsException>(StatusCodes.Status412PreconditionFailed);

            options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
        }
    }
}
