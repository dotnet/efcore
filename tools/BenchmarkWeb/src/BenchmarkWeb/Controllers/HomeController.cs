using BenchmarkWeb.Models;
using Microsoft.AspNet.Mvc;

namespace BenchmarkWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly BenchmarkRepository _repository;

        public HomeController(BenchmarkRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            return View(_repository.GetLatestResults());
        }

        public IActionResult History(string testClass, string testMethod)
        {
            return View(_repository.GetTestHistory(testClass, testMethod));
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
