USE InmobiliariaNN;

ALTER TABLE Inmueble ENGINE=InnoDB;
ALTER TABLE Reserva ENGINE=InnoDB;
ALTER TABLE Pago ENGINE=InnoDB;

ALTER TABLE Reserva
    ADD COLUMN FechaFinalizacion DATE NULL,
    ADD COLUMN FinalizadoPorId INT NULL,
    ADD CONSTRAINT FK_Reserva_FinalizadoPor
        FOREIGN KEY (FinalizadoPorId) REFERENCES Usuario(Id);

ALTER TABLE Pago
    ADD COLUMN CreadoPorId INT NULL,
    ADD COLUMN AnuladoPorId INT NULL,
    ADD CONSTRAINT FK_Pago_CreadoPor
        FOREIGN KEY (CreadoPorId) REFERENCES Usuario(Id),
    ADD CONSTRAINT FK_Pago_AnuladoPor
        FOREIGN KEY (AnuladoPorId) REFERENCES Usuario(Id);
