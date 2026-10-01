-- ==========================================================
--  INMOBILIARIA - Script único de base de datos
--  Se puede correr varias veces sin romper nada:
--    * crea lo que falta (tablas con IF NOT EXISTS)
--    * no duplica datos de prueba
--    * deja los usuarios de prueba con contraseña hasheada
-- ==========================================================

CREATE DATABASE IF NOT EXISTS InmobiliariaDB
    CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE InmobiliariaDB;

-- 1. Tipos de inmueble (para el ABM de tipos)
CREATE TABLE IF NOT EXISTS TipoInmueble (
    IdTipo INT AUTO_INCREMENT PRIMARY KEY,
    Descripcion VARCHAR(100) NOT NULL UNIQUE
);

-- 2. Propietarios
CREATE TABLE IF NOT EXISTS Propietario (
    IdPropietario INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Email VARCHAR(150),
    Telefono VARCHAR(50)
);

-- 3. Inquilinos
CREATE TABLE IF NOT EXISTS Inquilino (
    IdInquilino INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    NombreCompleto VARCHAR(150) NOT NULL,
    Telefono VARCHAR(50),
    Email VARCHAR(150)
);

-- 4. Usuarios del sistema (roles: administrador, empleado)
CREATE TABLE IF NOT EXISTS Usuario (
    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    NombreCompleto VARCHAR(150) NOT NULL,
    Rol ENUM('administrador', 'empleado') NOT NULL,
    Avatar VARCHAR(255)
);

-- 5. Inmuebles
CREATE TABLE IF NOT EXISTS Inmueble (
    IdInmueble INT AUTO_INCREMENT PRIMARY KEY,
    IdPropietario INT NOT NULL,
    IdTipo INT NOT NULL,
    Direccion VARCHAR(255) NOT NULL,
    Cupo INT NOT NULL COMMENT 'Cantidad máxima de personas',
    Coordenadas VARCHAR(100),
    PrecioPorDia DECIMAL(10, 2) NOT NULL,
    ImagenPortada VARCHAR(255),
    Disponible BOOLEAN DEFAULT TRUE COMMENT 'TRUE = disponible, FALSE = suspendido por el propietario',
    FOREIGN KEY (IdPropietario) REFERENCES Propietario(IdPropietario),
    FOREIGN KEY (IdTipo) REFERENCES TipoInmueble(IdTipo)
);

-- 6. Galería de imágenes de cada inmueble
--    (la usa RepositorioImagenInmueble; el nombre va en minúsculas
--     porque así está escrito en el código)
--    ON DELETE CASCADE: al borrar un inmueble se borran sus filas de imágenes.
CREATE TABLE IF NOT EXISTS inmuebleimagen (
    IdImagen INT AUTO_INCREMENT PRIMARY KEY,
    IdInmueble INT NOT NULL,
    RutaUrl VARCHAR(255) NOT NULL,
    EsPortada BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (IdInmueble) REFERENCES Inmueble(IdInmueble) ON DELETE CASCADE
);

-- 7. Reservas
CREATE TABLE IF NOT EXISTS Reserva (
    IdReserva INT AUTO_INCREMENT PRIMARY KEY,
    IdInquilino INT NOT NULL,
    IdInmueble INT NOT NULL,
    IdUsuarioCreacion INT NOT NULL,
    FechaDesde DATE NOT NULL,
    FechaHasta DATE NOT NULL,
    MontoDiario DECIMAL(10, 2) NOT NULL,
    FechaFinalizacion DATE COMMENT 'Fecha real de finalización si termina antes',
    Multa DECIMAL(10, 2) COMMENT 'Multa por terminación anticipada',
    IdUsuarioFinalizacion INT COMMENT 'Usuario que finalizó la reserva',
    FOREIGN KEY (IdInquilino) REFERENCES Inquilino(IdInquilino),
    FOREIGN KEY (IdInmueble) REFERENCES Inmueble(IdInmueble),
    FOREIGN KEY (IdUsuarioCreacion) REFERENCES Usuario(IdUsuario),
    FOREIGN KEY (IdUsuarioFinalizacion) REFERENCES Usuario(IdUsuario)
);

-- 8. Pagos de cada reserva
CREATE TABLE IF NOT EXISTS Pago (
    IdPago INT AUTO_INCREMENT PRIMARY KEY,
    IdReserva INT NOT NULL,
    IdUsuarioCreacion INT NOT NULL,
    Concepto VARCHAR(255) NOT NULL,
    FechaPago DATE NOT NULL,
    Importe DECIMAL(10, 2) NOT NULL,
    Anulado BOOLEAN DEFAULT FALSE COMMENT 'TRUE si el pago fue anulado',
    IdUsuarioAnulacion INT COMMENT 'Usuario que anuló el pago',
    FechaAnulacion DATETIME,
    FOREIGN KEY (IdReserva) REFERENCES Reserva(IdReserva),
    FOREIGN KEY (IdUsuarioCreacion) REFERENCES Usuario(IdUsuario),
    FOREIGN KEY (IdUsuarioAnulacion) REFERENCES Usuario(IdUsuario)
);

-- ==========================================================
--  DATOS DE PRUEBA
--  (INSERT IGNORE: si ya existen, los salta sin dar error)
-- ==========================================================
INSERT IGNORE INTO TipoInmueble (Descripcion) VALUES
('Casa'), ('Departamento'), ('Monoambiente'), ('Loft');

INSERT IGNORE INTO Propietario (Nombre, Apellido, Dni, Email, Telefono) VALUES
('Juan', 'Pérez', '20123456', 'juan.perez@email.com', '11-1234-5678');

INSERT IGNORE INTO Inquilino (Dni, NombreCompleto, Telefono, Email) VALUES
('30111222', 'Carlos López', '11-2222-3333', 'carlos.lopez@email.com');

-- Usuarios de prueba. Contraseñas hasheadas con PBKDF2 (formato de PasswordHelper.cs):
--   admin@inmobiliaria.com    -> admin123
--   empleado@inmobiliaria.com -> empleado123
-- Si el usuario ya existía (por ejemplo con el hash viejo), se le actualiza la contraseña.
INSERT INTO Usuario (Email, PasswordHash, NombreCompleto, Rol) VALUES
('admin@inmobiliaria.com',
 '100000.M/py3J3QEDF68Aax9+sRIA==.eej0dMdewEO25vySpoYcc9NfED0SvljdXhxUwRH4L4A=',
 'Administrador Principal', 'administrador'),
('empleado@inmobiliaria.com',
 '100000.E/J04IF02pVM69pWuNDZ5A==.VpXoCZykRfQlMoGIhpoCIF5baY+qcU05uoL+eBYlJmM=',
 'Empleado de Prueba', 'empleado')
ON DUPLICATE KEY UPDATE PasswordHash = VALUES(PasswordHash);