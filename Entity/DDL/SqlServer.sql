﻿CREATE TABLE Person (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(20) NOT NULL,
    LastName NVARCHAR(20) NOT NULL,
    Email NVARCHAR(29) NOT NULL
);

CREATE TABLE "User" (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserName NVARCHAR(20) NOT NULL,
      PasswordHash NVARCHAR(4000) NULL,  -- Aquí se agrega la columna PasswordHash
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Active BIT NOT NULL DEFAULT 1,
    CreateAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    DeleteAt DATETIME2 NULL,
    PersonId INT NOT NULL,
    FOREIGN KEY (PersonId) REFERENCES Person(Id) ON DELETE CASCADE
);

CREATE TABLE Rol (
    Id INT PRIMARY KEY IDENTITY(1,1),
    "Description" NVARCHAR(100) NOT NULL,
    "Name" NVARCHAR(20) NOT NULL,
    Active BIT NOT NULL DEFAULT 1,
    CreateAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    DeleteAt DATETIME2 NULL
);

CREATE TABLE Permission (
    Id INT PRIMARY KEY IDENTITY(1,1),
    "Name" NVARCHAR(20) NOT NULL,
    Active BIT NOT NULL DEFAULT 1,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    CreateAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    DeleteAt DATETIME2 NULL
);

CREATE TABLE Form (
    Id INT PRIMARY KEY IDENTITY(1,1),
    "Name" NVARCHAR(20) NOT NULL,
    Code NVARCHAR(50) NOT NULL UNIQUE,
    Active BIT NOT NULL DEFAULT 1,
    CreateAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    DeleteAt DATETIME2 NULL
);

CREATE TABLE Module (
    Id INT PRIMARY KEY IDENTITY(1,1),
    "Name" NVARCHAR(20) NOT NULL,
    Active BIT NOT NULL DEFAULT 1,
    CreateAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    DeleteAt DATETIME2 NULL
);

CREATE TABLE RolUser (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RolId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES "User"(Id) ON DELETE CASCADE,
    FOREIGN KEY (RolId) REFERENCES Rol(Id) ON DELETE CASCADE
);

CREATE TABLE RoleFormPermission (
    Id INT PRIMARY KEY IDENTITY(1,1),
    RolId INT NOT NULL,
    PermissionId INT NOT NULL,
    FormId INT NOT NULL,
    FOREIGN KEY (RolId) REFERENCES Rol(Id) ON DELETE CASCADE,
    FOREIGN KEY (PermissionId) REFERENCES Permission(Id) ON DELETE CASCADE,
    FOREIGN KEY (FormId) REFERENCES Form(Id) ON DELETE CASCADE
);

CREATE TABLE FormModule (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ModuleId INT NOT NULL,
    FormId INT NOT NULL,
    FOREIGN KEY (ModuleId) REFERENCES Module(Id) ON DELETE CASCADE,
    FOREIGN KEY (FormId) REFERENCES Form(Id) ON DELETE CASCADE
);
