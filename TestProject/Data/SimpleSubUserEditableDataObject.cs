using JMayer.Data.Data;
using System.ComponentModel.DataAnnotations;

namespace TestProject.Data;

/// <summary>
/// The represents a very simple sub user editable data object to be used for testing.
/// </summary>
public class SimpleSubUserEditableDataObject : SubUserEditableDataObject
{
    /// <summary>
    /// The property gets/sets a value associated with the simple user editable data object.
    /// </summary>
    [Range(0, 100)]
    public int Value { get; set; }

    /// <inheritdoc/>
    public SimpleSubUserEditableDataObject() : base() { }

    /// <inheritdoc/>
    public SimpleSubUserEditableDataObject(SimpleSubUserEditableDataObject copy) : base(copy) { }

    /// <inheritdoc/>
    public override void MapProperties(DataObject dataObject)
    {
        base.MapProperties(dataObject);

        if (dataObject is SimpleSubUserEditableDataObject simpleConfigurationDataObject)
        {
            Value = simpleConfigurationDataObject.Value;
        }
    }
}
