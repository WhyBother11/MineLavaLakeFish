namespace MineLavaLakeFish.HarmonyUtil;

public class ModHarmonyException : Exception
{
    public HarmonyExceptionSeverity Severity { get; set; }
    
    public ModHarmonyException(string message, HarmonyExceptionSeverity severity = HarmonyExceptionSeverity.SEVERE)
        : base(message)
    {
        Severity = severity;
    }

    public ModHarmonyException(string message, Exception innerException, HarmonyExceptionSeverity severity = HarmonyExceptionSeverity.SEVERE)
        : base(message, innerException)
    {
        Severity = severity;
    }
    
}

public enum HarmonyExceptionSeverity
{
    MINOR, SEVERE
}