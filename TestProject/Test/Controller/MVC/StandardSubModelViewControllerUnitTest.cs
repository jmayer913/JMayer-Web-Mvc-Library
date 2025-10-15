using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestProject.Controller.Mvc;
using TestProject.Data;
using TestProject.Database;

namespace TestProject.Test.Controller.Mvc;

#warning I think I can cheat to test the string id methods. Try this in the big refactor.

/// <summary>
/// The class manages tests for the StandardSubModelViewContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleStandardSubModelViewController object which inherits from the StandardSubModelViewContoller and
/// the SimpleStandardSubModelViewController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the StandardSubModelViewContoller class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the string ID methods 
/// (DeleteAsync(), DeletePartialViewAsync(), DeleteViewAsync(), EditPartialViewAsync and EditViewAsync()) 
/// are not tested.
/// 
/// StandardSubModelViewContoller class inherits from the StandardModelViewController. Because of this,
/// only new and overridden methods in the StandardSubModelViewContoller are tested because
/// the StandardModelViewControllerUnitTest already tests the StandardModelViewController.
/// 
/// Not all negative responses can be tested and adding a property to force a certain response
/// is risky so if there's missing fact/theory that's why.
/// </remarks>
public class StandardSubModelViewControllerUnitTest
{
    /// <summary>
    /// The constant for the default id.
    /// </summary>
    private const int DefaultId = 1;

    /// <summary>
    /// The constant for the default name.
    /// </summary>
    private const string DefaultName = "A Name";

    /// <summary>
    /// The constant for the default owner id.
    /// </summary>
    private const int DefaultOwnerId = 1;

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
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleStandardSubModelViewController>();
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
    /// The method verifies the StandardModelViewContoller.AddPartialViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyAddPartialView()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        SimpleStandardSubModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.AddPartialViewAsync(DefaultOwnerId);

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleSubUserEditableDataObject).Name}AddPartial", ((PartialViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.Null(((PartialViewResult)actionResult).Model); //Confirm there's no model.
        Assert.NotEmpty(((PartialViewResult)actionResult).ViewData); //Confirm there's view data; owner id is stored here.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.AddViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyAddView()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        SimpleStandardSubModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.AddViewAsync(DefaultOwnerId);

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleSubUserEditableDataObject).Name}Add", ((ViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.Null(((ViewResult)actionResult).Model); //Confirm there's no model.
        Assert.NotEmpty(((ViewResult)actionResult).ViewData); //Confirm there's view data; owner id is stored here.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a RedirectToActionResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnRedirectOnSuccess()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        SimpleStandardSubModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.CreateAsync(new SimpleSubUserEditableDataObject() { Name = DefaultName });

        Assert.IsType<RedirectToActionResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal(nameof(Index), ((RedirectToActionResult)actionResult).ActionName); //Confirm the redirect is for Index.
        Assert.Null(((RedirectToActionResult)actionResult).ControllerName); //Confirm there's no controller name.
        Assert.NotEmpty(((RedirectToActionResult)actionResult).RouteValues); //Confirm there's additional route values; owner id is injected into the redirect.
        Assert.Null(((RedirectToActionResult)actionResult).Fragment); //Confirm there's no fragment.
        Assert.False(((RedirectToActionResult)actionResult).Permanent); //Confirm the redirect isn't permanent.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeleteAsync() return a RedirectToActionResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyDeleteReturnRedirectOnSuccess()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = DefaultName });
        SimpleStandardSubModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.DeleteAsync(DefaultId);

        Assert.IsType<RedirectToActionResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal(nameof(Index), ((RedirectToActionResult)actionResult).ActionName); //Confirm the redirect is for Index.
        Assert.Null(((RedirectToActionResult)actionResult).ControllerName); //Confirm there's no controller name.
        Assert.NotEmpty(((RedirectToActionResult)actionResult).RouteValues); //Confirm there's additional route values; owner id is injected into the redirect.
        Assert.Null(((RedirectToActionResult)actionResult).Fragment); //Confirm there's no fragment.
        Assert.False(((RedirectToActionResult)actionResult).Permanent); //Confirm the redirect isn't permanent.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.IndexAsync() returns a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyIndex()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer, OwnerOne);
        await PopulateDataObjects(dataLayer, OwnerTwo);

        SimpleStandardSubModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.IndexAsync(OwnerOne);

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleSubUserEditableDataObject).Name}{nameof(Index)}", ((ViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.IsType<List<SimpleSubUserEditableDataObject>>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a RedirectToActionResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnRedirectOnSuccess()
    {
        SimpleSubUserEditableDataLayer dataLayer = new();
        SimpleSubUserEditableDataObject dataObject = await dataLayer.CreateAsync(new SimpleSubUserEditableDataObject() { Name = DefaultName });
        SimpleStandardSubModelViewController controller = new(dataLayer, CreateConsoleLogger());

        dataObject.Value += 1;
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<RedirectToActionResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal(nameof(Index), ((RedirectToActionResult)actionResult).ActionName); //Confirm the redirect is for Index.
        Assert.Null(((RedirectToActionResult)actionResult).ControllerName); //Confirm there's no controller name.
        Assert.NotEmpty(((RedirectToActionResult)actionResult).RouteValues); //Confirm there's additional route values; owner id is injected into the redirect.
        Assert.Null(((RedirectToActionResult)actionResult).Fragment); //Confirm there's no fragment.
        Assert.False(((RedirectToActionResult)actionResult).Permanent); //Confirm the redirect isn't permanent.
    }
}
