<div align="center">

<img src="src/Webo.App/Assets/Brand/derived/icons/dark/preview-256.png" alt="Webo" width="120" />

# Webo — ويبو

**ابنِ موقعًا متعدد الصفحات بصريًا، من غير ما تكتب سطر كود.**
**Build multi-page websites visually — no code required.**

[![Build](https://github.com/opaidamazed/webo/actions/workflows/build.yml/badge.svg)](https://github.com/opaidamazed/webo/actions/workflows/build.yml)
![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-0078D4)
![.NET](https://img.shields.io/badge/.NET-8-512BD4)
![WinUI](https://img.shields.io/badge/WinUI-3-2090F0)
![Status](https://img.shields.io/badge/status-early%20preview-orange)

[التحميل](../../releases) · [الفكرة](#-الفكرة) · [الهندسة البرمجية](#-الهندسة-البرمجية) · [خارطة الطريق](#-خارطة-الطريق) · [English](#english-summary)

</div>

---

<div dir="rtl" align="right">

## ⚠️ حالة المشروع

**Webo في مرحلة المعاينة المبكرة (Early Preview).** الأساس مُنفَّذ وما زال قيد الاختبار: واجهة عربية/إنجليزية، إدارة المشاريع والصفحات، الحفظ الآمن، سياسة أيقونة التطبيق، وبرنامج التثبيت. وفي v0.3.0 أُضيفت **المرحلة الأولى من المحرر البصري** (أقسام الصفحة، معاينة حقيقية، تحديد بالنقر، تراجع/إعادة) وهي **لم تُجرَّب على أجهزة Windows بعد**. أما **القوالب والتصدير والاختبارات وإدارة الصور فلم تُنفَّذ بعد** (انظر [خارطة الطريق](#-خارطة-الطريق)). لا يوجد في هذا الإصدار ما يدّعي غير ذلك، والواجهة نفسها تكتب "قريبًا" على كل ما لم يكتمل.

## 💡 الفكرة

كثير من الناس يحتاجون موقعًا (شركة، مطعم، معرض أعمال، صفحة هبوط، موقع تعليمي) ولا يعرفون HTML أو CSS أو JavaScript. أدوات بناء المواقع الجاهزة تعمل غالبًا على الإنترنت، وتفرض خدمتها، وتضعف دعم العربية واتجاه RTL.

**Webo تطبيق لسطح مكتب Windows** يعمل محليًا، ويعطيك:

- **مشروع منظَّم لا ملفات متناثرة:** موقعك يُحفظ كنموذج بيانات (صفحات ومكونات وخصائص) وليس كملفات HTML تعدّلها يدويًا.
- **محرر بصري** (قيد التطوير): تحدد العنصر من المعاينة نفسها وتعدّله، دون لمس الكود.
- **العربية أولًا:** واجهة RTL كاملة بخط IBM Plex Sans Arabic، مع فصل لغة التطبيق عن لغة الموقع الذي تبنيه.
- **ملكية كاملة لبياناتك:** مشاريعك في مكان تختاره أنت، وإزالة البرنامج لا تحذفها.
- **صدق في الأداء:** لا تُعرض وظيفة تحتاج خادمًا أو قاعدة بيانات على أنها تعمل لمجرد وجود واجهتها.

## ✅ ما هو مُنفَّذ في v0.3.0 (قيد الاختبار)

| المجال | الحالة (التحقق على أجهزة Windows جارٍ) |
|---|---|
| واجهة عربية / إنجليزية (RTL / LTR) | مُنفَّذ |
| ثيم فاتح / داكن / مثل النظام | مُنفَّذ |
| إنشاء مشروع (اسم، نوع الموقع، لغة الموقع، مكان الحفظ) | مُنفَّذ |
| صفحات المشروع: إضافة، إعادة تسمية، تعيين رئيسية، حذف | مُنفَّذ |
| أحجام المعاينة: كمبيوتر / تابلت / موبايل (إطار تجريبي) | مُنفَّذ |
| حفظ ذري مع نسخة `.bak` + حفظ تلقائي كل 15 ثانية | مُنفَّذ |
| المشاريع الأخيرة (الإزالة من القائمة لا تحذف ملفات) | مُنفَّذ |
| الأيقونة: الأصلية الداكنة ثابتة خارج التطبيق، وتتبدّل داخله فقط (فاتحة/داكنة) مع الثيم + إعادة إنشاء اختصارات Webo بموافقتك | مُنفَّذ |
| برنامج تثبيت لكل مستخدم (بدون صلاحيات مدير) | مُنفَّذ |
| أقسام الصفحة: إضافة، ترتيب، تكرار، حذف (5 أنواع: رئيسي، نص، مزايا، دعوة لإجراء، تذييل) | مُنفَّذ (جديد، لم يُجرَّب بعد) |
| معاينة حقيقية للصفحة داخل WebView2 + تحديد القسم بالنقر + تعديل حقوله | مُنفَّذ (جديد، لم يُجرَّب بعد) |
| التراجع والإعادة (يشمل الصفحات والأقسام والحقول) | مُنفَّذ (جديد، لم يُجرَّب بعد) |
| القوالب ومكتبة المكونات، إدارة الصور والأصول | لم يُنفَّذ |
| الاختبارات والتصدير ومعالج النشر | لم يُنفَّذ |

## 📥 التحميل والتثبيت

من صفحة [Releases](../../releases):

1. **`Webo-Setup-<version>.exe`** — برنامج التثبيت (موصى به). يثبّت لك كمستخدم دون صلاحيات مدير.
2. **`Webo-<version>-portable-win-x64.zip`** — نسخة محمولة: فك الضغط وشغّل `Webo.exe`.
3. **`SHA256SUMS.txt`** — للتحقق من سلامة الملفات: `Get-FileHash <الملف> -Algorithm SHA256`.

> الملفات **غير موقّعة رقميًا** حاليًا، فقد يعرض Windows SmartScreen تحذيرًا؛ اختر **More info ← Run anyway** إذا تحققت من الـ SHA256.

**المتطلبات:** Windows 10 (1809+) أو Windows 11، معمارية x64. لا حاجة لتثبيت .NET أو Windows App SDK (التطبيق self-contained). المعاينة تحتاج مكوّن **Microsoft Edge WebView2 Runtime** (موجود مع Windows 11 ومعظم أجهزة Windows 10 المحدّثة)؛ لو غاب تظهر رسالة واضحة ويستمر التحرير والحفظ بدونها.

## 🏗️ الهندسة البرمجية

### التقنيات

| الطبقة | الاختيار | السبب |
|---|---|---|
| الواجهة | **WinUI 3** (Windows App SDK 1.5) | تطبيق أصلي بمظهر Windows الحديث ودعم RTL ممتاز |
| اللغة والمنصة | **C# / .NET 8** | أداء جيد وأدوات ناضجة |
| التوزيع | **Unpackaged + Self-contained** | بدون MSIX، ويعمل بمثبّت تقليدي أو نسخة محمولة |
| المثبّت | **Inno Setup 6** | تثبيت لكل مستخدم وإزالة نظيفة |
| البناء والإصدار | **GitHub Actions** (`windows-latest`) | بناء قابل للتكرار + Release تلقائي عند وسم `v*` |
| المعاينة | **WebView2** | عرض الصفحة الحقيقية (HTML/CSS) داخل المحرر |
| الخط | **IBM Plex Sans Arabic** (SIL OFL) | مقروء ومناسب للواجهات |

### هيكل المستودع

```text
webo/
├─ src/Webo.App/
│  ├─ App.xaml(.cs)            نقطة الدخول والموارد العامة
│  ├─ MainWindow.xaml(.cs)     الإطار: شريط العنوان، التنقل، اللغة، الثيم، الأيقونة
│  ├─ Pages/
│  │  ├─ HomePage              الرئيسية والمشاريع الأخيرة
│  │  ├─ EditorPage            المحرر (الصفحات، الأقسام، المعاينة الحقيقية، الخصائص، التراجع)
│  │  └─ SettingsPage          اللغة، الثيم، الأيقونة (للعرض فقط)، الاختصارات
│  ├─ Services/
│  │  ├─ ProjectStore          إنشاء/تحميل/حفظ المشاريع والمشاريع الأخيرة
│  │  ├─ Settings              إعدادات المستخدم (LocalAppData)
│  │  ├─ IconService           سياسة الأيقونة: ثابتة داكنة للنظام، وتتبدّل داخل التطبيق فقط مع الثيم
│  │  ├─ ShortcutHelper        اختصارات .lnk عبر IShellLink (باتفاقية موافقة)
│  │  ├─ SiteRenderer          يحوّل النموذج إلى HTML (المعاينة الآن، والتصدير لاحقًا)
│  │  ├─ SectionCatalog        أنواع الأقسام وحقولها ومحتواها الابتدائي
│  │  ├─ EditHistory           التراجع والإعادة (لقطات من النموذج)
│  │  ├─ Loc                   نصوص الواجهة (ar/en)
│  │  └─ Dialogs, NewProjectDialog, AppLog
│  ├─ Models/                  ProjectDoc, PageDoc, RecentProject
│  ├─ Themes/                  Webo.xaml (ألوان/خطوط) + Styles.xaml (أنماط)
│  └─ Assets/                  Fonts/ و Brand/derived/icons/
├─ installer/webo.iss          سكربت Inno Setup
└─ .github/workflows/          البناء والاختبار الدخاني وإنتاج المثبّت
```

### نموذج المشروع

الموقع **بيانات منظَّمة** وليس HTML يُحرَّر يدويًا. كل مشروع مجلد يحتوي `project.webo.json`:

```json
{
  "Schema": 2,
  "Id": "…",
  "Name": "موقعي",
  "SiteType": "business",
  "Language": "ar",
  "Direction": "rtl",
  "Pages": [
    {
      "Id": "…", "Title": "الرئيسية", "Slug": "index", "IsHome": true,
      "Sections": [
        { "Id": "…", "Type": "hero", "Props": { "title": "…", "subtitle": "…" } }
      ]
    }
  ]
}
```

هذا النموذج سيكون **مصدر الحقيقة الوحيد** للتحرير والمعاينة والاختبار والتصدير، فيُولَّد الـ HTML من النموذج لا العكس.

### قرارات تصميمية

- **لا فقد صامت للبيانات:** الحفظ ذري (ملف مؤقت ثم استبدال) مع الاحتفاظ بنسخة `.bak`، ولا يُكتب داخل مجلد موجود بالفعل، ولا تُحذف ملفات المستخدم عند الإزالة من قائمة المشاريع.
- **الإعدادات لكل مستخدم:** في `%LOCALAPPDATA%\Webo`. الأيقونة ليست إعدادًا: لا شيء يُحفظ عنها فلا يمكن أن تختلف عن الثيم.
- **سياسة الأيقونة:** خارج التطبيق (الملف التنفيذي، شريط المهام، سطح المكتب، قائمة Start، قائمة التطبيقات المثبّتة) تظهر دائمًا الأيقونة الأصلية **الداكنة**. داخل التطبيق فقط تتبدّل صورة شريط العنوان بين الفاتحة والداكنة مع الثيم. أيقونة **mono** يستخدمها المثبّت (`Setup.exe` ونافذة المعالج) فقط. لا يغيّر التطبيق أي أيقونة خارجه.
- **لا تعديل لما لا يخص Webo:** لا يلمس التطبيق إلا الاختصارات التي أنشأها بنفسه (يميّزها بعلامة في الوصف) وبعد موافقة المستخدم.
- **فشل ناعم:** ملف إعدادات تالف أو أيقونة مفقودة لا يُسقطان التطبيق، بل يعود للقيمة الافتراضية ويُسجَّل الخطأ محليًا.
- **لا اتصال بالإنترنت** في الوظائف الأساسية، ولا تتبّع.
- **فصل اللغتين:** لغة واجهة التطبيق مستقلة تمامًا عن لغة واتجاه الموقع الذي تبنيه.

## 🔨 البناء من المصدر

المتطلبات: Windows، و[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)، و(اختياري للمثبّت) [Inno Setup 6](https://jrsoftware.org/isinfo.php).

```powershell
dotnet publish src/Webo.App/Webo.App.csproj -c Release -r win-x64 -p:Platform=x64 -o out
# المثبّت (اختياري)
ISCC.exe /DAppVersion=0.3.0 installer\webo.iss
```

أو دع **GitHub Actions** يبني لك: شغّل workflow **Build Webo (Windows)** من تبويب Actions، أو ادفع وسمًا مثل `v0.3.0` ليُنشر Release تلقائيًا.

## 🗺️ خارطة الطريق

- [x] **المرحلة 1 — الأساس:** الهيكل، الواجهة ثنائية اللغة، المشاريع والصفحات، الحفظ الآمن، الإعدادات، المثبّت
- [x] **المحرر البصري — المرحلة 1 (v0.3.0، قيد الاختبار):** معاينة حقيقية عبر WebView2، تحديد الأقسام بالنقر، لوحة الخصائص، تراجع/إعادة
- [ ] **المحرر البصري — المرحلة 2:** تعديل النص مباشرة داخل المعاينة، السحب والإفلات، الصور والأصول، الهوية البصرية (ألوان وخطوط الموقع)
- [ ] **القوالب ومكتبة المكونات**، ومعالج الأسئلة التوجيهية
- [ ] **التصدير** (ملفات HTML/CSS/JS جاهزة) والاختبارات قبل التصدير ومعالج النشر
- [ ] **المراحل المتقدمة:** نماذج، SEO، تقارير، إدارة الأصول
- [ ] **التجهيز للإصدار:** اختبار التوافق والأمان، توقيع الملفات، حزم تحديث يدوية
- [ ] **موقع المنتج الرسمي**

## 🤝 المساهمة

المشروع في بدايته، وأي ملاحظة مفيدة: افتح [Issue](../../issues) مع وصف المشكلة وإصدار Windows وخطوات التكرار. ولو أعجبتك الفكرة، **⭐ نجمة للمستودع** تساعد المشروع كثيرًا.

## 📄 الترخيص والأصول

- **الخط:** IBM Plex Sans Arabic بترخيص [SIL OFL 1.1](src/Webo.App/Assets/Fonts/OFL-IBMPlexSansArabic.txt).
- **الشعار والأيقونات:** هوية Webo البصرية، جميع الحقوق محفوظة لصاحبها ما لم يُذكر خلاف ذلك.
- **كود المشروع:** سيُضاف ملف `LICENSE` قريبًا؛ إلى ذلك الحين لا يُمنح ترخيص إعادة استخدام صريح.

</div>

---

## English summary

**Webo** is a Windows desktop app for building **multi-page websites visually, without writing code**. It is built with **C# / WinUI 3 / .NET 8**, ships as an unpackaged, self-contained app with an Inno Setup installer, and is designed **Arabic-first** (full RTL UI, IBM Plex Sans Arabic) while also supporting English (LTR).

**Status: early preview (v0.3.0).** Implemented (still under testing): bilingual UI, light/dark themes, project creation and page management, atomic autosave with `.bak` backups, recent projects, a fixed dark app icon (switching light/dark inside the app only), and a per-user installer. **New in v0.3.0, not yet verified on real Windows machines:** page sections (add / reorder / duplicate / delete), a real WebView2 page preview with click-to-select, per-section field editing, and undo/redo. **Not implemented yet:** templates and components, image/asset management, testing and export. The UI labels everything unfinished as "coming soon".

**Architecture in one paragraph:** a site is stored as structured data (`project.webo.json`: pages, properties, language, direction), not as hand-edited HTML. That model will be the single source of truth for editing, preview, testing and export. Settings are per-user in `%LOCALAPPDATA%\Webo`; saves are atomic; the app never touches files or shortcuts it didn't create; and the UI language is independent from the language of the site being built.

**Install:** grab `Webo-Setup-<version>.exe` (or the portable ZIP) from [Releases](../../releases). Files are currently **unsigned**, so SmartScreen may warn; verify with `SHA256SUMS.txt`.

**Build:** `dotnet publish src/Webo.App/Webo.App.csproj -c Release -r win-x64 -p:Platform=x64 -o out` (Windows + .NET 8 SDK), or run the **Build Webo (Windows)** GitHub Actions workflow.

If you find the idea useful, a ⭐ helps a lot.
