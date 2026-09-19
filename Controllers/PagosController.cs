using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaGrupoNN.Controllers
{
    [Authorize]
    public class PagosController : Controller
    {
        private readonly IRepositorioPago _repoPago;
        private readonly IRepositorioUsuario _repoUsuario;
        private readonly IRepositorioReserva _repoReserva;

        public PagosController(IRepositorioPago repoPago, IRepositorioReserva repoReserva, IRepositorioUsuario repoUsuario)
        {
            _repoPago = repoPago;
            _repoUsuario = repoUsuario;
            _repoReserva = repoReserva;
        }

        public IActionResult Index(int reservaId, int pagina = 1, int tamanio = 10)
        {
            if (reservaId <= 0) return BadRequest("La reserva no es válida.");
            var reserva = _repoReserva.ObtenerPorId(reservaId);
            if (reserva == null) return NotFound();
            if (tamanio != 5 && tamanio != 10 && tamanio != 20) tamanio = 10;
            int cantidad = _repoPago.ObtenerCantidadPorReserva(reservaId);
            int paginas = Math.Max(1, (int)Math.Ceiling(cantidad / (decimal)tamanio));
            pagina = Math.Clamp(pagina, 1, paginas);
            ViewBag.Reserva = reserva;
            ViewBag.PaginaActual = pagina;
            ViewBag.TamanioPagina = tamanio;
            ViewBag.TotalPaginas = paginas;
            ViewBag.TotalRegistros = cantidad;
            return View(_repoPago.ObtenerPorReserva(reservaId, pagina, tamanio));
        }

        public IActionResult Create(int reservaId)
        {
            if (reservaId <= 0) return BadRequest("La reserva no es válida.");
            var reserva = _repoReserva.ObtenerPorId(reservaId);
            if (reserva == null) return NotFound();
            return View(new Pago { ReservaId = reservaId, Reserva = reserva });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int reservaId, string? concepto, DateTime? fechaPago, decimal? importe)
        {
            if (reservaId <= 0) return BadRequest("La reserva no es válida.");
            var reserva = _repoReserva.ObtenerPorId(reservaId);
            if (reserva == null) return NotFound();
            var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out int usuarioId) || usuarioId <= 0) return Forbid();
            var pago = new Pago
            {
                CreadoPorId = usuarioId,
                ReservaId = reservaId,
                Reserva = reserva,
                Concepto = concepto ?? "",
                FechaPago = fechaPago ?? DateTime.Today,
                Importe = importe ?? 0m
            };
            ValidarConcepto(concepto);
            if (!fechaPago.HasValue)
                ModelState.AddModelError(nameof(Pago.FechaPago), "La fecha del pago es obligatoria.");
            else if (fechaPago.Value.Date < new DateTime(1000, 1, 1))
                ModelState.AddModelError(nameof(Pago.FechaPago), "La fecha del pago no es válida para MySQL.");
            if (!importe.HasValue || importe.Value < 0.01m || importe.Value > 99999999.99m)
                ModelState.AddModelError(nameof(Pago.Importe), "El importe debe estar entre 0,01 y 99.999.999,99.");
            else if (decimal.Round(importe.Value, 2) != importe.Value)
                ModelState.AddModelError(nameof(Pago.Importe), "El importe debe tener como máximo dos decimales.");
            if (!ModelState.IsValid) return View(pago);
            try
            {
                _repoPago.Alta(pago);
                TempData["Mensaje"] = "Pago registrado correctamente.";
                return Volver(pago.ReservaId);
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo guardar el pago. Verifique la reserva y la conexión a la base de datos.");
                return View(pago);
            }
        }

        public IActionResult Details(int id)
        {
            var pago = BuscarPago(id);
            if (pago == null) return NotFound();
            if (User.IsInRole("Administrador"))
            {
                if (pago.CreadoPorId.HasValue) pago.CreadoPor = _repoUsuario.ObtenerPorId(pago.CreadoPorId.Value);
                if (pago.AnuladoPorId.HasValue) pago.AnuladoPor = _repoUsuario.ObtenerPorId(pago.AnuladoPorId.Value);
            }
            return View(pago);
        }

        public IActionResult Edit(int id)
        {
            var pago = BuscarPago(id);
            if (pago == null) return NotFound();
            if (!pago.EstadoActivo) return PagoAnulado(pago.ReservaId);
            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, string? concepto)
        {
            var pago = BuscarPago(id);
            if (pago == null) return NotFound();
            if (!pago.EstadoActivo) return PagoAnulado(pago.ReservaId);
            ValidarConcepto(concepto);
            if (!ModelState.IsValid)
            {
                pago.Concepto = concepto ?? "";
                return View(pago);
            }
            try
            {
                int filas = _repoPago.ModificarConcepto(pago.Id, concepto!);
                if (filas == 0)
                {
                    
                    var actual = _repoPago.ObtenerPorId(pago.Id);
                    if (actual == null) return NotFound();
                    if (!actual.EstadoActivo) return PagoAnulado(actual.ReservaId);
                    TempData["Mensaje"] = "El concepto no tuvo cambios.";
                }
                else TempData["Mensaje"] = "Concepto actualizado correctamente.";
                return Volver(pago.ReservaId);
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo actualizar el concepto. Intente nuevamente.");
                pago.Concepto = concepto ?? "";
                return View(pago);
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Anular(int id)
        {
            var pago = BuscarPago(id);
            if (pago == null) return NotFound();
            if (!pago.EstadoActivo) return PagoAnulado(pago.ReservaId);
            return View(pago);
        }

        [HttpPost, ActionName("Anular")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult AnularConfirmed(int id)
        {
            var pago = BuscarPago(id);
            if (pago == null) return NotFound();
            if (!pago.EstadoActivo) return PagoAnulado(pago.ReservaId);
            try
            {
                var claimId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(claimId, out int usuarioId) || usuarioId <= 0) return Forbid();
                int filas = _repoPago.Anular(id, usuarioId);
                TempData[filas == 1 ? "Mensaje" : "Error"] = filas == 1
                    ? "Pago anulado correctamente. El registro permanece en el listado."
                    : "El pago ya estaba anulado o no se pudo anular. No se modificó su fecha de anulación.";
                return Volver(pago.ReservaId);
            }
            catch (MySqlException)
            {
                ModelState.AddModelError("", "No se pudo anular el pago. Intente nuevamente.");
                return View("Anular", pago);
            }
        }

        private Pago? BuscarPago(int id)
        {
            if (id <= 0) return null;
            var pago = _repoPago.ObtenerPorId(id);
            if (pago != null) pago.Reserva = _repoReserva.ObtenerPorId(pago.ReservaId);
            return pago;
        }

        private void ValidarConcepto(string? concepto)
        {
            if (string.IsNullOrWhiteSpace(concepto))
            {
                ModelState.AddModelError(nameof(Pago.Concepto), "El concepto es obligatorio.");
                return;
            }
            concepto = concepto.Trim();
            if (concepto.Length > 150)
                ModelState.AddModelError(nameof(Pago.Concepto), "El concepto no puede superar los 150 caracteres.");
        }

        private IActionResult Volver(int reservaId) => RedirectToAction(nameof(Index), new { reservaId });

        private IActionResult PagoAnulado(int reservaId)
        {
            TempData["Error"] = "El pago ya está anulado. No puede editarse ni anularse nuevamente.";
            return Volver(reservaId);
        }
    }
}
