namespace PhoneBook.Frontend.Services;

public class PhoneBookApi
{
    private readonly PhoneBookServiceSoapClient _client;

    public PhoneBookApi()
    {
        _client = new PhoneBookServiceSoapClient(
            PhoneBookServiceSoapClient.EndpointConfiguration.PhoneBookServiceSoap);
    }

    public async Task<PhoneContact[]> GetContactsAsync(PhoneContactFilter filter)
    {
        var response = await _client.GetContactsAsync(filter);

        return response.Body.GetContactsResult ?? Array.Empty<PhoneContact>();
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
