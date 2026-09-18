using Microsoft.AspNetCore.Mvc;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using Microsoft.AspNetCore.Authorization;
using System;

namespace InmobiliariaGrupoNN.Controllers
{ [Authorize]
    
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
            tamanio = NormalizarTamanio(tamanio);
            pagina = PrepararPaginacion(_repoReserva.ObtenerCantidad(), pagina, tamanio);
            var lista = _repoReserva.ObtenerTodos(pagina, tamanio);
            return View(lista);
        }

        // GET: Reservas/Details/id
        public IActionResult Details(int id)
        {
            if (id <= 0) return NotFound();
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            
            return View(reserva);
        }

        public IActionResult BuscarDisponibilidad(DateTime? fechaInicio, DateTime? fechaFin,
            int? cupoMinimo, string? tipo, int pagina = 1, int tamanio = 10, bool buscar = false)
        {
            fechaInicio = fechaInicio?.Date;
            fechaFin = fechaFin?.Date;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.CupoMinimo = cupoMinimo;
            ViewBag.Tipo = tipo;
            ViewBag.BusquedaRealizada = false;
            tamanio = NormalizarTamanio(tamanio);
            PrepararPaginacion(0, 1, tamanio);
            if (!buscar) return View(new List<Inmueble>());

            ValidarFechas(fechaInicio, fechaFin);
            if (cupoMinimo.HasValue && cupoMinimo.Value < 1)
                ModelState.AddModelError(nameof(cupoMinimo), "El cupo mínimo debe ser mayor a cero.");
            if (!ModelState.IsValid) return View(new List<Inmueble>());

            int cantidad = _repoInmueble.ObtenerCantidadDisponibles(
                fechaInicio!.Value, fechaFin!.Value, cupoMinimo, tipo);
            pagina = PrepararPaginacion(cantidad, pagina, tamanio);
            ViewBag.BusquedaRealizada = true;
            return View(_repoInmueble.BuscarDisponibles(
                fechaInicio.Value, fechaFin.Value, cupoMinimo, tipo, pagina, tamanio));
        }

        // GET: Reservas/Create desde los resultados de disponibilidad.
        public IActionResult Create(int? inmuebleId, DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!inmuebleId.HasValue) return RedirectToAction(nameof(BuscarDisponibilidad));
            if (inmuebleId.Value <= 0) return BadRequest("El inmueble no es válido.");
            ValidarFechas(fechaInicio, fechaFin);
            if (!ModelState.IsValid) return BadRequest("Debe seleccionar un período válido desde la búsqueda.");
            var inmueble = _repoInmueble.ObtenerPorId(inmuebleId.Value);
            if (inmueble == null) return NotFound();
            if (!inmueble.EstadoActivo || !inmueble.Disponible)
                return BadRequest("El inmueble no está habilitado para nuevas reservas.");
            ViewBag.Inquilinos = _repoInquilino.ObtenerTodos();
            return View(new Reserva
            {
                InmuebleId = inmueble.Id,
                Inmueble = inmueble,
                FechaInicio = fechaInicio!.Value.Date,
                FechaFin = fechaFin!.Value.Date,
                MontoPorDia = inmueble.PrecioPorDia
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("InmuebleId,InquilinoId,FechaInicio,FechaFin,MontoPorDia")] Reserva reserva)
        {
            reserva.Id = 0;
            reserva.FechaInicio = reserva.FechaInicio.Date;
            reserva.FechaFin = reserva.FechaFin.Date;
            ValidarFechas(reserva.FechaInicio, reserva.FechaFin);

            if (reserva.FechaInicio < DateTime.Today)
                ModelState.AddModelError(nameof(reserva.FechaInicio), "No se pueden crear reservas con inicio en el pasado.");

            var inmueble = reserva.InmuebleId > 0 ? _repoInmueble.ObtenerPorId(reserva.InmuebleId) : null;
            
            if (inmueble == null)
                ModelState.AddModelError(nameof(reserva.InmuebleId), "El inmueble seleccionado no existe.");
            else if (!inmueble.EstadoActivo || !inmueble.Disponible)
                ModelState.AddModelError(nameof(reserva.InmuebleId), "El inmueble ya no está habilitado para nuevas reservas.");
                
            reserva.Inmueble = inmueble;

            try
            {
                if (ModelState.IsValid)
                {
                    var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(claimId, out int usuarioId))
                    {
                        reserva.CreadoPorId = usuarioId;
                    }
                    _repoReserva.Alta(reserva);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            
            ViewBag.Inquilinos = _repoInquilino.ObtenerTodos();
            ViewBag.Inmuebles = _repoInmueble.ObtenerTodos();
            return View(reserva);
        }

        // GET: Reservas/Edit/id
        public IActionResult Edit(int id)
        {
            if (id <= 0) return NotFound();
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            if (!reserva.EstadoActivo) return Conflict("La reserva está anulada.");

            ViewBag.Inmuebles = _repoInmueble.ObtenerTodos();
            ViewBag.Inquilinos = _repoInquilino.ObtenerTodos();
            return View(reserva);
        }

        // POST: Reservas/Edit/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([FromRoute] int id, [FromForm, Bind("Id,InmuebleId,InquilinoId,FechaInicio,FechaFin,MontoPorDia")] Reserva reserva)
        {
            if (id <= 0 || id != reserva.Id) return BadRequest("El identificador de la reserva no coincide.");
            var actual = _repoReserva.ObtenerPorId(id);
            if (actual == null) return NotFound();
            if (!actual.EstadoActivo) return Conflict("La reserva está anulada.");
            reserva.FechaInicio = reserva.FechaInicio.Date;
            reserva.FechaFin = reserva.FechaFin.Date;
            try
            {
                if (ModelState.IsValid)
                {
                    int filas = _repoReserva.Modificacion(reserva);
                    if (filas == 0)
                    {
                        var vigente = _repoReserva.ObtenerPorId(id);
                        if (vigente == null) return NotFound();
                        if (!vigente.EstadoActivo) return Conflict("La reserva está anulada.");
                    }
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

        private static int NormalizarTamanio(int tamanio) =>
            tamanio == 5 || tamanio == 10 || tamanio == 20 ? tamanio : 10;

        private int PrepararPaginacion(int cantidad, int pagina, int tamanio)
        {
            int totalPaginas = Math.Max(1, (int)Math.Ceiling(cantidad / (decimal)tamanio));
            pagina = Math.Clamp(pagina, 1, totalPaginas);
            ViewBag.TotalRegistros = cantidad;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.PaginaActual = pagina;
            ViewBag.TamanioPagina = tamanio;
            return pagina;
        }

        private void ValidarFechas(DateTime? fechaInicio, DateTime? fechaFin)
        {
            if (!fechaInicio.HasValue || fechaInicio.Value.Date < new DateTime(1000, 1, 1))
                ModelState.AddModelError(nameof(Reserva.FechaInicio), "La fecha de inicio es obligatoria y debe ser válida.");
            if (!fechaFin.HasValue || fechaFin.Value.Date < new DateTime(1000, 1, 1))
                ModelState.AddModelError(nameof(Reserva.FechaFin), "La fecha de fin es obligatoria y debe ser válida.");
            if (fechaInicio.HasValue && fechaFin.HasValue && fechaFin.Value.Date <= fechaInicio.Value.Date)
                ModelState.AddModelError(nameof(Reserva.FechaFin), "La fecha de fin debe ser posterior a la de inicio.");
        }
        // GET: Reservas/Delete/id
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return NotFound();
            var reserva = _repoReserva.ObtenerPorId(id);
            if (reserva == null) return NotFound();
            if (!reserva.EstadoActivo) return Conflict("La reserva está anulada.");
            
            return View(reserva);
        }

        // POST: Reservas/Delete/id
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (id <= 0) return NotFound();
            var actual = _repoReserva.ObtenerPorId(id);
            if (actual == null) return NotFound();
            if (!actual.EstadoActivo) return Conflict("La reserva ya está anulada.");
            try
            {
                int? anuladoPorId = null;
                var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (int.TryParse(claimId, out int usuarioId))
                {
                    anuladoPorId = usuarioId;
                }
                if (_repoReserva.Baja(id, anuladoPorId) != 1)
                {
                    if (_repoReserva.ObtenerPorId(id) == null) return NotFound();
                    return Conflict("La reserva ya está anulada. No se modificó su anulador.");
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                var reserva = _repoReserva.ObtenerPorId(id);
                return View("Delete", reserva);
            }
        }

        // GET: Reservas/Renovar/5
        public IActionResult Renovar(int id)
        {
            if (id <= 0) return NotFound();
            var reservaOriginal = _repoReserva.ObtenerPorId(id);
            if (reservaOriginal == null) return NotFound();
            if (!reservaOriginal.EstadoActivo) return Conflict("No se puede renovar una reserva anulada.");

            var nuevaReserva = new Reserva
            {
                InmuebleId = reservaOriginal.InmuebleId,
                InquilinoId = reservaOriginal.InquilinoId,
                FechaInicio = reservaOriginal.FechaFin, 
                FechaFin = reservaOriginal.FechaFin.AddMonths(1),
                MontoPorDia = reservaOriginal.MontoPorDia
            };

            ViewBag.ReservaOriginal = reservaOriginal;

            return View(nuevaReserva);
        }

        // POST: Reservas/Renovar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Renovar(int reservaOriginalId,
            [Bind("FechaInicio,FechaFin,MontoPorDia")] Reserva nuevaReserva)
        {
            if (reservaOriginalId <= 0) return NotFound();
            var reservaOriginal = _repoReserva.ObtenerPorId(reservaOriginalId);
            if (reservaOriginal == null) return NotFound();
            if (!reservaOriginal.EstadoActivo) return Conflict("No se puede renovar una reserva anulada.");

            nuevaReserva.InmuebleId = reservaOriginal.InmuebleId;
            nuevaReserva.InquilinoId = reservaOriginal.InquilinoId;
            ViewBag.ReservaOriginal = reservaOriginal;
            try
            {
                if (nuevaReserva.FechaFin <= nuevaReserva.FechaInicio)
                {
                    ModelState.AddModelError("", "La fecha de finalizacion debe ser posterior a la fecha de inicio.");
                }

                if (ModelState.IsValid)
                {
                    var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(claimId, out int usuarioId))
                    {
                        nuevaReserva.CreadoPorId = usuarioId;
                    }
                    _repoReserva.Alta(nuevaReserva);
                    TempData["Mensaje"] = "El contrato ha sido renovado exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View(nuevaReserva);
        }
    }
}