using Microsoft.CodeAnalysis.Scripting;

namespace FlowersshoesCoreMVC.Models.Vistas
{
    public class TallasVista
    {
        public TbTalla NuevaTalla { get; set; } = new TbTalla();
        public IEnumerable<TbTalla> listaTallas { get; set; } = Enumerable.Empty<TbTalla>();
    }
}
