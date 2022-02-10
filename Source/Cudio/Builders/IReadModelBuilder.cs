namespace Cudio
{
    /// <summary>
    /// Marks a class as a read model builder that uses convention to declare create, update and delete handler.
    /// </summary>
    /// <typeparam name="TTable">The type of the table this builder manages.</typeparam>
    public interface IReadModelBuilder<TTable>
    {
    }
}
