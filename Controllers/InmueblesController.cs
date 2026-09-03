using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace InmobiliariaGrupoNN.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly IRepositorioInmueble _repositorioInmueble;
        private readonly IRepositorioPropietario _repositorioPropietario;
        private readonly IRepositorioTipoInmueble _repositorioTipoInmueble;
        private readonly IRepositorioImagen _repositorioImagen;
        private readonly IWebHostEnvironment _environment;

        public InmueblesController(
            IRepositorioInmueble repositorioInmueble,
            IRepositorioPropietario repositorioPropietario,
            IRepositorioTipoInmueble repositorioTipoInmueble,
            IRepositorioImagen repositorioImagen,
            IWebHostEnvironment environment)
        {
            _repositorioInmueble = repositorioInmueble;
            _repositorioPropietario = repositorioPropietario;
            _repositorioTipoInmueble = repositorioTipoInmueble;
            _repositorioImagen = repositorioImagen;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var inmuebles = _repositorioInmueble.ObtenerTodos();

            return View(inmuebles);
        }

        public IActionResult Details(int id)
        {
            var inmueble = _repositorioInmueble.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            inmueble.Imagenes =
                _repositorioImagen.BuscarPorInmueble(id);

            return View(inmueble);
        }

        public IActionResult Create()
        {
            CargarListas();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _repositorioInmueble.Alta(inmueble);

                return RedirectToAction(nameof(Index));
            }

            CargarListas(
                inmueble.PropietarioId,
                inmueble.TipoInmuebleId);

            return View(inmueble);
        }

        public IActionResult Edit(int id)
        {
            var inmueble =
                _repositorioInmueble.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            CargarListas(
                inmueble.PropietarioId,
                inmueble.TipoInmuebleId);

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _repositorioInmueble.Modificacion(inmueble);

                return RedirectToAction(nameof(Index));
            }

            CargarListas(
                inmueble.PropietarioId,
                inmueble.TipoInmuebleId);

            return View(inmueble);
        }

        public IActionResult Delete(int id)
        {
            var inmueble =
                _repositorioInmueble.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioInmueble.Baja(id);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Imagenes(int id)
        {
            var inmueble =
                _repositorioInmueble.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            inmueble.Imagenes =
                _repositorioImagen.BuscarPorInmueble(id);

            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Portada(
            Imagen imagen)
        {
            var inmueble =
                _repositorioInmueble.ObtenerPorId(
                    imagen.InmuebleId);

            if (inmueble == null)
            {
                return NotFound();
            }

            string carpeta = Path.Combine(
                _environment.WebRootPath,
                "Uploads",
                "Inmuebles");

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            if (!string.IsNullOrEmpty(inmueble.Portada))
            {
                string rutaAnterior =
                    ObtenerRutaFisica(inmueble.Portada);

                if (System.IO.File.Exists(rutaAnterior))
                {
                    System.IO.File.Delete(rutaAnterior);
                }
            }

            if (imagen.Archivo == null ||
                imagen.Archivo.Length == 0)
            {
                _repositorioInmueble.ModificarPortada(
                    imagen.InmuebleId,
                    null);

                TempData["Mensaje"] =
                    "Portada eliminada correctamente.";

                return RedirectToAction(
                    nameof(Imagenes),
                    new { id = imagen.InmuebleId });
            }

            string extension =
                Path.GetExtension(
                    imagen.Archivo.FileName);

            string nombreArchivo =
                $"portada_{imagen.InmuebleId}_{Guid.NewGuid()}{extension}";

            string rutaFisica =
                Path.Combine(
                    carpeta,
                    nombreArchivo);

            using (var stream =
                new FileStream(
                    rutaFisica,
                    FileMode.Create))
            {
                await imagen.Archivo.CopyToAsync(stream);
            }

            string url =
                $"/Uploads/Inmuebles/{nombreArchivo}";

            _repositorioInmueble.ModificarPortada(
                imagen.InmuebleId,
                url);

            TempData["Mensaje"] =
                "Portada actualizada correctamente.";

            return RedirectToAction(
                nameof(Imagenes),
                new { id = imagen.InmuebleId });
        }

        private void CargarListas(
            int? propietarioSeleccionado = null,
            int? tipoSeleccionado = null)
        {
            var propietarios =
                _repositorioPropietario
                    .ObtenerTodos()
                    .Where(p => p.EstadoActivo)
                    .Select(p => new
                    {
                        p.Id,
                        NombreCompleto =
                            $"{p.Nombre} {p.Apellido}"
                    })
                    .ToList();

            ViewBag.Propietarios =
                new SelectList(
                    propietarios,
                    "Id",
                    "NombreCompleto",
                    propietarioSeleccionado);

            var tipos =
                _repositorioTipoInmueble
                    .ObtenerTodos(1, 100);

            ViewBag.TiposInmueble =
                new SelectList(
                    tipos,
                    "Id",
                    "Nombre",
                    tipoSeleccionado);
        }

        private string ObtenerRutaFisica(
            string rutaRelativa)
        {
            string rutaLimpia =
                rutaRelativa
                    .TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);

            return Path.Combine(
                _environment.WebRootPath,
                rutaLimpia);
        }
    }
}