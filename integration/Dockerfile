# Build Stage
FROM microsoft/dotnet:2.2-sdk as build-env

WORKDIR /integration

# restore 
COPY integration.csproj .
RUN dotnet restore

#RUN ls -alR

# copy src
COPY . .

ENTRYPOINT ["dotnet", "test"]