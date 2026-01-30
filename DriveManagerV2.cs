using System;
using System.Management;
using System.Linq;
using Spectre.Console;

namespace ProjectT4;

class DriveManager2
{
    public static void Main_Menu_Drives()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[yellow]Менеджер Драйверов[/]").RuleStyle("grey").LeftJustified());

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Выберите категорию для просмотра драйверов:")
                    .PageSize(10)
                    .AddChoices([
                        "1 - Все устройства",
                        "2 - Процессоры (CPU)",
                        "3 - Видеокарты (GPU)",
                        "4 - Сетевые адаптеры",
                        "5 - Звуковые устройства",
                        "6 - Дисковые накопители",
                        "0 - Вернуться в главное меню"
                    ]));

            switch (choice[0])
            {
                case '1': GetAllDrives(); break;
                case '2': ShowCpuDrives(); break;
                case '3': ShowGpuDrives(); break;
                case '4': ShowNetworkDrives(); break;
                case '5': ShowAudioDrives(); break;
                case '6': ShowDiskDrives(); break;
                case '0': return;
            }

            AnsiConsole.MarkupLine("\n[grey]Нажмите любую клавишу для продолжения...[/]");
            Console.ReadKey();
        }
    }

    private static void ShowDriverTable(string query, string title, string color)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.Title($"[{color}]{title}[/]"); 
        table.AddColumn("[bold]Устройство[/]");
        table.AddColumn("[bold]Версия драйвера[/]");
        table.AddColumn("[bold]Производитель[/]");

        try
        {
            AnsiConsole.Status()
                .Start("Сбор данных из WMI...", ctx =>
                {
#pragma warning disable CA1416 
                    ManagementObjectSearcher searcher = new(query);
#pragma warning restore CA1416
#pragma warning disable CA1416
                    var results = searcher.Get().Cast<ManagementObject>();
#pragma warning restore CA1416

                    foreach (var obj in results)
                    {

#pragma warning disable CA1416
                        string name = Markup.Escape((obj["Name"] ?? obj["FriendlyName"] ?? "Неизвестно").ToString());
#pragma warning restore CA1416
#pragma warning disable CA1416
                        string version = Markup.Escape((obj.Properties["DriverVersion"]?.Value ?? "Н/Д").ToString());
#pragma warning restore CA1416
#pragma warning disable CA1416
                        string manufacturer = Markup.Escape((obj.Properties["Manufacturer"]?.Value ?? "Н/Д").ToString());
#pragma warning restore CA1416

                        table.AddRow(name, version, manufacturer);
                    }
                });

            AnsiConsole.Write(table);
        }
        catch (Exception e)
        {
            
            AnsiConsole.MarkupLine($"[red]Ошибка:[/] {Markup.Escape(e.Message)}");
        }
    }

    public static void GetAllDrives() =>
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE Signer IS NOT NULL", "Все подписанные драйверы", "white");

    public static void ShowCpuDrives() =>
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'PROCESSOR'", "Драйверы Процессора", "blue");

    public static void ShowGpuDrives() =>
        ShowDriverTable("SELECT * FROM Win32_VideoController", "Видеоконтроллеры", "green");

    public static void ShowNetworkDrives() =>
        ShowDriverTable("SELECT * FROM Win32_NetworkAdapter WHERE PhysicalAdapter = True", "Сетевые адаптеры", "cyan");

    public static void ShowAudioDrives() =>
        ShowDriverTable("SELECT * FROM Win32_SoundDevice", "Звуковые устройства", "yellow");

    public static void ShowDiskDrives() =>
        ShowDriverTable("SELECT * FROM Win32_DiskDrive", "Дисковые накопители", "magenta");
}