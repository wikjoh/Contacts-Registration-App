﻿using Business.Dtos;
using Business.Factories;
using Business.Interfaces;
using Business.Models;
using System.Diagnostics;

namespace Business.Services;

public class ContactService(IContactRepository contactRepository) : IContactService
{
    private readonly IContactRepository _contactRepository = contactRepository;
    private List<ContactModel> _contacts = contactRepository.ReadFromFile() ?? [];

    public event EventHandler? ContactsUpdated;

    private readonly List<ContactModel> _sampleContactList = new()
    {
        new ContactModel
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            FirstName = "Sample",
            LastName = "Contact",
            Email = "sample.contact@domain.com",
            PhoneNumber = "0700123456",
            StreetAddress = "Sample Street 1",
            PostalCode = 12345,
            City = "Sample City"
        }
    };

    public IEnumerable<ContactModel> GetContacts()
    {
        _contacts = _contactRepository.ReadFromFile() ?? _sampleContactList;
        return _contacts;
    }

    public bool CreateContact(ContactDto contactForm)
    {
        try
        {
            var contactModel = ContactFactory.Create(contactForm);

            if (contactModel != null)
            {
                contactModel.Id = _contacts.Count() != 0 ? _contacts.Last().Id + 1 : 1;

                _contacts.Add(contactModel);
                _contactRepository.SaveToFile(_contacts);
                ContactsUpdated?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to create contact. {ex.Message}");
            return false;
        }
    }




    public bool DeleteContact(int contactId)
    {
        var contactToDelete = _contacts.Find(x => x.Id == contactId);
        if (contactToDelete != null)
        {
            _contacts.Remove(contactToDelete);
            _contactRepository.SaveToFile(_contacts);
            ContactsUpdated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }
    public bool UpdateContact(ContactModel contact)
    {
        var contactToUpdate = _contacts.Find(x => x.Id == contact.Id);
        if (contactToUpdate != null)
        {
            contactToUpdate = contact;
            _contactRepository.SaveToFile(_contacts);
            ContactsUpdated?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return false;
    }
}
