DROP DATABASE IF EXISTS InmobiliariaNN;
CREATE DATABASE IF NOT EXISTS InmobiliariaNN;
USE InmobiliariaNN;

-- 1. TABLAS INDEPENDIENTES
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

CREATE TABLE IF NOT EXISTS Usuario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Apellido VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Clave VARCHAR(255) NOT NULL,
    Avatar VARCHAR(255) NULL,
    Rol INT NOT NULL
);

-- 2. TABLAS DEPENDIENTES
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
    CreadoPorId INT NULL,
    AnuladoPorId INT NULL,
    EstadoActivo BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (InmuebleId) REFERENCES Inmueble(Id),
    FOREIGN KEY (InquilinoId) REFERENCES Inquilino(Id),
    FOREIGN KEY (CreadoPorId) REFERENCES Usuario(Id),
    FOREIGN KEY (AnuladoPorId) REFERENCES Usuario(Id)
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


-- ==========================================
-- INSERTS DE PRUEBA (MÁS DE 10 REGISTROS)
-- ==========================================

-- Usuarios Base
INSERT INTO Usuario (Nombre, Apellido, Email, Clave, Rol) VALUES
('Admin', 'Administrador', 'admin@inmobiliaria.com', '123456', 1),
('Empleado', 'Empleado', 'empleado@inmobiliaria.com', '123456', 2);

-- 12 Propietarios
INSERT INTO Propietario (Dni, Nombre, Apellido, Telefono, Email) VALUES
('11111111', 'Carlos', 'Gomez', '2664111111', 'carlos.gomez@gmail.com'),
('22222222', 'Maria', 'Rodriguez', '2664222222', 'maria.rod@gmail.com'),
('33333333', 'Tomas', 'Abatedaga', '2664123456', 'abatedagatomas@gmail.com'),
('44444444', 'Lucia', 'Fernandez', '2664444444', 'lucia.f@gmail.com'),
('55555555', 'Roberto', 'Martinez', '2664555555', 'roberto.m@gmail.com'),
('66666666', 'Laura', 'Gimenez', '2664666666', 'laura.g@gmail.com'),
('77777777', 'Esteban', 'Quito', '2664777777', 'esteban.q@gmail.com'),
('88888888', 'Florencia', 'Peña', '2664888888', 'flor.p@gmail.com'),
('99999999', 'Diego', 'Maradona', '2664999999', 'diego.m@gmail.com'),
('10101010', 'Lionel', 'Messi', '2664101010', 'lionel.m@gmail.com'),
('12121212', 'Marta', 'Minujin', '2664121212', 'marta.m@gmail.com'),
('13131313', 'Susana', 'Gimenez', '2664131313', 'susana.g@gmail.com');

-- 12 Inquilinos
INSERT INTO Inquilino (Dni, Nombre, Apellido, Telefono, Email) VALUES
('14141414', 'Juan', 'Perez', '2664141414', 'jperez@gmail.com'),
('15151515', 'Ana', 'Lopez', '2664151515', 'analopez@gmail.com'),
('16161616', 'Facundo', 'Calderon', '2664161616', 'facucal@gmail.com'),
('17171717', 'Pedro', 'Alfonso', '2664171717', 'pedro.a@gmail.com'),
('18181818', 'Carla', 'Peterson', '2664181818', 'carla.p@gmail.com'),
('19191919', 'Gaston', 'Pauls', '2664191919', 'gaston.p@gmail.com'),
('20202020', 'Nancy', 'Duplaa', '2664202020', 'nancy.d@gmail.com'),
('21212121', 'Pablo', 'Echarri', '2664212121', 'pablo.e@gmail.com'),
('23232323', 'Guillermo', 'Francella', '2664232323', 'guille.f@gmail.com'),
('24242424', 'Ricardo', 'Darin', '2664242424', 'ricardo.d@gmail.com'),
('25252525', 'Erica', 'Rivas', '2664252525', 'erica.r@gmail.com'),
('26262626', 'Marcelo', 'Tinelli', '2664262626', 'marcelo.t@gmail.com');

-- Tipos de Inmueble
INSERT INTO TipoInmueble (Nombre) VALUES ('Casa'), ('Departamento'), ('Cabaña'), ('Local'), ('Oficina'), ('Galpón');

-- 12 Inmuebles
INSERT INTO Inmueble (Direccion, Ambientes, Cupo, PrecioPorDia, Latitud, Longitud, PorcentajeReserva, Disponible, PropietarioId, TipoInmuebleId) VALUES
('Av. Illia 123', 3, 5, 45000.00, -33.3017, -66.3378, 30.00, TRUE, 1, 2),
('Los Lapachos 450', 4, 6, 60000.00, -33.2905, -66.3201, 25.00, TRUE, 2, 1),
('San Martin 850', 2, 2, 35000.00, -33.3022, -66.3360, 20.00, TRUE, 3, 2),
('Rivadavia 1200', 5, 8, 80000.00, -33.3000, -66.3350, 40.00, TRUE, 4, 1),
('Colon 45', 1, 1, 20000.00, -33.3050, -66.3400, 50.00, TRUE, 5, 5),
('Pringles 999', 3, 4, 50000.00, -33.2980, -66.3300, 30.00, TRUE, 6, 4),
('Belgrano 333', 6, 10, 120000.00, -33.2950, -66.3250, 20.00, TRUE, 7, 3),
('Chacabuco 777', 2, 3, 40000.00, -33.3080, -66.3450, 35.00, TRUE, 8, 2),
('Mitre 150', 4, 5, 55000.00, -33.3010, -66.3380, 25.00, TRUE, 9, 1),
('Junin 500', 1, 2, 25000.00, -33.3040, -66.3320, 50.00, TRUE, 10, 2),
('Lavalle 222', 8, 15, 200000.00, -33.2900, -66.3100, 10.00, TRUE, 11, 6),
('Las Heras 666', 3, 4, 48000.00, -33.2970, -66.3280, 30.00, TRUE, 12, 1);

-- 12 Reservas (Mezcla de fechas pasadas, actuales y futuras para probar los colores de los Badges)
INSERT INTO Reserva (InmuebleId, InquilinoId, FechaInicio, FechaFin, MontoPorDia, CreadoPorId) VALUES
(1, 1, '2023-01-01', '2023-01-15', 45000.00, 1), -- Pasada
(2, 2, '2023-05-10', '2023-05-20', 60000.00, 2), -- Pasada
(3, 3, DATE_ADD(CURDATE(), INTERVAL -5 DAY), DATE_ADD(CURDATE(), INTERVAL 5 DAY), 35000.00, 1), -- En Curso
(4, 4, DATE_ADD(CURDATE(), INTERVAL -2 DAY), DATE_ADD(CURDATE(), INTERVAL 10 DAY), 80000.00, 2), -- En Curso
(5, 5, DATE_ADD(CURDATE(), INTERVAL 10 DAY), DATE_ADD(CURDATE(), INTERVAL 20 DAY), 20000.00, 1), -- Futura
(6, 6, '2026-12-01', '2026-12-15', 50000.00, 2), -- Futura
(7, 7, '2027-01-10', '2027-01-25', 120000.00, 1), -- Futura
(8, 8, '2027-02-05', '2027-02-15', 40000.00, 2), -- Futura
(9, 9, '2027-03-01', '2027-03-10', 55000.00, 1), -- Futura
(10, 10, '2027-04-15', '2027-04-30', 25000.00, 2), -- Futura
(11, 11, '2027-05-01', '2027-05-20', 200000.00, 1), -- Futura
(12, 12, '2027-06-10', '2027-06-25', 48000.00, 2); -- Futura

-- Algunos Pagos de prueba
INSERT INTO Pago (ReservaId, Concepto, FechaPago, Importe) VALUES
(1, 'Pago Total Contrato Finalizado', '2023-01-01', 675000.00),
(2, 'Pago Total Contrato Finalizado', '2023-05-10', 600000.00),
(3, 'Seña Inicial', DATE_ADD(CURDATE(), INTERVAL -10 DAY), 100000.00),
(4, 'Seña Inicial', DATE_ADD(CURDATE(), INTERVAL -5 DAY), 200000.00);