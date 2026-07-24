using IPO.Correspondence.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IPO.Correspondence.API.Models
{
    [ExcludeFromCodeCoverage]
    public class NotificationModelExamples : IMultipleExamplesProvider<NotificationModel>
    {
        public IEnumerable<SwaggerExample<NotificationModel>> GetExamples()
        {
            yield return SwaggerExample.Create("Email", GetEmailRequestBody());
            yield return SwaggerExample.Create("Email With EmailReplyToId", GetEmailRequestBodyWithEmailReplyToId());
            yield return SwaggerExample.Create("Email With No SendType", GetEmailRequestBodyWithNoSendType());
            yield return SwaggerExample.Create("SMS", GetSMSRequestBody());
            yield return SwaggerExample.Create("Letter", GetLetterRequestBody());
            yield return SwaggerExample.Create("API", GetAPIRequestBody());
        }
        private static NotificationModel GetEmailRequestBody()
        {
            return new NotificationModel
            {
                Channel = NotificationType.Email,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content =  new Dictionary<string, object>()
                {
                          {"EmailAddress", "test@email.com"},
                          {"TemplateId", "00000000-0000-0000-0000-00000000"},
                          {"Personalisation", new {
                                                   TemplatePropertyNameA = "DataA",
                                                   TemplatePropertyNameB = "DataB"
                                                  }
                          }
                }
            };
        }

        private static NotificationModel GetEmailRequestBodyWithEmailReplyToId()
        { 
            return new NotificationModel
            {
                Channel = NotificationType.Email,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = new Dictionary<string, object>()
                {
                          {"EmailAddress", "test@email.com"},
                          {"TemplateId", "00000000-0000-0000-0000-00000000"},
                          {"EmailReplyToId", "00000000-0000-0000-0000-00000000"},
                          {"Personalisation", new {
                                                   TemplatePropertyNameA = "DataA",
                                                   TemplatePropertyNameB = "DataB"
                                                  }                        
                          }
                }

            };
        }

        private static NotificationModel GetEmailRequestBodyWithNoSendType()
        {
            return new NotificationModel
            {
                Channel = NotificationType.Email,
                Content = new Dictionary<string, object>()
                {
                          {"EmailAddress", "test@email.com"},
                          {"TemplateId", "00000000-0000-0000-0000-00000000"},
                          {"Personalisation", new {
                                                   TemplatePropertyNameA = "DataA",
                                                   TemplatePropertyNameB = "DataB"
                                                  }
                          }
                }

            };
        }
        
        private static NotificationModel GetSMSRequestBody()
        {
            return new NotificationModel
            {
                Channel = NotificationType.SMS,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = new Dictionary<string, object>()
                {
                          {"TemplateId", "00000000-0000-0000-0000-00000000"},
                          {"MobileNumber", "07000000000"},
                          {"Personalisation", new {
                                                   TemplatePropertyNameA = "DataA",
                                                   TemplatePropertyNameB = "DataB"
                                                  }
                          }
                }
            };
        }

        private static NotificationModel GetLetterRequestBody()
        {
            return new NotificationModel
            {
                Channel = NotificationType.Letter,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = new Dictionary<string, object>(){
                          {"TemplateId", "00000000-0000-0000-0000-00000000"},
                          {"Personalisation", new {
                                                   address_line_1 = "DataA",
                                                   address_line_2 = "DataB",
                                                   address_line_3 = "DataC",
                                                   address_line_4 = "DataD",
                                                   address_line_5 = "NP10 8YJ"
                                                  }
                          }
                }
            };
        }        

        private static NotificationModel GetAPIRequestBody()
        {
            return new NotificationModel
            {
                Channel = NotificationType.API,
                Content = new Dictionary<string, object>(){
                          {"TemplateId", "00000000-0000-0000-0000-00000000"},
                          {"EmailAddress", "test@email.com"}
                }

            };
        }
    }
}
