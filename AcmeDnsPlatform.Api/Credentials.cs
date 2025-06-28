namespace AcmeDnsPlatform.Api;

public class Credentials
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public List<string> AllowFrom { get; set; } =  new List<string>();

}