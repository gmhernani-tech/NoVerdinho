using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NoVerdinho.Models;
using NoVerdinho.Models.Login;

namespace NoVerdinho.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login()
        {
            string email = Request.Form["email"];
            string senha = Request.Form["password"];

            TratamentoLogin tratamento = new TratamentoLogin();
            TratamentoLogin Model = tratamento.ValidarLogin(email, senha);

            return View("TelaUsuario/TelaUsuario", Model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
