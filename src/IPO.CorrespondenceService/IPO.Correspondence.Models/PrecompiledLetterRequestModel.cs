using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace IPO.Correspondence.Models
{
    public class PrecompiledLetterRequestModel
    {
        [FromRoute]
        public Guid organisationId { get; set; }       

        [SwaggerSchema(Title = "The user specified API Key. PretendToSend/SendToListOnly/SendToAnyone")]
        public GovNotifyAPIKeyType? SendType { get; set; }

        [SwaggerSchema(Title = "Set the postage for your precompiled letter. first or second class. If not set then second class is set as default value")]
        public PrecompiledLetterPostage? Postage { get; set; }

        [Required(ErrorMessage = "The file is required.")]
        public IFormFile? File { get; set; }

    }
}
