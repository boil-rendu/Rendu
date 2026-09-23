using System.Net;
using RD.Core;

namespace RD.Server
{
    public interface IHttpHandler
    {
        Promise Handle(Scene scene, HttpListenerContext context);
    }
}