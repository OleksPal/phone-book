using PhoneBookWebService.Data;
using PhoneBookWebService.Models;
using System.Web.Services;

namespace PhoneBookWebService
{
    [WebService(Namespace = "http://mycompany.com/phonebook")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class PhoneBookService : WebService
    {
        private readonly PhoneContactRepository _repository;

        public PhoneBookService()
        {
            _repository = new PhoneContactRepository();
        }

        [WebMethod]
        public PagedItems<PhoneContact> GetContacts(PhoneContactFilter filter, 
            int pageNumber, int pageSize, 
            string sortColumn, string sortOrder)
        {
            return _repository.GetContacts(filter, pageNumber, pageSize, sortColumn, sortOrder);
        }

        [WebMethod]
        public PhoneContact GetContact(int id)
        {
            return _repository.GetContactById(id);
        }

        [WebMethod]
        public int CreateContact(PhoneContact contact)
        {
            return _repository.AddContact(contact);
        }

        [WebMethod]
        public bool UpdateContact(PhoneContact contact)
        {
            return _repository.UpdateContact(contact);
        }

        [WebMethod]
        public bool DeleteContact(int id)
        {
            return _repository.DeleteContact(id);
        }
    }
}
