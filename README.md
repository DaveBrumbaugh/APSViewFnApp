# APSViewFnApp
Serverless Autodesk Platform Services Viewer for Azure Function Apps

This is a Visual Studio project that should be suitable for publishing to an Azure Function App.

To use, you'll need to register for an Autodesk Platform Services account and create an application. From the application you'll need the client id and the secret. Put those in your app by going to Configuration>Application Settings. Add APS_CLIENT_ID and APS_CLIENT_SECRET. Additionally, add DEFAULT_WEB_PAGE. Mine points to the included viewer.html which I've stored in an Azure Storage Blob.

You should be able to upload files via this app and have them display. Previously uploaded files will show in the models drop down list. The uploaded models are stored in an Amazon S3 bucket managed by Autodesk.

This project is a port of this one: https://github.com/autodesk-platform-services/aps-simple-viewer-dotnet

And, the original is viewable here with a lot of different drawing types uploaded. https://aps-simple-viewer-dotnet.autodesk.io/#dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6cGV0cmJyb3otc2FtcGxlcy9yYWRpdXMlMjBjb3JuZXJzLmR4Zg

