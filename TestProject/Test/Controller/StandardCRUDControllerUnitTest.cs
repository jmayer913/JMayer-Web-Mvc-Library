using JMayer.Data.Data;
using JMayer.Data.Data.Query;
using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using TestProject.Controller;
using TestProject.Data;
using TestProject.Database;

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
/// 
/// Not all negative responses can be tested and adding a property to force a certain response
/// is risky so if there's missing fact/theory that's why.
/// </remarks>
public class StandardCRUDControllerUnitTest
{
    /// <summary>
    /// The constant for the default value.
    /// </summary>
    private const int DefaultValue = 1;

    /// <summary>
    /// The constant for the maximum records.
    /// </summary>
    private const int MaxRecords = 100;

    /// <summary>
    /// The constant for the invalid value.
    /// </summary>
    private const int InvalidValue = 9999;

    /// <summary>
    /// The method returns a console logger.
    /// </summary>
    /// <returns>An ILogger.</returns>
    private static ILogger CreateConsoleLogger()
    {
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleCRUDController>();
    }

    /// <summary>
    /// The method populates data objects in a datalayer with values that increment by 1.
    /// </summary>
    /// <param name="dataLayer">The data layer to populate</param>
    /// <returns>A Task object for the async.</returns>
    private static async Task PopulateDataObjects(SimpleStandardCRUDDataLayer dataLayer)
    {
        for (int index = 1; index <= MaxRecords; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleDataObject() { Value = index });
        }
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.CountAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCountOkResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CountAsync();

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<long>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with an integer value.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.CreateAsync() returns a 400 (Bad Request) response when the data object is not valid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateBadRequestResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CreateAsync(new SimpleDataObject() { Value = InvalidValue });

        Assert.IsType<BadRequestObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ServerSideValidationResult>(((BadRequestObjectResult)actionResult).Value); //Confirm the action is responding with a validation result.
        Assert.False(((ServerSideValidationResult)((BadRequestObjectResult)actionResult).Value).IsSuccess); //Confirm the action is responding with a non-successful validation result.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.CreateAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateInternalErrorResponse()
    {
        SimpleCRUDController simpleCRUDController = new(new SimpleStandardCRUDDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CreateAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm the action is responding with problem details.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.CreateAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateOkResponse()
    {
        SimpleDataObject originalDataObject = new() { Value = DefaultValue };
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.CreateAsync(originalDataObject);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<SimpleDataObject>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a data object.
        Assert.Equal(originalDataObject.Value, ((SimpleDataObject)((OkObjectResult)actionResult).Value).Value); //Confirm the action is responding with the created data object.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.CreateAsync() returns a 400 (Bad Request) response when the data object is not valid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyDeleteConflictResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer);

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.DeleteAsync(SimpleStandardCRUDDataLayer.DeleteConflictId);

        Assert.IsType<ConflictResult>(actionResult); //Confirm the correct action is returned.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.DeleteAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyDeleteOkResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.DeleteAsync(dataObject.Integer64ID);

        //Confirm the correct action is returned.
        Assert.IsType<OkResult>(actionResult);
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.GetAllAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetAllOkResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer);

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetAllAsync();

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<List<SimpleDataObject>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a list of data objects.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.GetPageAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageInternalErrorResponse()
    {
        SimpleCRUDController simpleCRUDController = new(new SimpleStandardCRUDDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm the action is responding with problem details.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.GetPageAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageOkResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();

        await PopulateDataObjects(dataLayer);

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(queryDefinition);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<PagedList<SimpleDataObject>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a paged list of data objects.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.GetSingleAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetSingleOkResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetSingleAsync();

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<SimpleDataObject>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a data object.
    }

    /// <summary>
    /// The method confirms the StandardCRUDContoller.GetSingleAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetSingleOkResponseForId()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync([ new SimpleDataObject(), new SimpleDataObject() ]);

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetSingleAsync(2);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<SimpleDataObject>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a data object.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.UpdateAsync() returns a 400 (Bad Request) response when the data object is not valid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateBadRequestResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject originalDataObject = await dataLayer.CreateAsync(new SimpleDataObject());

        originalDataObject.Value = InvalidValue;

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.UpdateAsync(originalDataObject);

        Assert.IsType<BadRequestObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ServerSideValidationResult>(((BadRequestObjectResult)actionResult).Value); //Confirm the action is responding with a validation result.
        Assert.False(((ServerSideValidationResult)((BadRequestObjectResult)actionResult).Value).IsSuccess); //Confirm the action is responding with a non-successful validation result.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.UpdateAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateInternalErrorResponse()
    {
        SimpleCRUDController simpleCRUDController = new(new SimpleStandardCRUDDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.UpdateAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm the action is responding with problem details.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
    }

    /// <summary>
    /// The method verifies the StandardCRUDContoller.UpdateAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateOkResponse()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject originalDataObject = await dataLayer.CreateAsync(new SimpleDataObject());

        originalDataObject.Value = DefaultValue;

        SimpleCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.UpdateAsync(originalDataObject);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<SimpleDataObject>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a data object.
        Assert.Equal(originalDataObject.Integer64ID, ((SimpleDataObject)((OkObjectResult)actionResult).Value).Integer64ID); //Confirm the action is responding with the updated data object.
        Assert.Equal(originalDataObject.Value, ((SimpleDataObject)((OkObjectResult)actionResult).Value).Value); //Confirm the action is responding with the updated data object.
    }
}
