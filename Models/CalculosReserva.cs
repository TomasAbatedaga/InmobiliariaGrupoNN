namespace InmobiliariaGrupoNN.Models;

public static class CalculosReserva
{
    public static void ValidarMontoPorDia(decimal monto)
    {
        if (monto <= 0m || monto > 99999999.99m || decimal.Round(monto, 2) != monto)
            throw new ArgumentException("El monto por día debe estar entre 0,01 y 99.999.999,99 y tener como máximo dos decimales.");
    }

    public static void ValidarImportePago(decimal importe)
    {
        if (importe < 0.01m || importe > 99999999.99m || decimal.Round(importe, 2) != importe)
            throw new ArgumentException("El importe calculado debe estar entre 0,01 y 99.999.999,99 y tener como máximo dos decimales.");
    }

    public static decimal CalcularSena(Reserva reserva, decimal porcentaje)
    {
        ValidarMontoPorDia(reserva.MontoPorDia);
        int dias = (reserva.FechaFin.Date - reserva.FechaInicio.Date).Days;
        if (dias <= 0) throw new ArgumentException("El período de la reserva no es válido.");
        if (porcentaje < 0m || porcentaje > 100m)
            throw new ArgumentException("El porcentaje del inmueble debe estar entre 0 y 100.");
        if (porcentaje == 0m) return 0m;
        decimal importe = decimal.Round(dias * reserva.MontoPorDia * porcentaje / 100m,
            2, MidpointRounding.AwayFromZero);
        ValidarImportePago(importe);
        return importe;
    }

    public static FinalizarReservaViewModel CalcularFinalizacion(Reserva reserva, DateTime fecha, DateTime hoy)
    {
        fecha = fecha.Date;
        if (!reserva.EstadoActivo) throw new InvalidOperationException("La reserva está anulada.");
        if (reserva.FechaFinalizacion.HasValue)
            throw new InvalidOperationException("La reserva ya tiene una finalización anticipada registrada.");
        if (fecha < reserva.FechaInicio.Date || fecha < hoy.Date || fecha >= reserva.FechaFin.Date)
            throw new ArgumentException("La fecha efectiva debe ser desde hoy y desde el inicio del contrato, y anterior al fin original.");
        ValidarMontoPorDia(reserva.MontoPorDia);
        int duracion = (reserva.FechaFin.Date - reserva.FechaInicio.Date).Days;
        int transcurridos = (fecha - reserva.FechaInicio.Date).Days;
        int restantes = (reserva.FechaFin.Date - fecha).Days;
        decimal porcentaje = 2L * transcurridos < duracion ? 0.50m : 0.25m;
        decimal multa = decimal.Round(restantes * reserva.MontoPorDia * porcentaje,
            2, MidpointRounding.AwayFromZero);
        ValidarImportePago(multa);
        return new FinalizarReservaViewModel
        {
            Reserva = reserva, FechaFinalizacion = fecha, DuracionOriginal = duracion,
            DiasTranscurridos = transcurridos, DiasRestantes = restantes,
            PorcentajeMulta = porcentaje * 100m, ImporteMulta = multa, CalculoValido = true
        };
    }
}
