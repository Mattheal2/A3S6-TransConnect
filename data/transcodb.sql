DROP DATABASE IF EXISTS transcodb;
CREATE DATABASE transcodb;
USE transcodb;

CREATE TABLE company(
	company_name VARCHAR(100) PRIMARY KEY,
    address VARCHAR(100),
    money FLOAT
);

CREATE TABLE person (
	user_id VARCHAR(30) NOT NULL PRIMARY KEY,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(30) NOT NULL,
    phone VARCHAR(30) NOT NULL,
    email VARCHAR(50) NOT NULL,
    address VARCHAR(100) NOT NULL,
    birth_date DATE NOT NULL,
    #Employee's specific
    position VARCHAR(50),
    salary FLOAT,
    hire_date DATE,
    #Driver's specific
    license_type ENUM('CAR', 'TRUCK')
);

CREATE TABLE vehicle (
	license_plate VARCHAR(30) NOT NULL PRIMARY KEY,
	brand VARCHAR(30) NOT NULL,
    model VARCHAR(30) NOT NULL,
    price FLOAT NOT NULL,
    vehicle_type ENUM('TRUCK', 'CAR', 'VAN'),
    #Truck's specific
    volume INT,
    truck_type VARCHAR(30),
    #Van's specific
    van_usage VARCHAR(30),
    #Car's specific
    seats INT,
    CONSTRAINT check_seats CHECK (seats < 9)
);

CREATE TABLE orders (
	order_id VARCHAR(30) NOT NULL PRIMARY KEY,
    client_id VARCHAR(30) NOT NULL,
    driver_id VARCHAR(30) NOT NULL,
    vehicle_id VARCHAR(30) NOT NULL,
    departure_date DATE NOT NULL,
    departure_city VARCHAR(40) NOT NULL,
    arrival_city VARCHAR(40) NOT NULL,
    order_status ENUM('Pending', 'InProgress', 'Stuck', 'WaitingPayment', 'Closed') DEFAULT 'Pending',
	FOREIGN KEY (client_id) REFERENCES person(user_id),
    FOREIGN KEY (driver_id) REFERENCES person(user_id),
    FOREIGN KEY (vehicle_id) REFERENCES vehicle(license_plate)
);

INSERT INTO company VALUES ('company', '1 rue de la defense', 1000);

INSERT INTO person (user_id, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date, license_type) VALUES ('E0', 'Pierre', 'Dupont', '0692129501', 'pierre.dupont@tmail.com', '7 Avenue des Catalpas', '1977-10-21', 'Driver', '30000', '2020-04-16', 'CAR');
INSERT INTO person (user_id, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date, license_type) VALUES ('E1', 'Marc', 'Marque', '0629190801', 'marc.marque@tmail.com', '8 Avenue des Catalpas', '1978-10-21', 'Driver', '30000', '2020-04-16', 'CAR');
INSERT INTO person (user_id, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date, license_type) VALUES ('E2', 'Jean', 'Martin', '0692129501', 'jean.martin@tmail.com', '9 Avenue des Catalpas', '1979-10-21', 'Driver', '30000', '2020-04-16', 'CAR');



INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('EN-789-NL', 'Nissan', 'X-trail', 14000.0, 'CAR', 5);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('DZ-171-GT', 'Audi', 'TT', 7000.0, 'CAR', 4);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('FT-519-KG', 'Mercedes', 'Classe B', 40000.0, 'CAR', 5);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, volume, truck_type) VALUES ('ME-302-ZB', 'Mercedes', 'Actros', 150000.0, 'TRUCK', 500, 'TRANSPORT');

SELECT * FROM person;
SELECT * FROM vehicle;
SELECT * FROM company;

UPDATE company 
SET money = money + (-1000)
WHERE company_name = 'company';

