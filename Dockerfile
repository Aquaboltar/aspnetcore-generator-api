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

# test


# publish


# runtime stage




#COPY api.csproj .
#RUN dotnet restore

#COPY . .
#RUN dotnet publish -o /publish

# Runtime Image Stage
#FROM microsoft/aspnetcore:2
#WORKDIR /publish
#COPY --from=build-env /publish .
#ENTRYPOINT ["dotnet", "api.dll"]