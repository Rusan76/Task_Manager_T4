using System;
using System.Threading;
using System.Diagnostics;
using Spectre.Console;
using ProjectT4;
using Task_Manager_T4;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "Task Manager T4";

        if (args.Length > 0 && args[0] == "--fix-keyboard")
        {
            Other.Keyboard.FixKeyboard();
            return;
        }

        bool keepRunning = true;
        while (keepRunning)
        {
            AnsiConsole.Clear();

            // 1. Красивый заголовок
            AnsiConsole.Write(
                new FigletText("T4 Manager")
                    .Centered()
                    .Color(Color.Orange1));

            // 2. Создаем таблицу с инфо о системе (компактную)
            var sysInfo = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .AddColumn("Параметр")
                .AddColumn("Значение");

            sysInfo.AddRow("[green]OS[/]", Environment.OSVersion.ToString());
            sysInfo.AddRow("[green]User[/]", Environment.UserName);
            sysInfo.AddRow("[green]Machine[/]", Environment.MachineName);
            sysInfo.AddRow("[green]CPU Cores[/]", Environment.ProcessorCount.ToString());

            // 3. Выводим информацию и сразу предлагаем выбор
            AnsiConsole.Write(
                new Panel(sysInfo)
                    .Header("[bold yellow] System Dashboard [/]")
                    .Expand()
                    .BorderColor(Color.Yellow));
            AnsiConsole.MarkupLine("[green]Press any key to continue[/]");
            Console.ReadKey();
            Function_list();
        }
    }

    public static void Function_list()
    {
        Console.Clear();
        while (true)
        {
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
                        "File and folder manager",
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
                case "File and folder manager":
                    MainFF.PrintFunctions();
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