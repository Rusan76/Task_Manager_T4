using System;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console;
using Task_Manager_T4;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;
using System.Linq;
class WindowsOptimization : Other
{
    private static readonly Dictionary<string, string> visualEffects = new()
    {
        { "Animation", "DisableAnimations" },
        { "Aero Peek", "DisableAeroPeek" },
        { "Transparency", "DisableTransparency" },
        { "Shadows", "DisableShadow" },
        { "Thumbnail previews", "DisableThumbnailPreviews" },
        { "Smooth scrolling", "DisableSmoothScrolling" },
        { "Cursor shadow", "DisableCursorShadow" }
    };
    private static readonly Dictionary<string, string> AppsToRemove = new()
    {
        { "*Skype*", "Skype" },
        { "*ZuneMusic*", "Groove Music" },
        { "*ZuneVideo*", "Кино и ТВ" },
        { "*Xbox*", "Сервисы Xbox" },
        { "*OneNote*", "OneNote" },
        { "*OfficeHub*", "Get Office" },
        { "*BingWeather*", "Погода" },
        { "*YourPhone*", "Ваш телефон (Связь с телефоном)" },
        { "*CommunicationsApps*", "Почта и Календарь" },
        { "*3dbuilder*", "3D Builder" },
        { "*solitaire*", "Microsoft Solitaire Collection" },
        { "*maps*", "Карты"},
        { "*camera*", "Камера" },
        { "*alamrs*", "Будильник и часы" },
        { "*onenote*", "OneNote"},
        { "*bing*", "Приложения Новости, спорт, погода, финансы (все сразу)"},
        { "*soundrecorder *", "Запись голоса"}
    };
    private static readonly Dictionary<string, string> ServicesToDisable = new()
    {

        { "Fax", "Факс" },
        { "RemoteRegistry", "Удаленный реестр" },
        { "TermService", "Служба виртуализации удаленных рабочих столов" },
        { "NetTcpPortSharing", "Служба общего доступа к портам Net.Tcp" },
        { "SecondaryLogon", "Вторичный вход в систему" },
        { "WorkFoldersSVC", "Рабочие папки" },
        { "Browser", "Браузер компьютеров" },


        { "NvStStereoSvc", "NVIDIA Stereoscopic 3D Driver Service" },
        { "bthserv", "Служба поддержки Bluetooth" },
        { "Spooler", "Диспетчер печати" },
        { "SysMain", "Superfetch (SysMain)" },


        { "DiagTrack", "Функциональные возможности для пользователей (Телеметрия)" },
        { "dmwappushservice", "Dmwappushservice (WAP Push)" },
        { "lfsvc", "Служба географического положения" },


        { "SensorDataService", "Служба данных датчиков" },
        { "SensorService", "Служба датчиков" },
        { "SensrSvc", "Служба наблюдения за датчиками" },


        { "XblAuthManager", "Сетевая служба Xbox Live" },
        { "ClipSVC", "Служба лицензий клиента (ClipSVC)" },


        { "vmickvpexchange", "Служба обмена данными (Hyper-V)" },
        { "vmicguestinterface", "Служба завершения работы в качестве гостя (Hyper-V)" },
        { "vmicheartbeat", "Служба пульса (Hyper-V)" },
        { "vmicrdv", "Служба виртуализации удаленных рабочих столов Hyper-V" },
        { "vmictimesync", "Служба синхронизации времени Hyper-V" },
        { "vmicvmsession", "Служба сеансов виртуальных машин Hyper-V" },


        { "SharedAccess", "Общий доступ к подключению к Интернету (ICS)" },
        { "AllJoynRouter", "Служба маршрутизатора AllJoyn" },
        { "AppID", "Удостоверение приложения" },
        { "BDESVC", "Служба шифрования дисков BitLocker" },
        { "WBioSrvc", "Биометрическая служба Windows" }
    };

    private readonly static string[] tempPaths =
                [
                    Path.GetTempPath(),
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Temp",
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Temp",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\Prefetch")
                ];
    public static void MainMenuOptimization()
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Оптимизация Windows[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{GraphicSettings.SecondaryColor}]Выберите действие:[/]")
                    .AddChoices([
                        "Чистка временных файлов",
                        "Оптимизация служб (Отключить лишнее)",
                        "Откат служб (Вернуть как было)",
                        "Полная очистка автозагрузки",
                        "Удаление встроенных системных программ (OneDrive, Skype etc)",
                        "Сброс сети и DNS",
                        "Очистка системных журналов",
                        "Отключение визуальных эффектов",
                        "Максимальная производительность",
                        "Назад"
                    ]));

            switch (choice)
            {
                case "Удаление встроенных системных программ (Кино, Skype etc)":
                    UninstallSystemApps();
                    Console.Clear();
                    break;
                case "Отключение визуальных эффектов":
                    DisableVisualEffects();
                    Console.Clear();
                    break;
                case "Полная очистка автозагрузки":
                    RemoveAllStartupPrograms();
                    Console.Clear();
                    break;
                case "Чистка временных файлов":
                    CleanTempFiles();
                    Console.Clear();
                    break;
                case "Оптимизация служб (Отключить лишнее)":
                    if (AnsiConsole.Confirm($"[{GraphicSettings.AccentColor}]Вы уверены?[/][{GraphicSettings.SecondaryColor}] Это отключит Bluetooth, Печать и др. службы.[/]"))
                        SetServicesState(true);
                    Console.Clear();
                    break;
                case "Откат служб (Вернуть как было)":
                    SetServicesState(false);
                    AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Службы переведены в режим 'Вручную'. Перезагрузите ПК.[/]");
                    Console.Clear();
                    break;
                case "Сброс сети и DNS":
                    ResetNetworkStack();
                    Console.Clear();
                    break;
                case "Очистка системных журналов":
                    ClearEventLogs();
                    Console.Clear();
                    break;
                case "Максимальная производительность(Энергосбережение)":
                    EnableUltimatePerformance();
                    Console.Clear();
                    break;
                case "Назад":
                    return;
            }
        }
    }

    public static void UninstallSystemApps()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]UNINSTALL_SYSTEM_APPS[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        var appsToDelete = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title($"Выберите приложения для [{GraphicSettings.AccentColor}]удаления[/] (Пробел - выбор, Enter - подтверждение):")
                .NotRequired()
                .AddChoices(AppsToRemove.Values));

        if (appsToDelete.Count == 0) return;

        AnsiConsole.Status().Start("Удаление выбранных приложений...", ctx =>
        {
            foreach (var appFriendlyName in appsToDelete)
            {
                var appInternalName = AppsToRemove.FirstOrDefault(x => x.Value == appFriendlyName).Key;

                ctx.Status($"Удаление {appFriendlyName}...");

                string psCommand = $"Get-AppxPackage {appInternalName} | Remove-AppxPackage";

                ExecutePowerShell(psCommand);

                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][[OK]][/] {appFriendlyName} удален.");
            }
        });

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Готово. Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    public static void DisableVisualEffects()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]DISABLE_VISUAL_EFFECTS[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());

        try
        {
            var effectsToDisable = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title($"[{GraphicSettings.SecondaryColor}]Выберите эффекты для отключения:[/]")
                    .NotRequired()
                    .AddChoices(visualEffects.Keys));

            if (effectsToDisable.Count > 0)
            {
                foreach (var effect in effectsToDisable)
                {
                    string regPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects";
                    using (RegistryKey key = Registry.CurrentUser.CreateSubKey(regPath))
                    {
                        key?.SetValue(visualEffects[effect], 0, RegistryValueKind.DWord);
                    }
                    AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][✓][/] {effect} отключен");
                }

                ExecuteCommand("taskkill.exe", "/f /im explorer.exe");
                ExecuteCommand("explorer.exe", "");

                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]Визуальные эффекты отключены![/]");
            }

            AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Ошибка отключения визуальных эффектов: {ex.Message}[/]");
            AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
            Console.ReadKey();
        }
    }
    public static void RemoveAllStartupPrograms()
    {
        string[] registryKeys = [
            @"Software\Microsoft\Windows\CurrentVersion\Run",
            @"Software\Microsoft\Windows\CurrentVersion\RunOnce"
        ];

        foreach (string keyPath in registryKeys)
        {

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
            {
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        key.DeleteValue(valueName);
                    }
                }
            }
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
            {
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        key.DeleteValue(valueName);
                    }
                }
            }
        }
        string[] startupFolders = [
            Environment.GetFolderPath(Environment.SpecialFolder.Startup),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
        ];
        foreach (string folder in startupFolders)
        {
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder);
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Ошибка удаления {file}:[/] [{GraphicSettings.AccentColor}]{ex.Message} [/]");
                    }
                }
            }
        }
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Press any key to return.[/]");
        Console.ReadKey();
    }
    public static void EnableUltimatePerformance()
    {
        string schemaId = "e9a42b02-d5df-448d-aa00-03f14749eb61";

        AnsiConsole.Status().Start("Активация режима производительности...", ctx =>
        {

            ExecuteCommand("powercfg", $"-duplicatescheme {schemaId}");


            ExecuteCommand("powercfg", $"-setactive {schemaId}");

            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}] Режим 'Максимальная производительность' активирован! [/]");
        });
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Press any key to return.[/]");
        Console.ReadKey();
    }

    public static void ClearEventLogs()
    {
        AnsiConsole.Status().Start("Очистка журналов событий...", ctx =>
        {

            var eventLogs = EventLog.GetEventLogs();
            foreach (var log in eventLogs)
            {
                try
                {
                    ctx.Status($"Очистка журнала: {log.LogDisplayName}...");
                    log.Clear();
                }
                catch { }
            }
            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]Все доступные системные журналы очищены. [/]");
        });
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Press any key to return.[/]");
        Console.ReadKey();
    }

    public static void ResetNetworkStack()
    {
        AnsiConsole.Status().Start("Сброс сетевого стека...", ctx =>
        {

            ctx.Status("Очистка DNS-кэша...");
            ExecuteCommand("ipconfig", "/flushdns");


            ctx.Status("Сброс Winsock...");
            ExecuteCommand("netsh", "winsock reset");


            ctx.Status("Сброс стека TCP/IP...");
            ExecuteCommand("netsh", "int ip reset");

            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}] Сетевые настройки сброшены. Рекомендуется перезагрузка. [/]");
        });
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Press any key to return.[/]");
        Console.ReadKey();
    }

    private static void SetServicesState(bool disable)
    {
        Console.Clear();
        string actionName = disable ? "ОТКЛЮЧЕНИЕ" : "ВОССТАНОВЛЕНИЕ";
        string scState = disable ? "disabled" : "demand";

        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]{actionName}_СЛУЖБ[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());

        foreach (var service in ServicesToDisable)
        {
            try
            {
                ProcessStartInfo psi = new("sc", $"config {service.Key} start= {scState}")
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(psi).WaitForExit();

                if (disable)
                {
                    using ServiceController sc = new(service.Key);
                    if (sc.Status != ServiceControllerStatus.Stopped && sc.CanStop)
                    {
                        sc.Stop();
                    }
                }

                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][[OK]][/] {service.Value}");
            }
            catch
            {
                AnsiConsole.MarkupLine($"[red][[ERROR]][/] Не удалось обработать {service.Value}");
            }
        }
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Press any to return[/]");
        Console.ReadKey();
    }

    private static void CleanTempFiles()
    {
        Console.Clear();
        long totalCleaned = 0;
        if (!IsRunningAsAdmin())
        {
            ShowAdminWarning();
            return;
        }
        foreach (var path in tempPaths)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        FileInfo fi = new(files[i]);
                        long fileSize = fi.Length;
                        fi.Delete();
                        totalCleaned += fileSize;
                    }
                    catch
                    {
                        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Не удалось удалить файл, возможно, используется другой программой.[/]");
                    }
                }
            }
        }
        double cleanedMB = totalCleaned / (1024.0 * 1024.0);
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Было очищено[/][{GraphicSettings.AccentColor}] {cleanedMB}[/][{GraphicSettings.SecondaryColor}] MB файлов[/]");
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Press any key to return.[/]");
        Console.ReadKey();
    }

    private static void ExecuteCommand(string fileName, string args)
    {
        try
        {
            ProcessStartInfo psi = new(fileName, args)
            {
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
            Process.Start(psi)?.WaitForExit();
        }
        catch { }
    }
    private static void ExecutePowerShell(string command)
    {
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
            Process.Start(psi)?.WaitForExit();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][[ERR]][/] Ошибка PS: {ex.Message}");
        }
    }
}