namespace WarehouseApp;

public class WarehouseRepository : IWarehouseRepository
{
    private string connectionString = "Server=xxxxx;Database=xxxx;User Id=xxxx;Password=xxxx;";

    public async Task AddProduct(ProductWarehouse productWarehouse)
    {
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

                await command.ExecuteScalarAsync();

                transaction.Commit();
            }
        }
    }

    public async Task AddProductWithStoredProcedure(ProductWarehouse productWarehouse)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                SqlCommand command = new SqlCommand("AddProductToWarehouse", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Transaction = transaction;

                command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                command.Parameters.AddWithValue("@Price", productWarehouse.Price * productWarehouse.Amount);

                await command.ExecuteScalarAsync();

                transaction.Commit();
            }
        }
    }
}