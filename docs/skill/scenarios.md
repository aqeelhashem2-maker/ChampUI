# Five coding tasks

Each pair describes one task in Arabic and English. Use either language to request the task. These prompts can be reused with a new model or a different agent environment.

| # | العربية | English | Observable acceptance |
| --- | --- | --- | --- |
| 1 | أضف صفحة عملاء إلى لوحة ChampUI. أريد قائمة قابلة للبحث وزر تفاصيل يفتح حوارًا، مع استعادة تركيز لوحة المفاتيح عند إغلاقه. | Add a searchable customer list and a details dialog that restores keyboard focus after dismissal. | Locate one matching row, inspect its actual customer details, close with Escape, verify the same row launcher regains focus, and clear an empty search. |
| 2 | أنشئ نموذج موظف يرفض الاسم الفارغ ويختار قسمًا ويؤكد الحفظ بإشعار. | Build an employee form that rejects a blank name, selects a department and confirms a successful save with a toast. | Invalid submission produces an error and no record. Saving a valid employee shows the selected department and a success message. |
| 3 | أضف قائمة مواعيد صالون بيوم قابل للتغيير مع أسماء الخدمات وإظهار حالة الفراغ. | Add a salon appointment list with day selection, service names and an empty state. | Select two dates and verify the matching services, select an empty day, and check the selected day under Arabic RTL. |
| 4 | أضف صفحة مؤشرات مالية باللغتين تعرض الإيراد ورسمًا شهريًا وتعمل مع تبديل اللغة والوضع الداكن. | Add a bilingual revenue KPI and monthly chart page that responds to language and dark-theme switching. | Verify the decimal total, three monthly values, chart bars, computed theme color, translated accessible names and mobile RTL layout. |
| 5 | أضف صفحة تعرض أسعار خدمات من REST عبر خدمة ASP.NET Core وتوضح تعذر التحميل. | Show service rates from REST through an ASP.NET Core service and handle a failed request. | Exercise successful, empty and HTTP 503 responses via a typed client, and recover by selecting a working location. |
