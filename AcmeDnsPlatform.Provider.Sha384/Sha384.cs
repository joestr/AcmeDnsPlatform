using System.Security.Cryptography;
using AcmeDnsPlatform.Api;

namespace AcmeDnsPlatform.Provider.Sha384Provider;

public class Sha384 : IHashFunction
{
    public byte[] Hash(byte[] bytes)
    {
        return SHA3_384.HashData(bytes);
    }
}