using Prectice_Interview.Data;
using Prectice_Interview.Services;

namespace Prectice_Interview.UI
{
    public record ColumnDef<T>(string Header, Func<T, object?> Value, float Weight = 1, bool Right = false);
}
