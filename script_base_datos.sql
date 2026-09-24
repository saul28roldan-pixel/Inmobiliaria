CREATE DATABASE IF NOT EXISTS InmobiliariaDB;
USE InmobiliariaDB;

-- 1. Tabla TipoInmueble (para el ABM de tipos)
CREATE TABLE TipoInmueble (
    IdTipo INT AUTO_INCREMENT PRIMARY KEY,
    Descripcion VARCHAR(100) NOT NULL UNIQUE
);

-- 2. Tabla Propietario
CREATE TABLE Propietario (
    IdPropietario INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Apellido VARCHAR(100) NOT NULL,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Email VARCHAR(150),
    Telefono VARCHAR(50)
);

-- 3. Tabla Inquilino
CREATE TABLE Inquilino (
    IdInquilino INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    NombreCompleto VARCHAR(150) NOT NULL,
    Telefono VARCHAR(50),
    Email VARCHAR(150)
);

-- 4. Tabla Usuario (Roles: administrador, empleado)
CREATE TABLE Usuario (
    IdUsuario INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(150) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    NombreCompleto VARCHAR(150) NOT NULL,
    Rol ENUM('administrador', 'empleado') NOT NULL,
    Avatar VARCHAR(255)
);

-- 5. Tabla Inmueble
CREATE TABLE Inmueble (
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

-- 6. Tabla Reserva
CREATE TABLE Reserva (
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

-- 7. Tabla Pago
CREATE TABLE Pago (
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

-- ==========================================
-- DATOS DE PRUEBA INICIALES
-- ==========================================
INSERT INTO TipoInmueble (Descripcion) VALUES 
('Casa'), ('Departamento'), ('Monoambiente'), ('Loft');

INSERT INTO Propietario (Nombre, Apellido, Dni, Email, Telefono) VALUES 
('Juan', 'Pérez', '20123456', 'juan.perez@email.com', '11-1234-5678');

INSERT INTO Inquilino (Dni, NombreCompleto, Telefono, Email) VALUES 
('30111222', 'Carlos López', '11-2222-3333', 'carlos.lopez@email.com');

INSERT INTO Usuario (Email, PasswordHash, NombreCompleto, Rol) VALUES 
('admin@inmobiliaria.com', 'hash_de_contraseña', 'Administrador Principal', 'administrador');
