using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using ChatWithAPIDemo.Services;

namespace ChatWithAPIDemo.Plugins
{
    public class PPersonPlugin
    {
        private readonly PersonService _personService;
        private static readonly JsonSerializerOptions J = new() { WriteIndented = true };

        public PPersonPlugin(PersonService personService) => _personService = personService;

        [KernelFunction, Description("Find a person by email")]
        public string FindByEmail([Description("Email address")] string email)
            => JsonSerializer.Serialize(_personService.FindPersonByEmail(email), J);

        [KernelFunction, Description("Find a person by phone number")]
        public string FindByPhone([Description("Phone number")] string phone)
            => JsonSerializer.Serialize(_personService.FindPersonByPhone(phone), J);

        [KernelFunction, Description("Find a person by id")]
        public string FindById([Description("Person id")] int personId)
            => JsonSerializer.Serialize(_personService.FindPersonById(personId), J);

        [KernelFunction, Description("Create a new person")]
        public string CreatePerson(
             [Description("id")] int id,
            [Description("First name")] string firstName,
            [Description("Last name")] string lastName,
            [Description("Email")] string email,
            [Description("Phone (optional)")] string? phone = null)
            => JsonSerializer.Serialize(_personService.CreatePerson(id,firstName, lastName, email, phone), J);

        [KernelFunction, Description("Add loyalty points to a person")]
        public string AddLoyaltyPoints(
              [Description("Person id")] int personId,
              [Description("Points to add (int)")] int points)
        {
            _personService.AddLoyaltyPoints(personId, points);
            var updated = _personService.FindPersonById(personId);
            return updated is null
                ? JsonSerializer.Serialize(new { error = "Person not found" }, J)
                : JsonSerializer.Serialize(updated, J);
        }
    }
}