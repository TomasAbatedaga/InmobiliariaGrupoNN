using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using InmobiliariaGrupoNN.Models;
using InmobiliariaGrupoNN.Repositories;
using System;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace InmobiliariaGrupoNN.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _repo;
        private readonly IWebHostEnvironment _environment;

        public UsuariosController(IRepositorioUsuario repo, IWebHostEnvironment environment)
        {
            _repo = repo;
            _environment = environment;
        }

        // GET: Usuarios
        [Authorize(Roles = "Administrador")]
        public IActionResult Index(int pagina = 1, int tamanio = 10)
        {
            var usuarios = _repo.ObtenerTodos(pagina, tamanio);
            
            int totalRegistros = _repo.ObtenerTotal();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / tamanio);

            ViewBag.PaginaActual = pagina;
            ViewBag.TamanioPagina = tamanio;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalRegistros = totalRegistros;

            return View(usuarios);
        }
        // GET: Usuarios/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Create(Usuario usuario, IFormFile? avatarFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (avatarFile != null && avatarFile.Length > 0)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                        
                        string path = Path.Combine(wwwRootPath, "uploads", "avatars");
                        
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        string fullPath = Path.Combine(path, fileName);
                        
                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            avatarFile.CopyTo(fileStream);
                        }

                        usuario.Avatar = "/uploads/avatars/" + fileName;
                    }

                    _repo.Alta(usuario);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            
            return View(usuario);
        }
        
        // GET: Usuarios/Edit/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(Usuario usuario, IFormFile? avatarFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (avatarFile != null && avatarFile.Length > 0)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                        string path = Path.Combine(wwwRootPath, "uploads", "avatars");
                        
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        string fullPath = Path.Combine(path, fileName);
                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            avatarFile.CopyTo(fileStream);
                        }

                        if (!string.IsNullOrEmpty(usuario.Avatar))
                        {
                            string oldImagePath = Path.Combine(wwwRootPath, usuario.Avatar.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        usuario.Avatar = "/uploads/avatars/" + fileName;
                    }

                    _repo.Modificacion(usuario);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var usuario = _repo.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var usuario = _repo.ObtenerPorId(id);
                if (usuario != null && !string.IsNullOrEmpty(usuario.Avatar))
                {
                    string imagePath = Path.Combine(_environment.WebRootPath, usuario.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _repo.Baja(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                var usuario = _repo.ObtenerPorId(id);
                return View("Delete", usuario);
            }
        }

        // GET: Usuarios/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Usuarios/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid) return View(modelo);

            var usuario = _repo.ObtenerPorEmail(modelo.Email);

            if (usuario == null || usuario.Clave != modelo.Clave)
            {
                ViewBag.Error = "Email o contrasenia incorrectos.";
                return View(modelo);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("Avatar", usuario.Avatar ?? "") 
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // GET: Usuarios/Logout
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuarios");
        }

        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        // GET: Usuarios/Perfil
        public IActionResult Perfil()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idClaim, out int idUsuario)) return RedirectToAction("Login");

            var usuario = _repo.ObtenerPorId(idUsuario);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // POST: Usuarios/Perfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Perfil(Usuario usuario, IFormFile? avatarFile)
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuario.Id.ToString() != idClaim) 
            {
                return RedirectToAction("AccesoDenegado");
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var usuarioOriginal = _repo.ObtenerPorId(usuario.Id);
                    usuario.Rol = usuarioOriginal!.Rol; 

                    if (avatarFile != null && avatarFile.Length > 0)
                    {
                        string wwwRootPath = _environment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                        string path = Path.Combine(wwwRootPath, "uploads", "avatars");
                        
                        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                        string fullPath = Path.Combine(path, fileName);
                        using (var fileStream = new FileStream(fullPath, FileMode.Create))
                        {
                            avatarFile.CopyTo(fileStream);
                        }

                        if (!string.IsNullOrEmpty(usuario.Avatar))
                        {
                            string oldImagePath = Path.Combine(wwwRootPath, usuario.Avatar.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                        }

                        usuario.Avatar = "/uploads/avatars/" + fileName;
                    }

                    _repo.Modificacion(usuario);
                    ViewBag.Mensaje = "Perfil actualizado correctamente. (Si cambiaste tu foto, inicia sesion nuevamente para verla en la barra superior).";
                    return View(usuario);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            
            return View(usuario);
        }
        
    }
}