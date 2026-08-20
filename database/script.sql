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