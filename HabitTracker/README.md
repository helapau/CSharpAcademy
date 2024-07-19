# Habit Tracker App 

This is a console application that lets the user track habits by quantity (not by time).
Example: 
I did 3 sit-ups on 2024-07-18. 

Requirements:
	- Use SQLite database (not in-memory)
	- When the application starts, the DB should be created, if it doesn't exist 
	- Only interact with the DB using raw SQL, can't use mappers such as Entity Framework
	
	
This project uses SQLite database. SQLite is a relational database engine implemented in C.
Its usecase is local data storage for individual applications and devices which is a different usecase
from client/server SQL databases (MS SQL Server, MySQL, etc..) that aim to provide
a shared repository of enterprise data. 

The SQLite database is a single file. The file format is multiplatform,
so it can be copied from Windows and opened on Linux for example.
The file extension is purely your choice, common conventions are `.db` or `.sqlite`
which can be followed by a number indicating the version fo sqlite - `.db3` or `.sqlite3`. 

The are several ways to interact with SQL database in c#.
The most high-level being Entity Framework and the most low-level being ADO.NET. 

ADO.NET provides classes for data access, it was introduced way back with the original .NET Framework. 
Entity Framework is an ORM and it lets you query the DB using c# rather than raw sql.

Because the requirement is to write raw sql queries, I will use ADO.NET. 

I installed the package `Microsoft.Data.Sqlite` using NuGet.
`Microsoft.Data.Sqlite` is a lightweight ADO.NET provider for SQLite. The package description said that 
the sqlite3 engine is included, therefore I didn't download sqlite separately. :) 

In the first step I created the class `DataStore` that creates the table and does basic CRUD operations on it.
I the second step I created the Main method of the program - show menu to the user and react to the input.



