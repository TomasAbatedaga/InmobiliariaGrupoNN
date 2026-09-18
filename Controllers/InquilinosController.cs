using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaGrupoNN.Controllers
{
    [Authorize]
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino _repositorio;

        public InquilinosController(IRepositorioInquilino repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: Inquilinos
        [Authorize]
        public IActionResult Index(int pagina = 1, int tamanio = 10)
        {
            var inquilinos = _repositorio.ObtenerTodos(pagina, tamanio);
            
            int totalRegistros = _repositorio.ObtenerTotal();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanio);

            ViewBag.PaginaActual = pagina;
            ViewBag.TamanioPagina = tamanio;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(inquilinos);
        }

        // GET: Inquilinos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquilinos/Create
        [HttpPost]
        public IActionResult Create(Inquilino inquilino)
        {
            if (ModelState.IsValid)
            {
                _repositorio.Alta(inquilino);
                return RedirectToAction(nameof(Index));
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Edit/id
        public IActionResult Edit(int id)
        {
            var inquilino = _repositorio.ObtenerPorId(id);

            if (inquilino == null)
                return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Edit/id
        [HttpPost]
        public IActionResult Edit(int id, Inquilino inquilino)
        {
            if (ModelState.IsValid)
            {
                inquilino.Id = id;
                _repositorio.Modificacion(inquilino);

                return RedirectToAction(nameof(Index));
            }

            return View(inquilino);
        }

        // GET: Inquilinos/Delete/id
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var inquilino = _repositorio.ObtenerPorId(id);

            if (inquilino == null)
                return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorio.Baja(id);

            return RedirectToAction(nameof(Index));
        }
    }
}