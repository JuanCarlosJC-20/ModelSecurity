-- ========================== 
-- TABLA PERSON 
-- ========================== 
CREATE TABLE Person (
    Id SERIAL PRIMARY KEY,
    FirstName VARCHAR(20) NOT NULL,
    LastName VARCHAR(20) NOT NULL,
    Email VARCHAR(50) NOT NULL
);

-- ========================== 
-- TABLA USER 
-- ========================== 
CREATE TABLE "User" (
    Id SERIAL PRIMARY KEY,
    UserName VARCHAR(20) NOT NULL,
    PasswordHash VARCHAR(4000),
    Code VARCHAR(50) NOT NULL UNIQUE,
    Active BOOLEAN NOT NULL DEFAULT TRUE,
    CreateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeleteAt TIMESTAMP NULL,
    PersonId INTEGER NOT NULL,
    FOREIGN KEY (PersonId) REFERENCES Person(Id) ON DELETE CASCADE
);

-- ========================== 
-- TABLA ROL 
-- ========================== 
CREATE TABLE Rol (
    Id SERIAL PRIMARY KEY,
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
    Id SERIAL PRIMARY KEY,
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
    Id SERIAL PRIMARY KEY,
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
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
    Active BOOLEAN NOT NULL DEFAULT TRUE,
    CreateAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    DeleteAt TIMESTAMP NULL
);

-- ========================== 
-- TABLA ROLUSER 
-- ========================== 
CREATE TABLE RolUser (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER NOT NULL,
    RolId INTEGER NOT NULL,
    FOREIGN KEY (UserId) REFERENCES "User"(Id) ON DELETE CASCADE,
    FOREIGN KEY (RolId) REFERENCES Rol(Id) ON DELETE CASCADE
);

-- ========================== 
-- TABLA ROLEFORMPERMISSION 
-- ========================== 
CREATE TABLE RoleFormPermission (
    Id SERIAL PRIMARY KEY,
    RolId INTEGER NOT NULL,
    PermissionId INTEGER NOT NULL,
    FormId INTEGER NOT NULL,
    FOREIGN KEY (RolId) REFERENCES Rol(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permission(Id) ON DELETE CASCADE,
    FOREIGN KEY (FormId) REFERENCES Form(Id) ON DELETE CASCADE
);

-- ========================== 
-- TABLA FORMMODULE 
-- ========================== 
CREATE TABLE FormModule (
    Id SERIAL PRIMARY KEY,
    ModuleId INTEGER NOT NULL,
    FormId INTEGER NOT NULL,
    FOREIGN KEY (ModuleId) REFERENCES Module(Id) ON DELETE CASCADE,
    FOREIGN KEY (FormId) REFERENCES Form(Id) ON DELETE CASCADE
);

-- ========================== 
-- DATOS INICIALES DE ROL 
-- ========================== 
INSERT INTO Rol (Description, Name) VALUES ('Rol Administrador', 'Admin');
INSERT INTO Rol (Description, Name) VALUES ('Rol Usuario', 'User');