dotnet ef migrations add InitialCreate
dotnet ef database update



CREATE TABLE "Roles" (
    "RoleID" SERIAL PRIMARY KEY,
    "RoleName" VARCHAR(255) NOT NULL
);

CREATE TABLE "Categories" (
    "CategoryID" SERIAL PRIMARY KEY,
    "CategoryName" VARCHAR(255) NOT NULL
);

CREATE TABLE "SubCategories" (
    "SubCategoryID" SERIAL PRIMARY KEY,
    "SubCategoryName" VARCHAR(255) NOT NULL,
    "CategoryID" INTEGER NOT NULL REFERENCES "Categories"("CategoryID")
);

CREATE TABLE "Businesses" (
    "BusinessID" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "EmailId" VARCHAR(255) NOT NULL,
    "Password" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Location" VARCHAR(255),
    "Latitude" DOUBLE PRECISION,
    "Longitude" DOUBLE PRECISION,
    "VisitingCard" VARCHAR(255),
    "CategoryID" INTEGER REFERENCES "Categories"("CategoryID"),
    "SubCategoryID" INTEGER REFERENCES "SubCategories"("SubCategoryID"),
    "RoleID" INTEGER REFERENCES "Roles"("RoleID")
);

CREATE TABLE "Customers" (
    "CustomerID" SERIAL PRIMARY KEY,
    "Name" VARCHAR(255) NOT NULL,
    "EmailId" VARCHAR(255) NOT NULL,
    "Password" VARCHAR(255) NOT NULL,
    "Phone" VARCHAR(20),
    "RoleID" INTEGER REFERENCES "Roles"("RoleID")
);

CREATE TABLE "LoginRequests" (
    "LoginRequestID" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL,
    "Password" VARCHAR(255) NOT NULL
);

CREATE TABLE "AdminLoginRequests" (
    "AdminLoginRequestID" SERIAL PRIMARY KEY,
    "Email" VARCHAR(255) NOT NULL,
    "Password" VARCHAR(255) NOT NULL
);

CREATE TABLE "BusinessRatings" (
    "BusinessRatingID" SERIAL PRIMARY KEY,
    "BusinessID" INTEGER NOT NULL REFERENCES "Businesses"("BusinessID"),
    "CustomerID" INTEGER NOT NULL REFERENCES "Customers"("CustomerID"),
    "Rating" DECIMAL(3,2),
    "Review" TEXT
);