# The Featured Bugz - auth service

Welcome to the README for the Auth Service project developed by The Featured Bugz team. This project consists of two main projects: the API testing project and the authenticationServiceAPI project. The authenticationServiceAPI provides functionality for user authentication and interacts with another service called [user-service](https://github.com/The-Featured-Bugz-Semester-4-Exam/user-service.git) to validate login credentials.

## Table of Contents

- Overview
- API Endpoints
    - Post Login
    - Version
- Getting Started
    - Prerequisites
    - Installation
    - Configuration
- Running with Docker
- Contributing

## Overview

The Auth Service project provides an API for **`user authentication`**. It offers two main API endpoints: **`postLogin`** and **`version`**. The postLogin endpoint is used for user login, where the provided credentials are validated by the [user-service](https://github.com/The-Featured-Bugz-Semester-4-Exam/user-service.git). The version endpoint returns the project's version information using NLog.

## API Endpoints
### **Post Login**
```bash
POST /api/Login
```

This endpoint is used for user login.

### Request Body
The request should include the following data and this is a example
```Json
{
  "UserLogin": "JohnDoe",
  "UserPassword": "password1234"
}
```

### Response
The response will indicate whether the login is successful or not. If the login is valid, the API will respond with a status code **`200 OK`**. If the login is invalid, the API will return a status code **`404 Not Found`**.

### When the login is succes with status code **`200 OK`**
```json
{
    "userID": 1,
    "userName": "JohnDoe",
    "userPassword": "password123",
    "userEmail": "john.doe@example.com",
    "userAddress": "123 Street, City",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IkpvaG5Eb2UiLCJleHAiOjE2ODUzNDgxMTAsImlzcyI6Im15SXNzdWVySXNBbklzc3VlIiwiYXVkIjoiaHR0cDovL2xvY2FsaG9zdCJ9.Zsc_SS2Z8jVW9Qi5tItztZ7PckzRd8Pg6AmzbBkeOcU"
}
```

### **Version**
```bash
GET /api/version
```

This endpoint allows you to retrieve the version information of the authenticationServiceAPI project.

### Response

The API will respond with the version information of the project.

## **Getting Started**

To get started with the Auth Service project, follow the instructions below.

### **Prerequisites**

Before running the project, make sure you have the following installed:

- Mongodb
- bash

### **Installation**

1. Clone the repository to your local machine
2. Clone the [user-service](https://github.com/The-Featured-Bugz-Semester-4-Exam/user-service.git) respository to your local machine

3. Go into the directory userServiceAPI and authenticationServiceAPI.

4. run the following command in the both projects

```bash
. ./startup.sh
```

### **Configuration**

Before starting the API, you may need to configure some settings. Open the **`startup.sh file`** and modify it according to your requirements. Additionally, ensure that the "user-service" is up and running for the authentication to work properly.

Running with Docker

The Auth Service API can also be set up using Docker. A Dockerfile is provided to simplify the setup process. Make sure you have Docker installed on your system, and then follow these steps:


# Fix med compose fil