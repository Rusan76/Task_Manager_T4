namespace Task_Manager_T4;

using System;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

public class TextDisplay
{
    public static void TypeWrite(string text, int delay = 1)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(delay);
        }
        Console.WriteLine();
    }
}

public class Rain
{
    public static void ShowReadMeWithRain()
    {
        Console.Clear();
        Console.CursorVisible = false;


        var originalForeground = Console.ForegroundColor;
        var originalBackground = Console.BackgroundColor;

        try
        {

            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();


            var cancellationTokenSource = new CancellationTokenSource();
            var rainTask = Task.Run(() => PrintRain(cancellationTokenSource.Token));


            Console.SetCursorPosition(Console.WindowWidth / 2 - 15, 5);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[ PRESS ANY KEY TO CONTINUE ]");


            Console.ReadKey(true);


            cancellationTokenSource.Cancel();
            rainTask.Wait(1000);


            Console.Clear();
            ShowReadMeInformation();
            Console.ReadKey(true);
        }
        finally
        {

            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
            Console.CursorVisible = true;
        }
    }

    private static void PrintRain(CancellationToken cancellationToken)
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        int rainCount = Math.Min(width, 80);
        int[] x = new int[rainCount];
        int[] y = new int[rainCount];
        char[] chars = new char[rainCount];
        ConsoleColor[] colors = new ConsoleColor[rainCount];
        Random rand = new();

        for (int i = 0; i < rainCount; i++)
        {
            x[i] = rand.Next(0, width);
            y[i] = rand.Next(0, height);
            chars[i] = GetRandomRainChar(rand);
            colors[i] = GetRandomRainColor(rand);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                for (int i = 0; i < rainCount; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (y[i] >= 0 && y[i] < height && x[i] >= 0 && x[i] < width)
                    {
                        Console.SetCursorPosition(x[i], y[i]);
                        Console.Write(" ");
                    }

                    y[i]++;

                    if (y[i] >= height)
                    {
                        y[i] = 0;
                        x[i] = rand.Next(0, width);
                        chars[i] = GetRandomRainChar(rand);
                        colors[i] = GetRandomRainColor(rand);
                    }
                    if (y[i] >= 0 && y[i] < height && x[i] >= 0 && x[i] < width)
                    {
                        Console.SetCursorPosition(x[i], y[i]);
                        Console.ForegroundColor = colors[i];
                        Console.Write(chars[i]);
                    }
                }
                Thread.Sleep(30);
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    private static char GetRandomRainChar(Random rand)
    {
        char[] rainChars = ['|', '│', '┃', '╽', '╿', '║', ':', '\'', '.'];
        return rainChars[rand.Next(rainChars.Length)];
    }

    private static ConsoleColor GetRandomRainColor(Random rand)
    {
        ConsoleColor[] rainColors =
        [
            ConsoleColor.Cyan,
            ConsoleColor.Blue,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkBlue,
            ConsoleColor.White,
            ConsoleColor.Gray
        ];
        return rainColors[rand.Next(rainColors.Length)];
    }

    private static void ShowReadMeInformation()
    {
        Console.Clear();
        // AnsiConsole.Write(new Rule().RuleStyle("DarkOrange").Centered());
        // string[] glowColors = ["grey15", "darkorange3", "orange1", "white", "orange1", "darkorange3"];
        // int colorIndex = 0;

        // while (!Console.KeyAvailable)
        // {
        //     Console.SetCursorPosition(0, 1);

        //     string currentColor = glowColors[colorIndex];

        //     AnsiConsole.MarkupLine($"[{currentColor}]╔══════════════════════════════════════════════════════════════════════════════════════╗[/]");
        //     AnsiConsole.MarkupLine($"[{currentColor}]║ This programm made by me =)                                                          ║[/]");
        //     AnsiConsole.MarkupLine($"[{currentColor}]║                                                                                      ║[/]");
        //     AnsiConsole.MarkupLine($"[{currentColor}]║ Anyone can predict the future... but only the dreamer makes it come true             ║[/]");
        //     AnsiConsole.MarkupLine($"[{currentColor}]╚══════════════════════════════════════════════════════════════════════════════════════╝[/]");

        //     colorIndex = (colorIndex + 1) % glowColors.Length;
        //     Thread.Sleep(225);
        //     AnsiConsole.Write(new Rule("[white]Press any key to return in main menu[/]").RuleStyle("DarkOrange").Centered());
        // }

        // Console.ReadKey(true);
        // Console.Clear();
        string textUp = "This programm made by me =)";
        string textDown = "Anyone can predict the future... but only the dreamer makes it come true";
        AnsiConsole.Write(new Rule().RuleStyle("DarkOrange").Centered());
        AnsiConsole.MarkupLine($"[white]╔══════════════════════════════════════════════════════════════════════════════════════╗[/]");
        AnsiConsole.MarkupLine($"[white]║[/] [italic orange1]{textUp}[/][white]                                                          ║[/]");
        AnsiConsole.MarkupLine($"[white]║                                                                                      ║[/]");
        AnsiConsole.MarkupLine($"[white]║[/] [italic orange1]{textDown}[/][white]             ║[/]");
        AnsiConsole.MarkupLine($"[white]╚══════════════════════════════════════════════════════════════════════════════════════╝[/]");
        AnsiConsole.Write(new Rule("[white]Press any key to return in main menu[/]").RuleStyle("DarkOrange").Centered());
        Console.ReadKey();
        Program.Function_list();
    }
}