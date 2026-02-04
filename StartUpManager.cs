using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Spectre.Console;
using Task_Manager_T4;

internal class StartUpManager
{
    public static bool ShowStartupManagerUI()
    {
        try
        {
            while (true)
            {
                Console.Clear();
                AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]StartUp Management[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[{GraphicSettings.SecondaryColor}]Select category[/]")
                        .PageSize(12)
                        .AddChoices([
                        "📋 View All Startup Items",
                        "📁 View Startup Folder",
                        "🔧 View Registry Entries",
                        "📊 Startup Statistics",
                        "Delete the file from startup",
                        "🔙 Back to Main Menu"
                        ]));

                switch (choice)
                {
                    case "📋 View All Startup Items":
                        ShowAllStartupItemsTable();
                        break;
                    case "📁 View Startup Folder":
                        ShowStartupFolderTable();
                        break;
                    case "🔧 View Registry Entries":
                        ShowRegistryStartupTable();
                        break;
                    case "📊 Startup Statistics":
                        ShowStartupStatistics();
                        break;
                    case "Delete the file from startup":
                        RemoveFromStartup();
                        break;
                    case "🔙 Back to Main Menu":
                        Console.Clear();
                        return true;
                }

                AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Press any key to continue...[/]");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"ERROR in Startup Manager: {ex.Message}");
            Console.WriteLine("\nPress any key to return to main menu...");
            Console.ReadKey();
            return true;
        }
    }

    private static void RemoveFromStartup()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Remove From Startup[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        const string runPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        try
        {
            // 1. Открываем ветку на чтение и запись
            using RegistryKey key = Registry.CurrentUser.OpenSubKey(runPath, true);
            if (key == null)
            {
                AnsiConsole.MarkupLine("[red][!] Не удалось получить доступ к автозагрузке.[/]");
                return;
            }

            // 2. Получаем список всех программ в автозагрузке
            string[] valueNames = key.GetValueNames();

            if (valueNames.Length == 0)
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Список автозагрузки пуст.[/]");
                Console.ReadKey();
                return;
            }

            // 3. Выбор программы для удаления
            var appToRemove = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{GraphicSettings.SecondaryColor}]ВЫБЕРИТЕ ПРОГРАММУ ДЛЯ ИСКЛЮЧЕНИЯ ИЗ АВТОЗАПУСКА:[/]")
                    .PageSize(12)
                    // Не забываем Markup.Escape, чтобы не "крашнулось" от скобок в именах
                    .AddChoices(valueNames.Select(n => Markup.Escape(n)).Concat(["⬅ ОТМЕНА"])));

            if (appToRemove == "⬅ ОТМЕНА") return;

            // Находим оригинальное имя (до экранирования)
            string originalName = valueNames.First(n => Markup.Escape(n) == appToRemove);

            // 4. Подтверждение и удаление
            if (AnsiConsole.Confirm($"[bold red]ВНИМАНИЕ:[/] Убрать [{GraphicSettings.SecondaryColor}]{appToRemove}[/] из автозагрузки?"))
            {
                key.DeleteValue(originalName);
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor} Запись '{appToRemove}' успешно удалена из реестра.[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red][!] Ошибка при модификации реестра:[/] {ex.Message}");
        }

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.SecondaryColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
        Console.Clear();
    }


    private static void ShowAllStartupItemsTable()
    {
        var table = new Table()
            .Title($"[{GraphicSettings.SecondaryColor}]All Startup Items[/]")
            .BorderColor(Color.DarkOrange) //исправить
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Type[/]").Centered())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]").LeftAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Path/Value[/]").LeftAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Location[/]").LeftAligned());


        try
        {
            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(userStartup))
            {
                foreach (var file in Directory.GetFiles(userStartup))
                {
                    table.AddRow(
                        $"[{GraphicSettings.SecondaryColor}]File[/]",
                        $"[{GraphicSettings.SecondaryColor}]{Path.GetFileName(file)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{TruncateString(file, 40)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]User Startup[/]"
                    );
                }
            }

            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            if (Directory.Exists(commonStartup))
            {
                foreach (var file in Directory.GetFiles(commonStartup))
                {
                    table.AddRow(
                        $"[{GraphicSettings.SecondaryColor}]File[/]",
                        $"[{GraphicSettings.SecondaryColor}]{Path.GetFileName(file)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{TruncateString(file, 40)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]Common Startup[/]"
                    );
                }
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            AnsiConsole.MarkupLine($"[red]Access denied: {ex.Message}[/]");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            table.AddRow(
                "[red]Error[/]",
                "Access Denied",
                ex.Message,
                "N/A"
            );
        }


        try
        {


            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (key != null)
                {

                    foreach (string valueName in key.GetValueNames())
                    {

                        string value = key.GetValue(valueName)?.ToString() ?? "";
                        table.AddRow(
                            $"[{GraphicSettings.SecondaryColor}]Registry[/]",
                            $"[{GraphicSettings.SecondaryColor}]{valueName}[/]",
                            $"[{GraphicSettings.SecondaryColor}]{TruncateString(value, 40)}[/]",
                            $"[{GraphicSettings.SecondaryColor}]HKCU\\Run[/]"
                        );
                    }
                }
            }



            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (key != null)
                {

                    foreach (string valueName in key.GetValueNames())
                    {

                        string value = key.GetValue(valueName)?.ToString() ?? "";
                        table.AddRow(
                            $"[{GraphicSettings.SecondaryColor}]Registry[/]",
                            $"[{GraphicSettings.SecondaryColor}]{valueName}[/]",
                            $"[{GraphicSettings.SecondaryColor}]{TruncateString(value, 40)}[/]",
                            $"[{GraphicSettings.SecondaryColor}]HKLM\\Run[/]"
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            table.AddRow(
                "[red]Error[/]",
                "Registry Access",
                ex.Message,
                "N/A"
            );
        }

        AnsiConsole.Write(table);
    }

    private static void ShowStartupFolderTable()
    {

        var table = new Table()
            .Title($"[{GraphicSettings.SecondaryColor}]Startup Folder Files[/]")
            .BorderColor(Color.DarkOrange)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]File Name[/]").LeftAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Path[/]").LeftAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Size[/]").RightAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Type[/]").Centered());

        try
        {
            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(userStartup))
            {
                foreach (var file in Directory.GetFiles(userStartup))
                {
                    FileInfo fi = new(file);
                    table.AddRow(
                        $"[{GraphicSettings.SecondaryColor}]{Path.GetFileName(file)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{TruncateString(file, 50)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{fi.Length:N0} bytes[/]",
                        $"[{GraphicSettings.SecondaryColor}]{fi.Extension}[/]"
                    );
                }
            }

            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            if (Directory.Exists(commonStartup))
            {
                foreach (var file in Directory.GetFiles(commonStartup))
                {
                    FileInfo fi = new(file);
                    table.AddRow(
                        $"[{GraphicSettings.SecondaryColor}]{Path.GetFileName(file)}[/]",
                       $"[{GraphicSettings.SecondaryColor}]{TruncateString(file, 50)}[/]",
                       $"[{GraphicSettings.SecondaryColor}]{fi.Length:N0} bytes[/]",
                        $"[{GraphicSettings.SecondaryColor}]{fi.Extension}[/]"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            table.AddRow(
                "[red]ERROR[/]",
                $"[red]{ex.Message}[/]",
                "N/A",
                "N/A"
            );
        }

        AnsiConsole.Write(table);
    }

    private static void ShowRegistryStartupTable()
    {

        var table = new Table()
            .Title($"[ {GraphicSettings.SecondaryColor}]Registry Startup Entries[/]")
            .BorderColor(Color.DarkOrange)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn($"[ {GraphicSettings.SecondaryColor}]Name[/]").LeftAligned())
            .AddColumn(new TableColumn($"[ {GraphicSettings.SecondaryColor}]Value[/]").LeftAligned())
            .AddColumn(new TableColumn($"[ {GraphicSettings.SecondaryColor}]Registry Path[/]").LeftAligned());

        try
        {

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (key != null)
                {

                    foreach (string valueName in key.GetValueNames())
                    {

                        string value = key.GetValue(valueName)?.ToString() ?? "";
                        table.AddRow(
                            $"[ {GraphicSettings.SecondaryColor}]{valueName}[/]",
                            $"[ {GraphicSettings.SecondaryColor}]{TruncateString(value, 60)}[/]",
                            $"[ {GraphicSettings.SecondaryColor}]HKCU\\...\\Run[/]"
                        );
                    }
                }
            }


            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (key != null)
                {

                    foreach (string valueName in key.GetValueNames())
                    {

                        string value = key.GetValue(valueName)?.ToString() ?? "";
                        table.AddRow(
                            $"[ {GraphicSettings.SecondaryColor}]{valueName}[/]",
                            $"[ {GraphicSettings.SecondaryColor}]{TruncateString(value, 60)}[/]",
                            $"[ {GraphicSettings.SecondaryColor}]HKLM\\...\\Run[/]"
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            table.AddRow(
                "[red]ERROR[/]",
                $"[red]{ex.Message}[/]",
                "[red]Access Denied[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    private static void ShowStartupStatistics()
    {
        int folderCount = 0;
        int registryCount = 0;

        try
        {
            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

            if (Directory.Exists(userStartup))
                folderCount += Directory.GetFiles(userStartup).Length;

            if (Directory.Exists(commonStartup))
                folderCount += Directory.GetFiles(commonStartup).Length;


            using (RegistryKey key1 = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run"))
            {

                registryCount += key1?.GetValueNames().Length ?? 0;
            }


            using RegistryKey key2 = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run");

            registryCount += key2?.GetValueNames().Length ?? 0;
        }
        catch { }

        var panel = new Panel($"[{GraphicSettings.SecondaryColor}]Startup Items Statistics[/]\n\n" +
                              $"📁 Files in Startup Folders: [{GraphicSettings.SecondaryColor}]{folderCount}[/]\n" +
                              $"🔧 Registry Startup Entries: [{GraphicSettings.SecondaryColor}]{registryCount}[/]\n" +
                              $"📊 Total Startup Items: [{GraphicSettings.SecondaryColor}]{folderCount + registryCount}[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.DarkOrange),
            Padding = new Padding(2, 1, 2, 1)
        };

        AnsiConsole.Write(panel);

        if (folderCount + registryCount > 15)
        {
            AnsiConsole.MarkupLine("\n[red]⚠ Warning: Many startup items may slow down boot time.[/]");
        }
        else if (folderCount + registryCount < 5)
        {
            AnsiConsole.MarkupLine($"\n[{GraphicSettings.SecondaryColor}]✓ Good: Minimal startup items.[/]");
        }
    }


    private static string TruncateString(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return string.Concat(text.AsSpan(0, maxLength - 3), "...");
    }
}