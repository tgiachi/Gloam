using Gloam.Core.Json;
using Gloam.Data.Entities.Base;
using Gloam.Data.Interfaces.Content;
using Gloam.Data.Interfaces.Loader;
using Gloam.Data.Interfaces.Validation;
using Serilog;

namespace Gloam.Data.Loaders;

public class EntityDataLoader : IEntityDataLoader
{
    private readonly IContentLoader _contentLoader;

    private readonly Dictionary<Type, List<Func<BaseGloamEntity, ValueTask>>> _entityLoadSubscribers = new();
    private readonly IEntitySchemaValidator _entitySchemaValidator;
    private readonly ILogger _logger = Log.ForContext<EntityDataLoader>();


    public EntityDataLoader(IContentLoader contentLoader, IEntitySchemaValidator entitySchemaValidator)
    {
        _contentLoader = contentLoader;
        _entitySchemaValidator = entitySchemaValidator;
    }


    public void SubscribeToEntityLoad<TEntity>(Func<TEntity, ValueTask> onEntityLoaded) where TEntity : BaseGloamEntity
    {
        ArgumentNullException.ThrowIfNull(onEntityLoaded);
        var entityType = typeof(TEntity);
        if (!_entityLoadSubscribers.TryGetValue(entityType, out var value))
        {
            value = [];
            _entityLoadSubscribers[entityType] = value;
        }

        _logger.Debug("Subscribing to entity load {entityType}", entityType);
        value.Add(entity => onEntityLoaded((TEntity)entity));
    }

    public async ValueTask LoadEntitiesAsync(string path, CancellationToken cancellationToken = default)
    {
        await foreach (var file in _contentLoader.EnumerateFilesAsync(path, "*.json", cancellationToken))
        {
            _logger.Information("Loading entities from {file}", file);

            var fileContent = await _contentLoader.ReadTextAsync(file, cancellationToken);

            var schema = await _entitySchemaValidator.GetSchemaAsync<BaseGloamEntity>(cancellationToken);
            var validationResult = _entitySchemaValidator.Validate(fileContent, schema);
            if (!validationResult.Ok)
            {
                _logger.Error(
                    "Validation failed for file {file}: {errors}",
                    file,
                    string.Join(", ", validationResult.Errors)
                );
                continue;
            }

            var obj = JsonUtils.Deserialize<BaseGloamEntity>(fileContent);

            if (obj == null)
            {
                _logger.Warning("Deserialized object is null for file {file}", file);
                continue;
            }

            var entityType = obj.GetType();
            if (_entityLoadSubscribers.TryGetValue(entityType, out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    await subscriber(obj);
                }
            }
            else
            {
                _logger.Warning("No subscribers found for entity type {entityType}", entityType);
            }
        }
    }
}
