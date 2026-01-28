using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Spectre.Console;

internal class StartUpManager
{
    public bool ShowStartupManagerUI()
    {
        try
        {
            while (true)
            {
                Console.Clear();

                AnsiConsole.Write(
                    new FigletText("Startup Manager")
                        .Centered()
                        .Color(Color.Cyan));

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Startup Management[/]")
                        .PageSize(10)
                        .AddChoices(new[] {
                        "📋 View All Startup Items",
                        "📁 View Startup Folder",
                        "🔧 View Registry Entries",
                        "📊 Startup Statistics",
                        "🔙 Back to Main Menu"
                        }));

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
                    case "🔙 Back to Main Menu":
                        return true;
                }

                AnsiConsole.MarkupLine("\n[grey]Press any key to continue...[/]");
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

    private void ShowAllStartupItemsTable()
    {
        var table = new Table()
            .Title("[bold green]All Startup Items[/]")
            .BorderColor(Color.Blue)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[cyan]Type[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Name[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Path/Value[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Location[/]").LeftAligned());

        
        try
        {
            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(userStartup))
            {
                foreach (var file in Directory.GetFiles(userStartup))
                {
                    table.AddRow(
                        "[green]File[/]",
                        $"[white]{Path.GetFileName(file)}[/]",
                        $"[grey]{TruncateString(file, 40)}[/]", 
                        "[yellow]User Startup[/]"
                    );
                }
            }

            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            if (Directory.Exists(commonStartup))
            {
                foreach (var file in Directory.GetFiles(commonStartup))
                {
                    table.AddRow(
                        "[green]File[/]",
                        $"[white]{Path.GetFileName(file)}[/]",
                        $"[grey]{TruncateString(file, 40)}[/]", 
                        "[yellow]Common Startup[/]"
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
                            "[blue]Registry[/]",
                            $"[white]{valueName}[/]",
                            $"[grey]{TruncateString(value, 40)}[/]", 
                            "[yellow]HKCU\\Run[/]"
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
                            "[blue]Registry[/]",
                            $"[white]{valueName}[/]",
                            $"[grey]{TruncateString(value, 40)}[/]", 
                            "[yellow]HKLM\\Run[/]"
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

    private void ShowStartupFolderTable()
    {
        
        var table = new Table()
            .Title("[bold green]Startup Folder Files[/]")
            .BorderColor(Color.Yellow)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[cyan]File Name[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Path[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Size[/]").RightAligned())
            .AddColumn(new TableColumn("[cyan]Type[/]").Centered());

        try
        {
            string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (Directory.Exists(userStartup))
            {
                foreach (var file in Directory.GetFiles(userStartup))
                {
                    FileInfo fi = new FileInfo(file);
                    table.AddRow(
                        $"[white]{Path.GetFileName(file)}[/]",
                        $"[grey]{TruncateString(file, 50)}[/]", 
                        $"[yellow]{fi.Length:N0} bytes[/]",
                        $"[cyan]{fi.Extension}[/]"
                    );
                }
            }

            string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
            if (Directory.Exists(commonStartup))
            {
                foreach (var file in Directory.GetFiles(commonStartup))
                {
                    FileInfo fi = new FileInfo(file);
                    table.AddRow(
                        $"[white]{Path.GetFileName(file)}[/]",
                        $"[grey]{TruncateString(file, 50)}[/]", 
                        $"[yellow]{fi.Length:N0} bytes[/]",
                        $"[cyan]{fi.Extension}[/]"
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

    private void ShowRegistryStartupTable()
    {
        
        var table = new Table()
            .Title("[bold blue]Registry Startup Entries[/]")
            .BorderColor(Color.Blue)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[cyan]Name[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Value[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Registry Path[/]").LeftAligned());

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
                            $"[white]{valueName}[/]",
                            $"[grey]{TruncateString(value, 60)}[/]", 
                            "[yellow]HKCU\\...\\Run[/]"
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
                            $"[white]{valueName}[/]",
                            $"[grey]{TruncateString(value, 60)}[/]", 
                            "[yellow]HKLM\\...\\Run[/]"
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

    private void ShowStartupStatistics()
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

            using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                registryCount += key2?.GetValueNames().Length ?? 0;
            }
        }
        catch { }

        var panel = new Panel($"[bold]Startup Items Statistics[/]\n\n" +
                              $"📁 Files in Startup Folders: [green]{folderCount}[/]\n" +
                              $"🔧 Registry Startup Entries: [yellow]{registryCount}[/]\n" +
                              $"📊 Total Startup Items: [cyan]{folderCount + registryCount}[/]")
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green),
            Padding = new Padding(2, 1, 2, 1)
        };

        AnsiConsole.Write(panel);

        if (folderCount + registryCount > 15)
        {
            AnsiConsole.MarkupLine("\n[red]⚠ Warning: Many startup items may slow down boot time.[/]");
        }
        else if (folderCount + registryCount < 5)
        {
            AnsiConsole.MarkupLine("\n[green]✓ Good: Minimal startup items.[/]");
        }
    }

    
    private string TruncateString(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 3) + "...";
    }
}