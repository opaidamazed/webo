# Webo v0.4.0 — ما تغيّر (انسخ الملفات فوق المشروع بنفس المسارات)

## جديد (ملفات جديدة)
- Services/ThemeCatalog.cs : لوحات الألوان (6 + مخصص)، الخطوط، شكل الحواف، حساب لون النص فوق اللون الأساسي
- Services/FontAssets.cs   : الخط المضمّن (IBM Plex Sans Arabic) كـ base64 للمعاينة وكملفات للتصدير
- Services/SiteExporter.cs : تصدير الموقع إلى <مجلد المشروع>\export + فحوصات قبل النشر

## معدّل
- Models/ProjectDoc.cs        : Schema 3، ThemeDoc، EditorSnapshot (التراجع يشمل الهوية الآن)
- Services/SiteRenderer.cs    : ثيم بمتغيرات CSS، 5 أقسام جديدة، وضع التصدير (CSS خارجي بلا JavaScript)
- Services/SectionCatalog.cs  : أنواع الأقسام الجديدة وحقولها ومحتواها الابتدائي + اكتشاف النص الافتراضي
- Services/ProjectStore.cs    : رفع Schema إلى 3 عند الحفظ، وضمان وجود Theme
- Services/Loc.cs             : نصوص الهوية والأقسام والتصدير والفحوصات (ar/en)
- Pages/EditorPage.xaml(.cs)  : لوحة "هوية الموقع"، زر التصدير، حقول مرقّمة
- Webo.App.csproj / installer/webo.iss / Pages/SettingsPage.xaml.cs / README.md : الإصدار 0.4.0 وتحديث التوثيق

## سلوك التصدير
- المجلد: <مجلد المشروع>\export  (index.html + صفحات + assets\site.css + assets\fonts)
- عند إعادة التصدير يُحذف فقط ما كتبه التصدير السابق (قائمة في export\.webo-export.json)؛ لا يُحذف أي ملف آخر
- الفحوصات لا تمنع التصدير؛ تعرض ملاحظات (نص افتراضي، زر بلا رابط، قسم تواصل بلا وسيلة…)
