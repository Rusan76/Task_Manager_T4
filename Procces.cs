using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using Spectre.Console;

class Process_management
{
    public static void GetProcces()
    {
        while (true)
        {
            Console.Clear();
            
            AnsiConsole.Write(new Rule("[DarkOrange]Process Management[/]").RuleStyle("white").LeftJustified());
            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[white]Select category[/]")
                    .PageSize(12)
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
            
            Console.WriteLine("\nPress any key to continue...");
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
                ctx.SpinnerStyle(Style.Parse("green"));
                Thread.Sleep(500);
            });
        
        Process[] runningProcesses = Process.GetProcesses();
        
        var table = new Table()
            .Title($"[bold DarkOrange]Running Processes: {runningProcesses.Length}[/]")
            .BorderColor(Color.Blue)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[white]ID[/]").Centered())
            .AddColumn(new TableColumn("[white]Name[/]").LeftAligned())
            .AddColumn(new TableColumn("[white]Memory (MB)[/]").RightAligned())
            .AddColumn(new TableColumn("[white]Priority[/]").Centered())
            .AddColumn(new TableColumn("[white]Status[/]").Centered());
        
        foreach (Process proc in runningProcesses.OrderBy(p => p.ProcessName))
        {
            try
            {
                string status = proc.Responding ? "[DarkOrange]YES[/]" : "[red] NO[/]";
                string memory = $"{proc.WorkingSet64 / 1024 / 1024:N0}";
                string priority = proc.BasePriority.ToString();
                
                table.AddRow(
                    $"[white]{proc.Id}[/]",
                    $"[white]{proc.ProcessName}[/]",
                    $"[white]{memory}[/]",
                    $"[white]{priority}[/]",
                    $"[white]{status}[/]");
            }
            catch 
            {
                table.AddRow(
                    $"[white]{proc.Id}[/]",
                    $"[white]{proc.ProcessName}[/]",
                    $"[white]N/A[/]",
                    $"[white]N/A[/]",
                    "[white]?[/]");
            }
        }
        
        AnsiConsole.Write(table);
        
        
        var grid = new Grid()
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn())
            .AddRow(
                new Panel($"[bold]{runningProcesses.Length}[/]\nTotal").BorderColor(Color.White),
                new Panel($"[bold]{runningProcesses.Count(p => 
                {
                    try { return p.Responding; } 
                    catch { return false; }
                })}[/]\nResponding").BorderColor(Color.White),
                new Panel($"[bold]{runningProcesses.Count(p => p.BasePriority > 8)}[/]\nSystem").BorderColor(Color.White)
            );
        
        AnsiConsole.Write(grid);
    }
    
    
    public static void FindProcessByNameSpectre()
    {
        Console.Clear();
        
        string processName = AnsiConsole.Prompt(
            new TextPrompt<string>("[DarkOrange]Enter process name:[/]")
                .PromptStyle("white"));
        
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
            .Title($"[bold DarkOrange]Found {processes.Length} processes[/]")
            .BorderColor(Color.Green)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[white]ID[/]"))
            .AddColumn(new TableColumn("[white]Name[/]"))
            .AddColumn(new TableColumn("[white]Memory (MB)[/]"))
            .AddColumn(new TableColumn("[white]Status[/]"));
        
        foreach (Process proc in processes)
        {
            try
            {
                string status = proc.Responding ? "[DarkOrange]Running[/]" : "[red]Not Responding[/]";
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
            new TextPrompt<int>("[white]Enter process ID to kill:[/]")
                .PromptStyle("DarkOrange")
                .ValidationErrorMessage("[red]Invalid process ID![/]"));
        
        try
        {
            Process process = Process.GetProcessById(processId);
            
            AnsiConsole.MarkupLine($"[white]Found process: {process.ProcessName} (ID: {process.Id})[/]");
            
            if (AnsiConsole.Confirm("[red]Are you sure you want to kill this process?[/]", false))
            {
                process.Kill();
                AnsiConsole.MarkupLine($"[white]✓ Process {process.ProcessName} (ID: {process.Id}) killed successfully.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[DarkOrange]Operation cancelled.[/]");
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
                AnsiConsole.MarkupLine("[DarkOrange]Try running this program as administrator.[/]");
            }
        }
    }
    
    
    public static void KillProcessByNameSpectre()
    {
        Console.Clear();
        
        string processName = AnsiConsole.Prompt(
            new TextPrompt<string>("[DarkOrange]Enter process name to kill:[/]")
                .PromptStyle("white"));
        
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
        
        AnsiConsole.MarkupLine($"[DarkOrange]Found {processes.Length} processes with name '{processName}'[/]");
        
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
                    AnsiConsole.MarkupLine($"[DarkOrange]✓ {proc.ProcessName} (ID: {proc.Id}) killed.[/]");
                }
                catch (Exception ex)
                {
                    failedCount++;
                    AnsiConsole.MarkupLine($"[red]✗ Failed to kill {proc.ProcessName}: {ex.Message}[/]");
                }
            }
            
            AnsiConsole.MarkupLine($"[yellow]Successfully killed: {killedCount} of {processes.Length}[/]");
            if (failedCount > 0)
            {
                AnsiConsole.MarkupLine($"[red]Failed to kill: {failedCount} processes[/]");
            }
        }
        else
        {
            AnsiConsole.MarkupLine("[DarkOrange]Operation cancelled.[/]");
        }
    }
    
    
    public static void StartNewProcessSpectre()
    {
        Console.Clear();
        
        string programPath = AnsiConsole.Prompt(
            new TextPrompt<string>("[DarkOrange]Enter program path or name:[/]")
                .PromptStyle("white")
                .DefaultValue("notepad.exe"));
        
        string arguments = AnsiConsole.Prompt(
            new TextPrompt<string>("[DarkOrange]Enter arguments (optional):[/]")
                .PromptStyle("white")
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
                AnsiConsole.MarkupLine($"[DarkOrange]✓ Process started! ID: {process.Id}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[DarkOrange]✓ Process started (using ShellExecute).[/]");
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
                ctx.SpinnerStyle(Style.Parse("white"));
                Thread.Sleep(500);
            });
        
        Process[] processes = Process.GetProcesses();
        
        var systemProcesses = processes
            .Where(p => p.BasePriority > 8)
            .OrderByDescending(p => p.BasePriority)
            .ThenBy(p => p.ProcessName);
        
        var table = new Table()
            .Title($"[bold DarkOrange]System Processes: {systemProcesses.Count()}[/]")
            .BorderColor(Color.White)
            .Border(TableBorder.HeavyHead)
            .AddColumn(new TableColumn("[white]ID[/]"))
            .AddColumn(new TableColumn("[white]Name[/]"))
            .AddColumn(new TableColumn("[white]Priority[/]"))
            .AddColumn(new TableColumn("[white]Memory (MB)[/]"));
        
        foreach (Process proc in systemProcesses)
        {
            try
            {
                string memory = $"{proc.WorkingSet64 / 1024 / 1024:N0}";
                string priorityColor = proc.BasePriority > 12 ? "red" : "yellow";
                
                table.AddRow(
                    $"{proc.Id}",
                    $"[white]{proc.ProcessName}[/]",
                    $"[{priorityColor}]{proc.BasePriority}[/]",
                    $"[white]{memory}[/]");
            }
            catch
            {
                table.AddRow(
                    $"{proc.Id}",
                    $"[white]{proc.ProcessName}[/]",
                    $"[white]{proc.BasePriority}[/]",
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
            new TextPrompt<int>("[DarkOrange]Enter process ID for details:[/]")
                .PromptStyle("white")
                .ValidationErrorMessage("[red]Invalid process ID![/]"));
        
        try
        {
            Process process = Process.GetProcessById(processId);
            
            var panel = new Panel($"[bold DarkOrange]{process.ProcessName}[/] (ID: {process.Id})")
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.White),
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
                infoTable.AddRow("[bold]Start Time:[/]", $"[white]{process.StartTime:yyyy-MM-dd HH:mm:ss}[/]");
                infoTable.AddRow("[bold]Total CPU Time:[/]", $"[white]{process.TotalProcessorTime}[/]");
            }
            catch
            {
                infoTable.AddRow("[bold]Start Time:[/]", "[red]Access Denied[/]");
            }
            
            infoTable.AddRow("[bold]Priority:[/]", $"[white]{process.BasePriority}[/]");
            infoTable.AddRow("[bold]Responding:[/]", process.Responding ? "[green]Yes[/]" : "[red]No[/]");
            infoTable.AddRow("[bold]Session ID:[/]", $"[white]{process.SessionId}[/]");
            
            try
            {
                infoTable.AddRow("[bold]Working Memory:[/]", $"[DarkOrange]{process.WorkingSet64 / 1024 / 1024:N0} MB[/]");
                infoTable.AddRow("[bold]Private Memory:[/]", $"[DarkOrange]{process.PrivateMemorySize64 / 1024 / 1024:N0} MB[/]");
            }
            catch
            {
                infoTable.AddRow("[bold]Memory Info:[/]", "[red]Access Denied[/]");
            }
            
            AnsiConsole.Write(infoTable);
            
            
            try
            {
                var modules = process.Modules.Cast<ProcessModule>();
                if (modules.Any())
                {
                    AnsiConsole.MarkupLine("\n[bold white]Top Modules:[/]");
                    foreach (var module in modules)
                    {
                        AnsiConsole.MarkupLine($"  [white]•[/] [white]{module.ModuleName}[/]");
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
            new TextPrompt<string>("[white]Enter file name (without extension):[/]")
                .PromptStyle("DarkOrange")
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
        
        AnsiConsole.MarkupLine($"[DarkOrange]✓ Process list exported to:[/] [DarkOrange]{filePath}[/]");
        
        if (AnsiConsole.Confirm("[white]Open the file?[/]", false))
        {
            Process.Start("notepad.exe", filePath);
        }
    }
    
    
    public static void CleanDeadProcessesSpectre()
    {
        Console.Clear();
        
        if (!AnsiConsole.Confirm("[red]This will attempt to kill all non-responding processes. Continue?[/]", false))
        {
            AnsiConsole.MarkupLine("[DarkOrange]Operation cancelled.[/]");
            return;
        }
        
        Process[] processes = Process.GetProcesses();
        int cleanedCount = 0;
        int failedCount = 0;
        
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask("[DarkOrange]Cleaning dead processes...[/]", maxValue: processes.Length);
                
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
                            AnsiConsole.MarkupLine($"[DarkOrange]✓ Cleaned: {proc.ProcessName} (ID: {proc.Id})[/]");
                        }
                        catch
                        {
                            failedCount++;
                        }
                    }
                    
                    task.Increment(1);
                }
            });
        
        AnsiConsole.MarkupLine($"[DarkOrange]Cleaning completed![/]");
        AnsiConsole.MarkupLine($"[white]Cleaned processes: {cleanedCount}[/]");
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