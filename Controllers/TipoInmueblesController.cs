using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;

namespace InmobiliariaGrupoNN.Controllers
{
    public class TipoInmueblesController : Controller
    {
        private readonly IRepositorioTipoInmueble _repo;

        public TipoInmueblesController(IRepositorioTipoInmueble repo)
        {
            _repo = repo;
        }

        // GET: TipoInmuebles
        public IActionResult Index(int pagina = 1, int tamanio = 10)
        {
            ViewBag.PaginaActual = pagina; 
            ViewBag.TamanioPagina = tamanio;

            var lista = _repo.ObtenerTodos(pagina, tamanio);
            return View(lista);
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
        public IActionResult Delete(int id)
        {
            var tipo = _repo.ObtenerPorId(id);
            if (tipo == null) return NotFound();
            
            return View(tipo);
        }

        // POST: TipoInmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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