-- Create the database
CREATE DATABASE ActivitiesDb;
GO

USE ActivitiesDb;
GO

-- Create the Activities base table
CREATE TABLE Activities (
    ActivityID INT IDENTITY(1,1) PRIMARY KEY,
    DateStartTime DATETIME NOT NULL UNIQUE, -- Enforces one activity per day
    Title NVARCHAR(50) NOT NULL CHECK (LEN(Title) >= 3),
    Cost DECIMAL(10, 2) NOT NULL CHECK (Cost >= 0)
);
GO

-- Create the EntertainmentActivities table
CREATE TABLE EntertainmentActivities (
    ActivityID INT PRIMARY KEY,
    MinParticipants INT NOT NULL CHECK (MinParticipants >= 2),
    FOREIGN KEY (ActivityID) REFERENCES Activities(ActivityID)
);
GO

-- Create the FitnessActivities table
CREATE TABLE FitnessActivities (
    ActivityID INT PRIMARY KEY,
    Location NVARCHAR(100) NOT NULL,
    FOREIGN KEY (ActivityID) REFERENCES Activities(ActivityID)
);
GO

--- Stored procedure to check whether an activity already exists on a given date
CREATE PROCEDURE CheckActivityExistsByDate
    @DateToCheck DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM Activities
        WHERE CAST(DateStartTime AS DATE) = @DateToCheck
    )
    BEGIN
        SELECT 1 AS ActivityExists;
    END
    ELSE
    BEGIN
        SELECT 0 AS ActivityExists;
    END
END;
GO

---- Stored procedure to add a new fitness activity
CREATE PROCEDURE AddFitnessActivity
    @DateStartTime DATETIME,
    @Title NVARCHAR(50),
    @Cost DECIMAL(10, 2),
    @Location NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Activities table
        INSERT INTO Activities (DateStartTime, Title, Cost)
        VALUES (@DateStartTime, @Title, @Cost);

        DECLARE @ActivityID INT = SCOPE_IDENTITY();

        -- Insert into FitnessActivities table
        INSERT INTO FitnessActivities (ActivityID, Location)
        VALUES (@ActivityID, @Location);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Re-throw the error for the caller to handle
        THROW;
    END CATCH
END;
GO


---- Stored procedure to add a new entertainment activity
CREATE PROCEDURE AddEntertainmentActivity
    @DateStartTime DATETIME,
    @Title NVARCHAR(50),
    @Cost DECIMAL(10, 2),
    @MinParticipants INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Activities table
        INSERT INTO Activities (DateStartTime, Title, Cost)
        VALUES (@DateStartTime, @Title, @Cost);

        DECLARE @ActivityID INT = SCOPE_IDENTITY();

        -- Insert into EntertainmentActivities table
        INSERT INTO EntertainmentActivities (ActivityID, MinParticipants)
        VALUES (@ActivityID, @MinParticipants);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        -- Re-throw the error for the caller to handle
        THROW;
    END CATCH
END;
GO


---- Stored procedure to update an activity
CREATE PROCEDURE UpdateActivityWithTypeCheck
    @ActivityID INT,
    @NewDateStartTime DATETIME,
    @NewTitle NVARCHAR(50),
    @NewCost DECIMAL(10, 2),
    @NewType NVARCHAR(20), -- 'Fitness' or 'Entertainment'
    @Location NVARCHAR(100) = NULL,
    @MinParticipants INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Get current type
        DECLARE @CurrentType NVARCHAR(20);
        SELECT @CurrentType = 
            CASE 
                WHEN EXISTS (SELECT 1 FROM FitnessActivities WHERE ActivityID = @ActivityID) THEN 'Fitness'
                WHEN EXISTS (SELECT 1 FROM EntertainmentActivities WHERE ActivityID = @ActivityID) THEN 'Entertainment'
                ELSE NULL
            END;

        -- Update Activities table
        UPDATE Activities
        SET DateStartTime = @NewDateStartTime,
            Title = @NewTitle,
            Cost = @NewCost
        WHERE ActivityID = @ActivityID;

        -- If type has changed, delete from old table
        IF @CurrentType IS NOT NULL AND @CurrentType <> @NewType
        BEGIN
            IF @CurrentType = 'Fitness'
                DELETE FROM FitnessActivities WHERE ActivityID = @ActivityID;
            ELSE IF @CurrentType = 'Entertainment'
                DELETE FROM EntertainmentActivities WHERE ActivityID = @ActivityID;
        END

        -- Insert or update into the correct table
        IF @NewType = 'Fitness'
        BEGIN
            IF EXISTS (SELECT 1 FROM FitnessActivities WHERE ActivityID = @ActivityID)
                UPDATE FitnessActivities SET Location = @Location WHERE ActivityID = @ActivityID;
            ELSE
                INSERT INTO FitnessActivities (ActivityID, Location) VALUES (@ActivityID, @Location);
        END
        ELSE IF @NewType = 'Entertainment'
        BEGIN
            IF EXISTS (SELECT 1 FROM EntertainmentActivities WHERE ActivityID = @ActivityID)
                UPDATE EntertainmentActivities SET MinParticipants = @MinParticipants WHERE ActivityID = @ActivityID;
            ELSE
                INSERT INTO EntertainmentActivities (ActivityID, MinParticipants) VALUES (@ActivityID, @MinParticipants);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO

---  Stored procedure to retrieve all fitness activities
CREATE PROCEDURE GetAllFitnessActivities
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        A.ActivityID,
        A.DateStartTime,
        A.Title,
        A.Cost,
        F.Location
    FROM Activities A
    INNER JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
    ORDER BY A.DateStartTime;
END;
GO

---  Stored procedure to retrieve all entertainment activities
CREATE PROCEDURE GetAllEntertainmentActivities
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        A.ActivityID,
        A.DateStartTime,
        A.Title,
        A.Cost,
        E.MinParticipants
    FROM Activities A
    INNER JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
    ORDER BY A.DateStartTime;
END;
GO

---  Stored procedure to retrieve all activities
CREATE PROCEDURE GetAllActivities
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        A.ActivityID,
        A.DateStartTime,
        A.Title,
        A.Cost,
        CASE 
            WHEN F.ActivityID IS NOT NULL THEN 'Fitness'
            WHEN E.ActivityID IS NOT NULL THEN 'Entertainment'
            ELSE 'Unknown'
        END AS ActivityType,
        F.Location,
        E.MinParticipants
    FROM Activities A
    LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
    LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
    ORDER BY A.DateStartTime;
END;
GO


---- Stored procedure to search activities by date
CREATE PROCEDURE SearchActivitiesByDate
    @SearchDate DATE,
    @Operator NVARCHAR(10) -- 'before', 'on', 'after'
AS
BEGIN
    SET NOCOUNT ON;

    IF @Operator = 'before'
        SELECT 
            A.ActivityID,
            A.DateStartTime,
            A.Title,
            A.Cost,
            F.Location,
            E.MinParticipants
        FROM Activities A
        LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
        LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
        WHERE CAST(A.DateStartTime AS DATE) < @SearchDate;
    ELSE IF @Operator = 'on'
        SELECT 
            A.ActivityID,
            A.DateStartTime,
            A.Title,
            A.Cost,
            F.Location,
            E.MinParticipants
        FROM Activities A
        LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
        LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
        WHERE CAST(A.DateStartTime AS DATE) = @SearchDate;
    ELSE IF @Operator = 'after'
        SELECT 
            A.ActivityID,
            A.DateStartTime,
            A.Title,
            A.Cost,
            F.Location,
            E.MinParticipants
        FROM Activities A
        LEFT JOIN FitnessActivities F ON A.ActivityID = F.ActivityID
        LEFT JOIN EntertainmentActivities E ON A.ActivityID = E.ActivityID
        WHERE CAST(A.DateStartTime AS DATE) > @SearchDate;
    ELSE
        RAISERROR('Invalid operator. Use "before", "on", or "after".', 16, 1);
END;
GO


-- 01/03/2025 14:00:00, Walking, 0, 30, Petersham Park
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-01 14:00:00', 'Walking', 0);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Petersham Park');

-- 02/03/2025 09:00:00, Running, 0, 30, Sydney Park
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-02 09:00:00', 'Running', 0);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Sydney Park');

-- 08/03/2025 10:15:00, Cycling, 0, 30, Bicentennial Park
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-08 10:15:00', 'Cycling', 0);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Bicentennial Park');

-- 09/03/2025 08:00:00, Yoga, 10, 60, Bondi Beach
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-09 08:00:00', 'Yoga', 10);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Bondi Beach');

-- 22/03/2025 09:00:00, Swimming, 0, 45, Bondi Beach
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-22 09:00:00', 'Swimming', 0);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Bondi Beach');

-- 23/03/2025 09:00:00, Swimming, 10, 120, Sydney Olympic Park Aquatic Centre
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-23 09:00:00', 'Swimming', 10);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Sydney Olympic Park Aquatic Centre');

-- 25/03/2025 18:00:00, Walking, 0, 30, Petersham Park
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-25 18:00:00', 'Walking', 0);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Petersham Park');

-- 29/03/2025 10:30:00, Walking, 0, 40, Sydney Park
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-29 10:30:00', 'Walking', 0);
INSERT INTO FitnessActivities (ActivityID, Location)
VALUES (SCOPE_IDENTITY(), 'Sydney Park');

-- 03/03/2025 19:00:00, Monopoly, 0, 2
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-03 19:00:00', 'Monopoly', 0);
INSERT INTO EntertainmentActivities (ActivityID, MinParticipants)
VALUES (SCOPE_IDENTITY(), 2);

-- 10/03/2025 19:30:00, Painting, 5, 1
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-10 19:30:00', 'Painting', 5);
INSERT INTO EntertainmentActivities (ActivityID, MinParticipants)
VALUES (SCOPE_IDENTITY(), 2);

-- 16/03/2025 19:30:00, Scrabble, 0, 2
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-16 19:30:00', 'Scrabble', 0);
INSERT INTO EntertainmentActivities (ActivityID, MinParticipants)
VALUES (SCOPE_IDENTITY(), 2);

-- 27/03/2025 20:00:00, Charades, 0, 4
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-27 20:00:00', 'Charades', 0);
INSERT INTO EntertainmentActivities (ActivityID, MinParticipants)
VALUES (SCOPE_IDENTITY(), 4);

-- 28/03/2025 20:00:00, Cluedo, 0, 3
INSERT INTO Activities (DateStartTime, Title, Cost)
VALUES ('2025-03-28 20:00:00', 'Cluedo', 0);
INSERT INTO EntertainmentActivities (ActivityID, MinParticipants)
VALUES (SCOPE_IDENTITY(), 3);
