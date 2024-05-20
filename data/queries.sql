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
SET @departure_time = 1742373192;
SET @arrival_time = 1742396793;

SELECT user_id FROM person
WHERE user_id not IN (
SELECT driver_id 
FROM orders
WHERE (@departure_time < arrival_date AND @arrival_time > departure_date)
) AND user_type = 'EMPLOYEE'
ORDER BY RAND()
LIMIT 1;

SELECT driver_id 
FROM orders
WHERE (@departure_time < arrival_date AND departure_date < @arrival_time);

SELECT license_plate FROM vehicle
WHERE license_plate not IN (
	SELECT vehicle_id 
	FROM orders
	WHERE (@departure_time < arrival_date AND @arrival_time > departure_date)
) 
AND vehicle_type = 'CAR'
ORDER BY RAND()
LIMIT 1;

SELECT * FROM person WHERE user_id = '5' OR '1' = '1'