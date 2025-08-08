using ChatWithAPIDemo.Models;

namespace ChatWithAPIDemo.Services;

public class PersonService
{
    private readonly List<Person> _persons;

    public PersonService(List<Person> persons)
    {
        _persons = persons;
    }

    public Person? FindPersonByEmail(string email)
    {
        return _persons.FirstOrDefault(p => p.Email == email);
    }

    public Person? FindPersonByPhone(string phone)
    {
        return _persons.FirstOrDefault(p => p.Phone == phone);
    }

    public Person CreatePerson(string firstName, string lastName, string email, string? phone = null)
    {
        var person = new Person
        {
            Id = Guid.NewGuid(), // Otomatik GUID ataması
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            LoyaltyPoints = 0
        };
        _persons.Add(person);
        return person;
    }

    public void AddLoyaltyPoints(Guid personId, int points)
    {
        var person = _persons.FirstOrDefault(p => p.Id == personId);
        if (person != null)
        {
            person.LoyaltyPoints = (person.LoyaltyPoints ?? 0) + points;
        }
    }

    public Person? FindPersonById(Guid id)
    {
        return _persons.FirstOrDefault(p => p.Id == id);
    }
}