using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using EcommerceApp.Models;
using EcommerceApp.Dao.Interfaces;

namespace EcommerceApp.Dao.Impl
{
    public class OrderDaoImpl : IOrderDao
    {
        private readonly string _connectionString;

        public OrderDaoImpl(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool PlaceOrder(Customer customer, List<(Product, int)> items, string shippingAddress, int pointsToRedeem)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    decimal total = 0;
                    foreach (var (product, quantity) in items)
                    {
                        total += product.Price * quantity;
                    }

                    if (pointsToRedeem > customer.RewardPoints)
                        pointsToRedeem = customer.RewardPoints;

                    total -= pointsToRedeem;

                    // Insert Order
                    string orderQuery = @"INSERT INTO Orders (CustomerId, TotalPrice, ShippingAddress, IsShipped, PointsRedeemed)
                                          OUTPUT INSERTED.OrderId
                                          VALUES (@CustomerId, @TotalPrice, @ShippingAddress, 0, @PointsRedeemed)";
                    SqlCommand orderCmd = new SqlCommand(orderQuery, conn, transaction);
                    orderCmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    orderCmd.Parameters.AddWithValue("@TotalPrice", total);
                    orderCmd.Parameters.AddWithValue("@ShippingAddress", shippingAddress);
                    orderCmd.Parameters.AddWithValue("@PointsRedeemed", pointsToRedeem);

                    int orderId = (int)orderCmd.ExecuteScalar();

                    // Insert Order Items
                    foreach (var (product, quantity) in items)
                    {
                        string itemQuery = @"INSERT INTO OrderItems (OrderId, ProductId, Quantity, Price)
                                             VALUES (@OrderId, @ProductId, @Quantity, @Price)";
                        SqlCommand itemCmd = new SqlCommand(itemQuery, conn, transaction);
                        itemCmd.Parameters.AddWithValue("@OrderId", orderId);
                        itemCmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                        itemCmd.Parameters.AddWithValue("@Quantity", quantity);
                        itemCmd.Parameters.AddWithValue("@Price", product.Price);
                        itemCmd.ExecuteNonQuery();
                    }

                    // Update reward points
                    int earnedPoints = (int)(total / 100); // 1 point per ₹100 spent
                    int newPoints = customer.RewardPoints - pointsToRedeem + earnedPoints;

                    SqlCommand rewardCmd = new SqlCommand("UPDATE Customers SET RewardPoints = @Points WHERE CustomerId = @CustomerId", conn, transaction);
                    rewardCmd.Parameters.AddWithValue("@Points", newPoints);
                    rewardCmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    rewardCmd.ExecuteNonQuery();

                    // Clear cart
                    SqlCommand clearCart = new SqlCommand("DELETE FROM Cart WHERE CustomerId = @CustomerId", conn, transaction);
                    clearCart.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                    clearCart.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    return false;
                }
            }
        }

        public List<Order> GetOrdersByCustomerId(int customerId)
        {
            List<Order> orders = new List<Order>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Orders WHERE CustomerId = @CustomerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        OrderId = (int)reader["OrderId"],
                        CustomerId = (int)reader["CustomerId"],
                        OrderDate = (DateTime)reader["OrderDate"],
                        TotalPrice = (decimal)reader["TotalPrice"],
                        ShippingAddress = reader["ShippingAddress"].ToString(),
                        IsShipped = (bool)reader["IsShipped"],
                        PointsRedeemed = (int)reader["PointsRedeemed"]
                    });
                }
            }

            return orders;
        }

        public bool MarkAsShipped(int orderId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "UPDATE Orders SET IsShipped = 1 WHERE OrderId = @OrderId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OrderId", orderId);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }
    }
}
