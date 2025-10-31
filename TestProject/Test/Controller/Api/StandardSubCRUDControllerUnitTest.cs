using JMayer.Data.Data.Query;
using JMayer.Data.Data;
using Microsoft.AspNetCore.Mvc;
using TestProject.Data;
using TestProject.Database;
using Microsoft.Extensions.Logging;
using System.Net;
using TestProject.Controller.Api;

#warning I think I can cheat to test the string id methods. Try this in the big refactor.

namespace TestProject.Test.Controller.Api;

/// <summary>
/// The class manages tests for the StandardSubCRUDContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleStandardSubCRUDController object which inherits from the StandardSubCRUDContoller and
/// the SimpleStandardSubCRUDController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the StandardSubCRUDContoller class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the two string ID methods 
/// (DeleteAsync() and SingleAsync()) are not tested.
/// 
/// StandardSubCRUDContoller class inherits from the StandardCRUDContoller. Because of this,
/// only new and overriden methods in the StandardCRUDContoller are tested because
/// the StandardSubCRUDControllerUnitTest already tests the UserEditableController.
/// 
/// Not all negative responses can be tested and adding a property to force a certain response
/// is risky so if there's missing fact/theory that's why.
/// </remarks>
public class StandardSubCRUDControllerUnitTest
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
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleStandardCRUDController>();
    }

    /// <summary>
    /// The method populates data objects in a datalayer with values that increment by 1.
    /// </summary>
    /// <param name="dataLayer">The data layer to populate</param>
    /// <returns>A Task object for the async.</returns>
    private static async Task PopulateDataObjects(SimpleStandardSubCRUDDataLayer dataLayer, long ownerID)
    {
        for (int index = 1; index <= MaxRecords; index++)
        {
            _ = await dataLayer.CreateAsync(new SimpleSubDataObject() { Name = $"{ownerID}-{index}", OwnerInteger64ID = ownerID, Value = index });
        }
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetAllAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetAllOkResponse()
    {
        SimpleStandardSubCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        SimpleStandardSubCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetAllAsync(OwnerOne);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<List<SimpleSubDataObject>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a list of data objects.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetAllListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetAllListViewOkResponse()
    {
        SimpleStandardSubCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        SimpleStandardSubCRUDController controller = new(dataLayer, CreateConsoleLogger());
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
        SimpleStandardSubCRUDController simpleCRUDController = new(new SimpleStandardSubCRUDDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm the action is responding with problem details.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
        Assert.NotEmpty(((ProblemDetails)((ObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
        Assert.NotEmpty(((ProblemDetails)((ObjectResult)actionResult).Value).Title); //Confirm a title is set.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetPageAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageOkResponse()
    {
        SimpleStandardSubCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleStandardSubCRUDController simpleCRUDController = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageAsync(OwnerOne, queryDefinition);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<PagedList<SimpleSubDataObject>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a paged list of data objects.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetPageListViewAsync() returns a 500 (Interal Server Error) response when an exception is thrown by the data layer.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageListViewInternalErrorResponse()
    {
        SimpleStandardSubCRUDController simpleCRUDController = new(new SimpleStandardSubCRUDDataLayer(), CreateConsoleLogger());
        IActionResult actionResult = await simpleCRUDController.GetPageListViewAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm the action is responding with problem details.
        Assert.Equal((int)HttpStatusCode.InternalServerError, ((ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
        Assert.NotEmpty(((ProblemDetails)((ObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
        Assert.NotEmpty(((ProblemDetails)((ObjectResult)actionResult).Value).Title); //Confirm a title is set.
    }

    /// <summary>
    /// The method verifies the SubUserEditableContoller.GetPageListViewAsync() returns a 200 (OK) response when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyGetPageListViewOkResponse()
    {
        SimpleStandardSubCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        QueryDefinition queryDefinition = new()
        {
            Skip = 0,
            Take = 20,
        };
        SimpleStandardSubCRUDController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.GetPageListViewAsync(OwnerTwo, queryDefinition);

        Assert.IsType<OkObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<PagedList<ListView>>(((OkObjectResult)actionResult).Value); //Confirm the action is responding with a list of list views.
    }
}
