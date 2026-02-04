using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Linq;
using Spectre.Console;

namespace Task_Manager_T4;

public class GetInfoPc
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int width, int height);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    static string GetHardwareInfo(string win32Class, string classProperty)
    {
        string result = "";
        try
        {
#pragma warning disable CA1416 
            ManagementObjectSearcher searcher = new($"SELECT {classProperty} FROM {win32Class}");
#pragma warning restore CA1416 
#pragma warning disable CA1416 
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                string value = obj[classProperty]?.ToString() ?? "";
                
                value = value.Replace("[", "\\[").Replace("]", "\\]");
                result += value + Environment.NewLine;
            }
#pragma warning restore CA1416 
        }
        catch (Exception ex)
        {
            result = $"Ошибка получения данных: {ex.Message.Replace("[", "\\[").Replace("]", "\\]")}";
        }

        result = result.Trim();
        return string.IsNullOrEmpty(result) ? "Данные не найдены" : result;
    }

    static bool IsUserAdministrator()
    {
        try
        {
#pragma warning disable CA1416 
            var identity = WindowsIdentity.GetCurrent();
#pragma warning restore CA1416 
#pragma warning disable CA1416 
            var principal = new WindowsPrincipal(identity);
#pragma warning restore CA1416 
#pragma warning disable CA1416 
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 
        }
        catch { return false; }
    }

    public static void Main_Information_Collection()
    {
        Console.Clear();

        try
        {
            AnsiConsole.Write(new Rule($"[{GraphicSettings.AccentColor}]System Information report[/]").RuleStyle(GraphicSettings.SecondaryColor).LeftJustified());
            AnsiConsole.WriteLine();

            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            var generalInfo = new Panel(
                $"[{GraphicSettings.AccentColor}]General Information[/]\n\n" +
                $"Computer: [{GraphicSettings.SecondaryColor}]{Environment.MachineName}[/]\n" +
                $"User: [{GraphicSettings.SecondaryColor}]{Environment.UserName}[/]\n" +
                $"OS: [{GraphicSettings.SecondaryColor}]{Environment.OSVersion}[/]\n" +
                $"Processors: [{GraphicSettings.SecondaryColor}]{Environment.ProcessorCount}[/]\n" +
                $"Admin: [{GraphicSettings.SecondaryColor}]{(IsUserAdministrator() ? "Yes" : "No")}[/]\n" +
                $"Uptime: [{GraphicSettings.SecondaryColor}]{TimeSpan.FromMilliseconds(Environment.TickCount):dd\\.hh\\:mm\\:ss}[/]")
                .BorderColor(GraphicSettings.GetThemeColor)
                .Padding(1, 1);

            string cpuInfo = "Not available";
            string gpuInfo = "Not available";

            try
            {
                cpuInfo = GetHardwareInfoSimple("Win32_Processor", "Name");
                gpuInfo = GetHardwareInfoSimple("Win32_VideoController", "Name");
            }
            catch { }

            var hardwareInfo = new Panel(
                $"[{GraphicSettings.AccentColor}]Hardware Information[/]\n\n" +
                $"CPU: [{GraphicSettings.SecondaryColor}]{cpuInfo}[/]\n" +
                $"GPU: [{GraphicSettings.SecondaryColor}]{gpuInfo}[/]\n" +
                $"64-bit OS: [{GraphicSettings.SecondaryColor}]{(Environment.Is64BitOperatingSystem ? "Yes" : "No")}[/]\n" +
                $".NET: [{GraphicSettings.SecondaryColor}]{Environment.Version}[/]")
                .BorderColor(GraphicSettings.GetThemeColor)
                .Padding(1, 1);

            grid.AddRow(generalInfo, hardwareInfo);
            AnsiConsole.Write(grid);

            AnsiConsole.WriteLine();
            ShowDriveInfoSimple();

            AnsiConsole.WriteLine();
            if (AnsiConsole.Confirm($"[{GraphicSettings.AccentColor}]Create detailed report file?[/]", true))
            {
                CreateDetailedReport();
            }

            AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Press any key to continue...[/]");
            Console.ReadKey();
            Console.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    static string GetHardwareInfoSimple(string win32Class, string classProperty)
    {
        try
        {
#pragma warning disable CA1416
            using var searcher = new ManagementObjectSearcher($"SELECT {classProperty} FROM {win32Class}");
            var result = new System.Text.StringBuilder();

            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                string value = obj[classProperty]?.ToString() ?? "";
                
                value = value.Replace("[", "").Replace("]", "");
                if (!string.IsNullOrEmpty(value))
                {
                    result.AppendLine(value.Trim());
                }
            }

            string info = result.ToString().Trim();
            return string.IsNullOrEmpty(info) ? "Not available" : info.Split('\n')[0]; 
#pragma warning restore CA1416
        }
        catch
        {
            return "Not available";
        }
    }

    private static void ShowDriveInfoSimple()
    {
        try
        {
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

            var table = new Table()
                .Title($"[{GraphicSettings.AccentColor}]Storage Drives[/]")
                .Border(TableBorder.Simple)
                .BorderColor(GraphicSettings.GetThemeColor)
                .AddColumn($"[{GraphicSettings.SecondaryColor}]Drive[/]")
                .AddColumn($"[{GraphicSettings.SecondaryColor}]Label[/]")
                .AddColumn($"[{GraphicSettings.SecondaryColor}]Type[/]")
                .AddColumn($"[{GraphicSettings.SecondaryColor}]Total[/]")
                .AddColumn($"[{GraphicSettings.SecondaryColor}]Free[/]")
                .AddColumn($"[{GraphicSettings.SecondaryColor}]Usage[/]");

            foreach (var drive in drives)
            {
                double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                double freeGB = drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                double usedPercent = 100 - (drive.TotalFreeSpace * 100 / drive.TotalSize);

                string usageBar = GetSimpleUsageBar(usedPercent);

                table.AddRow(
                    $"[{GraphicSettings.AccentColor}]{drive.Name}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{drive.VolumeLabel}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{drive.DriveType}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{totalGB:F1} GB[/]",
                    $"[{GraphicSettings.SecondaryColor}]{freeGB:F1} GB[/]",
                    $"[{GraphicSettings.SecondaryColor}]{usageBar} {usedPercent:F1}%[/]");
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error reading drive info: {ex.Message}[/]");
        }
    }

    private static string GetSimpleUsageBar(double percent)
    {
        int filled = (int)(percent / 10);
        return new string('█', filled) + new string('░', 10 - filled);
    }

    private static void CreateDetailedReport()
    {
        try
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string folderPath = Path.Combine(desktopPath, "SystemReport");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string reportFile = Path.Combine(folderPath, $"report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            using (StreamWriter sw = new(reportFile))
            {
                sw.WriteLine("=== SYSTEM REPORT ===");
                sw.WriteLine($"Date: {DateTime.Now}");
                sw.WriteLine();

                sw.WriteLine("=== GENERAL INFO ===");
                sw.WriteLine($"Computer: {Environment.MachineName}");
                sw.WriteLine($"User: {Environment.UserName}");
                sw.WriteLine($"OS: {Environment.OSVersion}");
                sw.WriteLine($"Processors: {Environment.ProcessorCount}");
                sw.WriteLine($"Admin: {IsUserAdministrator()}");
                sw.WriteLine($"Uptime: {TimeSpan.FromMilliseconds(Environment.TickCount):dd\\.hh\\:mm\\:ss}");
                sw.WriteLine();

                sw.WriteLine("=== HARDWARE INFO ===");
                try
                {
                    sw.WriteLine($"CPU: {GetHardwareInfoForFile("Win32_Processor", "Name")}");
                    sw.WriteLine($"GPU: {GetHardwareInfoForFile("Win32_VideoController", "Name")}");
                }
                catch (Exception ex)
                {
                    sw.WriteLine($"Hardware info error: {ex.Message}");
                }
                sw.WriteLine();

                sw.WriteLine("=== STORAGE INFO ===");
                try
                {
                    foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                    {
                        double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                        double freeGB = drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                        sw.WriteLine($"{drive.Name} ({drive.VolumeLabel}): {freeGB:F1} GB free of {totalGB:F1} GB");
                    }
                }
                catch (Exception ex)
                {
                    sw.WriteLine($"Storage info error: {ex.Message}");
                }
            }

            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]Report created: {reportFile}[/]");

            if (Directory.Exists(folderPath))
            {
                Process.Start("explorer.exe", folderPath);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating report: {ex.Message}[/]");
        }
    }

    private static string GetHardwareInfoForFile(string win32Class, string classProperty)
    {
        try
        {
#pragma warning disable CA1416
            using var searcher = new ManagementObjectSearcher($"SELECT {classProperty} FROM {win32Class}");
            var result = new System.Text.StringBuilder();

            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                string value = obj[classProperty]?.ToString() ?? "";
                if (!string.IsNullOrEmpty(value))
                {
                    result.AppendLine(value.Trim());
                }
            }

            string info = result.ToString().Trim();
            return string.IsNullOrEmpty(info) ? "Not available" : info;
#pragma warning restore CA1416
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public static void ShowSystemInfoPanels()
    {
        Console.Clear();

        try
        {
            AnsiConsole.Progress()
                .Columns(
                [
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
                ])
                .Start(ctx =>
                {
                    var task1 = ctx.AddTask($"[{GraphicSettings.AccentColor}]Collecting hardware info[/]");
                    var task2 = ctx.AddTask($"[{GraphicSettings.AccentColor}]Gathering system data[/]");

                    for (int i = 0; i < 100; i += 10)
                    {
                        task1.Increment(10);
                        task2.Increment(10);
                        Thread.Sleep(50);
                    }
                });

            Console.Clear();

            var root = new Tree($"[{GraphicSettings.AccentColor}]📊 System Information[/]")
            {
                Style = new Style(GraphicSettings.GetColor(GraphicSettings.AccentColor), null, Decoration.None)
            };

            var generalNode = root.AddNode($"[{GraphicSettings.AccentColor}]📋 General Information[/]");
            generalNode.AddNode(EscapeMarkup($"💻 Computer Name: [{GraphicSettings.SecondaryColor}]{Environment.MachineName}[/]"));
            generalNode.AddNode(EscapeMarkup($"👤 User Name: [{GraphicSettings.SecondaryColor}]{Environment.UserName}[/]"));
            generalNode.AddNode(EscapeMarkup($"🏢 Domain: [{GraphicSettings.SecondaryColor}]{Environment.UserDomainName}[/]"));
            generalNode.AddNode(EscapeMarkup($"👑 Admin Rights: [{GraphicSettings.SecondaryColor}]{(IsUserAdministrator() ? "Yes" : "No")}[/]"));
            generalNode.AddNode(EscapeMarkup($"⏱️ System Uptime: [{GraphicSettings.SecondaryColor}]{TimeSpan.FromMilliseconds(Environment.TickCount):dd\\.hh\\:mm\\:ss}[/]"));
            generalNode.AddNode(EscapeMarkup($"🔢 Processors: [{GraphicSettings.SecondaryColor}]{Environment.ProcessorCount}[/]"));

            var osNode = root.AddNode($"[{GraphicSettings.AccentColor}]💿 Operating System[/]");
            osNode.AddNode(EscapeMarkup($"🏷️ OS Version: [{GraphicSettings.SecondaryColor}]{Environment.OSVersion}[/]"));
            osNode.AddNode(EscapeMarkup($"⚡ 64-bit OS: [{GraphicSettings.SecondaryColor}]{(Environment.Is64BitOperatingSystem ? "Yes" : "No")}[/]"));
            osNode.AddNode(EscapeMarkup($"🔧 64-bit Process: [{GraphicSettings.SecondaryColor}]{(Environment.Is64BitProcess ? "Yes" : "No")}[/]"));

            var hardwareNode = root.AddNode($"[{GraphicSettings.AccentColor}]🖥️ Hardware Information[/]");

            try
            {
                string cpuInfo = GetHardwareInfo("Win32_Processor", "Name");
                hardwareNode.AddNode(EscapeMarkup($"💻 CPU: [{GraphicSettings.SecondaryColor}]{TruncateString(cpuInfo, 60)}[/]"));
            }
            catch (Exception ex)
            {
                hardwareNode.AddNode(EscapeMarkup($"[red]CPU Error: {ex.Message}[/]"));
            }

            try
            {
                string gpuInfo = GetHardwareInfo("Win32_VideoController", "Name");
                hardwareNode.AddNode(EscapeMarkup($"🎮 GPU: [{GraphicSettings.SecondaryColor}]{TruncateString(gpuInfo, 60)}[/]"));
            }
            catch (Exception ex)
            {
                hardwareNode.AddNode(EscapeMarkup($"[red]GPU Error: {ex.Message}[/]"));
            }

            try
            {
                string ramInfo = GetHardwareInfo("Win32_ComputerSystem", "TotalPhysicalMemory");
                if (!string.IsNullOrEmpty(ramInfo) && long.TryParse(ramInfo, out long ramBytes))
                {
                    double ramGB = ramBytes / (1024.0 * 1024.0 * 1024.0);
                    hardwareNode.AddNode(EscapeMarkup($"🧠 RAM: [{GraphicSettings.SecondaryColor}]{ramGB:F2} GB[/]"));
                }
                else
                {
                    hardwareNode.AddNode(EscapeMarkup($"🧠 RAM: [{GraphicSettings.SecondaryColor}]Information unavailable[/]"));
                }
            }
            catch (Exception ex)
            {
                hardwareNode.AddNode(EscapeMarkup($"[red]RAM Error: {ex.Message}[/]"));
            }

            var dotnetNode = root.AddNode($"[{GraphicSettings.AccentColor}]🔷 .NET Information[/]");
            dotnetNode.AddNode(EscapeMarkup($"📦 .NET Version: [{GraphicSettings.SecondaryColor}]{Environment.Version}[/]"));

            var storageNode = root.AddNode($"[{GraphicSettings.AccentColor}]💾 Storage Information[/]");
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Take(5);
                foreach (var drive in drives)
                {
                    double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    double freeGB = drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    double usedPercent = 100 - (drive.TotalFreeSpace * 100 / drive.TotalSize);

                    string statusColor = usedPercent > 90 ? "red" : usedPercent > 70 ? "yellow" : "green";

                    storageNode.AddNode(EscapeMarkup(
                        $"{drive.Name} {drive.VolumeLabel} | " +
                        $"[{statusColor}]{usedPercent:F1}% used[/] | " +
                        $"[{GraphicSettings.AccentColor}]{freeGB:F1} GB free of {totalGB:F1} GB[/]"));
                }
            }
            catch (Exception ex)
            {
                storageNode.AddNode(EscapeMarkup($"[red]Drive Error: {ex.Message}[/]"));
            }

            AnsiConsole.Write(root);

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[{GraphicSettings.NeutralColor}]Press any key to continue...[/]").RuleStyle(GraphicSettings.SecondaryColor).LeftJustified());

            Console.ReadKey();

            Console.Clear();
            ShowDriveInfo();

            if (AnsiConsole.Confirm($"\n[{GraphicSettings.AccentColor}]Do you want to create a detailed report file?[/]", true))
            {
                Main_Information_Collection();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }

    private static string EscapeMarkup(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = text.Replace("\\[", "TEMP_OPEN")
                   .Replace("\\]", "TEMP_CLOSE")
                   .Replace("[", "\\[")
                   .Replace("]", "\\]")
                   .Replace("TEMP_OPEN", "\\[")
                   .Replace("TEMP_CLOSE", "\\]");

        return text;
    }

    private static string TruncateString(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text[..(maxLength - 3)] + "...";
    }

    public static void ShowDriveInfo()
    {
        var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

        var table = new Table()
            .Title($"[{GraphicSettings.AccentColor}]Storage Drives[/]")
            .Border(TableBorder.Rounded)
            .BorderColor(GraphicSettings.GetThemeColor)
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Drive[/]").Centered())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Label[/]").LeftAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Type[/]").Centered())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Total[/]").RightAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Free[/]").RightAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Usage[/]").Centered());

        foreach (var drive in drives)
        {
            double freePercent = (double)drive.TotalFreeSpace / drive.TotalSize * 100;
            string usageBar = GetUsageBar(freePercent);
            string color = freePercent > 20 ? "green" : freePercent > 10 ? "yellow" : "red";

            table.AddRow(
                $"[{GraphicSettings.AccentColor}]{drive.Name}[/]",
                $"[{GraphicSettings.SecondaryColor}]{drive.VolumeLabel}[/]",
                $"[{GraphicSettings.NeutralColor}]{drive.DriveType}[/]",
                $"[{GraphicSettings.SecondaryColor}]{drive.TotalSize / 1_000_000_000:N0} GB[/]",
                $"[{color}]{drive.TotalFreeSpace / 1_000_000_000:N0} GB[/]",
                $"[{color}]{usageBar} {freePercent:N1}%[/]");
        }

        AnsiConsole.Write(table);
    }

    private static string GetUsageBar(double percent)
    {
        int filled = (int)(percent / 100 * 10);
        int empty = 10 - filled;
        return $"[{new string('█', filled)}{new string('░', empty)}]";
    }
}