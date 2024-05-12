DROP DATABASE IF EXISTS transcodb;
CREATE DATABASE transcodb;
USE transcodb;

CREATE TABLE company(
	company_name VARCHAR(100) PRIMARY KEY,
    address VARCHAR(100),
    money FLOAT
);

CREATE TABLE person (
	user_id INT AUTO_INCREMENT NOT NULL PRIMARY KEY,
    user_type ENUM('EMPLOYEE', 'CLIENT') NOT NULL,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(30) NOT NULL,
    phone VARCHAR(30) NOT NULL,
    email VARCHAR(50) NOT NULL,
    address VARCHAR(100) NOT NULL,
    birth_date DATE NOT NULL,
    password_hash VARCHAR(100),
    -- Employee's specific
    position VARCHAR(50),
    salary FLOAT,
    hire_date DATE,
    license_type VARCHAR(30),
);

CREATE TABLE auth_tokens(
    token VARCHAR(50) PRIMARY KEY,
    user_id INT NOT NULL,
    FOREIGN KEY (user_id) REFERENCES person(user_id)
);

CREATE TABLE vehicle (
	license_plate VARCHAR(30) NOT NULL PRIMARY KEY,
	brand VARCHAR(30) NOT NULL,
    model VARCHAR(30) NOT NULL,
    price FLOAT NOT NULL,
    vehicle_type ENUM('TRUCK', 'CAR', 'VAN'),
    -- Truck's specific
    volume INT,
    truck_type VARCHAR(30),
    -- Van's specific
    van_usage VARCHAR(30),
    -- Car's specific
    seats INT,
    CONSTRAINT check_seats CHECK (seats < 9)
);

CREATE TABLE orders (
	order_id INT AUTO_INCREMENT,
    client_id INT NOT NULL,
    driver_id INT NOT NULL,
    vehicle_id VARCHAR(30) NOT NULL,
    departure_date DATETIME NOT NULL,
    arrival_date DATETIME,
    departure_city VARCHAR(40) NOT NULL,
    arrival_city VARCHAR(40) NOT NULL,
    order_status ENUM('Pending', 'InProgress', 'Stuck', 'WaitingPayment', 'Closed') DEFAULT 'Pending',
	PRIMARY KEY(order_id),
    FOREIGN KEY (client_id) REFERENCES person(user_id),
    FOREIGN KEY (driver_id) REFERENCES person(user_id),
    FOREIGN KEY (vehicle_id) REFERENCES vehicle(license_plate)
);

INSERT INTO company VALUES ('company', '1 rue de la defense', 1000);

INSERT INTO person (user_type, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date) VALUES ('EMPLOYEE', 'Pierre', 'Dupont', '0692129501', 'pierre.dupont@tmail.com', '7 Avenue des Catalpas', '1977-10-27', 'Driver', '30000', '2020-03-16');
INSERT INTO person (user_type, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date) VALUES ('EMPLOYEE', 'Marc', 'Marque', '0629190801', 'marc.marque@tmail.com', '8 Avenue des Catalpas', '1978-10-21', 'Driver', '30000', '2020-04-19');
INSERT INTO person (user_type, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date) VALUES ('EMPLOYEE', 'Jean', 'Martin', '0692129501', 'jean.martin@tmail.com', '9 Avenue des Catalpas', '1979-9-20', 'Driver', '30000', '2020-04-16');



INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('EN-789-NL', 'Nissan', 'X-trail', 14000.0, 'CAR', 5);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('DZ-171-GT', 'Audi', 'TT', 7000.0, 'CAR', 4);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('FT-519-KG', 'Mercedes', 'Classe B', 40000.0, 'CAR', 5);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, volume, truck_type) VALUES ('ME-302-ZB', 'Mercedes', 'Actros', 150000.0, 'TRUCK', 500, 'TRANSPORT');

INSERT INTO orders (client_id, driver_id, vehicle_id, departure_date, departure_city, arrival_city) VALUES(1, 2, 'EN-789-NL', '2025-10-21 09:00:00', 'Toulouse', 'Paris');
INSERT INTO orders (client_id, driver_id, vehicle_id, departure_date, departure_city, arrival_city) VALUES(2, 3, 'FT-519-KG', '2025-11-21 09:00:00', 'Toulouse', 'Paris');

SELECT * FROM person;
SELECT * FROM vehicle;
SELECT * FROM company;
SELECT * FROM orders;

UPDATE company 
SET money = money + (-1000)
WHERE company_name = 'company';

SELECT departure_date, departure_city, arrival_city, order_id, driver_id, vehicle_id, vehicle_type
FROM orders
INNER JOIN vehicle ON orders.vehicle_id = vehicle.license_plate
;
SET @start = '2025-11-21 8:00:00';
SET @end = NULL;
SET @vehicle_type = 'CAR'
SELECT user_id, user_type, first_name, last_name, phone, email, address, birth_date, position, salary, hire_date FROM person WHERE user_id NOT IN (SELECT driver_id FROM orders WHERE departure_date BETWEEN @start AND @end OR arrival_date BETWEEN @start AND @end OR DATE(departure_date) = DATE(@start) OR DATE(arrival_date) = DATE(@end)) ORDER BY hire_date DESC;
SELECT license_plate, brand, model FROM vehicle WHERE vehicle_type = @vehicle_type AND license_plate NOT IN (SELECT vehicle_id FROM orders WHERE departure_date BETWEEN @start AND @end OR arrival_date BETWEEN @start AND @end OR DATE(departure_date) BETWEEN DATE(@start) AND DATE(@end) OR DATE(arrival_date) BETWEEN DATE(@start) AND DATE(@end));