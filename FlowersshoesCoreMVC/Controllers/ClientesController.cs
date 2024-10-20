using FlowersshoesCoreMVC.Models;
using FlowersshoesCoreMVC.Models.Vistas;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace FlowersshoesCoreMVC.Controllers
{
    public class ClientesController : Controller
    {
        List<TbCliente> lista = new List<TbCliente>();

        public async Task<List<TbCliente>> GetClientes()
        {

            using (var httpcliente = new HttpClient())
            {

                var respuesta =
                    await httpcliente.GetAsync("http://localhost:5050/api/Clientes/GetClientes");
                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<TbCliente>>(respuestaAPI)!;
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


        public async Task<string> crearCliente(TbCliente obj)
        {
            string cadena = string.Empty;


            using (var httpcliente = new HttpClient())
            {

                StringContent contenido = new StringContent(
                   JsonConvert.SerializeObject(obj), Encoding.UTF8,
                          "application/json");

                HttpResponseMessage respuesta = new HttpResponseMessage();
                respuesta =
                        await httpcliente.PostAsync("http://localhost:5050/api/Clientes/GrabarClientes", contenido);

                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                cadena = respuestaAPI;
            }
            return cadena;
        }

        public async Task<string> EditarCliente(TbCliente obj)
        {
            string cadena = string.Empty;


            using (var httpcliente = new HttpClient())
            {

                StringContent contenido = new StringContent(
                   JsonConvert.SerializeObject(obj), Encoding.UTF8,
                          "application/json");

                HttpResponseMessage respuesta = new HttpResponseMessage();
                respuesta =
                   await httpcliente.PutAsync("http://localhost:5050/api/Clientes/ActualizarClientes", contenido);

                string respuestaAPI = await respuesta.Content.ReadAsStringAsync();
                cadena = respuestaAPI;
            }
            return cadena;
        }



        public async Task<string> EliminarRestaurarCliente(int id, int option)
        {
            string cadena = string.Empty;

            using (var httpClient = new HttpClient())
            {
                if (option == 1)
                {
                    HttpResponseMessage respuesta = await httpClient.DeleteAsync($"http://localhost:5050/api/Clientes/EliminarCliente/{id}");
                    cadena = await respuesta.Content.ReadAsStringAsync();
                }
                else
                {
                    HttpResponseMessage respuesta = await httpClient.DeleteAsync($"http://localhost:5050/api/Clientes/RestaurarCliente/{id}");
                    cadena = await respuesta.Content.ReadAsStringAsync();
                }
            }

            return cadena;
        }



        [HttpGet]
        public async Task<IActionResult> Clientes(int id, string accion)
        {
            trabajadorActual = RecuperarTrabajador()!;

            if (trabajadorActual != null)
            {
                ViewBag.trabajador = trabajadorActual;
                ViewBag.rolTrabajador = trabajadorActual.Idrol;
            }

            lista = await GetClientes();
            ClientesVista viewmodel;
            ViewBag.abrirModal = "No";

            if (id == 0)
            {
                viewmodel = new ClientesVista
                {
                    NuevoClientes = new TbCliente(),
                    listaClientes = lista
                };
            }
            else
            {
                viewmodel = new ClientesVista
                {
                    NuevoClientes = lista.Find(c => c.Idcli == id)!,
                    listaClientes = lista
                };
                ViewBag.abrirModal = accion;
            }



            return View(viewmodel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(ClientesVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbCliente nuevoCliente = model.NuevoClientes;

                    TempData["mensaje"] = await crearCliente(nuevoCliente);

                    return RedirectToAction(nameof(Clientes));
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

            lista = await GetClientes();

            var viewmodel = new ClientesVista
            {
                NuevoClientes = new TbCliente(),
                listaClientes = lista
            };

            return View("Clientes", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ClientesVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbCliente nuevoCliente = model.NuevoClientes;

                    TempData["mensaje"] = await EditarCliente(nuevoCliente);

                    return RedirectToAction(nameof(Clientes));
                }
                else
                {
                    TempData["mensaje"] = "No se pudo editar el registro, intentalo nuevamente";
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetClientes();

            var viewmodel = new ClientesVista
            {
                NuevoClientes = new TbCliente(),
                listaClientes = lista
            };

            return View("Clientes", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(ClientesVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbCliente nuevoCliente = model.NuevoClientes;

                    TempData["mensaje"] = await EliminarRestaurarCliente(model.NuevoClientes.Idcli, 1);

                    return RedirectToAction(nameof(Clientes));
                }
                else
                {
                    TempData["mensaje"] = "No se pudo editar el registro, intentalo nuevamente";
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetClientes();

            var viewmodel = new ClientesVista
            {
                NuevoClientes = new TbCliente(),
                listaClientes = lista
            };

            return View("Clientes", viewmodel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restaurar(ClientesVista model)
        {
            try
            {
                if (ModelState.IsValid == true)
                {
                    TbCliente nuevoCliente = model.NuevoClientes;

                    TempData["mensaje"] = await EliminarRestaurarCliente(model.NuevoClientes.Idcli, 2);

                    return RedirectToAction(nameof(Clientes));
                }
                else
                {
                    TempData["mensaje"] = "No se pudo editar el registro, intentalo nuevamente";
                }
            }
            catch (Exception ex)
            {
                TempData["mensaje"] = "Error: " + ex.Message;
            }

            lista = await GetClientes();

            var viewmodel = new ClientesVista
            {
                NuevoClientes = new TbCliente(),
                listaClientes = lista
            };

            return View("Clientes", viewmodel);
        }




    }
}