using dmr_api.Models;
using DMR_API._Services.Interface;
using DMR_API.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Services.Interface
{
   public interface IDispatchService : IECService<Dispatch>
    {
        bool AddDispatching(Dispatch dispatch);
        bool AddDispatchingRange(List<Dispatch> dispatch);
    }
}
