# Webo v0.4.0 — ما تغيّر مقارنةً بنسخة 0.3.0 التي بنيتها (انسخ الملفات فوق المشروع بنفس المسارات)

## ملاحظاتك الأخيرة
1) المودالز (إنشاء مشروع، التأكيدات...) والخط
   - Services/Dialogs.cs      : Dialogs.Prepare = ثيم التطبيق + اتجاه + خط Plex على كل مودال (المودال يُفتح خارج نافذة التطبيق فلا يرث الثيم)
                                + PromptAsync جديد (سؤال بسطر واحد)
   - Services/NewProjectDialog.cs : يستخدم Prepare ويثبّت الخط على كل حقل
   - Themes/Webo.xaml         : ألوان المودال (الخلفية، الحدود، الفاصل) بلوحة التطبيق بدل الرمادي الافتراضي
   - Pages/EditorPage.xaml.cs : التلميحات (tooltips) وقائمة إضافة القسم بخط التطبيق صراحةً
2) الشريط الجانبي (الرئيسية/القوالب/الإعدادات) يختفي أثناء العمل داخل المشروع ويرجع في باقي الصفحات
   - MainWindow.xaml.cs
3) إعادة تسمية المشروع: زر قلم بجانب اسم المشروع في شريط الأدوات (يشمل التراجع، ويحدّث قائمة المشاريع الأخيرة فورًا).
   اسم المجلد على القرص لا يتغيّر.
   - Pages/EditorPage.xaml(.cs), Models/ProjectDoc.cs, Services/Loc.cs

## هوية الموقع والأقسام والتصدير (من الرسالة السابقة)
- جديد: Services/ThemeCatalog.cs, FontAssets.cs, SiteExporter.cs
- معدّل: SiteRenderer.cs, SectionCatalog.cs, ProjectStore.cs, Loc.cs, ProjectDoc.cs, EditorPage.xaml(.cs)

## الإصدار
- Webo.App.csproj / installer/webo.iss / Pages/SettingsPage.xaml.cs / README.md : 0.4.0
