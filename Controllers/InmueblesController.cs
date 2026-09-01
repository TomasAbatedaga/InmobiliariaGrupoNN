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

        public InmueblesController(
            IRepositorioInmueble repositorioInmueble,
            IRepositorioPropietario repositorioPropietario,
            IRepositorioTipoInmueble repositorioTipoInmueble)
        {
            _repositorioInmueble = repositorioInmueble;
            _repositorioPropietario = repositorioPropietario;
            _repositorioTipoInmueble = repositorioTipoInmueble;
        }


        // LISTADO
        public IActionResult Index()
        {
            var lista = _repositorioInmueble.ObtenerTodos();

            return View(lista);
        }


        // DETALLES
        public IActionResult Details(int id)
        {
            var inmueble = _repositorioInmueble.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }


        // CREAR - GET
        public IActionResult Create()
        {
            CargarListas();

            return View();
        }


        // CREAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _repositorioInmueble.Alta(inmueble);

                return RedirectToAction(nameof(Index));
            }

            CargarListas();

            return View(inmueble);
        }


        // EDITAR - GET
        public IActionResult Edit(int id)
        {
            var inmueble = _repositorioInmueble.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            CargarListas();

            return View(inmueble);
        }


        // EDITAR - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _repositorioInmueble.Modificacion(inmueble);

                return RedirectToAction(nameof(Index));
            }

            CargarListas();

            return View(inmueble);
        }


        // ELIMINAR - GET
        public IActionResult Delete(int id)
        {
            var inmueble = _repositorioInmueble.ObtenerPorId(id);

            if (inmueble == null)
            {
                return NotFound();
            }

            return View(inmueble);
        }


        // ELIMINAR - POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repositorioInmueble.Baja(id);

            return RedirectToAction(nameof(Index));
        }


        // CARGA LOS DESPLEGABLES DE PROPIETARIO Y TIPO DE INMUEBLE
        private void CargarListas()
        {
            var propietarios = _repositorioPropietario
                .ObtenerTodos()
                .Select(p => new
                {
                    p.Id,
                    NombreCompleto = $"{p.Nombre} {p.Apellido}"
                });

            ViewBag.Propietarios = new SelectList(
                propietarios,
                "Id",
                "NombreCompleto");


            var tipos = _repositorioTipoInmueble
                .ObtenerTodos(1, 100);

            ViewBag.TiposInmueble = new SelectList(
                tipos,
                "Id",
                "Nombre");
        }
    }
}