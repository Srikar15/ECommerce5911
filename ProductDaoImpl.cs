using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using EcommerceApp.Models;
using EcommerceApp.Dao.Interfaces;

namespace EcommerceApp.Dao.Impl
{
    public class ProductDaoImpl : IProductDao
    {
        private readonly string _connectionString;

        public ProductDaoImpl(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool CreateProduct(Product product)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "INSERT INTO Products (Name, Price, Description, StockQuantity) VALUES (@Name, @Price, @Description, @StockQuantity)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", product.Name);
                cmd.Parameters.AddWithValue("@Price", product.Price);
                cmd.Parameters.AddWithValue("@Description", product.Description);
                cmd.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public bool DeleteProduct(int productId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Products WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductId", productId);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();
                return rows > 0;
            }
        }

        public List<Product> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Products";
                SqlCommand cmd = new SqlCommand(query, conn);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        ProductId = (int)reader["ProductId"],
                        Name = reader["Name"].ToString(),
                        Price = (decimal)reader["Price"],
                        Description = reader["Description"].ToString(),
                        StockQuantity = (int)reader["StockQuantity"]
                    });
                }
            }

            return products;
        }

        public Product GetProductById(int productId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Products WHERE ProductId = @ProductId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ProductId", productId);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Product
                    {
                        ProductId = (int)reader["ProductId"],
                        Name = reader["Name"].ToString(),
                        Price = (decimal)reader["Price"],
                        Description = reader["Description"].ToString(),
                        StockQuantity = (int)reader["StockQuantity"]
                    };
                }

                return null;
            }
        }
    }
}
