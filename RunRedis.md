# 📘 ملف أوامر التيرمينال — الطلب السادس (Redis + Docker + AOP Cache)

## 🟦 1) تشغيل Redis لأول مرة
> تعمل هذا الأمر مرة واحدة فقط لإنشاء وتشغيل Redis داخل Docker

```bash
docker run -d -p 6379:6379 --name redis redis
```

---

## 🟩 2) التأكد أن Redis شغّال

```bash
docker ps
```

إذا ظهر `redis` → Redis يعمل بشكل صحيح.

---

## 🟧 3) الدخول إلى Redis CLI
> تستخدمه فقط إذا بدك تفحص المفاتيح أو تمسح الكاش

```bash
docker exec -it redis redis-cli
```

---

# 🟥 4) أوامر Redis المهمة داخل redis-cli

## 🔹 عرض كل المفاتيح المخزّنة

```bash
KEYS *
```

## 🔹 مسح الكاش بالكامل

```bash
FLUSHALL
```

## 🔹 اختبار Redis

```bash
PING
```

النتيجة المتوقعة:

```
PONG
```

## 🔹 جلب قيمة مفتاح معيّن

```bash
GET mykey
```

---

# 🟦 5) تشغيل مشروع الـ API

## 🔹 تشغيل عادي

```bash
dotnet run
```

## 🔹 تشغيل مع مراقبة التغييرات

```bash
dotnet watch run
```

---

# 🟩 6) اختبار الكاش عبر ملف .http

## 🔹 إرسال طلب GET للمنتجات

```http
GET http://localhost:5202/api/products
Accept: application/json
```

✔️ أول مرة → بطيء (من قاعدة البيانات)  
✔️ ثاني مرة → سريع جداً (من Redis)

---

# 🟧 7) فحص الكاش بعد إرسال الطلب

افتح Redis CLI:

```bash
docker exec -it redis redis-cli
```

ثم:

```bash
KEYS *
```

إذا ظهر:

```
"GetAllProductsAsyncWithCache-"
```

فهذا يعني:

✔️ الكاش شغّال  
✔️ AOP شغّال  
✔️ Redis خزّن البيانات

---

# 🟦 8) تنظيف الكاش أثناء العرض

```bash
docker exec -it redis redis-cli
FLUSHALL
```

ثم:

```bash
KEYS *
```

النتيجة:

```
(empty array)
```

---

# 🟩 9) إعادة تشغيل Redis (إذا لزم الأمر)

## 🔹 إيقاف Redis

```bash
docker stop redis
```

## 🔹 تشغيل Redis

```bash
docker start redis
```

## 🔹 حذف Redis بالكامل (إذا بدك تعيد من الصفر)

```bash
docker rm -f redis
```

ثم إعادة تشغيله:

```bash
docker run -d -p 6379:6379 --name redis redis
```

---

# 🟧 10) أوامر إضافية مفيدة

## 🔹 عرض لوجات Redis

```bash
docker logs redis
```

## 🔹 عرض معلومات Redis

```bash
INFO
```

## 🔹 عرض حجم الذاكرة المستخدمة

```bash
INFO memory
```

---

# 🎉 أهم 3 أوامر لازم تفرجيها للدكتور

## 1) إرسال طلب GET:

```http
GET http://localhost:520