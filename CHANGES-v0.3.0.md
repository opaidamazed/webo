# Webo v0.3.0 — ما تغيّر (انسخ الملفات فوق المشروع بنفس المسارات)

## المحرر البصري (المرحلة 1)
- جديد: Services/SiteRenderer.cs, SectionCatalog.cs, EditHistory.cs
- معدّل: Models/ProjectDoc.cs, Pages/EditorPage.xaml(.cs), Services/Loc.cs, Services/ProjectStore.cs

## سياسة الأيقونات الجديدة
- Services/IconService.cs : الأيقونة الداكنة ثابتة للنافذة/شريط المهام؛ شريط العنوان داخل التطبيق فقط يتبدّل مع الثيم
- Services/Settings.cs    : حُذف الحقل AppIcon (ملفات الإعدادات القديمة تُتجاهَل فيها هذه الخاصية بدون مشاكل)
- Pages/SettingsPage.xaml(.cs) : حُذف منتقي الأيقونات؛ أُضيف شرح السياسة ومعاينة الأيقونة الحالية داخل التطبيق
- MainWindow.xaml.cs      : تحديث صورة شريط العنوان عند تغيّر الثيم
- installer/webo.iss      : SetupIconFile = mono (للمثبّت فقط)، وباقي الأماكن الداكنة. + دعم صور المعالج تلقائيًا
- installer/assets/README.md : أسماء ومقاسات صور المعالج

## إصلاح اهتزاز صفحة الإعدادات (مُضمَّن)
- Pages/SettingsPage.xaml(.cs), MainWindow.xaml.cs

## ملفات fix2 (للاطمئنان فقط، لا تغيير عليها)
- App.xaml, Themes/Webo.xaml, Themes/Styles.xaml

## يمكن حذفه (اختياري)
- لا شيء إلزامي. (ShortcutHelper.cs ما زال مستخدمًا لإعادة إنشاء الاختصارات.)
