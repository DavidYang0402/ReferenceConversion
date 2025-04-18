using Microsoft.Extensions.Hosting;
using ReferenceConversion.Presentation;
using Microsoft.Extensions.DependencyInjection;
using ReferenceConversion.Applications.Interfaces;
using ReferenceConversion.Infrastructure.ConversionStrategies;
using ReferenceConversion.Domain.Interfaces;
using StrategyBasedConverter = ReferenceConversion.Infrastructure.ConversionStrategies.StrategyBasedConverter;
using ReferenceConversion.Modifier;
using ReferenceConversion.Infrastructure.Services;

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
            ApplicationConfiguration.Initialize();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
                {
                    // 1) Allowlist �޲z
                    services.AddSingleton<IAllowlistManager, AllowlistManager>();

                    // 2) .sln �קﾹ factory
                    services.AddSingleton<Func<string, ISlnModifier>>(sp =>
                        slnPath => new SlnModifier(slnPath));

                    // 3) �������U
                    services.AddSingleton<IReferenceConversionStrategy, ProjectToDllConverter>();
                    services.AddSingleton<IReferenceConversionStrategy, DllToProjectConverter>();

                    // 4) �����`���� (��@�F IReferenceConverter)
                    services.AddSingleton<IStrategyBasedConverter, StrategyBasedConverter>();

                    // 5) �ɮ� Processor
                    services.AddSingleton<CsprojFileProcessor>();

                    // 6) �̤W�h�� Form
                    services.AddSingleton<Form1>();
                })
                .Build();

            // �� DI �إ� Form1�A�ñҰ� WinForms ����
            var form = host.Services.GetRequiredService<Form1>();
            Application.Run(form);
        }
    }
}