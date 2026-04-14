using System;
using System.Linq;

namespace RestaurantManagementSystem
{
    static class MenuModule
    {
        public static void Show()
        {
            while (true)
            {
                UI.Header("MENU MANAGEMENT");
                UI.MenuOption(1, "View Full Menu");
                UI.MenuOption(2, "View by Category");
                UI.MenuOption(3, "Add Menu Item");
                UI.MenuOption(4, "Update Menu Item");
                UI.MenuOption(5, "Toggle Availability");
                UI.MenuOption(6, "Delete Item");
                UI.MenuOption(0, "Back");
                int choice = UI.PromptInt("Choice", 0, 6);
                switch (choice)
                {
                    case 1: ViewAll(); break;
                    case 2: ViewByCategory(); break;
                    case 3: AddItem(); break;
                    case 4: UpdateItem(); break;
                    case 5: ToggleAvailability(); break;
                    case 6: DeleteItem(); break;
                    case 0: return;
                }
            }
        }

        public static void ViewAll(bool pause = true)
        {
            UI.Header("FULL MENU");
            foreach (Category cat in Enum.GetValues(typeof(Category)))
            {
                var items = DataStore.Menu.Where(m => m.Category == cat).ToList();
                if (!items.Any()) continue;
                UI.SubHeader(cat.ToString().ToUpper());
                UI.Divider();
                UI.TableRow(
                    ("ID", 4, ConsoleColor.DarkGray),
                    ("Name", 22, ConsoleColor.White),
                    ("Description", 24, ConsoleColor.Gray),
                    ("Price", 8, ConsoleColor.Green),
                    ("Prep", 5, ConsoleColor.Cyan),
                    ("Status", 8, ConsoleColor.Yellow));
                UI.Divider();
                foreach (var item in items)
                {
                    ConsoleColor statusClr = item.IsAvailable ? ConsoleColor.Green : ConsoleColor.Red;
                    string status = item.IsAvailable ? "✔ Avail" : "✘ N/A  ";
                    UI.TableRow(
                        (item.Id.ToString(), 4, ConsoleColor.DarkGray),
                        (item.Name, 22, ConsoleColor.White),
                        (item.Description, 24, ConsoleColor.DarkGray),
                        ($"Rs.{item.Price}", 8, ConsoleColor.Green),
                        ($"{item.PrepTimeMins}m", 5, ConsoleColor.Cyan),
                        (status, 8, statusClr));
                }
            }
            if (pause) UI.PressAny();
        }

        static void ViewByCategory()
        {
            UI.Header("SELECT CATEGORY");
            var cats = Enum.GetValues(typeof(Category)).Cast<Category>().ToList();
            for (int i = 0; i < cats.Count; i++) UI.MenuOption(i + 1, cats[i].ToString());
            UI.MenuOption(0, "Back");
            int c = UI.PromptInt("Category", 0, cats.Count);
            if (c == 0) return;
            var selected = cats[c - 1];
            var items = DataStore.Menu.Where(m => m.Category == selected).ToList();
            UI.Header(selected.ToString());
            UI.Divider();
            foreach (var item in items)
                Console.WriteLine($"  [{item.Id:D2}] {item.Name,-22} Rs.{item.Price,6}  {(item.IsAvailable ? "✔" : "✘")}  {item.Description}");
            UI.PressAny();
        }

        static void AddItem()
        {
            if (!AuthModule.HasAccess(UserRole.Manager)) { UI.Error("Access denied."); UI.PressAny(); return; }
            UI.Header("ADD MENU ITEM");
            string name = UI.Prompt("Item Name");
            if (string.IsNullOrWhiteSpace(name)) return;

            UI.Info("Categories: " + string.Join(", ", Enum.GetNames(typeof(Category))));
            string catStr = UI.Prompt("Category");
            if (!Enum.TryParse(catStr, true, out Category cat)) { UI.Error("Invalid category."); UI.PressAny(); return; }

            decimal price = UI.PromptDecimal("Price (Rs)");
            string desc = UI.Prompt("Description");
            int prep = UI.PromptInt("Prep Time (mins)", 1, 120);
            int newId = DataStore.Menu.Max(m => m.Id) + 1;
            DataStore.Menu.Add(new MenuItem(newId, name, cat, price, desc, prep));
            UI.Success($"'{name}' added with ID {newId}.");
            UI.PressAny();
        }

        static void UpdateItem()
        {
            if (!AuthModule.HasAccess(UserRole.Manager)) { UI.Error("Access denied."); UI.PressAny(); return; }
            int id = UI.PromptInt("Enter Item ID to update");
            var item = DataStore.Menu.FirstOrDefault(m => m.Id == id);
            if (item == null) { UI.Error("Item not found."); UI.PressAny(); return; }

            UI.Info($"Updating: {item.Name}  (leave blank to keep current)");
            string name = UI.Prompt($"New Name [{item.Name}]");
            if (!string.IsNullOrWhiteSpace(name)) item.Name = name;
            string priceStr = UI.Prompt($"New Price [{item.Price}]");
            if (decimal.TryParse(priceStr, out decimal p)) item.Price = p;
            string desc = UI.Prompt($"New Description [{item.Description}]");
            if (!string.IsNullOrWhiteSpace(desc)) item.Description = desc;
            string prepStr = UI.Prompt($"New Prep Time [{item.PrepTimeMins}]");
            if (int.TryParse(prepStr, out int pt)) item.PrepTimeMins = pt;
            UI.Success("Item updated.");
            UI.PressAny();
        }

        static void ToggleAvailability()
        {
            if (!AuthModule.HasAccess(UserRole.Manager)) { UI.Error("Access denied."); UI.PressAny(); return; }
            int id = UI.PromptInt("Enter Item ID");
            var item = DataStore.Menu.FirstOrDefault(m => m.Id == id);
            if (item == null) { UI.Error("Item not found."); UI.PressAny(); return; }
            item.IsAvailable = !item.IsAvailable;
            UI.Success($"'{item.Name}' is now {(item.IsAvailable ? "Available" : "Unavailable") }.");
            UI.PressAny();
        }

        static void DeleteItem()
        {
            if (!AuthModule.HasAccess(UserRole.Admin)) { UI.Error("Admins only."); UI.PressAny(); return; }
            int id = UI.PromptInt("Enter Item ID to delete");
            var item = DataStore.Menu.FirstOrDefault(m => m.Id == id);
            if (item == null) { UI.Error("Item not found."); UI.PressAny(); return; }
            if (UI.Confirm($"Delete '{item.Name}'?"))
            {
                DataStore.Menu.Remove(item);
                UI.Success("Item deleted.");
            }
            UI.PressAny();
        }
    }
}
