using JMayer.Data.Data.Query;
using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using TestProject.Controller;
using TestProject.Data;
using TestProject.Database;

#warning Right now, I've created a console logger just so the test don't fail. Need to come up with a better solution.

namespace TestProject.Test.Controller;

/// <summary>
/// The class manages tests for the StandardCRUDContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleCRUDController object which inherits from the StandardCRUDController and
/// the SimpleCRUDController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the StandardCRUDController class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the two string ID methods 
/// (DeleteAsync() and SingleAsync()) are not tested.
/// </remarks>
public class StandardCRUDControllerUnitTest
{
    /// <summary>
    /// The method confirms the StandardCRUDContoller.CountAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task CountAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleListDataLayer.");
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.CountAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task CountAsyncOkResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CountAsync();

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is int value //Confirm the action is responding with an integer value.
            && value == 1 //Confirm its the correct count.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.CreateAsync() returns a 400 (Bad Request) response when the data object is not valid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task CreateAsyncBadRequestResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();
        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CreateAsync(new SimpleDataObject() { Value = 9999 });

        Assert.True
        (
            actionResult is BadRequestObjectResult badRequestObjectResult //Confirm the correct action is returned.
            && badRequestObjectResult.Value is ServerSideValidationResult serverSideValidationResult //Confirm the action is responding with a validation result.
            && !serverSideValidationResult.IsSuccess //Confirm the action is responding with a non-successful validation result.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.CreateAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task CreateAsyncInternalErrorResponse()
    {
        SimpleCRUDController simpleCRUDController = new(new SimpleMemoryDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CreateAsync(null);

        Assert.True
        (
            actionResult is StatusCodeResult statusCodeResult //Confirm the correct action is returned.
            && statusCodeResult.StatusCode == (int)HttpStatusCode.InternalServerError //Confirm the correct action is returned.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.CreateAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task CreateAsyncOkResponse()
    {
        SimpleDataObject originalDataObject = new() { Value = 10 };
        SimpleMemoryDataLayer dataLayer = new();
        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CreateAsync(originalDataObject);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is SimpleDataObject returnedDataObject //Confirm the action is responding with a data object.
            && returnedDataObject.Integer64ID == 1 && returnedDataObject.Value == originalDataObject.Value //Confirm the action is responding with the created data object.
        );
    }

    /// <summary>
    /// The method returns a console logger.
    /// </summary>
    /// <returns>An ILogger.</returns>
    private static ILogger CreateConsoleLogger()
    {
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleCRUDController>();
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.DeleteAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task DeleteAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleListDataLayer.");
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.DeleteAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task DeleteAsyncOkResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.DeleteAsync(dataObject?.Integer64ID ?? 0);

        //Confirm the correct action is returned.
        Assert.IsType<OkResult>(actionResult);
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetAllAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleListDataLayer.");
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetAllAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetAllAsyncOkResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();

        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 10 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 20 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 30 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 40 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 50 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 60 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 70 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 80 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 90 });
        _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = 100 });

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetAllAsync();

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is List<SimpleDataObject> list //Confirm the action is responding with a list of data objects.
            && list.Count == 10 //Confirm the list matches the amount created.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetPageAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleListDataLayer.");
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetPageAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetPageAsyncOkResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();

        for (int index = 1; index <= 100; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = index });
        }

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(queryDefinition);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is List<SimpleDataObject> list //Confirm the action is responding with a list of data objects.
            && list.Count == queryDefinition.Take //Confirm the list matches the amount created.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetSingleAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetSingleAsyncInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleListDataLayer.");
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetSingleAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetSingleAsyncOkResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetSingleAsync();

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is SimpleDataObject dataObject //Confirm the action is responding with a data objects.
            && dataObject.Integer64ID == 1 //Confirm the action is responding with the correct data object.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetSingleAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetSingleAsyncWithIntegerIDInternalErrorResponse()
    {
        throw new NotImplementedException("Cannot test this. There isn't an easy way to force an exception in the SimpleListDataLayer.");
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetSingleAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task GetSingleAsyncWithIntegerIDOkResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync([ new SimpleDataObject(), new SimpleDataObject() ]);

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetSingleAsync(2);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is SimpleDataObject dataObject //Confirm the action is responding with a data objects.
            && dataObject.Integer64ID == 2 //Confirm the action is responding with the correct data object.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.UpdateAsync() returns a 400 (Bad Request) response when the data object is not valid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task UpdateAsyncBadRequestResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();
        SimpleDataObject originalDataObject = await dataLayer.CreateAsync(new SimpleDataObject());

        originalDataObject.Value = 9999;

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.UpdateAsync(originalDataObject);

        Assert.True
        (
            actionResult is BadRequestObjectResult badRequestObjectResult //Confirm the correct action is returned.
            && badRequestObjectResult.Value is ServerSideValidationResult serverSideValidationResult //Confirm the action is responding with a validation result.
            && !serverSideValidationResult.IsSuccess //Confirm the action is responding with a non-successful validation result.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.UpdateAsync() returns a 409 (Conflict) response when the data object is old.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task UpdateAsyncConflictResponse()
    {
        throw new NotImplementedException("Cannot test this with a SimpleListDataLayer.");
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.UpdateAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task UpdateAsyncInternalErrorResponse()
    {
        SimpleCRUDController simpleCRUDController = new(new SimpleMemoryDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.UpdateAsync(null);

        Assert.True
        (
            actionResult is StatusCodeResult statusCodeResult //Confirm the correct action is returned.
            && statusCodeResult.StatusCode == (int)HttpStatusCode.InternalServerError //Confirm the correct action is returned.
        );
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.UpdateAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task UpdateAsyncOkResponse()
    {
        SimpleMemoryDataLayer dataLayer = new();
        SimpleDataObject originalDataObject = await dataLayer.CreateAsync(new SimpleDataObject());

        originalDataObject.Value = 10;

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.UpdateAsync(originalDataObject);

        Assert.True
        (
            actionResult is OkObjectResult okObjectResult //Confirm the correct action is returned.
            && okObjectResult.Value is SimpleDataObject returnedDataObject //Confirm the action is responding with a data object.
            && returnedDataObject.Integer64ID == 1 && returnedDataObject.Value == originalDataObject.Value //Confirm the action is responding with the updated data object.
        );
    }
}
