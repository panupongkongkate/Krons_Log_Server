# Krons_Log_Server

ASP.NET Core Web API สำหรับรับไฟล์ log จาก `Krons_Auto_Update`

## ประเภทโปรเจกต์

- เป็น ASP.NET Core Minimal API
- ใช้สำหรับรับไฟล์ log แบบ `multipart/form-data`
- มี solution file สำหรับเปิดจาก Visual Studio ที่ `Krons_Log_Server.sln`

## Endpoint

### Health check

```http
GET /api/logs/health
```

### Upload log file

```http
POST /api/logs/upload
```

header:

```text
X-Api-Key: <api key>
```

form fields:

- `file`
- `machineName`
- `appVersion`
- `fileName`
- `createdAtUtc`

## การเก็บไฟล์

- server จะเก็บไฟล์ไว้ใต้โฟลเดอร์ `Storage`
- path รูปแบบ:

```text
<ContentRoot>\Storage\<machineName>\<yyyy-MM-dd>\<fileName>
```

## config

อยู่ใน `appsettings.json`

```json
"Swagger": {
  "Enabled": true
},
"LogUpload": {
  "ApiKey": "test-api-key-123",
  "StorageRoot": "Storage"
}
```

ความหมาย:

- `Swagger.Enabled = true` จะเปิดทั้ง `/swagger` และ `/swagger/v1/swagger.json`
- `Swagger.Enabled = false` จะปิด Swagger ทั้งคู่
- `LogUpload.ApiKey` คือค่า API key ที่ฝั่ง client ต้องส่งมาใน header `X-Api-Key`
- `LogUpload.StorageRoot` คือโฟลเดอร์หลักสำหรับเก็บไฟล์ที่ upload เข้ามา
- Swagger UI มีปุ่ม `Authorize` สำหรับกรอก `X-Api-Key`

## วิธีรัน

```powershell
dotnet run
```

หรือเปิดจาก Visual Studio ด้วยไฟล์:

```text
Krons_Log_Server.sln
```

## วิธีใช้งานแบบเร็ว

1. รัน server
2. เปิด `http://localhost:5085/swagger` เพื่อลองยิง API
3. ตั้งค่า `X-Api-Key` ให้ตรงกับ `appsettings.json`
4. เรียก `POST /api/logs/upload` พร้อมไฟล์ `.txt`

ถ้าต้องการเข้า Swagger UI หลังรัน:

```text
http://localhost:5085/swagger
```

Swagger JSON:

```text
http://localhost:5085/swagger/v1/swagger.json
```

## Playwright Test

มี Playwright test สำหรับเช็ก Swagger UI flow อยู่ที่:

```text
tests-swagger
```

สิ่งที่ test ครอบคลุม:

- เปิด Swagger UI
- กด `Authorize`
- กรอก `X-Api-Key`
- ทดสอบ `GET /api/logs/health`
- ทดสอบ `POST /api/logs/upload`

วิธีรัน:

```powershell
cd tests-swagger
npm test
```

ถ้าต้องการดู browser ตอนรัน:

```powershell
cd tests-swagger
npm run test:headed
```

## หมายเหตุ

- endpoint upload ใช้ `X-Api-Key`
- ถ้า API key ไม่ถูกต้อง จะตอบ `401 Unauthorized`
- ตัวอย่างผลทดสอบจริงอยู่ใน `TEST_RESULTS.md`
