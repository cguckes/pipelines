using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pipelines;

namespace PipelinesTest
{
    public class ExampleCode
    {
        private readonly ILogger _logger;

        public ExampleCode(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<Workset> RunExampleCode()
        {
            var fetchTransformAndStore = APipeline
                .WithSteps(
                    FetchFromExternalSystemStep(),
                    FilterUnneededEntitiesStep(),
                    TransformStep(),
                    SaveToLocalStorageStep()
                )
                .LoggingTo(_logger);

            var result = await fetchTransformAndStore.Execute(new Workset());
            return result;
        }

        private static async Task<Workset> FetchFromExternalSystem(Workset ws)
        {
            ws.External = await Task.FromResult(new ExternalEntity
            {
                Id = "ExternalId",
                RelevantUntil = DateTime.Today.AddDays(1)
            });
            return ws;
        }

        private static IStepBuilder FetchFromExternalSystemStep()
            => AStep
                .ThatExecutes<Workset>(FetchFromExternalSystem)
                .AssumingThat(ExternalEntityIsNull)
                .AssumingAfter(ExternalEntityIsNotNull);

        private static Task<Workset> FilterUnneededEntities(Workset ws)
        {
            if (ws.External.RelevantUntil < DateTime.Now)
            {
                ws.External = null;
            }

            return Task.FromResult(ws);
        }

        private static IStepBuilder FilterUnneededEntitiesStep()
            => AStep
                .ThatExecutes<Workset>(FilterUnneededEntities)
                .Named("RenamedStep")
                .AssumingThat(ExternalEntityIsNotNull);

        private static Task<Workset> Transform(Workset ws)
        {
            ws.Internal = new InternalEntity
            {
                ExternalId = ws.External.Id,
            };
            return Task.FromResult(ws);
        }

        private static IStepBuilder TransformStep()
            => AStep.ThatExecutes<Workset>(Transform)
                .AssumingThat(ExternalEntityIsNotNull)
                .AssumingAfter(InternalEntityIsNotNull);

        private static Task<Workset> SaveToLocalStorage(Workset ws)
        {
            ws.Internal.Id = Guid.NewGuid();
            return Task.FromResult(ws);
        }

        private IStepBuilder SaveToLocalStorageStep()
            => AStep
                .ThatExecutes<Workset>(SaveToLocalStorage)
                .AssumingThat(InternalEntityIsNotNull);

        private static bool ExternalEntityIsNull(Workset ws)
            => ws.External == null;

        private static bool ExternalEntityIsNotNull(Workset ws)
            => ws.External != null;

        private static bool InternalEntityIsNotNull(Workset ws)
            => ws.Internal != null;
        
        
        public class Workset
        {
            public ExternalEntity External { get; set; }
            public InternalEntity Internal { get; set; }
        }

        public class ExternalEntity
        {
            public string Id { get; set; }
            public Dictionary<string, object> HorribleObjectData { get; set; }
            public DateTime RelevantUntil { get; set; }
        }

        public class InternalEntity
        {
            public Guid Id { get; set; }
            public string ExternalId { get; set; }
        }
    }
}