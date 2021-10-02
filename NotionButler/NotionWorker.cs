using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notion.Client;

namespace NotionButler
{
    public class NotionWorker
    {
        private readonly NotionClient _client;
        private readonly string _databaseId;
        private static int _pageSize = 100;

        public NotionWorker(string authToken, string databaseId)
        {
            var clientOptions = new ClientOptions { AuthToken = authToken };
            _client = new NotionClient(clientOptions);
            _databaseId = databaseId;
        }

        public async Task<Page> AddTodoToInbox(string title)
        {
            var todoParent = new DatabaseParent
            {
                DatabaseId = _databaseId,
            };
            var todoTitle = new TitlePropertyValue
            {
                Title = new List<RichTextBase>
                {
                    new RichTextText { Text = new Text { Content = title } },
                }
            };

            var todo = new NewPage(todoParent).AddProperty("Что", todoTitle);
            return await _client.Pages.CreateAsync(todo);
        }

        public async Task<List<Page>> FetchCurrentTodos()
        {
            return await GetAllQueryPages(GetCurrentTodosFilter());
        }

        public async Task<List<Page>> FetchInboxTodos()
        {
            return await GetAllQueryPages(GetInboxTodosFilter());
        }

        public async Task<List<Page>> GetAllQueryPages(Filter queryFilter)
        {
            var queryParams = new DatabasesQueryParameters { Filter = queryFilter, PageSize = _pageSize };
            var queryResponse = await _client.Databases.QueryAsync(_databaseId, queryParams);

            if (queryResponse.HasMore)
            {
                var results = new List<Page>();
                results.AddRange(queryResponse.Results);
                while (queryResponse.HasMore)
                {
                    queryParams.StartCursor = queryResponse.NextCursor;
                    queryResponse = await _client.Databases.QueryAsync(_databaseId, queryParams);
                    results.AddRange(queryResponse.Results);
                }
                return results;
            }
            else return queryResponse.Results;
        }

        private CompoundFilter GetCurrentTodosFilter()
        {
            var statusIsTodo = new SelectFilter("Статус", equal: "Делать");
            var statusIsInProgress = new SelectFilter("Статус", equal: "В процессе");
            var statusIsWatch = new SelectFilter("Статус", equal: "На контроле");
            var statuses = new List<Filter> { statusIsTodo, statusIsInProgress, statusIsWatch };

            // Compound filter for todos that have one of required statuses
            var statusesFilterGroup = new CompoundFilter(or: statuses);

            var todayFilter = new DateFilter("Когда", onOrBefore: DateTime.Today);
            return new CompoundFilter(and: new List<Filter> { statusesFilterGroup, todayFilter });
        }

        private CompoundFilter GetInboxTodosFilter()
        {
            var onOrBeforeToday = new DateFilter("Когда", onOrBefore: DateTime.Today);
            var setAsideStatus = new SelectFilter("Статус", equal: "Отложил");

            // Compound filter for todos, that were set aside for today or earlier
            var currentSetAside =
                new CompoundFilter(and: new List<Filter> { onOrBeforeToday, setAsideStatus });

            var notBacklog = new SelectFilter("Статус", doesNotEqual: "Бэклог");
            var emptyDate = new DateFilter("Когда", isEmpty: true);

            // Compound filter for todos with empty date that are not in backlog
            var emptyDateNotBacklog =
                new CompoundFilter(and: new List<Filter> { emptyDate, notBacklog });

            var emptyStatus = new SelectFilter("Статус", isEmpty: true);

            var allConditions = new List<Filter> { currentSetAside, emptyDateNotBacklog, emptyStatus };
            return new CompoundFilter(or: allConditions);
        }
    }
}