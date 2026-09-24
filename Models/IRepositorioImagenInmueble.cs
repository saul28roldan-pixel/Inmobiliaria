using System.Collections.Generic;

namespace Inmobiliaria.Models
{
    public interface IRepositorioImagenInmueble
    {
        int Alta(ImagenInmueble img);
        bool Baja(int idImagen);
        ImagenInmueble? ObtenerPorId(int idImagen);
        IList<ImagenInmueble> ObtenerPorInmueble(int idInmueble);

        // Deja EsPortada=1 solo en la imagen indicada y en 0 el resto de las
        // imágenes del mismo inmueble.
        bool MarcarComoPortada(int idImagen, int idInmueble);
    }
}
