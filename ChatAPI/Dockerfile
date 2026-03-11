# Stage 1: The Build Environment (Heavy SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the project file first to cache the NuGet restore process
COPY ["ChatAPI/ChatAPI.csproj", "ChatAPI/"]
RUN dotnet restore "ChatAPI/ChatAPI.csproj"

# Copy the rest of the code and publish it
COPY . .
WORKDIR "/src/ChatAPI"
RUN dotnet publish "ChatAPI.csproj" -c Release -o /app/publish

# Stage 2: The Runtime Environment (Lightweight ASP.NET Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# In .NET 8, the default port inside the container is 8080
EXPOSE 8080

# Copy the compiled app from the build stage into this final image
COPY --from=build /app/publish .

# Tell Docker how to start your application
ENTRYPOINT ["dotnet", "ChatAPI.dll"]