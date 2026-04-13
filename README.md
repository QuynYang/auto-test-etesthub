# 🚀 Auto Test E-TestHub  
> Automated Testing dự án hệ thống thi trắc nghiệm trực tuyến E-TestHub

---

## 📌 Introduction
**Auto Test E-TestHub** là dự án kiểm thử tự động dành cho hệ thống thi trắc nghiệm trực tuyến **E-TestHub**.  
Mục tiêu của dự án là đảm bảo chất lượng phần mềm thông qua việc xây dựng các test case và thực thi kiểm thử tự động cho các chức năng chính của hệ thống.

Dự án thuộc môn **Bảo đảm chất lượng phần mềm (Software Quality Assurance)**.

---

## 🎯 Objectives
- Tự động hóa kiểm thử cho hệ thống E-TestHub  
- Giảm thiểu lỗi trong quá trình phát triển phần mềm  
- Tăng độ tin cậy và hiệu suất kiểm thử  
- Hỗ trợ regression testing nhanh chóng  
- Chuẩn hóa quy trình kiểm thử  

---

## 🛠️ Technologies Used
- Programming Language: Java / JavaScript / Python  
- Automation Framework: Selenium / Playwright / TestNG / JUnit  
- API Testing: Postman / RestAssured  
- Build Tool: Maven / npm  
- Version Control: Git, GitHub  
- CI/CD (optional): GitHub Actions  

---

## 🧪 Testing Scope

### 1. Functional Testing
- Đăng ký / Đăng nhập  
- Quản lý người dùng 

### 2. Project Testing
- Test các trang:
  - `https://e-testhub-frontend.onrender.com/Home/Login`
  - `https://e-testhub-frontend.onrender.com/Admin/UserManagement`

### 3. UI Testing
- Kiểm tra giao diện người dùng  
- Kiểm tra phân vùng tương đương
- Kiểm tra biên
- Kiểm tra logic điều kiện
- Kiểm tra trạng thái hệ thống
- Kiểm tra integration test

### 4. Regression Testing
- Đảm bảo các chức năng cũ không bị lỗi khi update  

---

## 📋 Test Plan

### Test Strategy
- Áp dụng Automation Testing để giảm effort manual  
- Kết hợp:
  - Unit Test  
  - Integration Test  
  - End-to-End Test  

### Test Data
- excel file: `baocao.xlsx`

---

## ▶️ How to Run Project

### 1. Clone repository
```bash
git clone https://github.com/QuynYang/auto-test-etesthub.git
cd auto-test-etesthub
```

### 2. Install dependencies
```bash
npm install
```
hoặc:
```bash
mvn clean install
```

### 3. Run tests
```bash
npm test
```
hoặc:
```bash
mvn test
```

---

## 📊 Test Reports
- word file: `baocao.docx` 

---

## ⚠️ Challenges & Solutions

| Problem | Solution |
|--------|---------|
| Dữ liệu test không ổn định | Tạo mock data |
| Test bị flaky | Thêm wait & retry |
| API thay đổi | Update test case |

---

## 📈 Achievements
- Xây dựng được hệ thống test tự động  
- Bao phủ các chức năng chính  
- Giảm thời gian test thủ công  
- Tăng độ chính xác kiểm thử  

---

## 🔮 Future Improvements
- Tích hợp CI/CD  
- Tăng coverage test  
- Thêm performance testing  
- Tối ưu test data management  

---

## 👨‍💻 Author
- Quỳnh Giang  
- Project: Auto Test E-TestHub - Bảo đảm chất lượng phần mềm 

---

## 📎 Related Project
- E-TestHub (Main System)  
- Automation Testing Framework  

---

## ⭐ Conclusion
Dự án giúp áp dụng thực tế các kỹ thuật kiểm thử tự động trong phát triển phần mềm, từ đó nâng cao chất lượng và độ ổn định của hệ thống.

