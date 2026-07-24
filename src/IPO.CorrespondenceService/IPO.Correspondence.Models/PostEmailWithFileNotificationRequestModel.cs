using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPO.Correspondence.Models
{
    public class PostEmailWithFileNotificationRequestModel
    {
        [FromRoute]
        //Parameter must be in camelCase, otherwise doesn't match with Route parameter and breaks OpenAPI spec and pipeline.
        public Guid organisationId { get; set; }
        [SwaggerSchema(Title = "The user specified API Key. PretendToSend/SendToListOnly/SendToAnyone")]
        public GovNotifyAPIKeyType? SendType { get; set; }
        [Required]
        public Dictionary<string, object>? Content { get; set; }
        [Required(ErrorMessage = "The file is required.")]
        public IFormFile? File { get; set; }
    }
}
