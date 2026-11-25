namespace ChampUI.Demo.Components;

public sealed record BilingualText(string English, string Arabic)
{
    public string For(bool arabic) => arabic ? Arabic : English;
}

public static class Copy
{
    public static BilingualText Navigation { get; } = new("Showcase navigation", "تنقّل المعرض");
    public static BilingualText Foundations { get; } = new("Foundations", "الأساسيات");
    public static BilingualText Forms { get; } = new("Forms", "النماذج");
    public static BilingualText Data { get; } = new("Data", "البيانات");
    public static BilingualText Charts { get; } = new("Charts", "الرسوم البيانية");
    public static BilingualText Theme { get; } = new("Switch theme", "تبديل المظهر");
    public static BilingualText Language { get; } = new("العربية", "English");
}
