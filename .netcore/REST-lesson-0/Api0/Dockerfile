FROM microsoft/dotnet:2.2-sdk as build

WORKDIR /app
COPY *.fsproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/bin

FROM microsoft/dotnet:2.2-runtime

WORKDIR /app
COPY db db
COPY --from=build /app/bin /app

ENV PORT=80
EXPOSE $PORT

CMD dotnet /app/Api0.dll