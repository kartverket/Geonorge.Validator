FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /
COPY *.sln .
COPY Geonorge.Validator.Application/. ./Geonorge.Validator.Application/
COPY Geonorge.Validator.Common/. ./Geonorge.Validator.Common/
COPY Geonorge.Validator.GeoJson/. ./Geonorge.Validator.GeoJson/
COPY Geonorge.Validator.Map/. ./Geonorge.Validator.Map/
COPY Geonorge.Validator.Rules.GeoJson/. ./Geonorge.Validator.Rules.GeoJson/
COPY Geonorge.Validator.Web/. ./Geonorge.Validator.Web/
COPY Geonorge.Validator.XmlSchema/. ./Geonorge.Validator.XmlSchema/
RUN dotnet build -c Release -o /app_output
RUN dotnet publish -c Release -o /app_output

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app
COPY --from=build /app_output .

EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "Geonorge.Validator.Web.dll"]
