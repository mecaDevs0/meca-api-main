# Use a imagem base do .NET Runtime 5.0 (conforme seu projeto)
FROM mcr.microsoft.com/dotnet/aspnet:5.0

# Define o diretório de trabalho no container
WORKDIR /app

# Copia os arquivos publicados para o container
COPY publish/ .

# Expõe a porta 80
EXPOSE 80

# Define variável de ambiente para porta do ASP.NET Core
ENV ASPNETCORE_URLS=http://+:80

# Comando para rodar a aplicação
ENTRYPOINT ["dotnet", "Meca.Api.dll"]
