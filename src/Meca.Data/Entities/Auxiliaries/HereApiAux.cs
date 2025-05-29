using System.Collections.Generic;

public class HereApiResponse
{
    public List<Route> Routes { get; set; }
}

public class Route
{
    public List<Section> Sections { get; set; }
}

public class Section
{
    public int Duration { get; set; } // Em segundos
    public int Distance { get; set; } // Em metros
    public Coordinates Start { get; set; }
    public Coordinates Arrival { get; set; }
}

public class Coordinates
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}