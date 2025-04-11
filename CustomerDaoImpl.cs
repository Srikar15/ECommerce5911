using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using EcommerceApp.Models;
using EcommerceApp.Dao.Interfaces;

namespace EcommerceApp.Dao.Impl
{
    public class CustomerDaoImpl : ICustomerDao
    {
        private readonly string _connectionString;

        public CustomerDaoImpl(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool CreateCustomer(Customer customer)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Customers (Name, Email, Password, RewardPoints) VALUES (@Name, @Email, @Password, @RewardPoints)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", customer.Name);
                cmd.Parameters.AddWithValue("@Email", customer.Email);
                cmd.Parameters.AddWithValue("@Password", customer.Password);
                cmd.Parameters.AddWithValue("@RewardPoints", customer.RewardPoints);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public bool DeleteCustomer(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Customers WHERE CustomerId = @CustomerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public Customer GetCustomerById(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Customers WHERE CustomerId = @CustomerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Customer
                    {
                        CustomerId = (int)reader["CustomerId"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        RewardPoints = (int)reader["RewardPoints"]
                    };
                }
                else
                {
                    return null;
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
    }
}
