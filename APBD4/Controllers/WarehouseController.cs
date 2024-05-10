using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseController(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }

    [HttpPost]
    public async Task<IEnumerable<ProductWarehouse>> Post([FromBody] ProductWarehouse productWarehouse)
    {
        return await _warehouseRepository.AddProduct(productWarehouse);
    }

    [HttpGet("AddProductWithDetails")]
    public async Task<IActionResult> AddProductWithDetails([FromQuery] ProductDetails productDetails)
    {
        try
        {
            var productWarehouse = new ProductWarehouse
            {
                IdProduct = productDetails.IdProduct,
                IdWarehouse = productDetails.IdWarehouse,
                Amount = productDetails.Amount,
                CreatedAt = productDetails.CreatedAt
            };

            await _warehouseRepository.AddProduct(productWarehouse);
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error");
        }
    }
}