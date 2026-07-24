using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using IPO.Correspondence.Models.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;

namespace IPO.Correspondence.API.Controllers
{
    [Route("internal")]
    [ApiController]
    public class InternalController : ControllerBase
    {
        private readonly ICorrespondenceManagementService _correspondenceManagementService;

        public InternalController(ICorrespondenceManagementService correspondenceManagementService)
                => _correspondenceManagementService = correspondenceManagementService;

        [HttpGet]
        [Route("{organisationId}/pending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Returns an array of unread messages that had been previously sent and not internally read.",
             Description = SwaggerDescriptions.InternalPendingNotifications)]
        public async Task<ActionResult<IEnumerable<NotificationViewModel>>> PendingNotifications(Guid organisationId)
            => Ok(await _correspondenceManagementService.GetPendingNotificationsAsync(organisationId, isInternalRead: true));

        [HttpGet]
        [Route("list/{organisationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Produces("application/json")]
        [SwaggerOperation(Summary = "Returns an array of messages that had been previously sent on the specified datetime.",
            Description = SwaggerDescriptions.InternalGetAllNotifications)]
        public async Task<ActionResult<IEnumerable<NotificationViewModel>>> GetAllNotificationsFromTimeStamp(
            Guid organisationId,
            [Required] DateTime from,
            [Required] DateTime to)
            => Ok(await _correspondenceManagementService.GetAllNotificationsFromTimeStampAsync(organisationId, from, to, isInternalRead: true));

        [HttpPost]
        [Route("{organisationId}")]
        [SwaggerOperation(Summary = "Sends a message to be processed by Gov.UK Notify.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Accepted", typeof(Guid), "text/plain")]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<Guid>> PostNotification([FromRoute] Guid organisationId, NotificationModel correspondence)
            => Accepted(await _correspondenceManagementService.PostNotificationAsync(organisationId, correspondence));

        [HttpPost]
        [Route("emailWithFile/{organisationId}")]
        [SwaggerOperation(Summary = "Sends an email message with a file attachment to be processed by Gov.UK Notify",
          Description = SwaggerDescriptions.EmailWithFile)]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Accepted", typeof(Guid), "text/plain")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<ActionResult<Guid>> PostEmailWithFileNotification([FromForm, OnlyOneFormFileIsAllowed] PostEmailWithFileNotificationRequestModel model)
        {
            try
            {
                model.Content = IPOJsonSerialization.Deserialize<Dictionary<string, object>>(Request.Form[nameof(model.Content).ToLowerInvariant()]!);
            }
            catch (JsonException ex)
            {
                var error = Error.Validation;
                error.Description += $". Message: {ex.Message}";
                throw new StatusCodeException(error, "", null, 422);
            }

            return Accepted(await _correspondenceManagementService.PostEmailWithFileNotificationAsync(model));
        }

        [HttpPost]
        [Route("preview")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TemplatePreviewResponse))]
        [Consumes("application/json")]
        [Produces("application/json")]
        public async Task<ActionResult<TemplatePreviewResponse>> PostPreviewTemplate(TemplatePreviewRequest previewRequest)
             => Ok(await _correspondenceManagementService.GenerateTemplatePreviewAsync(previewRequest));

        [HttpPost]
        [Route("precompiledLetter/{organisationId}")]
        [SwaggerOperation(Summary = "Sends a precompiled letter request to Gov.UK Notify.",
          Description = SwaggerDescriptions.PrecompiledLetter)]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Accepted", typeof(Guid), "text/plain")]
        [Consumes("multipart/form-data")]
        [Produces("application/json")]
        public async Task<ActionResult<Guid>> PostPrecompiledLetterRequest(
            [FromForm, OnlyOneFormFileIsAllowed] PrecompiledLetterRequestModel model)
            => Accepted(await _correspondenceManagementService.PostPrecompiledLetterRequestAsync(model));

    }
}