namespace ChatWithAPIDemo.Plugins;

using System.ComponentModel;
using System.Text.Json;
using ChatWithAPIDemo.Models;
using ChatWithAPIDemo.Services;
using Microsoft.SemanticKernel;

namespace ChatWithAPIDemo.Plugins;

public class PersonPlugin
{
    private readonly PersonService _personService;
    private static readonly JsonSerializerOptions J = new() { WriteIndented = true };

    public PersonPlugin(PersonService personService) => _personService = personService;

    [KernelFunction, Description("Find a person by email")]
    public string FindByEmail([Description("Email address")] string email)
        => JsonSerializer.Serialize(_personService.FindPersonByEmail(email), J);

    [KernelFunction, Description("Find a person by phone number")]
    public string FindByPhone([Description("Phone number")] string phone)
        => JsonSerializer.Serialize(_personService.FindPersonByPhone(phone), J);

    [KernelFunction, Description("Find a person by id (GUID)")]
    public string FindById([Description("Person id (GUID)")] string personId)
        => Guid.TryParse(personId, out var id)
            ? JsonSerializer.Serialize(_personService.FindPersonById(id), J)
            : JsonSerializer.Serialize(new { error = "Invalid GUID" }, J);

    [KernelFunction, Description("Create a new person")]
    public string CreatePerson(
        [Description("First name")] string firstName,
        [Description("Last name")] string lastName,
        [Description("Email")] string email,
        [Description("Phone (optional)")] string? phone = null)
        => JsonSerializer.Serialize(_personService.CreatePerson(firstName, lastName, email, phone), J);

    [KernelFunction, Description("Add loyalty points to a person")]
    public string AddLoyaltyPoints(
        [Description("Person id (GUID)")] string personId,
        [Description("Points to add (int)")] int points)
    {
        if (!Guid.TryParse(personId, out var id))
            return JsonSerializer.Serialize(new { error = "Invalid GUID" }, J);

        _personService.AddLoyaltyPoints(id, points);
        var updated = _personService.FindPersonById(id);
        return JsonSerializer.Serialize(updated ?? new { error = "Person not found" }, J);
    }
}
