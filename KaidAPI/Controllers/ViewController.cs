namespace KaidAPI.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ViewController : ControllerBase
    {
        private readonly ILogger<ViewController> _logger;

        public ViewController(ILogger<ViewController> logger)
        {
            _logger = logger;
        }

    }
}
