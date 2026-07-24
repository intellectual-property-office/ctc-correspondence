# Correspondence Microservice

# About
The Correspondence Microservice provides a set of APIs that allows the IPO to send notifications to customers via SMS, Email, Letter and API for machine to machine integration. The Microservice handles the sending of messages via the Gov.UK Notify service. The Microservice logs all correspondence within a database and also regularly polls Gov.UK Notify for status updates for these pending notifications. Template messages, for each message type, are created via Gov.UK Notify and then populated with content provided by the users.


# Installation guide
### System Requirements
- IDE capable of running .NET 10 or above i.e. Visual Studio

### Prerequisites
- Visual Studio 2026 installed on your local machine.
- Gov.Notify unique API Key.
- Azure SQL Server Database
- Azure Storager Account and Container
- Azure Service Bus and Topic
- A passthrough service to handle requests and responses to and from the GovNotify RESTful API. This is currently implemented as a separate API, but could be implemented directly in the gateways project within this solution.

### Installation instructions
1. Clone the repository to your local machine.

2. Open the 'IPO.Correspondence.sln' solution file in Visual Studio.

3. Follow the 'configuration steps' below to create local development configuration settings for each project in the solution, then return here.

4. Build the solution.

5. Set the IPO.Correspondence.API project as the Startup project in Visual Studio and run in debug configuration.

6. A command window will launch, in which you will see the Console output.

7. The swagger page will launch in your default browser ready to test the endpoints. Any data changes you make can be reviewed in your database.

## Configuration Steps:

1. In the IPO.Correspondence.API project add a local development settings file called 'appsettings.Development.json' in the Web API project. Copy the contents of the project 'appsettings.json' File.

2. Obtain the connection string for your Azure storage account and update the 'AzureBlobStorageConnectionString' value in the 'appsettings.Development.json' file.

3. Obtain the connection string for your Azure storage blob container and update the 'ContainerName' value in the 'appsettings.Development.json' file.

4. Obtain the connection string for your local SQL database and update the 'NotificationDbConnection' value in the 'appsettings.Development.json' file.

5. Obtain the connection string for your Azure service bus and update the 'ServiceBusConnectionString' value in the 'appsettings.Development.json' file.

6. Obtain the Topic name for your Azure service bus topic and update the 'Topic' value in the 'appsettings.Development.json' file.

7. Set the local development address of the Correspondence function applications in the following settings in the 'appsettings.Development.json' file.
   1. ApiFunctionService
   2. EmailService
   3. LetterFunctionService
   4. SmsFunctionService
   5. StatusUpdateFunctionService


8. For each of the IPO.Correspondence.APIFunction, IPO.Correspondence.Email, IPO.Correspondence.Letter and IPO.Correspondence.SMS projects, add a local settings file called 'local.settings.json'. Copy the contents of the below Configuration Files for each project and paste into the matching 'local.settings.json' files.

9.  In each 'local.settings.json' file update the 'NotificationDbConnection', 'ServiceBusConnectionString' and 'Topic' values in the same ways as described by steps 6, 7 and 8 above.

10. Obtain your Gov.Notify API Key and update the 'GovNotifyAPIKEY' value in the 'local.settings.json' files.

11. For each of the IPO.Correspondence.APIFunction, IPO.Correspondence.Email, IPO.Correspondence.Letter and IPO.Correspondence.SMS projects, go to your service bus topic and create a subscription with the same name as the 'Subscription' value in the project's 'local.settings.json' file. 

12. For each subscription delete the default filter and replace it with a correlation filter with a custom property where 'Key' is "type" and 'Value' is the same value, in double quote marks, as appears for 'Subscriptiom' in the project's 'local.settings.json' file.

13. For the IPO.Correspondence.StatusUpdate project you may also need to amend the CronPattern value to one which matches your local testing requirements. The default value ("0 */5 * * * *") corresponds to the correspondence status being updated from Gov.Notify every 5 minutes. NOTE: For a live system the SystemUpdate will be set up with azure as the scheduler rather than requiring the CronPattern to be specified locally.

## Configuration files:

IPO.Correspondence.Email:
```JSON
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": ".",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",	
    "AzureBlobStorageConnectionString": "",
    "ServiceBusConnectionString": "",
    "AzureFunctionsJobHost__extensions__serviceBus__transportType": "AmqpWebSockets",
    "Topic": "",
    "ContainerName": "",
    "Subscription": "Email",
    "GovNotifyApiKey:Default":"",
    "GovNotifyApiKey:PretendToSend":"",
    "GovNotifyApiKey:SendToAnyone":"",
    "GovNotifyApiKey:SendToListOnly":"",
    "NotificationDbConnection": ""
  }
}
```

IPO.Correspondence.Letter:
```JSON
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": ".",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",	
    "AzureBlobStorageConnectionString": "",
    "ServiceBusConnectionString": "",
    "AzureFunctionsJobHost__extensions__serviceBus__transportType": "AmqpWebSockets",
    "Topic": "",
    "ContainerName": "",
    "Subscription": "Letter",
    "GovNotifyApiKey:Default":"",
    "GovNotifyApiKey:PretendToSend":"",
    "GovNotifyApiKey:SendToAnyone":"",
    "GovNotifyApiKey:SendToListOnly":"",
    "NotificationDbConnection": ""
  }
}
```

IPO.Correspondence.APIFunction:
```JSON
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": ".",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",	
    "AzureBlobStorageConnectionString": "",
    "ServiceBusConnectionString": "",
    "AzureFunctionsJobHost__extensions__serviceBus__transportType": "AmqpWebSockets",
    "Topic": "",
    "ContainerName": "",
    "Subscription": "API",
    "GovNotifyAPIKEY": ""
  },
  "ConnectionStrings": {
    "NotificationDbConnection": ""
  }
}
```

For IPO.Correspondence.SMS:
```JSON
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": ".",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnectionString": "",
    "AzureFunctionsJobHost__extensions__serviceBus__transportType": "AmqpWebSockets",
    "Topic": "",
    "Subscription": "SMS",
    "GovNotifyApiKey:Default":"",
    "GovNotifyApiKey:PretendToSend":"",
    "GovNotifyApiKey:SendToAnyone":"",
    "GovNotifyApiKey:SendToListOnly":"",
    "NotificationDbConnection": ""
  }
}
```

The **Subscription** property should match the relevant **Service Bus Topic Subscription**.  This should be inline with the **NotificationType** enum.
```CSharp
namespace IPO.Correspondence.Notification.Models
{
    public enum NotificationType
    {
        Email,
        Letter,
        API,
        SMS
    }
}
```

For IPO.Correspondence.StatusUpdate (Timer):
```JSON
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "AzureWebJobsSecretStorageType": "files",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ServiceBusConnectionString": "",
    "Topic": "",
    "GovNotifyAPIKEY": "",
    "CronPattern": "0 */5 * * * *"
  },
    "ConnectionStrings": {
      "NotificationDbConnection": ""
    }
}
```

**DEVELOPERS, PLEASE NOTE**
To successfuly run an Azure Function from your development machine, the following properties of the **Values** (in your config file) object need setting.
```JSON
    "AzureFunctionsJobHost__extensions__serviceBus__transportType": "AmqpWebSockets"
```
add at the end of the connection string for the **SubscriptionConnection** you must add (if it doesn't exist) before the closing "
```
;TransportType=AmqpWebSockets
```


