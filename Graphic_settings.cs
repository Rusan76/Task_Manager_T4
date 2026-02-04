using System;
using Spectre.Console;

namespace Task_Manager_T4;

public class GraphicSettings
{
    public static int PageSize = 12;
    public static string AccentColor = "orange1";  //MainColor
    public static string SecondaryColor = "white"; //SecondColorForText
    public static string NeutralColor = "grey";    //Additional color
    public static string ThemeName = "Asiimov";

    public static void ChangeTheme()
    {
        Console.Clear();
        AnsiConsole.Write(new Rule("[white]THEME_SELECTOR // SETTINGS[/]").RuleStyle(AccentColor).LeftJustified());

        var themeChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[white]Выберите визуальный стиль интерфейса:[/]")
                .AddChoices(
                [
                    "🟠 Asiimov (Orange/White)",
                    "🔵 Classic (Blue/Grey)",
                    "🟢 Matrix (Green/Black)",
                    "⚪ Light (Black/White)",
                    "⬅ Назад"
                ]));

        switch (themeChoice)
        {
            case "🟠 Asiimov (Orange/White)":
                AccentColor = "orange1";
                SecondaryColor = "white";
                NeutralColor = "grey";
                ThemeName = "Asiimov";
                break;
            case "🔵 Classic (Blue/Grey)":
                AccentColor = "dodgerblue1";
                SecondaryColor = "grey100";
                NeutralColor = "grey54";
                ThemeName = "Classic";
                break;
            case "🟢 Matrix (Green/Black)":
                AccentColor = "green3";
                SecondaryColor = "green1";
                NeutralColor = "darkgreen";
                ThemeName = "Matrix";
                break;
            case "⚪ Light (Black/White)":
                AccentColor = "white";
                SecondaryColor = "black";
                NeutralColor = "grey35";
                ThemeName = "Light";
                break;
            default:
                return;
        }

        AnsiConsole.MarkupLine($"[{SecondaryColor}]Тема '{ThemeName}' успешно применена![/]");
        Console.ReadKey();
    }

    public static Color GetColor(string colorName)
    {
        return colorName switch
        {
            "orange1" => Color.Orange1,
            "white" => Color.White,
            "grey" => Color.Grey,
            "dodgerblue1" => Color.DodgerBlue1,
            "grey100" => Color.Grey100,
            "grey54" => Color.Grey54,
            "green3" => Color.Green3,
            "green1" => Color.Green1,
            "darkgreen" => Color.DarkGreen,
            "black" => Color.Black,
            "grey10" => Color.Grey11,
            "grey35" => Color.Grey35,
            _ => Color.White
        };
    }
    public static Color GetThemeColor
    {
        get
        {
            return AccentColor switch
            {
                "orange1" => Color.Orange1,
                "green3" => Color.Green3,
                "dodgerblue1" => Color.DodgerBlue1,
                "black" => Color.Black,
                _ => Color.White // Цвет по умолчанию
            };
        }
    }
}