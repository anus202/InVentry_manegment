namespace Prectice_Interview.UI
{
    public class FieldDef
    {
        public string Label { get; init; } = "";
        public FieldKind Kind { get; init; } = FieldKind.Text;
        public bool Required { get; init; }
        public Func<object?> Get { get; init; } = () => null;
        public Action<object?> Set { get; init; } = _ => { };
        public List<KeyValuePair<int?, string>>? Options { get; init; }
        public string? Hint { get; init; }
        public decimal Max { get; init; } = 99_999_999;
    }
}
