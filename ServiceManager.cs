using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Spectre.Console;
namespace ProjectT4;
public class ServiceManagerUI
{
    public static void ShowServicesMenu()
    {
        while (true)
        {
            Console.Clear();
            
            AnsiConsole.Write(
                new FigletText("Service Manager")
                    .Centered()
                    .Color(Color.Red));
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Service Management[/]")
                    .PageSize(12)
                    .AddChoices([
                        "📋 List All Services",
                        "⚡ List Running Services",
                        "💤 List Stopped Services",
                        "🚀 Start Service",
                        "⏹️ Stop Service",
                        "🔄 Restart Service",
                        "⚙️ Change Startup Type",
                        "🔍 Search Service",
                        "📊 Service Dependencies",
                        "📈 Service Statistics",
                        "🔙 Back to Main Menu"
                    ]));
            
            switch (choice)
            {
                case "📋 List All Services":
                    ShowAllServices();
                    break;
                case "⚡ List Running Services":
                    ShowRunningServices();
                    break;
                case "💤 List Stopped Services":
                    ShowStoppedServices();
                    break;
                case "🚀 Start Service":
                    StartService();
                    break;
                case "⏹️ Stop Service":
                    StopService();
                    break;
                case "🔄 Restart Service":
                    RestartService();
                    break;
                case "⚙️ Change Startup Type":
                    ChangeStartupType();
                    break;
                case "🔍 Search Service":
                    SearchService();
                    break;
                case "📊 Service Dependencies":
                    ShowServiceDependencies();
                    break;
                case "📈 Service Statistics":
                    ShowServiceStatistics();
                    break;
                case "🔙 Back to Main Menu":
                    return;
            }
            
            AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
            Console.ReadKey();
        }
    }
    
    private static void ShowAllServices()
    {
        try
        {
#pragma warning disable CA1416
            var services = ServiceController.GetServices();
#pragma warning restore CA1416
            
            var table = new Table()
                .Title($"[bold red]Windows Services ({services.Length})[/]")
                .BorderColor(Color.Red)
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[cyan]Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Status[/]").Centered())
                .AddColumn(new TableColumn("[cyan]Type[/]").Centered())
                .AddColumn(new TableColumn("[cyan]Can Stop[/]").Centered());

#pragma warning disable CA1416 // Проверка совместимости платформы
            foreach (var service in services.OrderBy(s => s.ServiceName))
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                string status = GetStatusColor(service.Status);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                string type = GetServiceType(service.ServiceType);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                string canStop = service.CanStop ? "[green]YES[/]" : "[red] NO[/]";
#pragma warning restore CA1416 // Проверка совместимости платформы

#pragma warning disable CA1416 // Проверка совместимости платформы
                table.AddRow(
                    $"[white]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[grey]{Ellipsis(service.DisplayName, 30)}[/]",
                    status,
                    $"[yellow]{type}[/]",
                    canStop
                );
#pragma warning restore CA1416 // Проверка совместимости платформы
            }

            AnsiConsole.Write(table);
            
            // Показываем статистику
            ShowServiceCounts(services);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to get services: {ex.Message}");
        }
    }
    
    private static void ShowRunningServices()
    {
        try
        {
#pragma warning disable CA1416
            var runningServices = ServiceController.GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Running)
                .OrderBy(s => s.ServiceName);
#pragma warning restore CA1416
            
#pragma warning disable CA1416 // Проверка совместимости платформы
            var table = new Table()
                .Title($"[bold green]Running Services ({runningServices.Count()})[/]")
                .BorderColor(Color.Green)
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[cyan]Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Startup Type[/]").Centered())
                .AddColumn(new TableColumn("[cyan]Memory[/]").RightAligned());
#pragma warning restore CA1416 // Проверка совместимости платформы
            
            foreach (var service in runningServices)
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                table.AddRow(
                    $"[white]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[grey]{Ellipsis(service.DisplayName, 30)}[/]",
                    $"[yellow]{GetStartupType(service.ServiceName)}[/]",
                    $"[cyan]{GetServiceMemoryUsage(service.ServiceName):N0} KB[/]"
                );
#pragma warning restore CA1416 // Проверка совместимости платформы
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to get running services: {ex.Message}");
        }
    }
    
    private static void ShowStoppedServices()
    {
        try
        {
#pragma warning disable CA1416
            var stoppedServices = ServiceController.GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Stopped)
                .OrderBy(s => s.ServiceName);
#pragma warning restore CA1416
            
#pragma warning disable CA1416 // Проверка совместимости платформы
            var table = new Table()
                .Title($"[bold red]Stopped Services ({stoppedServices.Count()})[/]")
                .BorderColor(Color.Red)
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[cyan]Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Startup Type[/]").Centered())
                .AddColumn(new TableColumn("[cyan]Can Start[/]").Centered());
#pragma warning restore CA1416 // Проверка совместимости платформы
            
            foreach (var service in stoppedServices)
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                string canStart = service.Status == ServiceControllerStatus.Stopped ? "[green]✓[/]" : "[red]✗[/]";
#pragma warning restore CA1416 // Проверка совместимости платформы

#pragma warning disable CA1416 // Проверка совместимости платформы
                table.AddRow(
                    $"[white]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[grey]{Ellipsis(service.DisplayName, 30)}[/]",
                    $"[yellow]{GetStartupType(service.ServiceName)}[/]",
                    canStart
                );
#pragma warning restore CA1416 // Проверка совместимости платформы
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to get stopped services: {ex.Message}");
        }
    }
    
    private static void SearchService()
    {
        try
        {
            string searchTerm = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter service name or display name to search:[/]")
                    .PromptStyle("yellow"));
            
#pragma warning disable CA1416
            var services = ServiceController.GetServices()
                .Where(s => s.ServiceName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           s.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.ServiceName);
#pragma warning restore CA1416
            
#pragma warning disable CA1416 // Проверка совместимости платформы
            if (!services.Any())
            {
                AnsiConsole.MarkupLine($"[yellow]No services found matching '{searchTerm}'.[/]");
                return;
            }
#pragma warning restore CA1416 // Проверка совместимости платформы
            
#pragma warning disable CA1416 // Проверка совместимости платформы
            var table = new Table()
                .Title($"[bold blue]Search Results: '{searchTerm}' ({services.Count()})[/]")
                .BorderColor(Color.Blue)
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[cyan]Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Status[/]").Centered())
                .AddColumn(new TableColumn("[cyan]Startup Type[/]").Centered());
#pragma warning restore CA1416 // Проверка совместимости платформы
            
            foreach (var service in services)
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                string status = GetStatusColor(service.Status);
#pragma warning restore CA1416 // Проверка совместимости платформы

#pragma warning disable CA1416 // Проверка совместимости платформы
                table.AddRow(
                    $"[white]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[grey]{Ellipsis(service.DisplayName, 30)}[/]",
                    status,
                    $"[yellow]{GetStartupType(service.ServiceName)}[/]"
                );
#pragma warning restore CA1416 // Проверка совместимости платформы
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to search services: {ex.Message}");
        }
    }
    
    private static void ShowServiceDependencies()
    {
        try
        {
            string serviceName = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter service name to view dependencies:[/]")
                    .PromptStyle("yellow"));

#pragma warning disable CA1416
            using var service = new ServiceController(serviceName);
            var table = new Table()
                .Title($"[bold cyan]Dependencies for '{serviceName}'[/]")
                .BorderColor(Color.Cyan)
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn("[cyan]Dependent Service[/]").LeftAligned())
                .AddColumn(new TableColumn("[cyan]Status[/]").Centered())
                .AddColumn(new TableColumn("[cyan]Type[/]").Centered());

            var dependents = service.DependentServices;

            if (dependents.Length == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]No dependent services found for '{serviceName}'.[/]");
                return;
            }

            foreach (var dependent in dependents.OrderBy(d => d.ServiceName))
            {
                string status = GetStatusColor(dependent.Status);

                table.AddRow(
                    $"[white]{Ellipsis(dependent.ServiceName, 30)}[/]",
                    status,
                    $"[yellow]{GetServiceType(dependent.ServiceType)}[/]"
                );
            }

            AnsiConsole.Write(table);

            // Также показываем службы, от которых зависит данная служба
            var dependencies = service.ServicesDependedOn;
            if (dependencies.Length > 0)
            {
                AnsiConsole.MarkupLine("\n[bold cyan]Depends on:[/]");
                foreach (var dep in dependencies)
                {
                    AnsiConsole.MarkupLine($"  • [white]{dep.ServiceName}[/] - {GetStatusColor(dep.Status)}");
                }
            }
#pragma warning restore CA1416
        }
        catch (Exception ex)
        {
            ShowError($"Failed to get service dependencies: {ex.Message}");
        }
    }
    
    private static void StartService()
    {
        try
        {
            string serviceName = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter service name to start:[/]")
                    .PromptStyle("yellow"));

#pragma warning disable CA1416
            using var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                AnsiConsole.MarkupLine($"[yellow]Service '{serviceName}' is already running.[/]");
                return;
            }

            AnsiConsole.Status()
                .Start($"Starting {serviceName}...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("green"));
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                });

            AnsiConsole.MarkupLine($"[green]✓ Service '{serviceName}' started successfully![/]");
#pragma warning restore CA1416
        }
        catch (Exception ex)
        {
            ShowError($"Failed to start service: {ex.Message}");
        }
    }
    
    private static void StopService()
    {
        try
        {
            string serviceName = AnsiConsole.Prompt(
                new TextPrompt<string>("[red]Enter service name to stop:[/]")
                    .PromptStyle("yellow"));

#pragma warning disable CA1416
            using var service = new ServiceController(serviceName);
            if (!service.CanStop)
            {
                AnsiConsole.MarkupLine($"[red]Service '{serviceName}' cannot be stopped.[/]");
                return;
            }

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                AnsiConsole.MarkupLine($"[yellow]Service '{serviceName}' is already stopped.[/]");
                return;
            }

            // Предупреждение для критических служб
            if (IsCriticalService(serviceName))
            {
                if (!AnsiConsole.Confirm($"[bold red]WARNING: '{serviceName}' is a critical system service. Stop anyway?[/]", false))
                {
                    AnsiConsole.MarkupLine("[yellow]Operation cancelled.[/]");
                    return;
                }
            }

            AnsiConsole.Status()
                .Start($"Stopping {serviceName}...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("red"));
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                });

            AnsiConsole.MarkupLine($"[green]✓ Service '{serviceName}' stopped successfully![/]");
#pragma warning restore CA1416
        }
        catch (Exception ex)
        {
            ShowError($"Failed to stop service: {ex.Message}");
        }
    }
    
    private static void RestartService()
    {
        try
        {
            string serviceName = AnsiConsole.Prompt(
                new TextPrompt<string>("[yellow]Enter service name to restart:[/]")
                    .PromptStyle("yellow"));

#pragma warning disable CA1416
            using var service = new ServiceController(serviceName);
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var task1 = ctx.AddTask("[green]Stopping service...[/]");
                    var task2 = ctx.AddTask("[blue]Starting service...[/]");

                    // Останавливаем службу
                    if (service.Status != ServiceControllerStatus.Stopped && service.CanStop)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                    }
                    task1.Increment(100);

                    // Запускаем службу
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                    task2.Increment(100);
                });

            AnsiConsole.MarkupLine($"[green]✓ Service '{serviceName}' restarted successfully![/]");
#pragma warning restore CA1416
        }
        catch (Exception ex)
        {
            ShowError($"Failed to restart service: {ex.Message}");
        }
    }
    
    private static void ChangeStartupType()
    {
        try
        {
            string serviceName = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter service name:[/]")
                    .PromptStyle("yellow"));
            
            var startupType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Select startup type:[/]")
                    .AddChoices(new[] {
                        "Automatic",
                        "Automatic (Delayed)",
                        "Manual", 
                        "Disabled"
                    }));
            
            // Используем PowerShell для изменения типа запуска
            string command = $"Set-Service -Name '{serviceName}' -StartupType {startupType.Split(' ')[0]}";
            
            AnsiConsole.Status()
                .Start($"Changing startup type to {startupType}...", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("blue"));
                    
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-Command \"{command}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            Verb = "runas" // Запуск от имени администратора
                        }
                    };
                    
                    process.Start();
                    process.WaitForExit();
                });
            
            AnsiConsole.MarkupLine($"[green]✓ Startup type changed to {startupType}[/]");
        }
        catch (Exception ex)
        {
            ShowError($"Failed to change startup type: {ex.Message}");
        }
    }
    
    private static void ShowServiceStatistics()
    {
        try
        {
#pragma warning disable CA1416
            var services = ServiceController.GetServices();
#pragma warning restore CA1416

#pragma warning disable CA1416 // Проверка совместимости платформы
            int running = services.Count(s => s.Status == ServiceControllerStatus.Running);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
            int stopped = services.Count(s => s.Status == ServiceControllerStatus.Stopped);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
            int paused = services.Count(s => s.Status == ServiceControllerStatus.Paused);
#pragma warning restore CA1416 // Проверка совместимости платформы
            int automatic = GetAutomaticServicesCount();
            
            var panel = new Panel(
                $"[bold cyan]Service Statistics[/]\n\n" +
                $"[green]▶ Running:[/] {running} services\n" +
                $"[red]⏹ Stopped:[/] {stopped} services\n" +
                $"[yellow]⏸ Paused:[/] {paused} services\n" +
                $"[blue]⚡ Automatic:[/] {automatic} services\n" +
                $"[white]📊 Total:[/] {services.Length} services")
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Cyan),
                Padding = new Padding(2, 1, 2, 1)
            };
            
            AnsiConsole.Write(panel);
            
            // Круговая диаграмма
            var chart = new BreakdownChart()
                .Width(60)
                .ShowPercentage()
                .AddItem("Running", running, Color.Green)
                .AddItem("Stopped", stopped, Color.Red)
                .AddItem("Paused", paused, Color.Yellow);
            
            AnsiConsole.Write(chart);
        }
        catch (Exception ex)
        {
            ShowError($"Failed to get statistics: {ex.Message}");
        }
    }
    
    // ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ============
    
    private static string GetStatusColor(ServiceControllerStatus status)
    {
#pragma warning disable CA1416 // Проверка совместимости платформы
        return status switch
        {
            ServiceControllerStatus.Running => "[green]Running[/]",
            ServiceControllerStatus.Stopped => "[red]Stopped[/]",
            ServiceControllerStatus.Paused => "[yellow]Paused[/]",
            ServiceControllerStatus.StartPending => "[blue]Starting...[/]",
            ServiceControllerStatus.StopPending => "[orange3]Stopping...[/]",
            _ => "[grey]Unknown[/]"
        };
#pragma warning restore CA1416 // Проверка совместимости платформы
    }

    private static string GetServiceType(ServiceType type)
    {
#pragma warning disable CA1416 // Проверка совместимости платформы
        if ((type & ServiceType.InteractiveProcess) != 0) return "Interactive";
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
        if ((type & ServiceType.Win32OwnProcess) != 0) return "Win32";
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
        if ((type & ServiceType.Win32ShareProcess) != 0) return "Shared";
#pragma warning restore CA1416 // Проверка совместимости платформы
        return type.ToString();
    }
    
    private static string GetStartupType(string serviceName)
    {
        try
        {
#pragma warning disable CA1416 // Проверка совместимости платформы
            using var searcher = new ManagementObjectSearcher(
                $"SELECT StartMode FROM Win32_Service WHERE Name = '{serviceName}'");
#pragma warning disable CA1416 // Проверка совместимости платформы
            foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                return service["StartMode"]?.ToString() ?? "Unknown";
#pragma warning restore CA1416 // Проверка совместимости платформы
            }
        }
        catch
        {
            // В случае ошибки возвращаем Unknown
        }
        return "Unknown";
    }
    
    private static long GetServiceMemoryUsage(string serviceName)
    {
        try
        {
            // Получаем PID службы
            int pid = 0;
#pragma warning disable CA1416 // Проверка совместимости платформы
            using (var searcher = new ManagementObjectSearcher(
                $"SELECT ProcessId FROM Win32_Service WHERE Name = '{serviceName}'"))
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    pid = Convert.ToInt32(service["ProcessId"]);
#pragma warning restore CA1416 // Проверка совместимости платформы
                    break;
                }
            }

            if (pid == 0) return 0;

            // Получаем информацию о процессе
            using var process = Process.GetProcessById(pid);
            return process.WorkingSet64 / 1024; // Конвертируем в KB
        }
        catch
        {
            return 0;
        }
    }
    
    private static int GetAutomaticServicesCount()
    {
        try
        {
            int count = 0;
#pragma warning disable CA1416 // Проверка совместимости платформы
            using (var searcher = new ManagementObjectSearcher(
                "SELECT StartMode FROM Win32_Service"))
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    var startMode = service["StartMode"]?.ToString();
#pragma warning restore CA1416 // Проверка совместимости платформы
                    if (startMode == "Auto" || startMode == "Automatic")
                        count++;
                }
            }

            return count;
        }
        catch
        {
            return 0;
        }
    }
    
    private static void ShowServiceCounts(ServiceController[] services)
    {
        var grid = new Grid()
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn());

#pragma warning disable CA1416 // Проверка совместимости платформы
        grid.AddRow(
            new Panel($"[bold]{services.Count(s => s.Status == ServiceControllerStatus.Running)}[/]\nRunning")
                .BorderColor(Color.Green),
            new Panel($"[bold]{services.Count(s => s.Status == ServiceControllerStatus.Stopped)}[/]\nStopped")
                .BorderColor(Color.Red),
            new Panel($"[bold]{services.Count(s => s.Status == ServiceControllerStatus.Paused)}[/]\nPaused")
                .BorderColor(Color.Yellow),
            new Panel($"[bold]{services.Length}[/]\nTotal").BorderColor(Color.Blue)
        );
#pragma warning restore CA1416 // Проверка совместимости платформы

        AnsiConsole.Write(grid);
    }
    
    private static bool IsCriticalService(string serviceName)
    {
        string[] criticalServices = [
            "lsass", "wininit", "services", "svchost", 
            "csrss", "smss", "system", "winlogon"
        ];
        
        return criticalServices.Any(cs => 
            serviceName.Contains(cs, StringComparison.OrdinalIgnoreCase));
    }
    
    private static string Ellipsis(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength - 3) + "...";
    }
    
    private static void ShowError(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗ {message}[/]");
    }
}