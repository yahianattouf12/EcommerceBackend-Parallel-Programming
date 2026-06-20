using Microsoft.AspNetCore.Mvc;

namespace ECommerceBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoadSimulationController : ControllerBase
    {
        [HttpGet("simulate-task")]
        public IActionResult SimulateTask(int taskId)
        {
            var port = HttpContext.Connection.LocalPort;
            return Ok($"Task{taskId}: handled by node on port {port}");
        }
    }
}
