using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaGrupoNN.Controllers
{
    [Authorize]
    public class TipoInmueblesController : Controller
    {
        private readonly IRepositorioTipoInmueble _repo;

        public TipoInmueblesController(IRepositorioTipoInmueble repo)
        {
            _repo = repo;
        }

        // GET: TipoInmuebles
        [Authorize]
        public IActionResult Index(int pagina = 1, int tamanio = 10)
        {
            var tipos = _repo.ObtenerTodos(pagina, tamanio);
            
            int totalRegistros = _repo.ObtenerTotal();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanio);

            ViewBag.PaginaActual = pagina;
            ViewBag.TamanioPagina = tamanio;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(tipos);
        }

        // GET: TipoInmuebles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoInmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                _repo.Alta(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }
        // GET: TipoInmuebles/Edit/5
        public IActionResult Edit(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null) return NotFound();
            
            return View(tipo);
        }

        // POST: TipoInmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TipoInmueble tipo)
        {
            if (ModelState.IsValid)
            {
                _repo.Modificacion(tipo);
                return RedirectToAction(nameof(Index));
            }
            return View(tipo);
        }

        // GET: TipoInmuebles/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null) return NotFound();
            
            return View(tipo);
        }

        // POST: TipoInmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repo.Baja(id);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ViewBag.Error = ex.Message;
                var tipo = _repo.ObtenerPorId(id);
                return View("Delete", tipo);
            }
        }
    }
    
}