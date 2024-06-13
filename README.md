Welcome to:
## Name
Co-Working Space Web App

## Project status
This project is currently in development..

## Description
This is a web application for booking a space for work. It gives the possibility to people who doesn`t own his own working space for meetings or individual work to book a space in a co-working environment and do his job. 
The application will support 2 roles:

1/ User(worker) role:
It provides an option for booking a specific period of time in the space for a day choosen by the user.
When register the user will have an option to book his own time and to see what is the price for the time period. The application will provide also the possibilities to see a form with all personal bookings for the month with details about time usage, price and an option for edit if some entry is incorrect.

2/ Manager role:
This is the person who manages the co-working space. He will be able to see statistics about the usage of the space according to days and time period for every different user. He can check payments and approve them if everything is correct.

## About the project:
The project is a .NET MVC application with SQL database. It has a modified integrated authentication and use the following dependencies:
- EntityFramework.Core
- EntityFramework.Core.SqlServer
- EntityFramework.core.Tools

The solution is separated in 4 projects:

1/ CoWorkingSpace main part:
It holds all the logic for the UI, the view models, the controllers logic, some app settings and the Program.cs file with the main app configurations. 

2/ CoWorkingSpace.Core:
It holds all the services and interfaces for communication between front end and back end.

3/ CoWorkingSpace.Infrastructure:
It holds all the logic for the back end implementations(database models, and services).

4/ CoWorkingSpace.Tests
It holds all the application unit tests.