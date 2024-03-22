using AcmeDnsOtc.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net.Mime;

namespace AcmeDnsOtc.Controllers
{
    [Route("acme-dns")]
    [ApiController]
    public class AcmeDnsController : ControllerBase
    {
        [Route("register")]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RegisterResponseDto))]
        [SwaggerOperation("Register an account.", "Register an account for ACME DNS usage.")]
        [SwaggerResponse(StatusCodes.Status201Created, "Account has been sucessfully registered.", Type = typeof(RegisterResponseDto))]
        public RegisterResponseDto Register([FromBody] RegisterRequestDto registerRequest)
        {
            // TODO: register account in database
            // TODO: create domain entry

            var result = new RegisterResponseDto()
            {
                Username = Guid.NewGuid().ToString(),
                Password = Guid.NewGuid().ToString(),
                FullDomain = "ffff." + Guid.NewGuid().ToString(),
                SubDomain = "ffff",
                AllowFrom = new string[0]
            };

            return result;
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
        [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden to authrize given subdomain.")]
        public UpdateResponeDto PostUpdate([FromBody] UpdateRequestDto updateRequestDto, [FromHeader(Name = "X-Api-User")] string username, [FromHeader(Name = "X-Api-Key")] string password)
        {
            // TODO: Lookup username + password
            // TODO: Update entry with value

            var result = new UpdateResponeDto()
            {
                TxtRecordValue = updateRequestDto.TxtRecordValue,
            };

            return result;
        }
    }
}
