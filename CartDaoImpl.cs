using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using EcommerceApp.Models;
using EcommerceApp.Dao.Interfaces;

namespace EcommerceApp.Dao.Impl
{
    public class CartDaoImpl : ICartDao
    {
        private readonly string _connectionString;

        public CartDaoImpl(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool AddToCart(Customer customer, Product product, int quantity)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Cart (CustomerId, ProductId, Quantity) VALUES (@CustomerId, @ProductId, @Quantity)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                cmd.Parameters.AddWithValue("@ProductId", product.ProductId);
                cmd.Parameters.AddWithValue("@Quantity", quantity);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public bool RemoveFromCart(Customer customer, Product product)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Cart WHERE CustomerId = @CustomerId AND ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
                cmd.Parameters.AddWithValue("@ProductId", product.ProductId);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public List<Cart> GetCartItems(Customer customer)
        {
            List<Cart> cartItems = new List<Cart>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Cart WHERE CustomerId = @CustomerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cartItems.Add(new Cart
                    {
                        CartId = (int)reader["CartId"],
                        CustomerId = (int)reader["CustomerId"],
                        ProductId = (int)reader["ProductId"],
                        Quantity = (int)reader["Quantity"]
                    });
                }
            }

            return cartItems;
        }

        public bool ClearCart(Customer customer)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Cart WHERE CustomerId = @CustomerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }
    }
}
