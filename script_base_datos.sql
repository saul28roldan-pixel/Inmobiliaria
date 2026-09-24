CREATE DATABASE IF NOT EXISTS `inmobiliariadb` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `inmobiliariadb`;

-- --------------------------------------------------------
-- 1. Tabla: tipoinmueble
-- --------------------------------------------------------
CREATE TABLE `tipoinmueble` (
  `IdTipo` int(11) NOT NULL AUTO_INCREMENT,
  `Descripcion` varchar(100) NOT NULL,
  PRIMARY KEY (`IdTipo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------
-- 2. Tabla: propietario
-- --------------------------------------------------------
CREATE TABLE `propietario` (
  `IdPropietario` int(11) NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Dni` varchar(20) NOT NULL,
  `Email` varchar(150) NOT NULL,
  `Telefono` varchar(50) NOT NULL,
  PRIMARY KEY (`IdPropietario`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------
-- 3. Tabla: inquilino
-- --------------------------------------------------------
CREATE TABLE `inquilino` (
  `IdInquilino` int(11) NOT NULL AUTO_INCREMENT,
  `Dni` varchar(20) NOT NULL,
  `NombreCompleto` varchar(150) NOT NULL,
  `Telefono` varchar(50) NOT NULL,
  `Email` varchar(150) NOT NULL,
  PRIMARY KEY (`IdInquilino`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------
-- 4. Tabla: usuario
-- --------------------------------------------------------
CREATE TABLE `usuario` (
  `IdUsuario` int(11) NOT NULL AUTO_INCREMENT,
  `Email` varchar(150) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `NombreCompleto` varchar(150) NOT NULL,
  `Rol` enum('administrador','empleado') NOT NULL DEFAULT 'empleado',
  `Avatar` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`IdUsuario`),
  UNIQUE KEY `UK_Usuario_Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------
-- 5. Tabla: inmueble
-- --------------------------------------------------------
CREATE TABLE `inmueble` (
  `IdInmueble` int(11) NOT NULL AUTO_INCREMENT,
  `IdPropietario` int(11) NOT NULL,
  `IdTipo` int(11) NOT NULL,
  `Direccion` varchar(255) NOT NULL,
  `Cupo` int(11) NOT NULL,
  `Coordenadas` varchar(100) DEFAULT NULL,
  `PrecioPorDia` decimal(10,2) NOT NULL,
  `ImagenPortada` varchar(255) DEFAULT NULL,
  `Disponible` tinyint(1) NOT NULL DEFAULT 1,
  PRIMARY KEY (`IdInmueble`),
  KEY `FK_Inmueble_Propietario` (`IdPropietario`),
  KEY `FK_Inmueble_Tipo` (`IdTipo`),
  CONSTRAINT `FK_Inmueble_Propietario` FOREIGN KEY (`IdPropietario`) REFERENCES `propietario` (`IdPropietario`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_Inmueble_Tipo` FOREIGN KEY (`IdTipo`) REFERENCES `tipoinmueble` (`IdTipo`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------
-- 6. Tabla: inmuebleimagen 
-- --------------------------------------------------------
CREATE TABLE `inmuebleimagen` (
  `IdImagen` int(11) NOT NULL AUTO_INCREMENT,
  `IdInmueble` int(11) NOT NULL,
  `RutaUrl` varchar(255) NOT NULL,
  `EsPortada` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`IdImagen`),
  KEY `FK_InmuebleImagen_Inmueble` (`IdInmueble`),
  CONSTRAINT `FK_InmuebleImagen_Inmueble` FOREIGN KEY (`IdInmueble`) REFERENCES `inmueble` (`IdInmueble`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------
-- 7. Tabla: reserva
-- --------------------------------------------------------
CREATE TABLE `reserva` (
  `IdReserva` int(11) NOT NULL AUTO_INCREMENT,
  `IdInquilino` int(11) NOT NULL,
  `IdInmueble` int(11) NOT NULL,
  `IdUsuarioCreacion` int(11) NOT NULL,
  `FechaDesde` date NOT NULL,
  `FechaHasta` date NOT NULL,
  `MontoDiario` decimal(10,2) NOT NULL,
  `FechaFinalizacion` date DEFAULT NULL,
  `Multa` decimal(10,2) DEFAULT NULL,
  `IdUsuarioFinalizacion` int(11) DEFAULT NULL,
  PRIMARY KEY (`IdReserva`),
  KEY `FK_Reserva_Inquilino` (`IdInquilino`),
  KEY `FK_Reserva_Inmueble` (`IdInmueble`),
  KEY `FK_Reserva_UsuarioCreacion` (`IdUsuarioCreacion`),
  KEY `FK_Reserva_UsuarioFinalizacion` (`IdUsuarioFinalizacion`),
  CONSTRAINT `FK_Reserva_Inmueble` FOREIGN KEY (`IdInmueble`) REFERENCES `inmueble` (`IdInmueble`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_Reserva_Inquilino` FOREIGN KEY (`IdInquilino`) REFERENCES `inquilino` (`IdInquilino`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_Reserva_UsuarioCreacion` FOREIGN KEY (`IdUsuarioCreacion`) REFERENCES `usuario` (`IdUsuario`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_Reserva_UsuarioFinalizacion` FOREIGN KEY (`IdUsuarioFinalizacion`) REFERENCES `usuario` (`IdUsuario`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- --------------------------------------------------------
-- 8. Tabla: pago
-- --------------------------------------------------------
CREATE TABLE `pago` (
  `IdPago` int(11) NOT NULL AUTO_INCREMENT,
  `IdReserva` int(11) NOT NULL,
  `IdUsuarioCreacion` int(11) NOT NULL,
  `Concepto` varchar(255) NOT NULL,
  `FechaPago` date NOT NULL,
  `Importe` decimal(10,2) NOT NULL,
  `Anulado` tinyint(1) NOT NULL DEFAULT 0,
  `IdUsuarioAnulacion` int(11) DEFAULT NULL,
  `FechaAnulacion` datetime DEFAULT NULL,
  PRIMARY KEY (`IdPago`),
  KEY `FK_Pago_Reserva` (`IdReserva`),
  KEY `FK_Pago_UsuarioCreacion` (`IdUsuarioCreacion`),
  KEY `FK_Pago_UsuarioAnulacion` (`IdUsuarioAnulacion`),
  CONSTRAINT `FK_Pago_Reserva` FOREIGN KEY (`IdReserva`) REFERENCES `reserva` (`IdReserva`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_Pago_UsuarioAnulacion` FOREIGN KEY (`IdUsuarioAnulacion`) REFERENCES `usuario` (`IdUsuario`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `FK_Pago_UsuarioCreacion` FOREIGN KEY (`IdUsuarioCreacion`) REFERENCES `usuario` (`IdUsuario`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Usuario inicial
INSERT INTO `usuario` (`IdUsuario`, `Email`, `PasswordHash`, `NombreCompleto`, `Rol`) 
VALUES (1, 'admin@inmobiliaria.com', 'hash_provisorio', 'Administrador General', 'administrador');