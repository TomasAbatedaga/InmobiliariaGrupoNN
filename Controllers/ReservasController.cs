using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using System;

namespace InmobiliariaGrupoNN.Controllers
{
    public class ReservasController : Controller
    {
        private readonly IRepositorioReserva _repoReserva;
        private readonly IRepositorioInmueble _repoInmueble;
        private readonly IRepositorioInquilino _repoInquilino;

        public ReservasController(
            IRepositorioReserva repoReserva, 
            IRepositorioInmueble repoInmueble, 
            IRepositorioInquilino repoInquilino)
        {
            _repoReserva = repoReserva;
            _repoInmueble = repoInmueble;
            _repoInquilino = repoInquilino;
        }

        // GET: Reservas
        public IActionResult Index(int pagina = 1, int tamanio = 10)
        {
            ViewBag.PaginaActual = pagina;
            ViewBag.TamanioPagina = tamanio;
            var lista = _repoReserva.ObtenerTodos(pagina, tamanio);
            return View(lista);
        }

        // GET: Reservas/Details/id
        public IActionResult Details(int id)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            
            return View(reserva);
        }

        // GET: Reservas/Create
        public IActionResult Create()
        {
            ViewBag.Inmuebles = _repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = _repoInquilino.ObtenerTodos();
            return View();
        }

        // POST: Reservas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reserva reserva)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _repoReserva.Alta(reserva);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            ViewBag.Inmuebles = _repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = _repoInquilino.ObtenerTodos();
            return View(reserva);
        }

        // GET: Reservas/Edit/id
        public IActionResult Edit(int id)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();

            ViewBag.Inmuebles = _repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = _repoInquilino.ObtenerTodos();
            return View(reserva);
        }

        // POST: Reservas/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Reserva reserva)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _repoReserva.Modificacion(reserva);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            ViewBag.Inmuebles = _repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = _repoInquilino.ObtenerTodos();
            return View(reserva);
        }

        // GET: Reservas/Delete/id
        public IActionResult Delete(int id)
        {
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            
            return View(reserva);
        }

        // POST: Reservas/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _repoReserva.Baja(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                var reserva = _repoReserva.ObtenerPorId(id);
                return View("Delete", reserva);
            }
        }
    }
}