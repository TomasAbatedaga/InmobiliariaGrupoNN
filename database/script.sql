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
    PropietarioId INT NOT NULL,
    TipoInmuebleId INT NOT NULL,

    FOREIGN KEY (PropietarioId)
        REFERENCES Propietario(Id),

    FOREIGN KEY (TipoInmuebleId)
        REFERENCES TipoInmueble(Id)
);

CREATE TABLE IF NOT EXISTS Reserva (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    InmuebleId INT NOT NULL,
    InquilinoId INT NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaFin DATE NOT NULL,
    Monto DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (InmuebleId) REFERENCES Inmueble(Id),
    FOREIGN KEY (InquilinoId) REFERENCES Inquilino(Id)
);