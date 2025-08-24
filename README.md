# Game of Life (.NET 8 Solution)

This project implements Conway's Game of Life using .NET 8. It provides a flexible, extensible, and performant simulation engine, along with a user interface (swagger) and Docker support for easy deployment.

## Site Endpoints

Redis: http://localhost:8001/
Swagger: http://localhost:8080/swagger/index.html
Test UI: http://localhost:8080/index.html

## Features

- **Core Simulation Engine**: Implements the rules of Conway's Game of Life.
- **Configurable Grid Size**: Easily adjust the grid dimensions.
- **Customizable Initial State**: Set up initial patterns via configuration or code.
- **Step-by-Step Simulation**: Advance the simulation one generation at a time.
- **Unit Tests**: Comprehensive test coverage for core logic.
- **.NET 8**: Modern, fast, and cross-platform.
- **Docker Support**: Run the application in a containerized environment.
- **Test UI**: Simple web interface to visualize and interact with the simulation.  Normall I would not include this in the API project, but I used it for testing and decided to keep it in
- **Logging**: Integrated logging for monitoring and debugging.  You can find the logs in the `logs` directory.
- **Swagger**: Auto-generated API documentation and testing interface.

## How It Was Built
Based on the functional requirements I was given, I designed the application with a focus on modularity and testability. The core logic is encapsulated in services, while controllers handle HTTP requests. Redis is used for state storage to allow for scalability and persistence.
I tried to avoid any 'gold plating' and kept the design as simple as possible while still meeting the requirements. I broke from this rule for the test UI, which is not strictly necessary but useful for visualizing the simulation.
I used dependency injection to manage dependencies and facilitate unit testing. I also implemented custom middleware for error handling and logging to ensure a robust application.

Most of the code in this project was written by me.  I keep a library of common project templates and snippets that I use to speed up development.  

Because this is an AI company, and because I was told in ther interview that developers are encouraged to use AI tools, I did use ChatGPT to help me with some of the boilerplate code and to update the Dockerfile and docker-compose.yml to fit this project and to generate my unit tests.  
As the UI was not a part of the functional requirements,I also used AI to generate a simple HTML/JS interface to visualize the simulation. Lastly, I used AI to review my code for spelling errors and general formatting/linting.

Decisions on how to implement certain features were based on my experience and best practices in software development. I prioritized simplicity, maintainability, and performance throughout the design and implementation process.

I decided to use Redis for state storage because it is a fast, in-memory data store that is well-suited for this type of application. It allows for easy scaling and persistence of the simulation state.  It is also free and easy to integrate with Docker deployments.

- 
## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (optional, for containerization)

### Build and Run Locally

1. Clone the repository:
    ```sh
    git clone <repository-url>
    cd GameOfLife
    ```

2. Build the solution:
    ```sh
    dotnet build
    ```

3. Run the application:
    ```sh
    dotnet run --project GameOfLife
    ```

### Run Tests
## Docker Setup

1. Build the Docker image:
    ```sh
    docker build -t gameoflife .
    ```

2. Run the container:
    ```sh
    docker run --rm -it gameoflife
    ```

## Configuration

- Grid size and initial state can be configured via environment variables or configuration files (see source code for details).

## Project Structure
```plaintext
GameOfLife/
├── Properties/
│   └── launchSettings.json
├── wwwroot/
│   └── index.html
├── Controllers/
│   └── GameOfLifeController.cs
├── logs/
├── Middleware/
│   └── ErrorHandlingMiddleware.cs
├── Models/
│   ├── ApiResponse.cs
│   ├── Board.cs
│   ├── Cell.cs
│   └── DTO/
│       ├── BoardIdResponseDto.cs
│       ├── BoardStateDto.cs
│       ├── BoardStateResponseDto.cs
│       └── UploadBoardRequestDto.cs
├── RedisRepositories/
│   ├── IRedisBoardRepository.cs
│   └── RedisBoardRepository.cs
├── Services/
│   ├── IGameOfLifeService.cs
│   └── GameOfLifeService.cs
├── Validators/
│   ├── IBoardValidator.cs
│   └── BoardValidator.cs
├── appsettings.json
├── appsettings.Development.json
├── docker-compose.yml
├── Dockerfile
├── GameOfLife.http
├── Program.cs
└── README.md

GameOfLifeTests/
├── ControllerUnitTests/
└── GameOfLifeServiceUnitTests/
```


## Design Patterns Used
- **Dependency Injection**: Services and repositories are injected into controllers for better testability and separation of concerns.
- **Repository Pattern**: Abstracts data access logic, making it easier to switch data sources if needed.
- **Middleware**: Custom middleware for error handling and logging.
- **DTOs (Data Transfer Objects)**: Used to transfer data between layers, ensuring a clear contract for API responses and requests.
- **Contoller-Service Pattern**: Controllers handle HTTP requests and delegate business logic to services.

## Program.cs Overview
- Configures services and middleware for the application.
- Uses Swagger for API documentation.
- Uses Redis for state storage.
- Uses custom error handling middleware.
- Uses ILogger for logging.
- Injects scoped services for game logic and data access.
- Does not include HTTPS redirection for simplicity in local development or RBAC

## Docker Documentation
# Dockerfile
This Dockerfile builds and runs the Game of Life API using .NET 8.

Build Stage: Uses the .NET 8 SDK image to restore dependencies, compile the source, and prepare the build output.

Publish Stage: Publishes the application into a self-contained directory optimized for deployment.

Runtime Stage: Runs the application using the lightweight .NET 8 ASP.NET runtime image.

Logs directory (/app/logs) is created with proper permissions.

Ports 80 (HTTP) and 443 (HTTPS) are exposed.

The application starts with dotnet GameOfLife.dll.

This multi-stage setup ensures smaller, production-ready images by separating build, publish, and runtime environments.

# docker-compose.yml

This docker-compose.yml sets up the Game of Life API along with a Redis instance for state storage.

API Service (api):

Builds from the local Dockerfile.

Maps port 8080 on the host to 80 in the container.

Configured for Development environment and connects to Redis.

Persists logs locally via a bind mount (./logs:/app/logs).

Depends on the Redis service and shares a custom network.

Redis Service (redis):

Uses the official redis-stack image.

Maps ports 6379 (Redis) and 8001 (UI) to the host.

Persists data via a Docker volume (redis-data).

Networking & Volumes:

Both services share a dedicated bridge network (gameoflife-network).

Redis data is persisted across container restarts using the redis-data volume.

This setup allows running the API and its Redis dependency together with minimal configuration.

## Logging
Application logs are stored in the `logs` directory. This is configured in Program.cs and the Dockerfile to ensure logs are persisted and accessible.
You can access the logs by navigating to the `logs` directory in the project root.

## License

This project is licensed under the MIT License.

## Credits

- Inspired by John Conway's Game of Life.

---

Feel free to contribute or open issues for feature requests and bug reports!
