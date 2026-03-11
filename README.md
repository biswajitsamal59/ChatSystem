## 📂 Project Structure

```text
ChatSystem/
├── ChatSystem.sln                 # The main solution file
├── .gitignore                     # Git ignore rules for .NET and VS
├── azure-pipelines.yml            # CI/CD Pipeline definition for Azure DevOps
├── README.md                      # Project documentation
│
├── ChatAPI/                       # Main Web API Project
│   ├── Controllers/               # API Endpoints (ChatController)
│   ├── Models/                    # Domain models (Agent, ChatSession, Enums)
│   ├── Services/                  # Core business logic (ChatManager)
│   ├── Workers/                   # IHostedService Background Tasks
│   ├── Dockerfile                 # Multi-stage Docker build configuration
│   ├── Program.cs                 # App startup and DI container setup
│   └── ChatAPI.csproj             # Contains SAST (.NET Analyzers) config
│
└── ChatAPI.Tests/                 # xUnit Test Project
    ├── ChatManagerTests.cs        # Unit tests verifying routing/shift rules
    └── ChatAPI.Tests.csproj