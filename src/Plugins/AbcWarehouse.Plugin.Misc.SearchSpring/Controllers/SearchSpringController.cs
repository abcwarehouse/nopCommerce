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

        public async Task<IActionResult> Results(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search term cannot be empty.");

            var results = await _searchSpringService.SearchAsync(q);
            return View("~/Plugins/AbcWarehouse.Plugin.Misc.SearchSpring/Views/Results.cshtml", results);
        }
    }
}
