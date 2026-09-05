   using Microsoft.Extensions.Configuration;
   using MySqlConnector;

   namespace Inmobiliaria.Models
   {
       public class RepositorioBase
       {
           private readonly IConfiguration _configuration;

           public RepositorioBase(IConfiguration configuration)
           {
               _configuration = configuration;
           }

           protected MySqlConnection ObtenerConexion()
           {
               var connectionString = _configuration.GetConnectionString("DefaultConnection");
               return new MySqlConnection(connectionString);
           }
       }
   }