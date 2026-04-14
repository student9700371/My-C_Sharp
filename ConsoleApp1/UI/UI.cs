using System;
using System.Collections.Generic;

namespace RestaurantManagementSystem
{
    static class UI
    {
        const int Width = 70;

        public static void Header(string title)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n" + new string('═', Width));
            Console.WriteLine(Center($"🍽  {title}  🍽"));
            Console.WriteLine(new string('═', Width));
            Console.ResetColor();
        }

        public static void SubHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  ── {title} ──");
            Console.ResetColor();
        }

        public static void Success(string msg)
        { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine($"  ✔  {msg}"); Console.ResetColor(); }

        public static void Error(string msg)
        { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"  ✘  {msg}"); Console.ResetColor(); }

        public static void Info(string msg)
        { Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine($"  ℹ  {msg}"); Console.ResetColor(); }

        public static void Warning(string msg)
        { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"  ⚠  {msg}"); Console.ResetColor(); }

        public static void Divider()
        { Console.ForegroundColor = ConsoleColor.DarkGray; Console.WriteLine(new string('─', Width)); Console.ResetColor(); }

        public static string Center(string text)
        { int pad = (Width - text.Length) / 2; return new string(' ', Math.Max(0, pad)) + text; }

        public static void MenuOption(int num, string label)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  [{num}] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(label);
            Console.ResetColor();
        }

        public static string Prompt(string label)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n  ► {label}: ");
            Console.ForegroundColor = ConsoleColor.White;
            string val = Console.ReadLine()?.Trim() ?? "";
            Console.ResetColor();
            return val;
        }

        public static int PromptInt(string label, int min = 0, int max = int.MaxValue)
        {
            while (true)
            {
                string raw = Prompt(label);
                if (int.TryParse(raw, out int v) && v >= min && v <= max) return v;
                Error($"Please enter a number between {min} and {max}.");
            }
        }

        public static decimal PromptDecimal(string label)
        {
            while (true)
            {
                string raw = Prompt(label);
                if (decimal.TryParse(raw, out decimal v) && v >= 0) return v;
                Error("Please enter a valid positive amount.");
            }
        }

        public static bool Confirm(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"\n  ? {msg} (y/n): ");
            Console.ResetColor();
            return (Console.ReadLine()?.Trim().ToLower() == "y");
        }

        public static void PressAny()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("\n  Press any key to continue...");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        public static void TableRow(params (string text, int width, ConsoleColor color)[] cols)
        {
            foreach (var (text, width, color) in cols)
            {
                Console.ForegroundColor = color;
                string cell = text.Length > width ? text[..(width - 1)] + "…" : text.PadRight(width);
                Console.Write(cell + " ");
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        public static void Badge(string text, ConsoleColor bg)
        {
            Console.BackgroundColor = bg;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write($" {text} ");
            Console.ResetColor();
        }
    }
}
