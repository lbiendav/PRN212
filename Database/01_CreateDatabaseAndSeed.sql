/*
    MỤC ĐÍCH:
    - Tạo database ExamManagementSystem nếu chưa có.
    - Tạo lại toàn bộ bảng đúng với Entity Framework Core trong project.
    - Chèn tài khoản mẫu, 3 đề thi và 15 câu hỏi có đáp án.

    LƯU Ý:
    - Phần DROP TABLE làm script có thể chạy lại trong lúc học.
    - Nếu chạy lại, toàn bộ dữ liệu cũ trong các bảng của project sẽ bị xóa.
*/

-- Chuyển sang master vì database đích có thể chưa tồn tại.
USE master;
GO

-- Chỉ tạo database khi tên này chưa có trên SQL Server.
IF DB_ID(N'ExamManagementSystem') IS NULL
BEGIN
    CREATE DATABASE ExamManagementSystem;
END;
GO

-- Mọi câu lệnh sau đây thao tác trong database của ứng dụng.
USE ExamManagementSystem;
GO

-- Xóa bảng con trước bảng cha để không vi phạm khóa ngoại khi chạy lại script.
IF OBJECT_ID(N'dbo.AttemptAnswers', N'U') IS NOT NULL DROP TABLE dbo.AttemptAnswers;
IF OBJECT_ID(N'dbo.ExamAttempts', N'U') IS NOT NULL DROP TABLE dbo.ExamAttempts;
IF OBJECT_ID(N'dbo.AnswerOptions', N'U') IS NOT NULL DROP TABLE dbo.AnswerOptions;
IF OBJECT_ID(N'dbo.Questions', N'U') IS NOT NULL DROP TABLE dbo.Questions;
IF OBJECT_ID(N'dbo.Exams', N'U') IS NOT NULL DROP TABLE dbo.Exams;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
GO

-- Users lưu tài khoản, mật khẩu đã băm, vai trò và trạng thái khóa/mở.
CREATE TABLE dbo.Users
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Username        NVARCHAR(50) NOT NULL,
    PasswordHash    NVARCHAR(300) NOT NULL,
    FullName        NVARCHAR(100) NOT NULL,
    Email           NVARCHAR(120) NOT NULL,
    Role            NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'Student',
    IsActive        BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    CreatedAt       DATETIME2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Student', N'Admin'))
);
GO

-- Exams lưu thông tin chung của một đề; câu hỏi được tách sang bảng Questions.
CREATE TABLE dbo.Exams
(
    Id               INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Exams PRIMARY KEY,
    Title            NVARCHAR(150) NOT NULL,
    Subject          NVARCHAR(80) NOT NULL,
    Description      NVARCHAR(500) NOT NULL CONSTRAINT DF_Exams_Description DEFAULT N'',
    DurationMinutes  INT NOT NULL,
    IsActive         BIT NOT NULL CONSTRAINT DF_Exams_IsActive DEFAULT 1,
    CreatedAt        DATETIME2 NOT NULL CONSTRAINT DF_Exams_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Exams_Duration CHECK (DurationMinutes BETWEEN 1 AND 300)
);
GO

-- Questions chứa nội dung và thứ tự câu trong từng đề.
CREATE TABLE dbo.Questions
(
    Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Questions PRIMARY KEY,
    ExamId       INT NOT NULL,
    Content      NVARCHAR(1000) NOT NULL,
    OrderNumber  INT NOT NULL,
    CONSTRAINT FK_Questions_Exams FOREIGN KEY (ExamId) REFERENCES dbo.Exams(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Questions_Exam_Order UNIQUE (ExamId, OrderNumber)
);
GO

-- AnswerOptions lưu các lựa chọn; mỗi câu trong dữ liệu mẫu có đúng một dòng IsCorrect = 1.
CREATE TABLE dbo.AnswerOptions
(
    Id          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AnswerOptions PRIMARY KEY,
    QuestionId  INT NOT NULL,
    Content     NVARCHAR(500) NOT NULL,
    IsCorrect   BIT NOT NULL CONSTRAINT DF_AnswerOptions_IsCorrect DEFAULT 0,
    CONSTRAINT FK_AnswerOptions_Questions FOREIGN KEY (QuestionId) REFERENCES dbo.Questions(Id) ON DELETE CASCADE
);
GO

-- ExamAttempts là phần đầu của một lần nộp bài, lưu điểm và số câu đúng.
CREATE TABLE dbo.ExamAttempts
(
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ExamAttempts PRIMARY KEY,
    UserId          INT NOT NULL,
    ExamId          INT NOT NULL,
    CorrectAnswers  INT NOT NULL,
    TotalQuestions  INT NOT NULL,
    Score           DECIMAL(4,2) NOT NULL,
    StartedAt       DATETIME2 NOT NULL,
    SubmittedAt     DATETIME2 NOT NULL,
    CONSTRAINT FK_ExamAttempts_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
    CONSTRAINT FK_ExamAttempts_Exams FOREIGN KEY (ExamId) REFERENCES dbo.Exams(Id),
    CONSTRAINT CK_ExamAttempts_Score CHECK (Score BETWEEN 0 AND 10)
);
GO

-- AttemptAnswers lưu từng lựa chọn để màn hình lịch sử biết câu nào đúng/sai.
CREATE TABLE dbo.AttemptAnswers
(
    Id                INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AttemptAnswers PRIMARY KEY,
    ExamAttemptId     INT NOT NULL,
    QuestionId        INT NOT NULL,
    SelectedOptionId  INT NULL,
    IsCorrect         BIT NOT NULL,
    CONSTRAINT FK_AttemptAnswers_Attempts FOREIGN KEY (ExamAttemptId) REFERENCES dbo.ExamAttempts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AttemptAnswers_Questions FOREIGN KEY (QuestionId) REFERENCES dbo.Questions(Id),
    CONSTRAINT FK_AttemptAnswers_SelectedOption FOREIGN KEY (SelectedOptionId) REFERENCES dbo.AnswerOptions(Id),
    CONSTRAINT UQ_AttemptAnswers_Attempt_Question UNIQUE (ExamAttemptId, QuestionId)
);
GO

-- Các index dưới đây tăng tốc trang lịch sử, tìm theo môn và tải câu hỏi của đề.
CREATE INDEX IX_ExamAttempts_UserId_SubmittedAt ON dbo.ExamAttempts(UserId, SubmittedAt DESC);
CREATE INDEX IX_Exams_Subject_IsActive ON dbo.Exams(Subject, IsActive);
CREATE INDEX IX_Questions_ExamId ON dbo.Questions(ExamId);
CREATE INDEX IX_AnswerOptions_QuestionId ON dbo.AnswerOptions(QuestionId);
GO

/*
    TÀI KHOẢN MẪU:
    - admin / Admin@123
    - student / Student@123
    Hai mật khẩu đã được băm PBKDF2-SHA256 100.000 vòng đúng với PasswordHasher.cs.
*/
INSERT INTO dbo.Users (Username, PasswordHash, FullName, Email, Role, IsActive)
VALUES
(N'admin', N'100000.AQIDBAUGBwgJCgsMDQ4PEA==.7gQDaNbD2TJ9Tv/U3z+oOOw+byXCRpvOoV5EbjMrc1w=', N'Quản trị hệ thống', N'admin@exam.local', N'Admin', 1),
(N'student', N'100000.ERITFBUWFxgZGhscHR4fIA==.AZq0CpNa3hXb2trC+9/yC/9S+lzgDecbtXc4Fmht4ik=', N'Nguyễn Minh Anh', N'student@exam.local', N'Student', 1);
GO

-- Chèn ba đề thuộc ba môn để kiểm tra chức năng tìm kiếm và lọc.
INSERT INTO dbo.Exams (Title, Subject, Description, DurationMinutes, IsActive)
VALUES
(N'Toán cơ bản - Đề 01', N'Toán học', N'Ôn tập phép tính, phần trăm và hình học cơ bản.', 10, 1),
(N'Tiếng Anh giao tiếp - Đề 01', N'Tiếng Anh', N'Kiểm tra từ vựng và ngữ pháp tiếng Anh trình độ cơ bản.', 10, 1),
(N'Tin học nhập môn - Đề 01', N'Tin học', N'Kiến thức nền tảng về máy tính, lập trình và cơ sở dữ liệu.', 12, 1);
GO

-- Lưu Id của từng đề để phần chèn câu hỏi không phụ thuộc số identity cụ thể.
DECLARE @MathExamId INT = (SELECT Id FROM dbo.Exams WHERE Title = N'Toán cơ bản - Đề 01');
DECLARE @EnglishExamId INT = (SELECT Id FROM dbo.Exams WHERE Title = N'Tiếng Anh giao tiếp - Đề 01');
DECLARE @ITExamId INT = (SELECT Id FROM dbo.Exams WHERE Title = N'Tin học nhập môn - Đề 01');

-- Chèn 5 câu hỏi Toán học.
INSERT INTO dbo.Questions (ExamId, Content, OrderNumber) VALUES
(@MathExamId, N'Kết quả của 15 + 27 là bao nhiêu?', 1),
(@MathExamId, N'25% của 200 bằng bao nhiêu?', 2),
(@MathExamId, N'Một hình vuông có cạnh 6 cm. Chu vi bằng bao nhiêu?', 3),
(@MathExamId, N'Phân số nào bằng 0,5?', 4),
(@MathExamId, N'Giá trị của 3² + 4² là bao nhiêu?', 5);

-- Chèn phương án cho từng câu Toán; cột thứ ba đánh dấu đáp án đúng.
INSERT INTO dbo.AnswerOptions (QuestionId, Content, IsCorrect)
SELECT q.Id, a.Content, a.IsCorrect
FROM dbo.Questions q
CROSS APPLY (VALUES
    (1, N'42', CAST(1 AS BIT)), (1, N'32', CAST(0 AS BIT)), (1, N'52', CAST(0 AS BIT)), (1, N'40', CAST(0 AS BIT)),
    (2, N'25', CAST(0 AS BIT)), (2, N'40', CAST(0 AS BIT)), (2, N'50', CAST(1 AS BIT)), (2, N'75', CAST(0 AS BIT)),
    (3, N'12 cm', CAST(0 AS BIT)), (3, N'24 cm', CAST(1 AS BIT)), (3, N'36 cm', CAST(0 AS BIT)), (3, N'18 cm', CAST(0 AS BIT)),
    (4, N'1/4', CAST(0 AS BIT)), (4, N'1/2', CAST(1 AS BIT)), (4, N'2/3', CAST(0 AS BIT)), (4, N'3/4', CAST(0 AS BIT)),
    (5, N'7', CAST(0 AS BIT)), (5, N'12', CAST(0 AS BIT)), (5, N'25', CAST(1 AS BIT)), (5, N'49', CAST(0 AS BIT))
) a(OrderNumber, Content, IsCorrect)
WHERE q.ExamId = @MathExamId AND q.OrderNumber = a.OrderNumber;

-- Chèn câu hỏi và phương án cho đề Tiếng Anh.
INSERT INTO dbo.Questions (ExamId, Content, OrderNumber) VALUES
(@EnglishExamId, N'Chọn dạng đúng: She ___ to school every day.', 1),
(@EnglishExamId, N'Từ nào có nghĩa là "thư viện"?', 2),
(@EnglishExamId, N'Đáp án phù hợp cho câu hỏi "How are you?" là gì?', 3),
(@EnglishExamId, N'Dạng quá khứ của động từ "go" là gì?', 4),
(@EnglishExamId, N'Chọn mạo từ đúng: I have ___ apple.', 5);

INSERT INTO dbo.AnswerOptions (QuestionId, Content, IsCorrect)
SELECT q.Id, a.Content, a.IsCorrect
FROM dbo.Questions q
CROSS APPLY (VALUES
    (1, N'go', CAST(0 AS BIT)), (1, N'goes', CAST(1 AS BIT)), (1, N'going', CAST(0 AS BIT)), (1, N'gone', CAST(0 AS BIT)),
    (2, N'Library', CAST(1 AS BIT)), (2, N'Hospital', CAST(0 AS BIT)), (2, N'Market', CAST(0 AS BIT)), (2, N'School', CAST(0 AS BIT)),
    (3, N'I am fine, thank you.', CAST(1 AS BIT)), (3, N'I am a student.', CAST(0 AS BIT)), (3, N'It is Monday.', CAST(0 AS BIT)), (3, N'At seven o''clock.', CAST(0 AS BIT)),
    (4, N'goed', CAST(0 AS BIT)), (4, N'gone', CAST(0 AS BIT)), (4, N'went', CAST(1 AS BIT)), (4, N'going', CAST(0 AS BIT)),
    (5, N'a', CAST(0 AS BIT)), (5, N'an', CAST(1 AS BIT)), (5, N'the', CAST(0 AS BIT)), (5, N'no article', CAST(0 AS BIT))
) a(OrderNumber, Content, IsCorrect)
WHERE q.ExamId = @EnglishExamId AND q.OrderNumber = a.OrderNumber;

-- Chèn câu hỏi và phương án cho đề Tin học.
INSERT INTO dbo.Questions (ExamId, Content, OrderNumber) VALUES
(@ITExamId, N'CPU là viết tắt của cụm từ nào?', 1),
(@ITExamId, N'Ngôn ngữ nào thường dùng để truy vấn cơ sở dữ liệu quan hệ?', 2),
(@ITExamId, N'Trong C#, kiểu dữ liệu nào lưu giá trị đúng/sai?', 3),
(@ITExamId, N'MVVM gồm Model, View và thành phần nào?', 4),
(@ITExamId, N'Khóa chính (Primary Key) có mục đích chính là gì?', 5);

INSERT INTO dbo.AnswerOptions (QuestionId, Content, IsCorrect)
SELECT q.Id, a.Content, a.IsCorrect
FROM dbo.Questions q
CROSS APPLY (VALUES
    (1, N'Central Processing Unit', CAST(1 AS BIT)), (1, N'Computer Personal Unit', CAST(0 AS BIT)), (1, N'Central Program Utility', CAST(0 AS BIT)), (1, N'Control Processing User', CAST(0 AS BIT)),
    (2, N'HTML', CAST(0 AS BIT)), (2, N'CSS', CAST(0 AS BIT)), (2, N'SQL', CAST(1 AS BIT)), (2, N'XML', CAST(0 AS BIT)),
    (3, N'int', CAST(0 AS BIT)), (3, N'string', CAST(0 AS BIT)), (3, N'bool', CAST(1 AS BIT)), (3, N'decimal', CAST(0 AS BIT)),
    (4, N'ViewManager', CAST(0 AS BIT)), (4, N'ViewModel', CAST(1 AS BIT)), (4, N'ValueModel', CAST(0 AS BIT)), (4, N'ViewModule', CAST(0 AS BIT)),
    (5, N'Định danh duy nhất mỗi dòng', CAST(1 AS BIT)), (5, N'Lưu mật khẩu', CAST(0 AS BIT)), (5, N'Tạo giao diện', CAST(0 AS BIT)), (5, N'Kết nối Internet', CAST(0 AS BIT))
) a(OrderNumber, Content, IsCorrect)
WHERE q.ExamId = @ITExamId AND q.OrderNumber = a.OrderNumber;
GO

-- Truy vấn kiểm tra nhanh số bản ghi vừa tạo.
SELECT N'Users' AS TableName, COUNT(*) AS TotalRows FROM dbo.Users
UNION ALL SELECT N'Exams', COUNT(*) FROM dbo.Exams
UNION ALL SELECT N'Questions', COUNT(*) FROM dbo.Questions
UNION ALL SELECT N'AnswerOptions', COUNT(*) FROM dbo.AnswerOptions;
GO
