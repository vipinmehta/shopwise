using ECommerce.Application.Common;
using ECommerce.Application.Dtos;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using MongoDB.Driver;

namespace ECommerce.Infrastructure.Services;

public class AddressService : IAddressService
{
    private readonly MongoDbContext _db;

    public AddressService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<List<AddressDto>> GetAllAsync(string userId)
    {
        var addresses = await _db.Addresses.Find(a => a.UserId == userId).ToListAsync();
        return addresses.Select(ToDto).ToList();
    }

    public async Task<AddressDto> CreateAsync(string userId, UpsertAddressRequest request)
    {
        if (request.IsDefault)
        {
            await _db.Addresses.UpdateManyAsync(a => a.UserId == userId, Builders<Address>.Update.Set(a => a.IsDefault, false));
        }

        var address = new Address
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            Line1 = request.Line1,
            Line2 = request.Line2,
            City = request.City,
            State = request.State,
            Pincode = request.Pincode,
            Country = request.Country,
            IsDefault = request.IsDefault
        };
        await _db.Addresses.InsertOneAsync(address);
        return ToDto(address);
    }

    public async Task<AddressDto> UpdateAsync(string userId, string id, UpsertAddressRequest request)
    {
        var existing = await _db.Addresses.Find(a => a.Id == id && a.UserId == userId).FirstOrDefaultAsync()
            ?? throw new AppException("Address not found", 404);

        if (request.IsDefault)
        {
            await _db.Addresses.UpdateManyAsync(a => a.UserId == userId, Builders<Address>.Update.Set(a => a.IsDefault, false));
        }

        var update = Builders<Address>.Update
            .Set(a => a.FullName, request.FullName)
            .Set(a => a.Phone, request.Phone)
            .Set(a => a.Line1, request.Line1)
            .Set(a => a.Line2, request.Line2)
            .Set(a => a.City, request.City)
            .Set(a => a.State, request.State)
            .Set(a => a.Pincode, request.Pincode)
            .Set(a => a.Country, request.Country)
            .Set(a => a.IsDefault, request.IsDefault);

        var updated = await _db.Addresses.FindOneAndUpdateAsync(
            a => a.Id == id, update,
            new FindOneAndUpdateOptions<Address> { ReturnDocument = ReturnDocument.After });
        return ToDto(updated);
    }

    public async Task DeleteAsync(string userId, string id)
    {
        await _db.Addresses.DeleteOneAsync(a => a.Id == id && a.UserId == userId);
    }

    private static AddressDto ToDto(Address a) => new(a.Id, a.FullName, a.Phone, a.Line1, a.Line2, a.City, a.State, a.Pincode, a.Country, a.IsDefault);
}
