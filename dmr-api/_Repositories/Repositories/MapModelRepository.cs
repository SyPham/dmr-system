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
    public class MapModelRepository : ECRepository<MapModel>, IMapModelRepository
    {
        private readonly DataContext _context;
        public MapModelRepository(DataContext context) : base(context)
        {
            _context = context;
        }

    }
}