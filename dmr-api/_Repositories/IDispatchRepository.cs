using dmr_api.Models;
using DMR_API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMR_API._Repositories
{
  public  interface IDispatchRepository : IECRepository<Dispatch>
    {
        void UpdateRange(List<Dispatch> dispatches);
    }
}
