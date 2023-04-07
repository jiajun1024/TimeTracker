using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker.Data;
using TimeTracker.Domain;
using TimeTracker.Models;

namespace TimeTracker.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [Authorize]
    [Route("/api/v{version:apiVersion}/clients")]
    public class ClientsController : Controller
    {
        private readonly TimeTrackerDbContext _dbContext;
        private readonly ILogger<UsersController> _logger;

        public ClientsController(TimeTrackerDbContext dbContext, ILogger<UsersController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientModel>> GetById(long id)
        {
            _logger.LogDebug($"Getting a client with id {id}");

            var client = await _dbContext.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return ClientModel.FromClient(client);
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<ClientModel>>> GetPage(int page = 1, int size = 5)
        {
            _logger.LogDebug($"Getting a page {page} of clients with page size {size}");

            var client = await _dbContext.Clients
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PagedList<ClientModel>
            {
                Items = client.Select(ClientModel.FromClient),
                Page = page,
                PageSize = size,
                TotalCount = await _dbContext.Clients.CountAsync()
            };
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogDebug($"Deleting client with id {id}");

            var client = await _dbContext.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            _dbContext.Clients.Remove(client);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<ClientModel>> Create(ClientInputModel model)
        {
            _logger.LogDebug($"Creating a new client with name {model.Name}");

            var client = new Domain.Client();
            model.MapTo(client);

            await _dbContext.Clients.AddAsync(client);
            await _dbContext.SaveChangesAsync();

            var resultModel = ClientModel.FromClient(client);

            return CreatedAtAction(nameof(GetById), "clients", new { id = client.Id, version = "2" }, resultModel);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ClientModel>> Update(long id, ClientInputModel model)
        {
            _logger.LogDebug($"Updating client with id {id}");

            var client = await _dbContext.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            model.MapTo(client);

            _dbContext.Clients.Update(client);
            await _dbContext.SaveChangesAsync();

            return ClientModel.FromClient(client);
        }
    }
}
