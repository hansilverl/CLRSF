namespace CurrencyComparisonTool.Models
{
    public class PdfSelection
    {
        public string Field { get; set; } = string.Empty;
        public int Page { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
