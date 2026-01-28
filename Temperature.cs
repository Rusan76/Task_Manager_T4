using System;
using LibreHardwareMonitor.Hardware;
using Spectre.Console;

public class AdvancedTemperatureMonitor
{
    private static Computer _computer;
    
    public static void Initialize()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = false,
            IsStorageEnabled = true
        };
        
        _computer.Open();
    }
    
    public static void ShowAllTemperatures()
    {
        Initialize();
        
        var table = new Table()
            .Title("[bold red]🌡️ Hardware Temperatures[/]")
            .BorderColor(Color.Red)
            .Border(TableBorder.Rounded)
            .AddColumn(new TableColumn("[cyan]Sensor[/]").LeftAligned())
            .AddColumn(new TableColumn("[cyan]Type[/]").Centered())
            .AddColumn(new TableColumn("[cyan]Temperature[/]").RightAligned())
            .AddColumn(new TableColumn("[cyan]Status[/]").Centered());
        
        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();
            
            foreach (var sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    double temp = sensor.Value.Value;
                    string status = GetTemperatureStatus(temp, hardware.HardwareType);
                    Color color = GetStatusColor(status);
                    
                    table.AddRow(
                        $"[white]{sensor.Name}[/]",
                        $"[grey]{hardware.HardwareType}[/]",
                        $"[{color.ToMarkup()}]{temp:F1}°C[/]",
                        $"[{color.ToMarkup()}]{status}[/]"
                    );
                }
            }
        }
        
        AnsiConsole.Write(table);
        
        
        _computer.Close();
        Console.ReadKey();
    }
    
    private static string GetTemperatureStatus(double temp, HardwareType type)
    {
        double threshold = type switch
        {
            HardwareType.Cpu => 80,
            HardwareType.GpuNvidia => 85,
            HardwareType.GpuAmd => 85,
            HardwareType.Storage => 60,
            HardwareType.Motherboard => 70,
            _ => 75
        };
        
        if (temp > threshold + 10) return "CRITICAL";
        if (temp > threshold) return "HIGH";
        if (temp > threshold - 20) return "NORMAL";
        return "LOW";
    }
    
    private static Color GetStatusColor(string status)
    {
        return status switch
        {
            "CRITICAL" => Color.Red,
            "HIGH" => Color.Yellow,
            "NORMAL" => Color.Green,
            "LOW" => Color.Blue,
            _ => Color.Grey
        };
    }
}