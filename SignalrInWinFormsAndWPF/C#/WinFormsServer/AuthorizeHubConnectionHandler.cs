using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SignalRChat
{
    public class AuthorizeHubConnectionHandler : AuthorizeAttribute
    {
     
        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            
            return true;
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            return true;
        }
    }
}
