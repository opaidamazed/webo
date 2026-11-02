using System.Globalization;

namespace Webo.App;

/// <summary>
/// App-UI strings in Arabic and English. The UI language is separate from the language of the
/// sites the user builds (requirements section 4). Strings are looked up at page load, so a
/// language change re-creates the current page.
/// </summary>
public static class Loc
{
    public static string Lang { get; set; } = "ar";
    public static bool IsArabic => Lang == "ar";
    public static CultureInfo Culture => new(IsArabic ? "ar-EG" : "en-US");

    public static string T(string key) =>
        Table.TryGetValue(key, out var v) ? (IsArabic ? v.ar : v.en) : key;

    public static string F(string key, params object[] args) => string.Format(T(key), args);

    private static readonly Dictionary<string, (string ar, string en)> Table = new()
    {
        // shell
        ["app.name"] = ("ويبو", "Webo"),
        ["nav.home"] = ("الرئيسية", "Home"),
        ["nav.templates"] = ("القوالب (قريبًا)", "Templates (soon)"),
        ["nav.settings"] = ("الإعدادات", "Settings"),

        // home
        ["home.welcome"] = ("أهلًا بك في ويبو", "Welcome to Webo"),
        ["home.sub"] = ("ابنِ موقعك متعدد الصفحات بصريًا، من غير ما تكتب سطر كود.", "Build multi-page websites visually, without writing code."),
        ["home.new"] = ("مشروع جديد", "New project"),
        ["home.open"] = ("فتح مشروع", "Open project"),
        ["home.recent"] = ("المشاريع الأخيرة", "Recent projects"),
        ["home.empty.title"] = ("لا توجد مشاريع بعد", "No projects yet"),
        ["home.empty.body"] = ("ابدأ بمشروع جديد، وسيظهر هنا مع تاريخ آخر تعديل.", "Start a new project and it will appear here with its last-modified date."),
        ["home.missing"] = ("الملف غير موجود: {0}", "File not found: {0}"),
        ["home.modified"] = ("آخر تعديل: {0}", "Last modified: {0}"),
        ["home.remove"] = ("إزالة من القائمة", "Remove from list"),
        ["home.remove.note"] = ("الإزالة من القائمة لا تحذف ملفات المشروع.", "Removing from the list never deletes project files."),
        ["home.honest"] = ("هذا إصدار مبكر: إنشاء المشاريع وإدارة الصفحات والأقسام والمعاينة الحقيقية والتراجع تعمل، أما القوالب والتصدير والاختبارات فلم تُنفَّذ بعد.", "Early build: projects, pages, sections, the real preview and undo/redo work; templates, export and testing are not implemented yet."),

        // new project
        ["np.title"] = ("مشروع جديد", "New project"),
        ["np.name"] = ("اسم المشروع", "Project name"),
        ["np.type"] = ("نوع الموقع", "Site type"),
        ["np.lang"] = ("لغة الموقع", "Site language"),
        ["np.location"] = ("مكان الحفظ", "Save location"),
        ["np.browse"] = ("تصفّح", "Browse"),
        ["np.create"] = ("إنشاء", "Create"),
        ["np.cancel"] = ("إلغاء", "Cancel"),
        ["np.err.name"] = ("اكتب اسمًا للمشروع.", "Enter a project name."),
        ["np.err.chars"] = ("الاسم يحتوي على رموز غير مسموحة في أسماء المجلدات.", "The name contains characters not allowed in folder names."),
        ["np.err.exists"] = ("يوجد مجلد بهذا الاسم بالفعل. اختر اسمًا آخر، ولن نستبدل أي ملفات.", "A folder with this name already exists. Pick another name; no files will be overwritten."),
        ["np.err.loc"] = ("اختر مكانًا صالحًا للحفظ.", "Choose a valid save location."),
        ["np.err.fail"] = ("تعذّر إنشاء المشروع: {0}", "Could not create the project: {0}"),
        ["np.site.rtl"] = ("عربي (من اليمين لليسار)", "Arabic (right-to-left)"),
        ["np.site.ltr"] = ("إنجليزي (من اليسار لليمين)", "English (left-to-right)"),

        // site types
        ["type.business"] = ("شركة أو نشاط تجاري", "Business"),
        ["type.portfolio"] = ("معرض أعمال شخصي", "Portfolio"),
        ["type.restaurant"] = ("مطعم أو كافيه", "Restaurant / café"),
        ["type.education"] = ("موقع تعليمي تعريفي", "Educational"),
        ["type.landing"] = ("صفحة هبوط", "Landing page"),
        ["type.blog"] = ("مدونة أو مجلة", "Blog / magazine"),

        // editor
        ["ed.back"] = ("الرئيسية", "Home"),
        ["ed.save"] = ("حفظ", "Save"),
        ["ed.saved"] = ("تم الحفظ", "Saved"),
        ["ed.unsaved"] = ("تعديلات غير محفوظة", "Unsaved changes"),
        ["ed.undo"] = ("تراجع", "Undo"),
        ["ed.redo"] = ("إعادة", "Redo"),
        ["ed.desktop"] = ("كمبيوتر", "Desktop"),
        ["ed.tablet"] = ("تابلت", "Tablet"),
        ["ed.mobile"] = ("موبايل", "Mobile"),
        ["ed.test"] = ("اختبار (قريبًا)", "Test (soon)"),
        ["ed.export"] = ("تصدير الموقع", "Export site"),
        ["ed.pages"] = ("الصفحات", "Pages"),
        ["ed.addpage"] = ("إضافة صفحة", "Add page"),
        ["ed.newpage"] = ("صفحة جديدة", "New page"),
        ["ed.home"] = ("الرئيسية", "Home"),
        ["ed.props"] = ("خصائص الصفحة", "Page properties"),
        ["ed.pagetitle"] = ("عنوان الصفحة", "Page title"),
        ["ed.slug"] = ("رابط الصفحة", "Page link"),
        ["ed.sethome"] = ("تعيين كصفحة رئيسية", "Set as home page"),
        ["ed.delete"] = ("حذف الصفحة", "Delete page"),
        ["ed.delete.confirm"] = ("حذف الصفحة «{0}»؟ يمكنك التراجع عنه بزر «تراجع».", "Delete page “{0}”? You can undo this with the Undo button."),
        ["ed.delete.last"] = ("لا يمكن حذف الصفحة الوحيدة في المشروع.", "You cannot delete the only page in a project."),
        ["ed.canvas.note"] = ("المحرر البصري قيد التطوير. هذه المساحة ستعرض معاينة الصفحة الحقيقية وتحديد العناصر فيها.", "The visual editor is under development. This area will show the real page preview with element selection."),
        ["ed.canvas.rtl"] = ("اتجاه الموقع: من اليمين لليسار", "Site direction: right-to-left"),
        ["ed.canvas.ltr"] = ("اتجاه الموقع: من اليسار لليمين", "Site direction: left-to-right"),
        ["ed.error.load"] = ("تعذّر فتح المشروع: {0}", "Could not open the project: {0}"),
        ["ed.error.save"] = ("تعذّر الحفظ: {0}", "Could not save: {0}"),
        ["ed.folder"] = ("مكان المشروع: {0}", "Project folder: {0}"),

        // editor: sections, preview, fields
        ["ed.sections"] = ("أقسام الصفحة", "Page sections"),
        ["ed.addsection"] = ("إضافة قسم", "Add section"),
        ["ed.sec.up"] = ("نقل لأعلى", "Move up"),
        ["ed.sec.down"] = ("نقل لأسفل", "Move down"),
        ["ed.sec.dup"] = ("تكرار القسم", "Duplicate section"),
        ["ed.sec.del"] = ("حذف القسم", "Delete section"),
        ["ed.sec.del.confirm"] = ("حذف هذا القسم؟ يمكنك التراجع عنه بزر «تراجع».", "Delete this section? You can undo this with the Undo button."),
        ["ed.sec.props"] = ("خصائص القسم", "Section properties"),
        ["ed.preview.empty"] = ("هذه الصفحة فارغة. أضِف أول قسم من لوحة «أقسام الصفحة».", "This page is empty. Add the first section from the “Page sections” panel."),
        ["ed.preview.fail"] = ("تعذّر تشغيل المعاينة: مكوّن Microsoft Edge WebView2 غير متاح على هذا الجهاز. ثبّته من موقع Microsoft ثم أعد فتح المشروع. بياناتك وحفظ المشروع لا يتأثران.", "The preview could not start: the Microsoft Edge WebView2 component is not available on this device. Install it from Microsoft, then reopen the project. Your data and saving are not affected."),
        ["sec.hero"] = ("القسم الرئيسي (Hero)", "Hero"),
        ["sec.text"] = ("نص", "Text"),
        ["sec.features"] = ("المزايا", "Features"),
        ["sec.cta"] = ("دعوة لاتخاذ إجراء", "Call to action"),
        ["sec.footer"] = ("التذييل", "Footer"),
        ["fld.title"] = ("العنوان", "Title"),
        ["fld.subtitle"] = ("النص التعريفي", "Subtitle"),
        ["fld.heading"] = ("العنوان", "Heading"),
        ["fld.body"] = ("النص", "Body"),
        ["fld.text"] = ("النص", "Text"),
        ["fld.btn"] = ("نص الزر", "Button text"),
        ["fld.link"] = ("رابط الزر (مثل https://… أو about.html)", "Button link (e.g. https://… or about.html)"),
        ["fld.desc"] = ("الوصف", "Description"),
        ["fld.f1t"] = ("الميزة 1 — العنوان", "Feature 1 — title"),
        ["fld.f2t"] = ("الميزة 2 — العنوان", "Feature 2 — title"),
        ["fld.f3t"] = ("الميزة 3 — العنوان", "Feature 3 — title"),

        // site identity
        ["ed.identity"] = ("هوية الموقع", "Site identity"),
        ["ed.palette"] = ("لوحة الألوان", "Colour palette"),
        ["ed.accent"] = ("اللون المخصص (HEX مثل #2090F0)", "Custom colour (HEX like #2090F0)"),
        ["ed.font"] = ("خط الموقع", "Site font"),
        ["ed.radius"] = ("شكل الحواف", "Corner style"),
        ["pal.ocean"] = ("أزرق المحيط", "Ocean blue"),
        ["pal.emerald"] = ("أخضر زمردي", "Emerald green"),
        ["pal.sunset"] = ("برتقالي الغروب", "Sunset orange"),
        ["pal.royal"] = ("بنفسجي ملكي", "Royal purple"),
        ["pal.rose"] = ("وردي", "Rose"),
        ["pal.slate"] = ("رمادي داكن", "Slate"),
        ["pal.custom"] = ("لون مخصص", "Custom colour"),
        ["font.plex"] = ("IBM Plex Sans Arabic (مضمّن)", "IBM Plex Sans Arabic (bundled)"),
        ["font.system"] = ("خط النظام (Segoe UI)", "System font (Segoe UI)"),
        ["font.serif"] = ("خط تقليدي (Serif)", "Traditional (Serif)"),
        ["radius.sharp"] = ("حواف حادة", "Sharp corners"),
        ["radius.medium"] = ("حواف متوسطة", "Medium corners"),
        ["radius.round"] = ("حواف دائرية", "Round corners"),

        // new sections and their fields
        ["sec.steps"] = ("خطوات العمل", "Steps"),
        ["sec.stats"] = ("أرقام وإحصائيات", "Numbers"),
        ["sec.testimonials"] = ("آراء العملاء", "Testimonials"),
        ["sec.faq"] = ("أسئلة شائعة", "FAQ"),
        ["sec.contact"] = ("التواصل", "Contact"),
        ["fld.steptitle"] = ("عنوان الخطوة", "Step title"),
        ["fld.value"] = ("الرقم أو القيمة", "Value"),
        ["fld.label"] = ("الوصف", "Label"),
        ["fld.quote"] = ("نص الرأي", "Quote"),
        ["fld.person"] = ("اسم صاحب الرأي", "Person"),
        ["fld.question"] = ("السؤال", "Question"),
        ["fld.answer"] = ("الإجابة", "Answer"),
        ["fld.whatsapp"] = ("رقم واتساب بالصيغة الدولية بلا + (مثل 201001234567)", "WhatsApp number, international, no + (e.g. 201001234567)"),
        ["fld.phone"] = ("رقم الهاتف", "Phone number"),
        ["fld.email"] = ("البريد الإلكتروني", "Email"),

        // export and pre-publish checks
        ["ed.export.done"] = ("تم تصدير {0} صفحة إلى:\n{1}\n\nالموقع ملفات HTML وCSS جاهزة، وتعمل بدون JavaScript.", "Exported {0} page(s) to:\n{1}\n\nThe site is plain HTML and CSS files and works without JavaScript."),
        ["ed.export.fail"] = ("تعذّر التصدير: {0}", "Export failed: {0}"),
        ["ed.export.warn"] = ("ملاحظات قبل النشر:", "Before you publish:"),
        ["ed.export.more"] = ("… و{0} ملاحظات أخرى", "… and {0} more"),
        ["ed.export.open"] = ("فتح المجلد الآن؟", "Open the folder now?"),
        ["chk.nohome"] = ("لا توجد صفحة رئيسية.", "There is no home page."),
        ["chk.dupfile"] = ("صفحتان لهما نفس اسم الملف: {0}", "Two pages have the same file name: {0}"),
        ["chk.empty"] = ("الصفحة «{0}» فارغة (بلا أقسام).", "The page “{0}” is empty (no sections)."),
        ["chk.default"] = ("في الصفحة «{0}» قسم «{1}» ما زال بالنص الافتراضي.", "On the page “{0}”, the “{1}” section still has the starter text."),
        ["chk.link"] = ("في الصفحة «{0}» زر في قسم «{1}» رابطه فارغ أو «#».", "On the page “{0}”, a button in the “{1}” section has an empty or “#” link."),
        ["chk.contact"] = ("في الصفحة «{0}» قسم التواصل بلا أي وسيلة (واتساب أو هاتف أو بريد).", "On the page “{0}”, the contact section has no channel (WhatsApp, phone or email)."),
        ["common.ok"] = ("حسنًا", "OK"),
        ["common.yes"] = ("نعم", "Yes"),
        ["common.no"] = ("لا", "No"),

        // settings
        ["st.title"] = ("الإعدادات", "Settings"),
        ["st.appearance"] = ("المظهر والتخصيص", "Appearance & personalization"),
        ["st.lang"] = ("لغة واجهة التطبيق", "App language"),
        ["st.lang.note"] = ("لا تغيّر لغة أو اتجاه المواقع التي تنشئها.", "Does not change the language or direction of the sites you build."),
        ["st.lang.ar"] = ("العربية", "Arabic"),
        ["st.lang.en"] = ("English", "English"),
        ["st.theme"] = ("ثيم الواجهة", "Interface theme"),
        ["st.theme.system"] = ("مثل النظام", "Same as system"),
        ["st.theme.light"] = ("فاتح", "Light"),
        ["st.theme.dark"] = ("داكن", "Dark"),
        ["st.icon"] = ("أيقونة التطبيق — App Icon", "App Icon"),
        ["st.icon.inapp"] = ("المعروضة داخل التطبيق الآن: {0}", "Shown inside the app right now: {0}"),
        ["st.icon.note"] = ("خارج التطبيق (سطح المكتب، قائمة Start، شريط المهام، الملف التنفيذي) تظهر دائمًا الأيقونة الأصلية الداكنة. داخل التطبيق فقط تتبدّل تلقائيًا بين الفاتحة والداكنة مع ثيم الواجهة، ولا يوجد ما تضبطه هنا.", "Outside the app (desktop, Start menu, taskbar, the executable) the original dark icon is always shown. Only inside the app it switches automatically between light and dark with the interface theme; there is nothing to configure."),
        ["st.icon.dark"] = ("النسخة الداكنة", "Dark"),
        ["st.icon.light"] = ("النسخة الفاتحة", "Light"),
        ["st.sc.title"] = ("اختصارات Webo", "Webo shortcuts"),
        ["st.sc.note"] = ("لإعادة إنشاء اختصار محذوف بالأيقونة الأصلية. لا نعدّل أي اختصار إلا الذي ينشئه Webo نفسه، وبعد موافقتك؛ الاختصارات الأخرى (مثل التي أنشأها المثبّت) تبقى كما هي.", "Re-creates a deleted shortcut with the original icon. Webo only updates shortcuts it created itself, and only with your consent; other shortcuts (such as the installer's) stay untouched."),
        ["st.sc.desktop"] = ("إنشاء/تحديث اختصار سطح المكتب", "Create/update desktop shortcut"),
        ["st.sc.start"] = ("إنشاء/تحديث اختصار قائمة Start", "Create/update Start menu shortcut"),
        ["st.sc.confirm"] = ("سيُنشأ أو يُحدَّث هذا الاختصار بالأيقونة الأصلية:\n{0}", "This shortcut will be created or updated with the original icon:\n{0}"),
        ["st.sc.ok"] = ("تم إنشاء/تحديث الاختصار. قد يحتاج Explorer لحظات أو إعادة تحديث لإظهار الأيقونة الجديدة.", "The shortcut was created/updated. Explorer may need a moment or a refresh to show the new icon."),
        ["st.sc.refused"] = ("هذا الاختصار موجود ولم ينشئه Webo، لذلك تُرك دون تغيير.", "This shortcut already exists and was not created by Webo, so it was left untouched."),
        ["st.sc.fail"] = ("تعذّرت العملية: {0}", "The operation failed: {0}"),
        ["st.about"] = ("عن التطبيق", "About"),
        ["st.about.body"] = ("Webo {0} — إصدار مبكر. الخط: IBM Plex Sans Arabic (SIL OFL 1.1).", "Webo {0} — early build. Font: IBM Plex Sans Arabic (SIL OFL 1.1)."),
        ["st.data"] = ("بياناتك: الإعدادات في %LOCALAPPDATA%\\Webo، والمشاريع في المكان الذي تختاره. إزالة البرنامج لا تحذف مشاريعك.", "Your data: settings live in %LOCALAPPDATA%\\Webo and projects live where you choose. Uninstalling never deletes your projects."),
    };
}
