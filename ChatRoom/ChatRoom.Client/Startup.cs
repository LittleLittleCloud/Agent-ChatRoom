using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatRoom.Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ChatRoomClientController>();
            services.AddControllers();
            services.AddMvc(option => option.EnableEndpointRouting = false);
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
#if DEBUG
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
#endif

            app.UseMvc();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
        }
    }
}
