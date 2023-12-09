using JMayer.Data.Data;
using JMayer.Data.Database.DataLayer;
using JMayer.Data.HTTP.DataLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace JMayer.Web.Mvc.Controllers
{
    /// <summary>
    /// The class manages HTTP requests for CRUD operations associated with a data object & a data layer.
    /// </summary>
    /// <typeparam name="T">Must be a DataObject since the data layer requires this.</typeparam>
    /// <typeparam name="U">Must be an IDataLayer so the controller can interact with the collection/table associated with it.</typeparam>
    [ApiController]
    [Route("api/[controller]")]
    public class StandardCRUDController<T, U> : ControllerBase 
        where T : DataObject
        where U : Data.Database.DataLayer.IDataLayer<T>
    {
        /// <summary>
        /// The data layer the controller will interact with.
        /// </summary>
        private Data.Database.DataLayer.IDataLayer<T> _dataLayer;

        /// <summary>
        /// The logger the controller will interact with.
        /// </summary>
        private ILogger _logger;

        /// <summary>
        /// The name of the data object.
        /// </summary>
        private readonly string _dataObjectTypeName = typeof(T).Name;

        /// <summary>
        /// The dependency injection constructor.
        /// </summary>
        /// <param name="dataLayer">The data layer the controller will interact with.</param>
        /// <param name="logger">The logger the controller will interact with.</param>
        public StandardCRUDController(Data.Database.DataLayer.IDataLayer<T> dataLayer, ILogger logger)
        {
            _dataLayer = dataLayer;
            _logger = logger;
        }

        /// <summary>
        /// The method creates a data object using the data layer.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <returns>The created data object.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] T dataObject)
        {
            try
            {
                dataObject = await _dataLayer.CreateAsync(dataObject);
                _logger.LogInformation($"The {_dataObjectTypeName} was successfully created.");
                return Ok(dataObject);
            }
            catch (DataObjectValidationException ex)
            {
                ServerSideValidationResult serverSideValidationResult = new(ex.ValidationResults);
                _logger.LogWarning(ex, $"Failed to create the {_dataObjectTypeName} because of a server-side validation error.");
                return BadRequest(serverSideValidationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create the {_dataObjectTypeName}.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method deletes a data object using the data layer.
        /// </summary>
        /// <param name="integerID">The id for the data object.</param>
        /// <returns>An IActionResult object.</returns>
        [HttpDelete("{integerID:long}")]
        public async Task<IActionResult> DeleteAsync(long integerID)
        {
            try
            {
                T? dataObject = await _dataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);

                if (dataObject != null)
                {
                    await _dataLayer.DeleteAsync(dataObject);
                    _logger.LogInformation($"The {integerID} for the {_dataObjectTypeName} was successfully deleted.");
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete the {integerID} {_dataObjectTypeName}.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method deletes a data object using the data layer.
        /// </summary>
        /// <param name="stringID">The id for the data object.</param>
        /// <returns>An IActionResult object.</returns>
        [HttpDelete("{stringID:string}")]
        public async Task<IActionResult> DeleteAsync(string stringID)
        {
            try
            {
                T? dataObject = await _dataLayer.GetSingleAsync(obj => obj.StringID == stringID);

                if (dataObject != null)
                {
                    await _dataLayer.DeleteAsync(dataObject);
                    _logger.LogInformation($"The {stringID} for the {_dataObjectTypeName} was successfully deleted.");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete the {stringID} {_dataObjectTypeName}.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method returns all the data objects using the data layer.
        /// </summary>
        /// <returns>A list of data objects.</returns>
        [HttpGet("All")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                List<T> dataObjects = await _dataLayer.GetAllAsync();
                return Ok(dataObjects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to return all the {_dataObjectTypeName} data objects.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method returns the first data object using the data layer.
        /// </summary>
        /// <returns>A data object.</returns>
        [HttpGet("Single")]
        public async Task<IActionResult> GetSingleAsync()
        {
            try
            {
                T? dataObject = await _dataLayer.GetSingleAsync();
                return Ok(dataObject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to return the first {_dataObjectTypeName} data object.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method returns a data object based on the ID using the data layer.
        /// </summary>
        /// <param name="integerID">The id to search for.</param>
        /// <returns>A data object.</returns>
        [HttpGet("Single/{integerID:long}")]
        public async Task<IActionResult> GetSingleAsync(long integerID)
        {
            try
            {
                T? dataObject = await _dataLayer.GetSingleAsync(obj => obj.Integer64ID == integerID);
                return Ok(dataObject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to return the {integerID} {_dataObjectTypeName} data object.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method returns a data object based on the ID using the data layer.
        /// </summary>
        /// <param name="stringID">The id to search for.</param>
        /// <returns>A data object.</returns>
        [HttpGet("Single/{stringID:string}")]
        public async Task<IActionResult> GetSingleAsync(string stringID)
        {
            try
            {
                T? dataObject = await _dataLayer.GetSingleAsync(obj => obj.StringID == stringID);
                return Ok(dataObject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to return the {stringID} {_dataObjectTypeName} data object.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method updates a data object using the data layer.
        /// </summary>
        /// <param name="dataObject">The data object to update.</param>
        /// <returns>The updated data object.</returns>
        public async Task<IActionResult> UpdateAsync([FromBody] T dataObject)
        {
            try
            {
                dataObject = await _dataLayer.UpdateAsync(dataObject);
                _logger.LogInformation($"The {_dataObjectTypeName} was successfully updated.");
                return Ok(dataObject);
            }
            catch (DataObjectUpdateConflictException ex)
            {
                _logger.LogWarning(ex, $"Failed to update {_dataObjectTypeName} because the data was considered old.");
                return Conflict();
            }
            catch (DataObjectValidationException ex)
            {
                ServerSideValidationResult serverSideValidationResult = new(ex.ValidationResults);
                _logger.LogWarning(ex, $"Failed to update the {_dataObjectTypeName} because of a server-side validation error.");
                return BadRequest(serverSideValidationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update the {_dataObjectTypeName}.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// The method validates a data object using the data layer.
        /// </summary>
        /// <param name="dataObject">The data object to validated.</param>
        /// <returns>The validation result.</returns>
        public async Task<IActionResult> ValidateAsync([FromBody] T dataObject)
        {
            try
            {
                List<ValidationResult> validationResults = await _dataLayer.ValidateAsync(dataObject);
                ServerSideValidationResult serverSideValidationResult = new(validationResults);
                _logger.LogInformation($"The {_dataObjectTypeName} was successfully validated.");
                return Ok(serverSideValidationResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to validate the {_dataObjectTypeName}.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
