using System;
using System.Management;
using System.Linq;
using Spectre.Console;

namespace Task_Manager_T4;

class DriveManager
{
    public static void Main_Menu_Drives()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[yellow]Менеджер Драйверов[/]").RuleStyle("grey").LeftJustified());

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Выберите категорию драйверов:")
                    .PageSize(12)
                    .AddChoices([
                        "1 - Все устройства (подписанные)",
                        "2 - Процессоры (CPU)",
                        "3 - Видеокарты (GPU)",
                        "4 - Сетевые адаптеры",
                        "5 - Звуковые устройства",
                        "6 - Дисковые накопители",
                        "7 - Мониторы",
                        "8 - Периферия (Мышь/Клавиатура)",
                        "0 - Вернуться в главное меню"
                    ]));

            switch (choice.Split(' ')[0]) // Берем цифру в начале строки
            {
                case "1": GetAllDrives(); break;
                case "2": ShowCpuDrives(); break;
                case "3": ShowGpuDrives(); break;
                case "4": ShowNetworkDrives(); break;
                case "5": ShowAudioDrives(); break;
                case "6": ShowDiskDrives(); break;
                case "7": ShowMonitorDrives(); break;
                case "8": ShowInputDrives(); break;
                case "0": return;
            }

            AnsiConsole.MarkupLine("\n[grey]Нажмите любую клавишу для продолжения...[/]");
            Console.ReadKey();
        }
    }

    private static void ShowDriverTable(string query, string title, string color)
    {
        var table = new Table().Border(TableBorder.Rounded);
        table.Title($"[{color}]{title}[/]");
        table.AddColumn("[bold]Устройство[/]");
        table.AddColumn("[bold]Версия[/]");
        table.AddColumn("[bold]Производитель[/]");

        try
        {
            AnsiConsole.Status().Start("Опрашиваю систему...", ctx => 
            {
                using var searcher = new ManagementObjectSearcher(query);
                var results = searcher.Get().Cast<ManagementObject>().ToList();

                if (!results.Any())
                {
                    table.AddRow("[grey]Ничего не найдено[/]", "-", "-");
                    return;
                }

                foreach (var obj in results)
                {
                    // Пробуем FriendlyName (понятное имя), если нет - DeviceName или Caption
                    string name = Markup.Escape(
                        obj["FriendlyName"]?.ToString() ?? 
                        obj["DeviceName"]?.ToString() ?? 
                        obj["Caption"]?.ToString() ?? "Неизвестно"
                    );

                    string version = Markup.Escape(obj["DriverVersion"]?.ToString() ?? "Н/Д");
                    string manufacturer = Markup.Escape(obj["Manufacturer"]?.ToString() ?? "Н/Д");

                    table.AddRow(name, version, manufacturer);
                }
            });

            AnsiConsole.Write(table);
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Ошибка WMI:[/] {Markup.Escape(e.Message)}");
        }
    }

    // Методы теперь используют фильтр DeviceClass, что гораздо надежнее
    public static void GetAllDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE Signer IS NOT NULL", "Все подписанные драйверы", "white");

    public static void ShowCpuDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'PROCESSOR'", "Процессоры", "blue");

    public static void ShowGpuDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'DISPLAY'", "Видеокарты (GPU)", "green");

    public static void ShowNetworkDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'NET'", "Сетевые адаптеры", "cyan");

    public static void ShowAudioDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'MEDIA'", "Звук", "yellow");

    public static void ShowDiskDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'DISKDRIVE'", "Диски", "magenta");

    public static void ShowMonitorDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'MONITOR'", "Мониторы", "teal");

    public static void ShowInputDrives() => 
        ShowDriverTable("SELECT * FROM Win32_PnPSignedDriver WHERE DeviceClass = 'KEYBOARD' OR DeviceClass = 'MOUSE'", "Устройства ввода", "orange1");
}