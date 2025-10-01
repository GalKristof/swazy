namespace Api.Swazy.Common;

public static class SwazyConstants
{
    public const byte JwtTokenClockSkewSeconds = 30;
    public const ushort JwtAccessTokenLifetimeMinutes = 24 * 60;
    public const byte HashSaltWorkforce = 13;
    public const string JwtOptionsSectionName = "JwtOptions";
    public const string DatabaseOptionsSectionName = "PostgresConnection";
    
    public const string BookingModuleName = "Booking";
    public static readonly string BookingModuleApi = BookingModuleName.ToLower();
    
    public const string BusinessModuleName = "Business";
    public static readonly string BusinessModuleApi = BusinessModuleName.ToLower();
    
    public const string ServiceModuleName = "Service";
    public static readonly string ServiceModuleApi = ServiceModuleName.ToLower();
    
    public const string UserModuleName = "User";
    public static readonly string UserModuleApi = UserModuleName.ToLower();
    
    public const string AuthModuleName = "Auth";
    public static readonly string AuthModuleApi = UserModuleName.ToLower();

    public const string BusinessServiceModuleName = "BusinessService";
    public static readonly string BusinessServiceModuleApi = BusinessServiceModuleName.ToLower();

    public const string EmployeeScheduleModuleName = "EmployeeSchedule";
    public static readonly string EmployeeScheduleModuleApi = EmployeeScheduleModuleName.ToLower();
}
