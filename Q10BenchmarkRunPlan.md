# Q10 - خطة تشغيل القياس الفعلي (Before/After CSV)

> هذا الملف يكمّل `Q10Plan.md`: يشرح بالضبط كيف تشغّل `load-test.ps1` بنفسك لتوليد `before-no-cache.csv` و `after-with-cache.csv` فعلياً، وتأكيد أن منطق Cache HIT/MISS Logging جاهز مسبقاً في الكود (تم تطبيقه في Q10، لا حاجة لأي إضافة جديدة).

---

## تأكيد: لا حاجة لتعديل `CacheAspect.cs` مرة أخرى

التعديل المطلوب أُضيف فعلاً سابقاً في `AOP/CacheAspect.cs`:
- `Stopwatch` يقيس الزمن حول منطق الكاش.
- `Console.WriteLine($"[Cache HIT] ...")` عند إيجاد القيمة بالكاش (Redis) بدون أي اتصال بقاعدة البيانات.
- `Console.WriteLine($"[Cache MISS] ...")` عند عدم وجودها، فيُنفَّذ الاستعلام الحقيقي ثم تُخزَّن النتيجة.

هذا المنطق يعمل فقط على الدوال التي تحمل `[Cache(60)]` — وهي حالياً `GetAllProductsAsyncWithCache()`، المستخدَمة من `GET /api/products/products-with-cache`. أما `GET /api/products` العادي (`GetAllAsync()`) فلا يمر بـ `CacheAspect` أصلاً، فلن تظهر له رسائل HIT/MISS — فقط سطر `PERF:` العادي من `PerformanceMiddleware`، وهذا متوقَّع وصحيح.

---

## خطوات التشغيل بالترتيب

### 1) تشغيل Redis والسيرفر

```powershell
dotnet run --urls=http://localhost:5202
```
اترك هذا الكونسول مفتوحاً طوال الاختبار — منه ستشاهد `[Cache HIT]` / `[Cache MISS]` و `PERF:` مباشرة.

### 2) تشغيل "قبل" (بدون كاش)

يستهدف `GetAllAsync()` غير المُخزَّن عبر `GET /api/products`:

```powershell
.\load-test.ps1 -Url "http://localhost:5202/api/products" -Method GET -TotalRequests 200 -Concurrency 20 -CsvOutput "before-no-cache.csv"
```

### 3) تشغيل "بعد" (مع كاش) - مرتين متتاليتين

يستهدف `GetAllProductsAsyncWithCache()` عبر `GET /api/products/products-with-cache`:

```powershell
.\load-test.ps1 -Url "http://localhost:5202/api/products/products-with-cache" -Method GET -TotalRequests 200 -Concurrency 20 -CsvOutput "after-with-cache.csv"
.\load-test.ps1 -Url "http://localhost:5202/api/products/products-with-cache" -Method GET -TotalRequests 200 -Concurrency 20 -CsvOutput "after-with-cache.csv"
```

> **مهم:** التشغيلة الأولى تحتوي أول طلب بطيء نسبياً (Cache Miss يضرب قاعدة البيانات لأول مرة). اعتمد نتائج **التشغيلة الثانية** فقط للتقرير النهائي، لأن الكاش يكون "دافئ" بالكامل فيها.

### 4) راقب كونسول السيرفر أثناء الخطوة 3

سترى أول رسالة `[Cache MISS] ... executed against the database in X ms`، وكل الرسائل بعدها `[Cache HIT] ... served from Redis in X ms (no DB call)` — هذا دليل مباشر ومرئي على الفرق قبل حتى النظر للأرقام النهائية.

### 5) النتيجة بعد التشغيل

سيظهر فعلياً بجذر المشروع:
- `before-no-cache.csv`
- `after-with-cache.csv`

كل ملف يحتوي تفصيل كل طلب فردي بالأعمدة: `StatusCode`, `Success`, `ElapsedMs`, `Error`.

### 6) تعبئة الجدول النهائي

افتح `Q10Plan.md`، وانسخ الأرقام الحقيقية (Total/Successful/Failed/Average/Min/Max/Duration) من ملخص الكونسول الذي يطبعه `load-test.ps1` نفسه بعد كل تشغيلة (`=== Stress Test Results ===`)، وعبّئ جدول "Before/After Measurement" بهذه الأرقام الحقيقية بدل القيم الفارغة.
