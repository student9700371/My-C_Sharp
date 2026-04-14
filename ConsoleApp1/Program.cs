using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace RestaurantManagementSystem
{
    // ══════════════════════════════════════════════════════════
    //  ENUMS
    // ══════════════════════════════════════════════════════════
    enum OrderStatus { Pending, InProgress, Ready, Served, Cancelled }
    enum TableStatus { Available, Occupied, Reserved }
    enum UserRole { Admin, Manager, Waiter, Cashier }
    enum PaymentMethod { Cash, Card, OnlineWallet }
    enum Category { Starter, MainCourse, Dessert, Beverage, Special }
    enum OrderPriority { Normal, Rush, LargeParty }
    enum TimeAlertLevel { OnTime, Approaching, Delayed }

    // ══════════════════════════════════════════════════════════
    //  MODELS
    // ══════════════════════════════════════════════════════════
    class MenuItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Category Category { get; set; }
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; }
        public string Description { get; set; }
        public int PrepTimeMins { get; set; }

        public MenuItem(int id, string name, Category cat, decimal price, string desc = "", int prepTime = 10)
        {
            Id = id; Name = name; Category = cat; Price = price;
            Description = desc; PrepTimeMins = prepTime; IsAvailable = true;
        }
    }

    class OrderItem
    {
        public MenuItem Item { get; set; }
        public int Quantity { get; set; }
        public string SpecialNote { get; set; }
        public decimal Subtotal => Item.Price * Quantity;
        public OrderItem(MenuItem item, int qty, string note = "")
        { Item = item; Quantity = qty; SpecialNote = note; }
    }

    class TimeEstimate
    {
        public int EstimatedTotalMins { get; set; }
        public int RemainingMins { get; set; }
        public DateTime EstimatedReadyTime { get; set; }
        public TimeAlertLevel AlertLevel { get; set; }
        public List<string> ItemBreakdown { get; set; } = new List<string>();

        public void UpdateRemaining()
        {
            var remaining = EstimatedReadyTime - DateTime.Now;
            RemainingMins = Math.Max(0, (int)remaining.TotalMinutes);

            if (RemainingMins <= 0)
                AlertLevel = TimeAlertLevel.Delayed;
            else if (RemainingMins <= 5)
                AlertLevel = TimeAlertLevel.Approaching;
            else
                AlertLevel = TimeAlertLevel.OnTime;
        }
    }

    class Order
    {
        private static int _nextId = 1000;
        public int Id { get; }
        public int TableId { get; set; }
        public List<OrderItem> Items { get; } = new List<OrderItem>();
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime PlacedAt { get; } = DateTime.Now;
        public DateTime? ServedAt { get; set; }
        public string WaiterName { get; set; }
        public string CustomerName { get; set; }
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public decimal Tax => Subtotal * 0.13m;
        public decimal Discount { get; set; }
        public decimal Total => Subtotal + Tax - Discount;
        public bool IsPaid { get; set; }
        public PaymentMethod PayMethod { get; set; }

        // New properties for time estimation
        public TimeEstimate TimeEstimate { get; set; }
        public OrderPriority Priority { get; set; } = OrderPriority.Normal;
        public DateTime? EstimatedReadyTime { get; set; }
        public DateTime? LastStatusUpdate { get; set; }
        public int TotalPrepTime { get; set; }

        public Order(int tableId, string waiter, string customer = "Guest")
        {
            Id = _nextId++; TableId = tableId;
            WaiterName = waiter; CustomerName = customer;
        }
    }

    class Table
    {
        public int Id { get; set; }
        public int Seats { get; set; }
        public TableStatus Status { get; set; } = TableStatus.Available;
        public int? CurrentOrderId { get; set; }
        public string ReservedBy { get; set; }
        public DateTime? ReservationTime { get; set; }
        public Table(int id, int seats) { Id = id; Seats = seats; }
    }

    class Reservation
    {
        private static int _nextId = 1;
        public int Id { get; }
        public string GuestName { get; set; }
        public string Phone { get; set; }
        public int TableId { get; set; }
        public DateTime DateTime { get; set; }
        public int GuestCount { get; set; }
        public bool IsConfirmed { get; set; }
        public Reservation(string name, string phone, int table, DateTime dt, int guests)
        { Id = _nextId++; GuestName = name; Phone = phone; TableId = table; DateTime = dt; GuestCount = guests; IsConfirmed = true; }
    }

    class Staff
    {
        private static int _nextId = 100;
        public int Id { get; }
        public string Name { get; set; }
        public UserRole Role { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;
        public Staff(string name, UserRole role, string user, string pass, string phone, decimal salary)
        { Id = _nextId++; Name = name; Role = role; Username = user; Password = pass; Phone = phone; Salary = salary; }
    }

    class SaleRecord
    {
        public int OrderId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
    }

    // ══════════════════════════════════════════════════════════
    //  DATA STORE (in-memory)
    // ══════════════════════════════════════════════════════════
    static class DataStore
    {
        public static List<MenuItem> Menu = new List<MenuItem>();
        public static List<Table> Tables = new List<Table>();
        public static List<Order> Orders = new List<Order>();
        public static List<Reservation> Reservations = new List<Reservation>();
        public static List<Staff> StaffList = new List<Staff>();
        public static List<SaleRecord> Sales = new List<SaleRecord>();
        public static Staff CurrentUser = null;

        public static void Seed()
        {
            // Tables
            int[] seats = { 2, 2, 4, 4, 4, 6, 6, 8, 8, 10 };
            for (int i = 0; i < seats.Length; i++)
                Tables.Add(new Table(i + 1, seats[i]));

            // Menu
            var m = Menu;
            m.Add(new MenuItem(1, "Spring Rolls", Category.Starter, 250m, "Crispy veggie rolls", 8));
            m.Add(new MenuItem(2, "Chicken Soup", Category.Starter, 300m, "Spiced chicken broth", 10));
            m.Add(new MenuItem(3, "Garlic Bread", Category.Starter, 180m, "Toasted with herb butter", 5));
            m.Add(new MenuItem(4, "Caesar Salad", Category.Starter, 350m, "With croutons & parmesan", 7));
            m.Add(new MenuItem(5, "Grilled Chicken", Category.MainCourse, 850m, "With mashed potato", 20));
            m.Add(new MenuItem(6, "Beef Steak", Category.MainCourse, 1200m, "200g tenderloin", 25));
            m.Add(new MenuItem(7, "Butter Chicken", Category.MainCourse, 750m, "Creamy tomato gravy", 18));
            m.Add(new MenuItem(8, "Veggie Pasta", Category.MainCourse, 600m, "Arabiata sauce", 15));
            m.Add(new MenuItem(9, "Fish & Chips", Category.MainCourse, 900m, "Tartar sauce", 20));
            m.Add(new MenuItem(10, "Biryani", Category.MainCourse, 700m, "Basmati rice & spices", 22));
            m.Add(new MenuItem(11, "Chocolate Lava Cake", Category.Dessert, 450m, "Warm with ice cream", 12));
            m.Add(new MenuItem(12, "Cheesecake", Category.Dessert, 400m, "New York style", 5));
            m.Add(new MenuItem(13, "Gulab Jamun", Category.Dessert, 200m, "Soaked in sugar syrup", 5));
            m.Add(new MenuItem(14, "Mango Sorbet", Category.Dessert, 250m, "Seasonal", 5));
            m.Add(new MenuItem(15, "Soft Drink", Category.Beverage, 120m, "Pepsi/7up/Sprite", 2));
            m.Add(new MenuItem(16, "Fresh Juice", Category.Beverage, 250m, "Orange/Apple/Mango", 5));
            m.Add(new MenuItem(17, "Mineral Water", Category.Beverage, 80m, "500ml", 1));
            m.Add(new MenuItem(18, "Cappuccino", Category.Beverage, 300m, "Italian espresso blend", 4));
            m.Add(new MenuItem(19, "Chef Special Platter", Category.Special, 1500m, "For 2 persons", 30));
            m.Add(new MenuItem(20, "BBQ Combo", Category.Special, 1800m, "Mixed grill for 4", 35));

            // Staff
            StaffList.Add(new Staff("Admin User", UserRole.Admin, "admin", "admin123", "0300-0000001", 80000));
            StaffList.Add(new Staff("Sara Khan", UserRole.Manager, "sara", "manager1", "0300-0000002", 60000));
            StaffList.Add(new Staff("Ali Hassan", UserRole.Waiter, "ali", "waiter1", "0300-0000003", 35000));
            StaffList.Add(new Staff("Usman Malik", UserRole.Waiter, "usman", "waiter2", "0300-0000004", 35000));
            StaffList.Add(new Staff("Zara Ahmed", UserRole.Cashier, "zara", "cashier1", "0300-0000005", 40000));
        }
    }

    // ══════════════════════════════════════════════════════════
    //  UI HELPER
    // ══════════════════════════════════════════════════════════
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

    // ══════════════════════════════════════════════════════════
    //  TIME ESTIMATION MODULE
    // ══════════════════════════════════════════════════════════
    static class TimeEstimationModule
    {
        public static TimeEstimate CalculateOrderTime(Order order)
        {
            var estimate = new TimeEstimate();

            // Calculate total preparation time based on items
            int maxPrepTime = 0;
            int totalPrepTime = 0;

            foreach (var item in order.Items)
            {
                int itemTotalTime = item.Item.PrepTimeMins * item.Quantity;
                totalPrepTime += itemTotalTime;
                maxPrepTime = Math.Max(maxPrepTime, item.Item.PrepTimeMins);

                estimate.ItemBreakdown.Add($"{item.Quantity}x {item.Item.Name}: {item.Item.PrepTimeMins} mins each");
            }

            // Base time calculation (parallel cooking consideration)
            int estimatedTime = maxPrepTime + (totalPrepTime / 4);

            // Add priority modifiers
            if (order.Priority == OrderPriority.Rush)
                estimatedTime = (int)(estimatedTime * 0.7);
            else if (order.Priority == OrderPriority.LargeParty)
                estimatedTime = (int)(estimatedTime * 1.3);

            // Add buffer for kitchen load
            int pendingOrders = DataStore.Orders.Count(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.InProgress);
            if (pendingOrders > 3)
                estimatedTime += (pendingOrders - 3) * 2;

            estimate.EstimatedTotalMins = Math.Max(5, estimatedTime);
            estimate.EstimatedReadyTime = DateTime.Now.AddMinutes(estimate.EstimatedTotalMins);
            estimate.UpdateRemaining();

            return estimate;
        }

        public static void DisplayTimeEstimate(Order order)
        {
            if (order.TimeEstimate == null)
                order.TimeEstimate = CalculateOrderTime(order);
            else
                order.TimeEstimate.UpdateRemaining();

            Console.Clear();
            UI.Header($"TIME ESTIMATE - ORDER #{order.Id}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  Customer: {order.CustomerName}");
            Console.WriteLine($"  Table: {order.TableId} | Waiter: {order.WaiterName}");
            Console.ResetColor();

            UI.Divider();

            // Display item breakdown
            Console.WriteLine("\n  📋 ORDER ITEMS:");
            foreach (var breakdown in order.TimeEstimate.ItemBreakdown)
            {
                Console.WriteLine($"     • {breakdown}");
            }

            UI.Divider();

            // Display time information
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  ⏱  ESTIMATED PREPARATION TIME: {order.TimeEstimate.EstimatedTotalMins} MINUTES");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  🕐  READY BY: {order.TimeEstimate.EstimatedReadyTime:hh:mm tt}");
            Console.ResetColor();

            if (order.TimeEstimate.RemainingMins > 0)
            {
                Console.ForegroundColor = order.TimeEstimate.AlertLevel == TimeAlertLevel.Approaching ?
                    ConsoleColor.Yellow : ConsoleColor.White;
                Console.WriteLine($"  ⏳  REMAINING: {order.TimeEstimate.RemainingMins} minutes");
                Console.ResetColor();
            }

            UI.Divider();

            Console.Write("  Priority: ");
            if (order.Priority == OrderPriority.Rush)
            {
                UI.Badge("RUSH ORDER", ConsoleColor.Red);
                Console.WriteLine(" - Being expedited");
            }
            else if (order.Priority == OrderPriority.LargeParty)
            {
                UI.Badge("LARGE PARTY", ConsoleColor.Magenta);
                Console.WriteLine(" - Extra time needed");
            }
            else
            {
                UI.Badge("Normal", ConsoleColor.Green);
                Console.WriteLine();
            }

            UI.Divider();

            Console.WriteLine();
            if (order.Status == OrderStatus.Pending)
                UI.Info("Your order has been received and will start preparing soon.");
            else if (order.Status == OrderStatus.InProgress)
                UI.Info("Your order is being prepared in the kitchen.");
            else if (order.Status == OrderStatus.Ready)
                UI.Success("Your order is ready to be served!");
        }

        public static void ShowCustomerWaitingScreen(Order order)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════════════════════╗
  ║                                                              ║
  ║                     ORDER STATUS                             ║
  ║                                                              ║
  ╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            if (order.TimeEstimate == null)
                order.TimeEstimate = CalculateOrderTime(order);
            else
                order.TimeEstimate.UpdateRemaining();

            // Animated progress bar
            int totalMins = order.TimeEstimate.EstimatedTotalMins;
            int elapsedMins = totalMins - order.TimeEstimate.RemainingMins;
            int progress = (int)((double)elapsedMins / totalMins * 40);

            Console.WriteLine($"\n  Order #{order.Id} - {order.CustomerName}");
            Console.WriteLine($"  Status: {order.Status}");

            Console.Write("\n  Progress: [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('█', progress));
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(new string('░', 40 - progress));
            Console.ResetColor();
            Console.WriteLine($"] {progress * 2.5:F0}%");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  ⏰ Estimated Ready Time: {order.TimeEstimate.EstimatedReadyTime:hh:mm:ss tt}");
            Console.WriteLine($"  ⏳ Time Remaining: ~{order.TimeEstimate.RemainingMins} minutes");
            Console.ResetColor();

            Console.WriteLine("\n  📌 Next Steps:");
            if (order.Status == OrderStatus.Pending)
                Console.WriteLine("     • Kitchen will start preparing your order shortly");
            else if (order.Status == OrderStatus.InProgress)
                Console.WriteLine("     • Your order is being cooked");
            else if (order.Status == OrderStatus.Ready)
                Console.WriteLine("     • Your order is ready! A waiter will serve you shortly");

            Console.WriteLine("\n  💡 Tip: Ask your waiter for any special requests");
            UI.Divider();
        }
    }

    // ══════════════════════════════════════════════════════════
    //  AUTH MODULE
    // ══════════════════════════════════════════════════════════
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

    // ══════════════════════════════════════════════════════════
    //  MENU MODULE
    // ══════════════════════════════════════════════════════════
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
            UI.Success($"'{item.Name}' is now {(item.IsAvailable ? "Available" : "Unavailable")}.");
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

    // ══════════════════════════════════════════════════════════
    //  TABLE MODULE
    // ══════════════════════════════════════════════════════════
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

    // ══════════════════════════════════════════════════════════
    //  ORDER MODULE
    // ══════════════════════════════════════════════════════════
    static class OrderModule
    {
        public static void Show()
        {
            while (true)
            {
                UI.Header("ORDER MANAGEMENT");
                UI.MenuOption(1, "New Order");
                UI.MenuOption(2, "View Active Orders");
                UI.MenuOption(3, "Add Items to Order");
                UI.MenuOption(4, "Remove Item from Order");
                UI.MenuOption(5, "Update Order Status");
                UI.MenuOption(6, "Cancel Order");
                UI.MenuOption(7, "View All Orders");
                UI.MenuOption(8, "Show Order Time Estimate");
                UI.MenuOption(9, "Customer Waiting Screen");
                UI.MenuOption(10, "Set Order Priority");
                UI.MenuOption(11, "Active Orders with Time");
                UI.MenuOption(0, "Back");
                int c = UI.PromptInt("Choice", 0, 11);
                switch (c)
                {
                    case 1: NewOrder(); break;
                    case 2: ViewActiveOrders(); break;
                    case 3: AddItemsToOrder(); break;
                    case 4: RemoveItemFromOrder(); break;
                    case 5: UpdateOrderStatus(); break;
                    case 6: CancelOrder(); break;
                    case 7: ViewAllOrders(); break;
                    case 8: ShowOrderTimeEstimate(); break;
                    case 9: CustomerWaitingScreen(); break;
                    case 10: SetOrderPriority(); break;
                    case 11: ActiveOrdersWithTime(); break;
                    case 0: return;
                }
            }
        }

        static void NewOrder()
        {
            UI.Header("NEW ORDER");
            int tableId = UI.PromptInt("Table Number", 1, DataStore.Tables.Max(t => t.Id));
            var table = DataStore.Tables.FirstOrDefault(t => t.Id == tableId);
            if (table == null) { UI.Error("Table not found."); UI.PressAny(); return; }

            string waiter = DataStore.CurrentUser?.Name ?? UI.Prompt("Waiter Name");
            string customer = UI.Prompt("Customer Name (leave blank for Guest)");
            if (string.IsNullOrWhiteSpace(customer)) customer = "Guest";

            var order = new Order(tableId, waiter, customer);

            // Add items
            while (true)
            {
                MenuModule.ViewAll(false);
                UI.Info("Enter item ID to add or 0 to finish");
                int itemId = UI.PromptInt("Item ID", 0, int.MaxValue);
                if (itemId == 0) break;
                var menuItem = DataStore.Menu.FirstOrDefault(m => m.Id == itemId && m.IsAvailable);
                if (menuItem == null) { UI.Error("Item not found or unavailable."); continue; }
                int qty = UI.PromptInt("Quantity", 1, 100);
                order.Items.Add(new OrderItem(menuItem, qty));
                UI.Success($"Added {qty} x {menuItem.Name}");
            }

            order.TimeEstimate = TimeEstimationModule.CalculateOrderTime(order);
            DataStore.Orders.Add(order);
            table.CurrentOrderId = order.Id;
            table.Status = TableStatus.Occupied;
            ShowOrderConfirmation(order);
        }

        static void ViewActiveOrders()
        {
            UI.Header("ACTIVE ORDERS");
            var active = DataStore.Orders.Where(o => o.Status != OrderStatus.Served && o.Status != OrderStatus.Cancelled).ToList();
            if (!active.Any()) { UI.Info("No active orders."); UI.PressAny(); return; }
            foreach (var o in active)
            {
                Console.WriteLine($"  #{o.Id} Table:{o.TableId} Customer:{o.CustomerName} Items:{o.Items.Count} Status:{o.Status}");
            }
            UI.PressAny();
        }

        static void AddItemsToOrder()
        {
            int id = UI.PromptInt("Order ID");
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) { UI.Error("Order not found."); UI.PressAny(); return; }
            while (true)
            {
                MenuModule.ViewAll(false);
                int itemId = UI.PromptInt("Item ID (0 to finish)", 0, int.MaxValue);
                if (itemId == 0) break;
                var menuItem = DataStore.Menu.FirstOrDefault(m => m.Id == itemId && m.IsAvailable);
                if (menuItem == null) { UI.Error("Item not found or unavailable."); continue; }
                int qty = UI.PromptInt("Quantity", 1, 100);
                order.Items.Add(new OrderItem(menuItem, qty));
                UI.Success($"Added {qty} x {menuItem.Name} to order #{order.Id}");
            }
            order.TimeEstimate = TimeEstimationModule.CalculateOrderTime(order);
            // Show confirmation after items are added
            ShowOrderConfirmation(order);
        }

        static void RemoveItemFromOrder()
        {
            int id = UI.PromptInt("Order ID");
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) { UI.Error("Order not found."); UI.PressAny(); return; }
            if (!order.Items.Any()) { UI.Info("No items to remove."); UI.PressAny(); return; }
            for (int i = 0; i < order.Items.Count; i++)
                Console.WriteLine($"  [{i+1}] {order.Items[i].Quantity}x {order.Items[i].Item.Name}");
            int idx = UI.PromptInt("Item number to remove", 1, order.Items.Count) - 1;
            var removed = order.Items[idx];
            order.Items.RemoveAt(idx);
            UI.Success($"Removed {removed.Quantity}x {removed.Item.Name}");
            order.TimeEstimate = TimeEstimationModule.CalculateOrderTime(order);
            UI.PressAny();
        }

        static void UpdateOrderStatus()
        {
            int id = UI.PromptInt("Order ID");
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) { UI.Error("Order not found."); UI.PressAny(); return; }
            UI.Info("Statuses: Pending, InProgress, Ready, Served, Cancelled");
            string s = UI.Prompt("New Status");
            if (!Enum.TryParse<OrderStatus>(s, true, out var st)) { UI.Error("Invalid status."); UI.PressAny(); return; }
            order.Status = st;
            order.LastStatusUpdate = DateTime.Now;
            if (st == OrderStatus.Served) { order.ServedAt = DateTime.Now; var t = DataStore.Tables.FirstOrDefault(x => x.Id == order.TableId); if (t != null) { t.Status = TableStatus.Available; t.CurrentOrderId = null; } }
            if (st == OrderStatus.Cancelled) { var t = DataStore.Tables.FirstOrDefault(x => x.Id == order.TableId); if (t != null) { t.Status = TableStatus.Available; t.CurrentOrderId = null; } }
            UI.Success("Order status updated.");
            UI.PressAny();
        }

        static void CancelOrder()
        {
            int id = UI.PromptInt("Order ID to cancel");
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) { UI.Error("Order not found."); UI.PressAny(); return; }
            order.Status = OrderStatus.Cancelled;
            var t = DataStore.Tables.FirstOrDefault(x => x.Id == order.TableId);
            if (t != null) { t.Status = TableStatus.Available; t.CurrentOrderId = null; }
            UI.Success($"Order #{order.Id} cancelled.");
            UI.PressAny();
        }

        static void ViewAllOrders()
        {
            UI.Header("ALL ORDERS");
            foreach (var o in DataStore.Orders)
            {
                Console.WriteLine($"  #{o.Id} Table:{o.TableId} Customer:{o.CustomerName} Items:{o.Items.Count} Status:{o.Status} Total:Rs.{o.Total}");
            }
            UI.PressAny();
        }

        static void ShowOrderTimeEstimate()
        {
            int id = UI.PromptInt("Order ID");
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) { UI.Error("Order not found."); UI.PressAny(); return; }
            TimeEstimationModule.DisplayTimeEstimate(order);
            UI.PressAny();
        }

        static void CustomerWaitingScreen()
        {
            int id = UI.PromptInt("Order ID");
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) { UI.Error("Order not found."); UI.PressAny(); return; }
            TimeEstimationModule.ShowCustomerWaitingScreen(order);
            UI.PressAny();
        }

        static void SetOrderPriority()
        {
            int id = UI.PromptInt("Order ID");
            var order = DataStore.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null) { UI.Error("Order not found."); UI.PressAny(); return; }
            UI.Info("Priorities: Normal, Rush, LargeParty");
            string p = UI.Prompt("Priority");
            if (!Enum.TryParse<OrderPriority>(p, true, out var pr)) { UI.Error("Invalid priority."); UI.PressAny(); return; }
            order.Priority = pr;
            order.TimeEstimate = TimeEstimationModule.CalculateOrderTime(order);
            UI.Success("Priority set.");
            UI.PressAny();
        }

        static void ActiveOrdersWithTime()
        {
            UI.Header("ACTIVE ORDERS WITH TIME");
            var active = DataStore.Orders.Where(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.InProgress || o.Status == OrderStatus.Ready).ToList();
            if (!active.Any()) { UI.Info("No active orders."); UI.PressAny(); return; }
            foreach (var o in active)
            {
                if (o.TimeEstimate == null) o.TimeEstimate = TimeEstimationModule.CalculateOrderTime(o);
                else o.TimeEstimate.UpdateRemaining();
                Console.WriteLine($"  #{o.Id} Table:{o.TableId} Status:{o.Status} Remaining:{o.TimeEstimate.RemainingMins} mins");
            }
            UI.PressAny();
        }

        static void ShowOrderConfirmation(Order order)
        {
            Console.Clear();
            UI.Header($"ORDER CONFIRMATION - #{order.Id}");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n  Customer: {order.CustomerName}");
            Console.WriteLine($"  Table: {order.TableId} | Waiter: {order.WaiterName}");
            Console.ResetColor();

            UI.Divider();

            Console.WriteLine("\n  Items:");
            foreach (var it in order.Items)
            {
                Console.WriteLine($"     • {it.Quantity} x {it.Item.Name}  Rs.{it.Item.Price} each   Subtotal: Rs.{it.Subtotal}");
            }

            UI.Divider();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n  Subtotal : Rs.{order.Subtotal}");
            Console.WriteLine($"  Tax (13%): Rs.{order.Tax}");
            Console.WriteLine($"  Discount : Rs.{order.Discount}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  TOTAL    : Rs.{order.Total}");
            Console.ResetColor();

            UI.Divider();

            if (order.TimeEstimate != null)
                Console.WriteLine($"  ⏱ Estimated ready in ~{order.TimeEstimate.EstimatedTotalMins} minutes ({order.TimeEstimate.EstimatedReadyTime:hh:mm tt})");
            UI.Success("Your order has been confirmed and sent to the kitchen.");
            UI.Info("Please wait — a waiter will bring any updates to your table.");
            UI.PressAny();
        }
    }

    // ══════════════════════════════════════════════════════════
    //  PROGRAM ENTRY
    // ══════════════════════════════════════════════════════════
    class Program
    {
        static void Main(string[] args)
        {
            DataStore.Seed();
            AuthModule.Login();

            while (true)
            {
                UI.Header("MAIN MENU");
                UI.MenuOption(1, "Menu Management");
                UI.MenuOption(2, "Table Management");
                UI.MenuOption(3, "Order Management");
                UI.MenuOption(0, "Exit");
                int c = UI.PromptInt("Choice", 0, 3);
                switch (c)
                {
                    case 1: MenuModule.Show(); break;
                    case 2: TableModule.Show(); break;
                    case 3: OrderModule.Show(); break;
                    case 0: Environment.Exit(0); break;
                }
            }
        }
    }

}
