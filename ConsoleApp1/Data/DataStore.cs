using System.Collections.Generic;
using System.Linq;

namespace RestaurantManagementSystem
{
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
}
