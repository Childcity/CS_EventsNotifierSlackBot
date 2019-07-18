FROM microsoft/dotnet:2.1-sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet CS_EventsNotifierSlackBot.dll

### By default (from template) ###
#FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
#WORKDIR /app
#EXPOSE 80
#EXPOSE 443
#
#FROM microsoft/dotnet:2.1-sdk AS build
#WORKDIR /src
#COPY ["CS_EventsNotifierSlackBot/CS_EventsNotifierSlackBot.csproj", "CS_EventsNotifierSlackBot/"]
#RUN dotnet restore "CS_EventsNotifierSlackBot/CS_EventsNotifierSlackBot.csproj"
#COPY . .
	#WORKDIR "/src/CS_EventsNotifierSlackBot"
	#RUN dotnet build "CS_EventsNotifierSlackBot.csproj" -c Release -o /app
	#
	#FROM build AS publish
#RUN dotnet publish "CS_EventsNotifierSlackBot.csproj" -c Release -o /app
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app .
#ENTRYPOINT ["dotnet", "CS_EventsNotifierSlackBot.dll"]