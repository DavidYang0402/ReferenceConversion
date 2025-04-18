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
                    // 1) Allowlist 管理
                    services.AddSingleton<IAllowlistManager, AllowlistManager>();

                    // 2) .sln 修改器 factory
                    services.AddSingleton<Func<string, ISlnModifier>>(sp =>
                        slnPath => new SlnModifier(slnPath));

                    // 3) 策略註冊
                    services.AddSingleton<IReferenceConversionStrategy, ProjectToDllConverter>();
                    services.AddSingleton<IReferenceConversionStrategy, DllToProjectConverter>();

                    // 4) 策略總指揮 (實作了 IReferenceConverter)
                    services.AddSingleton<IStrategyBasedConverter, StrategyBasedConverter>();

                    // 5) 檔案 Processor
                    services.AddSingleton<CsprojFileProcessor>();

                    // 6) 最上層的 Form
                    services.AddSingleton<Form1>();
                })
                .Build();

            // 由 DI 建立 Form1，並啟動 WinForms 應用
            var form = host.Services.GetRequiredService<Form1>();
            Application.Run(form);
        }
    }
}