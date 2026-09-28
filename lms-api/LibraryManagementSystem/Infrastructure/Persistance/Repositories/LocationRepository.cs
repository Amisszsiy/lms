using Application.Persistance;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        public LocationRepository(LibraryDbContext context) : base(context)
        {
        }

        public async Task<bool> Exists(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Locations.AnyAsync(l => l.Id == id, cancellationToken);
        }

        public async Task<bool> IsUsing(int id, CancellationToken cancellationToken = default)
        {
            return await _context.BookCopies.AnyAsync(bc => bc.LocationId == id, cancellationToken);
        }

        public async Task<bool> PositionExists(string area, string lineNo, string shelfNo, int rowNo, int columnNo, int excludeLocationId = 0, CancellationToken cancellationToken = default)
        {
            return await _context.Locations.AnyAsync(l =>
                l.Area == area && l.LineNo == lineNo && l.ShelfNo == shelfNo && l.RowNo == rowNo && l.ColumnNo == columnNo &&
                l.Id != excludeLocationId, cancellationToken);
        }
    }
}
