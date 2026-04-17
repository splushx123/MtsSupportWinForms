# MTS Support WinForms

Приложение Windows Forms для автоматизации работы отдела технической поддержки МТС.

## Что изменено в этой версии
- улучшен визуальный стиль интерфейса;
- реализована авторизация по e-mail без пароля;
- добавлено журналирование действий пользователей;
- доработано главное окно администратора;
- расширен модуль отчетов;
- сохранено подключение к SQL Server через `App.config`.

## Как открыть проект
1. Распакуйте архив.
2. Откройте файл решения `MtsSupportWinForms.sln`.
3. Назначьте проект `MtsSupportWinForms` стартовым, если Visual Studio не сделала это автоматически.
4. Соберите решение: `Build -> Rebuild Solution`.
5. Запустите проект клавишей `F5`.

## Настройка подключения к базе данных
Откройте файл `App.config` и проверьте строку подключения:

```xml
<add name="MtsSupportDb"
     connectionString="Data Source=.;Initial Catalog=MTS_SUPPORT;Integrated Security=True;TrustServerCertificate=True"
     providerName="System.Data.SqlClient" />
```

### Примеры
Если сервер `SQLEXPRESS`:

```xml
connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=MTS_SUPPORT;Integrated Security=True;TrustServerCertificate=True"
```

Если используется LocalDB:

```xml
connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MTS_SUPPORT;Integrated Security=True;TrustServerCertificate=True"
```

Если подключение выполняется по SQL-логину:

```xml
connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=MTS_SUPPORT;User ID=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
```

## Настройка e-mail авторизации
Вход выполняется по адресу электронной почты, указанному в `App.config`:

```xml
<add key="AdminEmail" value="admin@mts-support.local" />
<add key="OperatorEmail" value="operator@mts-support.local" />
<add key="EngineerEmail" value="engineer@mts-support.local" />
```

## Структура приложения
- `LoginForm` - вход по e-mail;
- `MainForm` - главное окно и навигация;
- `ClientsForm`, `RequestsForm`, `EquipmentForm`, `EmployeesForm`, `SolutionsForm` - рабочие модули;
- `ReportsForm` - отчеты и экспорт;
- `ActivityLogForm` - журналирование;
- `Db.cs` - выполнение SQL-запросов;
- `Theme.cs` - централизованное оформление интерфейса.

## Журналирование
Файл журнала создается автоматически при первом действии пользователя:

`logs/activity.log`

В журнал записываются:
- вход в систему;
- открытие модулей;
- операции изменения данных;
- экспорт отчетов;
- ошибки интерфейса.
