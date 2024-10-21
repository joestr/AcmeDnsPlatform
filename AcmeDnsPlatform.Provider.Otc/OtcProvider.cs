using HuaweiCloud.SDK.Core;
using HuaweiCloud.SDK.Core.Auth;
using HuaweiCloud.SDK.Dns.V2;
using HuaweiCloud.SDK.Dns.V2.Model;

namespace AcmeDnsPlatform.Provider.Otc;

public class OtcProvider
{
    public static void Test()
    {
        // Configure authentication
        // Authentication can be configured through environment variables and other methods. Please refer to Chapter 2.3 Authentication Management
        var ak = "n/a";
        var sk = "n/a";
        var domainId = "n/a";
        var projectId = "n/a";
        var auth = new BasicCredentials(ak, sk, projectId);

        // Create a service client
        var client = DnsClient.NewBuilder()
            .WithEndPoints(new List<string>(){ "n/a" })
            .WithCredential(auth)
            .WithRegion(DnsRegion.ValueOf("n/a"))
            .Build();

        // Create a request
        var request = new ListRecordSetsRequest();
        try
        {
            // Send the request and get the response
            var response = client.ListRecordSets(request);
            Console.WriteLine(response.HttpStatusCode);
        }
        catch (RequestTimeoutException requestTimeoutException)
        {
            Console.WriteLine(requestTimeoutException.ErrorMessage);
        }
        catch (ServiceResponseException clientRequestException)
        {
            Console.WriteLine(clientRequestException.HttpStatusCode);
            Console.WriteLine(clientRequestException.RequestId);
            Console.WriteLine(clientRequestException.ErrorCode);
            Console.WriteLine(clientRequestException.ErrorMsg);
        }
        catch (ConnectionException connectionException)
        {
            Console.WriteLine(connectionException.ErrorMessage);
        }
    }
}