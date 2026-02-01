using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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

    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;
    const uint SRCCOPY = 0x00CC0020;


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
            
            AnsiConsole.Write(new Rule("[DarkOrange]System Information report[/]").RuleStyle("white").LeftJustified());
            AnsiConsole.WriteLine();

            
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            
            var generalInfo = new Panel(
                $"[bold]General Information[/]\n\n" +
                $"Computer: [white]{Environment.MachineName}[/]\n" +
                $"User: [white]{Environment.UserName}[/]\n" +
                $"OS: [white]{Environment.OSVersion}[/]\n" +
                $"Processors: [white]{Environment.ProcessorCount}[/]\n" +
                $"Admin: [white]{(IsUserAdministrator() ? "Yes" : "No")}[/]\n" +
                $"Uptime: [white]{TimeSpan.FromMilliseconds(Environment.TickCount):dd\\.hh\\:mm\\:ss}[/]")
                .BorderColor(Spectre.Console.Color.Green)
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
                $"[bold]Hardware Information[/]\n\n" +
                $"CPU: [white]{cpuInfo}[/]\n" +
                $"GPU: [white]{gpuInfo}[/]\n" +
                $"64-bit OS: [white]{(Environment.Is64BitOperatingSystem ? "Yes" : "No")}[/]\n" +
                $".NET: [white]{Environment.Version}[/]")
                .BorderColor(Spectre.Console.Color.DarkOrange)
                .Padding(1, 1);

            grid.AddRow(generalInfo, hardwareInfo);
            AnsiConsole.Write(grid);

            
            AnsiConsole.WriteLine();
            ShowDriveInfoSimple();

            
            AnsiConsole.WriteLine();
            if (AnsiConsole.Confirm("[DarkOrange]Create detailed report file?[/]", true))
            {
                CreateDetailedReport();
            }

            Console.WriteLine("\nPress any key to continue...");
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
                .Title("Storage Drives")
                .Border(TableBorder.Simple)
                .AddColumn("Drive")
                .AddColumn("Label")
                .AddColumn("Type")
                .AddColumn("Total")
                .AddColumn("Free")
                .AddColumn("Usage");

            foreach (var drive in drives)
            {
                double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                double freeGB = drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                double usedPercent = 100 - (drive.TotalFreeSpace * 100 / drive.TotalSize);

                string usageBar = GetSimpleUsageBar(usedPercent);

                table.AddRow(
                    $"[bold]{drive.Name}[/]",
                    $"{drive.VolumeLabel}",
                    $"{drive.DriveType}",
                    $"{totalGB:F1} GB",
                    $"{freeGB:F1} GB",
                    $"{usageBar} {usedPercent:F1}%");
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

            AnsiConsole.MarkupLine($"[green]Report created: {reportFile}[/]");

            
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
    
    public static void TakeScreenshotMenu()
    {
        Console.Clear();
        TextDisplay.TypeWrite("=== SCREENSHOT MENU ===", 5);
        Console.WriteLine();

        TextDisplay.TypeWrite("1 - Create screenshot (default location)", 5);
        TextDisplay.TypeWrite("2 - Create screenshot with custom name", 5);
        TextDisplay.TypeWrite("3 - Create multiple screenshots", 5);
        TextDisplay.TypeWrite("4 - Open screenshots folder", 5);
        TextDisplay.TypeWrite("0 - Back to main menu", 5);

        Console.Write("\nChoose option: ");
        string choice = Console.ReadLine();

        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string folderPath = Path.Combine(desktopPath, "SystemReport");

        switch (choice)
        {
            case "1":
                TakeScreenshot();
                break;
            case "2":
                Console.Write("Enter file name (without extension): ");
                string fileName = Console.ReadLine();
                TakeScreenshotWithOptions(folderPath, $"{fileName}.jpg");
                break;
            case "3":
                Console.Write("How many screenshots? ");
                if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
                {
                    Console.Write("Delay between screenshots (ms): ");
                    if (int.TryParse(Console.ReadLine(), out int delay))
                    {
                        CreateMultipleScreenshots(count, delay, folderPath);
                    }
                }
                break;
            case "4":
                if (Directory.Exists(folderPath))
                {
                    Process.Start("explorer.exe", folderPath);
                }
                else
                {
                    Console.WriteLine("Folder doesn't exist!");
                }
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid choice!");
                break;
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    
    private static void CreateMultipleScreenshots(int count, int delayMs, string folderPath)
    {
        Console.WriteLine($"Creating {count} screenshots with {delayMs}ms delay...");

        for (int i = 1; i <= count; i++)
        {
            Console.WriteLine($"[{i}/{count}] Creating screenshot...");
            string fileName = $"screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{i}.jpg";
            string filePath = Path.Combine(folderPath, fileName);

            if (TakeScreenshotToFile(filePath))
            {
                Console.WriteLine($"  ✓ Saved: {fileName}");
            }
            else
            {
                Console.WriteLine($"  ✗ Failed to create: {fileName}");
            }

            if (i < count)
            {
                Thread.Sleep(delayMs);
            }
        }

        Console.WriteLine($"\nFinished! Created {count} screenshots in: {folderPath}");
    }
    
    public static void TakeScreenshot()
    {
        
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string folderPath = Path.Combine(desktopPath, "SystemReport");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Console.WriteLine($"[+] Папка создана: {folderPath}");
        }

        
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string screenshotPath = Path.Combine(folderPath, $"screenshot_{timestamp}.jpg");

        Console.WriteLine("[+] Создание скриншота...");

        IntPtr hdcSrc = IntPtr.Zero;
        IntPtr hdcDest = IntPtr.Zero;
        IntPtr hBitmap = IntPtr.Zero;
        IntPtr hOldBitmap = IntPtr.Zero;

        try
        {
            
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            
            IntPtr desktopWindow = GetDesktopWindow();
            hdcSrc = GetWindowDC(desktopWindow);
            hdcDest = CreateCompatibleDC(hdcSrc);
            hBitmap = CreateCompatibleBitmap(hdcSrc, screenWidth, screenHeight);
            hOldBitmap = SelectObject(hdcDest, hBitmap);

            
            BitBlt(hdcDest, 0, 0, screenWidth, screenHeight, hdcSrc, 0, 0, SRCCOPY);

            
#pragma warning disable CA1416 
            using (Bitmap bitmap = Image.FromHbitmap(hBitmap))
            {
                
#pragma warning disable CA1416 
                bitmap.Save(screenshotPath, ImageFormat.Jpeg);
#pragma warning restore CA1416 
            }

            Console.WriteLine($"[+] Скриншот сохранен: {screenshotPath}");
            Console.WriteLine($"[+] Размер: {screenWidth}x{screenHeight} пикселей");

            
            Console.WriteLine("[*] Открытие папки со скриншотом...");
            Process.Start("explorer.exe", folderPath);

            
            if (File.Exists(screenshotPath))
            {
                Console.WriteLine($"[+] Размер файла: {new FileInfo(screenshotPath).Length / 1024} KB");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании скриншота: {ex.Message}");
        }
        finally
        {
            if (hOldBitmap != IntPtr.Zero) SelectObject(hdcDest, hOldBitmap);
            if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
            if (hdcDest != IntPtr.Zero) DeleteDC(hdcDest);
            if (hdcSrc != IntPtr.Zero) ReleaseDC(GetDesktopWindow(), hdcSrc);
        }
    }
    
    public static string TakeScreenshotWithOptions(string folderPath = null, string fileName = null)
    {
        
        if (string.IsNullOrEmpty(folderPath))
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            folderPath = Path.Combine(desktopPath, "SystemReport");
        }

        
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Console.WriteLine($"[+] Папка создана: {folderPath}");
        }

        
        if (string.IsNullOrEmpty(fileName))
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            fileName = $"screenshot_{timestamp}.jpg";
        }

        string screenshotPath = Path.Combine(folderPath, fileName);

        Console.WriteLine($"[+] Создание скриншота в: {screenshotPath}");

        
        if (TakeScreenshotToFile(screenshotPath))
        {
            Console.WriteLine($"[✓] Скриншот успешно создан!");
            Console.WriteLine($"[*] Файл: {screenshotPath}");
            return screenshotPath;
        }
        else
        {
            Console.WriteLine($"[✗] Не удалось создать скриншот!");
            return null;
        }
    }

    
    private static bool TakeScreenshotToFile(string filePath)
    {
        IntPtr hdcSrc = IntPtr.Zero;
        IntPtr hdcDest = IntPtr.Zero;
        IntPtr hBitmap = IntPtr.Zero;
        IntPtr hOldBitmap = IntPtr.Zero;

        try
        {
            
            int screenWidth = GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = GetSystemMetrics(SM_CYSCREEN);

            Console.WriteLine($"[*] Размер экрана: {screenWidth}x{screenHeight}");

            
            IntPtr desktopWindow = GetDesktopWindow();
            hdcSrc = GetWindowDC(desktopWindow);
            hdcDest = CreateCompatibleDC(hdcSrc);
            hBitmap = CreateCompatibleBitmap(hdcSrc, screenWidth, screenHeight);
            hOldBitmap = SelectObject(hdcDest, hBitmap);

            
            BitBlt(hdcDest, 0, 0, screenWidth, screenHeight, hdcSrc, 0, 0, SRCCOPY);


#pragma warning disable CA1416
            using Bitmap bitmap = Image.FromHbitmap(hBitmap);

#pragma warning disable CA1416
            bitmap.Save(filePath, ImageFormat.Jpeg);
#pragma warning restore CA1416


            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании скриншота: {ex.Message}");
            return false;
        }
        finally
        {
            if (hOldBitmap != IntPtr.Zero) SelectObject(hdcDest, hOldBitmap);
            if (hBitmap != IntPtr.Zero) DeleteObject(hBitmap);
            if (hdcDest != IntPtr.Zero) DeleteDC(hdcDest);
            if (hdcSrc != IntPtr.Zero) ReleaseDC(GetDesktopWindow(), hdcSrc);
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
                    var task1 = ctx.AddTask("[green]Collecting hardware info[/]");
                    var task2 = ctx.AddTask("[blue]Gathering system data[/]");

                    
                    for (int i = 0; i < 100; i += 10)
                    {
                        task1.Increment(10);
                        task2.Increment(10);
                        Thread.Sleep(50);
                    }
                });

            Console.Clear();



            var root = new Tree("[bold white]📊 System Information[/]")
            {
                Style = new Style(Spectre.Console.Color.DarkOrange, null, Decoration.None)
            };



            var generalNode = root.AddNode("[DarkOrange]📋 General Information[/]");
            generalNode.AddNode(EscapeMarkup($"💻 Computer Name: {Environment.MachineName}"));
            generalNode.AddNode(EscapeMarkup($"👤 User Name: {Environment.UserName}"));
            generalNode.AddNode(EscapeMarkup($"🏢 Domain: {Environment.UserDomainName}"));
            generalNode.AddNode(EscapeMarkup($"👑 Admin Rights: {(IsUserAdministrator() ? "Yes" : "No")}"));
            generalNode.AddNode(EscapeMarkup($"⏱️ System Uptime: {TimeSpan.FromMilliseconds(Environment.TickCount):dd\\.hh\\:mm\\:ss}"));
            generalNode.AddNode(EscapeMarkup($"🔢 Processors: {Environment.ProcessorCount}"));

            
            var osNode = root.AddNode("[DarkOrange]💿 Operating System[/]");
            osNode.AddNode(EscapeMarkup($"🏷️ OS Version: {Environment.OSVersion}"));
            osNode.AddNode(EscapeMarkup($"⚡ 64-bit OS: {(Environment.Is64BitOperatingSystem ? "Yes" : "No")}"));
            osNode.AddNode(EscapeMarkup($"🔧 64-bit Process: {(Environment.Is64BitProcess ? "Yes" : "No")}"));

            
            var hardwareNode = root.AddNode("[DarkOrange]🖥️ Hardware Information[/]");

            try
            {
                string cpuInfo = GetHardwareInfo("Win32_Processor", "Name");
                hardwareNode.AddNode(EscapeMarkup($"💻 CPU: {TruncateString(cpuInfo, 60)}"));
            }
            catch (Exception ex)
            {
                hardwareNode.AddNode(EscapeMarkup($"[red]CPU Error: {ex.Message}[/]"));
            }

            try
            {
                string gpuInfo = GetHardwareInfo("Win32_VideoController", "Name");
                hardwareNode.AddNode(EscapeMarkup($"🎮 GPU: {TruncateString(gpuInfo, 60)}"));
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
                    hardwareNode.AddNode(EscapeMarkup($"🧠 RAM: {ramGB:F2} GB"));
                }
                else
                {
                    hardwareNode.AddNode(EscapeMarkup($"🧠 RAM: Information unavailable"));
                }
            }
            catch (Exception ex)
            {
                hardwareNode.AddNode(EscapeMarkup($"[red]RAM Error: {ex.Message}[/]"));
            }

            
            var dotnetNode = root.AddNode("[DarkOrange]🔷 .NET Information[/]");
            dotnetNode.AddNode(EscapeMarkup($"📦 .NET Version: {Environment.Version}"));

            
            var storageNode = root.AddNode("[DarkOrange]💾 Storage Information[/]");
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Take(5);
                foreach (var drive in drives)
                {
                    double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    double freeGB = drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                    double usedPercent = 100 - ((drive.TotalFreeSpace * 100) / drive.TotalSize);

                    string statusColor = usedPercent > 90 ? "red" : usedPercent > 70 ? "yellow" : "green";

                    storageNode.AddNode(EscapeMarkup(
                        $"{drive.Name} {drive.VolumeLabel} | " +
                        $"[{statusColor}]{usedPercent:F1}% used[/] | " +
                        $"[DarkOrange]{freeGB:F1} GB free of {totalGB:F1} GB[/]"));
                }
            }
            catch (Exception ex)
            {
                storageNode.AddNode(EscapeMarkup($"[red]Drive Error: {ex.Message}[/]"));
            }

            
            AnsiConsole.Write(root);

            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[yellow]Press any key to continue...[/]").RuleStyle("yellow").Centered());

            
            Console.ReadKey();

            
            Console.Clear();
            ShowDriveInfo();

            
            if (AnsiConsole.Confirm("\n[yellow]Do you want to create a detailed report file?[/]", true))
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
            .Title("[bold DarkOrange]Storage Drives[/]")
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[cyan]Drive[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Label[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Type[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Total[/]").RightAligned())
            .AddColumn(new TableColumn("[cyan]Free[/]").RightAligned())
            .AddColumn(new TableColumn("[cyan]Usage[/]").Centered());

        foreach (var drive in drives)
        {
            double freePercent = (double)drive.TotalFreeSpace / drive.TotalSize * 100;
            string usageBar = GetUsageBar(freePercent);
            string color = freePercent > 20 ? "green" : freePercent > 10 ? "yellow" : "red";

            table.AddRow(
                $"[bold]{drive.Name}[/]",
                $"[white]{drive.VolumeLabel}[/]",
                $"[grey]{drive.DriveType}[/]",
                $"[blue]{drive.TotalSize / 1_000_000_000:N0} GB[/]",
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
