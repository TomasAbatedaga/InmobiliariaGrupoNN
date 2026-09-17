CREATE DATABASE IF NOT EXISTS InmobiliariaNN;
USE InmobiliariaNN;

CREATE TABLE IF NOT EXISTS Propietario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(75) NOT NULL,
    Apellido VARCHAR(75) NOT NULL,
    Telefono VARCHAR(50),
    Email VARCHAR(100),
    EstadoActivo BOOLEAN DEFAULT TRUE,
    FechaAlta DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaBaja DATETIME NULL
);

CREATE TABLE IF NOT EXISTS Inquilino (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    Nombre VARCHAR(75) NOT NULL,
    Apellido VARCHAR(75) NOT NULL,
    Telefono VARCHAR(50),
    Email VARCHAR(100),
    EstadoActivo BOOLEAN DEFAULT TRUE,
    FechaAlta DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaBaja DATETIME NULL
);

CREATE TABLE IF NOT EXISTS TipoInmueble (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS Inmueble (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Direccion VARCHAR(100) NOT NULL,
    Ambientes INT NOT NULL,
    Cupo INT NOT NULL,
    PrecioPorDia DECIMAL(10,2) NOT NULL,
    Latitud DECIMAL(10,7) NOT NULL,
    Longitud DECIMAL(10,7) NOT NULL,
    PorcentajeReserva DECIMAL(5,2) NOT NULL,
    Disponible BOOLEAN DEFAULT TRUE,
    EstadoActivo BOOLEAN DEFAULT TRUE,
    FechaBaja DATETIME NULL,
    Portada VARCHAR(255) NULL,
    PropietarioId INT NOT NULL,
    TipoInmuebleId INT NOT NULL,
    FOREIGN KEY (PropietarioId) REFERENCES Propietario(Id),
    FOREIGN KEY (TipoInmuebleId) REFERENCES TipoInmueble(Id)
);

CREATE TABLE IF NOT EXISTS Imagen (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InmuebleId INT NOT NULL,
    Url VARCHAR(255) NOT NULL,
    FOREIGN KEY (InmuebleId) REFERENCES Inmueble(Id)
);

CREATE TABLE IF NOT EXISTS Reserva (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InmuebleId INT NOT NULL,
    InquilinoId INT NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    MontoPorDia DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (InmuebleId) REFERENCES Inmueble(Id),
    FOREIGN KEY (InquilinoId) REFERENCES Inquilino(Id)
);

CREATE TABLE IF NOT EXISTS Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Clave VARCHAR(255) NOT NULL,
    Avatar VARCHAR(255) NULL,
    Rol INT NOT NULL
);

CREATE TABLE IF NOT EXISTS Pago (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReservaId INT NOT NULL,
    Concepto VARCHAR(150) NOT NULL,
    FechaPago DATE NOT NULL,
    Importe DECIMAL(10,2) NOT NULL,
    EstadoActivo BOOLEAN NOT NULL DEFAULT TRUE,
    FechaAnulacion DATETIME NULL,
    INDEX IX_Pago_ReservaId_Id (ReservaId, Id),
    FOREIGN KEY (ReservaId) REFERENCES Reserva(Id)
);

-- DATOS DE PRUEBA

INSERT INTO Propietario (Dni, Nombre, Apellido, Telefono, Email)
VALUES
('11111', 'Carlos', 'Gomez', '2664111111', 'carlos.gomez@gmail.com'),
('22222', 'Maria', 'Rodriguez', '2664222222', 'maria.rod@gmail.com'),
('33333', 'Tomas', 'Abatedaga', '2664123456', 'abatedagatomas@gmail.com');

INSERT INTO Inquilino (Dni, Nombre, Apellido, Telefono, Email)
VALUES
('77777', 'Juan', 'Perez', '2664333333', 'jperez@gmail.com'),
('66666', 'Ana', 'Lopez', '2664444444', 'analopez@gmail.com'),
('55555', 'Facundo', 'Calderon', '2664555555', 'facucal@gmail.com');

INSERT INTO TipoInmueble (Nombre)
VALUES
('Casa'),
('Departamento'),
('Cabaña'),
('Local');

INSERT INTO Inmueble (
    Direccion,
    Ambientes,
    Cupo,
    PrecioPorDia,
    Latitud,
    Longitud,
    PorcentajeReserva,
    Disponible,
    PropietarioId,
    TipoInmuebleId
)
VALUES
('Av. Illia 123', 3, 5, 45000.00, -33.3017000, -66.3378000, 30.00, TRUE, 1, 2),
('Los Lapachos 450', 4, 6, 60000.00, -33.2905000, -66.3201000, 25.00, TRUE, 2, 1);

INSERT INTO Reserva (InmuebleId, InquilinoId, FechaInicio, FechaFin, MontoPorDia)
VALUES
(1, 1, '2026-10-01', '2026-10-10', 45000.00),
(2, 2, '2026-11-15', '2026-11-20', 60000.00);

INSERT INTO Pago (ReservaId, Concepto, FechaPago, Importe)
VALUES
(1, 'Seña de reserva', '2026-09-15', 135000.00),
(2, 'Pago total anticipado', '2026-09-17', 300000.00);


INSERT INTO Usuario (Nombre, Apellido, Email, Clave, Rol)
VALUES
('Admin', 'Administrador', 'admin@inmobiliaria.com', '123456', 1),
('Empleado', 'Empleado', 'empleado@inmobiliaria.com', '123456', 2);