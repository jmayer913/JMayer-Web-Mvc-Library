using JMayer.Data.Data.Query;
using JMayer.Data.Data;
using Microsoft.AspNetCore.Mvc;
using TestProject.Controller;
using TestProject.Data;
using TestProject.Database;
using Microsoft.Extensions.Logging;
using System.Net;

namespace TestProject.Test.Controller;

/// <summary>
/// The class manages tests for the SubUserEditableContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleSubUserEditableController object which inherits from the SubUserEditableContoller and
/// the SimpleSubUserEditableController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the SubUserEditableContoller class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the two string ID methods 
/// (DeleteAsync() and SingleAsync()) are not tested.
/// 
/// SubUserEditableController class inherits from the UserEditableController. Because of this,
/// only new and overriden methods in the SubUserEditableController are tested because
/// the UserEditableControllerUnitTest already tests the UserEditableController.
/// 
/// Not all negative responses can be tested and adding a property to force a certain response
/// is risky so if there's missing fact/theory that's why.
/// </remarks>
public class SubUserEditableControllerUnitTest
{
    /// <summary>
    /// The constant for the maximum records.
    /// </summary>
    private const int MaxRecords = 100;

    /// <summary>
    /// The constant for the fist owner.
    /// </summary>
    private const long OwnerOne = 1;

    /// <summary>
    /// The constant for the second owner.
    /// </summary>
    private const long OwnerTwo = 2;

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
    private static async Task PopulateDataObjects(SimpleSubUserEditableDataLayer dataLayer, long ownerID)
    {
        for (int index = 1; index <= MaxRecords; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = $"{ownerID}-{index}", OwnerInteger64ID = ownerID, Value = index });
        }
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetAllAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetAllOkResponse()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        SimpleSubUserEditableController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetAllAsync(OwnerOne);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<List<SimpleSubUserEditableDataObject>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a list of data objects.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetAllListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetAllListViewOkResponse()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        SimpleSubUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetAllListViewAsync(OwnerTwo);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<List<ListView>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a list of list views.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetPageAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageInternalErrorResponse()
    {
        SimpleSubUserEditableController simpleCRUDController = new(new SimpleSubUserEditableDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm the action is responding with problem details.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetPageAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageOkResponse()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleSubUserEditableController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(OwnerOne, queryDefinition);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<PagedList<SimpleSubUserEditableDataObject>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a paged list of data objects.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetPageListViewAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageListViewInternalErrorResponse()
    {
        SimpleSubUserEditableController simpleCRUDController = new(new SimpleSubUserEditableDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageListViewAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm the action is responding with problem details.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetPageListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageListViewOkResponse()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleSubUserEditableController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetPageListViewAsync(OwnerTwo, queryDefinition);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<PagedList<ListView>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a list of list views.
    }
}
