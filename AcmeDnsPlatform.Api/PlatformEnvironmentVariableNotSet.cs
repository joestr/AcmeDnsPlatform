using System.Runtime.Serialization;

namespace AcmeDnsPlatform.Api;

public class PlatformEnvironmentVariableNotSet : Exception
{
    public PlatformEnvironmentVariableNotSet()
    {
    }

    public PlatformEnvironmentVariableNotSet(string? message) : base(message)
    {
    }

    public PlatformEnvironmentVariableNotSet(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}