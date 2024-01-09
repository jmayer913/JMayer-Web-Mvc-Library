using JMayer.Data.Data;
using System.ComponentModel.DataAnnotations;

namespace TestProject.Data;

/// <summary>
/// The represents a very simple user editable data object to be used for testing.
/// </summary>
public class SimpleUserEditableDataObject : UserEditableDataObject
{
    /// <summary>
    /// The property gets/sets a value associated with the simple user editable data object.
    /// </summary>
    [Range(0, 100)]
    public int Value { get; set; }

    /// <summary>
    /// The default constructor.
    /// </summary>
    public SimpleUserEditableDataObject() { }

    /// <summary>
    /// The copy constructor.
    /// </summary>
    /// <param name="copy">The copy.</param>
    public SimpleUserEditableDataObject(SimpleUserEditableDataObject copy) => MapProperties(copy);

    /// <summary>
    /// The method maps a SimpleUserEditableDataObject to this object.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    public override void MapProperties(DataObject dataObject)
    {
        base.MapProperties(dataObject);

        if (dataObject is SimpleUserEditableDataObject simpleConfigurationDataObject)
        {
            Value = simpleConfigurationDataObject.Value;
        }
    }
}
