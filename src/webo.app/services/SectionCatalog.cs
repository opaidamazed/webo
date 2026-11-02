using Webo.App.Models;

namespace Webo.App;

/// <summary>
/// One editable field of a section type. <see cref="LabelKey"/> is an app-UI string key (Loc);
/// <see cref="Number"/> &gt; 0 appends the item number to the label ("Question 2").
/// </summary>
public sealed record FieldDef(string Key, string LabelKey, bool Multiline = false, bool IsLink = false, int Number = 0);

/// <summary>
/// The section types the editor can add, which fields each one has, and the starter content.
/// Starter content follows the language of the SITE being built, not the language of the app UI.
/// Repeated items (cards, steps, questions…) have a fixed number of slots; an empty slot is simply not rendered.
/// </summary>
public static class SectionCatalog
{
    public static readonly string[] Types =
        { "hero", "text", "features", "steps", "stats", "testimonials", "faq", "cta", "contact", "footer" };

    public const int StepSlots = 4;
    public const int StatSlots = 4;
    public const int QuoteSlots = 3;
    public const int FaqSlots = 6;

    public static IReadOnlyList<FieldDef> Fields(string type)
    {
        var list = new List<FieldDef>();
        switch (type)
        {
            case "hero":
                list.Add(new("title", "fld.title"));
                list.Add(new("subtitle", "fld.subtitle", true));
                list.Add(new("btn", "fld.btn"));
                list.Add(new("link", "fld.link", false, true));
                break;
            case "text":
                list.Add(new("heading", "fld.heading"));
                list.Add(new("body", "fld.body", true));
                break;
            case "features":
                list.Add(new("heading", "fld.heading"));
                list.Add(new("f1t", "fld.f1t")); list.Add(new("f1d", "fld.desc", true));
                list.Add(new("f2t", "fld.f2t")); list.Add(new("f2d", "fld.desc", true));
                list.Add(new("f3t", "fld.f3t")); list.Add(new("f3d", "fld.desc", true));
                break;
            case "steps":
                list.Add(new("heading", "fld.heading"));
                for (var i = 1; i <= StepSlots; i++)
                {
                    list.Add(new("p" + i + "t", "fld.steptitle", false, false, i));
                    list.Add(new("p" + i + "d", "fld.desc", true, false, i));
                }
                break;
            case "stats":
                for (var i = 1; i <= StatSlots; i++)
                {
                    list.Add(new("s" + i + "v", "fld.value", false, false, i));
                    list.Add(new("s" + i + "l", "fld.label", false, false, i));
                }
                break;
            case "testimonials":
                list.Add(new("heading", "fld.heading"));
                for (var i = 1; i <= QuoteSlots; i++)
                {
                    list.Add(new("q" + i, "fld.quote", true, false, i));
                    list.Add(new("n" + i, "fld.person", false, false, i));
                }
                break;
            case "faq":
                list.Add(new("heading", "fld.heading"));
                for (var i = 1; i <= FaqSlots; i++)
                {
                    list.Add(new("a" + i + "q", "fld.question", false, false, i));
                    list.Add(new("a" + i + "a", "fld.answer", true, false, i));
                }
                break;
            case "cta":
                list.Add(new("heading", "fld.heading"));
                list.Add(new("text", "fld.text", true));
                list.Add(new("btn", "fld.btn"));
                list.Add(new("link", "fld.link", false, true));
                break;
            case "contact":
                list.Add(new("heading", "fld.heading"));
                list.Add(new("text", "fld.text", true));
                list.Add(new("whatsapp", "fld.whatsapp"));
                list.Add(new("phone", "fld.phone"));
                list.Add(new("email", "fld.email"));
                break;
            case "footer":
                list.Add(new("text", "fld.text", true));
                break;
        }
        return list;
    }

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
            case "steps":
                P("heading", "كيف نعمل؟", "How we work");
                P("p1t", "نفهم احتياجك", "We listen");
                P("p1d", "نبدأ بالاستماع لك وتحديد الهدف.", "We start by understanding your goal.");
                P("p2t", "نخطّط ونصمّم", "We plan and design");
                P("p2d", "نضع خطة واضحة وتصورًا مبدئيًا.", "We prepare a clear plan and a first concept.");
                P("p3t", "ننفّذ ونراجع", "We build and review");
                P("p3d", "ننفّذ العمل ونراجعه معك خطوة بخطوة.", "We build it and review it with you step by step.");
                P("p4t", "نسلّم ونتابع", "We deliver and follow up");
                P("p4d", "نسلّم النتيجة ونبقى بجانبك بعد التسليم.", "We deliver and stay with you afterwards.");
                break;
            case "stats":
                P("s1v", "+500", "500+"); P("s1l", "عميل سعيد", "Happy clients");
                P("s2v", "+10", "10+"); P("s2l", "سنوات خبرة", "Years of experience");
                P("s3v", "24/7", "24/7"); P("s3l", "دعم متواصل", "Support");
                P("s4v", "100%", "100%"); P("s4l", "التزام بالمواعيد", "On-time delivery");
                break;
            case "testimonials":
                P("heading", "آراء عملائنا", "What our clients say");
                P("q1", "تجربة ممتازة وخدمة محترفة.", "An excellent experience and professional service.");
                P("n1", "اسم العميل", "Client name");
                P("q2", "التزام بالمواعيد وجودة عالية.", "On time and high quality.");
                P("n2", "اسم العميل", "Client name");
                P("q3", "أنصح بالتعامل معهم.", "I recommend working with them.");
                P("n3", "اسم العميل", "Client name");
                break;
            case "faq":
                P("heading", "أسئلة شائعة", "Frequently asked questions");
                P("a1q", "كيف أبدأ؟", "How do I start?");
                P("a1a", "تواصل معنا وسنرشدك للخطوة الأولى.", "Get in touch and we will guide you through the first step.");
                P("a2q", "كم تستغرق المدة؟", "How long does it take?");
                P("a2a", "تختلف المدة حسب حجم العمل، ونحدّدها معك مسبقًا.", "It depends on the size of the work, and we agree it with you up front.");
                P("a3q", "هل يمكن التعديل لاحقًا؟", "Can things be changed later?");
                P("a3a", "نعم، يمكن التعديل في أي وقت.", "Yes, changes can be made at any time.");
                break;
            case "cta":
                P("heading", "جاهز للبدء؟", "Ready to start?");
                P("text", "تواصل معنا اليوم ونساعدك خطوة بخطوة.", "Get in touch today and we will help you step by step.");
                P("btn", "تواصل معنا", "Contact us");
                P("link", "#", "#");
                break;
            case "contact":
                P("heading", "تواصل معنا", "Get in touch");
                P("text", "يسعدنا سماعك. اختر الطريقة الأنسب لك.", "We would love to hear from you. Choose what suits you best.");
                P("whatsapp", "", "");
                P("phone", "", "");
                P("email", "", "");
                break;
            case "footer":
                P("text", $"© {year} جميع الحقوق محفوظة.", $"© {year} All rights reserved.");
                break;
        }
        return s;
    }

    /// <summary>True when every field still holds the starter text (the owner has not written anything of their own yet).</summary>
    public static bool IsStillDefault(SectionDoc section, string siteLanguage)
    {
        var starter = Create(section.Type, siteLanguage);
        foreach (var field in Fields(section.Type))
            if (section.Get(field.Key) != (starter.Props.TryGetValue(field.Key, out var v) ? v : ""))
                return false;
        return true;
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
