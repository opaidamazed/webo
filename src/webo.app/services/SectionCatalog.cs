using Webo.App.Models;

namespace Webo.App;

/// <summary>One editable field of a section type. <see cref="LabelKey"/> is an app-UI string key (Loc).</summary>
public sealed record FieldDef(string Key, string LabelKey, bool Multiline = false, bool IsLink = false);

/// <summary>
/// The section types the editor can add, which fields each one has, and the starter content.
/// Starter content follows the language of the SITE being built, not the language of the app UI.
/// </summary>
public static class SectionCatalog
{
    public static readonly string[] Types = { "hero", "text", "features", "cta", "footer" };

    public static IReadOnlyList<FieldDef> Fields(string type) => type switch
    {
        "hero" => new FieldDef[]
        {
            new("title", "fld.title"),
            new("subtitle", "fld.subtitle", true),
            new("btn", "fld.btn"),
            new("link", "fld.link", false, true),
        },
        "text" => new FieldDef[]
        {
            new("heading", "fld.heading"),
            new("body", "fld.body", true),
        },
        "features" => new FieldDef[]
        {
            new("heading", "fld.heading"),
            new("f1t", "fld.f1t"), new("f1d", "fld.desc", true),
            new("f2t", "fld.f2t"), new("f2d", "fld.desc", true),
            new("f3t", "fld.f3t"), new("f3d", "fld.desc", true),
        },
        "cta" => new FieldDef[]
        {
            new("heading", "fld.heading"),
            new("text", "fld.text", true),
            new("btn", "fld.btn"),
            new("link", "fld.link", false, true),
        },
        "footer" => new FieldDef[]
        {
            new("text", "fld.text", true),
        },
        _ => Array.Empty<FieldDef>(),
    };

    public static SectionDoc Create(string type, string siteLanguage)
    {
        var ar = siteLanguage != "en";
        var s = new SectionDoc { Type = type };
        void P(string key, string arText, string enText) => s.Props[key] = ar ? arText : enText;
        var year = DateTime.Now.Year;

        switch (type)
        {
            case "hero":
                P("title", "مرحبًا بك في موقعنا", "Welcome to our website");
                P("subtitle", "اكتب هنا جملة قصيرة تشرح ما تقدّمه لزوّارك.", "Write a short sentence about what you offer your visitors.");
                P("btn", "ابدأ الآن", "Get started");
                P("link", "#", "#");
                break;
            case "text":
                P("heading", "من نحن", "About us");
                P("body", "اكتب هنا نصًا يعرّف بنشاطك. افصل بين الفقرات بسطر فارغ.", "Write a few lines about your work here. Separate paragraphs with a blank line.");
                break;
            case "features":
                P("heading", "ما الذي نقدّمه", "What we offer");
                P("f1t", "جودة عالية", "High quality");
                P("f1d", "وصف قصير للميزة الأولى.", "A short description of the first feature.");
                P("f2t", "سرعة في التنفيذ", "Fast delivery");
                P("f2d", "وصف قصير للميزة الثانية.", "A short description of the second feature.");
                P("f3t", "دعم مستمر", "Ongoing support");
                P("f3d", "وصف قصير للميزة الثالثة.", "A short description of the third feature.");
                break;
            case "cta":
                P("heading", "جاهز للبدء؟", "Ready to start?");
                P("text", "تواصل معنا اليوم ونساعدك خطوة بخطوة.", "Get in touch today and we will help you step by step.");
                P("btn", "تواصل معنا", "Contact us");
                P("link", "#", "#");
                break;
            case "footer":
                P("text", $"© {year} جميع الحقوق محفوظة.", $"© {year} All rights reserved.");
                break;
        }
        return s;
    }

    /// <summary>Sections a brand-new project's home page starts with, so the first preview is not empty.</summary>
    public static List<SectionDoc> Starter(string siteLanguage) => new()
    {
        Create("hero", siteLanguage),
        Create("features", siteLanguage),
        Create("cta", siteLanguage),
        Create("footer", siteLanguage),
    };
}
