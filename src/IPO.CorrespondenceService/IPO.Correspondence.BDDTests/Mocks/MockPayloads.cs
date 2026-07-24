namespace IPO.Correspondence.BDDTests.Mocks
{
    public static class MockPayloads
    {
        public static readonly Dictionary<string, string> Payloads = new()
        {
            ["MalformedPayload"] = "{\r\n  \"channel\": \"Email\",\r\n  \"sendType\": \"PretendToSend\",\r\n  \"content\": \r\n    \"EmailAddress\": \"test@email.com\",\r\n    \"Template00-0000-0000-0000-00000000\",\r\n    \"Personalisation\": {\r\n      \"templatePropertyNameA\": \r\n      \"templatePropertyNameB\": \"DataB\"\r\n    }\r\n  }\r\n}",

            ["ValidPayload"] = "{\r\n  \"channel\": \"Email\",\r\n  \"sendType\": \"PretendToSend\",\r\n  \"content\": {\r\n    \"EmailAddress\": \"test@email.com\",\r\n    \"TemplateId\": \"00000000-0000-0000-0000-00000000\",\r\n    \"Personalisation\": {\r\n      \"templatePropertyNameA\": \"DataA\",\r\n      \"templatePropertyNameB\": \"DataB\"\r\n    }\r\n  }\r\n}",

            ["XmlPayload"] = "<NotificationModel>\r\n  <channel>Email</channel>\r\n  <sendType>PretendToSend</sendType>\r\n  <content>\r\n    <EmailAddress>test@email.com</EmailAddress>\r\n    <TemplateId>00000000-0000-0000-0000-00000000</TemplateId>\r\n    <Personalisation>\r\n      <templatePropertyNameA>DataA</templatePropertyNameA>\r\n      <templatePropertyNameB>DataB</templatePropertyNameB>\r\n    </Personalisation>\r\n  </content>\r\n</NotificationModel>\r\n",

            ["EmptyPayload"] = ""
        };
    }
}
