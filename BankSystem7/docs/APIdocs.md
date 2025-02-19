
#  Documentation

##  Structure of project

  

1. Folder **AppContext** is folder where contains context classes for interaction with database without business logic.

2. Folder **docs** contains files for documentation.

3. Folder **Extensions** contains extension classes for different types.

4. Folder **Models** contains entity and configuration models for library.

5. Folder **Services** contains 4 sub folders: **Configuration**, **DependencyInjection**, **Interfaces** and **Repositories**.

6. Folder **Configuration** contains classes for configuration library.

7. Folder **DependencyInjection** contains classes for easy using Dependency Injection.

8. Folder **Interfaces** contains interfaces which describes structure of inherited classes.

9. Folder **Repositories** is folder with all business logic of project. Only there simple developer has access.

  

##  How to interact with library?

The library provides ways to pass and use own models. For example, You can inherit Your class from base class User and pass it as type to an instance of `BankSystemBuilder`

and use own model.

Developer can interact with library with:

1. `BankSystemBuilder` class.

2. `builder.Services.AddNationBankSystem<...>()` service in ASP.NET.

  

And by following next steps:

1. Create field of the class `ConfigurationOptions` that handles settings of services.

2. Next initialize `BankSystemBuilder` class and pass to the constructor field of `ConfigurationOptions` as parameter.

3. Create field of the interface `IServiceConfiguration` and assign to it value from method `Build()` of class `BankSystemBuilder`.

4. Interact with repositories throughout public properties of the created field of the interface `IServiceConfiguration`.

  

New feature for library is adding services to internal DI in ASP.Net application. For that You have to write something like this:

  

1. in Program.cs

```

builder.Services.AddNationBankSystem<User, Card, BankAccount, Bank, Credit>(o =>
{
	o.ConnectionConfiguration = new NpgsqlConnectionConfiguration()
	{
		EnsureCreated = false,
		EnsureDeleted = false,
		DatabaseName = "Test",
		DatabaseManagementSystemType = DatabaseManagementSystemType.PostgreSql,
		Host = "localhost",
		Port = "5432",
	};
	o.Credentials = new NpgsqlCredentials()
	{
		Username = "postgres",
		Password = "secret"
	};
	o.OperationOptions = new OperationServiceOptions()
	{
		DatabaseName = "Test",
	};
// here we add database context classes with class that inherit ModelConfiguration
	o.Contexts = new Dictionary<DbContext, ModelConfiguration?>
	{
		{ new ApplicationContext(), new ModelConfigurationTest() },
		{ new NewApplicationContext(), new NewModelConfigurationTest() },
	};
});

```

//or

```
builder.Services.AddNationBankSystem<User, Card, BankAccount, Bank, Credit>(new ConfigurationOptions()
{
	ConnectionConfiguration = new NpgsqlConnectionConfiguration()
	{
	EnsureCreated = false,
	EnsureDeleted = false,
	DatabaseName = "Test",
	DatabaseManagementSystemType = DatabaseManagementSystemType.PostgreSql,
	Host = "localhost",
	Port = "5432",
},
	Credentials = new NpgsqlCredentials()
	{
		Username = "postgres",
		Password = "secret"
	},
	OperationOptions = new OperationServiceOptions()
	{
		DatabaseName = "Test",
	},
// here we add database context classes with class that inherit ModelConfiguration

	Contexts = new Dictionary<DbContext, ModelConfiguration?>
	{
		{ new ApplicationContext(), new ModelConfigurationTest() },
		{ new NewApplicationContext(), new NewModelConfigurationTest() },
	};
});
```

But use only one of the above approaches to use service.

Now You can don't specify LoggerOptions because by default logger is disabled.

  

1. in Your controller

```

private readonly IServiceConfiguration<User, Card, BankAccount, Bank, Credit> _service;
public UsersController(IServiceConfiguration<User, Card, BankAccount, Bank, Credit> service)
{
	_service = service;
}

```

And all will work fine.
  

##  API documentation

###  AppContext

There are 3 classes context:

  

1. **GenericDbContext** - main db context class that handles all needed operations.

2. **ApplicationContext** - configures GenericDbContext to context class for this library.

3. **BankContext** - is responsible for handling operations when use ApplicationContext is impossible.

  

####  API GenericDbContext

**Methods:**

1. `private void DatabaseHandle()` - implements handling creating and deleting database.

2. `private void SetDatabaseManagementSystemType(DbContextOptionsBuilder optionsBuilder, DatabaseManagementSystemType dbmsType)` - method sets the database management system type for the given DbContextOptionsBuilder object.

3. `public void UpdateTracker<T>(T item, EntityState state, Action? action, DbContext context)` - updates states of entity. You should use this method with method `AvoidChanges(object[]? entities, DbContext context)` for the best state tracking of entities.

4. `public void UpdateTrackerRange(object[] items, EntityState state, Action? action, DbContext context)` - method updates states of entities. You should use this method with method `AvoidChanges(object[]? entities, DbContext context)` for the best state tracking of entities

5. `public void AvoidChanges(object[]? entities, DbContext context)` - ensures that passed entities won't be changed during call method `SaveChanges()`.

  

####  API ApplicationContext

  

**Methods:**

1. `internal ExceptionModel AvoidDuplication(Bank item)` - implements function for avoiding duplication in table Banks in the database.

  

**Properties:**

  

1. `protected internal DbSet<User> Users { get; set; }` - an instance of the table `Users` in database.

2. `protected internal DbSet<Bank> Banks { get; set; }` -an instance of the table `Banks` in database.

3. `protected internal DbSet<BankAccount> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.

4. `protected internal DbSet<Credit> Credits { get; set; }` - an instance of the table `Credits` in database.

  

####  API BankContext

  

**Methods:**

1. `public ExceptionModel CreateOperation(Operation operation, OperationKind operationKind)` - creates transaction operation.

2. `public async Task<ExceptionModel> CreateOperationAsync(Operation? operation, OperationKind operationKind)` - creates transaction operation asynchronously.

3. `public ExceptionModel DeleteOperation(Operation operation)` - deletes transaction operation

4. `public async Task<ExceptionModel> DeleteOperationAsync(Operation? operation)` - deletes transaction operation asynchronously.

5. `public ExceptionModel BankAccountWithdraw(User user, Bank bank, Operation operation)` - withdraws money from user bank account and accrual to bank's account.

6. `internal async Task<ExceptionModel> BankAccountWithdrawAsync(User? user, Bank? bank, Operation operation)` - withdraws money from user bank account and accrual to bank's account asynchronously.

7. `private ExceptionModel BankAccountAccrual(User user, Bank bank, Operation operation)` - accruals money to user bank account from bank's account.

8. `internal async Task<ExceptionModel> BankAccountAccrualAsync(User? user, Bank? bank, Operation operation)` - accruals money to user bank account from bank's account asynchronously.

9. `private StatusOperationCode GetStatusOperation(Operation operation, OperationKind operationKind)` - returns status of operation for next handling of operation.

  

**Properties:**

  

1. `public DbSet<User> Users { get; set; }` - an instance of the table `Users` in database.

2. `public DbSet<Bank> Banks { get; set; }` -an instance of the table `Banks` in database.

3. `public DbSet<Cards> Cards { get; set; }` - an instance of the table `Cards` in database.

4. `public DbSet<BankAccount> BankAccounts { get; set; }` - an instance of the table `BankAccounts` in database.

  

###  Services

Services are dividing on 3 sub-folders:

  

1. **Configuration**

2. **Interfaces**

3. **Repositories**

  

And some classes that don't belong to any of folders:

  

1. `BankServiceOptions` - defines options that used by internal services.

2. `Logger` - service for logging some info about repositories operations.

3. `OperationService` - provides connection to mongodb services.

  

###  Configuration

Here located services for configuring library.

1. `ConfigurationOptions` - service provides options for the most full configuring of library.

2. `ModelConfiguration` - service ensures correct relationships between models.

3. `ServiceConifiguration` - service that handles all services that there are in the library.

  
  ### DependencyInjection
 
  Here located services that provide easy ways to use Dependency Injection in project.
  1. `BankSystemRegistrar` - service that contains methods to use Dependency Injection.
  2. `Dependency` - file contains 2 models: `Dependency` and `TypedDependency`. They contains properties to collect all needed data to use methods in `BankSystemRegistrar`.


###  Interfaces (and abstract classes)

Here located interfaces which describes behavior of inherited repo-classes.

1. **Interface**  `IRepository<T> : IReaderService<T>, IWriterService<T>, IDisposable where T : class` - interface for implement standard library logic.

- `IQueryable<T> All { get; }` - implements getting a sequence of the objects from database.

  

2. **Interface**  `IRepositoryAsync<T> : IReaderServiceAsync<T>, IWriterServiceAsync<T> where T : class` - interface for implement standard library logic asynchronously.

- `IQueryable<T> All { get; }` - implements getting a sequence of the objects from database.

  

3. **Interface**  `IReaderServiceAsync<T> where T : class` - interface for implement reading data from database asynchronously.

  

Methods

- `Task<T> GetAsync(Expression<Func<T, bool>> predicate);` - implements getting an object from database with predicate asynchronously.

- `Task<bool> ExistAsync(Expression<Func<T, bool>> predicate);` - implements checking exist object with in database predicate asynchronously.

- `Task<bool> FitsConditionsAsync(T? item);` - implements logic for checking on conditions true of passed entity asynchronously.

  

4. **Interface**  `IWriterServiceAsync<in T> where T : class` - interface for implement writing, updating and deleting data in database

  

Methods

- `Task<ExceptionModel> CreateAsync(T item);` - implements adding entity in database asynchronously.

- `Task<ExceptionModel> UpdateAsync(T item);` - implements updating entity in database asynchronously.

- `Task<ExceptionModel> DeleteAsync(T item);` - implements deleting entity from database asynchronously.

  

5. **Interface**  `IReaderService<T> where T : class` - interface for implement reading data from database.

  

Methods

- `T Get(Expression<Func<T, bool>> predicate);` - implements getting an object from database with predicate.

- `bool Exist(Expression<Func<T, bool>> predicate);` - implements checking exist object with in database predicate.

- `bool FitsConditions(T? item);` - implements logic for checking on conditions true of passed entity.

  

6. **Interface**  `IReaderServiceWithTracking<T> where T : class` - interface for implement reading data from database with another type of parameters.

  

Methods

- `T GetWithTracking(Expression<Expression<Func<T, bool>>> predicate);` - implements getting an object from database with predicate.

- `bool ExistWithTracking(Expression<Expression<Func<T, bool>>> predicate);` - implements checking exist object with in database predicate.

Properties

- `IQueryable<T> AllWithTracking { get; }` - implements getting a sequence of the objects from database.

  

7. **Interface**  `IWriterService<in T> where T : class` - interface for implement writing, updating and deleting data in database

  

Methods

- `ExceptionModel Create(T item);` - implements adding entity in database.

- `ExceptionModel Update(T item);` - implements updating entity in database.

- `ExceptionModel Delete(T item);` - implements deleting entity from database.

  

8. **Interface**  `ILogger` - interface that provides standard set for logging

  

Methods

- `ExceptionModel Log(Report report);` - implements logging report in database.

- `ExceptionModel Log(IEnumerable<Report> reports);` - implements logging collection of reports in database.

  

Properties

- `public bool IsReused { get; set; }` - defines possibility use already initialized logger.

- `public LoggerOptions LoggerOptions { get; set; }` - defines options for logger configuration.

  

9. **Abstract class**  `LoggerExecutor<TOperationType> where TOperationType : Enum` - simple implementation of service for added reports to logger queue

  

Methods

- `virtual void Log(ExceptionModel exceptionModel, string methodName, string className, TOperationType operationType, ICollection<GeneralReport<TOperationType>> reports)` - implements standard logic of inserting log data to logger queue. Can be overrided.

  

###  Repositories

Repositories are implementation of various interfaces and working with context classes for interact with database.

  

1. `BankRepository` - implements interface `IRepository<T>` for handling bank model, contains methods for accrual or withdraw money from bank account and bank.

2. `BankAccountRepository` - implements interface `IRepository<T>` for handling bank account model and Transfer money method.

3. `CardRepository` - implements interface `IRepository<T>` for handling card model and Transfer money method.

4. `UserRepository` - implements interface `IRepository<T>` for handling user model.

5. `CreditRepository` - implements interface `IRepository<T>` for handling credit model and operations with credit(loan). For example: take, pay credit.

6. `LoggerRepository` - implements interface `IRepository<T>` for handling reports.

7. `OperationRepository` - implements interface `IRepository<T>` for handling operations.

  

##  When cause exception or error?

There are a lot of points when you can catch an exception or error while using api of project but we describe some of them:

  

1. You use default connection string instead of Your. Can happen so that on Your machine won't database with name which was specified in default connection string.

2. You change content of repositories or context class. If You change some of these You can get an error.

**Example**:

````

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
	optionsBuilder.EnableSensitiveDataLogging();
	optionsBuilder.UseSqlServer(queryConnection);
}
````

You'll write something like this

````
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
	optionsBuilder.EnableSensitiveDataLogging();
	optionsBuilder.UseSqlServer();
}
````

3. You use methods incorrectly.

**Example**:

You want to delete operation therefore You have to use method `DeleteOperation(...)` but You use method `CreateOperation(...)` and of course You'll get an exception because method `CreateOperation(...)`has return type `ExceptionModel` and it'll returns `ExceptionModel.OperationFailed` because the same operation already exist in the database which You are using.


### Sincerely, hayako.