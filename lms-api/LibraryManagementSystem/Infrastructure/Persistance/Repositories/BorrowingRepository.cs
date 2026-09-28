using Application.Persistance;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class BorrowingRepository : Repository<Borrowing>, IBorrowingRepository
    {
        public BorrowingRepository(LibraryDbContext context) : base(context)
        {
        }
    }
}
