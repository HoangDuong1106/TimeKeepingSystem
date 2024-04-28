using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.DTO;
using BusinessObject.Model;
using DataAccess.InterfaceRepository;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class FakeDataRepository : Repository<Wifi>
    {
        private readonly MyDbContext _dbContext;

        public FakeDataRepository(MyDbContext context) : base(context)
        {
            _dbContext = context;
        }



        
    }
}
