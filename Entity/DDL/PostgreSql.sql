CREATE TABLE "User" (
    id SERIAL PRIMARY KEY,
    username VARCHAR(255) NOT NULL,
    code VARCHAR(100) NOT NULL,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    create_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    delete_at TIMESTAMP,
    person_id INT NOT NULL,
    CONSTRAINT fk_user_person FOREIGN KEY (person_id) REFERENCES person(id)
);

CREATE TABLE person (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL
);

CREATE TABLE rol (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    create_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    delete_at TIMESTAMP
);

CREATE TABLE rol_user (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL,
    rol_id INT NOT NULL,
    CONSTRAINT fk_roluser_user FOREIGN KEY (user_id) REFERENCES "User"(id),
    CONSTRAINT fk_roluser_rol FOREIGN KEY (rol_id) REFERENCES rol(id)
);

CREATE TABLE module (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    create_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    delete_at TIMESTAMP
);

CREATE TABLE form (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    code VARCHAR(100) NOT NULL,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    create_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    delete_at TIMESTAMP
);

CREATE TABLE form_module (
    id SERIAL PRIMARY KEY,
    module_id INT NOT NULL,
    form_id INT NOT NULL,
    CONSTRAINT fk_formmodule_module FOREIGN KEY (module_id) REFERENCES module(id),
    CONSTRAINT fk_formmodule_form FOREIGN KEY (form_id) REFERENCES form(id)
);

CREATE TABLE permission (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    active BOOLEAN NOT NULL DEFAULT TRUE,
    code VARCHAR(100) NOT NULL,
    create_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    delete_at TIMESTAMP
);

CREATE TABLE rol_form_permission (
    id SERIAL PRIMARY KEY,
    rol_id INT NOT NULL,
    permission_id INT NOT NULL,
    form_id INT NOT NULL,
    CONSTRAINT fk_rolformpermission_rol FOREIGN KEY (rol_id) REFERENCES rol(id),
    CONSTRAINT fk_rolformpermission_permission FOREIGN KEY (permission_id) REFERENCES permission(id),
    CONSTRAINT fk_rolformpermission_form FOREIGN KEY (form_id) REFERENCES form(id)
);

CREATE TABLE changelog (
    id SERIAL PRIMARY KEY,
    table_id INT NOT NULL,
    user_id INT NOT NULL,
    date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_changelog_user FOREIGN KEY (user_id) REFERENCES "User"(id)
);
