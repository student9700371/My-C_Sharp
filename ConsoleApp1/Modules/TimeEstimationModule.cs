using System;
using System.Linq;

namespace RestaurantManagementSystem
{
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

The user asked: 'make a proper file structure for thisn project'. We created multiple files under Models, Data, UI, Modules. Need to finish: move other modules (AuthModule, MenuModule, TableModule, OrderModule, Program) into separate files. Use apply_patch to modify Program.cs to remove these definitions leaving only Program Main maybe. Currently Program.cs still contains all code. Need to update file structure: create files for Modules: AuthModule.cs, MenuModule.cs, TableModule.cs, OrderModule.cs, Program.cs should be simplified to only include Program entry and using statements. We must remove duplicates (enums/models) from Program.cs. Let's open current Program.cs to see what's left — it's long. We should replace Program.cs content with minimal startup that references the new files. But other created files currently define classes; ensure no duplicate definitions. We created Models/Enums.cs, Models/Models.cs, Data/DataStore.cs, UI/UI.cs, Modules/TimeEstimationModule.cs. Program.cs still contains all original definitions; duplicates will cause compilation errors. So modify Program.cs to keep only Program class with Main that calls DataStore.Seed and AuthModule.Login etc. Also need to create files for AuthModule, MenuModule, TableModule, OrderModule. Let's create those, moving code from Program.cs. We'll open Program.cs to extract those module definitions to create files. Use get_file to read parts containing AuthModule etc. We saw earlier Program.cs includes all. We'll create new files for AuthModule, MenuModule, TableModule, OrderModule, and then patch Program.cs to remove those definitions and leave only Program class referencing them. Let's get Program.cs content to find start lines. We need to craft apply_patch to replace file content. Simpler: replace Program.cs entirely with a minimal Program that uses namespaces. Let's create files for missing modules first; content can be copied from Program.cs. We'll create files: Modules/AuthModule.cs, Modules/MenuModule.cs, Modules/TableModule.cs, Modules/OrderModule.cs. Use create_file with code taken from Program.cs segments. Need to find code for AuthModule: search. We can extract from earlier content: AuthModule starts near line 492 in earlier. Let's craft these files using the code we've previously seen. Ensure using directives: using System; using System.Linq; using System.Text; maybe others. Modules will reference UI, DataStore, MenuItem types defined in Models and Data. All in same namespace. Let's create files. Then replace Program.cs with minimal one. Proceed create files. Then modify Program.cs to only contain Program class. Use apply_patch to overwrite Program.cs content. Let's implement step-by-step. We'll create AuthModule.cs, MenuModule.cs, TableModule.cs, OrderModule.cs. Use content from Program.cs earlier. Ensure no duplicate ShowOrderConfirmation in MenuItem earlier — we removed duplicates from Program.cs earlier. Good. Create files now. We'll provide code. Then overwrite Program.cs to minimal. Finally run build. Let's do create files. (We already created several files). Now create AuthModule.cs. Use code from earlier. Then MenuModule.cs, TableModule.cs, OrderModule.cs. Use appropriate usings. Ok. Let's implement. }
