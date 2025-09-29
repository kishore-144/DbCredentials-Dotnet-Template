# DotNetAuthTemplate

A ready-to-use .NET login and signup template with basic user information storage and Gmail SMTP email support. Ideal for small projects, demos, or as a starting point to learn authentication in .NET applications.

## Features

- User registration and login functionality
- Basic user information storage (can be extended to any database)
- Email notifications via Gmail SMTP
- Lightweight and easy to customize
- Quick setup—just configure your email credentials

## Prerequisites

- .NET 6/7 SDK (or compatible version)
- Gmail account with App Password enabled (for sending emails)

## Installation

1. Clone the repository:

```bash
git clone https://github.com/kishore-144/DbCredentials-Dotnet-Template.git
cd DotNetAuthTemplate
```


2. Open the project in Visual Studio or VS Code.

3. Update your Gmail credentials in the SMTP configuration inside the controller:

```csharp
var smtpClient = new SmtpClient("smtp.gmail.com")
{
    Port = 587,
    Credentials = new NetworkCredential("<your email>", "<your email app password>"),
    EnableSsl = true,
};
```

4. Run the project:

```bash
dotnet run
```

## Usage

- Access the login and signup pages from your browser  
- Register a new user and login using the credentials
- Email notifications will be sent using the configured Gmail account

## Customization

- Extend the user model to include more fields
- Connect to a database of your choice for persistent storage
- Modify email templates or add additional notifications

## License

This project is open-source. Feel free to use, modify, and distribute for personal or commercial purposes.
