using System;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Spectre.Console;
using System.Threading.Tasks;
using System.Net.Http;

namespace Task_Manager_T4;

public class NetWork
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] localPort;
        public uint remoteAddr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] remotePort;
        public uint owningPid;
    }

    [DllImport("iphlpapi.dll", SetLastError = true)]
    static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, int tblClass, uint reserved = 0);

    public static async Task MainMenuNetWork()
    {
        while (true)
        {
            Console.Clear();

            AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Network Tools (beta version)[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{GraphicSettings.SecondaryColor}]Select network function:[/]")
                    .PageSize(GraphicSettings.PageSize)
                    .AddChoices([
                    "COMPUTER_NETWORK_INFO",
                    "Display Network Adapters",
                    "Show Process Network Table",
                    "test001",
                    "test002",
                    "test003",
                    "return"
                    ]));

            switch (choice)
            {
                case "COMPUTER_NETWORK_INFO":
                    COMPUTER_NETWORK_INFO();
                    Console.Clear();
                    break;
                case "Show Process Network Table":
                    ShowProcessNetworkTable();
                    Console.Clear();
                    break;
                case "Display Network Adapters":
                    DisplayNetworkAdapters();
                    Console.Clear();
                    break;
                case "test001":
                    Test001Async().GetAwaiter().GetResult();
                    Console.Clear();
                    break;
                case "test002":
                    Test002();
                    Console.Clear();
                    break;
                case "test003":
                    Test003();
                    Console.Clear();
                    break;
                case "return":
                    await Program.Function_list();
                    break;
            }
        }
    }

    private static void Test003()
    {
        string FirstSite = "https://google.com"; //tut site dlya testa
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.AccentColor}]Internet Speed Test[/]").RuleStyle(GraphicSettings.SecondaryColor).LeftJustified());
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]This method does not display real data.[/]");
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Testing connection speed... Press ESC to exit[/]");
        AnsiConsole.WriteLine();

        Console.CancelKeyPress += (sender, e) => e.Cancel = true;
        Console.TreatControlCAsInput = true;

        while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
        {
            double speedMbps = MeasureSpeedMbps(FirstSite);
            double speedMBps = speedMbps / 8; 

            var liveTable = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(GraphicSettings.GetColor(GraphicSettings.AccentColor))
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Metric[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Value[/]").RightAligned());
            liveTable.AddRow(
                $"[{GraphicSettings.AccentColor}]Download[/]",
                $"[{GraphicSettings.SecondaryColor}]{speedMbps:F2} Mbps[/]"
            );

            liveTable.AddRow(
                $"[{GraphicSettings.AccentColor}]Download[/]",
                $"[{GraphicSettings.SecondaryColor}]{speedMBps:F2} MB/s[/]"
            );

            Console.Clear();
            AnsiConsole.Write(new Rule($"[{GraphicSettings.AccentColor}]Internet Speed Test[/]").RuleStyle(GraphicSettings.SecondaryColor).LeftJustified());
            AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Testing connection speed... Press ESC to exit[/]");
            AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]This method does not display real data.[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.Write(liveTable);

            Thread.Sleep(1000); //tut chastota obnovleniya (1 secunda)
        }

        Console.Clear();
    }

    private static double MeasureSpeedMbps(string url)
    {
        try
        {
            using HttpClient client = new();
            client.Timeout = TimeSpan.FromSeconds(10);

            DateTime startTime = DateTime.Now;

            byte[] data = client.GetByteArrayAsync(url).Result;

            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            double speedMbps = data.Length * 8 / (1024.0 * 1024.0) / duration.TotalSeconds;

            return speedMbps;
        }
        catch (Exception)
        {
            return 0; 
        }
    }


    private static void Test002()
    {
        Console.Clear();
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Enter URL: [/]");
        string user_link = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(user_link))
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]No URL entered. [/]");
            return;
        }

        try
        {
            Uri user_link1 = new(user_link);

            var table = new Table()
                .Border(TableBorder.HeavyEdge)
                .BorderColor(GraphicSettings.GetColor(GraphicSettings.AccentColor))
                .AddColumn($"[{GraphicSettings.AccentColor}]Property[/]")
                .AddColumn($"[{GraphicSettings.AccentColor}]Value[/]");

            table.AddRow("AbsolutePath", user_link1.AbsolutePath);
            table.AddRow("AbsoluteUri", user_link1.AbsoluteUri);
            table.AddRow("Fragment", user_link1.Fragment);
            table.AddRow("Host", user_link1.Host);
            table.AddRow("IsAbsoluteUri", user_link1.IsAbsoluteUri.ToString());
            table.AddRow("IsDefaultPort", user_link1.IsDefaultPort.ToString());
            table.AddRow("IsFile", user_link1.IsFile.ToString());
            table.AddRow("IsLoopback", user_link1.IsLoopback.ToString());
            table.AddRow("OriginalString", user_link1.OriginalString);
            table.AddRow("PathAndQuery", user_link1.PathAndQuery);
            table.AddRow("Port", user_link1.Port.ToString());
            table.AddRow("Query", user_link1.Query);
            table.AddRow("Scheme", user_link1.Scheme);
            table.AddRow("Segments", string.Join(", ", user_link1.Segments));
            table.AddRow("UserInfo", user_link1.UserInfo);
            table.AddRow("DnsSafeHost", user_link1.DnsSafeHost);
            table.AddRow("Authority", user_link1.Authority);

            AnsiConsole.Write(table);
        }
        catch (UriFormatException ex)
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Invalid URL format: {ex.Message}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Error: {ex.Message}[/]");
        }

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Press any key to continue...[/]");
        Console.ReadKey();
    }
    private static async Task Test001Async() 
    {
        Console.Clear();
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Enter hostname to resolve:[/]");
        string hostname = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(hostname))
        {
            AnsiConsole.WriteLine($"[{GraphicSettings.SecondaryColor}]No hostname entered. [/]");
            return;
        }

        try
        {
            var result = await Dns.GetHostEntryAsync(hostname);

            var panel = new Panel(
                $"[{GraphicSettings.AccentColor}]DNS Resolution Result[/]\n\n" +
                $"[{GraphicSettings.SecondaryColor}]Hostname:[/] {result.HostName}\n" +
                $"[{GraphicSettings.SecondaryColor}]IP Addresses:[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(GraphicSettings.GetColor(GraphicSettings.AccentColor)),
                Padding = new Padding(1, 1, 1, 1)
            };

            AnsiConsole.Write(panel);

            var table = new Table()
                .Border(TableBorder.Simple)
                .BorderColor(GraphicSettings.GetColor(GraphicSettings.SecondaryColor))
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Type[/]").Centered())
                .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Address[/]").LeftAligned());

            foreach (var ip in result.AddressList)
            {
                string type = ip.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
                table.AddRow(
                    $"[{GraphicSettings.AccentColor}]{type}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{ip}[/]"
                );
            }

            AnsiConsole.Write(table);
        }
        catch (SocketException ex)
        {
            AnsiConsole.MarkupLine($"[red]DNS resolution failed: {ex.Message}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Press any key to continue...[/]");
        Console.ReadKey();
    }

    private static void DisplayNetworkAdapters()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Network Adapters Information[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        AnsiConsole.WriteLine();

        try
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();

            var panel = new Panel(
                $"[{GraphicSettings.AccentColor}]Total Network Adapters:[/] [{GraphicSettings.SecondaryColor}]{adapters.Length}[/]")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(GraphicSettings.GetColor(GraphicSettings.AccentColor)),
                Padding = new Padding(1, 0, 1, 0)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();

            foreach (NetworkInterface adapter in adapters)
            {
                var adapterPanel = new Panel(
                    $"[{GraphicSettings.AccentColor}]Name:[/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(adapter.Name)}[/]\n" +
                    $"[{GraphicSettings.AccentColor}]Description:[/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(adapter.Description)}[/]\n" +
                    $"[{GraphicSettings.AccentColor}]Type:[/] [{GraphicSettings.SecondaryColor}]{adapter.NetworkInterfaceType}[/]\n" +
                    $"[{GraphicSettings.AccentColor}]MAC Address:[/] [{GraphicSettings.SecondaryColor}]{adapter.GetPhysicalAddress()}[/]\n" +
                    $"[{GraphicSettings.AccentColor}]Status:[/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(adapter.OperationalStatus.ToString())}[/]\n" +
                    $"[{GraphicSettings.AccentColor}]Speed:[/] [{GraphicSettings.SecondaryColor}]{adapter.Speed / 1000000:F1} Mbps[/]\n")
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(GraphicSettings.GetColor(GraphicSettings.AccentColor)),
                    Padding = new Padding(1, 1, 1, 1)
                };

                AnsiConsole.Write(adapterPanel);
                AnsiConsole.WriteLine();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Error retrieving adapter info: {ex.Message}[/]");
        }
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Press any key to return.[/]");
        Console.ReadKey();
    }
    private static void COMPUTER_NETWORK_INFO()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]COMPUTER_NETWORK_INFO[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        AnsiConsole.WriteLine();

        try
        {
            string hostName = Dns.GetHostName();

            var panel = new Panel($"[{GraphicSettings.AccentColor}]Host Name:[/] {hostName}")
            {
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(GraphicSettings.GetThemeColor),
                Padding = new Padding(2, 0, 2, 0)
            };
            AnsiConsole.Write(panel);

            var table = new Table()
                .Border(TableBorder.HeavyEdge)
                .BorderColor(GraphicSettings.GetThemeColor)
                .AddColumn("Тип")
                .AddColumn("Адрес")
                .AddColumn("Физический адрес (MAC)");

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;

                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    string type = addr.Address.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
                    string mac = ni.GetPhysicalAddress().ToString();
                    if (!string.IsNullOrEmpty(mac))
                    {
                        mac = string.Join("-", Enumerable.Range(0, mac.Length / 2)
                                    .Select(i => mac.Substring(i * 2, 2)));
                    }

                    table.AddRow(
                        $"[{GraphicSettings.AccentColor}]{type}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{Markup.Escape(addr.Address.ToString())}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{mac}[/]"
                    );
                }
            }

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][!] Что-то пошло не так:[/] {ex.Message}");
        }

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Нажмите любую клавишу для возврата...[/]");
        Console.ReadKey();
    }

    public static void ShowProcessNetworkTable()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Process_Network_Table(beta)[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        AnsiConsole.WriteLine();

        var mainInterface = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(ni => ni.OperationalStatus == OperationalStatus.Up);
        var stats = mainInterface?.GetIPv4Statistics();

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(GraphicSettings.GetColor(GraphicSettings.AccentColor));

        table.AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]PID[/]").Centered());
        table.AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Process[/]").LeftAligned());
        table.AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Local Address[/]").LeftAligned());
        table.AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Remote Address[/]").LeftAligned());
        table.AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]State[/]").Centered());
        table.AddColumn(new TableColumn($"[{GraphicSettings.AccentColor}]Traffic[/]").RightAligned());

        int bufferSize = 0;
        GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, 2, 5);
        IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);

        try
        {
            if (GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, 2, 5) == 0)
            {
                int rowCount = Marshal.ReadInt32(tcpTablePtr);
                IntPtr rowPtr = checked((IntPtr)((long)tcpTablePtr + 4));

                for (int i = 0; i < rowCount; i++)
                {
                    var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                    int pid = (int)row.owningPid;
                    string pName = "Unknown";

                    try { pName = Process.GetProcessById(pid).ProcessName; } catch { }

                    string trafficInfo = i == 0 ? $"{stats?.BytesReceived / 1024 / 1024} MB Recv | {stats?.BytesSent / 1024 / 1024} MB Sent" : "";

                    table.AddRow(
                        $"[{GraphicSettings.SecondaryColor}]{pid}[/]",
                        $"[{GraphicSettings.AccentColor}]{Markup.Escape(pName)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{new IPAddress(row.localAddr)}:{(row.localPort[0] << 8) + row.localPort[1]}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{new IPAddress(row.remoteAddr)}:{(row.remotePort[0] << 8) + row.remotePort[1]}[/]",
                        $"[{GetTcpStateColor((TcpState)row.state)}]{(TcpState)row.state}[/]",
                        i == 0 ? $"[{GraphicSettings.AccentColor}]{trafficInfo}[/]" : ""
                    );

                    rowPtr = checked((IntPtr)((long)rowPtr + Marshal.SizeOf<MIB_TCPROW_OWNER_PID>()));
                }
            }
        }
        finally
        {
            Marshal.FreeHGlobal(tcpTablePtr);
        }

        AnsiConsole.Write(table);
        if (stats != null)
        {
            AnsiConsole.MarkupLine($"\n[{GraphicSettings.AccentColor}]Session Total:[/] [{GraphicSettings.SecondaryColor}]Received {stats.BytesReceived / 1024 / 1024} MB | Sent {stats.BytesSent / 1024 / 1024} MB[/]");
        }
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}] Press any key to return.[/]");
        Console.ReadKey();
    }

    private static string GetTcpStateColor(TcpState state)
    {
        return state switch
        {
            TcpState.Established => "green",
            TcpState.Listen => "blue",
            TcpState.TimeWait => "yellow",
            TcpState.CloseWait => "orange3",
            TcpState.Closed => "red",
            _ => GraphicSettings.NeutralColor
        };
    }
}