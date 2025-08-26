USE ModelSecurityMy;

INSERT INTO Rol (Name, Description, Active, CreateAt)
VALUES 
    ('Admin', 'Rol Administrador', true, UTC_TIMESTAMP()),
    ('User', 'Rol Usuario', true, UTC_TIMESTAMP());

SELECT Id, Name, Description, Active FROM Rol;