using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IPO.Correspondence.API.Controllers
{
    [Route("/")]
    [ApiController]
    public class ExternalController : ControllerBase
    {
        private readonly ICorrespondenceManagementService _correspondenceManagementService;

        public ExternalController(ICorrespondenceManagementService correspondenceManagementService)
        {
            _correspondenceManagementService = correspondenceManagementService;
        }

        [HttpGet]
        [Route("{organisationId}/pending")]
        [SwaggerOperation(Summary = "Returns an array of unread messages that had been previously sent and not externally read.",
             Description = SwaggerDescriptions.PendingNotifications)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationViewModel>>> PendingNotifications(Guid organisationId)
            => Ok(await _correspondenceManagementService.GetPendingNotificationsAsync(organisationId, isInternalRead: false));

        [HttpGet]
        [Route("list/{organisationId}")]
        [SwaggerOperation(Summary = "Returns an array of messages that had been previously sent between two datetimes.",
            Description = SwaggerDescriptions.GetAllNotifications)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<NotificationViewModel>>> GetAllNotificationsFromTimeStamp(Guid organisationId, [Required] DateTime from, [Required] DateTime to)
            => Ok(await _correspondenceManagementService.GetAllNotificationsFromTimeStampAsync(organisationId, from, to, isInternalRead: false));

        [HttpGet]
        [Route("peek/{correspondenceId}")]
        [SwaggerOperation(Summary = "Returns a JSON model that has details of a correspondence that has been updated in the database without marking it as 'Read'.",
             Description = SwaggerDescriptions.PeekNotification)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<NotificationViewModel>> PeekNotification(Guid correspondenceId)
            => Ok(await _correspondenceManagementService.PeekNotificationAsync(correspondenceId));
    }
}