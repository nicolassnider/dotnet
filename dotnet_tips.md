# .NET / C# — Tips & Best Practices

Colección de tips y buenas prácticas recopilados de la comunidad .NET.

---

## Tabla de contenidos

- [Principios de Diseño](#principios-de-diseño)
  - [KISS — Keep It Simple, Stupid](#kiss--keep-it-simple-stupid)
  - [Top 5 Design Patterns en .NET](#top-5-design-patterns-en-net)
- [Estilo y legibilidad](#estilo-y-legibilidad)
  - [Clean Code: Clever vs Maintainable](#clean-code-clever-vs-maintainable)
  - [Vertical Coding Style](#vertical-coding-style)
- [Lenguaje C#](#lenguaje-c)
  - [Field-backed Properties — C# 14](#field-backed-properties--c-14)
  - [nameof vs ToString() en enums](#nameof-vs-tostring-en-enums)
- [Async / Await](#async--await)
  - [11 Reglas para Programación Asíncrona](#11-reglas-para-programación-asíncrona)
- [LINQ y colecciones](#linq-y-colecciones)
  - [OrderBy vs ThenBy](#orderby-vs-thenby)
  - [GroupBy vs CountBy — .NET 9](#groupby-vs-countby--net-9)
- [Serialización](#serialización)
  - [System.Text.Json vs Newtonsoft.Json](#systemtextjson-vs-newtonsoftjson)
- [Seguridad y API](#seguridad-y-api)
  - [CORS en ASP.NET Core](#cors-en-aspnet-core)
  - [Rate Limiting en .NET](#rate-limiting-en-net)

---

## Principios de Diseño

### KISS — Keep It Simple, Stupid

El código simple tiene menos bugs y acelera el desarrollo.

**❌ Mal ejemplo — condición verbosa:**
```csharp
if (user != null && user.IsActive == true && user.IsDeleted == false)
```

**✅ Buen ejemplo — null-conditional operator:**
```csharp
if (user?.IsActive == true)
```

**Sobre-ingeniería a evitar:** apilar capas innecesarias (Controller → Service → Manager → Helper → Utility) cuando Controller → Service alcanza.

**Regla de oro:** código simple = menos bugs. Código legible = desarrollo más rápido.

---

### Top 5 Design Patterns en .NET

Los design patterns son soluciones probadas a problemas recurrentes de diseño. En .NET se usan con frecuencia en la construcción de aplicaciones escalables y mantenibles, pero su valor está en aplicarlos donde realmente resuelven un problema concreto, no como un fin en sí mismos.

| # | Patrón | Qué es | Cuándo usarlo | Ejemplo en .NET |
|---|--------|--------|---------------|-----------------|
| 1 | **Dependency Injection** | Proveer dependencias desde afuera en lugar de crearlas internamente | Lograr bajo acoplamiento y mejor testabilidad | Inyectar `ILogger` o `IUserService` vía constructor |
| 2 | **Factory Method** | Define una interfaz para crear objetos; las subclases deciden qué instanciar | Cuando la clase no puede predecir el tipo exacto a crear | `LoggerFactory` creando `FileLogger` o `DatabaseLogger` |
| 3 | **Repository Pattern** | Interfaz similar a una colección para acceder y gestionar la fuente de datos | Separar la lógica de acceso a datos de la lógica de negocio | `UserRepository.GetById(id)` para consultar la DB |
| 4 | **Strategy Pattern** | Define una familia de algoritmos intercambiables en tiempo de ejecución | Cambiar el comportamiento de un objeto dinámicamente | Distintas estrategias de pago: `CreditCard`, `UPI`, `Wallet` |
| 5 | **Singleton Pattern** | Garantiza una única instancia de una clase con punto de acceso global | Recursos compartidos: configuración, logging, caché | `AppSettings.Instance` para acceder a settings globales |

> *Design patterns are best when they solve real problems, not when they are overused.*

---

## Estilo y legibilidad

### Clean Code: Clever vs Maintainable

> *Clean code is not about writing less code. It's about writing code that scales.*

**❌ Clever pero difícil de mantener:**
```csharp
public void ProcessData(string d) {
    var x = d.Split(',');
    for (int i = 0; i < x.Length; i++) {
        if (x[i].Contains("error")) {
            Console.WriteLine("Found error!");
        }
    }
}
```

**✅ Limpio y mantenible:**
```csharp
public void ProcessLogs(string logData) {
    var logEntries = ParseLogEntries(logData);

    foreach (var entry in logEntries) {
        if (IsError(entry)) {
            Console.WriteLine("Found error!");
        }
    }
}

private string[] ParseLogEntries(string logData) {
    return logData.Split(',');
}

private bool IsError(string logEntry) {
    return logEntry.Contains("error");
}
```

---

### Vertical Coding Style

*Fuente: @serkutyildirim*

Preferir el estilo vertical al encadenar LINQ u otras llamadas fluidas.

**❌ Estilo horizontal (difícil de leer):**
```csharp
var users = customers.Where(c => c.IsActive).OrderBy(c => c.Age).ThenBy(c => c.Name).Select(c => c.Email).ToList();
```

**✅ Estilo vertical (limpio y legible):**
```csharp
var users = customers
    .Where(c => c.IsActive)
    .OrderBy(c => c.Age)
    .ThenBy(c => c.Name)
    .Select(c => c.Email)
    .ToList();
```

---

## Lenguaje C#

### Field-backed Properties — C# 14

*Fuente: Mahmoud Ahmed — .NET Developer*

En C# 14 ya no hace falta declarar el campo de respaldo manualmente. La keyword contextual `field` referencia el campo generado por el compilador.

**❌ Enfoque tradicional:**
```csharp
private string _firstName = "";  // Manual backing field

public string FirstName
{
    get => _firstName;
    set => _firstName = value.Trim();
}
```

**✅ C# 14 — field-backed property:**
```csharp
public string FirstName
{
    get;
    set => field = value.Trim(); // compiler-generated backing field
}
```

Menos boilerplate, misma performance, misma semántica.

---

### nameof vs ToString() en enums

*Fuente: @serkutyildirim*

Cuando se necesita el nombre de un valor de enum como string, `nameof` es significativamente más rápido que `.ToString()`. La diferencia es que `ToString()` usa reflection en tiempo de ejecución y aloca memoria en el heap, mientras que `nameof` se resuelve en tiempo de compilación como una constante de string, sin allocations.

**❌ Lento — usa reflection y aloca 24 bytes:**
```csharp
[Benchmark]
public string ToStringExample()
{
    // Slow
    return UserType.Admin.ToString();
}
// Mean: 6.8073 ns | Allocated: 24 B
```

**✅ Rápido — se resuelve en compile time, sin allocations:**
```csharp
[Benchmark]
public string NameOfExample()
{
    // Fast
    return nameof(UserType.Admin);
}
// Mean: 0.5100 ns | Allocated: -
```

La diferencia es de ~13x en velocidad. Preferir `nameof` siempre que el nombre del enum no cambie en runtime.

---

## Async / Await

### 11 Reglas para Programación Asíncrona

*Fuente: Aram Tchekrekjian — @AramT87 / CodingSonata*

1. Nunca bloquees código async. Usá `await`, no `.Result` o `.Wait`
2. Nunca retornes `async void` para Tasks. Solo para Event Handlers.
3. Ejecutá tareas independientes en paralelo con `Task.WhenAll`
4. No envuelvas I/O async en `Task.Run`
5. Usá `ConfigureAwait(false)` en código de librería
6. Mantené async en toda la cadena de llamadas (*async all the way up*)
7. Siempre observá y manejá las excepciones de tasks
8. No marques métodos como async si no lo necesitan
9. Pasá `CancellationToken` a operaciones de larga duración
10. No hagas `await` dentro de loops si el trabajo puede correr en paralelo
11. Usá `Task.WhenEach` para streamear resultados concurrentemente

---

## LINQ y colecciones

### OrderBy vs ThenBy

*Fuente: @serkutyildirim*

Al ordenar por múltiples criterios en LINQ, encadenar varios `OrderBy` no produce el resultado esperado: cada llamada reinicia el ordenamiento desde cero, descartando el criterio anterior. Para ordenamiento compuesto se debe usar `OrderBy` para el criterio primario y `ThenBy` para los secundarios.

**❌ Incorrecto — múltiples OrderBy se pisan entre sí:**
```csharp
public List<Product> OrderByMethod()
{
    var productList = GetProducts();

    // This usage reorders the list (cada OrderBy descarta el anterior)
    productList = productList
        .OrderBy(prod => prod.Name)
        .OrderBy(prod => prod.ListPrice)
        .OrderBy(prod => prod.Size)
        .ToList();

    return productList;
}
```

**✅ Correcto — OrderBy para el primario, ThenBy para los siguientes:**
```csharp
public List<Product> OrderByWithThenByMethod()
{
    var productList = GetProducts();

    // The list is ordered by name first, then by ListPrice, then by Size
    productList = productList
        .OrderBy(prod => prod.Name)
        .ThenBy(prod => prod.ListPrice)
        .ThenBy(prod => prod.Size)
        .ToList();

    return productList;
}
```

---

### GroupBy vs CountBy — .NET 9

Cuando solo interesa el conteo por clave, `CountBy` es más eficiente que `GroupBy` + `Count()`.

**❌ Legacy — GroupBy (alto consumo de memoria):**
```csharp
public void LegacyCountOrders(List<Order> orders) {
    // Materializa grupos intermedios con todas sus referencias
    var oldStats = orders
        .GroupBy(o => o.Status)
        .Select(g => new {
            Status = g.Key,
            Count = g.Count()
        });
}
```

**✅ Moderno — CountBy (.NET 9, single pass):**
```csharp
public void ModernCountOrders(List<Order> orders)
{
    // Un solo recorrido, sin estructuras intermedias
    var modernStats = orders.CountBy(o => o.Status);
}
```

`CountBy` expresa la intención directamente y evita alocar las colecciones intermedias que genera `GroupBy`.

---

## Serialización

### System.Text.Json vs Newtonsoft.Json

*Fuente: Poorna Soysa — poornasoysa.tech*

**System.Text.Json** — built-in desde .NET Core 3.0, mejor performance, sin dependencias externas:
```csharp
string json = JsonSerializer.Serialize(users);
var userList = JsonSerializer.Deserialize<List<User>>(json);
```

**Newtonsoft.Json** — paquete externo, más maduro, mayor flexibilidad y ecosistema:
```csharp
string json = JsonConvert.SerializeObject(users);
var userList = JsonConvert.DeserializeObject<List<User>>(json);
```

Preferir `System.Text.Json` en proyectos nuevos salvo que se necesiten features específicos de Newtonsoft (converters complejos, soporte de tipos dinámicos, etc.).

---

## Seguridad y API

### CORS en ASP.NET Core

CORS (Cross-Origin Resource Sharing) es un mecanismo de seguridad del navegador que restringe las peticiones HTTP entre distintos orígenes. En ASP.NET Core se configura como middleware en el pipeline, y es fundamental definir políticas explícitas en lugar de abrir todo indiscriminadamente. Una política demasiado permisiva expone la API a peticiones de cualquier cliente web, lo que representa un riesgo de seguridad real en producción.

**❌ Política insegura — permite cualquier origen, header y método:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("BadPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

**✅ Política segura — orígenes, métodos y headers explícitos:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy", policy =>
    {
        policy.WithOrigins(
                  "https://codingsonata.com",
                  "https://admin.codingsonata.com"
              )
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization");
    });
});
```

---

### Rate Limiting en .NET

*Fuente: @serkutyildirim*

Rate Limiting es una técnica para limitar la cantidad de requests que un cliente puede hacer en un período de tiempo. Protege las APIs contra abuso, ataques de fuerza bruta y sobrecarga del servidor. Desde .NET 7, el framework incluye soporte nativo a través de `Microsoft.AspNetCore.RateLimiting`, sin necesidad de librerías externas.

**Fixed Window Limiter** — ventana de tiempo fija; reinicia el contador al terminar cada intervalo:
```csharp
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
});
```

**Sliding Window Limiter** — ventana deslizante; distribuye el límite en segmentos dentro del intervalo, evitando picos de tráfico al borde de la ventana:
```csharp
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(10);
        options.SegmentsPerWindow = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
});
```

La diferencia clave: Fixed Window puede permitir ráfagas de hasta el doble del límite en el borde entre dos ventanas. Sliding Window lo evita distribuyendo el conteo en segmentos solapados.

---

*Recopilado de posteos de la comunidad .NET en LinkedIn y redes.*
