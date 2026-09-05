   using Inmobiliaria.Models;

   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddControllersWithViews();

   builder.Services.AddScoped<IRepositorioPropietario, RepositorioPropietario>();
   builder.Services.AddScoped<IRepositorioInquilino, RepositorioInquilino>();
   builder.Services.AddScoped<IRepositorioInmueble, RepositorioInmueble>();
   builder.Services.AddScoped<IRepositorioReserva, RepositorioReserva>();
   builder.Services.AddScoped<IRepositorioTipoInmueble, RepositorioTipoInmueble>();

   var app = builder.Build();

   if (!app.Environment.IsDevelopment())
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }

   app.UseHttpsRedirection();
   app.UseRouting();
   app.UseAuthorization();
   app.MapStaticAssets();

   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}")
       .WithStaticAssets();

   app.Run();