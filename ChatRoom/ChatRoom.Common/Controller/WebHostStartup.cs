using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ChatRoom.SDK;

internal class WebHostStartup
{
    public WebHostStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
                    .AddMvcOptions(options =>
                    {
                        options.EnableEndpointRouting = false;
                    })
                    .ConfigureApplicationPartManager(manager =>
                    {
                        manager.FeatureProviders.Add(new ChatRoomClientControllerProvider());
                    });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatRoom.Server", Version = "v1" });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChatRoom.Server v1");
            });
            app.UseDeveloperExceptionPage();
        }
        // enable cors from the same origin
        app.UseCors(builder =>
        {
            builder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
        app.UseMvc();
        app.UseHttpsRedirection();
        app.UseDefaultFiles();
        app.UseStaticFiles();
    }
}
