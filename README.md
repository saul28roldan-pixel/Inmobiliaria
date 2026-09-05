# Inmobiliaria

Sistema de gestión de alquileres temporales, desarrollado como proyecto de la cátedra de Desarrollo de Aplicaciones Web.

## Integrantes del Grupo
- **Ontiveros José** - (GitHub: jontive21)
- **Roldan Saúl** - (GitHub: saul28roldan-pixel)
- **Orozco Miguel** - (GitHub: BLACK1895)

## Descripción
Permite administrar propietarios, inmuebles, inquilinos, reservas y pagos de una inmobiliaria dedicada a alquileres temporales. Una de las reglas centrales del sistema es que un inmueble no puede tener dos reservas que se superpongan en el tiempo.

## Tecnologías
- **.NET 10** (LTS)
- **C# 14**
- **ASP.NET Core MVC**
- **ADO.NET** con **MySqlConnector** para el acceso a datos
- **MySQL** como motor de base de datos
- **Bootstrap** para el front-end

## Entidades del dominio
| Entidad | Descripción |
|---|---|
| **Propietario** | Dueño de uno o más inmuebles |
| **Inquilino** | Quien reserva un inmueble |
| **Inmueble** | La propiedad: dirección, tipo, ambientes, precio |
| **TipoInmueble** | Clasificación del inmueble (Casa, Depto, etc.) |
| **Reserva** | Un período de alquiler: desde, hasta y monto |
| **Pago** | Cada cuota abonada de una reserva |
| **Usuario** | Quien opera el sistema (administrador o empleado) |


## Instrucciones para levantar la Base de Datos

1. Asegúrate de tener **XAMPP** instalado y el servicio **MySQL** iniciado (debe aparecer en verde).
2. Abre tu gestor de base de datos preferido (MySQL Workbench, DBeaver, HeidiSQL o phpMyAdmin).
3. Conéctate al servidor local (Usuario: `root`, Contraseña: vacía por defecto en XAMPP).
4. Abre el archivo `script_base_datos.sql` incluido en la raíz de este repositorio.
5. Ejecuta el script completo. Esto creará la base de datos `InmobiliariaDB`, todas las tablas con sus relaciones (Foreign Keys) y cargará datos de prueba iniciales.
   
   *Alternativamente, desde la terminal (PowerShell/CMD) en la carpeta del proyecto, puedes ejecutar:*
   ```bash
   mysql -u root -p < script_base_datos.sql

## Diagrama
![Diagrama](img/diagrama_bd.png)

