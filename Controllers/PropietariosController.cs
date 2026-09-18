using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaGrupoNN.Controllers
{
    [Authorize]
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario _repositorio;

        public PropietariosController(IRepositorioPropietario repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: Propietarios
        [Authorize]
        public IActionResult Index(int pagina = 1, int tamanio = 10)
        {
            var propietarios = _repositorio.ObtenerTodos(pagina, tamanio);
            
            int totalRegistros = _repositorio.ObtenerTotal();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanio);

            ViewBag.PaginaActual = pagina;
            ViewBag.TamanioPagina = tamanio;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(propietarios);
        }

        // GET: Propietarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Propietarios/Create
        [HttpPost]
        public IActionResult Create(Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                _repositorio.Alta(propietario);
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: Propietarios/Edit/id
        public IActionResult Edit(int id)
        {
            var propietario = _repositorio.ObtenerPorId(id);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // POST: Propietarios/Edit/id
        [HttpPost]
        public IActionResult Edit(int id, Propietario propietario)
        {
            if (ModelState.IsValid)
            {
                propietario.Id = id;
                _repositorio.Modificacion(propietario);
                return RedirectToAction(nameof(Index));
            }
            return View(propietario);
        }

        // GET: Propietarios/Delete/id
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var propietario = _repositorio.ObtenerPorId(id);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // POST: Propietarios/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repositorio.Baja(id);
                TempData["Mensaje"] = "El propietario fue dado de baja exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al intentar dar de baja: " + ex.Message;
                var propietario = _repositorio.ObtenerPorId(id);
                return View("Delete", propietario);
            }
        }
    }
}