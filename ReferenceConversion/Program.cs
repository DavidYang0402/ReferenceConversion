using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using ReferenceConversion.Presentation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReferenceConversion.Applications.Interfaces;
using ReferenceConversion.Infrastructure.ConversionStrategies;
using ReferenceConversion.Domain.Interfaces;
using ReferenceConverter = ReferenceConversion.Infrastructure.ConversionStrategies.ReferenceConverter;
using ReferenceConversion.Applications.Service;

namespace ReferenceConversion
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var host = CreateHostBuilder().Build();
            var form = host.Services.GetRequiredService<Form1>();
            Application.Run(form);
        }

        static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // ���U�ഫ����
                    services.AddSingleton<IReferenceConversionStrategy, DllToProjectConverter>();
                    services.AddSingleton<IReferenceConversionStrategy, ProjectToDllConverter>();
                    services.AddSingleton<IReferenceConvertionService, ReferenceConvertionService>();
                    services.AddSingleton<CsprojFileProcessor>();

                    // ���U�D�A��
                    services.AddSingleton<IReferenceConverter, ReferenceConverter>();

                    // ���U���
                    services.AddSingleton<Form1>();
                });
        }
    }
}