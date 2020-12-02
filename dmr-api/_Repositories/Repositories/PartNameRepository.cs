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
    public class PartNameRepository : ECRepository<PartName>, IPartNameRepository
    {
        private readonly DataContext _context;
        public PartNameRepository(DataContext context) : base(context)
        {
            _context = context;
        }

     
        //Login khi them repo
    }
}