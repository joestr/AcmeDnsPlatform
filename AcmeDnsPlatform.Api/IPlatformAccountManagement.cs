namespace AcmeDnsPlatform.Api;

public interface IPlatformAccountManagement
{
    public Account RegisterAccount(string[] allowFrom);
    
    public bool CheckCredentials(string username, string password, string ip);
}