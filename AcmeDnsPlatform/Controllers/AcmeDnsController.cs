using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;
using AcmeDnsPlatform.Api;
using AcmeDnsPlatform.DataTransferObjects;

namespace AcmeDnsPlatform.Controllers
{
    [Route("acme-dns")]
    [ApiController]
    public class AcmeDnsController : ControllerBase
    {
        private IPlatformAccountManagement _platformAccountManagement;
        private IPlatformDnsManagement _platformDnsManagement;

        public AcmeDnsController(IPlatformAccountManagement platformAccountManagement, IPlatformDnsManagement platformDnsManagement)
        {
            _platformAccountManagement = platformAccountManagement;
            _platformDnsManagement = platformDnsManagement;
        }
        
        [Route("register")]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegisterResponseDto))]
        [SwaggerOperation("Register an account.", "Register an account for ACME DNS usage.")]
        [SwaggerResponse(StatusCodes.Status201Created, "Account has been successfully registered.", Type = typeof(RegisterResponseDto))]
        public IActionResult Register([FromBody] RegisterRequestDto registerRequest)
        {
            var account = _platformAccountManagement.RegisterAccount(registerRequest.AllowFrom);

            var result = new RegisterResponseDto()
            {
                Username = account.Username,
                Password = account.Password,
                FullDomain = account.FullDomain,
                SubDomain = account.Subdomain,
                AllowFrom = account.AllowFrom
            };

            return new JsonResult(result)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

        [Route("update")]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UpdateResponeDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation("Update a DNS entry.", "Update the DNS entry of the given subdomain.")]
        [SwaggerResponse(StatusCodes.Status201Created, "Updated the DNS entry.", Type = typeof(UpdateResponeDto))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Not authenticated.")]
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden to authorize given subdomain.")]
        public IActionResult PostUpdate([FromBody] UpdateRequestDto updateRequestDto, [FromHeader(Name = "X-Api-User")] string username, [FromHeader(Name = "X-Api-Key")] string password)
        {
            var remoteIpAddress = Request.HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress == null)
            {
                return new BadRequestResult();
            }
            
            var successful = _platformAccountManagement.CheckCredentials(
                username,
                password,
                remoteIpAddress.ToString());

            if (!successful)
            {
                return new UnauthorizedResult();
            }

            var account = _platformAccountManagement.GetAccount(username);
            
            if (updateRequestDto.SubDomain != account.Subdomain)
            {
                return new ForbidResult();
            }

            var result = new UpdateResponeDto();
            
            if (string.IsNullOrWhiteSpace(updateRequestDto.TxtRecordValue))
            {
                _platformDnsManagement.RemoveTextRecord(account.Subdomain, updateRequestDto.TxtRecordValue);

                result = new UpdateResponeDto()
                {
                    TxtRecordValue = updateRequestDto.TxtRecordValue,
                };
            }
            else
            {
                _platformDnsManagement.AddTextRecord(account.Subdomain, updateRequestDto.TxtRecordValue);

                result = new UpdateResponeDto()
                {
                    TxtRecordValue = updateRequestDto.TxtRecordValue,
                };
            }

            return new JsonResult(result)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
    }
}
