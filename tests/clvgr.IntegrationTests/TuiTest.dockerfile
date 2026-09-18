# Stage 1
FROM ubuntu:24.04 AS host

RUN apt-get update && apt-get install -y \
    curl \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

RUN curl --proto '=https' --tlsv1.2 -LsSf https://raw.githubusercontent.com/microsoft/tui-test/main/install/install.sh | TUI_TEST_VERSION=beta sh


# Stage 2
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build

RUN apt-get update && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

COPY *.slnx ./
COPY Directory.*.props ./
COPY src/clvgr/clvgr.csproj ./src/clvgr/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore -v diag -r linux-x64  "src/clvgr/clvgr.csproj"

COPY . .

WORKDIR /src/src/clvgr
RUN dotnet build "clvgr.csproj" -c Release -o /app/build

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "clvgr.csproj" -c Release --no-restore -o /app/publish


# Stage 3
FROM host AS final

WORKDIR /app
COPY --from=build /app/publish .

WORKDIR /
ENTRYPOINT ["/bin/bash"]
