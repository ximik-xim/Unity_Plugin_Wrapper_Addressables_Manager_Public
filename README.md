Плагин является оберткой над логикой Addressables, для более легкого и удобного взаимодействия с этим плагином.

Он позволяет 
- В случае ошибки при запросе объекта, сцены, обновления и т.д, делать переотправку запроса(нужно на случай плохого не соединения)
При этом кол - во запросов и время между ними вы указываете сами
- Также он позволяет в случае запроса на обновление объектов, 1 большой запрос(состоящий из многих ключей объектов) разбить на много маленьких запросов
Кол - во элементов в запросе вы указываете сами
- Также есть механизм позволяющий автоматически в случае получения в итоге ошибки, переключиться на другой вариант получения обьекта(к примеру по другому ключу или еще как либо)
- А если вам все это не надо, не беда, вы можете напрямую, (не используя ранее описанные возможности) делать сразу запросы, т.к реализоано все через абстракцию

- Имеет готовую логику для обновления всех данных каталога, и самого каталога в том числе
- Есть логика которая получает список обновлений через обновление каталога
- Есть логика которая проходит по всем ключам текущего каталога и ищет для них обновления (нужна на случай, если каталог скачался, а обновление фаилов сломалось, тогда при след. запуске в 1 случае не будет обновлений, т.к он не найдет обнолений для каталога, а в этом случае он обновит и дозагрузит бандлы, т.к пройдеться по текущему скаченому каталогу)

---------------------------------------------------------------------------------------------------------

Примеры сцен находяться по пути

Wrapper Addressables Manager -> Example -> Example Scene Data -> Example All Method

Wrapper Addressables Manager -> Example -> Example Scene Data -> Example Test Method Button

---------------------------------------------------------------------------------------------------------

Для запуска сцен нужно пометить как Addressable следующие сцен и объекты (Обязательно указать ключи из описания)

Wrapper Addressables Manager -> Example -> Example Scene Data -> Data Example -> Example Object Addressable 
- (ключ Prefab Example Object Addressable, так же проставить у него Lable "default")

Wrapper Addressables Manager -> Example -> Example Scene Data -> Data Example -> Example Storage Addressables Is Get Obj 
- (ключ SO Example Storage Addressables Is Get Obj)

Wrapper Addressables Manager -> Example -> Example Scene Data -> Data Example -> Example Storage Addressables Update Obj 
- (ключ SO Example Storage Addressables Update Obj)

Wrapper Addressables Manager -> Example -> Example Scene Data -> Data Example -> Example Load Scene -> Example Load Scene Addressable 
- (ключ Scene Example Load Scene Addressable)

Так же добавить в сборку след. сцены

Wrapper Addressables Manager -> Example -> Example Scene Data -> Example All Method

Wrapper Addressables Manager -> Example -> Example Scene Data -> Example Test Method Button
