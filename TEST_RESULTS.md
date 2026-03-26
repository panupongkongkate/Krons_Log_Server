# Test Results

ผลการทดสอบ endpoint ของโปรเจกต์ `Krons_Log_Server`

## คำสั่งที่ใช้รัน server

```powershell
dotnet run --urls http://localhost:5085
```

## Test 1: Health endpoint

### Input

```http
GET http://localhost:5085/api/logs/health
```

### Output

```json
{
  "success": true,
  "service": "Krons_Log_Server",
  "utcNow": "2026-03-26T20:44:31.7245886Z"
}
```

## Test 2: Swagger UI endpoint

### Input

```http
GET http://localhost:5085/swagger
```

### Output

```text
HTTP 200 OK
พบคำว่า Swagger UI ใน HTML response
```

## Test 3: Swagger JSON endpoint

### Input

```http
GET http://localhost:5085/swagger/v1/swagger.json
```

### Output

```json
{
  "openapi": "3.0.1",
  "info": {
    "title": "Krons_Log_Server",
    "version": "1.0"
  }
}
```

## Test 4: Upload endpoint

### Input

header:

```text
X-Api-Key: test-api-key-123
```

form fields:

```text
machineName = TEST-PC
appVersion = 1.0.0
fileName = sample-log.txt
createdAtUtc = 2026-03-27T00:00:00Z
file = D:\repos\Krons_Log_Server\sample-log.txt
```

sample file content:

```text
ENC:sample-upload-content
```

request:

```http
POST http://localhost:5085/api/logs/upload
```

### Output

```json
{
  "success": true,
  "fileName": "sample-log.txt",
  "machineName": "TEST-PC",
  "appVersion": "1.0.0",
  "createdAtUtc": "2026-03-27T00:00:00Z",
  "storedPath": "D:\\repos\\Krons_Log_Server\\Storage\\TEST-PC\\2026-03-26\\sample-log.txt"
}
```

## ตรวจสอบไฟล์ที่ถูกเก็บจริง

stored file:

```text
D:\repos\Krons_Log_Server\Storage\TEST-PC\2026-03-26\sample-log.txt
```

result:

- พบไฟล์ถูกสร้างจริง
- ขนาดไฟล์ 27 bytes

## สรุปผลการทดสอบ Swagger

config ที่ใช้:

```json
"Swagger": {
  "Enabled": true
}
```

result:

- `/swagger` เข้าได้จริง
- `/swagger/v1/swagger.json` เข้าได้จริง
- เมื่อ `Swagger.Enabled` เป็น `true` ตัว server เปิดเอกสาร API ได้ตามคาด

## Test 5: ปิด Swagger ด้วย config

### Input

แก้ `appsettings.json` ชั่วคราวเป็น:

```json
"Swagger": {
  "Enabled": false
}
```

แล้วรัน:

```http
GET http://localhost:5086/swagger
```

### Output

```text
HTTP 404 Not Found
```

result:

- เมื่อ `Swagger.Enabled` เป็น `false` จะเข้า `/swagger` ไม่ได้
- หลังทดสอบเสร็จได้คืนค่า `appsettings.json` กลับเป็น `true` แล้ว
