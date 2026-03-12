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

## Notes
For simplicity, I didn't consider scaling and cloud queue servcies, rather imaplemented this using in memory queue.
This will have limitaions like:
- Because all data is in-memory, a server restart or crash will permanently delete the entire queue and all active chat sessions.
- We cannot add a second server (horizontal scaling) to handle increased traffic because in-memory queues cannot be shared between different machines.
