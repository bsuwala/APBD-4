public class WarehouseRepository : IWarehouseRepository
{
    private string connectionString = "Server=xxxxx;Database=xxxx;User Id=xxxx;Password=xxxx;";

    public async Task<IEnumerable<ProductWarehouse>> AddProduct(ProductWarehouse productWarehouse)
    {
        var products = new List<ProductWarehouse>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                SqlCommand command = new SqlCommand("INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, Amount, CreatedAt, Price) VALUES (@IdProduct, @IdWarehouse, @Amount, @CreatedAt, @Price); SELECT SCOPE_IDENTITY()", connection);
                command.Transaction = transaction;
                command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@Price", productWarehouse.Price * productWarehouse.Amount);

                var id = await command.ExecuteScalarAsync();

                transaction.Commit();

                products.Add(new ProductWarehouse
                {
                    IdProduct = productWarehouse.IdProduct,
                    IdWarehouse = productWarehouse.IdWarehouse,
                    Amount = productWarehouse.Amount,
                    CreatedAt = DateTime.Now,
                    Price = productWarehouse.Price * productWarehouse.Amount
                });
            }
        }

        return products;
    }
}