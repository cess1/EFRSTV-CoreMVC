using FlowersshoesCoreMVC.Models;
using FlowersshoesCoreMVC.Models.Vistas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;

namespace FlowersshoesCoreMVC.Controllers
{
    public class ProductosController : Controller
    {
        List<PA_LISTAR_PRODUCTOS> lista = new List<PA_LISTAR_PRODUCTOS>();

        private readonly IWebHostEnvironment _env;
        private readonly flowersshoesContext db;
        public ProductosController(IWebHostEnvironment env, flowersshoesContext ctx)
        {
            _env = env;
            db = ctx;
        }

       

        public async Task<List<PA_LISTAR_PRODUCTOS>> GetProductos()
        {

            using (var httpcliente = new HttpClient())
            {

                var respuesta =
                    await httpcliente.GetAsync(
                        "http://localhost:5050/api/Productos/GetProductos");
                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<PA_LISTAR_PRODUCTOS>>(respuestaAPI)!;
            }
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


        public async Task<string> CrearProducto(TbProducto obj)
        {
            string cadena = string.Empty;


            using (var httpcliente = new HttpClient())
            {

                StringContent contenido = new StringContent(
                   JsonConvert.SerializeObject(obj), Encoding.UTF8,
                          "application/json");

                HttpResponseMessage respuesta = new HttpResponseMessage();
                respuesta =
                        await httpcliente.PostAsync("http://localhost:5050/api/Productos/GrabarProducto", contenido);

                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                cadena = respuestaAPI;
            }
            return cadena;
        }

        public async Task<string> EditarProducto(TbProducto obj)
        {
            string cadena = string.Empty;


            using (var httpcliente = new HttpClient())
            {

                StringContent contenido = new StringContent(
                   JsonConvert.SerializeObject(obj), Encoding.UTF8,
                          "application/json");

                HttpResponseMessage respuesta = new HttpResponseMessage();
                respuesta =
                   await httpcliente.PutAsync("http://localhost:5050/api/Productos/ActualizarProducto", contenido);

                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                cadena = respuestaAPI;
            }
            return cadena;
        }



        public async Task<string> EliminarRestaurarProducto(int id, int option)
        {
            string cadena = string.Empty;

            using (var httpClient = new HttpClient())
            {
                if (option == 1)
                {

                    HttpResponseMessage respuesta = await httpClient.DeleteAsync($"http://localhost:5050/api/Productos/DeleteProductos/{id}");
                    cadena = await respuesta.Content.ReadAsStringAsync();
                    
                }
                else
                {
                    HttpResponseMessage respuesta = await httpClient.DeleteAsync($"http://localhost:5050/api/Productos/RestaurarProductos/{id}");
                    cadena = await respuesta.Content.ReadAsStringAsync();
                }
            }

            return cadena;
        }



        public async Task<List<TbTalla>> traerTallas()
        {
            // permite realizar una solicitud al servicio web api
            using (var httpcliente = new HttpClient())
            {
                // realizamos una solicitud Get
                var respuesta =
                    await httpcliente.GetAsync(
                        "http://localhost:5050/api/Talla/GetTallas");
                // convertimos el contenido de la variable respuesta a una cadena
                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<TbTalla>>(respuestaAPI)!;
            }
        }


        public async Task<List<TbColores>> traerColores()
        {
            // permite realizar una solicitud al servicio web api
            using (var httpcliente = new HttpClient())
            {
                // realizamos una solicitud Get
                var respuesta =
                    await httpcliente.GetAsync(
                        "http://localhost:5050/api/Colores/GetColores");
                // convertimos el contenido de la variable respuesta a una cadena
                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<TbColores>>(respuestaAPI)!;
            }
        }




        [HttpGet]
        public async Task<IActionResult> Productos(int id, string accion, string cacheBuster)
        {
            trabajadorActual = RecuperarTrabajador()!;

            if (trabajadorActual != null)
            {
                ViewBag.trabajador = trabajadorActual;
                ViewBag.rolTrabajador = trabajadorActual.Idrol;
            }

            lista = await GetProductos();
            ProductosVista viewmodel;
            ViewBag.abrirModal = "No";

            if (id == 0)
            {
                viewmodel = new ProductosVista
                {
                    NuevoProductos = new TbProducto(),
                    listaProductos = lista
                };
            }
            else
            {
                viewmodel = new ProductosVista
                {
                    NuevoProductos = db.TbProductos.Find(id)!,
                    listaProductos = lista
                };
                ViewBag.abrirModal = accion;
            }

            ViewBag.tallas =
               new SelectList(await traerTallas(), "Idtalla", "Talla");

            //
            ViewBag.color =
                new SelectList(await traerColores(), "Idcolor", "Color");

            return View(viewmodel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(ProductosVista model,  IFormFile imagenInput )
        {

            TbProducto nuevoProducto = model.NuevoProductos;

            if (imagenInput != null && imagenInput.Length > 0)
            {
                
                if (imagenInput.ContentType.StartsWith("image/"))
                {
                    
                    var nombreImagen = nuevoProducto.Nompro+nuevoProducto.Idtalla+nuevoProducto.Idcolor+".jpg";

                   
                    var rutaImagen = Path.Combine(_env.WebRootPath, "ImagenesProductos", nombreImagen);

                    
                    using (var stream = new FileStream(rutaImagen, FileMode.Create))
                    {
                        await imagenInput.CopyToAsync(stream);
                    }
                    try
                    {

                        TempData["mensaje"] = await CrearProducto(nuevoProducto); ;
                        return RedirectToAction(nameof(Productos));
                    }
                    catch (Exception ex)
                    {
                        TempData["mensaje"] = "Error: " + ex.Message;
                    }
                }
                else
                {
                    TempData["mensaje"] = "El archivo no es una imagen válida.";
                }
            }
            else
            {
                TempData["mensaje"] = "Agregue una imagen";
            }


            lista = await GetProductos();

            var viewmodel = new ProductosVista
            {
                NuevoProductos = new TbProducto(),
                listaProductos = lista
            };
            ViewBag.tallas =
               new SelectList(await traerTallas(), "Idtalla", "Talla");
            //
            ViewBag.color =
                new SelectList(await traerColores(), "Idcolor", "Color");

            return View("Productos", viewmodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ProductosVista model, IFormFile imagenInputEdit)
        {

            TbProducto nuevoProducto = model.NuevoProductos;

            if (imagenInputEdit != null && imagenInputEdit.Length > 0)
            {

                if (imagenInputEdit.ContentType.StartsWith("image/"))
                {

                    var nombreImagen = nuevoProducto.Nompro + nuevoProducto.Idtalla + nuevoProducto.Idcolor + ".jpg";
                    var rutaImagen = Path.Combine(_env.WebRootPath, "ImagenesProductos", nombreImagen);

                    
                    using (var stream = new FileStream(rutaImagen, FileMode.Create))
                    {
                        await imagenInputEdit.CopyToAsync(stream);
                    }

                    try
                    {
                        TempData["mensaje"] = await EditarProducto(nuevoProducto);

                        return RedirectToAction(nameof(Productos), new { cacheBuster = Guid.NewGuid().ToString() });

                    }
                    catch (Exception ex)
                    {
                        TempData["mensaje"] = "Error: " + ex.Message;
                    }
                }
                else
                {


                    TempData["mensaje"] = "El archivo no es una imagen válida.";
                }
            }
            else
            {
                TbProducto prodbusc = db.TbProductos.Find(nuevoProducto.Idpro)!;

                var rutaImagenAnterior = Path.Combine(_env.WebRootPath, "ImagenesProductos", prodbusc.Imagen!);

                if (System.IO.File.Exists(rutaImagenAnterior))
                {
                    var nuevoNombreImagen = nuevoProducto.Nompro + nuevoProducto.Idtalla + nuevoProducto.Idcolor + ".jpg";
                    var nuevaRutaImagen = Path.Combine(_env.WebRootPath, "ImagenesProductos", nuevoNombreImagen);

                    System.IO.File.Move(rutaImagenAnterior, nuevaRutaImagen);


                    TempData["mensaje"] = await EditarProducto(nuevoProducto);
                    return RedirectToAction(nameof(Productos));
                }
            }



            lista = await GetProductos();

            var viewmodel = new ProductosVista
            {
                NuevoProductos = new TbProducto(),
                listaProductos = lista
            };
            ViewBag.tallas =
              new SelectList(await traerTallas(), "Idtalla", "Talla");
            //
            ViewBag.color =
                new SelectList(await traerColores(), "Idcolor", "Color");

            return View("Productos", viewmodel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(ProductosVista model)
        {
            try
            {
             
                    TbProducto nuevoProducto = model.NuevoProductos;

                    TempData["mensaje"] = await EliminarRestaurarProducto(model.NuevoProductos.Idpro, 1);

                    return RedirectToAction(nameof(Productos));

            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetProductos();

            var viewmodel = new ProductosVista
            {
                NuevoProductos = new TbProducto(),
                listaProductos = lista
            };

            return View("Productos", viewmodel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restaurar(ProductosVista model)
        {
            try
            {

                    TbProducto nuevoProducto = model.NuevoProductos;

                    TempData["mensaje"] = await EliminarRestaurarProducto(model.NuevoProductos.Idpro, 2);

                    return RedirectToAction(nameof(Productos));

            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetProductos();

            var viewmodel = new ProductosVista
            {
                NuevoProductos = new TbProducto(),
                listaProductos = lista
            };

            return View("Productos", viewmodel);
        }






    }
}
