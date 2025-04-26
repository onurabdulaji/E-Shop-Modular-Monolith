# EShop ASP.NET Core Uygulaması

Bu proje, modern bir e-ticaret platformunun temelini oluşturan bir ASP.NET Core uygulamasıdır. Mikroservis mimarisi prensiplerine uygun olarak tasarlanmış olup, farklı işlevsel alanları bağımsız modüller ve servisler olarak ele alır.

## İçindekiler

1.  [Proje Amacı](#proje-amaci)
2.  [Mimari Desenler](#mimari-desenler)
    * [Mikroservis Mimarisi](#mikroservis-mimarisi)
    * [CQRS (Komut Sorgu Sorumluluk Ayrımı)](#cqrs-komut-sorgu-sorumluluk-ayrimi)
    * [Olay Güdümlü Mimari (Event-Driven Architecture)](#olay-gudumlu-mimari-event-driven-architecture)
3.  [Teknolojiler ve Bağımlılıklar](#teknolojiler-ve-bagimliliklar)
    * [.NET 8.0](#net-80)
    * [ASP.NET Core](#aspnet-core)
    * [Carter](#carter)
    * [MediatR](#mediatr)
    * [FluentValidation](#fluentvalidation)
    * [Mapster](#mapster)
    * [StackExchange.Redis](#stackexchange-redis)
    * [Npgsql ve Entity Framework Core](#npgsql-ve-entity-framework-core)
    * [MassTransit](#masstransit)
    * [Keycloak](#keycloak)
    * [Serilog](#serilog)
    * [Docker](#docker)
    * [Docker Compose](#docker-compose)
4.  [Proje Yapısı](#proje-yapisi)
    * [`Bootstrapper/Api`](#bootstrapperapi)
    * [`Modules/Basket/Basket`](#modulesbasketbasket)
    * [`Modules/Catalog/Catalog`](#modulescatalogcatalog)
    * [`Modules/Ordering/Ordering`](#modulesorderingordering)
    * [`Shared/Shared`](#sharedshared)
    * [`Shared/Shared.Contracts`](#sharedsharedcontracts)
    * [`Shared/Shared.Messaging`](#sharedsharedmessaging)
5.  [Başlangıç ve Yapılandırma](#baslangic-ve-yapilandirma)
    * [Ön Koşullar](#on-kosullar)
    * [Yerel Geliştirme İçin Yapılandırma](#yerel-gelistirme-icin-yapilandirma)
    * [Uygulamanın Çalıştırılması](#uygulamanin-calistirilmasi)
6.  [Dağıtım](#dagitim)
    * [Docker Entegrasyonu](#docker-entegrasyonu)
    * [Docker Compose ile Çalıştırma](#docker-compose-ile-calistirma)
7.  [İletişim](#iletisim)
8.  [Katkılar](#katkilar)
9.  [Lisans](#lisans)

## 1. Proje Amacı

Bu e-ticaret platformu projesi, ölçeklenebilir, bakımı kolay ve modern bir alışveriş deneyimi sunmayı hedeflemektedir. Farklı işlevsel alanları (ürün kataloğu, sepet yönetimi, sipariş işleme, kimlik doğrulama vb.) birbirinden bağımsız servisler olarak ele alarak, her birinin ayrı ayrı geliştirilmesini, test edilmesini ve ölçeklendirilmesini mümkün kılar.

## 2. Mimari Desenler

Bu proje, aşağıdaki temel mimari desenleri üzerine inşa edilmiştir:

### Mikroservis Mimarisi

Uygulama, farklı işlevsel alanları temsil eden bağımsız servislerden oluşur. Bu servisler, kendi veritabanlarına sahip olabilir ve birbirleriyle iyi tanımlanmış API'ler veya mesajlaşma sistemleri aracılığıyla iletişim kurarlar. Bu yaklaşım, büyük ve karmaşık uygulamaların daha küçük, yönetilebilir parçalara ayrılmasını sağlar.

### CQRS (Komut Sorgu Sorumluluk Ayrımı)

Bazı modüllerde (örneğin, Sipariş), Komut ve Sorgu Sorumluluklarının Ayrılması (CQRS) deseni uygulanmıştır. Bu desen, veri yazma (Komutlar) ve veri okuma (Sorgular) işlemlerini birbirinden ayırarak daha iyi performans, ölçeklenebilirlik ve model karmaşıklığını yönetme imkanı sunar. Projede kullanılan temel CQRS arayüzleri şunlardır:

```csharp
public interface ICommand : ICommand<Unit> { }
public interface ICommand<out TResponse> : IRequest<TResponse> { }
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Unit> where TCommand : ICommand<Unit> { }
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse> where TResponse : notnull { }
public interface IQuery<out T> : IRequest<T> where T : notnull { }
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse> where TResponse : notnull { }
```
Bu arayüzler, komutları, sorguları ve bunları işleyen işleyicileri (handler) tanımlamak için kullanılır ve MediatR kütüphanesiyle entegre bir şekilde çalışır.

### Olay Güdümlü Mimari (Event-Driven Architecture)

Servisler arasındaki bazı iletişimler, olaylar aracılığıyla asenkron olarak gerçekleştirilir. Bir serviste meydana gelen önemli bir durum değişikliği (örneğin, sepet ödemesi tamamlandı, ürün fiyatı değişti) bir olay olarak yayınlanır ve ilgili diğer servisler bu olayı dinleyerek kendi bağlamlarında gerekli işlemleri gerçekleştirirler. Bu, servisler arasındaki bağımlılığı azaltır ve daha esnek bir sistem sağlar. Projede kullanılan temel entegrasyon olayları şunlardır:

```csharp
public record IntegrationEvent
{
    public Guid EventId => Guid.NewGuid();
    public DateTime OccuredOn => DateTime.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName;
}

public record BasketCheckoutIntegrationEvent : IntegrationEvent
{
    public string UserName { get; set; } = default!;
    public Guid CustomerId { get; set; } = default!;
    public decimal TotalPrice { get; set; } = default!;
    // ... (Diğer teslimat, fatura ve ödeme bilgileri)
}

public record ProductPriceChangedIntegrationEvent : IntegrationEvent
{
    public Guid ProductId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public List<string> Category { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ImageFile { get; set; } = default!;
    public decimal Price { get; set; } = default!;
}
```

Bu olaylar, `IntegrationEvent` temel sınıfından miras alır ve servisler arasında bilgi paylaşımını sağlamak için kullanılır.

## 3\. Teknolojiler ve Bağımlılıklar

Bu proje aşağıdaki temel teknolojileri ve kütüphaneleri kullanmaktadır:

 * **[.NET 8.0]:** Uygulamanın geliştirildiği ve çalıştırıldığı en son .NET platformu.
  * **[ASP.NET Core]:** Modern, bulut tabanlı uygulamalar geliştirmek için kullanılan açık kaynaklı framework.
  * **[Carter (8.1.0)]:** Minimal API yaklaşımını benimseyen hafif bir web framework'ü. Rotaları modüller halinde organize etmeyi sağlar.
  * **[MediatR (12.2.0)]:** Uygulama içi mesajlaşma ve CQRS gibi desenleri uygulamayı kolaylaştıran bir kütüphane.
  * **[FluentValidation (11.11.0) ve FluentValidation.AspNetCore (11.3.0) ve FluentValidation.DependencyInjectionExtensions (11.11.0)]:** Yapılandırılmış ve akıcı bir şekilde veri doğrulama kuralları tanımlamak için kullanılan bir kütüphane. ASP.NET Core entegrasyonu ve bağımlılık enjeksiyonu (dependency injection) desteği içerir.
  * **[Mapster (7.4.0)]:** Nesneler arasında otomatik ve performanslı bir şekilde eşleme (mapping) yapmak için kullanılan bir kütüphane.
  * **[StackExchange.Redis (2.7.17)]:** Yüksek performanslı bir anahtar-değer deposu olan Redis ile etkileşim kurmak için kullanılan .NET istemci kütüphanesi. Dağıtılmış önbellekleme için kullanılır.
  * **[Npgsql.EntityFrameworkCore.PostgreSQL (8.0.11) ve Microsoft.EntityFrameworkCore (8.0.11) ve Microsoft.EntityFrameworkCore.Tools (8.0.11)]:** .NET uygulamalarında PostgreSQL veritabanı ile etkileşim kurmayı kolaylaştıran Entity Framework Core ORM framework'ü ve PostgreSQL sağlayıcısı.
  * **[MassTransit (8.2.2)]:** .NET için dağıtılmış mesajlaşma altyapısını basitleştiren bir framework. Farklı mesaj taşıma teknolojilerini (örneğin, RabbitMQ) destekler ve tüketici (consumer), saga gibi kavramları yönetmeyi kolaylaştırır. Projede `AddMassTransitWithAssemblies` genişletme metodu kullanılarak belirtilen assembly'lerdeki tüm MassTransit bileşenleri otomatik olarak yapılandırılır.
  * **[Keycloak (24.0.3)]:** Açık kaynaklı bir kimlik ve erişim yönetimi sunucusu. Uygulamanın güvenliğini sağlamak için kullanılır. Projede `AddKeycloakWebApiAuthentication` genişletme metodu ile entegre edilmiştir.
  * **[Serilog (3.1.1)]:** Zengin loglama yetenekleri sunan bir .NET loglama kütüphanesi. Yapılandırma, `appsettings.json` dosyasından okunur ve loglar konsola ve Seq loglama sunucusuna yazdırılır.
  * **[Docker]:** Uygulamayı ve bağımlılıklarını konteynerler içinde paketlemeyi sağlayan bir platform.
  * **[Docker Compose]:** Birden fazla Docker konteynerini tek bir YAML dosyası ile tanımlayıp yönetmeyi sağlayan bir araç.


  ## 4\. Proje Yapısı

Proje, aşağıdaki ana klasör ve projelere ayrılmıştır:

  * **`Bootstrapper/Api`:** Uygulamanın giriş noktası olan ASP.NET Core API projesi. Servislerin kaydı, ara katman yazılımı (middleware) yapılandırması ve modüllerin entegrasyonu burada yapılır.
  * **`Modules/Basket/Basket`:** Sepet yönetimi ile ilgili iş mantığını ve API uç noktalarını (endpoint) içeren modül projesi.
  * **`Modules/Catalog/Catalog`:** Ürün kataloğu yönetimi ile ilgili iş mantığını ve API uç noktalarını içeren modül projesi.
  * **`Modules/Ordering/Ordering`:** Sipariş işleme ile ilgili iş mantığını, API uç noktalarını ve CQRS uygulamasını içeren modül projesi.
  * **`Shared/Shared`:** Tüm modüller tarafından ortak olarak kullanılan temel yardımcı sınıfları ve genişletme metotlarını içeren proje.
  * **`Shared/Shared.Contracts`:** Modüller arasında paylaşılan sözleşmeleri (arayüzler, DTO'lar, CQRS arayüzleri) içeren proje.
  * **`Shared/Shared.Messaging`:** Entegrasyon olayları ve MassTransit ile ilgili yapılandırmaları içeren proje.

  ## 5\. Başlangıç ve Yapılandırma

### Ön Koşullar

Uygulamayı yerel olarak geliştirmek ve çalıştırmak için aşağıdaki yazılımların sisteminizde kurulu olması gerekmektedir:

  * [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  * [Docker](https://www.docker.com/products/docker-desktop/) (isteğe bağlı, konteynerleştirilmiş ortam için)
  * [Docker Compose](https://docs.docker.com/compose/install/) (isteğe bağlı, çoklu konteyner ortamı için)

### Yerel Geliştirme İçin Yapılandırma

1.  Proje reposunu klonlayın:

    ```bash
    git clone <proje_reposu_url>
    cd <proje_dizini>
    ```

2.  `appsettings.json` dosyasını inceleyin ve yerel geliştirme ortamınıza uygun bağlantı dizelerini ve diğer ayarları yapılandırın. Özellikle veritabanı (`ConnectionStrings:Database`), Redis (`ConnectionStrings:Redis`), mesaj brokerı (`MessageBroker`), Keycloak (`Keycloak`) ve Seq (`Serilog:WriteTo:Seq:Args:serverUrl`) ayarlarını kontrol edin.

### Uygulamanın Çalıştırılması

Uygulamayı yerel olarak çalıştırmanın birkaç yolu vardır:

  * **Visual Studio veya benzeri bir IDE kullanarak:** Projeyi IDE'de açın ve `Bootstrapper/Api` projesini başlatın.

  * **.NET CLI kullanarak:**

    ```bash
    cd Bootstrapper/Api
    dotnet run
    ```

## 6\. Dağıtım

### Docker Entegrasyonu

Bu proje, Docker ile konteynerleştirilmiştir. Proje kök dizininde bir `Dockerfile` (`Bootstrapper/Api/Dockerfile`) bulunmaktadır. Bu dosya, uygulamanın Docker imajının nasıl oluşturulacağını tanımlar. Çok aşamalı (multi-stage) build kullanılarak imaj boyutu optimize edilmiştir.

### Docker Compose ile Çalıştırma

Uygulamanın tüm bağımlılıklarını (PostgreSQL, Redis, Seq, RabbitMQ, Keycloak) tek bir komutla ayağa kaldırmak için `docker-compose.yml` dosyası kullanılır.

1.  Proje kök dizininde aşağıdaki komutu çalıştırın:

    ```bash
    docker-compose up -d
    ```

    Bu komut, tüm tanımlı servisleri arka planda başlatır.

2.  Servislerin durumunu kontrol etmek için:

    ```bash
    docker-compose ps
    ```

3.  Uygulamaya aşağıdaki adreslerden erişebilirsiniz:

      * API: `http://localhost:6000` (HTTP) veya `https://localhost:6060` (HTTPS - sertifika yapılandırmasına bağlı)
      * Seq: `http://localhost:5341` (UI)
      * RabbitMQ Yönetim Arayüzü: `http://localhost:15672` (kullanıcı adı/parola: `guest/guest`)
      * Keycloak: `http://localhost:9090` (yönetici kullanıcı adı/parola: `admin/admin`)


**`docker-compose.yml` Dosyası İçeriği:**

```yaml
services:
  eshopdb:
    container_name: eshopdb
    image: postgres
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=123456789
      - POSTGRES_DB=EShopDB
    restart: always
    ports:
      - "5434:5432"
    volumes:
      - postgres_eshopdb:/var/lib/postgresql/data/

  distributedcache:
    container_name: distributedcache
    image: redis
    restart: always
    ports:
      - "6379:6379"

  seq:
    container_name: seq
    image: datalust/seq:latest
    environment:
      - ACCEPT_EULA=Y
    restart: always
    ports:
      - "5341:5341"
      - "9091:80"

  messagebus:
    container_name: messagebus
    hostname: ecommerce-mq
    image: rabbitmq:management
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    restart: always
    ports:
      - "5672:5672"
      - "15672:15672"

  identity:
    container_name: identity
    image: quay.io/keycloak/keycloak:24.0.3
    environment:
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=admin
      - KC_DB=postgres
      - KC_DB_URL=jdbc:postgresql://eshopdb/EShopDB?currentSchema=identity
      - KC_DB_USERNAME=postgres
      - KC_DB_PASSWORD=123456789
      - KC_HOSTNAME=http://identity:9090/
      - KC_HTTP_PORT=9090
    restart: always
    ports:
      - "9090:9090"
    command:
      - start-dev

  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_HTTP_PORTS=8080
      - ASPNETCORE_HTTPS_PORTS=8081
      - ConnectionStrings__Database=Server=eshopdb;Port=5432;Database=BasketDb;User Id=postgres;Password=123456789;Include Error Detail=true
      - ConnectionStrings__Redis=distributedcache:6379
      - MessageBroker__Host=amqp://ecommerce-mq:5672
      - MessageBroker__UserName=guest
      - MessageBroker__Password=guest
      - Keycloak__AuthServerUrl=http://identity:9090
      - Serilog__Using__0=Serilog.Sinks.Console
      - Serilog__Using__1=Serilog.Sinks.Seq
      - Serilog__MinimumLevel__Default=Information
      - Serilog__MinimumLevel__Override__Microsoft=Information
      - Serilog__MinimumLevel__Override__System=Warning
      - Serilog__WriteTo__0__Name=Console
      - Serilog__WriteTo__1__Name=Seq
      - Serilog__WriteTo__1__Args__serverUrl=http://seq:5341
      - Serilog__Enrich__0=FromLogContext
      - Serilog__Enrich__1=WithMachineName
      - Serilog__Enrich__2=WithProcessId
      - Serilog__Enrich__3=WithThreadId
      - Serilog__Properties__Application=EShop ASP.NET Core App
      - Serilog__Properties__Environment=Development
    depends_on:
      - eshopdb
      - distributedcache
      - seq
      - messagebus
      - identity
    ports:
      - "6000:8080"
      - "6060:8081"
    volumes:
      - ${APPDATA}/Microsoft/UserSecrets:/home/app/.microsoft/usersecrets:ro
      - ${APPDATA}/Microsoft/UserSecrets:/root/.microsoft/usersecrets:ro
      - ${APPDATA}/ASP.NET/Https:/home/app/.aspnet/https:ro
      - ${APPDATA}/ASP.NET/Https:/root/.aspnet/https:ro
      volumes:
    postgres_eshopdb: