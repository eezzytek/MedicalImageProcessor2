# Medical Image Processor
### AI-додаток для виявлення пухлин мозку та переломів кісток

Технології
* .NET
* xUnit
* React
* .NET MAUI
* ONNX для ML

Даний додаток використовується для швидкого аналізу медичних зображень за допомогою штучного інтелекту:

* Виявлення пухлин мозку (MRI)
* Виявлення переломів кісток (X-Ray)
* JWT-автентифікація
* Автоматичне завантаження та аналіз зображень

### Структура проєкту
```
MedicalImageProcessor/
├── MedicalImageProcessor.Application/     ← Бізнес-логіка
├── MedicalImageProcessor.Core/            ← Ядро застосунку з основними структурами
├── MedicalImageProcessor.Infrastructure/  ← ONNX-моделі та обробка
├── MedicalImageProcessor.Mobile/          ← MAUI додаток (Android/iOS)
├── MedicalImageProcessor.Tests/           ← Тести на xUnit
├── MedicalImageProcessor.WebApi/          ← Backend API (.NET 9)
├── Vagrantfile
└── README.md                              
```

### Як запустити
1. Запуск бекенду
```
cd MedicalImageProcessor.WebApi
dotnet run
```
→ API доступний за адресою:
http://localhost:5077/swagger

2. Запуск мобільного додатку (MAUI)
```
cd MedicalImageProcessor.Mobile
dotnet build -t:Run -f net9.0-android
```
3. Запуск веб-застосунку
```
cd MedicalImageProcessor.Frontend
npm run dev
```

### Тестування
1. Запусти бекенд → http://localhost:5077/swagger
2. Зареєструйся або увійди
3. Запусти MAUI додаток
4. Завантаж будь-яке медичне зображення
5. Отримай результат за 2–4 секунди

### Автори
Гусенко Владислав, Євген Семенюк, Іванченко Артур
