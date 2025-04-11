using System;
using System.Collections.Generic;
using EcommerceApp.Models;
using EcommerceApp.Dao.Impl;
using EcommerceApp.Exceptions;

namespace EcommerceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Data Source=localhost;Initial Catalog=EcommerceDB;Integrated Security=True"; // change this to your DB

            var customerDao = new CustomerDaoImpl(connectionString);
            var productDao = new ProductDaoImpl(connectionString);
            var cartDao = new CartDaoImpl(connectionString);
            var orderDao = new OrderDaoImpl(connectionString);

            while (true)
            {
                Console.WriteLine("\n======= E-COMMERCE MENU =======");
                Console.WriteLine("1. Register Customer");
                Console.WriteLine("2. Create Product");
                Console.WriteLine("3. Delete Product");
                Console.WriteLine("4. Add to Cart");
                Console.WriteLine("5. View Cart");
                Console.WriteLine("6. Place Order");
                Console.WriteLine("7. View Customer Orders");
                Console.WriteLine("8. Mark Order As Shipped");
                Console.WriteLine("0. Exit");
                Console.Write("Enter choice: ");

                string choice = Console.ReadLine();
                try
                {
                    switch (choice)
                    {
                        case "1":
                            Console.Write("Name: ");
                            string name = Console.ReadLine();
                            Console.Write("Email: ");
                            string email = Console.ReadLine();
                            Console.Write("Password: ");
                            string password = Console.ReadLine();

                            var customer = new Customer { Name = name, Email = email, Password = password, RewardPoints = 0 };
                            customerDao.CreateCustomer(customer);
                            Console.WriteLine("Customer registered successfully.");
                            break;

                        case "2":
                            Console.Write("Product Name: ");
                            string pname = Console.ReadLine();
                            Console.Write("Price: ");
                            decimal price = decimal.Parse(Console.ReadLine());
                            Console.Write("Description: ");
                            string desc = Console.ReadLine();
                            Console.Write("Stock Quantity: ");
                            int qty = int.Parse(Console.ReadLine());

                            var product = new Product { Name = pname, Price = price, Description = desc, StockQuantity = qty };
                            productDao.CreateProduct(product);
                            Console.WriteLine("Product added.");
                            break;

                        case "3":
                            Console.Write("Enter Product ID to delete: ");
                            int deleteId = int.Parse(Console.ReadLine());
                            productDao.DeleteProduct(deleteId);
                            Console.WriteLine("Product deleted.");
                            break;

                        case "4":
                            Console.Write("Customer ID: ");
                            int custId = int.Parse(Console.ReadLine());
                            var cust = customerDao.GetCustomerById(custId) ?? throw new CustomerNotFoundException();

                            Console.Write("Product ID: ");
                            int prodId = int.Parse(Console.ReadLine());
                            var prod = productDao.GetProductById(prodId) ?? throw new ProductNotFoundException();

                            Console.Write("Quantity: ");
                            int addQty = int.Parse(Console.ReadLine());

                            cartDao.AddToCart(cust, prod, addQty);
                            Console.WriteLine("Added to cart.");
                            break;

                        case "5":
                            Console.Write("Customer ID: ");
                            int vcustId = int.Parse(Console.ReadLine());
                            var vcust = customerDao.GetCustomerById(vcustId) ?? throw new CustomerNotFoundException();

                            var cartItems = cartDao.GetCartItems(vcust);
                            Console.WriteLine("--- Your Cart ---");
                            foreach (var item in cartItems)
                            {
                                var itemProd = productDao.GetProductById(item.ProductId);
                                Console.WriteLine($"{itemProd.Name} - Qty: {item.Quantity}");
                            }
                            break;

                        case "6":
                            Console.Write("Customer ID: ");
                            int orderCustId = int.Parse(Console.ReadLine());
                            var orderCust = customerDao.GetCustomerById(orderCustId) ?? throw new CustomerNotFoundException();

                            var items = new List<(Product, int)>();
                            var cart = cartDao.GetCartItems(orderCust);
                            foreach (var item in cart)
                            {
                                var prodItem = productDao.GetProductById(item.ProductId) ?? throw new ProductNotFoundException();
                                items.Add((prodItem, item.Quantity));
                            }

                            Console.Write("Shipping Address: ");
                            string address = Console.ReadLine();

                            Console.Write($"You have {orderCust.RewardPoints} points. Redeem points? ");
                            int redeem = int.Parse(Console.ReadLine());

                            bool ordered = orderDao.PlaceOrder(orderCust, items, address, redeem);
                            Console.WriteLine(ordered ? "Order placed!" : "Failed to place order.");
                            break;

                        case "7":
                            Console.Write("Customer ID: ");
                            int ocustId = int.Parse(Console.ReadLine());
                            var orders = orderDao.GetOrdersByCustomerId(ocustId);
                            foreach (var o in orders)
                            {
                                Console.WriteLine($"Order #{o.OrderId} | ₹{o.TotalPrice} | Redeemed: {o.PointsRedeemed} | Shipped: {(o.IsShipped ? "Yes" : "No")}");
                            }
                            break;

                        case "8":
                            Console.Write("Order ID to mark as shipped: ");
                            int shipId = int.Parse(Console.ReadLine());
                            bool shipped = orderDao.MarkAsShipped(shipId);
                            Console.WriteLine(shipped ? "Marked as shipped." : "Failed.");
                            break;

                        case "0":
                            return;

                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                catch (CustomerNotFoundException e) { Console.WriteLine(e.Message); }
                catch (ProductNotFoundException e) { Console.WriteLine(e.Message); }
                catch (OrderNotFoundException e) { Console.WriteLine(e.Message); }
                catch (Exception e) { Console.WriteLine("Error: " + e.Message); }
            }
        }
    }
}
