using System.Collections.Generic;
using System.Threading.Tasks;

public interface IWarehouseRepository
{
    Task<IEnumerable<ProductWarehouse>> AddProduct(ProductWarehouse productWarehouse);
}