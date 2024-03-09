namespace OpenAI
{
    public class OpenAiResponse
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Stili di denominazione", Justification = "Needed for JSON match")]
        public OpenAi openai { get; set; } = new();
    }

    public class OpenAi
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Stili di denominazione", Justification = "Needed for JSON match")]
        public string status { get; set; } = string.Empty;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Stili di denominazione", Justification = "Needed for JSON match")]
        public List<Item> items { get; set; } = [];
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Stili di denominazione", Justification = "Needed for JSON match")]
        public double cost { get; set; }
    }

    public class Item
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Stili di denominazione", Justification = "Needed for JSON match")]
        public string image { get; set; } = string.Empty;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Stili di denominazione", Justification = "Needed for JSON match")]
        public string image_resource_url { get; set; } = string.Empty;
    }
}
