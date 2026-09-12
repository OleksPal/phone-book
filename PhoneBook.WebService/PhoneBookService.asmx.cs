using System.Collections.Generic;
using System.Web.Services;
using PhoneBookWebService.Data;
using PhoneBookWebService.Models;

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
        public List<PhoneContact> GetContacts(PhoneContactFilter filter)
        {
            return _repository.GetContacts(filter);
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
