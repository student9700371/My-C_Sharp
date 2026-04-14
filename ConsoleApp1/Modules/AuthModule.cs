using System;
using System.Linq;
using System.Text;

namespace RestaurantManagementSystem
{
    static class AuthModule
    {
        public static bool Login()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════════════════╗
  ║                                                          ║
  ║        R E S T A U R A N T   M A N A G E M E N T        ║
  ║                    S Y S T E M                           ║
  ║                                                          ║
  ║                  ★  PK Foods ★                          ║
  ╚══════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            for (int attempt = 0; attempt < 3; attempt++)
            {
                string user = UI.Prompt("Username");
                Console.Write("  ► Password: ");
                string pass = ReadPassword();

                var staff = DataStore.StaffList.FirstOrDefault(
                    s => s.Username == user && s.Password == pass && s.IsActive);
                if (staff != null)
                {
                    DataStore.CurrentUser = staff;
                    UI.Success($"Welcome, {staff.Name}! [{staff.Role}]");
                    System.Threading.Thread.Sleep(800);
                    return true;
                }
                UI.Error($"Invalid credentials. {2 - attempt} attempt(s) left.");
            }
            return false;
        }

        static string ReadPassword()
        {
            var sb = new StringBuilder();
            ConsoleKeyInfo key;
            Console.ForegroundColor = ConsoleColor.White;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && sb.Length > 0) { sb.Remove(sb.Length - 1, 1); Console.Write("\b \b"); }
                else if (key.Key != ConsoleKey.Backspace) { sb.Append(key.KeyChar); Console.Write('*'); }
            }
            Console.ResetColor();
            Console.WriteLine();
            return sb.ToString();
        }

        public static bool HasAccess(UserRole required) =>
            DataStore.CurrentUser != null &&
            (int)DataStore.CurrentUser.Role <= (int)required;
    }
}
