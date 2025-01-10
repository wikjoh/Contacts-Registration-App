﻿using Business.Dtos;
using Business.Interfaces;
using Business.Models;
using Business.Services;
using Moq;
using System.Text.Json;

namespace Business.Tests.Services;

public class ContactService_Tests
{
    private readonly Mock<IContactRepository> _contactRepositoryMock;
    private readonly IContactService _contactService;

    public ContactService_Tests()
    {
        _contactRepositoryMock = new Mock<IContactRepository>();
        _contactService = new ContactService(_contactRepositoryMock.Object);
    }

    [Fact]
    public void GetSampleContacts_ShouldReturnListWithSampleContact()
    {
        // arrange
        List<ContactModel> sampleContactList = [
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

        // act
        var result = _contactService.GetSampleContacts();

        // assert
        // Can't use Assert.Equal on lists directly since they are two separate lists with different references. Serialize to json and compare strings instead.
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(sampleContactList));
    }


    //GetContacts Tests
    [Fact]
    public void GetContacts_ShouldReturnListWithSampleContact_IfContactsFileDoesNotExist()
    {
        // arrange
        _contactRepositoryMock
            .Setup(cr => cr.ReadFromFile())
            .Returns((List<ContactModel>?)null);

        // act
        var result = _contactService.GetContacts();

        // assert
        // Can't use Assert.Equal on lists directly since they are two separate lists with different references. Serialize to json and compare strings instead.
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(_contactService.GetSampleContacts()));
    }


    [Fact]
    public void GetContacts_ShouldReturnListOfContacts_IfContactsFileContainsContacts()
    {
        // arrange
        List<ContactModel> mockedContactsListFromFile = [
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
            },
            new ContactModel
            {
                Id = 2,
                Guid = new Guid("4b25d304-d5de-475f-8445-a960402bae66"),
                FirstName = "Sample2",
                LastName = "Contact2",
                Email = "sample.contact@domain.com",
                PhoneNumber = "0700123456",
                StreetAddress = "Sample Street 1",
                PostalCode = 12345,
                City = "Sample City"
            }
        ];

        _contactRepositoryMock
            .Setup(cr => cr.ReadFromFile())
            .Returns(mockedContactsListFromFile);

        // act
        var result = _contactService.GetContacts();

        // assert
        // Can't use Assert.Equal on lists directly since they are two separate lists with different references. Serialize to json and compare strings instead.
        Assert.Equal(JsonSerializer.Serialize(result), JsonSerializer.Serialize(mockedContactsListFromFile));
    }


    [Fact]
    public void GetContacts_ShouldReturnEmptyList_IfContactsFileIsEmpty()
    {
        // arrange
        List<ContactModel> mockedContactsListFromFile = [];

        _contactRepositoryMock
            .Setup(cr => cr.ReadFromFile())
            .Returns(mockedContactsListFromFile);

        // act
        var result = _contactService.GetContacts();

        // assert
        Assert.Equal(result, []);
    }


    // CreateContact Tests
    [Fact]
    public void CreateContact_ShouldReturnTrue_WhenContactIsCreatedSuccessfully()
    {
        // arrange
        ContactModel sampleContact = _contactService.GetContacts().First();
        ContactDto sampleDto = new()
        {
            FirstName = sampleContact.FirstName,
            LastName = sampleContact.LastName,
            Email = sampleContact.Email,
            PhoneNumber = sampleContact.PhoneNumber,
            StreetAddress = sampleContact.StreetAddress,
            PostalCode = sampleContact.PostalCode,
            City = sampleContact.City
        };

        _contactRepositoryMock.Setup(cr => cr.SaveToFile(It.IsAny<List<ContactModel>>())).Returns(true);

        // act
        var result = _contactService.CreateContact(sampleDto);

        // assert
        Assert.True(result);
    }


    [Fact]
    public void CreateContact_ShouldReturnFalse_WhenSuppliedParameterIsNull()
    {
        // arrange

        // act
        var result = _contactService.CreateContact(null!);

        // assert
        Assert.False(result);
    }


    [Fact]
    public void CreateContact_ShouldHandleException_AndReturnFalse()
    {
        // arrange
        _contactRepositoryMock
            .Setup(cr => cr.SaveToFile(It.IsAny<List<ContactModel>>()))
            .Throws(new Exception("Mocktest"));

        // act
        var result = _contactService.CreateContact(new ContactDto());

        // assert
        Assert.False(result);
    }


    [Fact]
    public void CreateContact_ShouldInvokeContactsUpdated()
    {
        // arrange
        _contactRepositoryMock
            .Setup(cr => cr.SaveToFile(It.IsAny<List<ContactModel>>()))
            .Returns(true);

        bool contactsUpdatedInvoked = false;
        _contactService.ContactsUpdated += (sender, args) => contactsUpdatedInvoked = true;

        // act
        _contactService.CreateContact(new ContactDto());

        // assert
        Assert.True(contactsUpdatedInvoked);
    }


}
