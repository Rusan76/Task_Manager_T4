using System;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Spectre.Console;

namespace Task_Manager_T4;

public class ServiceManagerUI
{
    public static void ShowServicesMenu()
    {
        while (true)
        {
            Console.Clear();
            
            AnsiConsole.Write(new Rule($"[{GraphicSettings.AccentColor}]Service Manager[/]").RuleStyle(GraphicSettings.SecondaryColor).LeftJustified());
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{GraphicSettings.SecondaryColor}]Select category[/]")
                    .PageSize(GraphicSettings.PageSize)
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
                    Console.Clear();
                    return;
            }
            
            AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Press any key to continue...[/]");
            Console.ReadKey();
        }
    }
    
    private static void ShowAllServices()
    {
        try
        {
            var services = ServiceController.GetServices();
            var table = new Table()
                .Title($"[{GraphicSettings.AccentColor}]Windows Services ({services.Length})[/]")
                .BorderColor(GraphicSettings.GetColor(GraphicSettings.AccentColor))
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Status[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Type[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Can Stop[/]").Centered());
            foreach (var service in services.OrderBy(s => s.ServiceName))
            {
                string status = GetStatusColor(service.Status);
                string type = GetServiceType(service.ServiceType);
                string canStop = service.CanStop ? "[green]YES[/]" : "[red] NO[/]";

                table.AddRow(
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.DisplayName, 30)}[/]",
                    status,
                    $"[{GraphicSettings.SecondaryColor}]{type}[/]",
                    canStop
                );
            }

            AnsiConsole.Write(table);
            
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
            var runningServices = ServiceController.GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Running)
                .OrderBy(s => s.ServiceName);
            
            var table = new Table()
                .Title($"[{GraphicSettings.AccentColor}]Running Services ({runningServices.Count()})[/]")
                .BorderColor(GraphicSettings.GetColor(GraphicSettings.AccentColor))
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Startup Type[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Memory[/]").RightAligned());
            
            foreach (var service in runningServices)
            {
                table.AddRow(
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.DisplayName, 30)}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{GetStartupType(service.ServiceName)}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{GetServiceMemoryUsage(service.ServiceName):N0} KB[/]"
                );
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
            var stoppedServices = ServiceController.GetServices()
                .Where(s => s.Status == ServiceControllerStatus.Stopped)
                .OrderBy(s => s.ServiceName);
            
            var table = new Table()
                .Title($"[{GraphicSettings.AccentColor}]Stopped Services ({stoppedServices.Count()})[/]")
                .BorderColor(GraphicSettings.GetColor(GraphicSettings.AccentColor))
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Startup Type[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Can Start[/]").Centered());
            
            foreach (var service in stoppedServices)
            {
                string canStart = service.Status == ServiceControllerStatus.Stopped ? "[green]✓[/]" : "[red]✗[/]";

                table.AddRow(
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.DisplayName, 30)}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{GetStartupType(service.ServiceName)}[/]",
                    canStart
                );
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
                new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter service name or display name to search:[/]")
                    .PromptStyle(GraphicSettings.AccentColor));
            var services = ServiceController.GetServices()
                .Where(s => s.ServiceName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                           s.DisplayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s.ServiceName);
            
            if (!services.Any())
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]No services found matching '{searchTerm}'.[/]");
                return;
            }
            
            var table = new Table()
                .Title($"[{GraphicSettings.AccentColor}]Search Results: '{searchTerm}' ({services.Count()})[/]")
                .BorderColor(GraphicSettings.GetColor(GraphicSettings.AccentColor))
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Display Name[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Status[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Startup Type[/]").Centered());
            
            foreach (var service in services)
            {
                string status = GetStatusColor(service.Status);

                table.AddRow(
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.ServiceName, 20)}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(service.DisplayName, 30)}[/]",
                    status,
                    $"[{GraphicSettings.SecondaryColor}]{GetStartupType(service.ServiceName)}[/]"
                );
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
                new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter service name to view dependencies:[/]")
                    .PromptStyle(GraphicSettings.AccentColor));
            using var service = new ServiceController(serviceName);
            var table = new Table()
                .Title($"[{GraphicSettings.AccentColor}]Dependencies for '{serviceName}'[/]")
                .BorderColor(GraphicSettings.GetColor("cyan"))
                .Border(TableBorder.Rounded)
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Dependent Service[/]").LeftAligned())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Status[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Type[/]").Centered());

            var dependents = service.DependentServices;

            if (dependents.Length == 0)
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]No dependent services found for '{serviceName}'.[/]");
                return;
            }

            foreach (var dependent in dependents.OrderBy(d => d.ServiceName))
            {
                string status = GetStatusColor(dependent.Status);

                table.AddRow(
                    $"[{GraphicSettings.SecondaryColor}]{Ellipsis(dependent.ServiceName, 30)}[/]",
                    status,
                    $"[{GraphicSettings.SecondaryColor}]{GetServiceType(dependent.ServiceType)}[/]"
                );
            }

            AnsiConsole.Write(table);

            var dependencies = service.ServicesDependedOn;
            if (dependencies.Length > 0)
            {
                AnsiConsole.MarkupLine($"\n[{GraphicSettings.AccentColor}]Depends on:[/]");
                foreach (var dep in dependencies)
                {
                    AnsiConsole.MarkupLine($"  • [{GraphicSettings.SecondaryColor}]{dep.ServiceName}[/] - {GetStatusColor(dep.Status)}");
                }
            }
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
                new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter service name to start:[/]")
                    .PromptStyle(GraphicSettings.AccentColor));
            using var service = new ServiceController(serviceName);
            if (service.Status == ServiceControllerStatus.Running)
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]Service '{serviceName}' is already running.[/]");
                return;
            }

            AnsiConsole.Status()
                .Start($"[{GraphicSettings.NeutralColor}]Starting {serviceName}...[/]", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse(GraphicSettings.SecondaryColor));
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                });

            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]✓ Service '{serviceName}' started successfully![/]");
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
                new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter service name to stop:[/]")
                    .PromptStyle(GraphicSettings.AccentColor));
            using var service = new ServiceController(serviceName);
            if (!service.CanStop)
            {
                AnsiConsole.MarkupLine($"[red]Service '{serviceName}' cannot be stopped.[/]");
                return;
            }

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]Service '{serviceName}' is already stopped.[/]");
                return;
            }

            if (IsCriticalService(serviceName))
            {
                if (!AnsiConsole.Confirm($"[bold red]WARNING: '{serviceName}' is a critical system service. Stop anyway?[/]", false))
                {
                    AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]Operation cancelled.[/]");
                    return;
                }
            }

            AnsiConsole.Status()
                .Start($"[{GraphicSettings.NeutralColor}]Stopping {serviceName}...[/]", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse(GraphicSettings.AccentColor));
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                });

            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]✓ Service '{serviceName}' stopped successfully![/]");
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
                new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter service name to restart:[/]")
                    .PromptStyle(GraphicSettings.AccentColor));
            using var service = new ServiceController(serviceName);
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    var task1 = ctx.AddTask($"[{GraphicSettings.NeutralColor}]Stopping service...[/]");
                    var task2 = ctx.AddTask($"[{GraphicSettings.NeutralColor}]Starting service...[/]");

                    if (service.Status != ServiceControllerStatus.Stopped && service.CanStop)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(15));
                    }
                    task1.Increment(100);

                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                    task2.Increment(100);
                });

            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]✓ Service '{serviceName}' restarted successfully![/]");
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
                new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter service name:[/]")
                    .PromptStyle(GraphicSettings.AccentColor));
            
            var startupType = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{GraphicSettings.AccentColor}]Select startup type:[/]")
                    .AddChoices([
                        "Automatic",
                        "Automatic (Delayed)",
                        "Manual", 
                        "Disabled"
                    ]));
            
            string command = $"Set-Service -Name '{serviceName}' -StartupType {startupType.Split(' ')[0]}";
            
            AnsiConsole.Status()
                .Start($"[{GraphicSettings.NeutralColor}]Changing startup type to {startupType}...[/]", ctx =>
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse(GraphicSettings.SecondaryColor));
                    
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-Command \"{command}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            Verb = "runas"
                        }
                    };
                    
                    process.Start();
                    process.WaitForExit();
                });
            
            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]✓ Startup type changed to {startupType}[/]");
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
            var services = ServiceController.GetServices();

            int running = services.Count(s => s.Status == ServiceControllerStatus.Running);
            int stopped = services.Count(s => s.Status == ServiceControllerStatus.Stopped);
            int paused = services.Count(s => s.Status == ServiceControllerStatus.Paused);
            int automatic = GetAutomaticServicesCount();
            
            var panel = new Panel(
                $"[{GraphicSettings.AccentColor}]Service Statistics[/]\n\n" +
                $"[{GraphicSettings.SecondaryColor}]▶ Running:[/] {running} services\n" +
                $"[{GraphicSettings.SecondaryColor}]⏹ Stopped:[/] {stopped} services\n" +
                $"[{GraphicSettings.SecondaryColor}]⏸ Paused:[/] {paused} services\n" +
                $"[{GraphicSettings.SecondaryColor}]⚡ Automatic:[/] {automatic} services\n" +
                $"[{GraphicSettings.SecondaryColor}]📊 Total:[/] {services.Length} services")
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(GraphicSettings.GetColor(GraphicSettings.AccentColor)),
                Padding = new Padding(2, 1, 2, 1)
            };
            
            AnsiConsole.Write(panel);
            
            var chart = new BreakdownChart()
                .Width(60)
                .ShowPercentage()
                .AddItem("Running", running, GraphicSettings.GetColor(GraphicSettings.SecondaryColor))
                .AddItem("Stopped", stopped, GraphicSettings.GetColor(GraphicSettings.AccentColor))
                .AddItem("Paused", paused, Color.Gray);
            
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
        return status switch
        {
            ServiceControllerStatus.Running => "[green]Running[/]",
            ServiceControllerStatus.Stopped => "[red]Stopped[/]",
            ServiceControllerStatus.Paused => "[yellow]Paused[/]",
            ServiceControllerStatus.StartPending => "[blue]Starting...[/]",
            ServiceControllerStatus.StopPending => "[orange3]Stopping...[/]",
            _ => $"[{GraphicSettings.NeutralColor}]Unknown[/]"
        };
    }

    private static string GetServiceType(ServiceType type)
    {
        if ((type & ServiceType.InteractiveProcess) != 0) return "Interactive";
        if ((type & ServiceType.Win32OwnProcess) != 0) return "Win32";
        if ((type & ServiceType.Win32ShareProcess) != 0) return "Shared";
        return type.ToString();
    }
    
    private static string GetStartupType(string serviceName)
    {
        try
        {
#pragma warning disable CA1416
            using var searcher = new ManagementObjectSearcher(
                $"SELECT StartMode FROM Win32_Service WHERE Name = '{serviceName}'");
            foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
            {
                return service["StartMode"]?.ToString() ?? "Unknown";
            }
#pragma warning restore CA1416
        }
        catch
        {
        }
        return "Unknown";
    }
    
    private static long GetServiceMemoryUsage(string serviceName)
    {
        try
        {
            int pid = 0;
#pragma warning disable CA1416
            using (var searcher = new ManagementObjectSearcher(
                $"SELECT ProcessId FROM Win32_Service WHERE Name = '{serviceName}'"))
            {
                foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
                {
                    pid = Convert.ToInt32(service["ProcessId"]);
                    break;
                }
            }

            if (pid == 0) return 0;

            using var process = Process.GetProcessById(pid);
            return process.WorkingSet64 / 1024;
#pragma warning restore CA1416
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
#pragma warning disable CA1416
            using (var searcher = new ManagementObjectSearcher(
                "SELECT StartMode FROM Win32_Service"))
            {
                foreach (ManagementObject service in searcher.Get().Cast<ManagementObject>())
                {
                    var startMode = service["StartMode"]?.ToString();
                    if (startMode == "Auto" || startMode == "Automatic")
                        count++;
                }
            }

            return count;
#pragma warning restore CA1416
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
        grid.AddRow(
            new Panel($"[{GraphicSettings.AccentColor}]{services.Count(s => s.Status == ServiceControllerStatus.Running)}[/]\n[{GraphicSettings.SecondaryColor}]Running[/]")
                .BorderColor(Color.Green),
            new Panel($"[{GraphicSettings.AccentColor}]{services.Count(s => s.Status == ServiceControllerStatus.Stopped)}[/]\n[{GraphicSettings.SecondaryColor}]Stopped[/]")
                .BorderColor(Color.Red),
            new Panel($"[{GraphicSettings.AccentColor}]{services.Count(s => s.Status == ServiceControllerStatus.Paused)}[/]\n[{GraphicSettings.SecondaryColor}]Paused[/]")
                .BorderColor(Color.Yellow),
            new Panel($"[{GraphicSettings.AccentColor}]{services.Length}[/]\n[{GraphicSettings.SecondaryColor}]Total[/]").BorderColor(Color.Blue)
        );
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