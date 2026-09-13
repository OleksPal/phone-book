namespace PhoneBook.Frontend.Services;

public class PhoneBookService
{
    private readonly PhoneBookServiceSoapClient _client;

    public PhoneBookService()
    {
        _client = new PhoneBookServiceSoapClient(
            PhoneBookServiceSoapClient.EndpointConfiguration.PhoneBookServiceSoap);
    }

    public async Task<PagedItemsOfPhoneContact> GetContactsAsync(PhoneContactFilter filter, 
        int pageNumber, int pageSize, 
        string sortColumn, string sortOrder)
    {
        var response = await _client.GetContactsAsync(filter, pageNumber, pageSize, sortColumn, sortOrder);

        return response.Body.GetContactsResult;
    }

    public async Task<PhoneContact?> GetContactAsync(int id)
    {
        var response = await _client.GetContactAsync(id);

        return response.Body.GetContactResult;
    }

    public async Task<int> CreateContactAsync(PhoneContact contact)
    {
        var response = await _client.CreateContactAsync(contact);

        return response.Body.CreateContactResult;
    }

    public async Task<bool> UpdateContactAsync(PhoneContact contact)
    {
        var response = await _client.UpdateContactAsync(contact);

        return response.Body.UpdateContactResult;
    }

    public async Task<bool> DeleteContactAsync(int id)
    {
        return await _client.DeleteContactAsync(id);
    }
}
