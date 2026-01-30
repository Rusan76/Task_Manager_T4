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
            
             AnsiConsole.Write(
                new FigletText("Procces Manager")
                    .Centered()
                    .Color(Color.Red));

            
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]PROCESS MANAGEMENT[/]")
                    .PageSize(10)
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
            .Title($"[bold yellow]Running Processes: {runningProcesses.Length}[/]")
            .BorderColor(Color.Blue)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[cyan]ID[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Name[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Memory (MB)[/]").RightAligned())
            .AddColumn(new TableColumn("[cyan]Priority[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Status[/]").Centered());
        
        foreach (Process proc in runningProcesses.OrderBy(p => p.ProcessName))
        {
            try
            {
                string status = proc.Responding ? "[green]YES[/]" : "[red] NO[/]";
                string memory = $"{proc.WorkingSet64 / 1024 / 1024:N0}";
                string priority = proc.BasePriority.ToString();
                
                table.AddRow(
                    $"[grey]{proc.Id}[/]",
                    $"[white]{proc.ProcessName}[/]",
                    $"[yellow]{memory}[/]",
                    $"[cyan]{priority}[/]",
                    $"[cyan]{status}[/]");
            }
            catch 
            {
                table.AddRow(
                    $"[grey]{proc.Id}[/]",
                    $"[white]{proc.ProcessName}[/]",
                    $"[red]N/A[/]",
                    $"[grey]N/A[/]",
                    "[grey]?[/]");
            }
        }
        
        AnsiConsole.Write(table);
        
        
        var grid = new Grid()
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn().PadRight(2))
            .AddColumn(new GridColumn())
            .AddRow(
                new Panel($"[bold]{runningProcesses.Length}[/]\nTotal").BorderColor(Color.Green),
                new Panel($"[bold]{runningProcesses.Count(p => 
                {
                    try { return p.Responding; } 
                    catch { return false; }
                })}[/]\nResponding").BorderColor(Color.Blue),
                new Panel($"[bold]{runningProcesses.Count(p => p.BasePriority > 8)}[/]\nSystem").BorderColor(Color.Red)
            );
        
        AnsiConsole.Write(grid);
    }
    
    
    public static void FindProcessByNameSpectre()
    {
        Console.Clear();
        
        string processName = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Enter process name:[/]")
                .PromptStyle("yellow"));
        
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
            .Title($"[bold yellow]Found {processes.Length} processes[/]")
            .BorderColor(Color.Green)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[cyan]ID[/]"))
            .AddColumn(new TableColumn("[cyan]Name[/]"))
            .AddColumn(new TableColumn("[cyan]Memory (MB)[/]"))
            .AddColumn(new TableColumn("[cyan]Status[/]"));
        
        foreach (Process proc in processes)
        {
            try
            {
                string status = proc.Responding ? "[green]Running[/]" : "[red]Not Responding[/]";
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
            new TextPrompt<int>("[green]Enter process ID to kill:[/]")
                .PromptStyle("yellow")
                .ValidationErrorMessage("[red]Invalid process ID![/]"));
        
        try
        {
            Process process = Process.GetProcessById(processId);
            
            AnsiConsole.MarkupLine($"[yellow]Found process: {process.ProcessName} (ID: {process.Id})[/]");
            
            if (AnsiConsole.Confirm("[red]Are you sure you want to kill this process?[/]", false))
            {
                process.Kill();
                AnsiConsole.MarkupLine($"[green]✓ Process {process.ProcessName} (ID: {process.Id}) killed successfully.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Operation cancelled.[/]");
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
                AnsiConsole.MarkupLine("[yellow]Try running this program as administrator.[/]");
            }
        }
    }
    
    
    public static void KillProcessByNameSpectre()
    {
        Console.Clear();
        
        string processName = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Enter process name to kill:[/]")
                .PromptStyle("yellow"));
        
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
        
        AnsiConsole.MarkupLine($"[yellow]Found {processes.Length} processes with name '{processName}'[/]");
        
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
                    AnsiConsole.MarkupLine($"[green]✓ {proc.ProcessName} (ID: {proc.Id}) killed.[/]");
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
            AnsiConsole.MarkupLine("[yellow]Operation cancelled.[/]");
        }
    }
    
    
    public static void StartNewProcessSpectre()
    {
        Console.Clear();
        
        string programPath = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Enter program path or name:[/]")
                .PromptStyle("yellow")
                .DefaultValue("notepad.exe"));
        
        string arguments = AnsiConsole.Prompt(
            new TextPrompt<string>("[green]Enter arguments (optional):[/]")
                .PromptStyle("yellow")
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
                AnsiConsole.MarkupLine($"[green]✓ Process started! ID: {process.Id}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[green]✓ Process started (using ShellExecute).[/]");
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
                ctx.SpinnerStyle(Style.Parse("red"));
                Thread.Sleep(500);
            });
        
        Process[] processes = Process.GetProcesses();
        
        var systemProcesses = processes
            .Where(p => p.BasePriority > 8)
            .OrderByDescending(p => p.BasePriority)
            .ThenBy(p => p.ProcessName);
        
        var table = new Table()
            .Title($"[bold red]System Processes: {systemProcesses.Count()}[/]")
            .BorderColor(Color.Red)
            .Border(TableBorder.HeavyHead)
            .AddColumn(new TableColumn("[cyan]ID[/]"))
            .AddColumn(new TableColumn("[cyan]Name[/]"))
            .AddColumn(new TableColumn("[cyan]Priority[/]"))
            .AddColumn(new TableColumn("[cyan]Memory (MB)[/]"));
        
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
                    $"[cyan]{memory}[/]");
            }
            catch
            {
                table.AddRow(
                    $"{proc.Id}",
                    $"[grey]{proc.ProcessName}[/]",
                    $"[grey]{proc.BasePriority}[/]",
                    "[red]N/A[/]");
            }
        }
        
        AnsiConsole.Write(table);
        
        AnsiConsole.MarkupLine("\n[yellow]⚠️  Warning: These are system processes. Be careful when modifying them![/]");
    }
    
    
    public static void ShowProcessInfoSpectre()
    {
        Console.Clear();
        
        int processId = AnsiConsole.Prompt(
            new TextPrompt<int>("[green]Enter process ID for details:[/]")
                .PromptStyle("yellow")
                .ValidationErrorMessage("[red]Invalid process ID![/]"));
        
        try
        {
            Process process = Process.GetProcessById(processId);
            
            var panel = new Panel($"[bold cyan]{process.ProcessName}[/] (ID: {process.Id})")
            {
                Border = BoxBorder.Double,
                BorderStyle = new Style(Color.Cyan1),
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
                infoTable.AddRow("[bold]Working Memory:[/]", $"[yellow]{process.WorkingSet64 / 1024 / 1024:N0} MB[/]");
                infoTable.AddRow("[bold]Private Memory:[/]", $"[yellow]{process.PrivateMemorySize64 / 1024 / 1024:N0} MB[/]");
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
                    AnsiConsole.MarkupLine("\n[bold cyan]Top Modules:[/]");
                    foreach (var module in modules)
                    {
                        AnsiConsole.MarkupLine($"  [grey]•[/] [white]{module.ModuleName}[/]");
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
            new TextPrompt<string>("[green]Enter file name (without extension):[/]")
                .PromptStyle("yellow")
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
        
        AnsiConsole.MarkupLine($"[green]✓ Process list exported to:[/] [yellow]{filePath}[/]");
        
        if (AnsiConsole.Confirm("[blue]Open the file?[/]", false))
        {
            Process.Start("notepad.exe", filePath);
        }
    }
    
    
    public static void CleanDeadProcessesSpectre()
    {
        Console.Clear();
        
        if (!AnsiConsole.Confirm("[red]This will attempt to kill all non-responding processes. Continue?[/]", false))
        {
            AnsiConsole.MarkupLine("[yellow]Operation cancelled.[/]");
            return;
        }
        
        Process[] processes = Process.GetProcesses();
        int cleanedCount = 0;
        int failedCount = 0;
        
        AnsiConsole.Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask("[green]Cleaning dead processes...[/]", maxValue: processes.Length);
                
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
                            AnsiConsole.MarkupLine($"[green]✓ Cleaned: {proc.ProcessName} (ID: {proc.Id})[/]");
                        }
                        catch
                        {
                            failedCount++;
                        }
                    }
                    
                    task.Increment(1);
                }
            });
        
        AnsiConsole.MarkupLine($"[yellow]Cleaning completed![/]");
        AnsiConsole.MarkupLine($"[green]Cleaned processes: {cleanedCount}[/]");
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