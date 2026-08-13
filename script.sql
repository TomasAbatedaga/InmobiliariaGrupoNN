CREATE DATABASE IF NOT EXISTS InmobiliariaNN;
USE InmobiliariaNN;

CREATE TABLE IF NOT EXISTS Propietario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    NombreCompleto VARCHAR(150) NOT NULL,
    Telefono VARCHAR(50),
    Email VARCHAR(100),
    EstadoActivo BOOLEAN DEFAULT TRUE,
    FechaAlta DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaBaja DATETIME NULL
);

CREATE TABLE IF NOT EXISTS Inquilino (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Dni VARCHAR(20) NOT NULL UNIQUE,
    NombreCompleto VARCHAR(150) NOT NULL,
    Telefono VARCHAR(50),
    Email VARCHAR(100),
    EstadoActivo BOOLEAN DEFAULT TRUE,
    FechaAlta DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaBaja DATETIME NULL
);

INSERT INTO Propietario (Dni, NombreCompleto, Telefono, Email) 
VALUES 
('11111', 'Carlos Gomez', '2664111111', 'carlos.gomez@email.com'),
('22222', 'Maria Rodriguez', '2664222222', 'maria.rod@email.com'),
('33333', 'Tomas Abatedaga', '2664123456', 'abatedagatomas@email.com');

INSERT INTO Inquilino (Dni, NombreCompleto, Telefono, Email) 
VALUES 
('77777', 'Juan Perez', '2664333333', 'jperez@email.com'),
('66666', 'Ana Lopez', '2664444444', 'analopez@email.com'),
('55555', 'Facundo Calderon', '2664555555', 'facucal@email.com');