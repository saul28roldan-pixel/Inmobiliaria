   using Inmobiliaria.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddControllersWithViews();

   builder.Services.AddScoped<IRepositorioPropietario, RepositorioPropietario>();
   builder.Services.AddScoped<IRepositorioInquilino, RepositorioInquilino>();
   builder.Services.AddScoped<IRepositorioInmueble, RepositorioInmueble>();
   builder.Services.AddScoped<IRepositorioImagenInmueble, RepositorioImagenInmueble>();
   builder.Services.AddScoped<IRepositorioReserva, RepositorioReserva>();
   builder.Services.AddScoped<IRepositorioTipoInmueble, RepositorioTipoInmueble>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
 // Autenticación por cookies: sin esto, [Authorize] no tiene forma de
   // saber quién está logueado y la app rompe con una excepción.
   builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
       .AddCookie(options =>
       {
           options.LoginPath = "/Account/Login";
           options.AccessDeniedPath = "/Account/AccesoDenegado";
           options.ExpireTimeSpan = TimeSpan.FromHours(8);
           options.SlidingExpiration = true;
       });

   var app = builder.Build();

   if (!app.Environment.IsDevelopment())
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }

   app.UseHttpsRedirection();
   // Necesario para servir archivos subidos en runtime (ej: imágenes de Inmueble).
   // MapStaticAssets, más abajo, solo sirve lo que existía al compilar.
   app.UseStaticFiles();
   app.UseRouting();
   app.UseAuthentication();
   app.UseAuthorization();
   app.MapStaticAssets();

   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}")
       .WithStaticAssets();

   app.Run();