using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AbcWarehouse.Plugin.Misc.SearchSpring.Services;
using System;

namespace AbcWarehouse.Plugin.Misc.SearchSpring.Controllers
{
    public class SearchSpringController : Controller
    {
        private readonly ISearchSpringService _searchSpringService;

        public SearchSpringController(ISearchSpringService searchSpringService)
        {
            _searchSpringService = searchSpringService;
        }

        [HttpGet]
        public async Task<IActionResult> Query(string term)
        {
            var results = await _searchSpringService.SearchAsync(term);
            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> Results(string term)
        {
            Console.WriteLine($"[Controller] Received term: {term}");

            if (string.IsNullOrWhiteSpace(term))
                return BadRequest("Search term cannot be empty.");

            var results = await _searchSpringService.SearchAsync(term);
            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Results.cshtml", results);
        }
    }
}
