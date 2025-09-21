namespace Hackathon.Application.Exceptions;
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("ресурс не найден") 
        { }

        public NotFoundException(string entityName, object key)
            : base($"сущность: {entityName}, с ключом: {key} не найдена")
        {
            EntityName = entityName;
            Key = key;
        }

        public string? EntityName { get; }
        public object? Key { get; }
    }