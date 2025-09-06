namespace ASP_ITStep.Models.Rest
{
    public class RestResponse
    {
        public RestStatus Status { get; set; } = new();
        public RestMeta Meta { get; set; } = new();
        public Object? Data { get; set; }
    }
}



/* REST (Representational State Transfer) - набір архітектурних вимог
* до програмного комплекса.
* АРІ - (Application Program Interface) - інтерфейс взаємодії 
*  програми із застосунками.
*  Програма - інформаційний центр, частина у якій зберігаються всі дані
*  Застосунок - самостійна частина, що використовує дані, які постачає Програма
*   Зазвичай при одній Прогамі існує кілька Застосунків (сайт, десктоп, мобільний)
*  (також Додаток - несамостійний продукт, що працює тільки у складі
*    іншого продукту, - Плагін)
*  АРІ - інтерфейс (набір правил) обміну даними між частинами комплексу
*  REST - один з рекомендованих наборів таких правил
*  - Client/Server – традиціно для веб
*  - Stateless – відсутність стану ("пам'яті" про попередні дії - сесій)
*  - Cache – відповідь сервера має містити дані про можливість кешування
*  - Layered system – як аналог Middleware - можливість додавання проміжних
*     шарів (проксі) між клієнтом та сервером
*  - Code on demand (optional) – можливість передачі коду у складі відповіді
*  - Uniform interface - єдиний вигляд запитів та відповідей
*   = Resource identification in requests - включення даних про ресурс
*   = Resource manipulation through representations - включення 
*      метаданих про маніпуляцію ресурсом (CRUD)
*   = Self-descriptive messages - включення даних про спосіб оброблення 
*      (тип) переданого повідомлення
*   = Hypermedia as the engine of application state (HATEOAS) - додавання
*      змісту ресурсу (додаткові посилання на його підресурси)
*/