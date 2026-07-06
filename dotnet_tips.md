# .NET / C# — Guía Completa: Tips, Patrones y Entrevistas

Colección consolidada de conceptos, buenas prácticas y preguntas frecuentes de entrevista para desarrolladores .NET.

---

## Tabla de contenidos

### Core Fundamentals
- [Conceptos Básicos de C#](#conceptos-básicos-de-c)
- [Keywords Importantes](#keywords-importantes)
- [Delegates y Expresiones Lambda](#delegates-y-expresiones-lambda)

### Architecture & Patterns
- [Top 5 Design Patterns en .NET](#top-5-design-patterns-en-net)
- [Principios de Diseño](#principios-de-diseño)

### Data & Async
- [Colecciones: IEnumerable vs IQueryable vs List](#colecciones-ienumerable-vs-iqueryable-vs-list)
- [Async / Await — 11 Reglas](#async--await--11-reglas)

### Code Quality
- [Clean Code: Clever vs Maintainable](#clean-code-clever-vs-maintainable)
- [Vertical Coding Style](#vertical-coding-style)

### Performance & Memory
- [Memory Leaks en .NET](#memory-leaks-en-net)
- [Task vs ValueTask](#task-vs-valuetask)
- [Stop Using Substring — Span\<T\> y AsSpan()](#stop-using-substring--spant-y-asspan)

### Web & Security
- [ASP.NET Core Fundamentals](#aspnet-core-fundamentals)
- [ASP.NET Core Request Lifecycle](#aspnet-core-request-lifecycle)
- [CORS en ASP.NET Core](#cors-en-aspnet-core)
- [Rate Limiting en .NET](#rate-limiting-en-net)
- [Correlation IDs en ASP.NET Core](#correlation-ids-en-aspnet-core)

### Ecosystem & Tooling
- [Paquetes NuGet Recomendados](#paquetes-nuget-recomendados)

### Quick Reference
- [29 Preguntas Frecuentes de Entrevista](#29-preguntas-frecuentes-de-entrevista)
- [Cheat Sheet para Entrevistas](#cheat-sheet-para-entrevistas)

---

## Conceptos Básicos de C#

### Stack vs Heap

**Stack:** memoria LIFO (Last-In, First-Out) para variables locales y value types. Automáticamente libera memoria cuando la variable sale del scope.

**Heap:** memoria compartida donde viven los objetos (reference types). Requiere Garbage Collection para limpiar referencias no usadas.

```csharp
int age = 30;                          // Stack: value type
Person person = new Person();          // Stack: referencia; Heap: objeto
string name = "John";                  // Stack: referencia; Heap: string
```

---

### Value vs Reference Types

| Característica | Value Type | Reference Type |
|---|---|---|
| **Dónde viven** | Stack | Stack (referencia) + Heap (objeto) |
| **Copia** | Copia el valor completo | Copia la referencia (puntero) |
| **Predeterminado** | `struct`, `int`, `bool`, `enum` | `class`, `string`, `array`, `delegate` |
| **Pasar a método** | Se copia el valor | Se copia la referencia (misma instancia) |

```csharp
// Value Type
int x = 10;
int y = x;
y = 20;
Console.WriteLine(x);  // 10 (sin cambios)

// Reference Type
var list1 = new List<int> { 1, 2, 3 };
var list2 = list1;
list2.Add(4);
Console.WriteLine(list1.Count);  // 4 (ambas apuntan al mismo objeto)
```

---

### Boxing vs Unboxing

**Boxing:** convertir un value type en object (reference type). Requiere allocación de heap y copia de datos.

**Unboxing:** extraer el value type del object. Si el tipo no coincide exactamente, lanza `InvalidCastException`.

```csharp
int x = 42;
object boxed = x;           // Boxing: se crea caja en heap

int unboxed = (int)boxed;   // Unboxing: se extrae el valor

// ❌ Error: tipos no coinciden
// double d = (double)boxed;  // InvalidCastException
```

**⚠️ Evitar boxing innecesario:** impacta performance y aloca memoria.

---

### ref vs out

| Keyword | Propósito | Inicialización | Retorno obligatorio |
|---|---|---|---|
| **ref** | Pasar por referencia; modificar valor original | Debe estar inicializada | No |
| **out** | Retornar múltiples valores | No requiere inicialización | **Sí** |

```csharp
// ref: modificar valor existente
void Update(ref int x) => x += 10;
int age = 30;
Update(ref age);  // age = 40

// out: retornar valores
void GetUserInfo(int id, out string name, out int age)
{
    name = "John";
    age = 30;
}
GetUserInfo(1, out var userName, out var userAge);
```

---

## Keywords Importantes

### out vs ref

**out:** El método **debe** asignar un valor. Ideal para retornar múltiples datos.

```csharp
public bool TryParseInt(string input, out int result)
{
    if (int.TryParse(input, out result)) {
        return true;
    }
    result = 0;  // Obligatorio asignar
    return false;
}

TryParseInt("42", out int number);
```

**ref:** El método puede modificar pero no está obligado a asignar. La variable ya debe estar inicializada.

```csharp
public void Increment(ref int value) => value++;

int x = 10;
Increment(ref x);  // x = 11
```

---

### const vs readonly

**const:** Valor fijo en tiempo de compilación. No puede cambiar. Se inlinea en tiempo de compilación.

**readonly:** Valor fijo después de inicializar (en constructor o al declarar). Se resuelve en runtime.

```csharp
public const string ApiVersion = "v1";        // Compile-time constant
public readonly string ApiKey = "secret123";  // Runtime constant

// const debe ser inicializado al declarar
public const int MaxUsers = 100;

// readonly puede inicializarse en constructor
private readonly ILogger _logger;
public MyClass(ILogger logger)
{
    _logger = logger;  // Asignación única
}
```

**Performance:** `const` es ~13x más rápido que `readonly` porque se resuelve en compile-time.

---

### var vs dynamic

**var:** Tipado estáticamente en compile-time. Tipo se infiere de la asignación. Seguro.

**dynamic:** Tipado dinámicamente en runtime. Refleja propiedades. Menos seguro, más lento.

```csharp
// var: compile-time typing (seguro)
var name = "John";          // string
var age = 30;               // int
// name.Substring(0, 1);     // IntelliSense funciona

// dynamic: runtime typing (flexible pero arriesgado)
dynamic value = "Hello";
// value.NonExistentMethod();  // No error en compile-time, pero error en runtime

// Best for LINQ and reflection
var users = context.Users
    .Where(u => u.IsActive)
    .Select(u => new { u.Name, u.Email })
    .ToList();
```

---

### yield

Permite iterar sobre colecciones de forma lazy sin materializar toda la colección en memoria.

```csharp
// ❌ Sin yield: carga toda la colección
public List<int> GetNumbers()
{
    var list = new List<int>();
    for (int i = 1; i <= 1_000_000; i++) {
        list.Add(i);
    }
    return list;  // 1M items en memoria
}

// ✅ Con yield: lazy evaluation
public IEnumerable<int> GetNumbersLazy()
{
    for (int i = 1; i <= 1_000_000; i++) {
        yield return i;  // Se produce bajo demanda
    }
}

var nums = GetNumbersLazy().Take(10);  // Solo 10 números procesados
```

---

### Equals() vs ==

**==:** Operador sobrecargable. Por defecto compara referencias en `class`, valores en `struct`.

**Equals():** Método virtual, respeta lógica de igualdad del tipo. Más seguro para objetos.

```csharp
// Strings (immutable reference types)
string a = "hello";
string b = "hello";
Console.WriteLine(a == b);       // true (Content equality)
Console.WriteLine(a.Equals(b));  // true

// Objects
var obj1 = new { Name = "John" };
var obj2 = new { Name = "John" };
Console.WriteLine(obj1 == obj2);       // false (reference equality)
Console.WriteLine(obj1.Equals(obj2));  // true (content equality)
```

**Best practice:** Usar `.Equals()` para comparaciones lógicas, `==` para referencias.

---

### enum

Conjunto de constantes predefinidas. Más eficiente que usar strings para estados.

```csharp
public enum UserStatus
{
    Pending,    // 0
    Active,     // 1
    Suspended,  // 2
    Deleted     // 3
}

var status = UserStatus.Active;

// ⚠️ Cuidado: ToString() usa reflection
var name1 = status.ToString();     // Lento, aloca memoria (~24 bytes)

// ✅ Preferir nameof para valores conocidos en compile-time
var name2 = nameof(UserStatus.Active);  // Rápido, sin allocations
```

**Performance:** `nameof` es ~13x más rápido que `.ToString()`.

---

## Delegates y Expresiones Lambda

### Delegates: Action, Func y Predicate

Un **delegate** es un type-safe function pointer. Almacena una referencia a un método y permite pasar métodos como parámetros.

| Tipo | Parámetros | Retorno | Uso | Ejemplo |
|------|-----------|---------|-----|---------|
| **Action** | 0–16 | `void` | Ejecutar una acción sin retorno | `Action<string> log = msg => Console.WriteLine(msg);` |
| **Func** | 0–16 (último es retorno) | `T` | Ejecutar y retornar un valor | `Func<int, int, int> add = (a, b) => a + b;` |
| **Predicate** | 1 | `bool` | Evaluar una condición | `Predicate<int> isEven = x => x % 2 == 0;` |

```csharp
// Action: sin retorno
Action<string> greet = name => Console.WriteLine($"Hola {name}");
greet("Carlos");  // Hola Carlos

// Func: retorna un valor
Func<int, int, int> multiply = (a, b) => a * b;
int result = multiply(3, 4);  // 12

// Predicate: retorna bool
Predicate<int> isPositive = x => x > 0;
bool check = isPositive(-5);  // false

// Delegates en callbacks
public delegate void LogHandler(string message);
public class Logger
{
    public event LogHandler OnLog;
    
    public void LogMessage(string msg)
    {
        OnLog?.Invoke(msg);
    }
}

var logger = new Logger();
logger.OnLog += msg => Console.WriteLine($"[LOG] {msg}");
logger.LogMessage("Conexión establecida");  // [LOG] Conexión establecida
```

---

### Multicast Delegates

Permite asignar múltiples métodos a un delegate usando `+=` y `-=`.

```csharp
Action<string> handlers = null;

handlers += msg => Console.WriteLine($"Handler 1: {msg}");
handlers += msg => Console.WriteLine($"Handler 2: {msg}");

handlers?.Invoke("Evento disparado");
// Output:
// Handler 1: Evento disparado
// Handler 2: Evento disparado

// Remover un handler
handlers -= msg => Console.WriteLine($"Handler 1: {msg}");
```

---

### Delegates vs Events

**Delegate:** Es un tipo, puede ser reasignado (`=`), modificado (`+=`, `-=`) desde cualquier lugar.

**Event:** Encapsula un delegate. Solo puede ser asignado (`+=`, `-=`) desde dentro de la clase que lo define. Protege contra reasignaciones accidentales.

```csharp
public class Button
{
    public delegate void ClickHandler(string clickData);
    
    // ❌ Delegate directo (vulnerable)
    public ClickHandler OnClickDelegate;
    
    // ✅ Event (protegido)
    public event ClickHandler OnClick;
    
    public void Click()
    {
        OnClick?.Invoke("Clicked");
    }
}

var btn = new Button();
btn.OnClick += data => Console.WriteLine($"Click: {data}");
// btn.OnClick = null;  // ❌ Error: no se puede reasignar evento
```

---

## Top 5 Design Patterns en .NET

Los design patterns son soluciones probadas a problemas recurrentes. Su valor está en aplicarlos donde realmente resuelven un problema concreto, no como un fin en sí mismos.

### 1. Dependency Injection

Proveer dependencias desde afuera en lugar de crearlas internamente. Reduce acoplamiento, mejora testabilidad.

```csharp
// ❌ Sin DI (acoplado)
public class UserService
{
    private Database _db = new Database();  // Crea su propia dependencia
    
    public User GetUser(int id) => _db.Query(id);
}

// ✅ Con DI (desacoplado)
public class UserService
{
    private readonly IDatabase _db;
    
    public UserService(IDatabase db)
    {
        _db = db;  // Inyectada
    }
    
    public User GetUser(int id) => _db.Query(id);
}

// En el contenedor DI
builder.Services.AddScoped<IDatabase, Database>();
builder.Services.AddScoped<UserService>();
```

---

### 2. Factory Method

Define una interfaz para crear objetos; las subclases deciden qué instanciar. Útil cuando la creación es compleja o depende de condiciones.

```csharp
// ❌ Sin Factory
public class Logger
{
    public static Logger Create(string type)
    {
        if (type == "file") return new FileLogger();
        if (type == "db") return new DatabaseLogger();
        return new ConsoleLogger();
    }
}

// ✅ Con Factory Pattern
public interface ILogger { }
public class FileLogger : ILogger { }
public class DatabaseLogger : ILogger { }

public class LoggerFactory
{
    public static ILogger CreateLogger(string type) => type switch
    {
        "file" => new FileLogger(),
        "db" => new DatabaseLogger(),
        _ => new ConsoleLogger()
    };
}

var logger = LoggerFactory.CreateLogger("file");
```

---

### 3. Repository Pattern

Interfaz similar a una colección para acceder a datos. Separa lógica de acceso de datos de lógica de negocio.

```csharp
public interface IUserRepository
{
    User GetById(int id);
    List<User> GetAll();
    void Add(User user);
    void Update(User user);
    void Delete(int id);
}

public class UserRepository : IUserRepository
{
    private readonly DbContext _context;
    
    public User GetById(int id) => _context.Users.FirstOrDefault(u => u.Id == id);
    
    public List<User> GetAll() => _context.Users.ToList();
    
    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }
    
    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }
    
    public void Delete(int id)
    {
        var user = GetById(id);
        _context.Users.Remove(user);
        _context.SaveChanges();
    }
}

// Uso con DI
public class UserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public User FetchUser(int id) => _repository.GetById(id);
}
```

---

### 4. Strategy Pattern

Define una familia de algoritmos intercambiables en tiempo de ejecución. Permite cambiar comportamiento dinámicamente sin alterar la estructura principal.

```csharp
public interface IPaymentStrategy
{
    void Pay(decimal amount);
}

public class CreditCardPayment : IPaymentStrategy
{
    public void Pay(decimal amount) => Console.WriteLine($"Pagando ${amount} con tarjeta");
}

public class PayPalPayment : IPaymentStrategy
{
    public void Pay(decimal amount) => Console.WriteLine($"Pagando ${amount} vía PayPal");
}

public class BitcoinPayment : IPaymentStrategy
{
    public void Pay(decimal amount) => Console.WriteLine($"Pagando ${amount} BTC");
}

public class PaymentProcessor
{
    private IPaymentStrategy _strategy;
    
    public void SetPaymentMethod(IPaymentStrategy strategy)
    {
        _strategy = strategy;  // Cambiar estrategia en runtime
    }
    
    public void ProcessPayment(decimal amount)
    {
        _strategy.Pay(amount);
    }
}

// Uso
var processor = new PaymentProcessor();

processor.SetPaymentMethod(new CreditCardPayment());
processor.ProcessPayment(100);  // Pagando $100 con tarjeta

processor.SetPaymentMethod(new BitcoinPayment());
processor.ProcessPayment(100);  // Pagando $100 BTC
```

---

### 5. Singleton Pattern

Garantiza una única instancia de una clase con punto de acceso global. Ideal para recursos compartidos: configuración, logging, caché.

```csharp
// ❌ Singleton naive (no thread-safe)
public class AppSettings
{
    private static AppSettings _instance;
    
    public static AppSettings Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AppSettings();
            return _instance;
        }
    }
    
    private AppSettings() { }
}

// ✅ Singleton con Lazy<T> (thread-safe, lazy initialization)
public class AppSettings
{
    private static readonly Lazy<AppSettings> _instance = 
        new Lazy<AppSettings>(() => new AppSettings());
    
    public static AppSettings Instance => _instance.Value;
    
    private AppSettings() { }
}

// ✅ Singleton con .NET DI (recomendado)
builder.Services.AddSingleton<AppSettings>();

// Uso
var settings = AppSettings.Instance;
// o con DI
public class MyService
{
    private readonly AppSettings _settings;
    
    public MyService(AppSettings settings)
    {
        _settings = settings;
    }
}
```

---

## Principios de Diseño

### KISS — Keep It Simple, Stupid

El código simple tiene menos bugs y acelera el desarrollo.

```csharp
// ❌ Verboso e innecesario
if (user != null && user.IsActive == true && user.IsDeleted == false)
{
    // procesar
}

// ✅ Simple y legible
if (user?.IsActive == true)
{
    // procesar
}
```

**Sobre-ingeniería a evitar:** apilar capas innecesarias (Controller → Service → Manager → Helper → Utility) cuando Controller → Service es suficiente.

**Regla de oro:** código simple = menos bugs. Código legible = desarrollo más rápido.

---

### Clean Code: Clever vs Maintainable

> *Clean code is not about writing less code. It's about writing code that scales.*

```csharp
// ❌ Clever pero difícil de mantener
public void ProcessData(string d)
{
    var x = d.Split(',');
    for (int i = 0; i < x.Length; i++)
    {
        if (x[i].Contains("error"))
        {
            Console.WriteLine("Found error!");
        }
    }
}

// ✅ Limpio y mantenible
public void ProcessLogs(string logData)
{
    var logEntries = ParseLogEntries(logData);
    
    foreach (var entry in logEntries)
    {
        if (IsError(entry))
        {
            Console.WriteLine("Found error!");
        }
    }
}

private string[] ParseLogEntries(string logData) => logData.Split(',');

private bool IsError(string logEntry) => logEntry.Contains("error");
```

---

### Vertical Coding Style

Preferir el estilo vertical al encadenar LINQ u otras llamadas fluidas.

```csharp
// ❌ Horizontal (difícil de leer)
var users = customers.Where(c => c.IsActive).OrderBy(c => c.Age).ThenBy(c => c.Name).Select(c => c.Email).ToList();

// ✅ Vertical (limpio y legible)
var users = customers
    .Where(c => c.IsActive)
    .OrderBy(c => c.Age)
    .ThenBy(c => c.Name)
    .Select(c => c.Email)
    .ToList();
```

---

## Memory Leaks en .NET

> *"The Garbage Collector handles memory, so memory leaks aren't possible."* — **FALSO**

El GC gestiona allocación, pero no puede liberar objetos que aún están referenciados. La mayoría de memory leaks en .NET vienen de referencias no removidas.

### Conceptos Clave

**Garbage Collector:** Gestiona la allocación de memoria en el heap. Pero **no puede recolectar objetos que aún están referenciados.**

Si un objeto está referenciado desde:
- Un static field
- Un event subscriber no removido
- Un objeto en caché sin expiración
- Una closure capturando referencias grandes

...el GC **no puede limpiar**, causando memory leak.

---

### Causas Comunes de Memory Leaks

#### 1. Event Subscription Leaks

```csharp
// ❌ Memory leak: suscriptor no removido
public class View
{
    public View(Service service)
    {
        service.OnDataChanged += HandleDataChanged;  // Nunca se desuscribe
    }
    
    private void HandleDataChanged() { }
}

// ✅ Solución: implementar IDisposable
public class View : IDisposable
{
    private readonly Service _service;
    
    public View(Service service)
    {
        _service = service;
        _service.OnDataChanged += HandleDataChanged;
    }
    
    private void HandleDataChanged() { }
    
    public void Dispose()
    {
        _service.OnDataChanged -= HandleDataChanged;  // Desuscribirse
    }
}

// O usar WeakEventManager si está disponible
```

---

#### 2. Static References

```csharp
// ❌ Static dictionary reteniendo referencias
public static class Cache
{
    private static Dictionary<string, byte[]> _cache = new();
    
    public static void Set(string key, byte[] data)
    {
        _cache[key] = data;  // Nunca se limpia automáticamente
    }
}

// ✅ Solución: implementar límite de tamaño o expiración
public class SmartCache
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly int _maxSize;
    
    public SmartCache(int maxSize = 100)
    {
        _maxSize = maxSize;
    }
    
    public void Set(string key, object value)
    {
        if (_cache.Count >= _maxSize)
        {
            var oldest = _cache.OrderBy(kv => kv.Value.CreatedAt).First();
            _cache.Remove(oldest.Key);
        }
        
        _cache[key] = new CacheEntry
        {
            Value = value,
            CreatedAt = DateTime.UtcNow
        };
    }
    
    private class CacheEntry
    {
        public object Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
```

---

#### 3. Improper Caching

```csharp
// ❌ Caché sin límites
public class DataService
{
    private static Dictionary<int, DataModel> _cache = new();
    
    public DataModel GetData(int id)
    {
        if (!_cache.ContainsKey(id))
        {
            _cache[id] = FetchFromDatabase(id);  // Crece infinitamente
        }
        return _cache[id];
    }
    
    private DataModel FetchFromDatabase(int id) => new();
}

// ✅ Solución: usar MemoryCache con expiración
public class DataService
{
    private readonly IMemoryCache _cache;
    
    public DataService(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public DataModel GetData(int id)
    {
        return _cache.GetOrCreate(id, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return FetchFromDatabase(id);
        });
    }
    
    private DataModel FetchFromDatabase(int id) => new();
}
```

---

#### 4. Large Object Heap (LOH) Pressure

```csharp
// ❌ Allocaciones grandes frecuentes
public byte[] ReadLargeFile()
{
    using var file = new FileStream("largefile.bin", FileMode.Open);
    var buffer = new byte[file.Length];  // >85KB en LOH
    file.Read(buffer, 0, (int)file.Length);
    return buffer;
}

// ✅ Solución: usar ArrayPool para reutilizar buffers
using var file = new FileStream("largefile.bin", FileMode.Open);
var buffer = ArrayPool<byte>.Shared.Rent((int)file.Length);

try
{
    file.Read(buffer, 0, (int)file.Length);
    // Procesar buffer
}
finally
{
    ArrayPool<byte>.Shared.Return(buffer);  // Retornar al pool
}
```

---

#### 5. Task / Async Retention

```csharp
// ❌ Tareas incompletas reteniendo closures
public void StartBackgroundTask()
{
    var largeData = new byte[10_000_000];  // 10MB
    
    _ = Task.Run(async () =>
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
        Console.WriteLine(largeData.Length);  // Closure reteniendo largeData
    });
}

// ✅ Solución: permitir que largeData sea GC'd
public async Task StartBackgroundTask()
{
    var largeData = new byte[10_000_000];
    var size = largeData.Length;
    
    await Task.Run(async () =>
    {
        await Task.Delay(TimeSpan.FromMinutes(1));
        Console.WriteLine(size);  // Solo captura el int, no el array
    });
}
```

---

### Cómo Detectar Memory Leaks

| Herramienta | Especialidad |
|---|---|
| **PerfView** | Heap analysis, GC investigation, allocation tracking |
| **dotMemory / dotTrace** | Snapshot comparisons, dominator analysis, retained references |
| **Visual Studio Diagnostic Tools** | Local debugging, heap snapshots, allocation analysis |
| **Rider Profiler** | Memory profiling, retained object graphs |

---

### Practical Advice

✅ **Dispose resources correctly:** Implementar `IDisposable` para recursos no manejados (archivos, conexiones, eventos).

✅ **Unsubscribe from events:** Siempre desuscribirse de eventos públicos cuando ya no se necesitan.

✅ **Avoid unbounded caches:** Usar expiración, size limits, eviction policies.

✅ **Monitor allocation rates:** Medir tendencias, no solo picos. Un memory leak estable crece silenciosamente.

✅ **Profile before optimizing:** Medir primero, especular después.

---

### Rule of Thumb

Si la memoria solo sube bajo carga normal y **nunca se estabiliza**, probablemente tengas un **retention problem** (no un traffic problem).

```
Memory Usage
    ↑
    │     Leak (sube continuamente)
    │    /
    │   /
    │  /─────────────────────────
    │ /
    └─────────────────────────────→ Time
    
    Memory Usage
    ↑
    │  ─────────────────────────
    │ /                         \  Healthy (sube, baja, estable)
    │/
    └─────────────────────────────→ Time
```

---

## Task vs ValueTask

### Conceptos Clave

**Task:** Heap allocation (reference type). Siempre aloca, incluso si el resultado está disponible inmediatamente.

**ValueTask:** Stack allocation (value type) cuando el resultado está listo sincronicamente. Zero allocation en hot path.

---

### Comparación Visual

```
Cache Lookup → Async Method → Returned Result

TASK<T>
├─ Allocation: Siempre allocate
├─ Heap allocation: ↑ (managed heap)
├─ Heap pressure: ↑↑ (GC activity)
├─ GC collections: ↑↑
└─ Stack path: Fallback Task<T> (if async)

VALUETASK<T>
├─ Allocation: Zero allocation on sync completion
├─ Heap allocation: Only if async (Hot path optimization)
├─ Heap pressure: ↓ (heap allocations only for actual async cases)
├─ GC collections: ↓ (fewer collections)
└─ Stack path: Fast path result (no Task wrapper)
```

---

### Código Comparativo

```csharp
// ❌ Task<T> - Allocates every call
public Task<string> GetAsync()
{
    return Task.FromResult("cached");  // Allocates Task<T> even if result is ready
}

// ✅ ValueTask<T> - Zero allocation on sync completion
public ValueTask<string> GetAsyncOptimized()
{
    return new ValueTask<string>("cached");  // No allocation, stack-based
}

// Ejemplo real: caché
public async ValueTask<User> GetUserAsync(int id)
{
    // Si está en caché, retorna sincronicamente sin allocar
    if (_cache.TryGetValue(id, out var user))
    {
        return user;  // Zero allocation
    }
    
    // Si no, hacer I/O async
    var fetchedUser = await _database.QueryAsync(id);
    _cache[id] = fetchedUser;
    return fetchedUser;  // Hot path optimization
}
```

---

### Cuándo Usar Cada Uno

| Caso | Usa |
|---|---|
| Métodos que **siempre son async** (I/O, DB queries) | **Task** |
| Métodos que pueden ser sync o async (caché + I/O) | **ValueTask** |
| APIs públicas que no controlas frecuencia de sincronía | **Task** |
| Hot path interno con caché/resultados ready | **ValueTask** |
| Legacy code / frameworks que esperan Task | **Task** |

---

### ⚠️ Advertencia Sobre ValueTask

**No múltiplos awaits:** `ValueTask` no debe ser awaited múltiples veces (puede ser un issue si es un `struct`).

```csharp
// ❌ Riesgoso
var vt = GetAsyncOptimized();
var result1 = await vt;
var result2 = await vt;  // Potencialmente undefined behavior

// ✅ Correcto: await una sola vez
var result = await GetAsyncOptimized();
```

**Boxing:** Si no hay sync path, ValueTask puede boxing en la misma situación que Task.

---

## Stop Using Substring — Span\<T\> y AsSpan()

> *Faster parsing. Zero allocations. Maximum performance.*

`Span<T>` actúa como una ventana sobre memoria existente: permite leer un segmento de string sin copiarlo ni alocar en heap.

### El Problema con Substring()

```csharp
string log = "LOG_2026-05-27_ACTIVE";

// ❌ OLD WAY: aloca un nuevo string en heap
string yearStr = log.Substring(4, 4);
int year = int.Parse(yearStr);
// → Crea "2026" como objeto nuevo en memoria
```

**Lo que pasa en memoria:**

```
Original string (log):  [L][O][G][_][2][0][2][6][-][0][5][-][2][7][_][A][C][T][I][V][E]
                                      ↓
New string (yearStr):   [2][0][2][6]   ← HEAP ALLOCATION (objeto nuevo)
```

### La Solución Moderna: AsSpan()

```csharp
string log = "LOG_2026-05-27_ACTIVE";

// ✅ MODERN WAY: zero allocation
ReadOnlySpan<char> yearSpan = log.AsSpan(4, 4);
int year = int.Parse(yearSpan);
// → Solo una ventana sobre la memoria existente, sin copiar nada
```

**Lo que pasa en memoria:**

```
Original string (log):  [L][O][G][_][2][0][2][6][-][0][5][-][2][7][_][A][C][T][I][V][E]
                                      ↑           ↑
                         ReadOnlySpan<char> (window) — Start: 4, Length: 4
                         No new string. Just a window.
```

### Comparación de Performance

| Aspecto | Substring() | AsSpan() |
|---|---|---|
| **Allocaciones** | Nueva string en heap | Zero allocations |
| **GC pressure** | ↑ (más trabajo para el GC) | ↓ (no hay nada que recolectar) |
| **Velocidad** | Base | Hasta 2-3x más rápido |
| **Memoria** | Proporcional a tamaño extraído | Sin costo adicional |
| **Parsing** | Lento en workloads grandes | Óptimo para hot paths |

### Cuándo Usar Cada Uno

```csharp
// ✅ Usar Substring() cuando:
// - El resultado se almacena como string en una variable de larga vida
// - Se pasa a una API que requiere string concreto
string key = input.Substring(0, 8);  // Se guarda, se serializa, etc.

// ✅ Usar AsSpan() cuando:
// - Solo necesitás leer/parsear el segmento de forma inmediata
// - Estás en un hot path (loop, parsing masivo, bajo de latencia)
// - El resultado va directo a int.Parse, double.Parse, comparaciones, etc.
ReadOnlySpan<char> segment = input.AsSpan(0, 8);
bool isValid = segment.SequenceEqual("EXPECTED");
```

### Regla de Oro

> Usar `Span<T>` para high-performance text parsing: menos memoria, más velocidad, menos GC pressure.

---

## Colecciones: IEnumerable vs IQueryable vs List

| Característica | IEnumerable | IQueryable | List |
|---|---|---|---|
| **Definición** | Interfaz para iterar objetos en memoria | Extiende IEnumerable; representa query en una fuente de datos | Clase concreta que almacena objetos |
| **Namespace** | `System.Collections.Generic` | `System.Linq` | `System.Collections.Generic` |
| **Propósito** | Iteración en memoria | Operaciones query traducidas (SQL, etc.) | Almacenamiento y manipulación en memoria |
| **Ejecución** | En memoria (eager) | En la fuente de datos (deferred) | En memoria (eager) |
| **Ejemplo** | `var nums = new List<int> { 1, 2, 3 }.AsEnumerable();` | `var query = context.Users.Where(u => u.IsActive);` | `var list = new List<string> { "A", "B", "C" };` |
| **Performance** | Menos eficiente para grandes datasets (fetch all, then filter) | Más eficiente para datos remotos (filter at source) | Eficiente para colecciones pequeñas en memoria |
| **Herencia** | Base | Extiende IEnumerable | Implementa IEnumerable |

---

### Ejemplos Prácticos

```csharp
// ❌ Ineficiente: descarga 1M de registros y luego filtra en memoria
IEnumerable<User> users = dbContext.Users.AsEnumerable();
var activeUsers = users.Where(u => u.IsActive).ToList();  // 1M → 100K en memoria

// ✅ Eficiente: filtra en la BD (SQL WHERE clause)
IQueryable<User> users = dbContext.Users;
var activeUsers = users.Where(u => u.IsActive).ToList();  // Envía SQL: SELECT * FROM Users WHERE IsActive = 1

// LINQ to Objects (en memoria)
var numbers = new List<int> { 1, 2, 3, 4, 5 };
var evens = numbers
    .Where(n => n % 2 == 0)        // IEnumerable
    .Select(n => n * 2)
    .ToList();

// LINQ to SQL/EF Core (en BD)
var users = dbContext.Users
    .Where(u => u.Age > 18)        // IQueryable - translates to SQL
    .OrderBy(u => u.Name)
    .Select(u => u.Email)
    .ToList();
```

---

### LINQ Specifics

#### OrderBy vs ThenBy

```csharp
// ❌ Incorrecto: múltiples OrderBy se pisan entre sí
var products = list
    .OrderBy(p => p.Name)      // Ordena por Name
    .OrderBy(p => p.Price)     // ¡Reinicia el orden! Descarta Name
    .OrderBy(p => p.Size);     // ¡Reinicia el orden! Descarta Price

// ✅ Correcto: OrderBy + ThenBy
var products = list
    .OrderBy(p => p.Name)      // Primario
    .ThenBy(p => p.Price)      // Secundario
    .ThenBy(p => p.Size);      // Terciario
```

---

#### GroupBy vs CountBy (.NET 9)

Cuando solo interesa el conteo por clave, `CountBy` es más eficiente que `GroupBy`.

```csharp
// ❌ Legacy: GroupBy materializa grupos intermedios
var stats = orders
    .GroupBy(o => o.Status)
    .Select(g => new {
        Status = g.Key,
        Count = g.Count()
    });

// ✅ Moderno (.NET 9): CountBy single pass
var stats = orders.CountBy(o => o.Status);  // Más eficiente, sin materializar grupos
```

---

## Async / Await — 11 Reglas

1. **Nunca bloquees código async.** Usá `await`, no `.Result` o `.Wait()`
   ```csharp
   // ❌ Bloquea thread
   var result = asyncMethod().Result;
   
   // ✅ Async all the way
   var result = await asyncMethod();
   ```

2. **Nunca retornes `async void` para Tasks.** Solo para Event Handlers.
   ```csharp
   // ❌ Imposible awaitar o manejar excepciones
   public async void ProcessData() { }
   
   // ✅ Retorna Task
   public async Task ProcessData() { }
   ```

3. **Ejecutá tareas independientes en paralelo con `Task.WhenAll`**
   ```csharp
   // ✅ Paralelo
   var task1 = FetchUserAsync(1);
   var task2 = FetchUserAsync(2);
   await Task.WhenAll(task1, task2);
   ```

4. **No envuelvas I/O async en `Task.Run`**
   ```csharp
   // ❌ Innecesario
   await Task.Run(() => await dbContext.Users.ToListAsync());
   
   // ✅ Directo
   await dbContext.Users.ToListAsync();
   ```

5. **Usá `ConfigureAwait(false)` en código de librería**
   ```csharp
   // En librerías: libera el context
   await httpClient.GetStringAsync(url).ConfigureAwait(false);
   ```

6. **Mantené async en toda la cadena de llamadas** (*async all the way up*)

7. **Siempre observá y manejá excepciones de tasks**
   ```csharp
   var task = SomeAsync();
   try
   {
       var result = await task;
   }
   catch (Exception ex) { }
   ```

8. **No marques métodos como async si no lo necesitan**
   ```csharp
   // ❌ Async innecesario
   public async Task<string> GetString() => "hello";
   
   // ✅ Sincrónico
   public string GetString() => "hello";
   ```

9. **Pasá `CancellationToken` a operaciones de larga duración**
   ```csharp
   public async Task ProcessAsync(CancellationToken ct = default)
   {
       await Task.Delay(5000, ct);
   }
   ```

10. **No hagas `await` dentro de loops si el trabajo puede correr en paralelo**
    ```csharp
    // ❌ Secuencial
    foreach (var id in ids)
        await FetchAsync(id);
    
    // ✅ Paralelo
    await Task.WhenAll(ids.Select(FetchAsync));
    ```

11. **Usá `Task.WhenEach` para streamear resultados concurrentemente** (.NET 9)
    ```csharp
    await foreach (var result in Task.WhenEach(tasks))
    {
        Console.WriteLine(result);  // Procesa en orden de completación
    }
    ```

---

## ASP.NET Core Fundamentals

### Middleware Pipeline

El middleware es un componente que procesa solicitudes HTTP. El orden importa.

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Agregar servicios
builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddAuthentication();

var app = builder.Build();

// Configurar pipeline (orden es crítico)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();           // 1. Redirigir a HTTPS
app.UseRouting();                    // 2. Determinar qué endpoint ejecutar
app.UseCors();                       // 3. CORS
app.UseAuthentication();             // 4. Autenticar usuario
app.UseAuthorization();              // 5. Autorizar usuario
app.MapControllers();                // 6. Mapear controllers

app.Run();
```

---

### Dependency Injection Built-in

```csharp
// Registrar servicios
builder.Services.AddTransient<IUserService, UserService>();     // Nueva instancia cada vez
builder.Services.AddScoped<IDatabase, Database>();              // Una por request HTTP
builder.Services.AddSingleton<ICache, MemoryCache>();           // Una para toda la app

// Inyectar en controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _userService.GetUserAsync(id);
        return user == null ? NotFound() : Ok(user);
    }
}
```

---

### Model Binding y Validation

```csharp
public class CreateUserRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Range(18, 120)]
    public int Age { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // ASP.NET Core valida automáticamente
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        // request es válido
        return Created($"/users/{request.Email}", request);
    }
}
```

---

### Controllers vs Routing

```csharp
// Atributo routing (moderno, flexible)
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById(int id) { }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request) { }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request) { }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) { }
}

// Rutas generadas:
// GET    /api/users/{id}
// POST   /api/users
// PUT    /api/users/{id}
// DELETE /api/users/{id}
```

---

## ASP.NET Core Request Lifecycle

> *From Incoming Request to Final Response*

El flujo completo de una solicitud HTTP en ASP.NET Core pasa por capas bien definidas, cada una con responsabilidad clara.

### Flujo Completo

```
Client (browser / app)
        │
        ▼
┌─────────────────────────────────────────────┐
│           Middleware Pipeline               │
│  Logging · Exception Handling · CORS · Compression │
└─────────────────────────────────────────────┘
        │
        ▼
   Authentication          ← Valida identidad del usuario
        │
        ▼
   Authorization           ← Verifica permisos
        │
        ▼
   Routing & Endpoint Mapping  ← Determina qué controller/action ejecutar
        │
        ▼
   Controller              ← Recibe y delega el request
        │
        ▼
   Service Layer           ← Business logic
        │
        ▼
   Repository Layer        ← Data Access (DB, APIs externas)
        │         │
        ▼         ▼
   Database    External APIs & Services
        │
        ▼
   Response (JSON / Data / Status Code)
        │
        ▼
      Client
```

### Las Tres Fases Clave

**Fase 1 — Middleware (antes del request):** intercepta la petición antes de llegar a la lógica. Aquí se registran logs, se manejan excepciones globales, se aplica compresión, y se valida CORS. El orden de registro importa.

**Fase 2 — Security:** Authentication verifica *quién* es el usuario (JWT, Cookie, API Key). Authorization verifica *qué puede hacer*. Authentication debe correr antes que Authorization.

**Fase 3 — Execution:** el Controller delega al Service Layer (business logic), que a su vez accede a datos vía Repository. Esta separación en capas permite testear cada parte de forma independiente.

### Pipeline en Código

```csharp
// El orden de los middlewares refleja el flujo del diagrama
var app = builder.Build();

// Middleware transversal (siempre primero)
app.UseExceptionHandler("/error");
app.UseHttpsRedirection();

// Security (en este orden exacto)
app.UseAuthentication();   // ¿Quién sos?
app.UseAuthorization();    // ¿Qué podés hacer?

// Routing (después de security)
app.UseRouting();
app.MapControllers();

app.Run();
```

### Cómo se Relacionan las Capas

| Capa | Responsabilidad | En código |
|---|---|---|
| **Middleware** | Logging, CORS, compresión, errores globales | `app.UseX()` |
| **Authentication** | Validar token/cookie, poblar `User` | `[Authorize]`, JWT middleware |
| **Authorization** | Verificar roles/policies | `[Authorize(Roles = "Admin")]` |
| **Controller** | Mapear HTTP a lógica de negocio | `[ApiController]` |
| **Service Layer** | Business logic pura | Clases de servicio inyectadas |
| **Repository** | Abstraer acceso a datos | `IRepository<T>` |
| **Data Layer** | BD + APIs externas | EF Core, Dapper, HttpClient |

---

## CORS en ASP.NET Core

CORS (Cross-Origin Resource Sharing) es un mecanismo de seguridad que restringe peticiones HTTP entre orígenes. Una política permisiva expone la API a cualquier cliente web, representando un riesgo real en producción.

```csharp
// ❌ Política insegura — permite cualquier origen, header y método
builder.Services.AddCors(options =>
{
    options.AddPolicy("BadPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ Política segura — orígenes, métodos y headers explícitos
builder.Services.AddCors(options =>
{
    options.AddPolicy("SecurePolicy", policy =>
    {
        policy.WithOrigins(
                  "https://myapp.com",
                  "https://admin.myapp.com"
              )
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();  // Si necesitas cookies/auth
    });
});

var app = builder.Build();
app.UseCors("SecurePolicy");
```

---

## Rate Limiting en .NET

Rate Limiting protege APIs contra abuso, ataques de fuerza bruta y sobrecarga. Desde .NET 7, el framework incluye soporte nativo sin librerías externas.

### Fixed Window Limiter

Ventana de tiempo fija; reinicia el contador al terminar cada intervalo.

```csharp
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 10;        // 10 requests
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
});

var app = builder.Build();
app.UseRateLimiter();

app.MapGet("/api/users", () => "OK")
    .RequireRateLimiting("fixed");
```

---

### Sliding Window Limiter

Ventana deslizante; distribuye el límite en segmentos solapados, evitando picos al borde de la ventana.

```csharp
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
    {
        options.PermitLimit = 10;
        options.Window = TimeSpan.FromSeconds(10);
        options.SegmentsPerWindow = 2;   // Divide en 2 segmentos
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 5;
    });
});

var app = builder.Build();
app.UseRateLimiter();
```

**Diferencia:** Fixed Window puede permitir ráfagas de hasta 2x el límite al cruzar bordes. Sliding Window lo evita distribuyendo el conteo.

---

## Correlation IDs en ASP.NET Core

Un Correlation ID es un identificador único asignado a cada request entrante. Permite trazar esa request a través de todo el sistema (gateway, API, servicios downstream, logs).

### Por Qué Importa

✅ Debugging más rápido
✅ Log tracing más simple
✅ Mejor monitoreo
✅ Root cause analysis simplificado
✅ Esencial en sistemas distribuidos

### Cómo Funciona

```
Client → (sin ID) → API Gateway → (ID: abc123) → ASP.NET Core API
                                                        ├─ ID: abc123 → SQL Database
                                                        ├─ ID: abc123 → Redis
                                                        ├─ ID: abc123 → Payment Service
                                                        └─ ID: abc123 → Email Service

Response ← (Header: X-Correlation-ID: abc123) ← API Gateway
```

Si tu API llama a otros servicios, propagá el mismo header `X-Correlation-ID` para poder rastrear el request across multiple services.

### Middleware para Generar el Correlation ID

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var correlationId = context.Request.Headers["X-Correlation-ID"]
        .FirstOrDefault() ?? Guid.NewGuid().ToString();

    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;

    using (_logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId
    }))
    {
        await _next(context);
    }
}
```

`BeginScope` agrega el `CorrelationId` a todos los logs generados dentro del request, sin tener que pasarlo manualmente a cada llamada de logging.

### Ejemplo de Log Output

```
2024-05-19 10:15:23 INF [abc123] Request started
2024-05-19 10:15:23 INF [abc123] Getting customer details
2024-05-19 10:15:23 INF [abc123] Data fetched from DB
2024-05-19 10:15:23 INF [abc123] Calling Payment Service
2024-05-19 10:15:24 INF [abc123] Payment Service responded
2024-05-19 10:15:24 INF [abc123] Email notification sent
2024-05-19 10:15:24 INF [abc123] Request completed
```

Todas las líneas comparten el mismo `abc123`, lo que permite filtrar el log completo de un request específico entre múltiples servicios.

---

## Paquetes NuGet Recomendados

Selección consolidada de paquetes del ecosistema .NET para producción. Divididos por categoría según su función principal.

### Logging & Observability

**Serilog** — Logging estructurado. Sinks para Elasticsearch, Seq, Splunk, consola. Estándar de facto para apps .NET en producción.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("User {UserId} logged in at {Time}", userId, DateTime.UtcNow);
```

**OpenTelemetry** — Observabilidad completa: métricas, trazas distribuidas y logs. Integra con Jaeger, Prometheus, Grafana y Azure Monitor.

### Data Access

**Entity Framework Core** — ORM oficial de Microsoft. LINQ, migrations, soporte para SQL Server, MySQL, PostgreSQL y más. Simplifica CRUD y manejo de relaciones.

**Dapper** — Micro-ORM liviano. Control total sobre SQL, alto performance. Ideal para queries complejas o cuando EF Core es excesivo.

```csharp
// Dapper: SQL explícito, resultado tipado
var users = await connection.QueryAsync<User>(
    "SELECT * FROM Users WHERE IsActive = @active",
    new { active = true }
);
```

### Validation

**FluentValidation** — Validación de modelos con sintaxis fluida. Más organizado y testeable que DataAnnotations para reglas complejas.

```csharp
public class UserValidator : AbstractValidator<CreateUserRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

### Caching

**FusionCache** — Caching de alta performance con fail-safe incorporado, soporte distribuido (Redis), stampede prevention y timeouts configurables.

```csharp
var user = await cache.GetOrSetAsync(
    $"user:{id}",
    async _ => await db.GetUserAsync(id),
    TimeSpan.FromMinutes(5)
);
```

### Background Jobs

**Hangfire** — Background job processing con panel de control web, reintentos automáticos, jobs recurrentes (cron) y persistencia en BD.

```csharp
// Fire-and-forget
BackgroundJob.Enqueue(() => SendWelcomeEmail(userId));

// Recurrente
RecurringJob.AddOrUpdate("cleanup", () => CleanupOldRecords(), Cron.Daily);
```

### Testing

**xUnit** — Framework de unit testing estándar para .NET. Integrado con dotnet CLI y CI/CD pipelines.

**TestContainers** — Levanta dependencias reales en Docker para integration tests (SQL Server, Redis, RabbitMQ). Elimina mocks frágiles en tests de integración.

```csharp
public class UserRepositoryTests : IAsyncLifetime
{
    private readonly MsSqlContainer _db = new MsSqlBuilder().Build();
    
    public async Task InitializeAsync() => await _db.StartAsync();
    public async Task DisposeAsync() => await _db.StopAsync();
    
    [Fact]
    public async Task GetUser_ReturnsCorrectUser()
    {
        // Test contra SQL Server real en Docker
    }
}
```

### API Documentation

**Scalar** — UI moderna para OpenAPI/Swagger. Reemplaza la UI default de Swashbuckle con una experiencia más limpia para equipos.

### Email

**MailKit** — Envío de emails con soporte completo para SMTP, IMAP y POP3. Reemplaza el obsoleto `System.Net.Mail`.

### Resumen por Caso de Uso

| Necesidad | Paquete(s) |
|---|---|
| Logging estructurado | Serilog |
| Observabilidad / trazas | OpenTelemetry |
| ORM completo | Entity Framework Core |
| SQL con control total | Dapper |
| Validación de modelos | FluentValidation |
| Caching avanzado | FusionCache |
| Jobs en background | Hangfire |
| Unit testing | xUnit + Moq |
| Integration testing con Docker | TestContainers |
| Documentación API | Scalar (o Swashbuckle) |
| Envío de emails | MailKit |
| Mediator / CQRS | MediatR |
| Resilience / retry | Polly |
| DTO mapping | AutoMapper |

---

## 29 Preguntas Frecuentes de Entrevista

### Fundamentales

1. **Contate sobre ti.** Habla de background, skills, experience, qué te apasiona de .NET.

2. **Explicá tu proyecto reciente.** Objetivo, tu rol, tecnologías usadas, desafíos, resultados.

3. **¿Cuál es la diferencia entre .NET Framework y .NET Core?** Framework es Windows-only, viejo. Core es cross-platform, moderno, open-source.

4. **¿Qué son CLR y CTS?** CLR es el runtime que ejecuta código .NET. CTS define tipos que el runtime entiende.

5. **¿Cuál es la diferencia entre async/await y Task?** async/await es syntactic sugar para manejar Tasks. Task representa trabajo asincrónico.

6. **¿Cómo conectás frontend con backend?** Através de APIs (RESTful services) usando HTTP/HTTPS — JSON data exchange.

7. **¿Cómo mantenés secretos en un proyecto?** Usa appsettings.json (dev) o Azure Secrets / AWS Secrets Manager (prod). Nunca commits secrets.

8. **¿Qué son secure ways para guardar secret keys?** Azure Key Vault, AWS Secrets Manager, User Secrets (dev), environment variables.

9. **¿Cuál es la diferencia entre interfaces y clases abstractas?** Interface: contrato. Abstract class: base con comportamiento parcial.

10. **¿Qué es LINQ?** Language Integrated Query. Permite queries type-safe sobre colecciones, BD, XML.

### SQL & Database

11. **¿Cuál es la diferencia entre GROUP BY y HAVING?** GROUP BY agrupa. HAVING filtra grupos (como WHERE para grupos).

12. **¿Qué es GROUP BY en SQL?** Agrupa rows con valores iguales en una columna, aplica funciones de agregado (COUNT, SUM, MAX).

13. **¿Qué es HAVING en SQL?** Filtra grupos después de GROUP BY (como WHERE pero para grupos).

14. **¿Cómo configuras una BD en backend?** Usa Entity Framework Core: `appsettings.json` (connection string) + DbContext + Migrations.

15. **¿Cuál es la diferencia entre tablas normalizadas y denormalizadas?** Normalizadas: sin redundancia, joins. Denormalizadas: redundancia, faster reads.

16. **¿Qué es una Foreign Key?** Referencia a la primary key de otra tabla. Mantiene referential integrity.

17. **¿Cuál es la diferencia entre PRIMARY KEY y UNIQUE?** PRIMARY: único + NOT NULL. UNIQUE: único pero puede ser NULL.

18. **¿Cuándo ves un WITH clause en SQL?** Common Table Expression (CTE). Define una query temporal reutilizable.

### Async & Performance

19. **¿Cuál es la diferencia entre Task y ValueTask?** Task aloca siempre. ValueTask: zero alloc si resultado está listo (caché).

20. **¿Cuál es la diferencia entre Garbage Collection?** Gestiona memoria automáticamente. Pero NO recolecta objetos aún referenciados (causa memory leaks).

21. **¿Cómo reduces traffic/performance issues?** Caching, database indexing, async processing, CDN load balancing.

### Design & Architecture

22. **¿Cuál es la diferencia entre Interfaces y Abstracciones?** Abstraction: ocultar complejidad. Interface: contrato sin implementación.

23. **¿Cuál es la diferencia entre Dependency Injection y Service Locator?** DI: inyecta dependencias vía constructor (loose coupling). Service Locator: busca dependencias en runtime (tight coupling).

24. **¿Puedes explicar MVC Architecture?** Model (datos), View (UI), Controller (lógica). Separación de responsabilidades.

25. **¿Cuál es la diferencia entre Routing y MVC Architecture?** Routing: mapea URLs a controllers/actions. MVC: estructura de aplicación.

26. **¿Cuál es el uso del img tag en HTML?** Embeber imágenes. `<img src="..." alt="..." />`.

27. **¿Cuál es la diferencia entre Abstraction y Interface?** Abstraction: concepto (ocultar detalles). Interface: implementación de contrato (qué hace, no cómo).

### Security

28. **¿Cuál es la diferencia entre Authentication y Authorization?** Authentication: verificar identidad (login). Authorization: verificar permisos (qué puede hacer).

29. **¿Qué es Boxing y Unboxing?** Boxing: value type → object. Unboxing: object → value type. Cuidado: performance hit.

---

## Cheat Sheet para Entrevistas

### C# Fundamentals

```
OOP principles     → Encapsulation, Inheritance, Polymorphism, Abstraction
Value types        → int, double, bool, struct, enum (Stack)
Reference types    → class, string, array, delegate (Heap)
null coalescing    → x ?? y (si x null, retorna y)
safe navigation    → x?.Property (no error si x null)
switch expr        → condition ? true_value : false_value
```

### ASP.NET Core

```
Middleware         → Request pipeline processing
Dependency Inject  → Transient, Scoped, Singleton
Model Binding      → [FromBody], [FromQuery], [FromRoute]
HTTP Methods       → GET (read), POST (create), PUT (update), DELETE (delete)
Status Codes       → 200 OK, 201 Created, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 500 Internal Error
```

### Database & LINQ

```
CRUD               → Create, Read, Update, Delete
LINQ operators     → Where, Select, OrderBy, GroupBy, Join, Take, Skip
Deferred exec      → LINQ no ejecuta hasta ToList() / foreach
IQueryable         → Query traducida a SQL
IEnumerable        → Query en memoria
```

### Design Patterns

```
DI                 → Loose coupling, Testability
Factory            → Complex object creation
Repository        → Data access abstraction
Strategy          → Switchable algorithms
Singleton         → Single instance, global access
```

### Async Best Practices

```
Never .Result       → Causa deadlocks
async all the way   → Propagate async up the stack
ConfigureAwait      → Library code: ConfigureAwait(false)
WhenAll             → Parallel execution
CancellationToken   → Graceful cancellation
Task vs ValueTask   → ValueTask para caché sync path
```

### Performance Tips

```
Memory leaks        → Unsubscribe events, dispose resources
GC pressure         → Reduce allocations, use object pools
Database            → Index, lazy loading, pagination
Caching             → MemoryCache / FusionCache, expiración
Span<T> / AsSpan()  → Zero-alloc parsing, reemplaza Substring en hot paths
ValueTask           → Zero alloc en sync path (caché)
```

### Ecosystem — Paquetes Clave

```
Serilog             → Logging estructurado
OpenTelemetry       → Métricas, trazas, logs
EF Core / Dapper    → ORM completo / micro-ORM SQL
FluentValidation    → Validación fluida de modelos
FusionCache         → Caching con fail-safe y distribución
Hangfire            → Background jobs con dashboard
xUnit + TestContainers → Unit + integration testing con Docker
MediatR             → Mediator / CQRS
Polly               → Retry, circuit breaker, resiliencia
Scalar              → OpenAPI/Swagger UI moderna
```

---

## Notas Finales

Estos conceptos forman la base de una entrevista de .NET senior. La clave es:

✅ Entender **el por qué** detrás de cada patrón, no memorizar.
✅ Poder **explicar trade-offs** (performance vs readability, etc.).
✅ Tener **ejemplos reales** de proyectos personales.
✅ Hacer **preguntas** que muestren pensamiento crítico.
✅ Reconocer cuándo **no sé algo** y cómo lo aprendería.

**Master the fundamentals. Build real-world projects. Keep learning.**
