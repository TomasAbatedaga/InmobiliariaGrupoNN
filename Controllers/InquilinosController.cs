using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;

namespace InmobiliariaGrupoNN.Controllers
{
    public class InquilinosController : Controller
    {
        private readonly IRepositorioInquilino _repositorio;

        public InquilinosController(IRepositorioInquilino repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: Inquilinos
        public IActionResult Index()
        {
            var lista = _repositorio.ObtenerTodos();
            return View(lista);
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
        public IActionResult Delete(int id)
        {
            var inquilino = _repositorio.ObtenerPorId(id);

            if (inquilino == null)
                return NotFound();

            return View(inquilino);
        }

        // POST: Inquilinos/Delete/id
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorio.Baja(id);

            return RedirectToAction(nameof(Index));
        }
    }
}