namespace WarehouseApp;

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
    public async Task<IActionResult> Post([FromBody] ProductWarehouse productWarehouse)
    {
        await _warehouseRepository.AddProduct(productWarehouse);
        return NoContent();
    }

    [HttpGet("AddProductWithStoredProcedure")]
    public async Task<IActionResult> AddProductWithStoredProcedure([FromQuery] ProductDetails productDetails)
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