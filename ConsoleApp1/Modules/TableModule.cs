using System;
using System.Linq;

namespace RestaurantManagementSystem
{
    static class TableModule
    {
        public static void Show()
        {
            while (true)
            {
                UI.Header("TABLE MANAGEMENT");
                ViewAll();
                UI.Divider();
                UI.MenuOption(1, "View Table Details");
                UI.MenuOption(2, "Add Table");
                UI.MenuOption(3, "Update Table Seats");
                UI.MenuOption(0, "Back");
                int c = UI.PromptInt("Choice", 0, 3);
                switch (c)
                {
                    case 1: Details(); break;
                    case 2: AddTable(); break;
                    case 3: UpdateSeats(); break;
                    case 0: return;
                }
            }
        }

        public static void ViewAll()
        {
            Console.WriteLine();
            int cols = 5;
            var tables = DataStore.Tables;
            for (int i = 0; i < tables.Count; i++)
            {
                var t = tables[i];
                ConsoleColor clr = t.Status == TableStatus.Available ? ConsoleColor.Green
                                 : t.Status == TableStatus.Occupied ? ConsoleColor.Red
                                 : ConsoleColor.Yellow;
                Console.ForegroundColor = clr;
                string label = $"[T{t.Id:D2} {t.Seats}P {t.Status.ToString()[..3].ToUpper()}]";
                Console.Write(label.PadRight(16));
                Console.ResetColor();
                if ((i + 1) % cols == 0) Console.WriteLine();
            }
            Console.WriteLine();
        }

        static void Details()
        {
            int id = UI.PromptInt("Table Number", 1, DataStore.Tables.Max(t => t.Id));
            var t = DataStore.Tables.FirstOrDefault(tb => tb.Id == id);
            if (t == null) { UI.Error("Table not found."); UI.PressAny(); return; }
            UI.Header($"TABLE {id} DETAILS");
            Console.WriteLine($"  Seats   : {t.Seats}");
            Console.WriteLine($"  Status  : {t.Status}");
            if (t.CurrentOrderId.HasValue)
                Console.WriteLine($"  Order   : #{t.CurrentOrderId}");
            if (!string.IsNullOrEmpty(t.ReservedBy))
                Console.WriteLine($"  Reserved: {t.ReservedBy} @ {t.ReservationTime:g}");
            UI.PressAny();
        }

        static void AddTable()
        {
            if (!AuthModule.HasAccess(UserRole.Admin)) { UI.Error("Admins only."); UI.PressAny(); return; }
            int seats = UI.PromptInt("Number of seats", 1, 20);
            int newId = DataStore.Tables.Max(t => t.Id) + 1;
            DataStore.Tables.Add(new Table(newId, seats));
            UI.Success($"Table {newId} ({seats} seats) added.");
            UI.PressAny();
        }

        static void UpdateSeats()
        {
            if (!AuthModule.HasAccess(UserRole.Manager)) { UI.Error("Access denied."); UI.PressAny(); return; }
            int id = UI.PromptInt("Table Number");
            var t = DataStore.Tables.FirstOrDefault(tb => tb.Id == id);
            if (t == null) { UI.Error("Table not found."); UI.PressAny(); return; }
            int seats = UI.PromptInt("New seat count", 1, 20);
            t.Seats = seats;
            UI.Success("Updated.");
            UI.PressAny();
        }
    }
}
