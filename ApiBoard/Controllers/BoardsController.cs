using ApiBoard.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiBoard.Controllers;

[Route("[controller]")]
[ApiController]
public class BoardsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] BoardCloudStorageService service)
    {
        var boards = await service.GetAllBoards();
        return Ok(boards);
    }
}
