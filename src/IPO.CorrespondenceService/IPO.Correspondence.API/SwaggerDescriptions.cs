namespace IPO.Correspondence.API
{
    public static class SwaggerDescriptions
    {
        // External Controller
        public const string PendingNotifications =
            "<b>Notes:</b> \n\n Request status definitions can be found in the " +
            "<a href=\"https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/6851/Integration-Guide?anchor=**request-status-definitions**\" target=\"_blank\">Integration Guide wiki page</a>.";

        public const string GetAllNotifications =
            "<b>Notes:</b> \n\n Required Date Format: yyyy/MM/dd. E.g. 2023/07/25. \n\n" +
            "You can also use hyphens to separate the year, month and day.\n\n" +
            "Request status definitions can be found in the " +
            "<a href=\"https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/6851/Integration-Guide?anchor=**request-status-definitions**\" target=\"_blank\">Integration Guide wiki page</a>.";

        public const string PeekNotification =
            "<b>Notes:</b> \n\n Request status definitions can be found in the " +
            "<a href=\"https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/6851/Integration-Guide?anchor=**request-status-definitions**\" target=\"_blank\">Integration Guide wiki page</a>.";

        // Internal Controller
        public const string InternalPendingNotifications =
            "<b>Notes:</b> \n\n Request status definitions can be found in the " +
            "<a href=\"https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/6851/Integration-Guide?anchor=**request-status-definitions**\" target=\"_blank\">Integration Guide wiki page</a>.";

        public const string InternalGetAllNotifications =
            "<b>Notes:</b> \n\n Required Date Format: yyyy/MM/dd. E.g. 2023/07/25. \n\n" +
            "You can also use hyphens to separate the year, month and day.\n\n" +
            "Request status definitions can be found in the " +
            "<a href=\"https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/6851/Integration-Guide?anchor=**request-status-definitions**\" target=\"_blank\">Integration Guide wiki page</a>.";

        public const string EmailWithFile =
            "<b>Notes:</b> \n\n When sending an email with an attachment you will need to ensure that you are doing the following:\r\n\r\n" +
            "1.) Send request in multipart/form-data format.\r\n\r\n" +
            "2.) Attach a valid file (.PDF/.CSV/.ODT/.TXT/.RTF/.XLSX/.DOC/.DOCX). \r\n\r\n" +
            "3.) Set your content in JSON format with valid properties (EmailAddress/TemplateId/FilePersonalisationName/Personalisation). \r\n\r\n" +
            "4.) Please check the config for the default value for the 'sendType' field. \r\n\r\n" +
            "5.) Request body example can be found in the " +
            "<a href=\"https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/6851/Integration-Guide?anchor=**emailwithfile-body-request-example**\" target=\"_blank\">Integration Guide wiki page</a>.";

        public const string PrecompiledLetter =
            "<b>Notes:</b> \n\n When sending a precompiled letter request please make sure it complies with the following:\r\n\r\n" +
            "1.) .PDF file format only. (.TXT/.RTF/.DOC/.DOCX or any other text file format are invalid).\r\n\r\n" +
            "2.) Please review the " +
            "<a href=\"https://dev.azure.com/Ukipo/CTC-Programme/_wiki/wikis/CTC-Programme.wiki/6851/Integration-Guide?anchor=**precompiled-letter-specifications**\" target=\"_blank\">Gov.UK Notify's letter specifications</a>" +
            " and make sure your PDF upload meets the requirements.  \r\n\r\n" +
            "3.) To help you set up your letter, you can use this " +
            "<a href=\"https://dev.azure.com/Ukipo/67974730-00a5-4254-88e3-2c2f7664692d/_apis/git/repositories/687b4fc9-3382-4847-8191-e6ad44900e1d/Items?path=/.attachments/notify-pdf-letter-spec-v2.4-61f29e45-1e65-4d88-8da4-4dc62524d876.pdf&download=false&resolveLfs=true&%24format=octetStream&api-version=5.0-preview.1&sanitize=true&versionDescriptor.version=wikiMaster\" target=\"_blank\">Gov.UK Notify letter specification document</a>.\r\n\r\n" +
            "4.) If postage is not set in the request then Gov.UK Notify will set the default option as second class postage. \r\n\r\n";

        public const string PreviewTemplate =
            "<b>Notes:</b> \n\n Submit a template Id to preview a Gov.UK Notify template.\n\n" +
            "'templateId' is a required field. 'returnAsHtml' and 'personalisation' fields are optional. \n\n" +
            "Template Preview Body Request examples are available below to help you generate successful responses.\n\n" +
            "Use the 'With HTML Line Break tags' request example if you wish to preview a template body with HTML line break tags.\n\n" +
            "Use the 'With Template Personalisation Data' request example if the template requires personalisation data. \n\n" +
            "Use the 'With All Parameters' request example if the preview template requires personalisation data and you wish to have the body use HTML line break tags. \n\n" +
            "IMPORTANT: Make sure you use the correct template Id and its respective personalisation data placeholder names. This information can be found on the Gov.UK Notify portal. \n\n" +
            "When previewing SMS templates only, the 'subject' field in the response body will always return null. See the 'SMS Template Preview' response example.";
    }
}