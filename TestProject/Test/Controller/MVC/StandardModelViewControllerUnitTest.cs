using JMayer.Data.HTTP.Details;
using JMayer.Web.Mvc.Controller.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using TestProject.Controller.Mvc;
using TestProject.Data;
using TestProject.Database;

#warning I think I can cheat to test the string id methods. Try this in the big refactor.
#warning Could not test the delete conflict. The cheat I originally did in the api version doesn't work in the mvc version.

namespace TestProject.Test.Controller.Mvc;

/// <summary>
/// The class manages tests for the StandardModelViewContoller object.
/// </summary>
/// <remarks>
/// The tests are against a SimpleStandardModelViewController object which inherits from the StandardModelViewContoller and
/// the SimpleStandardModelViewController doesn't override any of the base methods. Because of this, we're testing 
/// the methods in the StandardModelViewContoller class.
/// 
/// The memory data layer is used and that utilizes an integer ID so the string ID methods 
/// (DeleteAsync(), DeletePartialViewAsync(), DeleteViewAsync(), EditPartialViewAsync and EditViewAsync()) 
/// are not tested.
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
    /// The constant for the default value.
    /// </summary>
    private const int DefaultValue = 1;

    /// <summary>
    /// The constant for the maximum records.
    /// </summary>
    private const int MaxRecords = 100;

    /// <summary>
    /// The constant for the invalid id.
    /// </summary>
    private const int InvalidId = 9999;

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
        return LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<SimpleStandardModelViewController>();
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
        Assert.Equal($"_{typeof(SimpleDataObject).Name}AddPartial", ((PartialViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.Null(((PartialViewResult)actionResult).Model); //Confirm there's no model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.AddViewAsync() return a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyAddView()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.AddViewAsync();

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Add", ((ViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.Null(((ViewResult)actionResult).Model); //Confirm there's no model.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a JsonResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnJsonOnSuccess()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsCUDActionRedirectedOnSuccess = false,
        };
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = DefaultValue });

        Assert.IsType<JsonResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<SimpleDataObject>(((JsonResult)actionResult).Value); //Confirm there's a simple data object.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a ObjectResult when the data layer has a validation error with the model.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnJsonOnDataLayerValidationError()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnJson,
        };
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = InvalidValue });

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(((ObjectResult)actionResult).Value); //Confirm there's a validation problem details.
        Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ValidationProblemDetails)((ObjectResult)actionResult).Value).Errors); //Confirm the validation problem details is not empty.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a ObjectResult when the model is invalid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnJsonOnInvalidModel()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnJson,
        };

        //Force a model error. The model state is handled by asp.net core and we're not testing that.
        controller.ModelState.AddModelError(nameof(SimpleDataObject.Value), "Range");
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = InvalidValue });

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(((ObjectResult)actionResult).Value); //Confirm there's a validation problem details.
        Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ValidationProblemDetails)((ObjectResult)actionResult).Value).Errors); //Confirm the validation problem details is not empty.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a ObjectResult when an unexpected exception occurs.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyCreateReturnJsonOnError(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details
        };
        IActionResult actionResult = await controller.CreateAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.

        if (details)
        {
            Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm there's a problem details.
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((Microsoft.AspNetCore.Mvc.ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ProblemDetails)((ObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ProblemDetails)((ObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a PartialViewResult when the data layer has a validation error with the model.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnPartialViewOnDataLayerValidationError()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnPartialView,
        };
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = InvalidValue });

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleDataObject).Name}AddPartial", ((PartialViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a PartialViewResult when the model is invalid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnPartialViewOnInvalidModel()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnPartialView,
        };

        //Force a model error. The model state is handled by asp.net core and we're not testing that.
        controller.ModelState.AddModelError(nameof(SimpleDataObject.Value), "Range");
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = InvalidValue });

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleDataObject).Name}AddPartial", ((PartialViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a RedirectToActionResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnRedirectOnSuccess()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = DefaultValue });

        Assert.IsType<RedirectToActionResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal(nameof(Index), ((RedirectToActionResult)actionResult).ActionName); //Confirm the redirect is for Index.
        Assert.Null(((RedirectToActionResult)actionResult).ControllerName); //Confirm there's no controller name.
        Assert.Null(((RedirectToActionResult)actionResult).RouteValues); //Confirm there's no additional route values.
        Assert.Null(((RedirectToActionResult)actionResult).Fragment); //Confirm there's no fragment.
        Assert.False(((RedirectToActionResult)actionResult).Permanent); //Confirm the redirect isn't permanent.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a ViewResult when the data layer has a validation error with the model.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnViewOnDataLayerValidationError()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = InvalidValue });

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Add", ((ViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.CreateAsync() return a ViewResult when the model is invalid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyCreateReturnViewOnInvalidModel()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());

        //Force a model error. The model state is handled by asp.net core and we're not testing that.
        controller.ModelState.AddModelError(nameof(SimpleDataObject.Value), "Range");
        IActionResult actionResult = await controller.CreateAsync(new SimpleDataObject() { Value = InvalidValue });

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Add", ((ViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
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
        Assert.Equal($"_{typeof(SimpleDataObject).Name}DeletePartial", ((PartialViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeletePartialViewAsync() return a NotFoundObjectResult when the data object doesn't exist.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyDeletePartialViewReturnNotFound(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details
        };
        IActionResult actionResult = await controller.DeletePartialViewAsync(InvalidId);

        if (details)
        {
            Assert.IsType<NotFoundObjectResult>(actionResult); //Confirm the correct action is returned.
            Assert.IsType<NotFoundDetails>(((NotFoundObjectResult)actionResult).Value); //Confirm the action is responding with not found details.
            Assert.Equal((int)HttpStatusCode.NotFound, ((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
        else
        {
            Assert.IsType<NotFoundResult>(actionResult); //Confirm the correct action is returned.
        }
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeleteAsync() return a ConflictObjectResult when the data object doesn't exist.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    //[Theory]
    //[InlineData(true)]
    //[InlineData(false)]
    //public async Task VerifyDeleteReturnConflict(bool details)
    //{
    //    SimpleStandardCRUDDataLayer dataLayer = new();
    //    _ = await dataLayer.CreateAsync(new SimpleDataObject()
    //    {
    //        //This is a cheat to force a delete conflict.
    //        Integer64ID = SimpleStandardCRUDDataLayer.DeleteConflictId,
    //    });

    //    SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
    //    {
    //        IsDetailsIncludedInNegativeResponse = details
    //    };
    //    IActionResult actionResult = await controller.DeleteAsync(InvalidId);

    //    if (details)
    //    {
    //        Assert.IsType<ConflictObjectResult>(actionResult); //Confirm the correct action is returned.
    //        Assert.IsType<ConflictDetails>(((ConflictObjectResult)actionResult).Value); //Confirm the action is responding with conflict details.
    //        Assert.Equal((int)HttpStatusCode.Conflict, ((ConflictDetails)((ConflictObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
    //        Assert.NotEmpty(((ConflictDetails)((ConflictObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
    //        Assert.NotEmpty(((ConflictDetails)((ConflictObjectResult)actionResult).Value).Title); //Confirm a title is set.
    //    }
    //    else
    //    {
    //        Assert.IsType<ConflictResult>(actionResult); //Confirm the correct action is returned.
    //    }
    //}

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeleteAsync() return a JsonResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyDeleteReturnJsonOnSuccess()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsCUDActionRedirectedOnSuccess = false,
        };
        IActionResult actionResult = await controller.DeleteAsync(DefaultId);

        Assert.IsType<JsonResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<SimpleDataObject>(((JsonResult)actionResult).Value); //Confirm there's a simple data object.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeleteAsync() return a NotFoundObjectResult when the data object doesn't exist.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyDeleteReturnNotFound(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details,
        };
        IActionResult actionResult = await controller.DeleteAsync(InvalidId);

        if (details)
        {
            Assert.IsType<NotFoundObjectResult>(actionResult); //Confirm the correct action is returned.
            Assert.IsType<NotFoundDetails>(((NotFoundObjectResult)actionResult).Value); //Confirm the action is responding with not found details.
            Assert.Equal((int)HttpStatusCode.NotFound, ((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
        else
        {
            Assert.IsType<NotFoundResult>(actionResult);
        }
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeleteAsync() return a RedirectToActionResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyDeleteReturnRedirectSuccess()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.DeleteAsync(DefaultId);

        Assert.IsType<RedirectToActionResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal(nameof(Index), ((RedirectToActionResult)actionResult).ActionName); //Confirm the redirect is for Index.
        Assert.Null(((RedirectToActionResult)actionResult).ControllerName); //Confirm there's no controller name.
        Assert.Null(((RedirectToActionResult)actionResult).RouteValues); //Confirm there's no additional route values.
        Assert.Null(((RedirectToActionResult)actionResult).Fragment); //Confirm there's no fragment.
        Assert.False(((RedirectToActionResult)actionResult).Permanent); //Confirm the redirect isn't permanent.
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
        Assert.Equal($"{typeof(SimpleDataObject).Name}Delete", ((ViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.DeleteViewAsync() return a NotFoundObjectResult when the data object doesn't exist.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyDeleteViewReturnNotFound(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details
        };
        IActionResult actionResult = await controller.DeleteViewAsync(InvalidId);

        if (details)
        {
            Assert.IsType<NotFoundObjectResult>(actionResult); //Confirm the correct action is returned.
            Assert.IsType<NotFoundDetails>(((NotFoundObjectResult)actionResult).Value); //Confirm the action is responding with not found details.
            Assert.Equal((int)HttpStatusCode.NotFound, ((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
        else
        {
            Assert.IsType<NotFoundResult>(actionResult); //Confirm the correct action is returned.
        }
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
        Assert.Equal($"_{typeof(SimpleDataObject).Name}EditPartial", ((PartialViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.EditPartialViewAsync() return a NotFoundObjectResult when the data object doesn't exist.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyEditPartialViewReturnNotFound(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details
        };
        IActionResult actionResult = await controller.EditPartialViewAsync(InvalidId);

        if (details)
        {
            Assert.IsType<NotFoundObjectResult>(actionResult); //Confirm the correct action is returned.
            Assert.IsType<NotFoundDetails>(((NotFoundObjectResult)actionResult).Value); //Confirm the action is responding with not found details.
            Assert.Equal((int)HttpStatusCode.NotFound, ((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
        else
        {
            Assert.IsType<NotFoundResult>(actionResult); //Confirm the correct action is returned.
        }
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
        Assert.Equal($"{typeof(SimpleDataObject).Name}Edit", ((ViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.EditViewAsync() return a NotFoundObjectResult when the data object doesn't exist.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyEditViewReturnNotFound(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details
        };
        IActionResult actionResult = await controller.EditViewAsync(InvalidId);

        if (details)
        {
            Assert.IsType<NotFoundObjectResult>(actionResult); //Confirm the correct action is returned.
            Assert.IsType<NotFoundDetails>(((NotFoundObjectResult)actionResult).Value); //Confirm the action is responding with not found details.
            Assert.Equal((int)HttpStatusCode.NotFound, ((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
        else
        {
            Assert.IsType<NotFoundResult>(actionResult); //Confirm the correct action is returned.
        }
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.IndexAsync() returns a ViewResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyIndex()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        await PopulateDataObjects(dataLayer);

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());
        IActionResult actionResult = await controller.IndexAsync();

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}{nameof(Index)}", ((ViewResult)actionResult).ViewName); //Confirm the view's name.
        Assert.IsType<List<SimpleDataObject>>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() returns a ConflictObjectResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyUpdateReturnConflict(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new()
        {
            IsOldDataObjectDetectionEnabled = true,
        };
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Name = "Update Conflict Test", Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsCUDActionRedirectedOnSuccess = false,
            IsDetailsIncludedInNegativeResponse = details,
        };

        dataObject.Value += 1;
        SimpleDataObject oldDataObject = new(dataObject);

        _ = await controller.UpdateAsync(dataObject);
        IActionResult actionResult = await controller.UpdateAsync(oldDataObject);

        if (details)
        {
            Assert.IsType<ConflictObjectResult>(actionResult); //Confirm the correct action is returned.
            Assert.IsType<ConflictDetails>(((ConflictObjectResult)actionResult).Value); //Confirm the action is responding with conflict details.
            Assert.Equal((int)HttpStatusCode.Conflict, ((ConflictDetails)((ConflictObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((ConflictDetails)((ConflictObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((ConflictDetails)((ConflictObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
        else
        {
            Assert.IsType<ConflictResult>(actionResult); //Confirm the correct action is returned.
        }
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a JsonResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnJsonOnSuccess()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsCUDActionRedirectedOnSuccess = false,
        };

        dataObject.Value += 1;
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<JsonResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<SimpleDataObject>(((JsonResult)actionResult).Value); //Confirm there's a simple data object.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a ObjectResult when the data layer has a validation error with the model.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnJsonOnDataLayerValidationError()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnJson,
        };

        dataObject.Value = InvalidValue;
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(((ObjectResult)actionResult).Value); //Confirm there's a validation problem details.
        Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ValidationProblemDetails)((ObjectResult)actionResult).Value).Errors); //Confirm the validation problem details is not empty.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a ObjectResult when the model is invalid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnJsonOnInvalidModel()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnJson,
        };

        dataObject.Value = InvalidValue;
        //Force a model error. The model state is handled by asp.net core and we're not testing that.
        controller.ModelState.AddModelError(nameof(SimpleDataObject.Value), "Range");
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.
        Assert.IsType<Microsoft.AspNetCore.Mvc.ValidationProblemDetails>(((ObjectResult)actionResult).Value); //Confirm there's a validation problem details.
        Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ValidationProblemDetails)((ObjectResult)actionResult).Value).Errors); //Confirm the validation problem details is not empty.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a ObjectResult when an unexpected exception occurs.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyUpdateReturnJsonOnError(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details
        };
        IActionResult actionResult = await controller.UpdateAsync(null);

        Assert.IsType<ObjectResult>(actionResult); //Confirm the correct action is returned.

        if (details)
        {
            Assert.IsType<Microsoft.AspNetCore.Mvc.ProblemDetails>(((ObjectResult)actionResult).Value); //Confirm there's a problem details.
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((Microsoft.AspNetCore.Mvc.ProblemDetails)((ObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ProblemDetails)((ObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((Microsoft.AspNetCore.Mvc.ProblemDetails)((ObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a NotFoundObjectResult when the data object doesn't exist.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyUpdateReturnNotFound(bool details)
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        _ = await dataLayer.CreateAsync(new SimpleDataObject());

        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            IsDetailsIncludedInNegativeResponse = details,
        };
        IActionResult actionResult = await controller.UpdateAsync(new SimpleDataObject() { Integer64ID = InvalidId });

        if (details)
        {
            Assert.IsType<NotFoundObjectResult>(actionResult); //Confirm the correct action is returned.
            Assert.IsType<NotFoundDetails>(((NotFoundObjectResult)actionResult).Value); //Confirm the action is responding with not found details.
            Assert.Equal((int)HttpStatusCode.NotFound, ((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Status); //Confirm the correct HTTP status code is returned.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Detail); //Confirm a detail is set.
            Assert.NotEmpty(((NotFoundDetails)((NotFoundObjectResult)actionResult).Value).Title); //Confirm a title is set.
        }
        else
        {
            Assert.IsType<NotFoundResult>(actionResult); //Confirm the correct action is returned.
        }
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a PartialViewResult when the data layer has a validation error with the model.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnPartialViewOnDataLayerValidationError()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnPartialView,
        };

        dataObject.Value = InvalidValue;
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleDataObject).Name}EditPartial", ((PartialViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a PartialViewResult when the model is invalid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnPartialViewOnInvalidModel()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger())
        {
            ValidationFailedAction = ValidationFailedAction.ReturnPartialView,
        };

        dataObject.Value = InvalidValue;
        //Force a model error. The model state is handled by asp.net core and we're not testing that.
        controller.ModelState.AddModelError(nameof(SimpleDataObject.Value), "Range");
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<PartialViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"_{typeof(SimpleDataObject).Name}EditPartial", ((PartialViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((PartialViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a RedirectToActionResult when ran successfully.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnRedirectOnSuccess()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());

        dataObject.Value += 1;
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<RedirectToActionResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal(nameof(Index), ((RedirectToActionResult)actionResult).ActionName); //Confirm the redirect is for Index.
        Assert.Null(((RedirectToActionResult)actionResult).ControllerName); //Confirm there's no controller name.
        Assert.Null(((RedirectToActionResult)actionResult).RouteValues); //Confirm there's no additional route values.
        Assert.Null(((RedirectToActionResult)actionResult).Fragment); //Confirm there's no fragment.
        Assert.False(((RedirectToActionResult)actionResult).Permanent); //Confirm the redirect isn't permanent.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a ViewResult when the data layer has a validation error with the model.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnViewOnDataLayerValidationError()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());

        dataObject.Value = InvalidValue;
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Edit", ((ViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }

    /// <summary>
    /// The method verifies the StandardModelViewContoller.UpdateAsync() return a ViewResult when the model is invalid.
    /// </summary>
    /// <returns>A Task object for the async.</returns>
    [Fact]
    public async Task VerifyUpdateReturnViewOnInvalidModel()
    {
        SimpleStandardCRUDDataLayer dataLayer = new();
        SimpleDataObject dataObject = await dataLayer.CreateAsync(new SimpleDataObject() { Value = DefaultValue });
        SimpleStandardModelViewController controller = new(dataLayer, CreateConsoleLogger());

        dataObject.Value = InvalidValue;
        //Force a model error. The model state is handled by asp.net core and we're not testing that.
        controller.ModelState.AddModelError(nameof(SimpleDataObject.Value), "Range");
        IActionResult actionResult = await controller.UpdateAsync(dataObject);

        Assert.IsType<ViewResult>(actionResult); //Confirm the correct action is returned.
        Assert.Equal($"{typeof(SimpleDataObject).Name}Edit", ((ViewResult)actionResult).ViewName); //Confirm there's a validation problem details.
        Assert.IsType<SimpleDataObject>(((ViewResult)actionResult).Model); //Confirm there's a model and its the correct type.
    }
}
