using Domain.Common;
using Domain.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Location : BaseEntity<int>, IAuditable
    {
        public string Area { get; private set; }
        public string LineNo { get; private set; }
        public string ShelfNo { get; private set; }
        public int RowNo { get; private set; }
        public int ColumnNo { get; private set; }

        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public static Result<Location> Create(string area, string lineNo, string shelfNo, int rowNo, int columnNo)
        {
            if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(lineNo) || string.IsNullOrWhiteSpace(shelfNo)) return Result<Location>.Failure(LocationErrors.PositionNotProvided);
            if (rowNo <= 0 || columnNo <= 0) return Result<Location>.Failure(LocationErrors.InvalidRowOrColumn);

            Location location = new Location()
            {
                Area = area,
                LineNo = lineNo,
                ShelfNo = shelfNo,
                RowNo = rowNo,
                ColumnNo = columnNo
            };

            return Result<Location>.Success(location);
        }

        public Result MoveTo(string area, string lineNo, string shelfNo, int rowNo, int columnNo)
        {
            if (string.IsNullOrWhiteSpace(area) || string.IsNullOrWhiteSpace(lineNo) || string.IsNullOrWhiteSpace(shelfNo)) return Result.Failure(LocationErrors.PositionNotProvided);
            if (rowNo <= 0 || columnNo <= 0) return Result.Failure(LocationErrors.InvalidRowOrColumn);

            Area = area;
            LineNo = lineNo;
            ShelfNo = shelfNo;
            RowNo = rowNo;
            ColumnNo = columnNo;

            return Result.Success();
        }
    }
}
