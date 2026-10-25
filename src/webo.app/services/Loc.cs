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
        ["home.honest"] = ("هذا إصدار مبكر: إنشاء المشاريع وإدارة الصفحات يعملان، أما المحرر البصري الكامل والمعاينة والتصدير فلم تُنفَّذ بعد.", "Early build: creating projects and managing pages works; the full visual editor, preview and export are not implemented yet."),

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
        ["ed.undo"] = ("تراجع (قريبًا)", "Undo (soon)"),
        ["ed.redo"] = ("إعادة (قريبًا)", "Redo (soon)"),
        ["ed.desktop"] = ("كمبيوتر", "Desktop"),
        ["ed.tablet"] = ("تابلت", "Tablet"),
        ["ed.mobile"] = ("موبايل", "Mobile"),
        ["ed.test"] = ("اختبار (قريبًا)", "Test (soon)"),
        ["ed.export"] = ("تصدير (قريبًا)", "Export (soon)"),
        ["ed.pages"] = ("الصفحات", "Pages"),
        ["ed.addpage"] = ("إضافة صفحة", "Add page"),
        ["ed.newpage"] = ("صفحة جديدة", "New page"),
        ["ed.home"] = ("الرئيسية", "Home"),
        ["ed.props"] = ("خصائص الصفحة", "Page properties"),
        ["ed.pagetitle"] = ("عنوان الصفحة", "Page title"),
        ["ed.slug"] = ("رابط الصفحة", "Page link"),
        ["ed.sethome"] = ("تعيين كصفحة رئيسية", "Set as home page"),
        ["ed.delete"] = ("حذف الصفحة", "Delete page"),
        ["ed.delete.confirm"] = ("حذف الصفحة «{0}»؟ لا يمكن التراجع عن هذا الآن.", "Delete page “{0}”? This cannot be undone yet."),
        ["ed.delete.last"] = ("لا يمكن حذف الصفحة الوحيدة في المشروع.", "You cannot delete the only page in a project."),
        ["ed.canvas.note"] = ("المحرر البصري قيد التطوير. هذه المساحة ستعرض معاينة الصفحة الحقيقية وتحديد العناصر فيها.", "The visual editor is under development. This area will show the real page preview with element selection."),
        ["ed.canvas.rtl"] = ("اتجاه الموقع: من اليمين لليسار", "Site direction: right-to-left"),
        ["ed.canvas.ltr"] = ("اتجاه الموقع: من اليسار لليمين", "Site direction: left-to-right"),
        ["ed.error.load"] = ("تعذّر فتح المشروع: {0}", "Could not open the project: {0}"),
        ["ed.error.save"] = ("تعذّر الحفظ: {0}", "Could not save: {0}"),
        ["ed.folder"] = ("مكان المشروع: {0}", "Project folder: {0}"),
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
        ["st.icon.note"] = ("اختيار الأيقونة منفصل عن ثيم الواجهة، ولا تتغير الأيقونة تلقائيًا مع الثيم.", "The icon choice is independent of the interface theme and never changes automatically with it."),
        ["st.icon.mono"] = ("أحادية اللون", "Monochrome"),
        ["st.icon.dark"] = ("النسخة الداكنة", "Dark"),
        ["st.icon.light"] = ("النسخة الفاتحة", "Light"),
        ["st.icon.current"] = ("الحالية", "Current"),
        ["st.icon.default"] = ("الافتراضية", "Default"),
        ["st.icon.choose"] = ("اختيار", "Select"),
        ["st.icon.reset"] = ("استعادة الافتراضي (الداكنة)", "Restore default (Dark)"),
        ["st.icon.applied"] = ("تم حفظ اختيارك وتطبيقه على نافذة التطبيق. أيقونة الملف التنفيذي والمثبّت ثابتة (الداكنة) ولا تتغير من هنا.", "Your choice was saved and applied to the app window. The executable and installer icons are fixed (Dark) and do not change from here."),
        ["st.icon.fail"] = ("تم حفظ اختيارك لكن تعذّر تطبيقه على النافذة: {0}", "Your choice was saved but could not be applied to the window: {0}"),
        ["st.sc.title"] = ("اختصارات Webo", "Webo shortcuts"),
        ["st.sc.note"] = ("لا نعدّل أي اختصار إلا الذي ينشئه Webo نفسه، وبعد موافقتك. الاختصارات الأخرى (مثل التي أنشأها المثبّت) تبقى كما هي.", "Webo only updates shortcuts it created itself, and only with your consent. Other shortcuts (such as the ones the installer made) stay untouched."),
        ["st.sc.desktop"] = ("إنشاء/تحديث اختصار سطح المكتب", "Create/update desktop shortcut"),
        ["st.sc.start"] = ("إنشاء/تحديث اختصار قائمة Start", "Create/update Start menu shortcut"),
        ["st.sc.confirm"] = ("سيُنشأ أو يُحدَّث هذا الاختصار بالأيقونة المختارة:\n{0}", "This shortcut will be created or updated with the selected icon:\n{0}"),
        ["st.sc.ok"] = ("تم إنشاء/تحديث الاختصار. قد يحتاج Explorer لحظات أو إعادة تحديث لإظهار الأيقونة الجديدة.", "The shortcut was created/updated. Explorer may need a moment or a refresh to show the new icon."),
        ["st.sc.refused"] = ("هذا الاختصار موجود ولم ينشئه Webo، لذلك تُرك دون تغيير.", "This shortcut already exists and was not created by Webo, so it was left untouched."),
        ["st.sc.fail"] = ("تعذّرت العملية: {0}", "The operation failed: {0}"),
        ["st.about"] = ("عن التطبيق", "About"),
        ["st.about.body"] = ("Webo {0} — إصدار مبكر. الخط: IBM Plex Sans Arabic (SIL OFL 1.1).", "Webo {0} — early build. Font: IBM Plex Sans Arabic (SIL OFL 1.1)."),
        ["st.data"] = ("بياناتك: الإعدادات في %LOCALAPPDATA%\\Webo، والمشاريع في المكان الذي تختاره. إزالة البرنامج لا تحذف مشاريعك.", "Your data: settings live in %LOCALAPPDATA%\\Webo and projects live where you choose. Uninstalling never deletes your projects."),
    };
}
