# API-Tests-RestSharp

This repository contains automated API tests written in **C#**, using **NUnit** and **RestSharp**.  
Each project targets a different training API (e.g. BookStore, Recipes), which was temporarily hosted during QA training using **Swagger** or **Docker Compose**.

The tested APIs themselves are **not included** in this repository.  
This code is intended to **demonstrate my skills** in API testing — not to serve as a fully runnable test suite out of the box.

## What’s Included

Test projects for:

- `BookStoreApi`
- `RecipesApi`
- `EShopApi`
- `TravelersApi`

Each folder includes:

- ✅ Test classes with structured test cases
- ✅ Setup constants (e.g. base URLs)
- ✅ Visual Studio project files (`.csproj`, `.sln`)

## Technologies Used

- Language: **C#**
- Testing Framework: **NUnit**
- HTTP Client: **RestSharp**
- IDE: **Visual Studio**
- API Interface: **Swagger** (used during manual/API testing)
- Hosting (external): **Docker Compose** (used during training)

## About Execution

The tests were created as part of a QA automation course.  

Run from training-provided backend projects

Or started using docker-compose setups

As the APIs are not public or included here, these tests are intended for review and demonstration only — not for direct execution.

## Author
Radoslav Dichev - QA enthusiast learning API testing through real-world examples using RestSharp and NUnit.