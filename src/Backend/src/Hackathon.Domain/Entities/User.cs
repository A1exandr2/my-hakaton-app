using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using Hackathon.Domain.Exceptions;

namespace Hackathon.Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public string? Tg { get; set; }

    public User(string email, string? tg)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("email is null");
        if (string.IsNullOrWhiteSpace(tg))
            throw new DomainException("email is null");
        Email = email;
        Tg = tg;
    }
}