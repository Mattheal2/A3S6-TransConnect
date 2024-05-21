DELETE FROM orders WHERE order_id > 2;
SELECT * FROM orders;
SELECT * FROM vehicle;
SELECT * FROM person;
SELECT * FROM auth_tokens;
INSERT INTO auth_tokens VALUES('xxx', 1);
SET @token_id = 'xxx';
SELECT * FROM auth_tokens NATURAL JOIN person WHERE token_id = @token_id LIMIT 1;

SELECT * FROM person WHERE user_type = 'EMPLOYEE' ORDER BY RAND() LIMIT 1;

-- Find driver
SET @departure_time = 0; -- 1742373192;
SET @arrival_time = 111742396793;

SELECT user_id FROM person
WHERE user_id not IN (
SELECT driver_id 
FROM orders
WHERE (@departure_time < arrival_time AND @arrival_time > departure_time)
) AND user_type = 'EMPLOYEE'
ORDER BY RAND()
LIMIT 1;

SELECT driver_id 
FROM orders
WHERE (@departure_time < arrival_time AND departure_time < @arrival_time);

SELECT license_plate FROM vehicle
WHERE license_plate not IN (
	SELECT vehicle_id 
	FROM orders
	WHERE (@departure_time < arrival_time AND @arrival_time > departure_time)
) 
AND vehicle_type = 'CAR'
ORDER BY RAND()
LIMIT 1;

SELECT *
FROM orders
WHERE departure_time >= @departure_time AND departure_time <= @arrival_time;

SET @user_id = 3;
SET @_start = 1742373190;
SET @_end = 1742373199;
SELECT departure_time, arrival_time, departure_city, arrival_city FROM orders
LEFT JOIN person ON orders.driver_id = person.user_id
WHERE person.user_id = @user_id 
	AND person.user_type = "EMPLOYEE" 
    AND LOWER(person.position) = 'driver'
    AND (orders.departure_time BETWEEN @_start AND @_end)
;