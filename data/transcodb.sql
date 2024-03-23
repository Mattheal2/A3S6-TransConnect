CREATE DATABASE transcodb;
USE transcodb;

CREATE TABLE personne (
	user_id VARCHAR(30)  PRIMARY KEY,
    first_name VARCHAR(30),
    last_name VARCHAR(30),
    phone VARCHAR(30),
    email VARCHAR(50),
    address VARCHAR(100),
    birth_date DATETIME,
    #Employee's specific
    position VARCHAR(50),
    salary INT,
    hire_date DATE,
    #Driver's specific
    license_type ENUM('CAR', 'TRUCK')
);

CREATE TABLE vehicle (
	vehicle_id VARCHAR(30) PRIMARY KEY,
	brand VARCHAR(30),
    model VARCHAR(30),
    licence_plate VARCHAR(30),
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
	order_id VARCHAR(30) PRIMARY KEY,
    FOREIGN KEY (client_id) REFERENCES personne(user_id),
    FOREIGN KEY (driver_id) REFERENCES personne(user_id),
    FOREIGN KEY (vehicle_id) REFERENCES vehicle(vehicle_id),
    departure_date DATETIME,
    departure_city VARCHAR(40),
    arrival_city VARCHAR(40),
    order_status ENUM('Pending', 'InProgress', 'Stuck', 'WaitingPayment', 'Closed')    
);