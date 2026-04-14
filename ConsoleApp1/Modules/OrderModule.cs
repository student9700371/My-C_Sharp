using System;
using System.Linq;

namespace RestaurantManagementSystem
{
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
}
