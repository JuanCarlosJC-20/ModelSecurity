-- ========================== 
-- TABLA PERSON 
-- ========================== 
CREATE TABLE Person (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    FirstName VARCHAR(20) NOT NULL,
    LastName VARCHAR(20) NOT NULL,
    Email VARCHAR(50) NOT NULL
);

-- ========================== 
-- TABLA USER 
-- ========================== 
CREATE TABLE `User` (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserName VARCHAR(20) NOT NULL,
    PasswordHash VARCHAR(4000) NULL,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Active BOOLEAN NOT NULL DEFAULT TRUE,
    CreateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeleteAt TIMESTAMP NULL,
    PersonId INT NOT NULL,
    FOREIGN KEY (PersonId) REFERENCES Person(Id) ON DELETE CASCADE
);

-- ========================== 
-- TABLA ROL 
-- ========================== 
CREATE TABLE Rol (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Description VARCHAR(100) NOT NULL,
    Name VARCHAR(20) NOT NULL UNIQUE,
    Active BOOLEAN NOT NULL DEFAULT TRUE,
    CreateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeleteAt TIMESTAMP NULL
);

-- ========================== 
-- TABLA PERMISSION 
-- ========================== 
CREATE TABLE Permission (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(50) NOT NULL,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Active BOOLEAN NOT NULL DEFAULT TRUE,
    CreateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeleteAt TIMESTAMP NULL
);

-- ========================== 
-- TABLA FORM 
-- ========================== 
CREATE TABLE Form (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(50) NOT NULL,
    Code VARCHAR(50) NOT NULL UNIQUE,
    Active BOOLEAN NOT NULL DEFAULT TRUE,
    CreateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeleteAt TIMESTAMP NULL
);

-- ========================== 
-- TABLA MODULE 
-- ========================== 
CREATE TABLE Module (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(50) NOT NULL,
    Active BOOLEAN NOT NULL DEFAULT TRUE,
    CreateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeleteAt TIMESTAMP NULL
);

-- ========================== 
-- TABLA ROLUSER 
-- ========================== 
CREATE TABLE RolUser (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    RolId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES `User`(Id) ON DELETE CASCADE,
    FOREIGN KEY (RolId) REFERENCES Rol(Id) ON DELETE CASCADE
);

-- ========================== 
-- TABLA ROLEFORMPERMISSION 
-- ========================== 
CREATE TABLE RoleFormPermission (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    RolId INT NOT NULL,
    PermissionId INT NOT NULL,
    FormId INT NOT NULL,
    FOREIGN KEY (RolId) REFERENCES Rol(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permission(Id) ON DELETE CASCADE,
    FOREIGN KEY (FormId) REFERENCES Form(Id) ON DELETE CASCADE
);

-- ========================== 
-- TABLA FORMMODULE 
-- ========================== 
CREATE TABLE FormModule (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    ModuleId INT NOT NULL,
    FormId INT NOT NULL,
    FOREIGN KEY (ModuleId) REFERENCES Module(Id) ON DELETE CASCADE,
    FOREIGN KEY (FormId) REFERENCES Form(Id) ON DELETE CASCADE
);

-- ========================== 
-- DATOS INICIALES DE ROL 
-- ========================== 
INSERT INTO Rol (Description, Name) VALUES ('Rol Administrador', 'Admin');
INSERT INTO Rol (Description, Name) VALUES ('Rol Usuario', 'User');