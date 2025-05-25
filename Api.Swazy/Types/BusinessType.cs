using System.Text.Json.Serialization;

namespace Api.Swazy.Types;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BusinessType
{
    None = 0,
    Other = 1,
    BarberSalon = 2,
    HairSalon = 3,
    MassageSalon = 4,
    BeautySalon = 5,
    NailSalon = 6,
    Trainer = 7,
    ServiceProvider = 8,
    Restaurant = 9
}
