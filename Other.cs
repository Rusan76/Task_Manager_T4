using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using Spectre.Console;
namespace ProjectT4;

public class Other
{
    public static void PrintAllOtherFunctions()
    {
        while (true)
        {
            Console.Clear();

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]🔧 Additional Tools[/]")
                    .PageSize(10)
                    .AddChoices(
                    [
                        "⌨️  Fix Keyboard Issues",
                        "👥 User Management",
                        "🛡️  Run as Administrator",
                        "🔙 Back to Main Menu"
                    ]));

            switch (choice)
            {
                case "⌨️  Fix Keyboard Issues":
                    Keyboard.FixKeyboard();
                    break;
                case "👥 User Management":
                    Users.SettingsUser();
                    break;
                case "🛡️  Run as Administrator":
                    RunAsAdmin();
                    break;
                case "🔙 Back to Main Menu":
                    return;
            }
        }
    }
    public class Keyboard
    {
        public static void FixKeyboard()
        {
            Console.Clear();

            if (!IsRunningAsAdmin())
            {
                ShowAdminWarning();
                return;
            }

            AnsiConsole.Write(
                new Panel("[bold red]⚠  WARNING: Keyboard Fix Utility[/]")
                    .BorderColor(Color.Red)
                    .Padding(1, 1));

            AnsiConsole.MarkupLine("[yellow]This tool attempts to fix common keyboard issues.[/]");
            AnsiConsole.MarkupLine("[grey]Some operations require administrator privileges.[/]");

            if (!AnsiConsole.Confirm("\n[red]Continue with keyboard fix?[/]", false))
                return;

            try
            {
                var progress = AnsiConsole.Progress()
                    .Columns(
                    [
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn(),
                        new PercentageColumn(),
                        new SpinnerColumn()
                    ]);

                progress.Start(ctx =>
                {
                    var task1 = ctx.AddTask("[green]Checking keyboard status...[/]");
                    task1.Increment(25);

                    if (IsKeyboardWorking())
                    {
                        AnsiConsole.MarkupLine("\n[green]✅ Keyboard appears to be working normally.[/]");
                        return;
                    }

                    task1.Increment(25);

                    var task2 = ctx.AddTask("[blue]Attempting driver reset...[/]");
                    ResetKeyboardDriver();
                    task2.Increment(100);

                    var task3 = ctx.AddTask("[yellow]Checking registry settings...[/]");
                    EnableKeyboardInRegistry();
                    task3.Increment(100);

                    var task4 = ctx.AddTask("[cyan]Restarting keyboard service...[/]");
                    RestartKeyboardService();
                    task4.Increment(100);

                    var task5 = ctx.AddTask("[magenta]Enabling in Device Manager...[/]");
                    EnableInDeviceManager();
                    task5.Increment(100);
                });

                ShowResultPanel();
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        private static bool IsKeyboardWorking()
        {
            AnsiConsole.Markup("[yellow]Press any key within 5 seconds to test...[/]");

            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalSeconds < 5)
            {
                if (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    AnsiConsole.MarkupLine("\n[green]✅ Key detected![/]");
                    return true;
                }
                Thread.Sleep(100);
            }

            AnsiConsole.MarkupLine("\n[red]⚠ No keypress detected.[/]");
            return false;
        }

        private static void ResetKeyboardDriver()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-PnpDevice -Class Keyboard | Reset-PnpDevice -Confirm:$false\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };

                Process.Start(psi)?.WaitForExit(10000);
            }
            catch { }
        }

        private static void EnableKeyboardInRegistry()
        {
            string[] regCommands =
            [
                @"add HKLM\SYSTEM\CurrentControlSet\Services\i8042prt /v Start /t REG_DWORD /d 1 /f",
                @"add HKLM\SYSTEM\CurrentControlSet\Services\kbdclass /v Start /t REG_DWORD /d 1 /f",
                @"add HKLM\SYSTEM\CurrentControlSet\Services\kbdhid /v Start /t REG_DWORD /d 1 /f"
            ];

            foreach (string cmd in regCommands)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "reg.exe",
                        Arguments = cmd,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"
                    })?.WaitForExit(3000);
                }
                catch { }
            }
        }

        private static void RestartKeyboardService()
        {
            string[] services = ["i8042prt", "kbdclass", "kbdhid"];

            foreach (string serviceName in services)
            {
                try
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    using var sc = new ServiceController(serviceName);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
#pragma warning disable CA1416 // Проверка совместимости платформы
                        sc.Stop();
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(3));
#pragma warning restore CA1416 // Проверка совместимости платформы
                    }
#pragma warning disable CA1416 // Проверка совместимости платформы
                    sc.Start();
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(3));
#pragma warning restore CA1416 // Проверка совместимости платформы
                }
                catch { }
            }
        }

        private static void EnableInDeviceManager()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "mmc.exe",
                    Arguments = "devmgmt.msc",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        private static void ShowResultPanel()
        {
            var panel = new Panel(
                "[bold green]🎯 KEYBOARD FIX COMPLETE[/]\n\n" +
                "[yellow]Recommended actions:[/]\n" +
                "1. [white]Test your keyboard now[/]\n" +
                "2. [white]Restart computer if issues persist[/]\n" +
                "3. [white]Check physical connections[/]\n" +
                "4. [white]Try different USB port[/]\n" +
                "5. [white]Update keyboard drivers[/]\n\n" +
                "[grey]Note: Some fixes require system restart.[/]")
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Green),
                Padding = new Padding(2, 1, 2, 1)
            };

            AnsiConsole.Write(panel);
        }
    }

    public class Users
    {
        public static void SettingsUser()
        {
            while (true)
            {
                Console.Clear();

                if (!IsRunningAsAdmin())
                {
                    ShowAdminWarning();
                    return;
                }

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold cyan]👥 User Management[/]")
                        .PageSize(10)
                        .AddChoices(
                        [
                            "📋 List All Users",
                            "➕ Create New User",
                            "🗑️  Delete User",
                            "🔒 Change User Password",
                            "👑 Add User to Administrators",
                            "📊 User Information",
                            "🔙 Back"
                        ]));

                switch (choice)
                {
                    case "📋 List All Users":
                        PrintAllUsers();
                        break;
                    case "➕ Create New User":
                        CreateNewUser();
                        break;
                    case "🗑️  Delete User":
                        DeleteUser();
                        break;
                    case "🔒 Change User Password":
                        ChangeUserPassword();
                        break;
                    case "👑 Add User to Administrators":
                        AddUserToAdminGroup();
                        break;
                    case "📊 User Information":
                        ShowUserInfo();
                        break;
                    case "🔙 Back":
                        return;
                }

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
        }

        public static void PrintAllUsers()
        {
            try
            {
                AnsiConsole.Write(new Rule("[bold cyan]Local Users[/]").RuleStyle("cyan"));

#pragma warning disable CA1416 // Проверка совместимости платформы
                using var ctx = new PrincipalContext(ContextType.Machine);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var qbeUser = new UserPrincipal(ctx);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var srch = new PrincipalSearcher(qbeUser);
#pragma warning restore CA1416 // Проверка совместимости платформы

                var table = new Table()
                    .Title($"[bold]Local User Accounts[/]")
                    .Border(TableBorder.Rounded)
                    .AddColumn(new TableColumn("[cyan]Username[/]").LeftAligned())
                    .AddColumn(new TableColumn("[cyan]Full Name[/]").LeftAligned())
                    .AddColumn(new TableColumn("[cyan]Description[/]").LeftAligned())
                    .AddColumn(new TableColumn("[cyan]Enabled[/]").Centered());

#pragma warning disable CA1416 // Проверка совместимости платформы
                foreach (UserPrincipal found in srch.FindAll())
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    table.AddRow(
                        $"[white]{found.SamAccountName}[/]",
                        $"[grey]{found.DisplayName ?? "N/A"}[/]",
                        $"[yellow]{found.Description ?? "N/A"}[/]",
                        found.Enabled == true ? "[green]Yes[/]" : "[red]No[/]");
#pragma warning restore CA1416 // Проверка совместимости платформы
                }

                AnsiConsole.Write(table);

                // Также показываем группы
                ShowLocalGroups();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }
        }

        public static void CreateNewUser()
        {
            Console.Clear();

            var username = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter username:[/]")
                    .PromptStyle("yellow")
                    .Validate(name =>
                        !string.IsNullOrWhiteSpace(name) && name.Length >= 3
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Username must be at least 3 characters[/]")));

            var password = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter password:[/]")
                    .PromptStyle("red")
                    .Secret()
                    .Validate(pass =>
                        pass.Length >= 6
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Password must be at least 6 characters[/]")));

            var fullName = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter full name (optional):[/]")
                    .PromptStyle("yellow")
                    .AllowEmpty());

            var description = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Enter description (optional):[/]")
                    .PromptStyle("grey")
                    .AllowEmpty());

            try
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var ctx = new PrincipalContext(ContextType.Machine);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var user = new UserPrincipal(ctx)
                {
                    SamAccountName = username,
                    UserPrincipalName = $"{username}@{Environment.MachineName}",
                    DisplayName = fullName,
                    Description = description,
                    PasswordNeverExpires = true,
                    Enabled = true
                };
#pragma warning restore CA1416 // Проверка совместимости платформы

#pragma warning disable CA1416 // Проверка совместимости платформы
                user.SetPassword(password);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                user.Save();
#pragma warning restore CA1416 // Проверка совместимости платформы

                AnsiConsole.MarkupLine($"[green]✅ User '{username}' created successfully![/]");

                if (AnsiConsole.Confirm("[yellow]Add user to 'Users' group?[/]", true))
                {
                    AddUserToGroup(username, "Users");
                }

                if (AnsiConsole.Confirm("[yellow]Create user folder in Documents?[/]", false))
                {
                    CreateUserFolder(username);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Error creating user: {ex.Message}[/]");
            }
        }

        public static void DeleteUser()
        {
            try
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var ctx = new PrincipalContext(ContextType.Machine);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var qbeUser = new UserPrincipal(ctx);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var srch = new PrincipalSearcher(qbeUser);
#pragma warning restore CA1416 // Проверка совместимости платформы

#pragma warning disable CA1416 // Проверка совместимости платформы
                var users = srch.FindAll()
                    .Cast<UserPrincipal>()
                    .Select(u => u.SamAccountName)
                    .Where(u => !u.Equals("Administrator", StringComparison.OrdinalIgnoreCase) &&
                               !u.Equals("Guest", StringComparison.OrdinalIgnoreCase))
                    .ToList();
#pragma warning restore CA1416 // Проверка совместимости платформы

                if (users.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No users found to delete.[/]");
                    return;
                }

                var userToDelete = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[red]Select user to delete:[/]")
                        .PageSize(10)
                        .AddChoices(users));

                if (!AnsiConsole.Confirm($"[bold red]⚠  Are you sure you want to delete user '{userToDelete}'?[/]", false))
                    return;

#pragma warning disable CA1416 // Проверка совместимости платформы
                using var user = UserPrincipal.FindByIdentity(ctx, userToDelete);
#pragma warning restore CA1416 // Проверка совместимости платформы
                if (user != null)
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    user.Delete();
#pragma warning restore CA1416 // Проверка совместимости платформы
                    AnsiConsole.MarkupLine($"[green]✅ User '{userToDelete}' deleted successfully![/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Error deleting user: {ex.Message}[/]");
            }
        }

        public static void ChangeUserPassword()
        {
            try
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var ctx = new PrincipalContext(ContextType.Machine);
#pragma warning restore CA1416 // Проверка совместимости платформы
                var users = GetUserList(ctx);

                var username = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select user to change password:")
                        .AddChoices(users));

                var newPassword = AnsiConsole.Prompt(
                    new TextPrompt<string>("[green]Enter new password:[/]")
                        .PromptStyle("red")
                        .Secret()
                        .Validate(pass =>
                            pass.Length >= 6
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red]Password must be at least 6 characters[/]")));

#pragma warning disable CA1416 // Проверка совместимости платформы
                using var user = UserPrincipal.FindByIdentity(ctx, username);
#pragma warning restore CA1416 // Проверка совместимости платформы
                if (user != null)
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    user.SetPassword(newPassword);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                    user.Save();
#pragma warning restore CA1416 // Проверка совместимости платформы
                    AnsiConsole.MarkupLine($"[green]✅ Password changed for user '{username}'![/]");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
            }
        }

        public static void AddUserToAdminGroup()
        {
            try
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var ctx = new PrincipalContext(ContextType.Machine);
#pragma warning restore CA1416 // Проверка совместимости платформы
                var users = GetUserList(ctx);

                var username = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select user to add to Administrators:")
                        .AddChoices(users));

#pragma warning disable CA1416 // Проверка совместимости платформы
                using var user = UserPrincipal.FindByIdentity(ctx, username);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var group = GroupPrincipal.FindByIdentity(ctx, "Administrators");
#pragma warning restore CA1416 // Проверка совместимости платформы

                if (user != null && group != null)
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    if (!group.Members.Contains(user))
                    {
#pragma warning disable CA1416 // Проверка совместимости платформы
                        group.Members.Add(user);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                        group.Save();
#pragma warning restore CA1416 // Проверка совместимости платформы
                        AnsiConsole.MarkupLine($"[green]✅ User '{username}' added to Administrators group![/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[yellow]⚠ User '{username}' is already in Administrators group.[/]");
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
            }
        }

        public static void ShowUserInfo()
        {
            try
            {
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var ctx = new PrincipalContext(ContextType.Machine);
#pragma warning restore CA1416 // Проверка совместимости платформы
                var users = GetUserList(ctx);

                var username = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select user for details:")
                        .AddChoices(users));

#pragma warning disable CA1416 // Проверка совместимости платформы
                using var user = UserPrincipal.FindByIdentity(ctx, username);
#pragma warning restore CA1416 // Проверка совместимости платформы
                if (user != null)
                {
#pragma warning disable CA1416 // Проверка совместимости платформы
                    var panel = new Panel(
                        $"[bold]👤 User Information: {user.SamAccountName}[/]\n\n" +
                        $"[green]Full Name:[/] [white]{user.DisplayName ?? "N/A"}[/]\n" +
                        $"[green]Description:[/] [white]{user.Description ?? "N/A"}[/]\n" +
                        $"[green]Email:[/] [white]{user.EmailAddress ?? "N/A"}[/]\n" +
                        $"[green]Enabled:[/] {(user.Enabled == true ? "[green]Yes[/]" : "[red]No[/]")}\n" +
                        $"[green]Password Never Expires:[/] {(user.PasswordNeverExpires ? "[green]Yes[/]" : "[red]No[/]")}\n" +
                        $"[green]Last Logon:[/] [white]{user.LastLogon?.ToString("yyyy-MM-dd HH:mm") ?? "Never"}[/]\n" +
                        $"[green]Account Created:[/] [white]{user.Context.ConnectedServer ?? "N/A"}[/]")
                    {
                        Border = BoxBorder.Rounded,
                        BorderStyle = new Style(Color.Cyan),
                        Padding = new Padding(1, 1, 1, 1)
                    };
#pragma warning restore CA1416 // Проверка совместимости платформы

                    AnsiConsole.Write(panel);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]❌ Error: {ex.Message}[/]");
            }
        }

        private static List<string> GetUserList(PrincipalContext ctx)
        {
#pragma warning disable CA1416 // Проверка совместимости платформы
            using var qbeUser = new UserPrincipal(ctx);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
            using var srch = new PrincipalSearcher(qbeUser);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
            return [.. srch.FindAll()
                .Cast<UserPrincipal>()
                .Select(u => u.SamAccountName)
                .Where(u => !u.Equals("Administrator", StringComparison.OrdinalIgnoreCase) &&
                           !u.Equals("Guest", StringComparison.OrdinalIgnoreCase))];
#pragma warning restore CA1416 // Проверка совместимости платформы

        }

        private static void ShowLocalGroups()
        {
            try
            {
                AnsiConsole.Write(new Rule("[bold yellow]Local Groups[/]").RuleStyle("yellow"));

#pragma warning disable CA1416 // Проверка совместимости платформы
                using var ctx = new PrincipalContext(ContextType.Machine);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var qbeGroup = new GroupPrincipal(ctx);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
                using var srch = new PrincipalSearcher(qbeGroup);
#pragma warning restore CA1416 // Проверка совместимости платформы

#pragma warning disable CA1416 // Проверка совместимости платформы
                var groups = srch.FindAll()
                    .Cast<GroupPrincipal>()
                    .Select(g => g.Name)
                    .OrderBy(g => g)
                    .ToList();
#pragma warning restore CA1416 // Проверка совместимости платформы

                var grid = new Grid()
                    .AddColumn(new GridColumn().PadRight(2))
                    .AddColumn(new GridColumn().PadRight(2))
                    .AddColumn(new GridColumn());

                for (int i = 0; i < groups.Count; i += 3)
                {
                    var row = new List<string>();
                    for (int j = 0; j < 3 && i + j < groups.Count; j++)
                    {
                        row.Add($"[cyan]•[/] {groups[i + j]}");
                    }
                    grid.AddRow(row.ToArray());
                }

                AnsiConsole.Write(grid);
            }
            catch { }
        }

        private static void AddUserToGroup(string username, string groupName)
        {
            try
            {
                var command = $"/c net localgroup \"{groupName}\" \"{username}\" /add";
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                })?.WaitForExit();
            }
            catch { }
        }

        private static void CreateUserFolder(string username)
        {
            try
            {
                string userFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    username);

                Directory.CreateDirectory(userFolder);
                AnsiConsole.MarkupLine($"[green]✅ Created folder: {userFolder}[/]");
            }
            catch { }
        }
    }

    private static bool IsRunningAsAdmin()
    {
#pragma warning disable CA1416 // Проверка совместимости платформы
        using var identity = WindowsIdentity.GetCurrent();
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
        var principal = new WindowsPrincipal(identity);
#pragma warning restore CA1416 // Проверка совместимости платформы
#pragma warning disable CA1416 // Проверка совместимости платформы
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 // Проверка совместимости платформы
    }

    private static void ShowAdminWarning()
    {
        var panel = new Panel(
            "[bold red]⚠  ADMINISTRATOR RIGHTS REQUIRED[/]\n\n" +
            "[yellow]This feature requires administrator privileges.[/]\n" +
            "[white]Please run the program as Administrator.[/]\n\n" +
            "[grey]To run as Administrator:[/]\n" +
            "1. Right-click the program\n" +
            "2. Select 'Run as administrator'\n" +
            "3. Confirm UAC prompt")
        {
            Border = BoxBorder.Double,
            BorderStyle = new Style(Color.Red),
            Padding = new Padding(2, 1, 2, 1)
        };

        AnsiConsole.Write(panel);
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static void RunAsAdmin()
    {
        try
        {
            var exePath = Environment.ProcessPath;
            Process.Start(new ProcessStartInfo
            {
                FileName = exePath,
                Verb = "runas",
                UseShellExecute = true
            });

            Environment.Exit(0);
        }
        catch
        {
            AnsiConsole.MarkupLine("[red]Failed to restart as administrator.[/]");
        }
    }
}
