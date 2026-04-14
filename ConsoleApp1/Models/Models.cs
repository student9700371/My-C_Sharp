using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantManagementSystem
{
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
}
