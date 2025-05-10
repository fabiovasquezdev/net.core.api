
using Common;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public abstract record class Entity : Validatable
{
    [BsonElement("_id")]
    public Guid Id { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public string CreatedName { get; init; }
    public bool IsActive { get; init; }
}