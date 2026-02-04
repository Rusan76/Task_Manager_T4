using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using Task_Manager_T4;

class MainFF
{

    public static void PrintFunctions()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]File and folder manager[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        
        var user_choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .PageSize(GraphicSettings.PageSize)
                .AddChoices([
                    "Перейти к папкам",
                    "Перейти к файлам",
                    "Назад"
                ]));
                
        switch (user_choice)
        {
            case "Перейти к папкам":
                Folders.Main_Menu_Folder();
                break;
            case "Перейти к файлам":
                Files.Main_Menu_Files();
                break;
            case "Назад":
                Console.Clear();
                return;
        }
    }
}

class Folders
{
    public static void Main_Menu_Folder()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Менеджер папок[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{GraphicSettings.SecondaryColor}]Что вы хотите сделать с папками?[/]")
                .PageSize(GraphicSettings.PageSize)
                .AddChoices([
                    "Создать папку",
                    "Удалить папку",
                    "Информация о папках",
                    "Назад"
                ]));
                
        switch (choice)
        {
            case "Создать папку":
                CreateFolder();
                Console.Clear();
                break;
            case "Удалить папку":
                DeleteFolder(AskCustomPath());
                Console.Clear();
                break;
            case "Информация о папках":
                CountAllFoldersSafe(AskCustomPath());
                Console.Clear();
                break;
            case "Назад":
                MainFF.PrintFunctions();
                break;
        }
    }

    private static void CountAllFoldersSafe(string path)
    {
        int count = 0;

        AnsiConsole.Status()
            .Start($"[{GraphicSettings.AccentColor}]Сканирование папок... это может занять время[/]", ctx =>
            {
                Stack<string> stack = new();
                stack.Push(path);

                while (stack.Count > 0)
                {
                    string currentDir = stack.Pop();
                    try
                    {
                        string[] subDirs = Directory.GetDirectories(currentDir);
                        foreach (string str in subDirs)
                        {
                            stack.Push(str);
                            count++;
                        }
                        ctx.Status($"[{GraphicSettings.NeutralColor}]Найдено папок: {count}...[/]");
                    }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (DirectoryNotFoundException) { continue; }
                    catch (IOException) { continue; }
                }
            });

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.SecondaryColor}]Готово![/] [{GraphicSettings.SecondaryColor}]Общее количество доступных папок в {path}:[/] {count}");
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    public static void DeleteFolder(string path)
    {
        if (!AnsiConsole.Confirm($"[{GraphicSettings.SecondaryColor}]Вы уверены, что хотите удалить папку и всё её содержимое:[/] {Markup.Escape(path)}?"))
        {
            return;
        }

        try
        {
            Directory.Delete(path, true);
            AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}][+] Папка успешно удалена.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][ERROR] Не удалось удалить папку:[/] {Markup.Escape(ex.Message)}");
        }

        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    public static void CreateFolder()
    {
        string basePath = "";

        var menuChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{GraphicSettings.SecondaryColor}]Выберите расположение для новой папки:[/]")
                .AddChoices([
                    "Рабочий стол",
                    "Документы",
                    "AppData",
                    "Ввести свой путь (Custom Path)",
                    "Назад"
                ]));

        switch (menuChoice)
        {
            case "Рабочий стол":
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                break;
            case "Документы":
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                break;
            case "AppData":
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                break;
            case "Ввести свой путь (Custom Path)":
                basePath = AskCustomPath();
                break;
            case "Назад":
                return;
        }

        if (string.IsNullOrEmpty(basePath)) return;

        string folderName = AskFolderName();

        if (!string.IsNullOrEmpty(folderName))
        {
            CreateReportFolder(basePath, folderName);
        }
    }

    protected static string AskCustomPath(bool isFile = false)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Введите полный путь к {(isFile ? "файлу" : "директории")}:[/]")
                .Validate(path =>
                {
                    if (isFile)
                        return File.Exists(path)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("[red]Файл не найден![/]");

                    return Directory.Exists(path)
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Директория не найдена![/]");
                }));
    }

    private static string AskFolderName()
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>($"[{GraphicSettings.SecondaryColor}]Введите имя для новой папки:[/]")
                .AllowEmpty());
    }

    public static void CreateReportFolder(string basePath, string folderName)
    {
        string folderPath = Path.Combine(basePath, folderName);

        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][+] Папка успешно создана:[/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(folderPath)}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[{GraphicSettings.SecondaryColor}][!] Папка уже существует:[/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(folderPath)}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][ERROR] Ошибка при создании:[/] {Markup.Escape(ex.Message)}");
        }

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Нажмите любую клавишу для продолжения...[/]");
        Console.ReadKey();
    }
}

class Files() : Folders
{
    public static void Main_Menu_Files()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[{GraphicSettings.SecondaryColor}]Файлы[/]").RuleStyle(GraphicSettings.AccentColor).LeftJustified());
        
        var fileChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{GraphicSettings.SecondaryColor}]Что вы хотите сделать с файлами?[/]")
                .PageSize(GraphicSettings.PageSize)
                .AddChoices([
                    "Информация о файле",
                    "Найти файл",
                    "Количество файлов в папке",
                    "Создать новый файл",
                    "Удалить файл",
                    "Назад"
                ]));
                
        switch (fileChoice)
        {
            case "Информация о файле":
                FileInfo();
                Console.Clear();
                break;
            case "Найти файл":
                FindFile(AskCustomPath());
                Console.Clear();
                break;
            case "Количество файлов в папке":
                CountFiles(AskCustomPath());
                Console.Clear();
                break;
            case "Создать новый файл":
                CreateFile(AskCustomPath());
                Console.Clear();
                break;
            case "Удалить файл":
                DeleteFile(AskCustomPath(isFile: true));
                Console.Clear();
                break;
            case "Назад":
                MainFF.PrintFunctions();
                Console.Clear();
                break;
        }
    }

    private static void FindFile(string path)
    {
        var FindFile = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{GraphicSettings.SecondaryColor}]Найти файл по расширению или по имени[/]")
                .PageSize(GraphicSettings.PageSize)
                .AddChoices([
                    "По имени",
                    "По расширении"
                ]));
                
        switch (FindFile)
        {
            case "По имени":
                FindFileByName(path);
                break;
            case "По расширении":
                FindFileByExtension(path);
                break;
        }
    }

    private static void FindFileByName(string path)
    {
        AnsiConsole.Markup($"[{GraphicSettings.SecondaryColor}]Введите имя файла или его часть:[/] ");
        string fileName = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(fileName)) return;

        string searchPattern = $"*{fileName}*";

        try
        {
            AnsiConsole.Status()
                .Start($"[{GraphicSettings.NeutralColor}]Поиск '{fileName}'...[/]", ctx =>
                {
                    var files = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][[match]][/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(file)}[/]");
                    }
                });
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Ошибка:[/] {e.Message}");
        }
        
        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    private static void FindFileByExtension(string path)
    {
        AnsiConsole.Markup($"[{GraphicSettings.SecondaryColor}]Введите расширение (например, exe или .txt):[/] ");
        string extension = Console.ReadLine()?.Trim().ToLower();

        if (string.IsNullOrEmpty(extension)) return;

        if (!extension.StartsWith("*."))
        {
            extension = extension.StartsWith(".") ? "*" + extension : "*." + extension;
        }

        try
        {
            AnsiConsole.Status()
                .Start($"[{GraphicSettings.NeutralColor}]Поиск файлов {extension}...[/]", ctx =>
                {
                    var files = Directory.EnumerateFiles(path, extension, SearchOption.AllDirectories);
                    bool found = false;
                    
                    foreach (string file in files)
                    {
                        AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][[found]][/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(file)}[/]");
                        found = true;
                    }

                    if (!found)
                    {
                        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Файлы с таким расширением не найдены.[/]");
                    }
                });
        }
        catch (UnauthorizedAccessException)
        {
            AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Ошибка: недостаточно прав для сканирования некоторых подпапок.[/]");
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[red]Ошибка:[/] {e.Message}");
        }

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    private static void DeleteFile(string path)
    {
        if (!AnsiConsole.Confirm($"[{GraphicSettings.AccentColor}]Удалить файл:[/] {Markup.Escape(path)}?"))
        {
            return;
        }

        try
        {
            File.Delete(path);
            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][+] Файл успешно удален.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][ERROR] Не удалось удалить файл:[/] {Markup.Escape(ex.Message)}");
        }

        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    private static void CreateFile(string path)
    {
        string fileName = AnsiConsole.Ask<string>($"[{GraphicSettings.SecondaryColor}]Введите имя файла (без расширения):[/]");

        string extension = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{GraphicSettings.AccentColor}]Выберите или введите расширение:[/]")
                .AddChoices([".txt", ".json", ".log", ".md", "Свое расширение..."]));

        if (extension == "Свое расширение...")
        {
            extension = AnsiConsole.Ask<string>($"[{GraphicSettings.SecondaryColor}]Введите расширение (с точкой, например .cfg):[/]");
        }

        if (!extension.StartsWith(".")) extension = "." + extension;
        string fullPath = Path.Combine(path, fileName + extension);

        try
        {
            using (FileStream fs = File.Create(fullPath)) { }
            AnsiConsole.MarkupLine($"[{GraphicSettings.AccentColor}][+] Файл успешно создан:[/] [{GraphicSettings.SecondaryColor}]{Markup.Escape(fullPath)}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red][ERROR] Не удалось создать файл:[/] {Markup.Escape(ex.Message)}");
        }

        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    private static void FileInfo()
    {
        string path = AskCustomPath(isFile: true);
        FileInfo info = new(path);

        var table = new Table()
            .Border(TableBorder.Rounded);
            // .BorderColor(Color.FromName(GraphicSettings.AccentColor)); //Починить как-нибуудь потом
            
        table.AddColumn($"[{GraphicSettings.AccentColor}]Свойство[/]");
        table.AddColumn($"[{GraphicSettings.AccentColor}]Значение[/]");

        table.AddRow("Имя", $"[{GraphicSettings.SecondaryColor}]{info.Name}[/]");
        table.AddRow("Размер", $"[{GraphicSettings.SecondaryColor}]{info.Length / 1024.0:F2} KB[/]");
        table.AddRow("Создан", $"[{GraphicSettings.SecondaryColor}]{info.CreationTime:G}[/]");

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }

    private static void CountFiles(string path)
    {
        int count = 0;

        AnsiConsole.Status()
            .Start($"[{GraphicSettings.NeutralColor}]Сканирование файлов... это может занять время[/]", ctx =>
            {
                Stack<string> stack = new();
                stack.Push(path);

                while (stack.Count > 0)
                {
                    string currentDir = stack.Pop();
                    try
                    {
                        string[] files = Directory.GetFiles(currentDir);
                        count += files.Length;

                        string[] subDirs = Directory.GetDirectories(currentDir);
                        foreach (string str in subDirs)
                        {
                            stack.Push(str);
                        }

                        ctx.Status($"[{GraphicSettings.NeutralColor}]Найдено файлов: {count}...[/]");
                    }
                    catch (UnauthorizedAccessException) { continue; }
                    catch (DirectoryNotFoundException) { continue; }
                }
            });

        AnsiConsole.MarkupLine($"\n[{GraphicSettings.AccentColor}]Готово![/] [{GraphicSettings.SecondaryColor}]Общее количество файлов в {path}:[/] {count}");
        AnsiConsole.MarkupLine($"[{GraphicSettings.NeutralColor}]Нажмите любую клавишу...[/]");
        Console.ReadKey();
    }
}