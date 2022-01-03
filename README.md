[![CI](https://github.com/cguckes/pipelines/actions/workflows/ci.yml/badge.svg)](https://github.com/cguckes/pipelines/actions/workflows/ci.yml)
[![Coverage Status](https://coveralls.io/repos/github/cguckes/pipelines/badge.svg?branch=main)](https://coveralls.io/github/cguckes/pipelines?branch=main)

# Pipelines

Ever had to move data from one system to another? Did it start out looking
very straightforward but in the end, you had to apply lots of complex rules
all over the place, leaving the code in a horrendous tangle of a mess? Happened
to me multiple times. After some refactoring I usually end up with a sort of
pipeline structure, applying several small and easily testable steps.

I finally managed to turn it into a small library, removing most of the
boilerplate I had to write. It looks like this:

```c#
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
```

A pipeline step executes a function and has several convenience functions.
You can define pre and postconditions to clear up your expectations, give it
a name, define how your in and output is identified (default is ToString()).
look like this:

```c#
private static async Task<Workset> FetchFromExternalSystem(Workset ws)
{
    // dummy fetch from api step
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
        .Named("API fetch") // optional, defaults to the name of the function above 
        .AssumingThat(ExternalEntityIsNull)
        .AssumingAfter(ExternalEntityIsNotNull);
```

If you run the code above and set the log level to `TRACE`, you get the
following output for free, never having to think about such messy things
as useful logging any more:

```
DEBUG Program - FetchFromExternalSystem: Checking preconditions for PipelinesTest.ExampleCode+Workset.
TRACE Program - FetchFromExternalSystem: Input:
TRACE Program - {"External":null,"Internal":null}
DEBUG Program - FetchFromExternalSystem: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - FetchFromExternalSystem: finished processing. Checking postconditions.
TRACE Program - FetchFromExternalSystem: Output:
TRACE Program - {"External":{"Id":"ExternalId","HorribleObjectData":null,"RelevantUntil":"2022-01-04T00:00:00+01:00"},"Internal":null}
DEBUG Program - FetchFromExternalSystem: Postconditions for PipelinesTest.ExampleCode+Workset met.
DEBUG Program - RenamedStep: Checking preconditions for PipelinesTest.ExampleCode+Workset.
TRACE Program - RenamedStep: Input:
TRACE Program - {"External":{"Id":"ExternalId","HorribleObjectData":null,"RelevantUntil":"2022-01-04T00:00:00+01:00"},"Internal":null}
DEBUG Program - RenamedStep: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - RenamedStep: finished processing. Checking postconditions.
TRACE Program - RenamedStep: Output:
TRACE Program - {"External":{"Id":"ExternalId","HorribleObjectData":null,"RelevantUntil":"2022-01-04T00:00:00+01:00"},"Internal":null}
DEBUG Program - RenamedStep: Postconditions for PipelinesTest.ExampleCode+Workset met.
DEBUG Program - Transform: Checking preconditions for PipelinesTest.ExampleCode+Workset.
TRACE Program - Transform: Input:
TRACE Program - {"External":{"Id":"ExternalId","HorribleObjectData":null,"RelevantUntil":"2022-01-04T00:00:00+01:00"},"Internal":null}
DEBUG Program - Transform: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - Transform: finished processing. Checking postconditions.
TRACE Program - Transform: Output:
TRACE Program - {"External":{"Id":"ExternalId","HorribleObjectData":null,"RelevantUntil":"2022-01-04T00:00:00+01:00"},"Internal":{"Id":"00000000-0000-0000-0000-000000000000","ExternalId":"ExternalId"}}
DEBUG Program - Transform: Postconditions for PipelinesTest.ExampleCode+Workset met.
DEBUG Program - SaveToLocalStorage: Checking preconditions for PipelinesTest.ExampleCode+Workset.
TRACE Program - SaveToLocalStorage: Input:
TRACE Program - {"External":{"Id":"ExternalId","HorribleObjectData":null,"RelevantUntil":"2022-01-04T00:00:00+01:00"},"Internal":{"Id":"00000000-0000-0000-0000-000000000000","ExternalId":"ExternalId"}}
DEBUG Program - SaveToLocalStorage: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - SaveToLocalStorage: finished processing. Checking postconditions.
TRACE Program - SaveToLocalStorage: Output:
TRACE Program - {"External":{"Id":"ExternalId","HorribleObjectData":null,"RelevantUntil":"2022-01-04T00:00:00+01:00"},"Internal":{"Id":"14a1bf0b-dcf8-4aaf-b846-318cd15097a8","ExternalId":"ExternalId"}}
DEBUG Program - SaveToLocalStorage: Postconditions for PipelinesTest.ExampleCode+Workset met.
```

Set to `DEBUG` your output will look like this:

```
DEBUG Program - FetchFromExternalSystem: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - FetchFromExternalSystem: finished processing. Checking postconditions.
DEBUG Program - FetchFromExternalSystem: Postconditions for PipelinesTest.ExampleCode+Workset met.
DEBUG Program - RenamedStep: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - RenamedStep: finished processing. Checking postconditions.
DEBUG Program - RenamedStep: Postconditions for PipelinesTest.ExampleCode+Workset met.
DEBUG Program - Transform: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - Transform: finished processing. Checking postconditions.
DEBUG Program - Transform: Postconditions for PipelinesTest.ExampleCode+Workset met.
DEBUG Program - SaveToLocalStorage: Preconditions for TraceableAsyncStep`2 met, start processing.
DEBUG Program - SaveToLocalStorage: finished processing. Checking postconditions.
DEBUG Program - SaveToLocalStorage: Postconditions for PipelinesTest.ExampleCode+Workset met.
```

The full example can be found in the test project under https://github.com/cguckes/pipelines/blob/84effafc8c99cff0395e852793100bffe61c0718/PipelinesTest/ExampleCode.cs


## FAQ

**Q: Great, so you build a C# inside a C#, why should I use it?**

**A:** While technically still writing down a list of functions you want to call,
you get automated logging and tracing, allowing you to control the situation before
and after every step. If you keep your pipeline steps reasonably small, you can test
them quite easily. If you name your steps like the business rules you need to follow,
it's very easy to find bugs in the code.

**Q: Why not just use a list of functions with the same input and output types?**

**A:** This library allows you to chain multiple functions with different input and outputs.
You can define a pipeline that looks like this:
```c#
var pipeline = new Pipeline(
    AStep.ThatExecutes<int, string>(i => Task.FromResult(${i})),
    AStep.ThatExecutes<string, string>(Task.FromResult)
);
string result = pipeline.Execute<int, string>(5); // result == "5"
```

**Q: But wait, this means I can't have type safety any more!**

**A:** I don't think it's possible to chain functions in a readable way like this and still have
type safety. Your steps are validated the second you create your pipeline though, so the closest
you get to type safety is a unit test, that instantiates your pipeline. Do that and you should be
fine.

**Q: This lacks major features, why?**

**A:** I'm using this for my own projects at the moment, feel free to ask for more
features. Since this will probably never be a big project, it should be easy enough
to add new ones.

## Roadmap

- Correlation IDs

## License
MIT
