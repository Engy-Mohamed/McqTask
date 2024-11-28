# Create a styled README file in HTML format

html_content = """
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Multiple Choice Exam System - README</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
            color: #333;
        }
        .container {
            max-width: 800px;
            margin: 20px auto;
            padding: 20px;
            background: #fff;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            border-radius: 8px;
        }
        h1, h2, h3 {
            color: #0056b3;
        }
        h1 {
            text-align: center;
        }
        ul {
            padding-left: 20px;
        }
        code {
            background: #e8e8e8;
            padding: 2px 4px;
            border-radius: 4px;
        }
        pre {
            background: #e8e8e8;
            padding: 10px;
            border-radius: 8px;
            overflow-x: auto;
        }
        .footer {
            margin-top: 20px;
            text-align: center;
            font-size: 0.9em;
            color: #555;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>Multiple Choice Exam System</h1>

        <h2>Overview</h2>
        <p>The <strong>Multiple Choice Exam System</strong> is a web application built using <strong>ASP.NET MVC</strong> and <strong>Entity Framework Core</strong>. It allows admins to manage exams and students to participate in them, providing a seamless and responsive interface.</p>

        <h2>Features</h2>
        <h3>Admin Functionality</h3>
        <ul>
            <li>Add multiple-choice questions with options and specify the correct answer.</li>
            <li>View a list of students along with their scores.</li>
            <li>Manage questions and options.</li>
        </ul>
        <h3>Student Functionality</h3>
        <ul>
            <li>Register for the exam by providing their name, email, and phone number.</li>
            <li>Take an exam with dynamically loaded questions.</li>
            <li>Submit answers and view their results.</li>
        </ul>

        <h2>Technologies Used</h2>
        <ul>
            <li><strong>Backend:</strong> ASP.NET Core MVC, Entity Framework Core</li>
            <li><strong>Frontend:</strong> Razor Views, HTML5, CSS, Bootstrap</li>
            <li><strong>Database:</strong> SQL Server (with EF Core Code First)</li>
            <li><strong>Development Tools:</strong> Visual Studio, .NET CLI</li>
        </ul>


