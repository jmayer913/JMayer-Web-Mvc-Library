using JMayer.Data.Data;
using System.ComponentModel.DataAnnotations;

namespace TestProject.Data;

/// <summary>
/// The represents a very simple data object to be used for testing.
/// </summary>
internal class SimpleDataObject : DataObject
{
    /// <summary>
    /// The property gets/sets a value associated with the simple data object.
    /// </summary>
    [Range(0, 100)]
    public int Value { get; set; }

    /// <summary>
    /// The default constructor.
    /// </summary>
    public SimpleDataObject() { }

    /// <summary>
    /// The copy constructor.
    /// </summary>
    /// <param name="copy">The copy.</param>
    public SimpleDataObject(SimpleDataObject copy) => MapProperties(copy);

    /// <summary>
    /// The method maps a SimpleDataObject to this object.
    /// </summary>
    /// <param name="dataObject">The data object.</param>
    public override void MapProperties(DataObject dataObject)
    {
        base.MapProperties(dataObject);

        if (dataObject is SimpleDataObject simpleDataObject)
        {
            Value = simpleDataObject.Value;
        }
    }
}
