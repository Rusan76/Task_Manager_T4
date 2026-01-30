using System;
using System.Threading;
using System.Diagnostics;
using Spectre.Console;
using ProjectT4;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        if (args.Length > 0 && args[0] == "--fix-keyboard")
        {
            Other.Keyboard.FixKeyboard();
            return;
        }
        
        Console.Clear();
        Console.Title = "Task Manager T4";

        AnsiConsole.Write(
            new FigletText("T4")
                .Centered()
                .Color(Color.Blue));

        AnsiConsole.Write(
            new FigletText("Task Manager")
                .Centered()
                .Color(Color.Yellow));

        
        var panel = new Panel("[yellow]v1.0 • Created with Spectre.Console[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1),
            Padding = new Padding(1, 1, 1, 1)
        };

        AnsiConsole.Write(panel);


        var table = new Table()
            .Centered()
            .BorderColor(Color.Green)
            .Border(TableBorder.Rounded)
            .Title("[bold yellow]System Information[/]")
            .AddColumn(new TableColumn("[cyan]Component[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Status[/]").Centered())
            .AddRow("[green]OS[/]", $"[white]{Environment.OSVersion}[/]")
            .AddRow("[green]User[/]", $"[white]{Environment.UserName}[/]")
            .AddRow("[green]Machine[/]", $"[white]{Environment.MachineName}[/]")
            .AddRow("[green]Processors[/]", $"[white]{Environment.ProcessorCount}[/]");

        AnsiConsole.Write(table);

        AnsiConsole.Write(new Rule("[yellow]Press any key to continue[/]").RuleStyle("yellow").Centered());

        Console.ReadKey();
        Console.Clear();
        Function_list();
    }

    public static void Function_list()
    {
        while (true)
        {
            Console.Clear();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]Select an option:[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                    .AddChoices(
                    [
                        "📊 Process Management",
                        "🔧 Service Manager",
                        "⚡ Startup Manager",
                        "💻 System Information",
                        "⚙️ Drives",
                        "🖥️ Show System Load",
                        "🌡️ Check Temperature",
                        "🔩 Benchmark",
                        "🚀 Program Launcher",
                        "📸 Screenshot Tool",
                        "❔ Other",
                        "🎨 OpenMe",
                        "❌ Exit"
                    ]));

            switch (choice)
            {
                case "📊 Process Management":
                    Process_management.GetProcces();
                    break;
                case "💻 System Information":
                    GetInfoPc.Main_Information_Collection();
                    break;
                case "📸 Screenshot Tool":
                    GetInfoPc.TakeScreenshotMenu();
                    break;
                case "🚀 Program Launcher":
                    OpenProgram.OpenPrograms();
                    break;
                case "🖥️ Show System Load":
                    ShowSystemLoad();
                    break;
                case "⚡ Startup Manager":
                    try
                    {
                        StartUpManager startupManager = new();
                        startupManager.ShowStartupManagerUI();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ReadKey();
                    }
                    break;
                case "🌡️ Check Temperature":
                    AdvancedTemperatureMonitor.ShowAllTemperatures();
                    break;
                case "🔧 Service Manager": 
                    ServiceManagerUI.ShowServicesMenu();
                    break;
                case "❔ Other":
                    Other.PrintAllOtherFunctions();
                    break;     
                case "⚙️ Drives":
                    DriveManager.Main_Menu_Drives();
                    break;
                case "🔩 Benchmark":
                    SystemBenchmark.ShowBenchmarkMenu();
                    break;
                case "🎨 OpenMe":
                    Rain.ShowReadMeWithRain();
                    break;
                case "❌ Exit":
                    Environment.Exit(0);
                    break;
            }
        }
    }

    private static void ShowSystemLoad()
    {
        Console.Clear();
        AnsiConsole.MarkupLine("[bold cyan]System Load Monitoring[/]");
        AnsiConsole.MarkupLine("[grey]Updates every two seconds. Press any key to exit.[/]");
        AnsiConsole.WriteLine();

        var table = new Table
        {
            Border = TableBorder.Rounded
        };

        table.AddColumn(new TableColumn("[bold]Time[/]").Centered());
        table.AddColumn(new TableColumn("[bold]CPU %[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Memory %[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Memory (bytes)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]GTD / Max Memory (bytes)[/]").Centered());
        table.AddColumn(new TableColumn("[bold]GTD / Max CPU (units)[/]").Centered());

        
        PerformanceCounter performanceCounterCpu = null;
        PerformanceCounter performanceCounterMemory = null;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                performanceCounterCpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                performanceCounterMemory = new PerformanceCounter("Memory", "% Committed Bytes In Use");

                
                performanceCounterCpu.NextValue();
                performanceCounterMemory.NextValue();
                Thread.Sleep(1000);
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Performance counters are only available on Windows.[/]");
                AnsiConsole.MarkupLine("[yellow]Using simulated data for demonstration.[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error initializing performance counters: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[yellow]Using simulated data for demonstration.[/]");
        }

        Random rand = new();
        long totalSystemMemory = GetTotalSystemMemory();

        while (!Console.KeyAvailable)
        {
            float cpuUsage;
            float memoryUsagePercent;
            long usedMemoryBytes;

            if (performanceCounterCpu != null && performanceCounterMemory != null)
            {
                
#pragma warning disable CA1416 
                cpuUsage = performanceCounterCpu.NextValue();
#pragma warning restore CA1416 
#pragma warning disable CA1416 
                memoryUsagePercent = performanceCounterMemory.NextValue();
#pragma warning restore CA1416 
                usedMemoryBytes = (long)(memoryUsagePercent / 100.0 * totalSystemMemory);
            }
            else
            {
                
                cpuUsage = rand.Next(0, 100);
                memoryUsagePercent = rand.Next(10, 80);
                usedMemoryBytes = (long)(memoryUsagePercent / 100.0 * totalSystemMemory);
            }

            
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            
            string cpuFormatted = $"{cpuUsage:F2} %";
            string memoryPercentFormatted = $"{memoryUsagePercent:F2} %";
            string memoryBytesFormatted = $"{usedMemoryBytes:N0}";
            string memoryMaxFormatted = $"{usedMemoryBytes:N0} / {totalSystemMemory:N0}";
            string cpuMaxFormatted = GetCpuGuaranteedMaxInfo();

            
            table.AddRow(
                currentTime,
                cpuFormatted,
                memoryPercentFormatted,
                memoryBytesFormatted,
                memoryMaxFormatted,
                cpuMaxFormatted
            );

            
            Console.Clear();
            AnsiConsole.MarkupLine("[bold cyan]System Load Monitoring[/]");

            if (performanceCounterCpu == null || performanceCounterMemory == null)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Using simulated data[/]");
            }

            AnsiConsole.MarkupLine("[grey]Updates every two seconds. Press any key to exit.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);

            Thread.Sleep(2000);
        }

        
        while (Console.KeyAvailable) Console.ReadKey(true);

        
        performanceCounterCpu?.Dispose();
        performanceCounterMemory?.Dispose();
    }

    private static string GetCpuGuaranteedMaxInfo()
    {
        if (OperatingSystem.IsWindows())
        {
            
            
            int maxCpu = Environment.ProcessorCount;

            
            int guaranteedCpu = 1; 

            return $"{guaranteedCpu} / {maxCpu}";
        }
        else
        {
            
            int maxCpu = Environment.ProcessorCount;
            int guaranteedCpu = 1;

            return $"{guaranteedCpu} / {maxCpu}";
        }
    }

    private static long GetTotalSystemMemory()
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                
                return GetWindowsTotalMemory();
            }
            else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                
                
                return 16L * 1024 * 1024 * 1024; 
            }
            else
            {
                return 8L * 1024 * 1024 * 1024; 
            }
        }
        catch
        {
            
            return 8L * 1024 * 1024 * 1024; 
        }
    }

    
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    private static long GetWindowsTotalMemory()
    {
        MEMORYSTATUSEX memStatus = new()
        {
            dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf<MEMORYSTATUSEX>()
        };

        if (GlobalMemoryStatusEx(ref memStatus))
        {
            return (long)memStatus.ullTotalPhys;
        }

        return 8L * 1024 * 1024 * 1024; 
    }
}