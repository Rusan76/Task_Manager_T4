using System;
using System.Threading;
using System.Diagnostics;
using Spectre.Console;
using Task_Manager_T4;
using System.IO;

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
        string fontPath = Path.Combine(AppContext.BaseDirectory, "resources", "fonts", "Speed.flf");
        while (true)
        {
            AnsiConsole.Clear();

            if (File.Exists(fontPath))
            {
                var font = FigletFont.Load(fontPath);
                AnsiConsole.Write(
                    new FigletText(font, "tm T4")
                        .Centered()
                        .Color(Color.Orange1));
            }
            else
            {
                AnsiConsole.Write(new FigletText("tm T4").Centered().Color(Color.Orange1));
            }
            var sysInfo = new Table()
                .Border(TableBorder.HeavyEdge)
                .BorderColor(Color.Orange1)
                .AddColumn("Параметр")
                .AddColumn("Значение");

            sysInfo.AddRow("[orange1]OS[/]", Environment.OSVersion.ToString());
            sysInfo.AddRow("[orange1]User[/]", Environment.UserName);
            sysInfo.AddRow("[orange1]Machine[/]", Environment.MachineName);
            sysInfo.AddRow("[orange1]CPU Cores[/]", Environment.ProcessorCount.ToString());

            AnsiConsole.Write(
                new Panel(sysInfo)
                    .Header("[bold white] System Dashboard [/]")
                    .Expand()
                    .BorderColor(Color.White));
            AnsiConsole.Write(new Rule("[white]V1.3 Press any key to continue[/]").RuleStyle("orange1"));
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
                    .Title($"[{GraphicSettings.AccentColor}]Select an option:[/]")
                    .PageSize(GraphicSettings.PageSize)
                    .MoreChoicesText($"[{GraphicSettings.NeutralColor}](Move up and down to reveal more options)[/]")
                    .AddChoices(
                    [
                        "📊 Process Management",
                        "🔧 Service Manager",
                        "⚡ Startup Manager",
                        "💻 System Information",
                        "⚙️ Drives",
                        "Registry",
                        "🖥️ Show System Load",
                        "🌡️ Check Temperature",
                        "🔩 Benchmark",
                        "🚀 Program Launcher",
                        // "📸 Screenshot Tool",
                        "File and folder manager",
                        "Program Manager",
                        "❔ Other",
                        "Graphic Settings",
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
                // case "📸 Screenshot Tool":
                //     GetInfoPc.TakeScreenshotMenu();
                //     break;
                case "🚀 Program Launcher":
                    OpenProgram.OpenPrograms();
                    break;
                case "🖥️ Show System Load":
                    ShowSystemLoad();
                    Console.Clear();
                    break;
                case "⚡ Startup Manager":
                    try
                    {
                        StartUpManager startupManager = new();
                        StartUpManager.ShowStartupManagerUI();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ReadKey();
                    }
                    break;
                case "🌡️ Check Temperature":
                    AdvancedTemperatureMonitor.ShowAllTemperatures();
                    Console.Clear();
                    break;
                case "🔧 Service Manager":
                    ServiceManagerUI.ShowServicesMenu();
                    break;
                case "❔ Other":
                    Other.PrintAllOtherFunctions();
                    Console.Clear();
                    break;
                case "⚙️ Drives":
                    DriveManager.Main_Menu_Drives();
                    break;
                case "🔩 Benchmark":
                    SystemBenchmark.ShowBenchmarkMenu();
                    break;
                case "🎨 OpenMe":
                    Rain.ShowReadMeWithRain();
                    Console.Clear();
                    break;
                case "File and folder manager":
                    Console.Clear();
                    MainFF.PrintFunctions();
                    break;
                case "Registry":
                    Management_Registry.Main_Menu_Registry();
                    Console.Clear();
                    break;
                case "Program Manager":
                    ProgramManager.MainMenuProgramManager();
                    break;      
                case "Graphic Settings":
                    GraphicSettings.ChangeTheme();
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
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]System Load Monitoring[/]");
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Updates every two seconds. Press any key to exit.[/]");

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


                cpuUsage = performanceCounterCpu.NextValue();


                memoryUsagePercent = performanceCounterMemory.NextValue();

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
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]System Load Monitoring[/]");

            if (performanceCounterCpu == null || performanceCounterMemory == null)
            {
                AnsiConsole.MarkupLine("[yellow]⚠ Using simulated data[/]");
            }

            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Updates every two seconds. Press any key to exit.[/]");
            AnsiConsole.Write(table.BorderColor(Color.DarkOrange3)); //dodelat'

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