using System;

namespace RestaurantManagementSystem
{
    enum OrderStatus { Pending, InProgress, Ready, Served, Cancelled }
    enum TableStatus { Available, Occupied, Reserved }
    enum UserRole { Admin, Manager, Waiter, Cashier }
    enum PaymentMethod { Cash, Card, OnlineWallet }
    enum Category { Starter, MainCourse, Dessert, Beverage, Special }
    enum OrderPriority { Normal, Rush, LargeParty }
    enum TimeAlertLevel { OnTime, Approaching, Delayed }
}
