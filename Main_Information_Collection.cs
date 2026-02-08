using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Linq;
using Spectre.Console;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.Generic;

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

    static bool IsUserAdministrator()
    {
        try
        {
            var identity = WindowsIdentity.GetCurrent();

            var principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);

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
                sw.WriteLine($"Generated by: Task Manager T4");
                sw.WriteLine();

                sw.WriteLine("=== GENERAL INFO ===");
                sw.WriteLine($"Computer: {Environment.MachineName}");
                sw.WriteLine($"User: {Environment.UserName}");
                sw.WriteLine($"Domain: {Environment.UserDomainName}");
                sw.WriteLine($"OS: {Environment.OSVersion}");
                sw.WriteLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
                sw.WriteLine($".NET Version: {Environment.Version}");
                sw.WriteLine($"Processors: {Environment.ProcessorCount}");
                sw.WriteLine($"Admin: {IsUserAdministrator()}");
                sw.WriteLine($"System Directory: {Environment.SystemDirectory}");
                sw.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
                sw.WriteLine($"Uptime: {TimeSpan.FromMilliseconds(Environment.TickCount):dd\\.hh\\:mm\\:ss}");
                sw.WriteLine();


                sw.WriteLine("=== HARDWARE INFO ===");
                try
                {

                    sw.WriteLine("[CPU]");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                        {
                            sw.WriteLine($"  Name: {obj["Name"]}");
                            sw.WriteLine($"  Manufacturer: {obj["Manufacturer"]}");
                            sw.WriteLine($"  Cores: {obj["NumberOfCores"]}");
                            sw.WriteLine($"  Threads: {obj["NumberOfLogicalProcessors"]}");
                            sw.WriteLine($"  Current Clock: {obj["CurrentClockSpeed"]} MHz");
                            sw.WriteLine($"  Max Clock: {obj["MaxClockSpeed"]} MHz");
                            sw.WriteLine($"  L2 Cache: {obj["L2CacheSize"]} KB");
                            sw.WriteLine($"  L3 Cache: {obj["L3CacheSize"]} KB");
                            sw.WriteLine($"  Architecture: {GetArchitecture(Convert.ToInt32(obj["Architecture"]))}");
                            sw.WriteLine($"  Socket: {obj["SocketDesignation"]}");
                        }
                    }
                    sw.WriteLine();

                    sw.WriteLine("[GPU]");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                        {
                            sw.WriteLine($"  Name: {obj["Name"]}");
                            sw.WriteLine($"  Adapter RAM: {Convert.ToUInt64(obj["AdapterRAM"]) / (1024 * 1024)} MB");
                            sw.WriteLine($"  Driver Version: {obj["DriverVersion"]}");
                            sw.WriteLine($"  Driver Date: {obj["DriverDate"]}");
                            sw.WriteLine($"  Resolution: {obj["CurrentHorizontalResolution"]}x{obj["CurrentVerticalResolution"]}");
                            sw.WriteLine($"  Color Depth: {obj["CurrentBitsPerPixel"]} bits");
                            sw.WriteLine($"  Refresh Rate: {obj["CurrentRefreshRate"]} Hz");
                        }
                    }
                    sw.WriteLine();

                    sw.WriteLine("[RAM]");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
                    {
                        ulong totalRam = 0;
                        int ramSlots = 0;

                        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                        {
                            ramSlots++;
                            ulong capacity = Convert.ToUInt64(obj["Capacity"]);
                            totalRam += capacity;

                            sw.WriteLine($"  Slot {ramSlots}:");
                            sw.WriteLine($"    Capacity: {capacity / (1024 * 1024 * 1024)} GB");
                            sw.WriteLine($"    Speed: {obj["Speed"]} MHz");
                            sw.WriteLine($"    Manufacturer: {obj["Manufacturer"]}");
                            sw.WriteLine($"    Part Number: {obj["PartNumber"]}");
                            sw.WriteLine($"    Serial: {obj["SerialNumber"]}");
                            sw.WriteLine($"    Type: {GetMemoryType(Convert.ToInt32(obj["MemoryType"]))}");
                        }

                        sw.WriteLine($"  Total RAM: {totalRam / (1024 * 1024 * 1024)} GB ({ramSlots} slots)");
                    }
                    sw.WriteLine();

                    sw.WriteLine("[MOTHERBOARD]");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                    {
                        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                        {
                            sw.WriteLine($"  Manufacturer: {obj["Manufacturer"]}");
                            sw.WriteLine($"  Model: {obj["Product"]}");
                            sw.WriteLine($"  Version: {obj["Version"]}");
                            sw.WriteLine($"  Serial: {obj["SerialNumber"]}");
                        }
                    }
                    sw.WriteLine();

                    sw.WriteLine("[BIOS]");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS"))
                    {
                        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                        {
                            sw.WriteLine($"  Manufacturer: {obj["Manufacturer"]}");
                            sw.WriteLine($"  Version: {obj["Version"]}");
                            sw.WriteLine($"  Serial: {obj["SerialNumber"]}");
                            sw.WriteLine($"  Release Date: {obj["ReleaseDate"]}");
                        }
                    }
                    sw.WriteLine();

                    sw.WriteLine("[MONITORS]");
                    using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor"))
                    {
                        int monitorCount = 0;
                        foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                        {
                            monitorCount++;
                            sw.WriteLine($"  Monitor {monitorCount}:");
                            sw.WriteLine($"    Name: {obj["Name"]}");
                            sw.WriteLine($"    Screen Height: {obj["ScreenHeight"]}");
                            sw.WriteLine($"    Screen Width: {obj["ScreenWidth"]}");
                            sw.WriteLine($"    PNP Device ID: {obj["PNPDeviceID"]}");
                        }
                        sw.WriteLine($"  Total Monitors: {monitorCount}");
                    }
                    sw.WriteLine();

                    sw.WriteLine("[NETWORK ADAPTERS]");
                    var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var ni in networkInterfaces)
                    {
                        if (ni.OperationalStatus == OperationalStatus.Up)
                        {
                            sw.WriteLine($"  Adapter: {ni.Name}");
                            sw.WriteLine($"    Type: {ni.NetworkInterfaceType}");
                            sw.WriteLine($"    Description: {ni.Description}");
                            sw.WriteLine($"    MAC: {ni.GetPhysicalAddress()}");
                            sw.WriteLine($"    Speed: {ni.Speed / 1000000} Mbps");
                            sw.WriteLine($"    Status: {ni.OperationalStatus}");
                            var stats = ni.GetIPv4Statistics();
                            sw.WriteLine($"    Bytes Received: {stats.BytesReceived / (1024 * 1024)} MB");
                            sw.WriteLine($"    Bytes Sent: {stats.BytesSent / (1024 * 1024)} MB");
                        }
                    }
                    sw.WriteLine();

                }
                catch (Exception ex)
                {
                    sw.WriteLine($"Hardware info error: {ex.Message}");
                }

                sw.WriteLine("=== STORAGE INFO ===");
                try
                {
                    var physicalDisks = new Dictionary<string, (string Model, string Serial)>();

                    try
                    {
                        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                        foreach (ManagementObject disk in searcher.Get().Cast<ManagementObject>())
                        {
                            string deviceId = disk["DeviceID"]?.ToString() ?? "";
                            string model = disk["Model"]?.ToString()?.Trim() ?? "Unknown Model";
                            string serial = disk["SerialNumber"]?.ToString()?.Trim() ?? "Unknown Serial";

                            physicalDisks[deviceId] = (model, serial);

                        }
                    }
                    catch (Exception e)
                    {
                        sw.WriteLine($"  WMI Disk Info Error: {e.Message}");
                    }

                    foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                    {
                        try
                        {
                            double totalGB = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                            double freeGB = drive.TotalFreeSpace / (1024.0 * 1024.0 * 1024.0);
                            double usedGB = totalGB - freeGB;
                            double usedPercent = totalGB > 0 ? usedGB / totalGB * 100 : 0;

                            sw.WriteLine($"[{drive.Name.TrimEnd('\\')}]");

                            (string Model, string Serial) diskInfo = ("Unknown Model", "Unknown Serial");

                            using (var partitionSearcher = new ManagementObjectSearcher(
                                $"ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{drive.Name.Replace("\\", "")}'}} WHERE ResultClass=Win32_DiskPartition"))
                            {
                                foreach (ManagementObject partition in partitionSearcher.Get().Cast<ManagementObject>())
                                {
                                    using var diskSearcher = new ManagementObjectSearcher(
                                        $"ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} WHERE ResultClass=Win32_DiskDrive");
                                    foreach (ManagementObject disk in diskSearcher.Get().Cast<ManagementObject>())
                                    {
                                        string deviceId = disk["DeviceID"]?.ToString() ?? "";
                                        if (physicalDisks.TryGetValue(deviceId, out var info))
                                        {
                                            diskInfo = info;
                                        }

                                    }
                                }
                            }
                            sw.WriteLine($"  Model: {diskInfo.Model}");
                            sw.WriteLine($"  Serial: {diskInfo.Serial}");
                            sw.WriteLine($"  Label: {drive.VolumeLabel}");
                            sw.WriteLine($"  Type: {drive.DriveType}");
                            sw.WriteLine($"  Format: {drive.DriveFormat}");
                            sw.WriteLine($"  Total: {totalGB:F2} GB");
                            sw.WriteLine($"  Free: {freeGB:F2} GB");
                            sw.WriteLine($"  Used: {usedGB:F2} GB ({usedPercent:F1}%)");
                            sw.WriteLine($"  Available: {drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0):F2} GB");
                            sw.WriteLine();
                        }
                        catch (Exception ex)
                        {
                            sw.WriteLine($"  Error processing drive {drive.Name}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    sw.WriteLine($"Storage info error: {ex.Message}");
                }

                sw.WriteLine();

                sw.WriteLine("=== SOFTWARE INFO ===");
                try
                {

                    sw.WriteLine($"Running Processes: {Process.GetProcesses().Length}");

                    sw.WriteLine("[.NET FRAMEWORK]");
                    string frameworkPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.NET\\Framework");
                    if (Directory.Exists(frameworkPath))
                    {
                        foreach (var dir in Directory.GetDirectories(frameworkPath))
                        {
                            string dirName = Path.GetFileName(dir);
                            if (dirName.StartsWith("v") && !dirName.Contains("64") && !dirName.Contains("Client"))
                            {
                                sw.WriteLine($"  {dirName}");
                            }
                        }
                    }

                    sw.WriteLine("[ENVIRONMENT VARIABLES]");
                    sw.WriteLine($"  OS: {Environment.GetEnvironmentVariable("OS")}");
                    sw.WriteLine($"  PROCESSOR_ARCHITECTURE: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")}");
                    sw.WriteLine($"  NUMBER_OF_PROCESSORS: {Environment.GetEnvironmentVariable("NUMBER_OF_PROCESSORS")}");
                    sw.WriteLine($"  TEMP: {Environment.GetEnvironmentVariable("TEMP")}");
                    sw.WriteLine($"  PATH: {Environment.GetEnvironmentVariable("PATH")?.Substring(0, Math.Min(100, Environment.GetEnvironmentVariable("PATH")?.Length ?? 0))}...");
                }
                catch (Exception ex)
                {
                    sw.WriteLine($"Software info error: {ex.Message}");
                }
                sw.WriteLine();

                sw.WriteLine("=== NETWORK INFO ===");
                try
                {
                    string hostName = Dns.GetHostName();
                    sw.WriteLine($"Host Name: {hostName}");

                    IPAddress[] addresses = Dns.GetHostAddresses(hostName);
                    sw.WriteLine("IP Addresses:");
                    foreach (IPAddress address in addresses)
                    {
                        sw.WriteLine($"  {address} ({address.AddressFamily})");
                    }

                    var ipProps = IPGlobalProperties.GetIPGlobalProperties();
                    sw.WriteLine($"Active TCP Connections: {ipProps.GetActiveTcpConnections().Length}");
                    sw.WriteLine($"Active UDP Listeners: {ipProps.GetActiveUdpListeners().Length}");
                }
                catch (Exception ex)
                {
                    sw.WriteLine($"Network info error: {ex.Message}");
                }
                sw.WriteLine();

                sw.WriteLine("=== SECURITY INFO ===");
                try
                {
                    sw.WriteLine($"Running as Admin: {IsUserAdministrator()}");
                    sw.WriteLine($"User Interactive: {Environment.UserInteractive}");
                    sw.WriteLine($"System Page Size: {Environment.SystemPageSize} bytes");

                    sw.WriteLine("[SECURITY SOFTWARE]");
                    using var searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntivirusProduct");
                    foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                    {
                        sw.WriteLine($"  Product: {obj["displayName"]}");
                        sw.WriteLine($"    State: {GetAntivirusState(Convert.ToInt32(obj["productState"]))}");

                    }
                }
                catch (Exception ex)
                {
                    sw.WriteLine($"Security info error: {ex.Message}");
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

    private static string GetArchitecture(int archCode)
    {
        return archCode switch
        {
            0 => "x86",
            1 => "MIPS",
            2 => "Alpha",
            3 => "PowerPC",
            5 => "ARM",
            6 => "IA64",
            9 => "x64",
            12 => "ARM64",
            _ => $"Unknown ({archCode})"
        };
    }

    private static string GetMemoryType(int typeCode)
    {
        return typeCode switch
        {
            0 => "Unknown",
            1 => "Other",
            2 => "DRAM",
            3 => "Synchronous DRAM",
            4 => "Cache DRAM",
            5 => "EDO",
            6 => "EDRAM",
            7 => "VRAM",
            8 => "SRAM",
            9 => "RAM",
            10 => "ROM",
            11 => "Flash",
            12 => "EEPROM",
            13 => "FEPROM",
            14 => "EPROM",
            15 => "CDRAM",
            16 => "3DRAM",
            17 => "SDRAM",
            18 => "SGRAM",
            19 => "RDRAM",
            20 => "DDR",
            21 => "DDR2",
            22 => "DDR2 FB-DIMM",
            24 => "DDR3",
            25 => "FBD2",
            26 => "DDR4",
            _ => $"Unknown ({typeCode})"
        };
    }

    private static string GetAntivirusState(int state)
    {
        // Product state is a 32-bit integer where:
        // - Bits 0-7: Product state
        // - Bits 8-15: Signatures up to date (0 = No, 1 = Yes)
        // - Bits 16-23: Product enabled (0 = No, 1 = Yes)

        bool enabled = ((state >> 16) & 0xFF) == 1;
        bool upToDate = ((state >> 8) & 0xFF) == 1;

        return $"Enabled: {enabled}, Up to date: {upToDate}";
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