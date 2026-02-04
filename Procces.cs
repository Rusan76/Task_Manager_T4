using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using Spectre.Console;
using Task_Manager_T4;

class Process_management
{
    public static void GetProcces()
    {
        while (true)
        {
            Console.Clear();
            
            AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Process Management[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{GraphicSettings.SecondaryColor}]Select category[/]")
                    .PageSize(GraphicSettings.PageSize)
                    .AddChoices([
                        "📋 Show All Processes",
                        "🔍 Find Process by Name",
                        "⚡ Show System Processes",
                        "❌ Kill Process by ID",
                        "🗑️ Kill Process by Name",
                        "🚀 Start New Process",
                        "ℹ️ Show Process Details",
                        "💾 Export Processes to File",
                        "🧹 Clean Dead Processes",
                        "🔙 Back to Main Menu"
                    ]));
            
            switch (choice)
            {
                case "📋 Show All Processes":
                    ShowAllProcessesSpectre();
                    break;
                case "🔍 Find Process by Name":
                    FindProcessByNameSpectre();
                    break;
                case "⚡ Show System Processes":
                    ShowSystemProcessesSpectre();
                    break;
                case "❌ Kill Process by ID":
                    KillProcessByIdSpectre();
                    break;
                case "🗑️ Kill Process by Name":
                    KillProcessByNameSpectre();
                    break;
                case "🚀 Start New Process":
                    StartNewProcessSpectre();
                    break;
                case "ℹ️ Show Process Details":
                    ShowProcessInfoSpectre();
                    break;
                case "💾 Export Processes to File":
                    ExportProcessesToFileSpectre();
                    break;
                case "🧹 Clean Dead Processes":
                    CleanDeadProcessesSpectre();
                    break;
                case "🔙 Back to Main Menu":
                    Console.Clear();
                    return;
            }
            
            AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Нажмите любую клавишу для продолжения...[/]");
            Console.ReadKey();
        }
    }
    
    
    public static void ShowAllProcessesSpectre()
    {
        Console.Clear();
        
        AnsiConsole.Status()
            .Start("Loading processes...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse(GraphicSettings.AccentColor));
                Thread.Sleep(500);
            });
        
        Process[] runningProcesses = Process.GetProcesses();
        
        var table = new Table()
            .Title($"[{GraphicSettings.SecondaryColor}]Running Processes: {runningProcesses.Length}[/]")
            .BorderColor(GraphicSettings.GetThemeColor)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]ID[/]").Centered())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]").LeftAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Memory (MB)[/]").RightAligned())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Priority[/]").Centered())
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Status[/]").Centered());
        
        foreach (Process proc in runningProcesses.OrderBy(p => p.ProcessName))
        {
            try
            {
                string status = proc.Responding ? $"[{GraphicSettings.SecondaryColor}]YES[/]" : "[red] NO[/]";
                string memory = $"{proc.WorkingSet64 / 1024 / 1024:N0}";
                string priority = proc.BasePriority.ToString();
                
                table.AddRow(
                    $"[{GraphicSettings.SecondaryColor}]{proc.Id}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{proc.ProcessName}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{memory}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{priority}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{status}[/]");
            }
            catch 
            {
                table.AddRow(
                    $"[{GraphicSettings.SecondaryColor}]{proc.Id}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{proc.ProcessName}[/]",
                    $"[{GraphicSettings.SecondaryColor}]N/A[/]",
                    $"[{GraphicSettings.SecondaryColor}]N/A[/]",
                    $"[{GraphicSettings.SecondaryColor}]?[/]");
            }
        }
        
        AnsiConsole.Write(table);
        
        
        var grid = new Grid()
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn())
            .AddRow(
                new Panel($"[bold]{runningProcesses.Length}[/]\nTotal").BorderColor(GraphicSettings.GetThemeColor), //исправить
                new Panel($"[bold]{runningProcesses.Count(p => 
                {
                    try { return p.Responding; } 
                    catch { return false; }
                })}[/]\nResponding").BorderColor(Color.White),
                new Panel($"[bold]{runningProcesses.Count(p => p.BasePriority > 8)}[/]\nSystem").BorderColor(GraphicSettings.GetThemeColor) //исрпавить
            );
        
        AnsiConsole.Write(grid);
    }
    
    
    public static void FindProcessByNameSpectre()
    {
        Console.Clear();
        
        string processName = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter process name:[/]")
                .PromptStyle(GraphicSettings.AccentColor));
        
        if (string.IsNullOrWhiteSpace(processName))
        {
            AnsiConsole.MarkupLine("[red]Process name cannot be empty![/]");
            return;
        }
        
        Process[] processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
        
        if (processes.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]Process '{processName}' not found.[/]");
            return;
        }
        
        var table = new Table()
            .Title($"[{GraphicSettings.AccentColor}]Found {processes.Length} processes[/]")
            .BorderColor(GraphicSettings.GetThemeColor)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]ID[/]"))
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]"))
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Memory (MB)[/]"))
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Status[/]"));
        
        foreach (Process proc in processes)
        {
            try
            {
                string status = proc.Responding ? $"[{GraphicSettings.SecondaryColor}]Running[/]" : "[red]Not Responding[/]";
                string memory = $"{proc.WorkingSet64 / 1024 / 1024:N0}";
                
                table.AddRow(
                    $"{proc.Id}",
                    $"{proc.ProcessName}",
                    memory,
                    status);
            }
            catch (Exception ex)
            {
                table.AddRow(
                    $"{proc.Id}",
                    $"{proc.ProcessName}",
                    "N/A",
                    $"[red]{ex.Message}[/]");
            }
        }
        
        AnsiConsole.Write(table);
    }
    
    
    public static void KillProcessByIdSpectre()
    {
        Console.Clear();
        
        int processId = AnsiConsole.Prompt(
            new TextPrompt<int>($"[{GraphicSettings.SecondaryColor}]Enter process ID to kill:[/]")
                .PromptStyle(GraphicSettings.AccentColor) //исправить
                .ValidationErrorMessage("[red]Invalid process ID![/]"));
        
        try
        {
            Process process = Process.GetProcessById(processId);
            
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Found process: {process.ProcessName} (ID: {process.Id})[/]");
            
            if (AnsiConsole.Confirm("[red]Are you sure you want to kill this process?[/]", false))
            {
                process.Kill();
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]✓ Process {process.ProcessName} (ID: {process.Id}) killed successfully.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Operation cancelled.[/]");
            }
        }
        catch (ArgumentException)
        {
            AnsiConsole.MarkupLine($"[red]Process with ID {processId} not found.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error killing process: {ex.Message}[/]");
            
            
            if (ex.Message.Contains("access") || ex.Message.Contains("denied"))
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Try running this program as administrator.[/]");
            }
        }
    }
    
    
    public static void KillProcessByNameSpectre()
    {
        Console.Clear();
        
        string processName = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter process name to kill:[/]")
                .PromptStyle(GraphicSettings.AccentColor));
        
        if (string.IsNullOrWhiteSpace(processName))
        {
            AnsiConsole.MarkupLine("[red]Process name cannot be empty![/]");
            return;
        }
        
        Process[] processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
        
        if (processes.Length == 0)
        {
            AnsiConsole.MarkupLine($"[red]Process '{processName}' not found.[/]");
            return;
        }
        
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Found {processes.Length} processes with name '{processName}'[/]");
        
        if (AnsiConsole.Confirm($"[red]Kill all {processes.Length} processes?[/]", false))
        {
            int killedCount = 0;
            int failedCount = 0;
            
            foreach (Process proc in processes)
            {
                try
                {
                    proc.Kill();
                    killedCount++;
                    AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]✓ {proc.ProcessName} (ID: {proc.Id}) killed.[/]");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    AnsiConsole.MarkupLine($"[red]✗ Failed to kill {proc.ProcessName}: {ex.Message}[/]");
                }
            }
            
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Successfully killed: {killedCount} of {processes.Length}[/]");
            if (failedCount > 0)
            {
                AnsiConsole.MarkupLine($"[red]Failed to kill: {failedCount} processes[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Operation cancelled.[/]");
        }
    }
    
    
    public static void StartNewProcessSpectre()
    {
        Console.Clear();
        
        string programPath = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter program path or name:[/]")
                .PromptStyle(GraphicSettings.AccentColor)
                .DefaultValue("notepad.exe"));
        
        string arguments = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter arguments (optional):[/]")
                .PromptStyle(GraphicSettings.AccentColor)
                .AllowEmpty());
        
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = programPath,
                Arguments = arguments,
                UseShellExecute = true
            };
            
            Process process = Process.Start(startInfo);
            
            if (process != null)
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]✓ Process started! ID: {process.Id}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]✓ Process started (using ShellExecute).[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error starting process: {ex.Message}[/]");
        }
    }
    
    
    public static void ShowSystemProcessesSpectre()
    {
        Console.Clear();
        
        AnsiConsole.Status()
            .Start("Loading system processes...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse(GraphicSettings.AccentColor));
                Thread.Sleep(150);
            });
        
        Process[] processes = Process.GetProcesses();
        
        var systemProcesses = processes
            .Where(p => p.BasePriority > 8)
            .OrderByDescending(p => p.BasePriority)
            .ThenBy(p => p.ProcessName);
        
        var table = new Table()
            .Title($"[{GraphicSettings.SecondaryColor}]System Processes: {systemProcesses.Count()}[/]")
            .BorderColor(GraphicSettings.GetThemeColor) 
            .Border(TableBorder.HeavyHead)
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]ID[/]"))
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Name[/]"))
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Priority[/]"))
            .AddColumn(new TableColumn($"[{GraphicSettings.SecondaryColor}]Memory (MB)[/]"));
        
        foreach (Process proc in systemProcesses)
        {
            try
            {
                string memory = $"{proc.WorkingSet64 / 1024 / 1024:N0}";
                string priorityColor = proc.BasePriority > 12 ? "red" : GraphicSettings.AccentColor;

                table.AddRow(
                    $"{proc.Id}",
                    $"[{GraphicSettings.SecondaryColor}]{proc.ProcessName}[/]",
                    $"[{priorityColor}]{proc.BasePriority}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{memory}[/]");
            }
            catch
            {
                table.AddRow(
                    $"{proc.Id}",
                    $"[{GraphicSettings.SecondaryColor}]{proc.ProcessName}[/]",
                    $"[{GraphicSettings.SecondaryColor}]{proc.BasePriority}[/]",
                    "[red]N/A[/]");
            }
        }
        
        AnsiConsole.Write(table);
        
        AnsiConsole.MarkupLine("\n[red]⚠️  Warning: These are system processes. Be careful when modifying them![/]");
    }
    
    
    public static void ShowProcessInfoSpectre()
    {
        Console.Clear();
        
        int processId = AnsiConsole.Prompt(
            new TextPrompt<int>($"[{GraphicSettings.SecondaryColor}]Enter process ID for details:[/]")
                .PromptStyle(GraphicSettings.AccentColor)
                .ValidationErrorMessage("[red]Invalid process ID![/]"));
        
        try
        {
            Process process = Process.GetProcessById(processId);
            
            var panel = new Panel($"[{GraphicSettings.SecondaryColor}]{process.ProcessName}[/] (ID: {process.Id})")
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(GraphicSettings.GetThemeColor),
                Padding = new Padding(1, 1, 1, 1)
            };
            
            AnsiConsole.Write(panel);
            
            
            var infoTable = new Table()
                .Border(TableBorder.None)
                .HideHeaders()
                .AddColumn("")
                .AddColumn("");
            
            try
            {
                infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]Start Time:[/]", $"[{GraphicSettings.SecondaryColor}]{process.StartTime:yyyy-MM-dd HH:mm:ss}[/]");
                infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]Total CPU Time:[/]", $"[{GraphicSettings.SecondaryColor}]{process.TotalProcessorTime}[/]");
            }
            catch
            {
                infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]Start Time:[/]", "[red]Access Denied[/]");
            }
            
            infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]:[/]", $"[{GraphicSettings.SecondaryColor}]{process.BasePriority}[/]");
            infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]:[/]", process.Responding ? "[green]Yes[/]" : "[red]No[/]");
            infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]:[/]", $"[{GraphicSettings.SecondaryColor}]{process.SessionId}[/]");
            
            try
            {
                infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]:[/]", $"[{GraphicSettings.SecondaryColor}]{process.WorkingSet64 / 1024 / 1024:N0} MB[/]");
                infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]:[/]", $"[{GraphicSettings.SecondaryColor}]{process.PrivateMemorySize64 / 1024 / 1024:N0} MB[/]");
            }
            catch
            {
                infoTable.AddRow($"[{GraphicSettings.SecondaryColor}]:[/]", "[red]Access Denied[/]");
            }
            
            AnsiConsole.Write(infoTable.BorderColor(GraphicSettings.GetThemeColor));
            
            
            try
            {
                var modules = process.Modules.Cast<ProcessModule>();
                if (modules.Any())
                {
                    AnsiConsole.MarkupLine($"\n[{GraphicSettings.SecondaryColor}]Top Modules:[/]");
                    foreach (var module in modules)
                    {
                        AnsiConsole.MarkupLine($"  [{GraphicSettings.SecondaryColor}]•[/] [{GraphicSettings.SecondaryColor}]{module.ModuleName}[/]");
                    }
                }
            }
            catch
            {
                
            }
        }
        catch (ArgumentException)
        {
            AnsiConsole.MarkupLine($"[red]Process with ID {processId} not found.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
        }
    }
    
    
    public static void ExportProcessesToFileSpectre()
    {
        Console.Clear();
        
        string fileName = AnsiConsole.Prompt(
            new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Enter file name (without extension):[/]")
                .PromptStyle(GraphicSettings.AccentColor)
                .DefaultValue("process_list"));
        
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string filePath = Path.Combine(desktopPath, $"{fileName}.txt");
        
        Process[] processes = Process.GetProcesses();
        
        using (StreamWriter sw = new(filePath))
        {
            sw.WriteLine("=".PadRight(80, '='));
            sw.WriteLine("PROCESS LIST");
            sw.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sw.WriteLine($"Total Processes: {processes.Length}");
            sw.WriteLine("=".PadRight(80, '='));
            sw.WriteLine();
            
            foreach (Process proc in processes.OrderBy(p => p.ProcessName))
            {
                try
                {
                    sw.WriteLine($"ID: {proc.Id,-8} | Name: {proc.ProcessName,-25} | Priority: {proc.BasePriority,-3} | Memory: {proc.WorkingSet64 / 1024 / 1024:N0} MB");
                }
                catch
                {
                    sw.WriteLine($"ID: {proc.Id,-8} | Name: {proc.ProcessName,-25} | [ACCESS DENIED]");
                }
            }
        }
        
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]✓ Process list exported to:[/] [{GraphicSettings.SecondaryColor}]{filePath}[/]");
        
        if (AnsiConsole.Confirm($"[{GraphicSettings.SecondaryColor}]Open the file?[/]", false))
        {
            Process.Start("notepad.exe", filePath);
        }
    }
    
    
    public static void CleanDeadProcessesSpectre()
    {
        Console.Clear();
        
        if (!AnsiConsole.Confirm("[red]This will attempt to kill all non-responding processes. Continue?[/]", false))
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Operation cancelled.[/]");
            return;
        }
        
        Process[] processes = Process.GetProcesses();
        int cleanedCount = 0;
        int failedCount = 0;
        
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask($"[{GraphicSettings.SecondaryColor}]Cleaning dead processes...[/]", maxValue: processes.Length);
                
                foreach (Process proc in processes)
                {
                    try
                    {
                        
                        bool responding = proc.Responding;
                    }
                    catch
                    {
                        
                        try
                        {
                            proc.Kill();
                            cleanedCount++;
                            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]✓ Cleaned: {proc.ProcessName} (ID: {proc.Id})[/]");
                        }
                        catch
                        {
                            failedCount++;
                        }
                    }
                    
                    task.Increment(1);
                }
            });
        
        AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}]Cleaning completed![/]");
        AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}]Cleaned processes: {cleanedCount}[/]");
        AnsiConsole.MarkupLine($"[red]Failed to clean: {failedCount}[/]");
    }
    
    
    public static void ShowAllProcesses() => ShowAllProcessesSpectre();
    public static void FindProcessByName() => FindProcessByNameSpectre();
    public static void KillProcessById() => KillProcessByIdSpectre();
    public static void KillProcessByName() => KillProcessByNameSpectre();
    public static void StartNewProcess() => StartNewProcessSpectre();
    public static void ShowProcessInfo() => ShowProcessInfoSpectre();
    public static void ShowSystemProcesses() => ShowSystemProcessesSpectre();
    public static void ExportProcessesToFile() => ExportProcessesToFileSpectre();
    public static void CleanDeadProcesses() => CleanDeadProcessesSpectre();
}