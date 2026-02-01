using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Spectre.Console;

namespace Task_Manager_T4;

class SystemBenchmark
{
    public static void ShowBenchmarkMenu()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule("[DarkOrange]System Performance Benchmark[/]").RuleStyle("white").LeftJustified());

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите тип теста:")
                .AddChoices(["CPU Stress Test (Parallel)", "RAM Speed Test", "Назад"]));

        if (choice == "CPU Stress Test (Parallel)") RunCpuBenchmark();
        else if (choice == "RAM Speed Test") RunRamBenchmark();
        else if (choice == "Назад") Program.Function_list();
    }

    private static void RunCpuBenchmark()
    {
        AnsiConsole.MarkupLine("[white]Запуск теста CPU... Все ядра будут нагружены на 5 секунд.[/]");
        
        Stopwatch sw = Stopwatch.StartNew();
        long operations = 0;

        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Star)
            .Start("Выполнение вычислений...", ctx =>
            {
                DateTime endTime = DateTime.Now.AddSeconds(5);
                
                
                Parallel.For(0, Environment.ProcessorCount, i =>
                {
                    while (DateTime.Now < endTime)
                    {
                        
                        double x = Math.Sqrt(Math.Pow(123.45, 67.89));
                        System.Threading.Interlocked.Increment(ref operations);
                    }
                });
            });

        sw.Stop();
        AnsiConsole.MarkupLine($"[DarkOrange]Тест завершен![/]");
        AnsiConsole.MarkupLine($"Выполнено математических операций: [bold]{operations:N0}[/]");
        AnsiConsole.MarkupLine($"Относительный балл (Ops/sec): [bold ]{operations / 5:N0}[/]");
        AnsiConsole.MarkupLine("Press any key to return.");
        Console.ReadLine();
        ShowBenchmarkMenu();
    }

    private static void RunRamBenchmark()
    {
        AnsiConsole.MarkupLine("[DarkOrange]Запуск теста RAM (Запись/Чтение 1 ГБ)...[/]");

        const int size = 1024 * 1024 * 256; 
        int[] array = new int[size];
        Stopwatch sw = new();

        AnsiConsole.Status()
            .Start("Тестирование скорости записи...", ctx =>
            {
                sw.Start();
                for (int i = 0; i < size; i++) array[i] = i;
                sw.Stop();
            });

        double writeSpeed = (1024.0) / sw.Elapsed.TotalSeconds; 
        AnsiConsole.MarkupLine($"Скорость записи: [bold DarkOrange]{writeSpeed:F2} MB/s[/]");

        sw.Restart();
        AnsiConsole.Status()
            .Start("Тестирование скорости чтения...", ctx =>
            {
                long sum = 0;
                for (int i = 0; i < size; i++) sum += array[i];
                sw.Stop();
            });

        double readSpeed = 1024.0 / sw.Elapsed.TotalSeconds;
        AnsiConsole.MarkupLine($"Скорость чтения: [bold ]{readSpeed:F2} MB/s[/]");
        
        
        array = null;
        GC.Collect();
        AnsiConsole.MarkupLine("Press any key to return.");
        Console.ReadLine();
        ShowBenchmarkMenu();
    }
}