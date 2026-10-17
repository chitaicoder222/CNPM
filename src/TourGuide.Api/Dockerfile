FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends python3 python3-pip \
    && pip3 install --break-system-packages --no-cache-dir edge-tts \
    && command -v edge-tts \
    && edge-tts --version \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/out .
COPY wwwroot ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV Tts__EdgeCommand=edge-tts
ENV PATH=/usr/local/bin:$PATH
ENTRYPOINT ["dotnet", "TourGuide.Api.dll"]
