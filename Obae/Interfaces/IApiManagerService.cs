using System.Net;
using System.Threading.Tasks;
using Obae.Models;

namespace Obae.Interfaces;

public interface IApiManagerService
{
    Task<T> Get<T>(string url);
    void AddUserCookie(UserCookie userCookie);
}