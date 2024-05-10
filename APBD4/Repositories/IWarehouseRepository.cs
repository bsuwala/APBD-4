namespace WarehouseApp;

public interface IWarehouseRepository
{
    Task AddProduct(ProductWarehouse productWarehouse);
    Task AddProductWithStoredProcedure(ProductWarehouse productWarehouse);
}