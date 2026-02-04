using System;
using Microsoft.Win32;
using Spectre.Console;
using System.Collections.Generic;
using System.Linq;
using Task_Manager_T4;
class ProgramManager
{

    public static void MainMenuProgramManager()
    {
        while (true)
        {
            Console.Clear();
            AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Менеджер программ[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
            var choiceProgramManager = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{GraphicSettings.SecondaryColor}]Select an option:[/]")
                    .PageSize(12)
                    .AddChoices(
                    [
                       "Показать все программы",
                       "Удалить программу",
                       "Назад"
                    ]));
            switch (choiceProgramManager)
            {
                case "Показать все программы":
                    ShowAllProgram();
                    Console.Clear();
                    break;
                case "Удалить программу":
                    UninstallProgram();
                    Console.Clear();
                    break;
                case "Назад":
                    return;
            }
        }
    }

    private static void UninstallProgram()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]UNINSTALLER[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());

        var apps = new List<(string Name, string UninstallCommand)>();
        string[] keys = [
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        ];

        foreach (string keyPath in keys)
        {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) continue;

            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using RegistryKey subkey = key.OpenSubKey(subkeyName);
                string name = subkey?.GetValue("DisplayName") as string;
                string command = subkey?.GetValue("UninstallString") as string;

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(command))
                {
                    apps.Add((name, command));
                }
            }
        }

        var appToUninstall = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"[{GraphicSettings.SecondaryColor}]ВЫБЕРИТЕ ПРОГРАММУ ДЛЯ УДАЛЕНИЯ (ESC для отмены):[/]")
            .PageSize(12)
            .AddChoices(apps.Select(a => Markup.Escape(a.Name)).OrderBy(n => n).Concat(["⬅ НАЗАД"])));

        if (appToUninstall == "⬅ НАЗАД") return;
        var (Name, UninstallCommand) = apps.First(a => Markup.Escape(a.Name) == appToUninstall);

        if (AnsiConsole.Confirm($"[bold red]ВНИМАНИЕ:[/] Запустить деинсталлятор для [{GraphicSettings.SecondaryColor}]{Markup.Escape(Name)}[/]?"))
        {
            try
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Запуск процесса деинсталляции...[/]");

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {UninstallCommand}",
                    CreateNoWindow = false,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red][ERROR] Не удалось запустить деинсталлятор:[/] {ex.Message}");
            }
        }
    }

    private static void ShowAllProgram()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]INSTALLED_SOFTWARE_LIST[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());

        var table = new Table().BorderColor(GraphicSettings.GetThemeColor).Border(TableBorder.Rounded); 
        table.AddColumn($"[{GraphicSettings.SecondaryColor}]ПРОГРАММА[/]");
        table.AddColumn($"[{GraphicSettings.SecondaryColor}]ИЗДАТЕЛЬ[/]");
        table.AddColumn($"[{GraphicSettings.SecondaryColor}]ДАТА[/]");

        string[] keys = [
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        ];

        foreach (string keyPath in keys)
        {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key == null) continue;

            foreach (string subkeyName in key.GetSubKeyNames())
            {
                using RegistryKey subkey = key.OpenSubKey(subkeyName);
                if (subkey == null) continue;

                string displayName = subkey.GetValue("DisplayName") as string;
                if (!string.IsNullOrEmpty(displayName))
                {

                    string publisher = subkey.GetValue("Publisher") as string ?? "Unknown";


                    string installDateRaw = subkey.GetValue("InstallDate") as string;
                    string formattedDate = "N/A";

                    if (!string.IsNullOrEmpty(installDateRaw) && installDateRaw.Length == 8)
                    {
                        try
                        {
                            formattedDate = $"{installDateRaw.Substring(6, 2)}.{installDateRaw.Substring(4, 2)}.{installDateRaw.Substring(0, 4)}";
                        }
                        catch { }
                    }

                    table.AddRow(
                        Markup.Escape(displayName),
                        $"[{GraphicSettings.SecondaryColor}]{Markup.Escape(publisher)}[/]",
                        $"[{GraphicSettings.SecondaryColor}]{formattedDate}[/]"
                    );
                }
            }
        }
        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[{GraphicSettings.SecondaryColor}]Нажмите любую клавишу для выхода...[/]");
        Console.ReadKey();
    }
}