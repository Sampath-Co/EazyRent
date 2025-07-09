
-- Create Users table
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    FullName VARCHAR(255) NOT NULL,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20),
    Role VARCHAR(50) CHECK (Role IN ('Owner', 'Tenant'))
);

-- Create Property table
CREATE TABLE Property (
    PropertyID INT PRIMARY KEY IDENTITY(1,1),
    OwnerID INT,
    Address VARCHAR(255),
    RentAmount DECIMAL(10, 2),
    AvailabilityStatus VARCHAR(50),
    PropertyImage VARBINARY(MAX),
    FOREIGN KEY (OwnerID) REFERENCES Users(UserID)
);

-- Create Lease table
CREATE TABLE Lease (
    LeaseID INT PRIMARY KEY IDENTITY(1,1),
    PropertyID INT,
    TenantID INT,
    StartDate DATE,
    EndDate DATE,
    RentAmount DECIMAL(10, 2),
    DigitalSignature VARBINARY(MAX),
    Status VARCHAR(50) CHECK (Status IN ('Pending', 'Active', 'Terminated')),
    FOREIGN KEY (PropertyID) REFERENCES Property(PropertyID),
    FOREIGN KEY (TenantID) REFERENCES Users(UserID)
);

-- Create Payment table
CREATE TABLE Payment (
    PaymentID INT PRIMARY KEY IDENTITY(1,1),
    LeaseID INT,
    Amount DECIMAL(10, 2),
    PaymentDate DATE,
    Status VARCHAR(50) CHECK (Status IN ('Pending', 'Paid')),
    FOREIGN KEY (LeaseID) REFERENCES Lease(LeaseID)
);

-- Create MaintenanceRequest table
CREATE TABLE MaintenanceRequest (
    RequestID INT PRIMARY KEY IDENTITY(1,1),
    PropertyID INT,
    TenantID INT,
    IssueDescription TEXT,
    Status VARCHAR(50) CHECK (Status IN ('Pending', 'Active', 'Terminated')),
    FOREIGN KEY (PropertyID) REFERENCES Property(PropertyID),
    FOREIGN KEY (TenantID) REFERENCES Users(UserID)
);

-- Insert sample data into Users table
INSERT INTO Users (FullName, Email, PasswordHash, PhoneNumber, Role)
VALUES 
('Ravi Kumar', 'ravi.kumar@example.com', 'hashed_password_1', '9876543210', 'Owner'),
('Anita Sharma', 'anita.sharma@example.com', 'hashed_password_2', '9123456780', 'Tenant'),
('Suresh Mehta', 'suresh.mehta@example.com', 'hashed_password_3', '9988776655', 'Tenant');

-- Insert sample data into Property table
INSERT INTO Property (OwnerID, Address, RentAmount, AvailabilityStatus, PropertyImage)
VALUES 
(1, '123 Main St, Vancouver, BC', 1500.00, 'Available', NULL),
(1, '456 Oak St, Vancouver, BC', 1800.00, 'Occupied', NULL),
(1, '789 Pine St, Vancouver, BC', 2000.00, 'Available', NULL);

-- Insert sample data into Lease table
INSERT INTO Lease (PropertyID, TenantID, StartDate, EndDate, RentAmount, DigitalSignature, Status)
VALUES 
(1, 2, '2023-01-01', '2023-12-31', 1500.00, NULL, 'Active'),
(2, 3, '2023-02-01', '2023-11-30', 1800.00, NULL, 'Active');

-- Insert sample data into Payment table
INSERT INTO Payment (LeaseID, Amount, PaymentDate, Status)
VALUES 
(1, 1500.00, '2023-01-01', 'Paid'),
(2, 1800.00, '2023-02-01', 'Paid');

-- Insert sample data into MaintenanceRequest table
INSERT INTO MaintenanceRequest (PropertyID, TenantID, IssueDescription, Status)
VALUES 
(1, 2, 'Leaky faucet', 'Pending'),
(2, 3, 'Broken window', 'Active');
