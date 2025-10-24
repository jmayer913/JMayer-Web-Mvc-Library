using JMayer.Data.Data;
using System.ComponentModel.DataAnnotations;

namespace TestProject.Data;

/// <summary>
/// The represents a very simple sub object to be used for testing.
/// </summary>
public class SimpleSubDataObject : SubDataObject
{
    /// <summary>
    /// The property gets/sets a value associated with the simple data object.
    /// </summary>
    [Range(0, 100)]
    public int Value { get; set; }

    /// <inheritdoc/>
    public SimpleSubDataObject() : base() { }

    /// <inheritdoc/>
    public SimpleSubDataObject(SimpleSubDataObject copy) : base(copy) { }

    /// <inheritdoc/>
    public override void MapProperties(DataObject dataObject)
    {
        base.MapProperties(dataObject);

        if (dataObject is SimpleSubDataObject simpleSubDataObject)
        {
            Value = simpleSubDataObject.Value;
        }
    }
}
