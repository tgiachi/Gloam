using Gloam.Data.Entities.Base;

namespace Gloam.Data.Interfaces.Loader;

/// <summary>
///     Provides functionality for loading and managing entity data from various sources.
/// </summary>
public interface IEntityDataLoader
{
    /// <summary>
    ///     Subscribes to entity load events for entities of a specific type.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to subscribe to.</typeparam>
    /// <param name="onEntityLoaded">The callback function to invoke when an entity is loaded.</param>
    void SubscribeToEntityLoad<TEntity>(Func<TEntity, ValueTask> onEntityLoaded) where TEntity : BaseGloamEntity;

    /// <summary>
    ///     Asynchronously loads entities from the specified path.
    /// </summary>
    /// <param name="path">The path to load entities from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    ValueTask LoadEntitiesAsync(string path, CancellationToken cancellationToken = default);
}
