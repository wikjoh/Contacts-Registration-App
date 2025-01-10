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


    public IEnumerable<ContactModel> GetContacts()
    {
        List<ContactModel> _contacts = _contactRepository.ReadFromFile() ?? GetSampleContacts();
        return _contacts;
    }


    public List<ContactModel> GetSampleContacts() =>
    [
        new ContactModel
        {
            Id = 1,
            Guid = new Guid("4b25d304-d5de-475f-8445-a960402bae65"),
            FirstName = "Sample",
            LastName = "Contact",
            Email = "sample.contact@domain.com",
            PhoneNumber = "0700123456",
            StreetAddress = "Sample Street 1",
            PostalCode = 12345,
            City = "Sample City"
        }
    ];


    public bool CreateContact(ContactDto contactForm)
    {
        try
        {
            var contactModel = ContactFactory.Create(contactForm);

            if (contactModel != null)
            {
                contactModel.Id = _contacts.Count() != 0 ? _contacts.Last().Id + 1 : 1;

                _contacts.Add(contactModel);
                if (_contactRepository.SaveToFile(_contacts))
                {
                    ContactsUpdated?.Invoke(this, EventArgs.Empty);
                    return true;
                }
            }

            Debug.WriteLine($"Failed to create contact.");
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
        try
        {
            var contactToDelete = _contacts.Find(x => x.Id == contactId);
            if (contactToDelete != null)
            {
                _contacts.Remove(contactToDelete);
                if (_contactRepository.SaveToFile(_contacts))
                {
                    ContactsUpdated?.Invoke(this, EventArgs.Empty);
                    return true;
                }
            }

            Debug.WriteLine($"Failed to delete contact.");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to delete contact. {ex.Message}");
            return false;
        }
    }


    public bool UpdateContact(ContactModel contact)
    {
        try
        {
            var contactIndexToUpdate = _contacts.FindIndex(x => x.Id == contact.Id);
            if (contactIndexToUpdate != -1)
            {
                _contacts[contactIndexToUpdate] = contact;
                if (_contactRepository.SaveToFile(_contacts))
                {
                    ContactsUpdated?.Invoke(this, EventArgs.Empty);
                    return true;
                }
            }

            Debug.WriteLine($"Failed to update contact.");
            return false;
        }
        catch (Exception ex )
        {
            Debug.WriteLine($"Failed to update contact. {ex.Message}");
            return false;
        }
    }
}
