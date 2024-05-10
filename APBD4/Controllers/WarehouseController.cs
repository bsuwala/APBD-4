using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WarehouseApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private string connectionString = "Server=xxxxx;Database=xxxx;User Id=xxxx;Password=xxxx;";
        
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ProductWarehouse productWarehouse)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1.
                        SqlCommand command = new SqlCommand("SELECT * FROM Product WHERE IdProduct = @IdProduct; SELECT * FROM Warehouse WHERE IdWarehouse = @IdWarehouse", connection);
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                        command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows || productWarehouse.Amount <= 0)
                            {
                                throw new Exception("Produkt, magazyn nie istnieje lub ilość jest mniejsza lub równa 0");
                            }

                            await reader.NextResultAsync();

                            if (!reader.HasRows)
                            {
                                throw new Exception("Produkt, magazyn nie istnieje lub ilość jest mniejsza lub równa 0");
                            }
                        }

                        // 2.
                        command = new SqlCommand("SELECT * FROM Order WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt", connection);
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                        command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                        command.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (!reader.HasRows)
                            {
                                throw new Exception("Nie istnieje odpowiednie zamówienie");
                            }
                        }

                        // 3.
                        command = new SqlCommand("SELECT * FROM Product_Warehouse WHERE IdOrder = @IdOrder", connection);
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@IdOrder", productWarehouse.IdOrder);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                throw new Exception("Zamówienie zostało już zrealizowane");
                            }
                        }

                        // 4.
                        command = new SqlCommand("UPDATE Order SET FullfilledAt = @FullfilledAt WHERE IdOrder = @IdOrder", connection);
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@FullfilledAt", DateTime.Now);
                        command.Parameters.AddWithValue("@IdOrder", productWarehouse.IdOrder);

                        await command.ExecuteNonQueryAsync();

                        // 5.
                        command = new SqlCommand("INSERT INTO Product_Warehouse (IdProduct, IdWarehouse, Amount, CreatedAt, Price) VALUES (@IdProduct, @IdWarehouse, @Amount, @CreatedAt, @Price); SELECT SCOPE_IDENTITY()", connection);
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                        command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                        command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        command.Parameters.AddWithValue("@Price", productWarehouse.Price * productWarehouse.Amount);

                        var id = await command.ExecuteScalarAsync();

                        // 6.
                        transaction.Commit();
                        return Ok(new { Id = id });
                    }
                    catch (Exception ex)
                    { 
                        transaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [HttpPost("AddProductWithStoredProcedure")]
        public async Task<ActionResult> AddProductWithStoredProcedure([FromBody] ProductWarehouse productWarehouse)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        
                        SqlCommand command = new SqlCommand("AddProductToWarehouse", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Transaction = transaction;

                       
                        command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
                        command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
                        command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
                        command.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);
                        command.Parameters.AddWithValue("@Price", productWarehouse.Price * productWarehouse.Amount);

                        
                        var id = await command.ExecuteScalarAsync();

                        transaction.Commit();
                        return Ok(new { Id = id });
                    }
                    catch (Exception ex)
                    { 
                        transaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }
    }

    public class ProductWarehouse
    {
        public int IdProduct { get; set; }
        public int IdWarehouse { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Price { get; set; }
        public int IdOrder { get; set; }
    }
}
