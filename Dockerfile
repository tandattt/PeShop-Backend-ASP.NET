# =======================
# 🔹 Build stage
# =======================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app

# =======================
# 🔹 Runtime stage
# =======================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Cài đặt tzdata để .NET nhận timezone hệ thống
RUN apt-get update && apt-get install -y tzdata \
    && ln -fs /usr/share/zoneinfo/Asia/Ho_Chi_Minh /etc/localtime \
    && dpkg-reconfigure -f noninteractive tzdata \
    && rm -rf /var/lib/apt/lists/*

# Đặt timezone môi trường (đảm bảo .NET DateTime.Now đúng giờ VN)
ENV TZ=Asia/Ho_Chi_Minh

COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "PeShop.dll"]
