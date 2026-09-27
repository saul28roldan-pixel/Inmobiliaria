namespace Inmobiliaria.Models
{
    public class InmuebleImagen
    {
        public int IdImagen { get; set; }
        public int IdInmueble { get; set; }
        public string RutaUrl { get; set; } = string.Empty;
        public bool EsPortada { get; set; }

        public Inmueble? Inmueble { get; set; }
    }
}