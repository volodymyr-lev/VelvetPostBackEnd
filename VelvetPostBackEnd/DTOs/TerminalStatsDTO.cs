namespace VelvetPostBackEnd.DTOs;

public class TerminalOverviewDTO
{
    public TerminalDataDTO TerminalData { get; set; }
    public List<ShipmentDataDTO> ShipmentData { get; set; }
    public List<TerminalShipmentTypeDTO> ShipmentTypeData { get; set; }
    public List<StatusDataDTO> StatusData { get; set; }
    public List<TimeDistributionDTO> TimeDistribution { get; set; }
    public List<PerformanceMetricDTO> PerformanceMetrics { get; set; }
    public List<DeliveryTargetDTO> DeliveryTargets { get; set; }
    public List<ParcelAnalysisDTO> ParcelAnalysis { get; set; }
}


public class TerminalDataDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Type { get; set; }
    public List<PostOfficeDTO> PostOffices { get; set; }
}


public class ShipmentDataDTO
{
    public string Date { get; set; }
    public int Incoming { get; set; }
    public int Outgoing { get; set; }
}


public class TerminalShipmentTypeDTO
{
    public string Name { get; set; }
    public int Value { get; set; }
}


public class StatusDataDTO
{
    public string Name { get; set; }
    public int Value { get; set; }
}


public class TimeDistributionDTO
{
    public string Name { get; set; }
    public int Sent { get; set; }
    public int Received { get; set; }
}


public class PerformanceMetricDTO
{
    public string Subject { get; set; }
    public int A { get; set; }
    public int FullMark { get; set; }
}

public class DeliveryTargetDTO
{
    public string Date { get; set; }
    public int Plan { get; set; }
    public int Fact { get; set; }
}

public class ParcelAnalysisDTO
{
    public int Id { get; set; }
    public double Weight { get; set; }
    public double Price { get; set; }
    public string Type { get; set; }
}