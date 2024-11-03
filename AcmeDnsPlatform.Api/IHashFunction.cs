namespace AcmeDnsPlatform.Api;

public interface IHashFunction
{
    public byte[] Hash(byte[] bytes);
}