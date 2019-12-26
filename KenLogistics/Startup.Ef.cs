using KenLogistics.Infrastructure.Configurations;
using KenLogistics.Infrastructure.IServices;
using KenLogistics.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KenLogistics.Web
{
    public partial class Startup
    {
        private void BindAndRegisterConfigurationSettings(IConfiguration configuration, IServiceCollection services)
        {
            var emailSettings = new EmailSettings();
            Configuration.Bind("EmailSettings", emailSettings);
            services.AddSingleton<IEmailSettings>(emailSettings);
        }

        public void DIServicesConfiguration(IServiceCollection services)
        {
            //Use Autofac
            services.AddHttpContextAccessor();

            //services.AddTransient<IPasswordValidator<AppUser>, CustomPasswordValidator>();
            //services.AddTransient<IUserValidator<AppUser>, CustomUserValidator>();
            

            services.AddTransient<IEmailService, EmailService>();

            services.AddTransient<IViewRenderService, ViewRenderService>();
        }
    }
}
