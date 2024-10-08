DROP DATABASE IF EXISTS transcodb;
CREATE DATABASE transcodb;
USE transcodb;

CREATE TABLE person (
	user_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
    user_type ENUM('EMPLOYEE', 'CLIENT') NOT NULL,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(30) NOT NULL,
    phone VARCHAR(30) NOT NULL,
    email VARCHAR(50) NOT NULL,
    address VARCHAR(100) NOT NULL,
    city VARCHAR(30) NOT NULL,
    birth_time LONG NOT NULL,
    password_hash VARCHAR(100),
    -- Employee's specific
    position VARCHAR(50),
    salary FLOAT,
    hire_time LONG,
    license_type VARCHAR(30),
    supervisor_id INT,
    show_on_org_chart BOOLEAN,
    -- Clients
    total_spent BIGINT DEFAULT 0
);

CREATE TABLE auth_tokens(
    token_id VARCHAR(50) PRIMARY KEY,
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
    departure_time LONG NOT NULL,
    arrival_time LONG NOT NULL,
    departure_city VARCHAR(40) NOT NULL,
    arrival_city VARCHAR(40) NOT NULL,
    price_per_km INT DEFAULT 80,
    total_price INT,
    order_status ENUM('Pending', 'InProgress', 'Stuck', 'WaitingPayment', 'Closed') DEFAULT 'Pending',
	PRIMARY KEY(order_id),
    FOREIGN KEY (client_id) REFERENCES person(user_id),
    FOREIGN KEY (driver_id) REFERENCES person(user_id),
    FOREIGN KEY (vehicle_id) REFERENCES vehicle(license_plate)
);


INSERT INTO person (user_type, first_name, last_name, phone, email, address, city, birth_time, password_hash, position, salary, hire_time, license_type, supervisor_id, show_on_org_chart) VALUES ('EMPLOYEE', 'Pierre', 'Dupont', '0692129501', 'pierre.dupont@tmail.com', '7 Avenue des Catalpas', 'Puteaux', '232878792', 'ASpyssxGlsfZVDfTtd+dsIqAyfcTEQL7HJQdtwPMT0aPOJJZiHEmNRBaLSTw12eQaw==', 'Manager', '30000', '1716102592', 'B', 3, FALSE);
INSERT INTO person (user_type, first_name, last_name, phone, email, address, city, birth_time, password_hash, position, salary, hire_time, license_type, supervisor_id, show_on_org_chart) VALUES ('EMPLOYEE', 'Marc', 'Marque', '0629190801', 'marc.marque@tmail.com', '8 Avenue des Catalpas', 'Paris', '230286792', 'ASpyssxGlsfZVDfTtd+dsIqAyfcTEQL7HJQdtwPMT0aPOJJZiHEmNRBaLSTw12eQaw==', 'Conducteur', '30000', '1716100592', 'B', 3, FALSE);
INSERT INTO person (user_type, first_name, last_name, phone, email, address, city, birth_time, password_hash, position, salary, hire_time, license_type, supervisor_id, show_on_org_chart) VALUES ('EMPLOYEE', 'Jean', 'Martin', '0692129501', 'jean.martin@tmail.com', '9 Avenue des Catalpas', 'Pibrac', '227608392', 'ASpyssxGlsfZVDfTtd+dsIqAyfcTEQL7HJQdtwPMT0aPOJJZiHEmNRBaLSTw12eQaw==', 'Conducteur', '30000', '1716107512', 'B', 3, FALSE);

INSERT INTO person (user_type, first_name, last_name, phone, email, address, city, birth_time, password_hash) VALUES ('CLIENT', 'Paul', 'Pan', '0100231311', 'paul@pan.fr', '1 rue de pole', 'Thouary', 232878794, 'ASpyssxGlsfZVDfTtd+dsIqAyfcTEQL7HJQdtwPMT0aPOJJZiHEmNRBaLSTw12eQaw==');

INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('EN-789-NL', 'Nissan', 'X-trail', 14000.0, 'CAR', 5);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('DZ-171-GT', 'Audi', 'TT', 7000.0, 'CAR', 4);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, seats) VALUES ('FT-519-KG', 'Mercedes', 'Classe B', 40000.0, 'CAR', 5);
INSERT INTO vehicle (license_plate, brand, model, price, vehicle_type, volume, truck_type) VALUES ('ME-302-ZB', 'Mercedes', 'Actros', 150000.0, 'TRUCK', 500, 'remorque');

INSERT INTO orders (client_id, driver_id, vehicle_id, departure_time, arrival_time, departure_city, arrival_city, price_per_km, total_price, order_status) VALUES(1, 2, 'EN-789-NL', 1742373192, 1742376792, 'Colombes', 'Paris', 80, 800, 'InProgress');
INSERT INTO orders (client_id, driver_id, vehicle_id, departure_time, arrival_time, departure_city, arrival_city, price_per_km, total_price, order_status) VALUES(2, 3, 'FT-519-KG', 1742373192, 1742376792, 'Nanterre', 'Noisy-le-Grand', 80, 800, 'InProgress');
INSERT INTO orders (client_id, driver_id, vehicle_id, departure_time, arrival_time, departure_city, arrival_city, price_per_km, total_price, order_status) VALUES(2, 3, 'FT-519-KG', 11742373192, 11742376792, 'Créteil', 'Saint-Denis', 80, 900, 'InProgress');


INSERT INTO auth_tokens VALUES('xxx', 1);

DELIMITER //

CREATE TRIGGER after_order_insert
AFTER INSERT ON orders
FOR EACH ROW
BEGIN
    UPDATE person
    SET total_spent = total_spent + NEW.total_price
    WHERE user_id = NEW.client_id;
END;
//

DELIMITER ;

DELIMITER //

CREATE TRIGGER after_order_delete
AFTER DELETE ON orders
FOR EACH ROW
BEGIN
    UPDATE person
    SET total_spent = total_spent - OLD.total_price
    WHERE user_id = OLD.client_id;
END;
//

DELIMITER ;

SELECT COUNT(orders.order_id) AS total
FROM orders;