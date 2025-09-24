using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestProject.Controller;
using TestProject.Data;
using TestProject.Database;

#warning The protected properties are an issue with the unit tests. It means I need two controllers (Pure MVC & Hybrid MVC & Ajax) to test different features.

namespace TestProject.Test.Controller.MVC;

/// <summary>
/// The class manages tests for the StandardModelViewContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleStandardModelViewController object which inherits from the StandardModelViewContoller and
/// the SimpleStandardModelViewController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the StandardModelViewContoller class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the two string ID methods 
/// (DeleteAsync() and SingleAsync()) are not tested.
/// 
/// Not all negative responses can be tested and adding a property to force a certain response
/// is risky so if there's missing fact/theory that's why.
/// </remarks>
public class StandardModelViewControllerUnitTest
{
    /// <summary>
    /// The constant for the default id.
    /// </summary>
    private const int DefaultId = 1;

    /// <summary>
    /// The constant for the maximum records.
    /// </summary>
    private const int MaxRecords = 100;

    /// <summary>
    /// The constant for the invalid id.
    /// </summary>
    private const int InvalidId = 9999;

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
    /// The method verifies the StandardModelViewContoller.AddPartialViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyAddPartialView()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.AddPartialViewAsync();

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleDataObject).Name}AddPartial", ((PartialViewResult)actionResult).ViewName); //Confirm the partial view result has SimpleDataObjectAddPartial for the name.
        Assert.Null(((PartialViewResult)actionResult).Model); //Confirm the partial view result has a null model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.AddViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyAddView()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.AddViewAsync();

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Add", ((ViewResult)actionResult).ViewName); //Confirm the view result has SimpleDataObjectAdd for the name.
        Assert.Null(((ViewResult)actionResult).Model); //Confirm the view result has a null model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeletePartialViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyDeletePartialView()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.DeletePartialViewAsync(DefaultId);

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleDataObject).Name}DeletePartial", ((PartialViewResult)actionResult).ViewName); //Confirm the partial view result has SimpleDataObjectDeletePartial for the name.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm the partial view result has a single SimpleDataObject object for the model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeleteViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyDeleteView()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.DeleteViewAsync(DefaultId);

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Delete", ((ViewResult)actionResult).ViewName); //Confirm the view result has SimpleDataObjectDelete for the name.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm the view result has a single SimpleDataObject object for the model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.EditPartialViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyEditPartialView()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.EditPartialViewAsync(DefaultId);

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleDataObject).Name}EditPartial", ((PartialViewResult)actionResult).ViewName); //Confirm the partial view result has SimpleDataObjectEditPartial for the name.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm the partial view result has a single SimpleDataObject object for the model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.EditViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyEditView()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.EditViewAsync(DefaultId);

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Edit", ((ViewResult)actionResult).ViewName); //Confirm the view result has SimpleDataObjectEdit for the name.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm the view result has a single SimpleDataObject object for the model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.IndexAsync() returns a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyIndexViewReturnViewResult()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer);

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.IndexAsync();

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Index", ((ViewResult)actionResult).ViewName); //Confirm the view result has SimpleDataObjectIndex for the name.
        Assert.IsType<List<SimpleDataObject>>(((ViewResult)actionResult).Model); //Confirm the view result has a list of SimpleDataObject objects for the model.
    }
}
