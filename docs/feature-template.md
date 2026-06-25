# Шаблон новой вкладки

Ниже — рекомендуемая минимальная структура новой integration/feature-вкладки.

```text
ServerMultiTool/
  Features/
    MyFeature/
      MyFeatureModuleServiceCollectionExtensions.cs
      MyFeatureViewModel.cs
      MyFeatureView.xaml
      MyFeatureView.xaml.cs
      MyFeatureNavigationRequest.cs   # опционально
      Services/                       # опционально
```

## Naming

- ключ вкладки: `PageKeys.MyFeature`
- модуль: `AddMyFeatureModule()`
- view: `MyFeatureView`
- view model: `MyFeatureViewModel`
- request: `MyFeatureNavigationRequest`

## Минимальный checklist

### 1. ViewModel

- наследуется от `BaseViewModel`;
- принимает только нужные сервисы;
- при необходимости реализует `INavigationAware`.

### 2. View

- получает `ViewModel` через конструктор;
- не создаёт зависимости вручную;
- не знает о shell-навигации.

### 3. Модуль регистрации

В `AddMyFeatureModule()`:

- зарегистрировать feature-сервисы;
- зарегистрировать `MyFeatureViewModel`;
- зарегистрировать `MyFeatureView`;
- добавить `IPageDescriptor` с ключом, заголовком, иконкой и порядком.

### 4. Подключение в приложение

Единственное центральное изменение для новой feature:

- добавить вызов `.AddMyFeatureModule()` в `AddFeatureModules()`.

После этого вкладка автоматически:

- появится в меню;
- станет доступна для `INavigationService.NavigateTo("MyFeature")`;
- будет участвовать в общем shell-потоке без правок в `MainWindowViewModel` и без `switch`.

## Typed navigation request

Если вкладка должна открываться в определённом состоянии:

- создайте `record MyFeatureNavigationRequest(...)`;
- передайте его через `INavigationService.NavigateTo(PageKeys.MyFeature, request)`;
- обработайте его в `OnNavigatedTo`.

## Живой пример

См. demo-модуль `InternalTools` в `ServerMultiTool/Features/InternalTools/`.

