using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InmobiliariaGrupoNN.Controllers
{
    public class ImagenesController : Controller
    {
        private readonly IRepositorioImagen _repositorioImagen;

        public ImagenesController(
            IRepositorioImagen repositorioImagen)
        {
            _repositorioImagen = repositorioImagen;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Alta(
            int id,
            List<IFormFile> imagenes,
            [FromServices] IWebHostEnvironment environment)
        {
            if (imagenes == null || imagenes.Count == 0)
            {
                TempData["Error"] =
                    "Debe seleccionar al menos una imagen.";

                return RedirectToAction(
                    "Imagenes",
                    "Inmuebles",
                    new { id });
            }

            string path = Path.Combine(
                environment.WebRootPath,
                "Uploads",
                "Inmuebles",
                id.ToString());

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var archivo in imagenes)
            {
                if (archivo.Length == 0)
                {
                    continue;
                }

                string extension =
                    Path.GetExtension(archivo.FileName);

                string nombreArchivo =
                    $"{Guid.NewGuid()}{extension}";

                string rutaFisica =
                    Path.Combine(path, nombreArchivo);

                using (var stream =
                    new FileStream(
                        rutaFisica,
                        FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                var imagen = new Imagen
                {
                    InmuebleId = id,
                    Url =
                        $"/Uploads/Inmuebles/{id}/{nombreArchivo}"
                };

                _repositorioImagen.Alta(imagen);
            }

            TempData["Mensaje"] =
                "Imágenes cargadas correctamente.";

            return RedirectToAction(
                "Imagenes",
                "Inmuebles",
                new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(
            int id,
            [FromServices] IWebHostEnvironment environment)
        {
            var imagen =
                _repositorioImagen.ObtenerPorId(id);

            if (imagen == null)
            {
                return NotFound();
            }

            int inmuebleId = imagen.InmuebleId;

            string rutaRelativa =
                imagen.Url.TrimStart('/')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar);

            string rutaFisica =
                Path.Combine(
                    environment.WebRootPath,
                    rutaRelativa);

            if (System.IO.File.Exists(rutaFisica))
            {
                System.IO.File.Delete(rutaFisica);
            }

            _repositorioImagen.Baja(id);

            TempData["Mensaje"] =
                "Imagen eliminada correctamente.";

            return RedirectToAction(
                "Imagenes",
                "Inmuebles",
                new { id = inmuebleId });
        }
    }
}