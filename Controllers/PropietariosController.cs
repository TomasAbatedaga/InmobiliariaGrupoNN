using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;

namespace InmobiliariaGrupoNN.Controllers
{
    public class PropietariosController : Controller
    {
        private readonly IRepositorioPropietario _repositorio;

        public PropietariosController(IRepositorioPropietario repositorio)
        {
            _repositorio = repositorio;
        }

        // GET: Propietarios
        public IActionResult Index()
        {
            var lista = _repositorio.ObtenerTodos();
            return View(lista);
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
        public IActionResult Delete(int id)
        {
            var propietario = _repositorio.ObtenerPorId(id);
            if (propietario == null) return NotFound();

            return View(propietario);
        }

        // POST: Propietarios/Delete/id
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorio.Baja(id); // baja logica
            return RedirectToAction(nameof(Index));
        }
    }
}