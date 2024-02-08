using JMayer.Data.Database.DataLayer.MemoryStorage;
using TestProject.Data;

namespace TestProject.Database;

/// <summary>
/// The class manages CRUD interactions with a list memory storage for the simple user editable data object.
/// </summary>
public class SimpleUserEditableMemoryDataLayer : UserEditableMemoryDataLayer<SimpleUserEditableDataObject>
{
}
