module.exports = {
  apps : [{
    name   : "meca-api-main",
    script : "dotnet",
    args   : "Meca.WebApi.dll",
    cwd    : "/home/ubuntu/meca-projects/meca-api-main/src/Meca.WebApi/bin/Release/net8.0/publish/",
    watch  : false,
    env: {
      "ASPNETCORE_ENVIRONMENT": "Production"
    }
  }]
}