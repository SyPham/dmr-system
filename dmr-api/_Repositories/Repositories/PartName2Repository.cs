using System.Threading.Tasks;
using DMR_API._Repositories.Interface;
using DMR_API.Data;
using DMR_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DMR_API.DTO;
using System.Collections.Generic;

namespace DMR_API._Repositories.Repositories
{
    public class PartName2Repository : ECRepository<PartName2>, IPartName2Repository
    {
        private readonly DataContext _context;
        public PartName2Repository(DataContext context) : base(context)
        {
            _context = context;
        }

     
        //Login khi them repo
    }
}