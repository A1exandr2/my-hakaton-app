using System.Runtime.CompilerServices;

namespace Hackathon.Domain.Entities;

public abstract class BaseEntity
{
    public int Id { get; protected set; }
}