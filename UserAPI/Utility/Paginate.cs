using System.Linq;
using UserAPI.Models;

namespace UserAPI.Utility
{
    public class Paginate
    {
        //[pqa] Simple pagination. It will accept the page number and the number of rows to display.
        public static IQueryable<UserModel> PagedQuery(int pageNumber, int paginationRows, IQueryable<UserModel> query)
        {
            //int remainder = 0;
            //[pqa] Get the count of the rows
            int rowCount = query.Count();

            //[pqa] Divide count of # of rows to get the total page
            //int pageCount = Math.DivRem(rowCount, paginationRows, out remainder);

            //[pqa] Determine the starting row
            int rowStart = ((pageNumber - 1) * paginationRows);

            //[pqa] Generate paged query
            IQueryable<UserModel> result = query.Skip(rowStart).Take(paginationRows);
            
            return result;
        }
    }
}
