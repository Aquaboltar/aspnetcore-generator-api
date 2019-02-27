# Build Stage
FROM microsoft/dotnet:2.2-sdk as build-env

WORKDIR /generator

# restore - CONSIDER USING SOLUTION INSTEAD of proj
COPY api/api.csproj ./api/
RUN dotnet restore api/api.csproj
COPY tests/tests.csproj ./tests/
RUN dotnet restore tests/tests.csproj

#RUN ls -alR

# copy src
COPY . .

# test
# for better xUnit TeamCity test runner integration
ENV TEAMCITY_PROJECT_NAME=fake 
RUN dotnet test tests/tests.csproj

# publish
RUN dotnet publish api/api.csproj -o /publish

# runtime stage
FROM microsoft/dotnet:2.2-aspnetcore-runtime
COPY --from=build-env /publish /publish
WORKDIR /publish
ENTRYPOINT ["dotnet", "api.dll"]


#COPY api.csproj .
#RUN dotnet restore

#COPY . .
#RUN dotnet publish -o /publish

# Runtime Image Stage
#FROM microsoft/aspnetcore:2
#WORKDIR /publish
#COPY --from=build-env /publish .
#ENTRYPOINT ["dotnet", "api.dll"]