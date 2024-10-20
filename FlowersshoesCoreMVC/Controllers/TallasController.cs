using FlowersshoesCoreMVC.Models;
using FlowersshoesCoreMVC.Models.Vistas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Text;


namespace FlowersshoesCoreMVC.Controllers
{
    public class TallasController : Controller
    {
        List<TbTalla> lista = new List<TbTalla>();
        // GET: TallasController
        public async Task<List<TbTalla>> GetTallas()
        {
            using (var httpcliente = new HttpClient())
            {
                var respuesta = await httpcliente.GetAsync("http://localhost:5050/api/Talla/GetTallas");
                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<TbTalla>>(respuestaAPI)!;
            }
        }
        public async Task<string> CrearTalla(TbTalla obj)
        {
            string cadena = string.Empty;


            using (var httpcliente = new HttpClient())
            {

                StringContent contenido = new StringContent(
                   JsonConvert.SerializeObject(obj), Encoding.UTF8,
                          "application/json");

                HttpResponseMessage respuesta = new HttpResponseMessage();
                respuesta = await httpcliente.PostAsync("http://localhost:5050/api/Talla/GrabarTalla", contenido);

                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                cadena = respuestaAPI;
            }
            return cadena;
        }

        public async Task<string> EditarTalla(TbTalla obj)
        {
            string cadena = string.Empty;


            using (var httpcliente = new HttpClient())
            {

                StringContent contenido = new StringContent(
                   JsonConvert.SerializeObject(obj), Encoding.UTF8,
                          "application/json");

                HttpResponseMessage respuesta = new HttpResponseMessage();
                respuesta = await httpcliente.PutAsync("http://localhost:5050/api/Colores/ActualizarColor", contenido);

                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                cadena = respuestaAPI;
            }
            return cadena;
        }

        public async Task<string> EliminarRestaurarTalla(int id, int option)
        {
            string cadena = string.Empty;

            using (var httpClient = new HttpClient())
            {
                if (option == 1)
                {
                    HttpResponseMessage respuesta = await httpClient.DeleteAsync($"hhttp://localhost:5050/api/Talla/EliminarTalla/{id}");
                    cadena = await respuesta.Content.ReadAsStringAsync();
                }
                else
                {
                    HttpResponseMessage respuesta = await httpClient.DeleteAsync($"http://localhost:5050/api/Talla/RestaurarTalla/{id}");
                    cadena = await respuesta.Content.ReadAsStringAsync();
                }
            }

            return cadena;
        }
        TbTrabajadore? RecuperarTrabajador()
        {
            var trabajadorJson = HttpContext.Session.GetString("trabajadorActual");

            if (!string.IsNullOrEmpty(trabajadorJson))
            {
                try
                {
                    return JsonConvert.DeserializeObject<TbTrabajadore>(trabajadorJson);
                }
                catch
                {
                    HttpContext.Session.Remove("trabajadorActual");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        TbTrabajadore trabajadorActual = new TbTrabajadore();

        void GrabarTrabajador()
        {
            HttpContext.Session.SetString("trabajadorActual",
                    JsonConvert.SerializeObject(trabajadorActual));
        }

        [HttpGet]
        public async Task<IActionResult> Tallas(int id, string accion)
        {
            trabajadorActual = RecuperarTrabajador()!;

            if (trabajadorActual != null)
            {
                ViewBag.trabajador = trabajadorActual;
                ViewBag.rolTrabajador = trabajadorActual.Idrol;
            }

            lista = await GetTallas();
            TallasVista viewmodel;
            ViewBag.abrirModal = "No";  // No abrir modal por defecto

            if (id == 0)
            {
                viewmodel = new TallasVista
                {
                    listaTallas = lista
                };
            }
            else
            {
                // Si el id no es 0, significa que estamos editando
                TbTalla tallaAEditar = lista.FirstOrDefault(t => t.Idtalla == id);

                if (tallaAEditar != null)
                {
                    viewmodel = new TallasVista
                    {
                        listaTallas = lista,
                        NuevaTalla = tallaAEditar
                    };
                    ViewBag.abrirModal = "Sí";  // Abrir modal de edición
                }
                else
                {
                    return NotFound();
                }
            }

            return View(viewmodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(TallasVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbTalla nuevaTalla = model.NuevaTalla;

                    TempData["mensaje"] = await CrearTalla(nuevaTalla);

                    return RedirectToAction(nameof(Tallas));
                }
                else
                {
                    TempData["mensaje"] = "No se pudo Agregar un nuevo Registro, intentalo nuevamente";
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetTallas();

            var viewmodel = new TallasVista
            {
                NuevaTalla = new TbTalla(),
                listaTallas = lista
            };

            return View("Tallas", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(TallasVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbTalla nuevaTalla = model.NuevaTalla;

                    TempData["mensaje"] = await EditarTalla(nuevaTalla);

                    return RedirectToAction(nameof(Tallas));
                }
                else
                {
                    TempData["mensaje"] = "No se pudo Editar el Registro, intentalo nuevamente";
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetTallas();

            var viewmodel = new TallasVista
            {
                NuevaTalla = new TbTalla(),
                listaTallas = lista
            };

            return View("Talla", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(TallasVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbTalla nuevaTalla = model.NuevaTalla;

                    TempData["mensaje"] = await EliminarRestaurarTalla(model.NuevaTalla.Idtalla, 1);

                    return RedirectToAction(nameof(Tallas));
                }
                else
                {
                    TempData["mensaje"] = "No se pudo Eliminar el Registro, intentalo nuevamente";
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetTallas();

            var viewmodel = new TallasVista
            {
                NuevaTalla = new TbTalla(),
                listaTallas = lista
            };

            return View("Tallas", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restaurar(TallasVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbTalla nuevoTallar = model.NuevaTalla;

                    TempData["mensaje"] = await EliminarRestaurarTalla(model.NuevaTalla.Idtalla, 2);

                    return RedirectToAction(nameof(Tallas));
                }
                else
                {
                    TempData["mensaje"] = "No se pudo Restaurar el Registro, intentalo nuevamente";
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetTallas();

            var viewmodel = new TallasVista
            {
                NuevaTalla = new TbTalla(),
                listaTallas = lista
            };

            return View("Tallas", viewmodel);
        }

    }
}

