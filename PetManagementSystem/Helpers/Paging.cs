using PetManagementSystem.Models;

namespace PetManagementSystem.Helpers
{
    public static class Paging
    {
        // Bổ sung hằng số DefaultPageSize cho các Service Commerce
        public const int DefaultPageSize = 10;

        public static PagedResult<T> Create<T>(List<T> items, int totalCount, int pageIndex, int pageSize)
        {
            return new PagedResult<T>(items, totalCount, pageIndex, pageSize);
        }
    }
}